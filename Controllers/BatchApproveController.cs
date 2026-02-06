using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace PcbErpApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BatchApproveController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public BatchApproveController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // 批次核准
        [HttpPost]
        public async Task<IActionResult> BatchApprove([FromBody] BatchApproveRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                return BadRequest(new { error = "UserId is required" });
            }

            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            var successCount = 0;
            var errorCount = 0;
            var errorMessages = new List<string>();

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // 1. 取得要批次核准的項目列表
                    var itemsToApprove = new List<(string MailSeq, string Receiver)>();

                    // 如果前端有指定勾選項目，需要從資料庫查詢正確的 RECEIVER 值
                    if (request.MailSeqList != null && request.MailSeqList.Count > 0)
                    {
                        foreach (var item in request.MailSeqList)
                        {
                            if (item != null && !string.IsNullOrEmpty(item.MailSeq))
                            {
                                // 從 XFLdeFlowInfo 查詢正確的 USERID (RECEIVER)
                                string receiver = "";
                                var query = "SELECT USERID FROM XFLdeFlowInfo WHERE MAILSEQ = @MailSeq";
                                using (var cmd = new SqlCommand(query, connection))
                                {
                                    cmd.Parameters.AddWithValue("@MailSeq", item.MailSeq);
                                    var result = await cmd.ExecuteScalarAsync();
                                    receiver = result?.ToString() ?? "";
                                }

                                if (!string.IsNullOrEmpty(receiver))
                                {
                                    itemsToApprove.Add((item.MailSeq, receiver));
                                }
                            }
                        }
                    }
                    else
                    {
                        // 沒有指定勾選項目，從 XFLdSendMutiSEQ 取得所有待審項目
                        try
                        {
                            using (var getMutiSeqCmd = new SqlCommand("XFLdSendMutiSEQ", connection))
                            {
                                getMutiSeqCmd.CommandType = CommandType.StoredProcedure;
                                getMutiSeqCmd.Parameters.AddWithValue("@Useid", request.UserId ?? "");

                                using (var reader = await getMutiSeqCmd.ExecuteReaderAsync())
                                {
                                    while (await reader.ReadAsync())
                                    {
                                        var mailSeq = reader["MAILSEQ"]?.ToString() ?? "";
                                        var receiver = reader["RECEIVER"]?.ToString() ?? "";

                                        if (!string.IsNullOrEmpty(mailSeq))
                                        {
                                            itemsToApprove.Add((mailSeq, receiver));
                                        }
                                    }
                                }
                            }
                        }
                        catch (SqlException ex) when (ex.Number == 2812) // 找不到 stored procedure
                        {
                            return StatusCode(500, new { error = "找不到批次核准的 stored procedure (XFLdSendMutiSEQ)", details = ex.Message });
                        }
                        catch (Exception ex)
                        {
                            return StatusCode(500, new { error = "取得批次核准項目時發生錯誤", details = ex.Message, stackTrace = ex.StackTrace });
                        }
                    }

                    if (itemsToApprove.Count == 0)
                    {
                        return BadRequest(new { error = "沒有可批次核准的項目" });
                    }

                    // 2. 檢查是否有踢回重審的單據
                    try
                    {
                        using (var checkReExamCmd = new SqlCommand("CURdPaperChkIsReExam_Betch", connection))
                        {
                            checkReExamCmd.CommandType = CommandType.StoredProcedure;
                            checkReExamCmd.Parameters.AddWithValue("@USERID", request.UserId ?? "");

                            using (var reader = await checkReExamCmd.ExecuteReaderAsync())
                            {
                                if (reader.HasRows)
                                {
                                    return BadRequest(new { error = "選有<踢回重審>單據，無法使用「批次核准」功能，請依各單據進行審核!" });
                                }
                            }
                        }
                    }
                    catch (SqlException ex) when (ex.Number == 2812)
                    {
                        // Stored procedure 不存在，跳過此檢查
                    }

                    // 3. 檢查是否有電子發票單據
                    try
                    {
                        using (var checkEInvCmd = new SqlCommand("CURdPaperChkIsEInv_Betch", connection))
                        {
                            checkEInvCmd.CommandType = CommandType.StoredProcedure;
                            checkEInvCmd.Parameters.AddWithValue("@USERID", request.UserId ?? "");

                            using (var reader = await checkEInvCmd.ExecuteReaderAsync())
                            {
                                if (reader.HasRows)
                                {
                                    return BadRequest(new { error = "電子發票單據，無法使用「批次核准」功能，請依各單據進行審核!" });
                                }
                            }
                        }
                    }
                    catch (SqlException ex) when (ex.Number == 2812)
                    {
                        // Stored procedure 不存在，跳過此檢查
                    }

                    // 4. 其他規則檢查
                    try
                    {
                        using (var checkRuleCmd = new SqlCommand("CURdPaperChkIsRule_Betch", connection))
                        {
                            checkRuleCmd.CommandType = CommandType.StoredProcedure;
                            checkRuleCmd.Parameters.AddWithValue("@USERID", request.UserId ?? "");

                            using (var reader = await checkRuleCmd.ExecuteReaderAsync())
                            {
                                if (reader.HasRows && await reader.ReadAsync())
                                {
                                    var returnStr = reader["ReturnStr"]?.ToString();
                                    if (!string.IsNullOrWhiteSpace(returnStr))
                                    {
                                        return BadRequest(new { error = returnStr });
                                    }
                                }
                            }
                        }
                    }
                    catch (SqlException ex) when (ex.Number == 2812)
                    {
                        // Stored procedure 不存在，跳過此檢查
                    }

                    // 5. 對每個項目執行核准
                    foreach (var (mailSeq, receiver) in itemsToApprove)
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(mailSeq))
                            {
                                continue;
                            }

                            var agentId = string.Empty;

                            // 如果 RECEIVER 不是當前用戶，則設定 AGENTID
                            if (!string.IsNullOrEmpty(receiver) && receiver != request.UserId)
                            {
                                agentId = request.UserId ?? "";
                            }

                            using (var handleCmd = new SqlCommand("XFLdMAILHANDLE", connection))
                            {
                                handleCmd.CommandType = CommandType.StoredProcedure;
                                handleCmd.CommandTimeout = 9600; // 與 Delphi 相同的 timeout
                                handleCmd.Parameters.AddWithValue("@MAILSEQ", mailSeq);
                                handleCmd.Parameters.AddWithValue("@HANDLER", string.IsNullOrEmpty(receiver) ? (object)DBNull.Value : receiver);
                                handleCmd.Parameters.AddWithValue("@AGENTID", string.IsNullOrEmpty(agentId) ? (object)DBNull.Value : agentId);
                                handleCmd.Parameters.AddWithValue("@DECISION", "同意");
                                handleCmd.Parameters.AddWithValue("@COMMENT", string.Empty);

                                await handleCmd.ExecuteNonQueryAsync();
                                successCount++;
                            }
                        }
                        catch (SqlException ex)
                        {
                            // 過濾佇列相關的訊息（副本通知寄送 Email）
                            if (ex.Message != null && !ex.Message.StartsWith("佇列"))
                            {
                                errorCount++;
                                errorMessages.Add($"編號 {mailSeq ?? "unknown"}: {ex.Message}");
                            }
                            else
                            {
                                // 佇列錯誤不計入失敗
                                successCount++;
                            }
                        }
                        catch (Exception ex)
                        {
                            errorCount++;
                            errorMessages.Add($"編號 {mailSeq ?? "unknown"}: {ex.Message ?? "未知錯誤"}");
                        }
                    }
                }

                if (errorCount == 0)
                {
                    return Ok(new { message = "批次審核完成!", successCount });
                }
                else
                {
                    return StatusCode(207, new
                    {
                        message = "部分批次審核失敗",
                        successCount,
                        errorCount,
                        errors = errorMessages
                    });
                }
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }
    }

    public class BatchApproveRequest
    {
        public string? UserId { get; set; }
        public List<MailSeqItem>? MailSeqList { get; set; }
    }

    public class MailSeqItem
    {
        public string? MailSeq { get; set; }
        public string? Receiver { get; set; }
    }
}
