using System.Data;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;

/// <summary>
/// 應付帳款沖帳單自訂按鈕 API ��制器
/// 對應 Delphi: StrikeSelectDLL.pas / StrikeSelectDLL.dfm
/// 功能: 選擇可沖帳之憑證加入沖帳單
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class StrikeSelectController : ControllerBase
{
    private readonly string _cs;

    public StrikeSelectController(IConfiguration cfg, PcbErpContext db)
    {
        _cs = cfg.GetConnectionString("DefaultConnection")
              ?? db?.Database.GetDbConnection().ConnectionString
              ?? throw new InvalidOperationException("找不到連線字串");
    }

    /// <summary>
    /// 取得主檔資料 (對應 btnGetParamsClick)
    /// GET /api/StrikeSelect/GetMainData?paperNum=xxx
    /// </summary>
    [HttpGet("GetMainData")]
    public async Task<IActionResult> GetMainData([FromQuery] string paperNum)
    {
        if (string.IsNullOrWhiteSpace(paperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            // 取得沖帳主檔資料
            var sql = @"
                SELECT CompanyId, RateToNTStrike, UseId, IsIn, StrikeMode, MoneyCode
                FROM APRdStrikeMain WITH (NOLOCK)
                WHERE PaperNum = @paperNum";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@paperNum", paperNum);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return Ok(new { ok = false, error = "找不到單據資料" });

            var companyId = reader["CompanyId"]?.ToString()?.Trim() ?? "";
            var rateToNTStrike = reader["RateToNTStrike"] == DBNull.Value ? 0.0 : Convert.ToDouble(reader["RateToNTStrike"]);
            var useId = reader["UseId"]?.ToString() ?? "";
            var isIn = reader["IsIn"] == DBNull.Value ? 0 : Convert.ToInt32(reader["IsIn"]);
            var strikeMode = reader["StrikeMode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["StrikeMode"]);
            var moneyCode = reader["MoneyCode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["MoneyCode"]);

            await reader.CloseAsync();

            // 驗證客戶編號是否必填
            if (string.IsNullOrWhiteSpace(companyId))
            {
                return Ok(new { ok = false, error = "客廠編號是必要欄位，請輸入" });
            }

            // 驗證匯率
            if (rateToNTStrike <= 0)
            {
                return Ok(new { ok = false, error = "幣別匯率不符，請輸入" });
            }

            // 檢查部門模式設定 (SSDepartMode)
            var showDepart = false;
            await using var departCmd = new SqlCommand(
                @"SELECT Value FROM CURdSysParams WITH (NOLOCK)
                  WHERE SystemId = 'SPO' AND ParamId = 'SSDepartMode'
                  AND ISNULL(Value, '') = '1'", conn);
            var departResult = await departCmd.ExecuteScalarAsync();
            if (departResult != null && departResult != DBNull.Value)
            {
                showDepart = true;
            }

            return Ok(new
            {
                ok = true,
                companyId,
                useId,
                isIn,
                strikeMode,
                moneyCode,
                showDepart
            });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 帳款型態選項 (對應 qryIsIn)
    /// GET /api/StrikeSelect/IsInOptions
    /// </summary>
    [HttpGet("IsInOptions")]
    public async Task<IActionResult> IsInOptions()
    {
        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();
            var sql = "SELECT IsIn AS value, IsInName AS text FROM APRdIsIn WITH (NOLOCK)";
            await using var cmd = new SqlCommand(sql, conn);
            var list = new List<object>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new
                {
                    value = reader["value"]?.ToString() ?? "",
                    text = reader["text"]?.ToString() ?? ""
                });
            }
            return Ok(list);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// 公司別選項 (對應 qryUseId)
    /// GET /api/StrikeSelect/UseIdOptions
    /// </summary>
    [HttpGet("UseIdOptions")]
    public async Task<IActionResult> UseIdOptions()
    {
        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();
            var sql = "SELECT BUId AS value, BUName AS text FROM dbo.CURdBU WITH (NOLOCK)";
            await using var cmd = new SqlCommand(sql, conn);
            var list = new List<object>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new
                {
                    value = reader["value"]?.ToString()?.Trim() ?? "",
                    text = reader["text"]?.ToString()?.Trim() ?? ""
                });
            }
            return Ok(list);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// 客廠名稱選項 (對應 qryVCompany)
    /// GET /api/StrikeSelect/CompanyOptions
    /// </summary>
    [HttpGet("CompanyOptions")]
    public async Task<IActionResult> CompanyOptions()
    {
        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();
            var sql = "SELECT CompanyId AS value, ShortName AS text FROM dbo.AJNdCompany WITH (NOLOCK)";
            await using var cmd = new SqlCommand(sql, conn);
            var list = new List<object>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new
                {
                    value = reader["value"]?.ToString() ?? "",
                    text = reader["text"]?.ToString() ?? ""
                });
            }
            return Ok(list);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// 幣別選項 (對應 qryMoneyAll，含「不限」選項)
    /// GET /api/StrikeSelect/MoneyOptions
    /// </summary>
    [HttpGet("MoneyOptions")]
    public async Task<IActionResult> MoneyOptions()
    {
        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();
            var sql = @"
                SELECT MoneyCode AS value, MoneyName AS text
                FROM dbo.AJNdClassMoney WITH (NOLOCK)
                UNION
                SELECT 255, '不限'";
            await using var cmd = new SqlCommand(sql, conn);
            var list = new List<object>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new
                {
                    value = reader["value"]?.ToString() ?? "",
                    text = reader["text"]?.ToString() ?? ""
                });
            }
            return Ok(list);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// 部門選項 (對應 qryDepart)
    /// GET /api/StrikeSelect/DepartOptions
    /// </summary>
    [HttpGet("DepartOptions")]
    public async Task<IActionResult> DepartOptions()
    {
        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();
            var sql = "SELECT DepartId AS value, DepartName AS text FROM dbo.AJNdDepart WITH (NOLOCK)";
            await using var cmd = new SqlCommand(sql, conn);
            var list = new List<object>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new
                {
                    value = reader["value"]?.ToString() ?? "",
                    text = reader["text"]?.ToString() ?? ""
                });
            }
            return Ok(list);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// 取得已選取的資料 (對應 qrySelected - APRdStrikeChoiceed)
    /// GET /api/StrikeSelect/GetSelected?paperNum=xxx
    /// </summary>
    [HttpGet("GetSelected")]
    public async Task<IActionResult> GetSelected([FromQuery] string paperNum)
    {
        if (string.IsNullOrWhiteSpace(paperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("APRdStrikeChoiceed", conn)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 120
            };
            cmd.Parameters.AddWithValue("@PaperNum", paperNum);

            var rows = new List<Dictionary<string, object?>>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object?>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var name = reader.GetName(i);
                    var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    if (value is DateTime dt)
                        row[name] = dt.ToString("yyyy-MM-dd");
                    else
                        row[name] = value;
                }
                rows.Add(row);
            }

            return Ok(new { ok = true, rows });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 查詢可選憑證 (對應 btFindClick - APRdStrikeChoice)
    /// POST /api/StrikeSelect/Search
    /// </summary>
    [HttpPost("Search")]
    public async Task<IActionResult> Search([FromBody] StrikeSearchRequest req)
    {
        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            // 建構條件字串 (對應 Delphi 的 sCondition)
            var conditions = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(req.IsIn))
                conditions.Append($" and t1.IsIn = '{req.IsIn}' ");
            if (!string.IsNullOrWhiteSpace(req.UseId))
                conditions.Append($" and t1.UseId = '{req.UseId}' ");
            if (!string.IsNullOrWhiteSpace(req.PaperDate))
                conditions.Append($" and t1.PaperDate >= '{req.PaperDate}' ");
            if (!string.IsNullOrWhiteSpace(req.ExpectDate))
                conditions.Append($" and t1.ExpectDate <= '{req.ExpectDate}' ");
            if (!string.IsNullOrWhiteSpace(req.PaperNum))
                conditions.Append($" and t1.PaperNum = '{req.PaperNum}' ");
            if (!string.IsNullOrWhiteSpace(req.DepartId))
                conditions.Append($" and t1.DepartId = '{req.DepartId}' ");

            // 幣別處理（255 表示不限）
            if (!string.IsNullOrWhiteSpace(req.MoneyCode) && req.MoneyCode != "255")
                conditions.Append($" and t1.MoneyCode = {req.MoneyCode} ");

            await using var cmd = new SqlCommand("APRdStrikeChoice", conn)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 120
            };
            cmd.Parameters.AddWithValue("@sCondition", conditions.ToString());
            cmd.Parameters.AddWithValue("@CompanyId", (req.CompanyId ?? "").Trim());
            cmd.Parameters.AddWithValue("@StrikePaperNum", req.MainPaperNum ?? "");

            var rows = new List<Dictionary<string, object?>>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object?>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var name = reader.GetName(i);
                    var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    if (value is DateTime dt)
                        row[name] = dt.ToString("yyyy-MM-dd");
                    else
                        row[name] = value;
                }
                rows.Add(row);
            }

            // 排除已存在明細的憑證
            if (req.ExcludePaperNums != null && req.ExcludePaperNums.Count > 0)
            {
                var excludeSet = new HashSet<string>(req.ExcludePaperNums, StringComparer.OrdinalIgnoreCase);
                rows = rows.Where(r =>
                {
                    var paperNum = r.TryGetValue("PaperNum", out var v) ? v?.ToString() : null;
                    return string.IsNullOrEmpty(paperNum) || !excludeSet.Contains(paperNum);
                }).ToList();
            }

            return Ok(new { ok = true, rows });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 取得憑證明細 (對應 msExSelectSourceClick - qryCertif)
    /// GET /api/StrikeSelect/GetCertifDetail?paperNum=xxx
    /// </summary>
    [HttpGet("GetCertifDetail")]
    public async Task<IActionResult> GetCertifDetail([FromQuery] string paperNum)
    {
        if (string.IsNullOrWhiteSpace(paperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            var sql = @"
                SELECT DISTINCT t1.*,
                    ItemSpec = CASE WHEN ISNULL(t1.IsStop, 0) = 0
                        THEN t2.ItemSpec
                        ELSE '暫停付款中' END,
                    MoneyCodeName = (SELECT MoneyName FROM dbo.AJNdClassMoney WITH (NOLOCK) WHERE MoneyCode = t1.MoneyCode),
                    InvoiceTypeName = (SELECT InvoiceTypeName FROM dbo.AJNdClassInvoiceType WITH (NOLOCK) WHERE InvoiceTypeId = t1.InvoiceTypeId),
                    TaxTypeName = (SELECT TaxTypeName FROM ATXdTaxType WITH (NOLOCK) WHERE TaxTypeId = t1.TaxTypeId),
                    PayWayName = (SELECT PayWayName FROM dbo.AJNdClassPayWay WITH (NOLOCK) WHERE PayWayCode = t1.PayWayCode),
                    CertifTypeName = (SELECT CertifTypeName FROM APRdCertifType WITH (NOLOCK) WHERE CertifTypeId = t1.CertifTypeId),
                    TaxCutTypeName = (SELECT TaxCutTypeName FROM ATXdTaxCutType WITH (NOLOCK) WHERE TaxCutTypeId = t1.TaxCutTypeId)
                FROM APRdCertifMain t1 WITH (NOLOCK)
                INNER JOIN APRdPayRecvSub t2 WITH (NOLOCK) ON t1.PaperNum = t2.SourNum
                WHERE t2.PaperNum = @paperNum
                    AND ISNULL(t2.SourNum, '') <> ''";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@paperNum", paperNum);

            var rows = new List<Dictionary<string, object?>>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object?>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var name = reader.GetName(i);
                    var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    if (value is DateTime dt)
                        row[name] = dt.ToString("yyyy-MM-dd");
                    else
                        row[name] = value;
                }
                rows.Add(row);
            }

            return Ok(new { ok = true, rows });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 確定儲存 (對應 btnOKClick)
    /// POST /api/StrikeSelect/Confirm
    /// </summary>
    [HttpPost("Confirm")]
    public async Task<IActionResult> Confirm([FromBody] StrikeConfirmRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.PaperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        if (req.Rows == null || req.Rows.Count == 0)
            return Ok(new { ok = false, error = "沒有選擇任何憑證" });

        await using var conn = new SqlConnection(_cs);
        SqlTransaction? tx = null;

        try
        {
            await conn.OpenAsync();
            tx = (SqlTransaction)await conn.BeginTransactionAsync();

            // 1. 刪除現有沖帳來源明細 (APRdStrikeSourceDel)
            await using var deleteCmd = new SqlCommand("APRdStrikeSourceDel", conn, tx)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 60
            };
            deleteCmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);
            await deleteCmd.ExecuteNonQueryAsync();

            // 2. 取得資料庫連線 SPId
            int spId = 0;
            await using var spIdCmd = new SqlCommand("SELECT @@SPID", conn, tx);
            var spIdResult = await spIdCmd.ExecuteScalarAsync();
            if (spIdResult != null && spIdResult != DBNull.Value)
                spId = Convert.ToInt32(spIdResult);

            // 3. 逐筆插入沖帳來源明細 (APRdStrikeSourceInsPre)
            for (int i = 0; i < req.Rows.Count; i++)
            {
                var sourNum = req.Rows[i];
                var isFirst = i == 0;
                var isLast = i == req.Rows.Count - 1;

                await using var insertCmd = new SqlCommand("APRdStrikeSourceInsPre", conn, tx)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 60
                };
                insertCmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);
                insertCmd.Parameters.AddWithValue("@SourNum", sourNum);
                insertCmd.Parameters.AddWithValue("@ClearDebit", isFirst ? 1 : 0);  // ������筆需要清除����項
                insertCmd.Parameters.AddWithValue("@Last", isLast ? 1 : 0);  // 最後一筆標記
                insertCmd.Parameters.AddWithValue("@SPId", spId);
                insertCmd.Parameters.AddWithValue("@Serial", i);

                try
                {
                    await insertCmd.ExecuteNonQueryAsync();
                }
                catch (Exception ex)
                {
                    await tx.RollbackAsync();
                    return Ok(new { ok = false, error = $"插入第 {i + 1} 筆失敗: {ex.Message}" });
                }
            }

            // 4. 更新狀態為 1 (表示已選擇憑證)
            await using var updateCmd = new SqlCommand(
                "UPDATE APRdStrikeMain SET Status = 1 WHERE PaperNum = @PaperNum", conn, tx);
            updateCmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);
            await updateCmd.ExecuteNonQueryAsync();

            await tx.CommitAsync();
            return Ok(new { ok = true, count = req.Rows.Count });
        }
        catch (Exception ex)
        {
            if (tx != null)
            {
                try { await tx.RollbackAsync(); }
                catch { }
            }
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    // Request Models
    public class StrikeSearchRequest
    {
        public string? IsIn { get; set; }
        public string? UseId { get; set; }
        public string? CompanyId { get; set; }
        public string? PaperDate { get; set; }
        public string? ExpectDate { get; set; }
        public string? PaperNum { get; set; }
        public string? MoneyCode { get; set; }
        public string? DepartId { get; set; }
        public string? MainPaperNum { get; set; }
        public List<string>? ExcludePaperNums { get; set; }
    }

    public class StrikeConfirmRequest
    {
        public string? PaperNum { get; set; }
        public List<string>? Rows { get; set; }
    }

    /// <summary>
    /// 取得 PR000004.cshtml 的內容 (用於動態載入)
    /// GET /api/StrikeSelect/GetViewContent
    /// </summary>
    [HttpGet("GetViewContent")]
    public async Task<IActionResult> GetViewContent()
    {
        try
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Pages", "CustomButton", "PR000004.cshtml");

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound(new { ok = false, error = "找不到 PR000004.cshtml 文件" });
            }

            var content = await System.IO.File.ReadAllTextAsync(filePath);

            return Content(content, "text/html");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { ok = false, error = ex.Message });
        }
    }

    #region 沖帳明細維護 (Strike Detail Maintenance)

    /// <summary>
    /// 取得明細主檔資料
    /// GET /api/StrikeSelect/GetDetailMasterData?paperNum=xxx
    /// </summary>
    [HttpGet("GetDetailMasterData")]
    public async Task<IActionResult> GetDetailMasterData([FromQuery] string paperNum)
    {
        if (string.IsNullOrWhiteSpace(paperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            // 直接查詢 APRdStrikeMain 表格
            var sql = @"
                SELECT RateToNTStrike, RateToNT, StrikeMode, MoneyCode, CashAmountOg, IsIn
                FROM APRdStrikeMain WITH (NOLOCK)
                WHERE PaperNum = @paperNum";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@paperNum", paperNum);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return Ok(new { ok = false, error = "找不到主檔資料" });

            var result = new Dictionary<string, object>
            {
                ["RateToNTStrike"] = reader["RateToNTStrike"] == DBNull.Value ? 0.0 : Convert.ToDouble(reader["RateToNTStrike"]),
                ["RateToNT"] = reader["RateToNT"] == DBNull.Value ? 0.0 : Convert.ToDouble(reader["RateToNT"]),
                ["StrikeMode"] = reader["StrikeMode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["StrikeMode"]),
                ["MoneyCode"] = reader["MoneyCode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["MoneyCode"]),
                ["CashAmountOg"] = reader["CashAmountOg"] == DBNull.Value ? 0.0 : Convert.ToDouble(reader["CashAmountOg"]),
                ["IsIn"] = reader["IsIn"] == DBNull.Value ? 0 : Convert.ToInt32(reader["IsIn"])
            };

            return Ok(new { ok = true, data = result });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 取得沖帳來源資料
    /// GET /api/StrikeSelect/GetDetailSourceData?paperNum=xxx
    /// </summary>
    [HttpGet("GetDetailSourceData")]
    public async Task<IActionResult> GetDetailSourceData([FromQuery] string paperNum)
    {
        if (string.IsNullOrWhiteSpace(paperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            // 直接查詢 APRdStrikeSourceInv 表格
            var sql = @"
                SELECT *
                FROM APRdStrikeSourceInv WITH (NOLOCK)
                WHERE PaperNum = @paperNum
                ORDER BY SortSerial";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@paperNum", paperNum);

            var rows = new List<Dictionary<string, object?>>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object?>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var name = reader.GetName(i);
                    var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    if (value is DateTime dt)
                        row[name] = dt.ToString("yyyy-MM-dd");
                    else
                        row[name] = value;
                }
                rows.Add(row);
            }

            return Ok(new { ok = true, rows });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 取得銀存沖帳明細
    /// GET /api/StrikeSelect/GetDetailBankData?paperNum=xxx
    /// </summary>
    [HttpGet("GetDetailBankData")]
    public async Task<IActionResult> GetDetailBankData([FromQuery] string paperNum)
    {
        if (string.IsNullOrWhiteSpace(paperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            // 直接查詢 APRdStrikeBank 表格，並 JOIN 銀行名稱
            var sql = @"
                SELECT b.*,
                       bank.ShortName AS BankName
                FROM APRdStrikeBank b WITH (NOLOCK)
                LEFT JOIN AJNdBankAccountMain bank WITH (NOLOCK) ON b.BankId = bank.BankId
                WHERE b.PaperNum = @paperNum
                ORDER BY b.Item";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@paperNum", paperNum);

            var rows = new List<Dictionary<string, object?>>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object?>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var name = reader.GetName(i);
                    var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    if (value is DateTime dt)
                        row[name] = dt.ToString("yyyy-MM-dd");
                    else
                        row[name] = value;
                }
                rows.Add(row);
            }

            return Ok(new { ok = true, rows });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 取得票據沖帳明細
    /// GET /api/StrikeSelect/GetDetailBillData?paperNum=xxx
    /// </summary>
    [HttpGet("GetDetailBillData")]
    public async Task<IActionResult> GetDetailBillData([FromQuery] string paperNum)
    {
        if (string.IsNullOrWhiteSpace(paperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            // 直接查詢 APRdStrikeBill 表格，並 JOIN 銀行名稱、票別名稱、科目名稱
            var sql = @"
                SELECT b.*,
                       payBank.ShortName AS PayBankName,
                       recvBank.ShortName AS RecvBankName,
                       bt.BillTypeName AS BillTypeName,
                       ba.AccIdName AS BillAccIdName
                FROM APRdStrikeBill b WITH (NOLOCK)
                LEFT JOIN AJNdBankAccountMain payBank WITH (NOLOCK) ON b.PayBankId = payBank.BankId
                LEFT JOIN AJNdBankAccountMain recvBank WITH (NOLOCK) ON b.RecvBankId = recvBank.BankId
                LEFT JOIN APRdBillType bt WITH (NOLOCK) ON b.BillTypeId = bt.BillTypeId
                LEFT JOIN APRdVStrikeBillAccId ba WITH (NOLOCK) ON b.BillAccId = ba.AccId
                WHERE b.PaperNum = @paperNum
                ORDER BY b.Item";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@paperNum", paperNum);

            var rows = new List<Dictionary<string, object?>>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object?>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var name = reader.GetName(i);
                    var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    if (value is DateTime dt)
                        row[name] = dt.ToString("yyyy-MM-dd");
                    else
                        row[name] = value;
                }
                rows.Add(row);
            }

            return Ok(new { ok = true, rows });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 取得其他扣款明細 (左側：科目明細)
    /// GET /api/StrikeSelect/GetDetailOtherData?paperNum=xxx
    /// </summary>
    [HttpGet("GetDetailOtherData")]
    public async Task<IActionResult> GetDetailOtherData([FromQuery] string paperNum)
    {
        if (string.IsNullOrWhiteSpace(paperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            // 直接查詢 APRdOtherAccDtl 表格，並 JOIN 科目名稱
            var sql = @"
                SELECT o.*,
                       a.AccIdName AS AccIdName,
                       s.SubAccName AS SubAccName
                FROM APRdOtherAccDtl o WITH (NOLOCK)
                LEFT JOIN AJNdAccId a WITH (NOLOCK) ON o.AccId = a.AccId
                LEFT JOIN AJNdSubAccId s WITH (NOLOCK) ON o.AccId = s.AccId AND o.SubAccId = s.SubAccId
                WHERE o.PaperNum = @paperNum
                ORDER BY o.Item";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@paperNum", paperNum);

            var rows = new List<Dictionary<string, object?>>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object?>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var name = reader.GetName(i);
                    var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    if (value is DateTime dt)
                        row[name] = dt.ToString("yyyy-MM-dd");
                    else
                        row[name] = value;
                }
                rows.Add(row);
            }

            return Ok(new { ok = true, rows });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 取得手續費明細 (右側：手續費本位幣)
    /// GET /api/StrikeSelect/GetDetailPostData?paperNum=xxx
    /// </summary>
    [HttpGet("GetDetailPostData")]
    public async Task<IActionResult> GetDetailPostData([FromQuery] string paperNum)
    {
        if (string.IsNullOrWhiteSpace(paperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            // 直接查詢 APRdStrikePost 表格，並 JOIN 銀行和科目名稱
            var sql = @"
                SELECT p.*,
                       bank.ShortName AS PostBankName,
                       a.AccIdName AS AccIdName,
                       s.SubAccName AS SubAccName
                FROM APRdStrikePost p WITH (NOLOCK)
                LEFT JOIN AJNdBankAccountMain bank WITH (NOLOCK) ON p.PostBankId = bank.BankId
                LEFT JOIN AJNdAccId a WITH (NOLOCK) ON p.PostAccId = a.AccId
                LEFT JOIN AJNdSubAccId s WITH (NOLOCK) ON p.PostAccId = s.AccId AND p.PostSubAccId = s.SubAccId
                WHERE p.PaperNum = @paperNum
                ORDER BY p.Item";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@paperNum", paperNum);

            var rows = new List<Dictionary<string, object?>>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object?>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var name = reader.GetName(i);
                    var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    if (value is DateTime dt)
                        row[name] = dt.ToString("yyyy-MM-dd");
                    else
                        row[name] = value;
                }
                rows.Add(row);
            }

            return Ok(new { ok = true, rows });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 取得扣款明細
    /// GET /api/StrikeSelect/GetDetailDebitData?paperNum=xxx
    /// </summary>
    [HttpGet("GetDetailDebitData")]
    public async Task<IActionResult> GetDetailDebitData([FromQuery] string paperNum)
    {
        if (string.IsNullOrWhiteSpace(paperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            // 直接查詢 APRdStrikeSourceDebit 表格
            var sql = @"
                SELECT *
                FROM APRdStrikeSourceDebit WITH (NOLOCK)
                WHERE PaperNum = @paperNum
                ORDER BY Item";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@paperNum", paperNum);

            var rows = new List<Dictionary<string, object?>>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object?>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var name = reader.GetName(i);
                    var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    if (value is DateTime dt)
                        row[name] = dt.ToString("yyyy-MM-dd");
                    else
                        row[name] = value;
                }
                rows.Add(row);
            }

            return Ok(new { ok = true, rows });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 執行自動計算 (對應 APRdStrikeNewAutoCalc)
    /// POST /api/StrikeSelect/DetailAutoCalc
    /// </summary>
    [HttpPost("DetailAutoCalc")]
    public async Task<IActionResult> DetailAutoCalc([FromBody] DetailAutoCalcRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.PaperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("APRdStrikeNewAutoCalc", conn)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 120
            };
            cmd.Parameters.AddWithValue("@PaperNum", req.PaperNum ?? "");
            cmd.Parameters.AddWithValue("@SourNum", req.SourNum ?? "");
            cmd.Parameters.AddWithValue("@Item", req.Item ?? 0);
            cmd.Parameters.AddWithValue("@CertifNum", req.CertifNum ?? "");
            cmd.Parameters.AddWithValue("@FieldName", req.SourceField ?? "");
            cmd.Parameters.AddWithValue("@SourceType", req.SourceType);

            await cmd.ExecuteNonQueryAsync();

            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 取得所有金額總計 (對應 APRdStrikeGetAllAmount)
    /// GET /api/StrikeSelect/GetDetailAllAmount?paperNum=xxx
    /// </summary>
    [HttpGet("GetDetailAllAmount")]
    public async Task<IActionResult> GetDetailAllAmount([FromQuery] string paperNum, [FromQuery] int iPage = 0)
    {
        if (string.IsNullOrWhiteSpace(paperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("APRdStrikeCalcAllAmount", conn)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 120
            };
            cmd.Parameters.AddWithValue("@PaperNum", paperNum);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var amounts = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var name = reader.GetName(i);
                    amounts[name] = reader.IsDBNull(i) ? (object)0.0 : reader.GetValue(i);
                }
                return Ok(new { ok = true, data = amounts });
            }

            return Ok(new { ok = true, data = new Dictionary<string, object>() });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 儲存現金沖帳金額 (對應 Delphi SaveCash: tblMaster.Post 更新 APRdStrikeMain.CashAmountOg)
    /// POST /api/StrikeSelect/SaveDetailCash
    /// </summary>
    [HttpPost("SaveDetailCash")]
    public async Task<IActionResult> SaveDetailCash([FromBody] SaveDetailCashRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.PaperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            // 對應 Delphi: tblMaster.Post → 直接更新 APRdStrikeMain.CashAmountOg
            var sql = @"UPDATE APRdStrikeMain SET CashAmountOg = @CashAmountOg WHERE PaperNum = @PaperNum";
            await using var cmd = new SqlCommand(sql, conn) { CommandTimeout = 120 };
            cmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);
            cmd.Parameters.AddWithValue("@CashAmountOg", req.CashAmountOg);

            await cmd.ExecuteNonQueryAsync();

            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 變更沖帳模式 (對應 APRdStrikeChangeMode)
    /// POST /api/StrikeSelect/ChangeDetailMode
    /// </summary>
    [HttpPost("ChangeDetailMode")]
    public async Task<IActionResult> ChangeDetailMode([FromBody] ChangeDetailModeRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.PaperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("APRdStrikeChangeMode", conn)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 120
            };
            cmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);
            cmd.Parameters.AddWithValue("@StrikeMode", req.StrikeMode);

            await cmd.ExecuteNonQueryAsync();

            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 儲存沖���明細 (對應 btnOK2Click - 多階段儲存)
    /// POST /api/StrikeSelect/SaveDetailStrike
    /// </summary>
    [HttpPost("SaveDetailStrike")]
    public async Task<IActionResult> SaveDetailStrike([FromBody] SaveDetailStrikeRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.PaperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        await using var conn = new SqlConnection(_cs);
        SqlTransaction? tx = null;

        try
        {
            await conn.OpenAsync();
            tx = (SqlTransaction)await conn.BeginTransactionAsync();

            // 1. 檢查是否有帳款來源
            var checkSql = "SELECT COUNT(*) FROM APRdStrikeSource WITH (NOLOCK) WHERE PaperNum = @PaperNum";
            await using var checkCmd = new SqlCommand(checkSql, conn, tx);
            checkCmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);
            var sourceCount = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());

            if (sourceCount == 0)
            {
                await tx.RollbackAsync();
                return Ok(new { ok = false, error = "沒有帳款來源資料，請先選擇憑證" });
            }

            // 2. 執行 APRdStrikeSaveBank (儲存銀存沖帳)
            if (req.SaveBank)
            {
                await using var bankCmd = new SqlCommand("APRdStrikeSaveBank", conn, tx)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 120
                };
                bankCmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);
                await bankCmd.ExecuteNonQueryAsync();
            }

            // 3. 執行 APRdStrikeSaveBill (儲存票據沖帳)
            if (req.SaveBill)
            {
                await using var billCmd = new SqlCommand("APRdStrikeSaveBill", conn, tx)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 120
                };
                billCmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);
                await billCmd.ExecuteNonQueryAsync();
            }

            // 4. 執行 APRdStrikeSaveOther (儲存其他扣款)
            if (req.SaveOther)
            {
                await using var otherCmd = new SqlCommand("APRdStrikeSaveOther", conn, tx)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 120
                };
                otherCmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);
                await otherCmd.ExecuteNonQueryAsync();
            }

            // 5. 執行 APRdStrikeSaveDebit (儲存扣款明細)
            if (req.SaveDebit)
            {
                await using var debitCmd = new SqlCommand("APRdStrikeSaveDebit", conn, tx)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 120
                };
                debitCmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);
                await debitCmd.ExecuteNonQueryAsync();
            }

            // 6. 執行 APRdStrikeCheck (最終驗證)
            await using var checkFinalCmd = new SqlCommand("APRdStrikeCheck", conn, tx)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 120
            };
            checkFinalCmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);

            await checkFinalCmd.ExecuteNonQueryAsync();

            // 7. 更新主檔狀態為 2 (表示已完成明細維護)
            await using var updateCmd = new SqlCommand(
                "UPDATE APRdStrikeMain SET Status = 2 WHERE PaperNum = @PaperNum", conn, tx);
            updateCmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);
            await updateCmd.ExecuteNonQueryAsync();

            await tx.CommitAsync();
            return Ok(new { ok = true, message = "儲存成功" });
        }
        catch (Exception ex)
        {
            if (tx != null)
            {
                try { await tx.RollbackAsync(); }
                catch { }
            }
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 檢查是否需要顯示「兌換對照」頁籤 (對應 qryOtherOg)
    /// GET /api/StrikeSelect/CheckShowCalcTab?paperNum=xxx
    /// </summary>
    [HttpGet("CheckShowCalcTab")]
    public async Task<IActionResult> CheckShowCalcTab([FromQuery] string paperNum)
    {
        if (string.IsNullOrWhiteSpace(paperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            // 執行 stored procedure APRdStrikeUseOtherOg
            await using var cmd = new SqlCommand("APRdStrikeUseOtherOg", conn)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 120
            };
            cmd.Parameters.AddWithValue("@PaperNum", paperNum);
            cmd.Parameters.AddWithValue("@ReWrite", 0);  // 第二個參數固定為 0

            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                return Ok(new
                {
                    ok = true,
                    showCalcTab = false,
                    specialMode = 0
                });
            }

            var result = reader["Result"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Result"]);
            var moneyName = reader["MoneyName"]?.ToString() ?? "";
            var useMoneyName = reader["UseMoneyName"]?.ToString() ?? "";

            // Result = 1 表示需要顯示「兌換對照」頁籤
            var showCalcTab = result == 1;

            return Ok(new
            {
                ok = true,
                showCalcTab,
                specialMode = showCalcTab ? 1 : 0,
                moneyName,
                useMoneyName,
                tabCaption = showCalcTab ? $"{useMoneyName}沖帳" : ""
            });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 取得兌換對照來源資料 (SpecialMode=1 時使用)
    /// GET /api/StrikeSelect/GetDetailSourceCalcData?paperNum=xxx
    /// </summary>
    [HttpGet("GetDetailSourceCalcData")]
    public async Task<IActionResult> GetDetailSourceCalcData([FromQuery] string paperNum)
    {
        if (string.IsNullOrWhiteSpace(paperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            // 直接查詢 APRdStrikeSourceCalcInv 表格 (對應 tblSourceCalc)
            var sql = @"
                SELECT *
                FROM APRdStrikeSourceCalcInv WITH (NOLOCK)
                WHERE PaperNum = @paperNum
                ORDER BY SortSerial";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@paperNum", paperNum);

            var rows = new List<Dictionary<string, object?>>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object?>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var name = reader.GetName(i);
                    var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    if (value is DateTime dt)
                        row[name] = dt.ToString("yyyy-MM-dd");
                    else
                        row[name] = value;
                }
                rows.Add(row);
            }

            return Ok(new { ok = true, rows });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 檢查是否有預收付或對沖可使用 (對應 APRdCheckHaveAdv)
    /// GET /api/StrikeSelect/CheckHaveAdv?paperNum=xxx
    /// </summary>
    [HttpGet("CheckHaveAdv")]
    public async Task<IActionResult> CheckHaveAdv([FromQuery] string paperNum)
    {
        if (string.IsNullOrWhiteSpace(paperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("APRdCheckHaveAdv", conn)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 120
            };
            cmd.Parameters.AddWithValue("@PaperNum", paperNum);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                return Ok(new { ok = true, data = new { isAdv = 0, isOther = 0, discount = "" } });
            }

            var isAdv = reader["IsAdv"] == DBNull.Value ? 0 : Convert.ToInt32(reader["IsAdv"]);
            var isOther = reader["IsOther"] == DBNull.Value ? 0 : Convert.ToInt32(reader["IsOther"]);
            var discount = reader["Discount"]?.ToString() ?? "";

            return Ok(new
            {
                ok = true,
                data = new
                {
                    isAdv,
                    isOther,
                    discount
                }
            });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 差額認列 (對應 Delphi btnElseClick)
    /// POST /api/StrikeSelect/ElseInsert
    /// 執行 SP APRdStrikeElseIns 計算並插入差額到 APRdOtherAccDtl
    /// </summary>
    [HttpPost("ElseInsert")]
    public async Task<IActionResult> ElseInsert([FromBody] ElseInsertRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.PaperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            // 執行差額認列 SP
            await using var cmd = new SqlCommand("APRdStrikeElseIns", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@PaperNum", request.PaperNum);
            await cmd.ExecuteNonQueryAsync();

            // 重新載入其他扣款明細資料
            var sql = @"
                SELECT o.*,
                       a.AccIdName AS AccIdName,
                       s.SubAccName AS SubAccName
                FROM APRdOtherAccDtl o WITH (NOLOCK)
                LEFT JOIN AJNdAccId a WITH (NOLOCK) ON o.AccId = a.AccId
                LEFT JOIN AJNdSubAccId s WITH (NOLOCK) ON o.AccId = s.AccId AND o.SubAccId = s.SubAccId
                WHERE o.PaperNum = @paperNum
                ORDER BY o.Item";

            await using var queryCmd = new SqlCommand(sql, conn);
            queryCmd.Parameters.AddWithValue("@paperNum", request.PaperNum);

            var rows = new List<Dictionary<string, object?>>();
            await using var reader = await queryCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object?>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var name = reader.GetName(i);
                    var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    if (value is DateTime dt)
                        row[name] = dt.ToString("yyyy-MM-dd");
                    else
                        row[name] = value;
                }
                rows.Add(row);
            }

            return Ok(new { ok = true, rows, message = "差額認列完成" });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    #endregion

    // Request Models for Detail Maintenance
    public class ElseInsertRequest
    {
        public string? PaperNum { get; set; }
    }
    public class DetailAutoCalcRequest
    {
        public string? PaperNum { get; set; }
        public string? SourNum { get; set; }
        public int? Item { get; set; }
        public string? CertifNum { get; set; }
        public string? SourceField { get; set; }
        public int SourceType { get; set; }
    }

    public class SaveDetailCashRequest
    {
        public string? PaperNum { get; set; }
        public double CashAmountOg { get; set; }
    }

    /// <summary>
    /// 取得科目選項
    /// GET /api/StrikeSelect/GetAccIdOptions
    /// </summary>
    [HttpGet("GetAccIdOptions")]
    public async Task<IActionResult> GetAccIdOptions()
    {
        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            var sql = @"
                SELECT AccId, AccIdName
                FROM AJNdAccId WITH (NOLOCK)
                ORDER BY AccId";

            await using var cmd = new SqlCommand(sql, conn);

            var rows = new List<Dictionary<string, object?>>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                rows.Add(new Dictionary<string, object?>
                {
                    ["AccId"] = reader["AccId"]?.ToString() ?? "",
                    ["AccIdName"] = reader["AccIdName"]?.ToString() ?? ""
                });
            }

            return Ok(new { ok = true, rows });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 取得子科目選項
    /// GET /api/StrikeSelect/GetSubAccIdOptions?accId=xxx
    /// </summary>
    [HttpGet("GetSubAccIdOptions")]
    public async Task<IActionResult> GetSubAccIdOptions([FromQuery] string accId)
    {
        if (string.IsNullOrWhiteSpace(accId))
            return Ok(new { ok = true, rows = new List<object>() });

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            var sql = @"
                SELECT SubAccId, SubAccName
                FROM AJNdVSubAccId WITH (NOLOCK)
                WHERE AccId = @AccId
                ORDER BY SubAccId";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@AccId", accId);

            var rows = new List<Dictionary<string, object?>>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                rows.Add(new Dictionary<string, object?>
                {
                    ["SubAccId"] = reader["SubAccId"]?.ToString() ?? "",
                    ["SubAccName"] = reader["SubAccName"]?.ToString() ?? ""
                });
            }

            return Ok(new { ok = true, rows });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 取得銀行選項
    /// GET /api/StrikeSelect/GetBankOptions
    /// </summary>
    [HttpGet("GetBankOptions")]
    public async Task<IActionResult> GetBankOptions()
    {
        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            var sql = @"
                SELECT BankId, Name
                FROM APRdVIsAccount WITH (NOLOCK)
                ORDER BY BankId";

            await using var cmd = new SqlCommand(sql, conn);

            var rows = new List<Dictionary<string, object?>>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                rows.Add(new Dictionary<string, object?>
                {
                    ["BankId"] = reader["BankId"]?.ToString()?.Trim() ?? "",
                    ["Name"] = reader["Name"]?.ToString()?.Trim() ?? ""
                });
            }

            return Ok(new { ok = true, rows });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 取得帳戶選項
    /// GET /api/StrikeSelect/GetAccountOptions?bankId=xxx
    /// </summary>
    [HttpGet("GetAccountOptions")]
    public async Task<IActionResult> GetAccountOptions([FromQuery] string bankId)
    {
        if (string.IsNullOrWhiteSpace(bankId))
            return Ok(new { ok = true, rows = new List<object>() });

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            var sql = @"
                SELECT AccountId, ShortName
                FROM AJNdVAccountId WITH (NOLOCK)
                WHERE BankId = @BankId
                ORDER BY AccountId";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@BankId", bankId);

            var rows = new List<Dictionary<string, object?>>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                rows.Add(new Dictionary<string, object?>
                {
                    ["AccountId"] = reader["AccountId"]?.ToString() ?? "",
                    ["ShortName"] = reader["ShortName"]?.ToString() ?? ""
                });
            }

            return Ok(new { ok = true, rows });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    public class ChangeDetailModeRequest
    {
        public string? PaperNum { get; set; }
        public int StrikeMode { get; set; }
    }

    public class SaveDetailStrikeRequest
    {
        public string? PaperNum { get; set; }
        public bool SaveBank { get; set; }
        public bool SaveBill { get; set; }
        public bool SaveOther { get; set; }
        public bool SaveDebit { get; set; }
    }

    public class SaveDetailRowsRequest
    {
        public string? PaperNum { get; set; }
        public List<Dictionary<string, object>>? Rows { get; set; }
    }

    #region 明細儲存 API (Detail Save APIs)

    /// <summary>
    /// 儲存銀存沖帳明細
    /// POST /api/StrikeSelect/SaveDetailBank
    /// </summary>
    [HttpPost("SaveDetailBank")]
    public async Task<IActionResult> SaveDetailBank([FromBody] SaveDetailRowsRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.PaperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        await using var conn = new SqlConnection(_cs);
        SqlTransaction? tx = null;

        try
        {
            await conn.OpenAsync();
            tx = (SqlTransaction)await conn.BeginTransactionAsync();

            // 1. 刪除現有資料
            await using var deleteCmd = new SqlCommand(
                "DELETE FROM APRdStrikeBank WHERE PaperNum = @PaperNum", conn, tx);
            deleteCmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);
            await deleteCmd.ExecuteNonQueryAsync();

            // 2. 插入新資料
            if (req.Rows != null && req.Rows.Count > 0)
            {
                for (int i = 0; i < req.Rows.Count; i++)
                {
                    var row = req.Rows[i];
                    await using var insertCmd = new SqlCommand(@"
                        INSERT INTO APRdStrikeBank (PaperNum, Item, PreCharged, PreChargeDate, BankId, AccountId, AmountOg)
                        VALUES (@PaperNum, @Item, @PreCharged, @PreChargeDate, @BankId, @AccountId, @AmountOg)", conn, tx);

                    insertCmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);
                    insertCmd.Parameters.AddWithValue("@Item", i + 1);
                    insertCmd.Parameters.AddWithValue("@PreCharged", GetIntValue(row, "PreCharged"));
                    insertCmd.Parameters.AddWithValue("@PreChargeDate", GetDateValue(row, "PreChargeDate") ?? (object)DBNull.Value);
                    insertCmd.Parameters.AddWithValue("@BankId", GetStringValue(row, "BankId") ?? (object)DBNull.Value);
                    insertCmd.Parameters.AddWithValue("@AccountId", GetStringValue(row, "AccountId") ?? (object)DBNull.Value);
                    insertCmd.Parameters.AddWithValue("@AmountOg", GetDecimalValue(row, "AmountOg"));

                    await insertCmd.ExecuteNonQueryAsync();
                }
            }

            await tx.CommitAsync();
            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            if (tx != null) try { await tx.RollbackAsync(); } catch { }
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 儲存票據沖帳明細
    /// POST /api/StrikeSelect/SaveDetailBill
    /// </summary>
    [HttpPost("SaveDetailBill")]
    public async Task<IActionResult> SaveDetailBill([FromBody] SaveDetailRowsRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.PaperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        await using var conn = new SqlConnection(_cs);
        SqlTransaction? tx = null;

        try
        {
            await conn.OpenAsync();
            tx = (SqlTransaction)await conn.BeginTransactionAsync();

            // 1. 刪除現有資料
            await using var deleteCmd = new SqlCommand(
                "DELETE FROM APRdStrikeBill WHERE PaperNum = @PaperNum", conn, tx);
            deleteCmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);
            await deleteCmd.ExecuteNonQueryAsync();

            // 2. 插入新資料
            if (req.Rows != null && req.Rows.Count > 0)
            {
                for (int i = 0; i < req.Rows.Count; i++)
                {
                    var row = req.Rows[i];
                    await using var insertCmd = new SqlCommand(@"
                        INSERT INTO APRdStrikeBill (PaperNum, Item, BillId, Amount, AmountOg, PayBankId, PayAccountId,
                            RecvBankId, RecvAccountId, ParaLine, Title, Inhibit, BillAccId, BillTypeId, DueDate,
                            IsExchged, BillCode, IsIn, BankNameRecv, BankNamePay)
                        VALUES (@PaperNum, @Item, @BillId, @Amount, @AmountOg, @PayBankId, @PayAccountId,
                            @RecvBankId, @RecvAccountId, @ParaLine, @Title, @Inhibit, @BillAccId, @BillTypeId, @DueDate,
                            @IsExchged, @BillCode, @IsIn, @BankNameRecv, @BankNamePay)", conn, tx);

                    insertCmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);
                    insertCmd.Parameters.AddWithValue("@Item", i + 1);
                    insertCmd.Parameters.AddWithValue("@BillId", GetStringValue(row, "BillId") ?? (object)DBNull.Value);
                    insertCmd.Parameters.AddWithValue("@Amount", GetDecimalValue(row, "Amount"));
                    insertCmd.Parameters.AddWithValue("@AmountOg", GetDecimalValue(row, "AmountOg"));
                    insertCmd.Parameters.AddWithValue("@PayBankId", GetStringValue(row, "PayBankId") ?? (object)DBNull.Value);
                    insertCmd.Parameters.AddWithValue("@PayAccountId", GetStringValue(row, "PayAccountId") ?? (object)DBNull.Value);
                    insertCmd.Parameters.AddWithValue("@RecvBankId", GetStringValue(row, "RecvBankId") ?? (object)DBNull.Value);
                    insertCmd.Parameters.AddWithValue("@RecvAccountId", GetStringValue(row, "RecvAccountId") ?? (object)DBNull.Value);
                    insertCmd.Parameters.AddWithValue("@ParaLine", GetIntValue(row, "ParaLine"));
                    insertCmd.Parameters.AddWithValue("@Title", GetIntValue(row, "Title"));
                    insertCmd.Parameters.AddWithValue("@Inhibit", GetIntValue(row, "Inhibit"));
                    insertCmd.Parameters.AddWithValue("@BillAccId", GetStringValue(row, "BillAccId") ?? (object)DBNull.Value);
                    insertCmd.Parameters.AddWithValue("@BillTypeId", GetIntValue(row, "BillTypeId") == 0 ? (object)DBNull.Value : GetIntValue(row, "BillTypeId"));
                    insertCmd.Parameters.AddWithValue("@DueDate", GetDateValue(row, "DueDate") ?? (object)DBNull.Value);
                    insertCmd.Parameters.AddWithValue("@IsExchged", GetIntValue(row, "IsExchged"));
                    insertCmd.Parameters.AddWithValue("@BillCode", GetStringValue(row, "BillCode") ?? (object)DBNull.Value);
                    insertCmd.Parameters.AddWithValue("@IsIn", GetIntValue(row, "IsIn"));
                    insertCmd.Parameters.AddWithValue("@BankNameRecv", GetStringValue(row, "BankNameRecv") ?? (object)DBNull.Value);
                    insertCmd.Parameters.AddWithValue("@BankNamePay", GetStringValue(row, "BankNamePay") ?? (object)DBNull.Value);

                    await insertCmd.ExecuteNonQueryAsync();
                }
            }

            await tx.CommitAsync();
            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            if (tx != null) try { await tx.RollbackAsync(); } catch { }
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 儲存其他扣款明細
    /// POST /api/StrikeSelect/SaveDetailOther
    /// </summary>
    [HttpPost("SaveDetailOther")]
    public async Task<IActionResult> SaveDetailOther([FromBody] SaveDetailRowsRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.PaperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        await using var conn = new SqlConnection(_cs);
        SqlTransaction? tx = null;

        try
        {
            await conn.OpenAsync();
            tx = (SqlTransaction)await conn.BeginTransactionAsync();

            // 1. 刪除現有資料
            await using var deleteCmd = new SqlCommand(
                "DELETE FROM APRdOtherAccDtl WHERE PaperNum = @PaperNum", conn, tx);
            deleteCmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);
            await deleteCmd.ExecuteNonQueryAsync();

            // 2. 插入新資料
            if (req.Rows != null && req.Rows.Count > 0)
            {
                for (int i = 0; i < req.Rows.Count; i++)
                {
                    var row = req.Rows[i];
                    await using var insertCmd = new SqlCommand(@"
                        INSERT INTO APRdOtherAccDtl (PaperNum, Item, AccId, SubAccId, Amount, AmountOg, SALNum, IsReverse, Notes)
                        VALUES (@PaperNum, @Item, @AccId, @SubAccId, @Amount, @AmountOg, @SALNum, @IsReverse, @Notes)", conn, tx);

                    insertCmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);
                    insertCmd.Parameters.AddWithValue("@Item", i + 1);
                    insertCmd.Parameters.AddWithValue("@AccId", GetStringValue(row, "AccId") ?? (object)DBNull.Value);
                    insertCmd.Parameters.AddWithValue("@SubAccId", GetStringValue(row, "SubAccId") ?? (object)DBNull.Value);
                    insertCmd.Parameters.AddWithValue("@Amount", GetDecimalValue(row, "Amount") == 0 ? (object)DBNull.Value : GetDecimalValue(row, "Amount"));
                    insertCmd.Parameters.AddWithValue("@AmountOg", GetDecimalValue(row, "AmountOg") == 0 ? (object)DBNull.Value : GetDecimalValue(row, "AmountOg"));
                    insertCmd.Parameters.AddWithValue("@SALNum", GetStringValue(row, "SALNum") ?? (object)DBNull.Value);
                    insertCmd.Parameters.AddWithValue("@IsReverse", GetIntValue(row, "IsReverse") == 0 ? (object)DBNull.Value : GetIntValue(row, "IsReverse"));
                    insertCmd.Parameters.AddWithValue("@Notes", GetStringValue(row, "Notes") ?? (object)DBNull.Value);

                    await insertCmd.ExecuteNonQueryAsync();
                }
            }

            await tx.CommitAsync();
            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            if (tx != null) try { await tx.RollbackAsync(); } catch { }
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 儲存手續費明細
    /// POST /api/StrikeSelect/SaveDetailPost
    /// </summary>
    [HttpPost("SaveDetailPost")]
    public async Task<IActionResult> SaveDetailPost([FromBody] SaveDetailRowsRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.PaperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        await using var conn = new SqlConnection(_cs);
        SqlTransaction? tx = null;

        try
        {
            await conn.OpenAsync();
            tx = (SqlTransaction)await conn.BeginTransactionAsync();

            // 1. 刪除現有資料
            await using var deleteCmd = new SqlCommand(
                "DELETE FROM APRdStrikePost WHERE PaperNum = @PaperNum", conn, tx);
            deleteCmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);
            await deleteCmd.ExecuteNonQueryAsync();

            // 2. 插入新資料
            if (req.Rows != null && req.Rows.Count > 0)
            {
                for (int i = 0; i < req.Rows.Count; i++)
                {
                    var row = req.Rows[i];
                    await using var insertCmd = new SqlCommand(@"
                        INSERT INTO APRdStrikePost (PaperNum, Item, PostAccId, PostSubAccId, PostBankId, PostAccountId, PostAmount)
                        VALUES (@PaperNum, @Item, @PostAccId, @PostSubAccId, @PostBankId, @PostAccountId, @PostAmount)", conn, tx);

                    insertCmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);
                    insertCmd.Parameters.AddWithValue("@Item", i + 1);
                    insertCmd.Parameters.AddWithValue("@PostAccId", GetStringValue(row, "PostAccId") ?? (object)DBNull.Value);
                    insertCmd.Parameters.AddWithValue("@PostSubAccId", GetStringValue(row, "PostSubAccId") ?? (object)DBNull.Value);
                    insertCmd.Parameters.AddWithValue("@PostBankId", GetStringValue(row, "PostBankId") ?? (object)DBNull.Value);
                    insertCmd.Parameters.AddWithValue("@PostAccountId", GetStringValue(row, "PostAccountId") ?? (object)DBNull.Value);
                    insertCmd.Parameters.AddWithValue("@PostAmount", GetDecimalValue(row, "PostAmount") == 0 ? (object)DBNull.Value : GetDecimalValue(row, "PostAmount"));

                    await insertCmd.ExecuteNonQueryAsync();
                }
            }

            await tx.CommitAsync();
            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            if (tx != null) try { await tx.RollbackAsync(); } catch { }
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 儲存扣款明細
    /// POST /api/StrikeSelect/SaveDetailDebit
    /// </summary>
    [HttpPost("SaveDetailDebit")]
    public async Task<IActionResult> SaveDetailDebit([FromBody] SaveDetailRowsRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.PaperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        await using var conn = new SqlConnection(_cs);
        SqlTransaction? tx = null;

        try
        {
            await conn.OpenAsync();
            tx = (SqlTransaction)await conn.BeginTransactionAsync();

            if (req.Rows != null)
            {
                foreach (var row in req.Rows)
                {
                    await using var updateCmd = new SqlCommand(@"
                        UPDATE APRdStrikeSourceDebit
                        SET SourItem = @SourItem, Notes = @Notes,
                            RestAmountOg = @RestAmountOg, RestAmount = @RestAmount,
                            CutAmountOg = @CutAmountOg, CutAmount = @CutAmount,
                            RateToNT = @RateToNT, SALNum = @SALNum, InvoiceNum = @InvoiceNum
                        WHERE PaperNum = @PaperNum AND Item = @Item
                            AND SourNum = @SourNum AND CertifNum = @CertifNum", conn, tx);

                    updateCmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);
                    updateCmd.Parameters.AddWithValue("@Item", GetIntValue(row, "Item"));
                    updateCmd.Parameters.AddWithValue("@SourNum", GetStringValue(row, "SourNum") ?? "");
                    updateCmd.Parameters.AddWithValue("@CertifNum", GetStringValue(row, "CertifNum") ?? "");
                    updateCmd.Parameters.AddWithValue("@SourItem", GetIntValue(row, "SourItem") == 0 ? (object)DBNull.Value : GetIntValue(row, "SourItem"));
                    updateCmd.Parameters.AddWithValue("@Notes", GetStringValue(row, "Notes") ?? (object)DBNull.Value);
                    updateCmd.Parameters.AddWithValue("@RestAmountOg", GetDecimalValue(row, "RestAmountOg"));
                    updateCmd.Parameters.AddWithValue("@RestAmount", GetDecimalValue(row, "RestAmount"));
                    updateCmd.Parameters.AddWithValue("@CutAmountOg", GetDecimalValue(row, "CutAmountOg"));
                    updateCmd.Parameters.AddWithValue("@CutAmount", GetDecimalValue(row, "CutAmount"));
                    updateCmd.Parameters.AddWithValue("@RateToNT", GetDecimalValue(row, "RateToNT"));
                    updateCmd.Parameters.AddWithValue("@SALNum", GetStringValue(row, "SALNum") ?? (object)DBNull.Value);
                    updateCmd.Parameters.AddWithValue("@InvoiceNum", GetStringValue(row, "InvoiceNum") ?? "");

                    await updateCmd.ExecuteNonQueryAsync();
                }
            }

            await tx.CommitAsync();
            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            if (tx != null) try { await tx.RollbackAsync(); } catch { }
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 儲存沖帳帳款明細
    /// POST /api/StrikeSelect/SaveDetailSource
    /// </summary>
    [HttpPost("SaveDetailSource")]
    public async Task<IActionResult> SaveDetailSource([FromBody] SaveDetailRowsRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.PaperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        await using var conn = new SqlConnection(_cs);
        SqlTransaction? tx = null;

        try
        {
            await conn.OpenAsync();
            tx = (SqlTransaction)await conn.BeginTransactionAsync();

            if (req.Rows != null)
            {
                foreach (var row in req.Rows)
                {
                    await using var updateCmd = new SqlCommand(@"
                        UPDATE APRdStrikeSourceInv
                        SET SourItem = @SourItem, Notes = @Notes,
                            RestAmountOg = @RestAmountOg, CashAmountOg = @CashAmountOg,
                            BankAmountOg = @BankAmountOg, BillAmountOg = @BillAmountOg,
                            CutAmountOg = @CutAmountOg, OtherAmountOg = @OtherAmountOg,
                            RestAmount = @RestAmount, CashAmount = @CashAmount,
                            BankAmount = @BankAmount, BillAmount = @BillAmount,
                            CutAmount = @CutAmount, OtherAmount = @OtherAmount,
                            RateToNT = @RateToNT, SALNum = @SALNum,
                            DelDate = @DelDate, JourId = @JourId,
                            StrikeAmount = @StrikeAmount, StrikeAmountOg = @StrikeAmountOg,
                            SortSerial = @SortSerial
                        WHERE PaperNum = @PaperNum AND Item = @Item
                            AND SourNum = @SourNum AND CertifNum = @CertifNum", conn, tx);

                    updateCmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);
                    updateCmd.Parameters.AddWithValue("@Item", GetIntValue(row, "Item"));
                    updateCmd.Parameters.AddWithValue("@SourNum", GetStringValue(row, "SourNum") ?? "");
                    updateCmd.Parameters.AddWithValue("@CertifNum", GetStringValue(row, "CertifNum") ?? "");
                    updateCmd.Parameters.AddWithValue("@SourItem", GetIntValue(row, "SourItem") == 0 ? (object)DBNull.Value : GetIntValue(row, "SourItem"));
                    updateCmd.Parameters.AddWithValue("@Notes", GetStringValue(row, "Notes") ?? (object)DBNull.Value);
                    updateCmd.Parameters.AddWithValue("@RestAmountOg", GetDecimalValue(row, "RestAmountOg"));
                    updateCmd.Parameters.AddWithValue("@CashAmountOg", GetDecimalValue(row, "CashAmountOg"));
                    updateCmd.Parameters.AddWithValue("@BankAmountOg", GetDecimalValue(row, "BankAmountOg"));
                    updateCmd.Parameters.AddWithValue("@BillAmountOg", GetDecimalValue(row, "BillAmountOg"));
                    updateCmd.Parameters.AddWithValue("@CutAmountOg", GetDecimalValue(row, "CutAmountOg"));
                    updateCmd.Parameters.AddWithValue("@OtherAmountOg", GetDecimalValue(row, "OtherAmountOg"));
                    updateCmd.Parameters.AddWithValue("@RestAmount", GetDecimalValue(row, "RestAmount"));
                    updateCmd.Parameters.AddWithValue("@CashAmount", GetDecimalValue(row, "CashAmount"));
                    updateCmd.Parameters.AddWithValue("@BankAmount", GetDecimalValue(row, "BankAmount"));
                    updateCmd.Parameters.AddWithValue("@BillAmount", GetDecimalValue(row, "BillAmount"));
                    updateCmd.Parameters.AddWithValue("@CutAmount", GetDecimalValue(row, "CutAmount"));
                    updateCmd.Parameters.AddWithValue("@OtherAmount", GetDecimalValue(row, "OtherAmount"));
                    updateCmd.Parameters.AddWithValue("@RateToNT", GetDecimalValue(row, "RateToNT"));
                    updateCmd.Parameters.AddWithValue("@SALNum", GetStringValue(row, "SALNum") ?? (object)DBNull.Value);
                    updateCmd.Parameters.AddWithValue("@DelDate", GetDateValue(row, "DelDate") ?? (object)DBNull.Value);
                    updateCmd.Parameters.AddWithValue("@JourId", GetStringValue(row, "JourId") ?? (object)DBNull.Value);
                    updateCmd.Parameters.AddWithValue("@StrikeAmount", GetDecimalValue(row, "StrikeAmount"));
                    updateCmd.Parameters.AddWithValue("@StrikeAmountOg", GetDecimalValue(row, "StrikeAmountOg"));
                    updateCmd.Parameters.AddWithValue("@SortSerial", GetIntValue(row, "SortSerial") == 0 ? (object)DBNull.Value : GetIntValue(row, "SortSerial"));

                    await updateCmd.ExecuteNonQueryAsync();
                }
            }

            await tx.CommitAsync();
            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            if (tx != null) try { await tx.RollbackAsync(); } catch { }
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 取得票據付款銀行選項
    /// GET /api/StrikeSelect/GetBillPayBankOptions
    /// </summary>
    [HttpGet("GetBillPayBankOptions")]
    public async Task<IActionResult> GetBillPayBankOptions([FromQuery] int isIn)
    {
        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            var sql = @"
                SELECT BankId, Name
                FROM APRdVStrikePayBankId WITH (NOLOCK)
                WHERE IsIn = @IsIn
                ORDER BY BankId";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@IsIn", isIn);

            var rows = new List<Dictionary<string, object?>>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                rows.Add(new Dictionary<string, object?>
                {
                    ["BankId"] = reader["BankId"]?.ToString()?.Trim() ?? "",
                    ["Name"] = reader["Name"]?.ToString()?.Trim() ?? ""
                });
            }

            return Ok(new { ok = true, rows });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 取得票據收款銀行選項
    /// GET /api/StrikeSelect/GetBillRecvBankOptions
    /// </summary>
    [HttpGet("GetBillRecvBankOptions")]
    public async Task<IActionResult> GetBillRecvBankOptions([FromQuery] int isIn)
    {
        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            var sql = @"
                SELECT BankId, Name
                FROM APRdVStrikeRecvBankId WITH (NOLOCK)
                WHERE IsIn = @IsIn
                ORDER BY BankId";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@IsIn", isIn);

            var rows = new List<Dictionary<string, object?>>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                rows.Add(new Dictionary<string, object?>
                {
                    ["BankId"] = reader["BankId"]?.ToString()?.Trim() ?? "",
                    ["Name"] = reader["Name"]?.ToString()?.Trim() ?? ""
                });
            }

            return Ok(new { ok = true, rows });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 取得票據帳戶選項（付款帳號 / 收款帳號，按銀行篩選）
    /// GET /api/StrikeSelect/GetBillAccountIdOptions?bankId=xxx
    /// </summary>
    [HttpGet("GetBillAccountIdOptions")]
    public async Task<IActionResult> GetBillAccountIdOptions([FromQuery] string bankId, [FromQuery] int isIn)
    {
        if (string.IsNullOrWhiteSpace(bankId))
            return Ok(new { ok = true, rows = new List<object>() });

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            var sql = @"
                SELECT AccountId, SaveTypeName
                FROM APRdVStrikeBillAccountId WITH (NOLOCK)
                WHERE (RecvBankId = @BankId OR PayBankId = @BankId)
                AND IsIn = @IsIn
                ORDER BY AccountId";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@BankId", bankId.Trim());
            cmd.Parameters.AddWithValue("@IsIn", isIn);

            var rows = new List<Dictionary<string, object?>>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                rows.Add(new Dictionary<string, object?>
                {
                    ["AccountId"] = reader["AccountId"]?.ToString()?.Trim() ?? "",
                    ["SaveTypeName"] = reader["SaveTypeName"]?.ToString()?.Trim() ?? ""
                });
            }

            return Ok(new { ok = true, rows });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 取得票別選項
    /// GET /api/StrikeSelect/GetBillTypeOptions
    /// </summary>
    [HttpGet("GetBillTypeOptions")]
    public async Task<IActionResult> GetBillTypeOptions()
    {
        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            var sql = @"
                SELECT BillTypeId, BillTypeName
                FROM APRdBillType WITH (NOLOCK)
                ORDER BY BillTypeId";

            await using var cmd = new SqlCommand(sql, conn);

            var rows = new List<Dictionary<string, object?>>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                rows.Add(new Dictionary<string, object?>
                {
                    ["BillTypeId"] = reader["BillTypeId"]?.ToString() ?? "",
                    ["BillTypeName"] = reader["BillTypeName"]?.ToString()?.Trim() ?? ""
                });
            }

            return Ok(new { ok = true, rows });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 取得票別代號選項
    /// GET /api/StrikeSelect/GetBillCodeOptions
    /// </summary>
    [HttpGet("GetBillCodeOptions")]
    public async Task<IActionResult> GetBillCodeOptions([FromQuery] string bankId, [FromQuery] int isIn)
    {
        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            var sql = @"
                SELECT BillCode
                FROM APRdVStrikeBillCode WITH (NOLOCK)
                WHERE PayBankId = @PayBankId
                AND IsIn = @IsIn
                ORDER BY BillCode";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@PayBankId", bankId?.Trim() ?? "");
            cmd.Parameters.AddWithValue("@IsIn", isIn);

            var rows = new List<Dictionary<string, object?>>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                rows.Add(new Dictionary<string, object?>
                {
                    ["BillCode"] = reader["BillCode"]?.ToString()?.Trim() ?? ""
                });
            }

            return Ok(new { ok = true, rows });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 取得票據科目選項
    /// GET /api/StrikeSelect/GetBillAccIdOptions
    /// </summary>
    [HttpGet("GetBillAccIdOptions")]
    public async Task<IActionResult> GetBillAccIdOptions([FromQuery] int isIn)
    {
        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            var sql = @"
                SELECT AccId, AccIdName
                FROM APRdVStrikeBillAccId WITH (NOLOCK)
                WHERE IsIn = @IsIn
                ORDER BY AccId";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@IsIn", isIn);

            var rows = new List<Dictionary<string, object?>>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                rows.Add(new Dictionary<string, object?>
                {
                    ["AccId"] = reader["AccId"]?.ToString()?.Trim() ?? "",
                    ["AccIdName"] = reader["AccIdName"]?.ToString()?.Trim() ?? ""
                });
            }

            return Ok(new { ok = true, rows });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    #endregion

    #region 預收付沖帳 (Advance Strike) - 對應 Delphi StrikeSelectadvance

    /// <summary>
    /// 取得預收付沖帳主檔資料 (沖銷模式、匯率等)
    /// GET /api/StrikeSelect/GetAdvanceMainData?paperNum=xxx
    /// </summary>
    [HttpGet("GetAdvanceMainData")]
    public async Task<IActionResult> GetAdvanceMainData([FromQuery] string paperNum)
    {
        if (string.IsNullOrWhiteSpace(paperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            var sql = @"
                SELECT StrikeMode, RateToNTStrike, MoneyCode, IsIn
                FROM APRdStrikeMain WITH (NOLOCK)
                WHERE PaperNum = @paperNum";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@paperNum", paperNum);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return Ok(new { ok = false, error = "找不到單據資料" });

            var strikeMode = reader["StrikeMode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["StrikeMode"]);
            var rateToNTStrike = reader["RateToNTStrike"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["RateToNTStrike"]);
            var moneyCode = reader["MoneyCode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["MoneyCode"]);
            var isIn = reader["IsIn"] == DBNull.Value ? 0 : Convert.ToInt32(reader["IsIn"]);

            await reader.CloseAsync();

            // 取得沖銷模式名稱
            var strikeModeName = strikeMode switch
            {
                0 => "外幣",
                1 => "本位幣",
                _ => "未知"
            };

            // 取得幣別名稱
            string moneyName = "";
            await using var moneyCmd = new SqlCommand(
                "SELECT MoneyName FROM dbo.AJNdClassMoney WITH (NOLOCK) WHERE MoneyCode = @moneyCode", conn);
            moneyCmd.Parameters.AddWithValue("@moneyCode", moneyCode);
            var moneyResult = await moneyCmd.ExecuteScalarAsync();
            if (moneyResult != null && moneyResult != DBNull.Value)
                moneyName = moneyResult.ToString() ?? "";

            return Ok(new
            {
                ok = true,
                strikeMode,
                strikeModeName,
                rateToNTStrike,
                moneyCode,
                moneyName,
                isIn
            });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 取得預收付沖帳清單 (執行 APRdStrikeChoiceadvance 並查詢 APRdAdvanceInStrike)
    /// GET /api/StrikeSelect/GetAdvanceList?paperNum=xxx
    /// </summary>
    [HttpGet("GetAdvanceList")]
    public async Task<IActionResult> GetAdvanceList([FromQuery] string paperNum)
    {
        if (string.IsNullOrWhiteSpace(paperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            // 先執行 SP 初始化資料
            await using var spCmd = new SqlCommand("APRdStrikeChoiceadvance", conn)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 120
            };
            spCmd.Parameters.AddWithValue("@PaperNum", paperNum);
            await spCmd.ExecuteNonQueryAsync();

            // 查詢 APRdAdvanceInStrike 表取得清單
            var sql = @"
                SELECT
                    t1.PaperNum,
                    t1.SourNum,
                    t1.PaperDate,
                    t1.TotalAmountOg,
                    t1.TotalAmount,
                    t1.MoneyCode,
                    t1.StrikeAmountOg,
                    t1.StrikeAmount,
                    t1.OriAmountOg,
                    t1.InInvoice,
                    t1.InPayRecv,
                    t1.InvoiceNum,
                    t1.PayRecvNum,
                    t1.CertifNum,
                    m.MoneyName
                FROM APRdAdvanceInStrike t1 WITH (NOLOCK)
                LEFT JOIN dbo.AJNdClassMoney m WITH (NOLOCK) ON t1.MoneyCode = m.MoneyCode
                WHERE t1.PaperNum = @paperNum
                ORDER BY t1.SourNum";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@paperNum", paperNum);

            var rows = new List<Dictionary<string, object?>>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object?>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var name = reader.GetName(i);
                    var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    if (value is DateTime dt)
                        row[name] = dt.ToString("yyyy-MM-dd");
                    else
                        row[name] = value;
                }
                rows.Add(row);
            }

            return Ok(new { ok = true, rows });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 計算預收付沖帳金額 (執行 APRdStrikeAdvCalc，對應雙擊事件)
    /// POST /api/StrikeSelect/CalculateAdvance
    /// </summary>
    [HttpPost("CalculateAdvance")]
    public async Task<IActionResult> CalculateAdvance([FromBody] AdvanceCalcRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.PaperNum) || string.IsNullOrWhiteSpace(req?.SourNum))
            return Ok(new { ok = false, error = "缺少必要參數" });

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            // 執行 APRdStrikeAdvCalc
            await using var cmd = new SqlCommand("APRdStrikeAdvCalc", conn)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 60
            };
            cmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);
            cmd.Parameters.AddWithValue("@SourNum", req.SourNum);
            await cmd.ExecuteNonQueryAsync();

            // 重新查詢該筆資料
            var sql = @"
                SELECT
                    t1.StrikeAmountOg,
                    t1.StrikeAmount,
                    t1.OriAmountOg
                FROM APRdAdvanceInStrike t1 WITH (NOLOCK)
                WHERE t1.PaperNum = @paperNum AND t1.SourNum = @sourNum";

            await using var queryCmd = new SqlCommand(sql, conn);
            queryCmd.Parameters.AddWithValue("@paperNum", req.PaperNum);
            queryCmd.Parameters.AddWithValue("@sourNum", req.SourNum);

            await using var reader = await queryCmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return Ok(new
                {
                    ok = true,
                    strikeAmountOg = reader["StrikeAmountOg"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["StrikeAmountOg"]),
                    strikeAmount = reader["StrikeAmount"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["StrikeAmount"]),
                    oriAmountOg = reader["OriAmountOg"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["OriAmountOg"])
                });
            }

            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 更新預收付沖帳金額 (用戶手動編輯欄位)
    /// POST /api/StrikeSelect/UpdateAdvanceAmount
    /// </summary>
    [HttpPost("UpdateAdvanceAmount")]
    public async Task<IActionResult> UpdateAdvanceAmount([FromBody] AdvanceUpdateRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.PaperNum) || string.IsNullOrWhiteSpace(req?.SourNum))
            return Ok(new { ok = false, error = "缺少必要參數" });

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            var sql = @"
                UPDATE APRdAdvanceInStrike
                SET StrikeAmountOg = @strikeAmountOg,
                    StrikeAmount = @strikeAmount,
                    OriAmountOg = @oriAmountOg
                WHERE PaperNum = @paperNum AND SourNum = @sourNum";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@paperNum", req.PaperNum);
            cmd.Parameters.AddWithValue("@sourNum", req.SourNum);
            cmd.Parameters.AddWithValue("@strikeAmountOg", req.StrikeAmountOg);
            cmd.Parameters.AddWithValue("@strikeAmount", req.StrikeAmount);
            cmd.Parameters.AddWithValue("@oriAmountOg", req.OriAmountOg);

            var affected = await cmd.ExecuteNonQueryAsync();
            return Ok(new { ok = true, affected });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 確定儲存預收付沖帳 (對應 btnOKClick)
    /// POST /api/StrikeSelect/ConfirmAdvance
    /// </summary>
    [HttpPost("ConfirmAdvance")]
    public async Task<IActionResult> ConfirmAdvance([FromBody] AdvanceConfirmRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.PaperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        await using var conn = new SqlConnection(_cs);
        SqlTransaction? tx = null;

        try
        {
            await conn.OpenAsync();
            tx = (SqlTransaction)await conn.BeginTransactionAsync();

            // 查詢所有有沖帳金額的記錄
            var selectSql = @"
                SELECT SourNum, StrikeAmount, StrikeAmountOg
                FROM APRdAdvanceInStrike WITH (NOLOCK)
                WHERE PaperNum = @paperNum
                  AND StrikeAmount > 0
                  AND StrikeAmountOg > 0
                ORDER BY SourNum";

            await using var selectCmd = new SqlCommand(selectSql, conn, tx);
            selectCmd.Parameters.AddWithValue("@paperNum", req.PaperNum);

            var recordsToProcess = new List<(string SourNum, decimal StrikeAmount, decimal StrikeAmountOg)>();
            await using (var reader = await selectCmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var sourNum = reader["SourNum"]?.ToString() ?? "";
                    var strikeAmount = reader["StrikeAmount"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["StrikeAmount"]);
                    var strikeAmountOg = reader["StrikeAmountOg"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["StrikeAmountOg"]);
                    recordsToProcess.Add((sourNum, strikeAmount, strikeAmountOg));
                }
            }

            bool hasAdvance = recordsToProcess.Count > 0;

            if (hasAdvance)
            {
                // 逐筆執行 APRdStrikeSourceInsadvance
                for (int i = 0; i < recordsToProcess.Count; i++)
                {
                    var (sourNum, _, _) = recordsToProcess[i];
                    var deleteHis = i == 0 ? 1 : 0; // 第一筆 DeleteHis = 1，其餘 = 0

                    await using var insertCmd = new SqlCommand("APRdStrikeSourceInsadvance", conn, tx)
                    {
                        CommandType = CommandType.StoredProcedure,
                        CommandTimeout = 60
                    };
                    insertCmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);
                    insertCmd.Parameters.AddWithValue("@SourNum", sourNum);
                    insertCmd.Parameters.AddWithValue("@Notes", "預收付沖帳款");
                    insertCmd.Parameters.AddWithValue("@DeleteHis", deleteHis);
                    await insertCmd.ExecuteNonQueryAsync();
                }
            }
            else
            {
                // 沒有使用預收，刪除對應記錄
                var deleteSql = @"
                    DELETE FROM APRdOtherAccDtl
                    WHERE PaperNum = @paperNum
                      AND ISNULL(SALNum, '') = 'Advance'";

                await using var deleteCmd = new SqlCommand(deleteSql, conn, tx);
                deleteCmd.Parameters.AddWithValue("@paperNum", req.PaperNum);
                await deleteCmd.ExecuteNonQueryAsync();
            }

            await tx.CommitAsync();
            return Ok(new { ok = true, hasAdvance, processedCount = recordsToProcess.Count });
        }
        catch (Exception ex)
        {
            if (tx != null)
                await tx.RollbackAsync();
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 重新整理預收付沖帳資料
    /// POST /api/StrikeSelect/RefreshAdvance
    /// </summary>
    [HttpPost("RefreshAdvance")]
    public async Task<IActionResult> RefreshAdvance([FromBody] AdvanceRefreshRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.PaperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        // 直接呼叫 GetAdvanceList 來重新載入資料
        return await GetAdvanceList(req.PaperNum);
    }

    // Request Models for Advance Strike
    public class AdvanceCalcRequest
    {
        public string? PaperNum { get; set; }
        public string? SourNum { get; set; }
    }

    public class AdvanceUpdateRequest
    {
        public string? PaperNum { get; set; }
        public string? SourNum { get; set; }
        public decimal StrikeAmountOg { get; set; }
        public decimal StrikeAmount { get; set; }
        public decimal OriAmountOg { get; set; }
    }

    public class AdvanceConfirmRequest
    {
        public string? PaperNum { get; set; }
    }

    public class AdvanceRefreshRequest
    {
        public string? PaperNum { get; set; }
    }

    #endregion

    #region 對沖帳功能 (Other Strike - StrikeSelectOther)

    /// <summary>
    /// 取得對沖帳主檔資料
    /// GET /api/StrikeSelect/GetOtherStrikeMainData?paperNum=xxx
    /// </summary>
    [HttpGet("GetOtherStrikeMainData")]
    public async Task<IActionResult> GetOtherStrikeMainData([FromQuery] string paperNum)
    {
        if (string.IsNullOrWhiteSpace(paperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            // 取得沖帳主檔資料
            var sql = @"
                SELECT StrikeMode, RateToNTStrike
                FROM APRdStrikeMain WITH (NOLOCK)
                WHERE PaperNum = @paperNum";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@paperNum", paperNum);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return Ok(new { ok = false, error = "找不到主檔資料" });

            var strikeMode = reader["StrikeMode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["StrikeMode"]);
            var rateToNTStrike = reader["RateToNTStrike"] == DBNull.Value ? 0.0 : Convert.ToDouble(reader["RateToNTStrike"]);

            var strikeModeName = strikeMode == 0 ? "外幣" : "本位幣";

            return Ok(new { ok = true, strikeMode, strikeModeName, rateToNTStrike });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 取得對沖帳清單 (呼叫 APRdStrikeChoiceOther 初始化資料，然後查詢 APRdOtherInStrike)
    /// GET /api/StrikeSelect/GetOtherStrikeList?paperNum=xxx
    /// </summary>
    [HttpGet("GetOtherStrikeList")]
    public async Task<IActionResult> GetOtherStrikeList([FromQuery] string paperNum)
    {
        if (string.IsNullOrWhiteSpace(paperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            // 1. 呼叫 APRdStrikeChoiceOther 初始化資料
            await using var initCmd = new SqlCommand("APRdStrikeChoiceOther", conn)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 60
            };
            initCmd.Parameters.AddWithValue("@PaperNum", paperNum);
            await initCmd.ExecuteNonQueryAsync();

            // 2. 查詢 APRdOtherInStrike 取得資料
            var sql = @"
                SELECT PaperNum, SourNum, PaperDate, TotalAmount, TotalAmountOg,
                       MoneyName, StrikeAmount, StrikeAmountOg, OriAmountOg
                FROM APRdOtherInStrike WITH (NOLOCK)
                WHERE PaperNum = @paperNum
                ORDER BY SourNum";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@paperNum", paperNum);

            var rows = new List<Dictionary<string, object?>>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object?>
                {
                    ["PaperNum"] = reader["PaperNum"]?.ToString()?.Trim(),
                    ["SourNum"] = reader["SourNum"]?.ToString()?.Trim(),
                    ["PaperDate"] = reader["PaperDate"] == DBNull.Value ? null : ((DateTime)reader["PaperDate"]).ToString("yyyy-MM-dd"),
                    ["TotalAmount"] = reader["TotalAmount"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["TotalAmount"]),
                    ["TotalAmountOg"] = reader["TotalAmountOg"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["TotalAmountOg"]),
                    ["MoneyName"] = reader["MoneyName"]?.ToString()?.Trim(),
                    ["StrikeAmount"] = reader["StrikeAmount"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["StrikeAmount"]),
                    ["StrikeAmountOg"] = reader["StrikeAmountOg"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["StrikeAmountOg"]),
                    ["OriAmountOg"] = reader["OriAmountOg"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["OriAmountOg"])
                };
                rows.Add(row);
            }

            return Ok(new { ok = true, rows });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 計算對沖帳金額 (呼叫 APRdStrikeOtherCalc)
    /// POST /api/StrikeSelect/CalculateOtherStrike
    /// </summary>
    [HttpPost("CalculateOtherStrike")]
    public async Task<IActionResult> CalculateOtherStrike([FromBody] OtherStrikeCalcRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.PaperNum) || string.IsNullOrWhiteSpace(req?.SourNum))
            return Ok(new { ok = false, error = "缺少必要參數" });

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            // 呼叫 APRdStrikeOtherCalc 計算金額
            await using var calcCmd = new SqlCommand("APRdStrikeOtherCalc", conn)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 60
            };
            calcCmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);
            calcCmd.Parameters.AddWithValue("@SourNum", req.SourNum);
            await calcCmd.ExecuteNonQueryAsync();

            // 查詢更新後的資料
            var sql = @"
                SELECT StrikeAmount, StrikeAmountOg, OriAmountOg
                FROM APRdOtherInStrike WITH (NOLOCK)
                WHERE PaperNum = @paperNum AND SourNum = @sourNum";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@paperNum", req.PaperNum);
            cmd.Parameters.AddWithValue("@sourNum", req.SourNum);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return Ok(new
                {
                    ok = true,
                    strikeAmount = reader["StrikeAmount"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["StrikeAmount"]),
                    strikeAmountOg = reader["StrikeAmountOg"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["StrikeAmountOg"]),
                    oriAmountOg = reader["OriAmountOg"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["OriAmountOg"])
                });
            }

            return Ok(new { ok = false, error = "找不到資料" });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 更新對沖帳金額
    /// POST /api/StrikeSelect/UpdateOtherStrikeAmount
    /// </summary>
    [HttpPost("UpdateOtherStrikeAmount")]
    public async Task<IActionResult> UpdateOtherStrikeAmount([FromBody] OtherStrikeUpdateRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.PaperNum) || string.IsNullOrWhiteSpace(req?.SourNum))
            return Ok(new { ok = false, error = "缺少必要參數" });

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            var sql = @"
                UPDATE APRdOtherInStrike
                SET StrikeAmountOg = @StrikeAmountOg,
                    StrikeAmount = @StrikeAmount,
                    OriAmountOg = @OriAmountOg
                WHERE PaperNum = @PaperNum AND SourNum = @SourNum";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);
            cmd.Parameters.AddWithValue("@SourNum", req.SourNum);
            cmd.Parameters.AddWithValue("@StrikeAmountOg", req.StrikeAmountOg);
            cmd.Parameters.AddWithValue("@StrikeAmount", req.StrikeAmount);
            cmd.Parameters.AddWithValue("@OriAmountOg", req.OriAmountOg);

            await cmd.ExecuteNonQueryAsync();
            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 確認對沖帳 (將 APRdOtherInStrike 的資料處理完成)
    /// POST /api/StrikeSelect/ConfirmOtherStrike
    /// </summary>
    [HttpPost("ConfirmOtherStrike")]
    public async Task<IActionResult> ConfirmOtherStrike([FromBody] OtherStrikeConfirmRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.PaperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            // 檢查是否有選擇對沖帳
            var checkSql = @"
                SELECT COUNT(*) FROM APRdOtherInStrike WITH (NOLOCK)
                WHERE PaperNum = @PaperNum
                  AND (StrikeAmountOg > 0 OR StrikeAmount > 0)";

            await using var checkCmd = new SqlCommand(checkSql, conn);
            checkCmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);
            var count = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());

            if (count == 0)
            {
                return Ok(new { ok = true, hasOtherStrike = false, processedCount = 0 });
            }

            return Ok(new { ok = true, hasOtherStrike = true, processedCount = count });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 重新載入對沖帳清單
    /// POST /api/StrikeSelect/RefreshOtherStrike
    /// </summary>
    [HttpPost("RefreshOtherStrike")]
    public async Task<IActionResult> RefreshOtherStrike([FromBody] OtherStrikeRefreshRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.PaperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        // 直接呼叫 GetOtherStrikeList 來重新載入資料
        return await GetOtherStrikeList(req.PaperNum);
    }

    // Request Models for Other Strike
    public class OtherStrikeCalcRequest
    {
        public string? PaperNum { get; set; }
        public string? SourNum { get; set; }
    }

    public class OtherStrikeUpdateRequest
    {
        public string? PaperNum { get; set; }
        public string? SourNum { get; set; }
        public decimal StrikeAmountOg { get; set; }
        public decimal StrikeAmount { get; set; }
        public decimal OriAmountOg { get; set; }
    }

    public class OtherStrikeConfirmRequest
    {
        public string? PaperNum { get; set; }
    }

    public class OtherStrikeRefreshRequest
    {
        public string? PaperNum { get; set; }
    }

    #endregion

    #region Helper Methods

    private static string? GetStringValue(Dictionary<string, object> row, string key)
    {
        if (row.TryGetValue(key, out var value) && value != null)
            return value.ToString()?.Trim();
        return null;
    }

    private static int GetIntValue(Dictionary<string, object> row, string key)
    {
        if (row.TryGetValue(key, out var value) && value != null)
        {
            if (value is int intVal) return intVal;
            if (value is long longVal) return (int)longVal;
            if (value is double doubleVal) return (int)doubleVal;
            if (value is decimal decVal) return (int)decVal;
            if (int.TryParse(value.ToString(), out var parsed)) return parsed;
        }
        return 0;
    }

    private static decimal GetDecimalValue(Dictionary<string, object> row, string key)
    {
        if (row.TryGetValue(key, out var value) && value != null)
        {
            if (value is decimal decVal) return decVal;
            if (value is double doubleVal) return (decimal)doubleVal;
            if (value is int intVal) return intVal;
            if (value is long longVal) return longVal;
            if (decimal.TryParse(value.ToString(), out var parsed)) return parsed;
        }
        return 0;
    }

    private static DateTime? GetDateValue(Dictionary<string, object> row, string key)
    {
        if (row.TryGetValue(key, out var value) && value != null)
        {
            if (value is DateTime dtVal) return dtVal;
            if (DateTime.TryParse(value.ToString(), out var parsed)) return parsed;
        }
        return null;
    }

    #endregion

    #region 折讓搜尋 (Debit Search) - 對應 Delphi StrikeSelectDebit

    /// <summary>
    /// 搜尋折讓單 (對應 btFindClick - APRdStrikeChoiceDebit)
    /// POST /api/StrikeSelect/SearchDebit
    /// </summary>
    [HttpPost("SearchDebit")]
    public async Task<IActionResult> SearchDebit([FromBody] DebitSearchRequest req)
    {
        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("APRdStrikeChoiceDebit", conn)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 120
            };
            cmd.Parameters.AddWithValue("@PaperNum", req.StrikePaperNum ?? "");

            // @DebitDate 和 @ExpectDate 是 datetime 類型
            if (!string.IsNullOrWhiteSpace(req.DebitDate) && DateTime.TryParse(req.DebitDate, out var debitDate))
                cmd.Parameters.AddWithValue("@DebitDate", debitDate);
            else
                cmd.Parameters.AddWithValue("@DebitDate", DBNull.Value);

            if (!string.IsNullOrWhiteSpace(req.ExpectDate) && DateTime.TryParse(req.ExpectDate, out var expectDate))
                cmd.Parameters.AddWithValue("@ExpectDate", expectDate);
            else
                cmd.Parameters.AddWithValue("@ExpectDate", DBNull.Value);

            cmd.Parameters.AddWithValue("@DebitNum", req.DebitNum ?? "");
            cmd.Parameters.AddWithValue("@DepartId", req.DepartId ?? "");

            var rows = new List<Dictionary<string, object?>>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object?>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var name = reader.GetName(i);
                    var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    if (value is DateTime dt)
                        row[name] = dt.ToString("yyyy-MM-dd");
                    else
                        row[name] = value;
                }
                rows.Add(row);
            }

            return Ok(new { ok = true, rows });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 確認折讓明細 (對應 btnOKClick - APRDStrikeSourceInsDebit)
    /// POST /api/StrikeSelect/ConfirmDebit
    /// </summary>
    [HttpPost("ConfirmDebit")]
    public async Task<IActionResult> ConfirmDebit([FromBody] DebitConfirmRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.PaperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        if (req.Rows == null || req.Rows.Count == 0)
            return Ok(new { ok = false, error = "沒有選擇任何折讓單" });

        await using var conn = new SqlConnection(_cs);
        SqlTransaction? tx = null;

        try
        {
            await conn.OpenAsync();
            tx = (SqlTransaction)await conn.BeginTransactionAsync();

            // 逐筆插入折讓明細 (APRDStrikeSourceInsDebit)
            foreach (var item in req.Rows)
            {
                await using var insertCmd = new SqlCommand("APRDStrikeSourceInsDebit", conn, tx)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 60
                };
                insertCmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);
                insertCmd.Parameters.AddWithValue("@SourNum", item.SourNum ?? "");
                insertCmd.Parameters.AddWithValue("@Notes", item.Notes ?? "");

                await insertCmd.ExecuteNonQueryAsync();
            }

            await tx.CommitAsync();
            return Ok(new { ok = true, count = req.Rows.Count });
        }
        catch (Exception ex)
        {
            if (tx != null)
            {
                try { await tx.RollbackAsync(); }
                catch { }
            }
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 清除折讓明細 (對應 qryUpdate - 清空折讓沖銷金額)
    /// POST /api/StrikeSelect/ClearDebitData
    /// </summary>
    [HttpPost("ClearDebitData")]
    public async Task<IActionResult> ClearDebitData([FromBody] ClearDebitRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.PaperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            // 清除折讓沖銷金額 (對應 qryUpdate)
            await using var cmd = new SqlCommand(@"
                UPDATE t1
                SET CutAmountOg = 0
                FROM APRdStrikeSource t1
                WHERE PaperNum = @PaperNum", conn);
            cmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);
            await cmd.ExecuteNonQueryAsync();

            // 刪除折讓明細記錄
            await using var delCmd = new SqlCommand(@"
                DELETE FROM APRdStrikeSourceDebit
                WHERE PaperNum = @PaperNum", conn);
            delCmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);
            await delCmd.ExecuteNonQueryAsync();

            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    // 折讓搜尋請求模型
    public class DebitSearchRequest
    {
        public string? StrikePaperNum { get; set; }
        public string? DebitDate { get; set; }
        public string? ExpectDate { get; set; }
        public string? DebitNum { get; set; }
        public string? DepartId { get; set; }
    }

    // 折讓確認請求模型
    public class DebitConfirmRequest
    {
        public string? PaperNum { get; set; }
        public List<DebitConfirmItem>? Rows { get; set; }
    }

    public class DebitConfirmItem
    {
        public string? SourNum { get; set; }
        public string? Notes { get; set; }
    }

    // 清除折讓請求模型
    public class ClearDebitRequest
    {
        public string? PaperNum { get; set; }
    }

    #endregion

    #region 上移/下移功能

    /// <summary>
    /// 移動沖帳明細順序 (對應 APRdStrikeSerialChg)
    /// POST /api/StrikeSelect/MoveRow
    /// </summary>
    [HttpPost("MoveRow")]
    public async Task<IActionResult> MoveRow([FromBody] MoveRowRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.PaperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        if (string.IsNullOrWhiteSpace(req.SourceTable))
            return Ok(new { ok = false, error = "沒有來源資料表" });

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("APRdStrikeSerialChg", conn)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 60
            };
            cmd.Parameters.AddWithValue("@SourceTable", req.SourceTable);
            cmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);
            cmd.Parameters.AddWithValue("@Item", req.Item);
            cmd.Parameters.AddWithValue("@Direction", req.Direction); // 1=上移, 0=下移

            await cmd.ExecuteNonQueryAsync();
            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    public class MoveRowRequest
    {
        public string? SourceTable { get; set; }
        public string? PaperNum { get; set; }
        public int Item { get; set; }
        public int Direction { get; set; } // 1=上移, 0=下移
    }

    #endregion

    #region 超收/等收處理

    /// <summary>
    /// 檢查沖帳差額並取得處理選項
    /// POST /api/StrikeSelect/CheckOverPayment
    /// </summary>
    [HttpPost("CheckOverPayment")]
    public async Task<IActionResult> CheckOverPayment([FromBody] CheckOverPaymentRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.PaperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            // 計算沖帳差額
            var sql = @"
                SELECT
                    ISNULL(SUM(StrikeAmount), 0) AS TotalStrike,
                    ISNULL(SUM(CashAmount), 0) + ISNULL(SUM(BankAmount), 0) +
                    ISNULL(SUM(BillAmount), 0) + ISNULL(SUM(OtherAmount), 0) +
                    ISNULL(SUM(CutAmount), 0) AS TotalPaid
                FROM APRdStrikeSource WITH (NOLOCK)
                WHERE PaperNum = @PaperNum";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);

            await using var reader = await cmd.ExecuteReaderAsync();
            decimal totalStrike = 0;
            decimal totalPaid = 0;

            if (await reader.ReadAsync())
            {
                totalStrike = reader["TotalStrike"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["TotalStrike"]);
                totalPaid = reader["TotalPaid"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["TotalPaid"]);
            }

            var difference = totalPaid - totalStrike;

            // 差額為 0 表示完全相等，不需要處理
            if (difference == 0)
                return Ok(new { ok = true, needHandle = false, difference = 0 });

            // 有差額，需要處理
            return Ok(new
            {
                ok = true,
                needHandle = true,
                difference,
                totalStrike,
                totalPaid,
                isOverPayment = difference > 0 // 超收
            });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 取得會計科目選項 (用於超收科目選定)
    /// GET /api/StrikeSelect/GetOverpaymentAccountOptions
    /// </summary>
    [HttpGet("GetOverpaymentAccountOptions")]
    public async Task<IActionResult> GetOverpaymentAccountOptions([FromQuery] string? accType)
    {
        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            // 取得會計科目
            var accSql = @"
                SELECT AccId AS value, AccName AS text
                FROM ACRdAccount WITH (NOLOCK)
                WHERE ISNULL(IsStop, 0) = 0
                ORDER BY AccId";

            await using var accCmd = new SqlCommand(accSql, conn);
            var accList = new List<object>();
            await using var accReader = await accCmd.ExecuteReaderAsync();
            while (await accReader.ReadAsync())
            {
                accList.Add(new
                {
                    value = accReader["value"]?.ToString() ?? "",
                    text = $"{accReader["value"]} - {accReader["text"]}"
                });
            }

            return Ok(new { ok = true, accounts = accList });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 取得子目選項 (用於超收科目選定)
    /// GET /api/StrikeSelect/GetSubAccountOptions
    /// </summary>
    [HttpGet("GetSubAccountOptions")]
    public async Task<IActionResult> GetSubAccountOptions([FromQuery] string accId)
    {
        if (string.IsNullOrWhiteSpace(accId))
            return Ok(new { ok = true, subAccounts = new List<object>() });

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            var sql = @"
                SELECT SubAccId AS value, SubAccName AS text
                FROM ACRdSubAccount WITH (NOLOCK)
                WHERE AccId = @AccId AND ISNULL(IsStop, 0) = 0
                ORDER BY SubAccId";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@AccId", accId);

            var list = new List<object>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new
                {
                    value = reader["value"]?.ToString() ?? "",
                    text = $"{reader["value"]} - {reader["text"]}"
                });
            }

            return Ok(new { ok = true, subAccounts = list });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 處理超收/等收
    /// POST /api/StrikeSelect/HandleOverPayment
    /// </summary>
    [HttpPost("HandleOverPayment")]
    public async Task<IActionResult> HandleOverPayment([FromBody] HandleOverPaymentRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.PaperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        await using var conn = new SqlConnection(_cs);
        SqlTransaction? tx = null;

        try
        {
            await conn.OpenAsync();
            tx = (SqlTransaction)await conn.BeginTransactionAsync();

            switch (req.HandleType)
            {
                case 1: // 放棄差額 - 不做任何處理
                    break;

                case 2: // 轉入其他科目 - 建立其他沖銷明細
                    if (string.IsNullOrWhiteSpace(req.AccId))
                        return Ok(new { ok = false, error = "請選擇會計科目" });

                    await using (var insertCmd = new SqlCommand(@"
                        INSERT INTO APRdStrikeOther (PaperNum, AccId, SubAccId, Amount, Notes)
                        VALUES (@PaperNum, @AccId, @SubAccId, @Amount, @Notes)", conn, tx))
                    {
                        insertCmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);
                        insertCmd.Parameters.AddWithValue("@AccId", req.AccId);
                        insertCmd.Parameters.AddWithValue("@SubAccId", req.SubAccId ?? "");
                        insertCmd.Parameters.AddWithValue("@Amount", req.Amount);
                        insertCmd.Parameters.AddWithValue("@Notes", "超收轉入");
                        await insertCmd.ExecuteNonQueryAsync();
                    }
                    break;

                case 3: // 等收處理 - 保留差額待下次沖帳
                    await using (var updateCmd = new SqlCommand(@"
                        UPDATE APRdStrikeMain
                        SET WaitAmount = ISNULL(WaitAmount, 0) + @Amount
                        WHERE PaperNum = @PaperNum", conn, tx))
                    {
                        updateCmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);
                        updateCmd.Parameters.AddWithValue("@Amount", req.Amount);
                        await updateCmd.ExecuteNonQueryAsync();
                    }
                    break;

                default:
                    return Ok(new { ok = false, error = "無效的處理方式" });
            }

            await tx.CommitAsync();
            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            if (tx != null)
            {
                try { await tx.RollbackAsync(); }
                catch { }
            }
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    public class CheckOverPaymentRequest
    {
        public string? PaperNum { get; set; }
    }

    public class HandleOverPaymentRequest
    {
        public string? PaperNum { get; set; }
        public int HandleType { get; set; } // 1=放棄差額, 2=轉入其他科目, 3=等收處理
        public decimal Amount { get; set; }
        public string? AccId { get; set; }
        public string? SubAccId { get; set; }
    }

    #endregion

    #region 補滿原值功能

    /// <summary>
    /// 補滿原值 (對應 APRdStrikeNewAutoFull，用於 SpecialMode=1)
    /// POST /api/StrikeSelect/AutoFillOriginal
    /// </summary>
    [HttpPost("AutoFillOriginal")]
    public async Task<IActionResult> AutoFillOriginal([FromBody] AutoFillOriginalRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.PaperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("APRdStrikeNewAutoFull", conn)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 60
            };
            cmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);

            await cmd.ExecuteNonQueryAsync();
            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    public class AutoFillOriginalRequest
    {
        public string? PaperNum { get; set; }
    }

    #endregion

    #region 票號自動生成

    /// <summary>
    /// 取得自動生成的票號 (對應 APRdBillNum)
    /// POST /api/StrikeSelect/GenerateBillNumber
    /// </summary>
    [HttpPost("GenerateBillNumber")]
    public async Task<IActionResult> GenerateBillNumber([FromBody] GenerateBillNumberRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.PaperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("APRdBillNum", conn)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 60
            };
            cmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);
            cmd.Parameters.AddWithValue("@BillType", req.BillType ?? "");

            // 輸出參數接收票號
            var billNumParam = new SqlParameter("@BillNum", SqlDbType.NVarChar, 50)
            {
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(billNumParam);

            await cmd.ExecuteNonQueryAsync();

            var billNum = billNumParam.Value?.ToString() ?? "";
            return Ok(new { ok = true, billNum });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    public class GenerateBillNumberRequest
    {
        public string? PaperNum { get; set; }
        public string? BillType { get; set; }
    }

    #endregion

    #region 確認後處理

    /// <summary>
    /// 執行確認後的彙總處理 (對應 APRdStrikeSubInsSum)
    /// POST /api/StrikeSelect/ExecuteSubInsSum
    /// </summary>
    [HttpPost("ExecuteSubInsSum")]
    public async Task<IActionResult> ExecuteSubInsSum([FromBody] SubInsSumRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.PaperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("APRdStrikeSubInsSum", conn)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 120
            };
            cmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);

            await cmd.ExecuteNonQueryAsync();
            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    public class SubInsSumRequest
    {
        public string? PaperNum { get; set; }
    }

    /// <summary>
    /// 清除其他沖帳暫存 (對應 APRdStrikeOtherClear，非正常關閉時呼叫)
    /// POST /api/StrikeSelect/ClearOtherTemp
    /// </summary>
    [HttpPost("ClearOtherTemp")]
    public async Task<IActionResult> ClearOtherTemp([FromBody] ClearOtherTempRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.PaperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("APRdStrikeOtherClear", conn)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 60
            };
            cmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);

            await cmd.ExecuteNonQueryAsync();
            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    public class ClearOtherTempRequest
    {
        public string? PaperNum { get; set; }
    }

    #endregion

    #region SAL模式 / 多國語言 / 折讓自動分配

    /// <summary>
    /// 取得 SAL 模式 (出納收款模式)
    /// GET /api/StrikeSelect/GetSALMode?paperNum=xxx
    /// 對應 Delphi: iSALMode 判斷邏輯
    /// </summary>
    [HttpGet("GetSALMode")]
    public async Task<IActionResult> GetSALMode([FromQuery] string paperNum)
    {
        if (string.IsNullOrWhiteSpace(paperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            // 檢查 RecvMode 參數
            var sqlRecvMode = @"
                SELECT COUNT(1) FROM CURdSysParams WITH (NOLOCK)
                WHERE SystemId = 'APR' AND ParamId = 'RecvMode'
                  AND ISNULL(Value, '') = '1'";
            await using var cmdRecv = new SqlCommand(sqlRecvMode, conn);
            var recvCount = Convert.ToInt32(await cmdRecv.ExecuteScalarAsync());

            if (recvCount == 0)
                return Ok(new { ok = true, iSALMode = 0 });

            // 查詢 APRdStrikeMain.IsSAL
            var sqlIsSAL = @"
                SELECT ISNULL(IsSAL, 0) AS IsSAL
                FROM APRdStrikeMain WITH (NOLOCK)
                WHERE PaperNum = @paperNum";
            await using var cmdSAL = new SqlCommand(sqlIsSAL, conn);
            cmdSAL.Parameters.AddWithValue("@paperNum", paperNum);
            var isSAL = Convert.ToInt32(await cmdSAL.ExecuteScalarAsync() ?? 0);

            // iSALMode: 直接對應 IsSAL 值 (0=一般, 1=出納已收款, 2=出納收款)
            var iSALMode = isSAL;
            return Ok(new { ok = true, iSALMode });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 取得多國語言欄位標題 (對應 GetFieldCaption)
    /// GET /api/StrikeSelect/GetFieldCaptions
    /// </summary>
    [HttpGet("GetFieldCaptions")]
    public async Task<IActionResult> GetFieldCaptions()
    {
        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            // 取得語系設定
            var sqlLang = @"
                SELECT ISNULL(Value, '') AS Value FROM CURdSysParams WITH (NOLOCK)
                WHERE SystemId = 'APR' AND ParamId = 'StrikeFormLang'";
            await using var cmdLang = new SqlCommand(sqlLang, conn);
            var langValue = (await cmdLang.ExecuteScalarAsync())?.ToString()?.Trim() ?? "";

            if (string.IsNullOrEmpty(langValue))
                return Ok(new { ok = true, captions = new List<object>() });

            // 查詢對應語言欄位標題
            var sql = @"
                SELECT f.FieldName,
                       ISNULL(l.DisplayLabel, f.DisplayLabel) AS DisplayLabel,
                       ISNULL(l.Items, f.Items) AS Items
                FROM CURdTableField f WITH (NOLOCK)
                LEFT JOIN CURdTableFieldLang l WITH (NOLOCK)
                    ON f.TableName = l.TableName AND f.FieldName = l.FieldName AND l.LangCode = @langCode
                WHERE f.TableName = 'APRdStrikeDtlForm'
                ORDER BY f.FieldName";
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@langCode", langValue);

            var captions = new List<Dictionary<string, object?>>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                captions.Add(new Dictionary<string, object?>
                {
                    ["fieldName"] = reader["FieldName"]?.ToString()?.Trim() ?? "",
                    ["displayLabel"] = reader["DisplayLabel"]?.ToString()?.Trim() ?? "",
                    ["items"] = reader["Items"]?.ToString()?.Trim() ?? ""
                });
            }

            return Ok(new { ok = true, captions });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 折讓自動分配 (對應 DebitAutoSet / APRdStrikeDebitAllSend)
    /// POST /api/StrikeSelect/DebitAutoSend
    /// </summary>
    [HttpPost("DebitAutoSend")]
    public async Task<IActionResult> DebitAutoSend([FromBody] DebitAutoSendRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.PaperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            // 檢查 DebitAutoSet 參數
            var sqlParam = @"
                SELECT COUNT(1) FROM CURdSysParams WITH (NOLOCK)
                WHERE SystemId = 'APR' AND ParamId = 'DebitAutoSet'
                  AND ISNULL(Value, '') = '1'";
            await using var cmdParam = new SqlCommand(sqlParam, conn);
            var autoSet = Convert.ToInt32(await cmdParam.ExecuteScalarAsync()) > 0;

            if (!autoSet)
                return Ok(new { ok = true, autoSet = false });

            // 執行自動分配 SP
            await using var cmd = new SqlCommand("APRdStrikeDebitAllSend", conn)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 120
            };
            cmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);
            await cmd.ExecuteNonQueryAsync();

            return Ok(new { ok = true, autoSet = true });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    public class DebitAutoSendRequest
    {
        public string? PaperNum { get; set; }
    }

    #endregion
}