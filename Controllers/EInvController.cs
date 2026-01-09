using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;
using System.Xml;

namespace PcbErpApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EInvController : ControllerBase
    {
        private readonly IConfiguration _config;

        public EInvController(IConfiguration config)
        {
            _config = config;
        }

        public class SendEInvRequest
        {
            public string TableName { get; set; } = "";
            public string PaperNum { get; set; } = "";
            public int IPass { get; set; } // 0: 刪除XML, 1: 確認產生XML, 2: 作廢產生XML
        }

        /// <summary>
        /// 執行 SendEInv 邏輯 (對應 Delphi 的 SendEInv procedure)
        /// </summary>
        [HttpPost("SendEInv")]
        public async Task<IActionResult> SendEInv([FromBody] SendEInvRequest req)
        {
            if (string.IsNullOrEmpty(req.TableName) || string.IsNullOrEmpty(req.PaperNum))
                return BadRequest("TableName 和 PaperNum 為必填");

            var connStr = _config.GetConnectionString("DefaultConnection");
            await using var conn = new SqlConnection(connStr);
            await conn.OpenAsync();

            try
            {
                // 1. 取得單據狀態
                int eInvStatus = 0;
                int uploaded = 0;
                await using (var cmd = new SqlCommand("SPOdEInvPaperStatusGet", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PaperId", req.TableName);
                    cmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);

                    await using var reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        eInvStatus = reader.GetInt32(reader.GetOrdinal("EInvStatus"));
                        uploaded = reader.GetInt32(reader.GetOrdinal("Uploaded"));
                    }
                }

                // 2. 檢查是否啟用電子發票XML
                bool useEInvXml = false;
                await using (var cmd = new SqlCommand(
                    "SELECT Value FROM CURdSysParams(NOLOCK) WHERE SystemId='SPO' AND ParamId='EInvXMLRev' AND ISNULL(Value,'') <> ''",
                    conn))
                {
                    var result = await cmd.ExecuteScalarAsync();
                    useEInvXml = result != null && !string.IsNullOrWhiteSpace(result.ToString());
                }

                if (!useEInvXml)
                {
                    return Ok(new { ok = true, message = "未啟用電子發票XML功能" });
                }

                // 3. 取得 EInvPath
                string einvPath = "";
                await using (var cmd = new SqlCommand(
                    "SELECT Value FROM CURdSysParams WHERE SystemId='SPO' AND ParamId='EInvPath'",
                    conn))
                {
                    var result = await cmd.ExecuteScalarAsync();
                    einvPath = result?.ToString() ?? "";
                }

                if (string.IsNullOrWhiteSpace(einvPath))
                {
                    return BadRequest("找不到電子發票路徑設定 (EInvPath)");
                }

                // 4. 呼叫 SPOdInvoiceToEInvCS 取得XML內容和路徑
                string uploadPath = "";
                string xmlCode = "";
                await using (var cmd = new SqlCommand("SPOdInvoiceToEInvCS", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PaperId", req.TableName);
                    cmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);
                    cmd.Parameters.AddWithValue("@Type", req.IPass);
                    cmd.Parameters.AddWithValue("@DoNotice", 0);
                    cmd.Parameters.AddWithValue("@ReWrite", 0);
                    cmd.Parameters.AddWithValue("@VoidReAct", 0);

                    await using var reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        uploadPath = reader["UploadPath"]?.ToString() ?? "";

                        // 組合所有 XMLCode 欄位 (XMLCode, XMLCode1~XMLCode20)
                        // 邏輯：XMLCode1-19 遇到空值就停止，但 XMLCode20 一定要加
                        var sb = new StringBuilder();
                        sb.Append(reader["XMLCode"]?.ToString() ?? "");

                        // 處理 XMLCode1 到 XMLCode19
                        for (int i = 1; i <= 19; i++)
                        {
                            var colName = $"XMLCode{i}";
                            try
                            {
                                int ordinal = reader.GetOrdinal(colName);
                                var val = reader[colName]?.ToString() ?? "";
                                if (!string.IsNullOrEmpty(val))
                                    sb.Append(val);
                                else
                                    break; // 遇到空值就停止
                            }
                            catch (IndexOutOfRangeException)
                            {
                                // 欄位不存在，停止讀取
                                break;
                            }
                        }

                        // 無論如何都要加上 XMLCode20
                        try
                        {
                            sb.Append(reader["XMLCode20"]?.ToString() ?? "");
                        }
                        catch (IndexOutOfRangeException)
                        {
                            // XMLCode20 不存在，忽略
                        }

                        xmlCode = sb.ToString();
                    }
                }

                if (string.IsNullOrWhiteSpace(uploadPath))
                {
                    return Ok(new { ok = true, message = "無需處理XML檔案" });
                }

                string fullPath = Path.Combine(einvPath, uploadPath, $"{req.PaperNum}.XML");

                // 5. 根據 iPass 處理XML檔案
                if (req.IPass == 0) // 退審：刪除XML
                {
                    if (eInvStatus == 0)
                    {
                        // 根據 Uploaded 欄位判斷是否要刪除檔案
                        if (uploaded == 0 && System.IO.File.Exists(fullPath))
                        {
                            try
                            {
                                System.IO.File.Delete(fullPath);

                                if (!System.IO.File.Exists(fullPath))
                                {
                                    return Ok(new { ok = true, message = "檔案已刪除" });
                                }
                                else
                                {
                                    return StatusCode(500, "檔案刪除失敗");
                                }
                            }
                            catch (Exception ex)
                            {
                                return StatusCode(500, $"刪除檔案失敗: {ex.Message}");
                            }
                        }
                        else if (uploaded == 1)
                        {
                            // 已上傳，需要確認是否要作廢
                            return Ok(new { ok = true, needConfirm = true, uploaded = true });
                        }
                    }
                }
                else // iPass = 1 或 2：確認或作廢，儲存XML
                {
                    if (!string.IsNullOrWhiteSpace(xmlCode))
                    {
                        try
                        {
                            // 確保目錄存在
                            var directory = Path.GetDirectoryName(fullPath);
                            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                            {
                                Directory.CreateDirectory(directory);
                            }

                            // 載入並儲存XML
                            var xmlDoc = new XmlDocument();
                            xmlDoc.LoadXml(xmlCode);
                            xmlDoc.Save(fullPath);

                            // 驗證檔案是否存在
                            if (!System.IO.File.Exists(fullPath))
                            {
                                // 清除欄位
                                await using var clearCmd = new SqlCommand("SPOdClearEInvField", conn);
                                clearCmd.CommandType = CommandType.StoredProcedure;
                                clearCmd.Parameters.AddWithValue("@PaperId", req.TableName);
                                clearCmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);
                                clearCmd.Parameters.AddWithValue("@Fin", 0);
                                clearCmd.Parameters.AddWithValue("@uploaded", 0);
                                await clearCmd.ExecuteNonQueryAsync();

                                return StatusCode(500, "檔案上傳無效，請重新審核!!");
                            }

                            // 清除欄位並更新狀態
                            await using var clearCmd2 = new SqlCommand("SPOdClearEInvField", conn);
                            clearCmd2.CommandType = CommandType.StoredProcedure;
                            clearCmd2.Parameters.AddWithValue("@PaperId", req.TableName);
                            clearCmd2.Parameters.AddWithValue("@PaperNum", req.PaperNum);
                            clearCmd2.Parameters.AddWithValue("@Fin", 1);
                            clearCmd2.Parameters.AddWithValue("@uploaded", uploaded == 1 ? 1 : 0);
                            await clearCmd2.ExecuteNonQueryAsync();

                            return Ok(new { ok = true, message = "XML檔案已產生", filePath = fullPath });
                        }
                        catch (Exception ex)
                        {
                            // 發生錯誤，清除欄位
                            await using var clearCmd = new SqlCommand("SPOdClearEInvField", conn);
                            clearCmd.CommandType = CommandType.StoredProcedure;
                            clearCmd.Parameters.AddWithValue("@PaperId", req.TableName);
                            clearCmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);
                            clearCmd.Parameters.AddWithValue("@Fin", 0);
                            clearCmd.Parameters.AddWithValue("@uploaded", 0);
                            await clearCmd.ExecuteNonQueryAsync();

                            return StatusCode(500, $"產生XML失敗: {ex.Message}");
                        }
                    }
                }

                return Ok(new { ok = true, message = "處理完成" });
            }
            catch (SqlException sqlEx)
            {
                return StatusCode(500, $"SQL 錯誤: {sqlEx.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 檢查XML檔案是否存在
        /// </summary>
        [HttpPost("CheckXMLExists")]
        public async Task<IActionResult> CheckXMLExists([FromBody] CheckXMLExistsRequest req)
        {
            if (string.IsNullOrEmpty(req.TableName) || string.IsNullOrEmpty(req.PaperNum))
                return BadRequest("TableName 和 PaperNum 為必填");

            var connStr = _config.GetConnectionString("DefaultConnection");
            await using var conn = new SqlConnection(connStr);
            await conn.OpenAsync();

            try
            {
                // 取得 EInvPath
                string einvPath = "";
                await using (var cmd = new SqlCommand(
                    "SELECT Value FROM CURdSysParams WHERE SystemId='SPO' AND ParamId='EInvPath'",
                    conn))
                {
                    var result = await cmd.ExecuteScalarAsync();
                    einvPath = result?.ToString() ?? "";
                }

                // 取得 UploadPath
                string uploadPath = "";
                await using (var cmd = new SqlCommand("SPOdInvoiceToEInvCS", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PaperId", req.TableName);
                    cmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);
                    cmd.Parameters.AddWithValue("@Type", 0);
                    cmd.Parameters.AddWithValue("@DoNotice", 0);
                    cmd.Parameters.AddWithValue("@ReWrite", 0);
                    cmd.Parameters.AddWithValue("@VoidReAct", 0);

                    await using var reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        uploadPath = reader["UploadPath"]?.ToString() ?? "";
                    }
                }

                if (string.IsNullOrWhiteSpace(einvPath) || string.IsNullOrWhiteSpace(uploadPath))
                {
                    return Ok(false);
                }

                string fullPath = Path.Combine(einvPath, uploadPath, $"{req.PaperNum}.XML");
                bool exists = System.IO.File.Exists(fullPath);

                return Ok(exists);
            }
            catch
            {
                return Ok(false);
            }
        }

        public class CheckXMLExistsRequest
        {
            public string TableName { get; set; } = "";
            public string PaperNum { get; set; } = "";
        }
    }
}
