using System.Data;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;

/// <summary>
/// 預收付明細維護 API 控制器
/// 對應 Delphi: AdvanceDtlDLL
/// 支援模組: advance (預收付), advanceRev (預收付沖銷), recv (收款)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AdvanceDtlController : ControllerBase
{
    private readonly string _cs;

    public AdvanceDtlController(IConfiguration cfg, PcbErpContext db)
    {
        _cs = cfg.GetConnectionString("DefaultConnection")
              ?? db?.Database.GetDbConnection().ConnectionString
              ?? throw new InvalidOperationException("找不到連線字串");
    }

    // 模組配置
    private static readonly Dictionary<string, ModuleConfig> _moduleConfigs = new(StringComparer.OrdinalIgnoreCase)
    {
        ["advance"] = new ModuleConfig(
            MainTable: "APRdAdvanceMain",
            SourceTable: "APRdAdvanceSource",
            BillTable: "APRdAdvanceBill",
            OtherAccDtlTable: "APRdAdvanceOtherAccDtl",
            StrikePostTable: "APRdAdvanceStrikePost",
            CheckProc: "APRDAdvanceCheck",
            AutoCountProc: "APRdAdvanceAutoCount",
            InsertSumProc: "APRdAdvanceSubInsSum",
            PayBankIdProc: "APRDAdvancePayBankIdGet",
            RecvBankIdProc: "APRDAdvanceRecvBankIdGet"
        ),
        ["advancerev"] = new ModuleConfig(
            MainTable: "APRdAdvanceRevMain",
            SourceTable: "APRdAdvanceRevSource",
            BillTable: "APRdAdvanceRevBill",
            OtherAccDtlTable: "APRdAdvanceRevOtherAccDtl",
            StrikePostTable: "APRdAdvanceRevStrikePost",
            CheckProc: "APRDAdvanceRevCheck",
            AutoCountProc: "APRdAdvanceRevAutoCount",
            InsertSumProc: "APRdAdvanceRevSubInsSum",
            PayBankIdProc: "APRDAdvanceRevPayBankIdGet",
            RecvBankIdProc: "APRDAdvanceRecvBankIdGet"
        ),
        ["recv"] = new ModuleConfig(
            MainTable: "SPOdRecvMain",
            SourceTable: "SPOdRecvSource",
            BillTable: "SPOdRecvBill",
            OtherAccDtlTable: "",
            StrikePostTable: "",
            CheckProc: "SPOdRecvCheck",
            AutoCountProc: "SPOdRecvAutoCount",
            InsertSumProc: "SPOdRecvSubInsSum",
            PayBankIdProc: "SPOdRecvBankIdGet",
            RecvBankIdProc: "SPOdRecvBankIdGet",
            CompanyIdColumn: "CustomerId",
            HasIsIn: false
        )
    };

    private record ModuleConfig(
        string MainTable,
        string SourceTable,
        string BillTable,
        string OtherAccDtlTable,
        string StrikePostTable,
        string CheckProc,
        string AutoCountProc,
        string InsertSumProc,
        string PayBankIdProc,
        string RecvBankIdProc,
        string CompanyIdColumn = "CompanyId",  // 客戶/廠商編號欄位名稱
        bool HasIsIn = true                     // 是否有 IsIn 欄位
    );

    /// <summary>
    /// 檢查必要欄位 (客戶編號、幣別匯率)
    /// GET /api/AdvanceDtl/Check?moduleType=advance&paperNum=xxx
    /// </summary>
    [HttpGet("Check")]
    public async Task<IActionResult> Check([FromQuery] string moduleType, [FromQuery] string paperNum)
    {
        if (string.IsNullOrWhiteSpace(paperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        if (!_moduleConfigs.TryGetValue(moduleType ?? "advance", out var cfg))
            return Ok(new { ok = false, error = "無效的模組類型" });

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            // 根據模組配置決定查詢欄位
            var selectColumns = cfg.HasIsIn
                ? $"{cfg.CompanyIdColumn}, IsIn, RateToNT, StrikeMode"
                : $"{cfg.CompanyIdColumn}, RateToNT, StrikeMode";

            var sql = $"SELECT {selectColumns} FROM [{cfg.MainTable}] WITH (NOLOCK) WHERE PaperNum = @paperNum";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@paperNum", paperNum);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return Ok(new { ok = false, error = "找不到單據資料" });

            var companyId = reader[cfg.CompanyIdColumn]?.ToString() ?? "";
            var rateToNT = reader["RateToNT"] == DBNull.Value ? 0 : Convert.ToDouble(reader["RateToNT"]);
            var isIn = cfg.HasIsIn && reader["IsIn"] != DBNull.Value ? Convert.ToInt32(reader["IsIn"]) : 0;
            var strikeMode = reader["StrikeMode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["StrikeMode"]);

            if (string.IsNullOrWhiteSpace(companyId))
                return Ok(new { ok = false, error = "客廠編號是必要欄位，請輸入" });

            if (rateToNT <= 0)
                return Ok(new { ok = false, error = "幣別匯率不符，請輸入" });

            return Ok(new { ok = true, isIn, strikeMode, rateToNT });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 取得主檔資料
    /// GET /api/AdvanceDtl/GetMaster?moduleType=advance&paperNum=xxx
    /// </summary>
    [HttpGet("GetMaster")]
    public async Task<IActionResult> GetMaster([FromQuery] string moduleType, [FromQuery] string paperNum)
    {
        if (string.IsNullOrWhiteSpace(paperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        if (!_moduleConfigs.TryGetValue(moduleType ?? "advance", out var cfg))
            return Ok(new { ok = false, error = "無效的模組類型" });

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            var sql = $"SELECT * FROM [{cfg.MainTable}] WITH (NOLOCK) WHERE PaperNum = @paperNum";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@paperNum", paperNum);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return Ok(new { ok = false, error = "找不到主檔資料" });

            var data = new Dictionary<string, object?>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                var name = reader.GetName(i);
                var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                data[name] = value;
            }

            return Ok(new { ok = true, data });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 取得來源資料
    /// GET /api/AdvanceDtl/GetSource?moduleType=advance&paperNum=xxx
    /// </summary>
    [HttpGet("GetSource")]
    public async Task<IActionResult> GetSource([FromQuery] string moduleType, [FromQuery] string paperNum)
    {
        if (string.IsNullOrWhiteSpace(paperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        if (!_moduleConfigs.TryGetValue(moduleType ?? "advance", out var cfg))
            return Ok(new { ok = false, error = "無效的模組類型" });

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            var sql = $"SELECT * FROM [{cfg.SourceTable}] WITH (NOLOCK) WHERE PaperNum = @paperNum ORDER BY Item";

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
    /// 取得銀行選項
    /// GET /api/AdvanceDtl/GetBankOptions?moduleType=advance&paperNum=xxx
    /// </summary>
    [HttpGet("GetBankOptions")]
    public async Task<IActionResult> GetBankOptions([FromQuery] string moduleType, [FromQuery] string paperNum)
    {
        if (!_moduleConfigs.TryGetValue(moduleType ?? "advance", out var cfg))
            return Ok(new { ok = false, error = "無效的模組類型" });

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            // 查詢銀行清單 (從 AJNdBankAccountMain 抓取 IsAccount=1 的資料)
            var sql = @"
                SELECT BankId, ShortName
                FROM AJNdBankAccountMain WITH (NOLOCK)
                WHERE ISNULL(IsAccount, 0) = 1
                ORDER BY BankId";

            await using var cmd = new SqlCommand(sql, conn);

            var banks = new List<Dictionary<string, object?>>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                banks.Add(new Dictionary<string, object?>
                {
                    ["BankId"] = reader["BankId"]?.ToString(),
                    ["ShortName"] = reader["ShortName"]?.ToString()
                });
            }

            return Ok(new { ok = true, banks });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 取得票據資料
    /// GET /api/AdvanceDtl/GetBill?moduleType=advance&paperNum=xxx
    /// </summary>
    [HttpGet("GetBill")]
    public async Task<IActionResult> GetBill([FromQuery] string moduleType, [FromQuery] string paperNum)
    {
        if (string.IsNullOrWhiteSpace(paperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        if (!_moduleConfigs.TryGetValue(moduleType ?? "advance", out var cfg))
            return Ok(new { ok = false, error = "無效的模組類型" });

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            var sql = $@"
                SELECT b.*,
                       pb.ShortName AS BankNamePay,
                       rb.ShortName AS BankNameRecv
                FROM [{cfg.BillTable}] b WITH (NOLOCK)
                LEFT JOIN AJNdBankAccountMain pb WITH (NOLOCK) ON b.PayBankId = pb.BankId
                LEFT JOIN AJNdBankAccountMain rb WITH (NOLOCK) ON b.RecvBankId = rb.BankId
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
                    // 日期格式化
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
    /// 取得其他科目明細
    /// GET /api/AdvanceDtl/GetOtherAcc?moduleType=advance&paperNum=xxx
    /// </summary>
    [HttpGet("GetOtherAcc")]
    public async Task<IActionResult> GetOtherAcc([FromQuery] string moduleType, [FromQuery] string paperNum)
    {
        if (string.IsNullOrWhiteSpace(paperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        if (!_moduleConfigs.TryGetValue(moduleType ?? "advance", out var cfg))
            return Ok(new { ok = false, error = "無效的模組類型" });

        if (string.IsNullOrWhiteSpace(cfg.OtherAccDtlTable))
            return Ok(new { ok = true, rows = new List<object>() });

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            var sql = $@"
                SELECT o.*,
                       a.AccIdName AS AccIdName,
                       s.SubAccName AS SubAccName
                FROM [{cfg.OtherAccDtlTable}] o WITH (NOLOCK)
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
    /// 取得手續費資料
    /// GET /api/AdvanceDtl/GetStrikePost?moduleType=advance&paperNum=xxx
    /// </summary>
    [HttpGet("GetStrikePost")]
    public async Task<IActionResult> GetStrikePost([FromQuery] string moduleType, [FromQuery] string paperNum)
    {
        if (string.IsNullOrWhiteSpace(paperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        if (!_moduleConfigs.TryGetValue(moduleType ?? "advance", out var cfg))
            return Ok(new { ok = false, error = "無效的模組類型" });

        if (string.IsNullOrWhiteSpace(cfg.StrikePostTable))
            return Ok(new { ok = true, rows = new List<object>() });

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            var sql = $@"
                SELECT p.*,
                       b.ShortName AS PostBankName,
                       a.AccIdName AS AccIdName,
                       s.SubAccName AS SubAccName
                FROM [{cfg.StrikePostTable}] p WITH (NOLOCK)
                LEFT JOIN AJNdBankAccountMain b WITH (NOLOCK) ON p.PostBankId = b.BankId
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
    /// 執行檢查 (APRDAdvanceCheck)
    /// POST /api/AdvanceDtl/RunCheck
    /// </summary>
    [HttpPost("RunCheck")]
    public async Task<IActionResult> RunCheck([FromBody] ModuleRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.PaperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        if (!_moduleConfigs.TryGetValue(req.ModuleType ?? "advance", out var cfg))
            return Ok(new { ok = false, error = "無效的模組類型" });

        await using var conn = new SqlConnection(_cs);
        SqlTransaction? tx = null;

        try
        {
            await conn.OpenAsync();
            tx = (SqlTransaction)await conn.BeginTransactionAsync();

            await using var cmd = new SqlCommand(cfg.CheckProc, conn, tx)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 120
            };
            cmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);

            await cmd.ExecuteNonQueryAsync();
            await tx.CommitAsync();

            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            if (tx != null) { try { await tx.RollbackAsync(); } catch { } }
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 執行插入彙總 (APRdAdvanceSubInsSum)
    /// POST /api/AdvanceDtl/InsertSum
    /// </summary>
    [HttpPost("InsertSum")]
    public async Task<IActionResult> InsertSum([FromBody] ModuleRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.PaperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        if (!_moduleConfigs.TryGetValue(req.ModuleType ?? "advance", out var cfg))
            return Ok(new { ok = false, error = "無效的模組類型" });

        await using var conn = new SqlConnection(_cs);
        SqlTransaction? tx = null;

        try
        {
            await conn.OpenAsync();
            tx = (SqlTransaction)await conn.BeginTransactionAsync();

            // 先取得 CJourName
            string cjourNum = "";
            var getCjourSql = $"SELECT CJourName FROM [{cfg.MainTable}] WITH (NOLOCK) WHERE PaperNum = @paperNum";
            await using (var getCjourCmd = new SqlCommand(getCjourSql, conn, tx))
            {
                getCjourCmd.Parameters.AddWithValue("@paperNum", req.PaperNum);
                var result = await getCjourCmd.ExecuteScalarAsync();
                cjourNum = result?.ToString() ?? "";
            }

            // 執行 InsertSum SP
            await using var cmd = new SqlCommand(cfg.InsertSumProc, conn, tx)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 120
            };
            cmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);
            cmd.Parameters.AddWithValue("@CJourNum", cjourNum);

            await cmd.ExecuteNonQueryAsync();
            await tx.CommitAsync();

            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            if (tx != null) { try { await tx.RollbackAsync(); } catch { } }
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 預設子科目 (APRdDefaultSubAccID)
    /// POST /api/AdvanceDtl/DefaultSubAccId
    /// </summary>
    [HttpPost("DefaultSubAccId")]
    public async Task<IActionResult> DefaultSubAccId([FromBody] ModuleRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.PaperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        if (!_moduleConfigs.TryGetValue(req.ModuleType ?? "advance", out var cfg))
            return Ok(new { ok = false, error = "無效的模組類型" });

        await using var conn = new SqlConnection(_cs);
        SqlTransaction? tx = null;

        try
        {
            await conn.OpenAsync();
            tx = (SqlTransaction)await conn.BeginTransactionAsync();

            // 根據模組類型決定 PaperId
            var paperId = req.ModuleType?.ToLower() switch
            {
                "advancerev" => "APRdAdvanceRevSub",
                "recv" => "SPOdRecvSub",
                _ => "APRdAdvanceSub"
            };

            await using var cmd = new SqlCommand("APRdDefaultSubAccID", conn, tx)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 120
            };
            cmd.Parameters.AddWithValue("@PaperId", paperId);
            cmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);

            await cmd.ExecuteNonQueryAsync();
            await tx.CommitAsync();

            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            if (tx != null) { try { await tx.RollbackAsync(); } catch { } }
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 儲存來源資料
    /// POST /api/AdvanceDtl/SaveSource
    /// </summary>
    [HttpPost("SaveSource")]
    public async Task<IActionResult> SaveSource([FromBody] SaveSourceRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.PaperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        if (!_moduleConfigs.TryGetValue(req.ModuleType ?? "advance", out var cfg))
            return Ok(new { ok = false, error = "無效的模組類型" });

        if (req.Rows == null || req.Rows.Count == 0)
            return Ok(new { ok = true, message = "沒有資料需要儲存" });

        await using var conn = new SqlConnection(_cs);
        SqlTransaction? tx = null;

        try
        {
            await conn.OpenAsync();
            tx = (SqlTransaction)await conn.BeginTransactionAsync();

            foreach (var row in req.Rows)
            {
                var item = GetIntValue(row, "Item");
                if (item == 0) continue;

                var sql = $@"
                    UPDATE [{cfg.SourceTable}] SET
                        CashAmount = @CashAmount,
                        BankAmount = @BankAmount,
                        BillAmount = @BillAmount,
                        OtherAmount = @OtherAmount,
                        CashAmountOg = @CashAmountOg,
                        BankAmountOg = @BankAmountOg,
                        BillAmountOg = @BillAmountOg,
                        OtherAmountOg = @OtherAmountOg
                    WHERE PaperNum = @PaperNum AND Item = @Item";

                await using var cmd = new SqlCommand(sql, conn, tx);
                cmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);
                cmd.Parameters.AddWithValue("@Item", item);
                cmd.Parameters.AddWithValue("@CashAmount", GetDecimalValue(row, "CashAmount"));
                cmd.Parameters.AddWithValue("@BankAmount", GetDecimalValue(row, "BankAmount"));
                cmd.Parameters.AddWithValue("@BillAmount", GetDecimalValue(row, "BillAmount"));
                cmd.Parameters.AddWithValue("@OtherAmount", GetDecimalValue(row, "OtherAmount"));
                cmd.Parameters.AddWithValue("@CashAmountOg", GetDecimalValue(row, "CashAmountOg"));
                cmd.Parameters.AddWithValue("@BankAmountOg", GetDecimalValue(row, "BankAmountOg"));
                cmd.Parameters.AddWithValue("@BillAmountOg", GetDecimalValue(row, "BillAmountOg"));
                cmd.Parameters.AddWithValue("@OtherAmountOg", GetDecimalValue(row, "OtherAmountOg"));

                await cmd.ExecuteNonQueryAsync();
            }

            await tx.CommitAsync();
            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            if (tx != null) { try { await tx.RollbackAsync(); } catch { } }
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 儲存銀行資料
    /// POST /api/AdvanceDtl/SaveBank
    /// </summary>
    [HttpPost("SaveBank")]
    public async Task<IActionResult> SaveBank([FromBody] SaveBankRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.PaperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        if (!_moduleConfigs.TryGetValue(req.ModuleType ?? "advance", out var cfg))
            return Ok(new { ok = false, error = "無效的模組類型" });

        await using var conn = new SqlConnection(_cs);
        SqlTransaction? tx = null;

        try
        {
            await conn.OpenAsync();
            tx = (SqlTransaction)await conn.BeginTransactionAsync();

            var sql = $@"
                UPDATE [{cfg.MainTable}] SET
                    BankId = @BankId,
                    AccountId = @AccountId
                WHERE PaperNum = @PaperNum";

            await using var cmd = new SqlCommand(sql, conn, tx);
            cmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);
            cmd.Parameters.AddWithValue("@BankId", req.BankId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@AccountId", req.AccountId ?? (object)DBNull.Value);

            await cmd.ExecuteNonQueryAsync();
            await tx.CommitAsync();

            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            if (tx != null) { try { await tx.RollbackAsync(); } catch { } }
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 取得帳戶選項
    /// GET /api/AdvanceDtl/GetAccountOptions?bankId=xxx
    /// </summary>
    [HttpGet("GetAccountOptions")]
    public async Task<IActionResult> GetAccountOptions([FromQuery] string bankId)
    {
        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            // 從 AJNdBankAccountSub 抓取帳戶資料
            var sql = @"
                SELECT AccountId
                FROM AJNdBankAccountSub WITH (NOLOCK)
                WHERE BankId = @BankId
                ORDER BY AccountId";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@BankId", bankId ?? "");

            var accounts = new List<Dictionary<string, object?>>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var accountId = reader["AccountId"]?.ToString() ?? "";
                accounts.Add(new Dictionary<string, object?>
                {
                    ["AccountId"] = accountId,
                    ["AccountName"] = accountId  // 沒有 AccountName 欄位，用 AccountId 顯示
                });
            }

            return Ok(new { ok = true, accounts });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    // Helper: 從 Dictionary 取得 int 值
    private static int GetIntValue(Dictionary<string, object?> row, string key)
    {
        if (!row.TryGetValue(key, out var val) || val == null)
            return 0;

        if (val is JsonElement je)
        {
            if (je.ValueKind == JsonValueKind.Number)
                return je.GetInt32();
            if (je.ValueKind == JsonValueKind.String && int.TryParse(je.GetString(), out var i))
                return i;
            return 0;
        }

        if (int.TryParse(val.ToString(), out var result))
            return result;

        return 0;
    }

    // Helper: 從 Dictionary 取得 decimal 值
    private static decimal GetDecimalValue(Dictionary<string, object?> row, string key)
    {
        if (!row.TryGetValue(key, out var val) || val == null)
            return 0;

        if (val is JsonElement je)
        {
            if (je.ValueKind == JsonValueKind.Number)
                return je.GetDecimal();
            if (je.ValueKind == JsonValueKind.String && decimal.TryParse(je.GetString(), out var d))
                return d;
            return 0;
        }

        if (decimal.TryParse(val.ToString(), out var result))
            return result;

        return 0;
    }

    /// <summary>
    /// 新增一筆資料列
    /// POST /api/AdvanceDtl/InsertRow
    /// </summary>
    [HttpPost("InsertRow")]
    public async Task<IActionResult> InsertRow([FromBody] InsertRowRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.PaperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        if (!_moduleConfigs.TryGetValue(req.ModuleType ?? "advance", out var cfg))
            return Ok(new { ok = false, error = "無效的模組類型" });

        await using var conn = new SqlConnection(_cs);
        SqlTransaction? tx = null;

        try
        {
            await conn.OpenAsync();
            tx = (SqlTransaction)await conn.BeginTransactionAsync();

            // 根據 tableType 決定要操作的表格
            string tableName = req.TableType?.ToLower() switch
            {
                "source" => cfg.SourceTable,
                "bill" => cfg.BillTable,
                "otheracc" => cfg.OtherAccDtlTable,
                "strikepost" => cfg.StrikePostTable,
                _ => cfg.SourceTable
            };

            if (string.IsNullOrWhiteSpace(tableName))
                return Ok(new { ok = false, error = "無效的表格類型" });

            // 取得最大 Item 編號
            var getMaxItemSql = $"SELECT ISNULL(MAX(Item), 0) FROM [{tableName}] WITH (NOLOCK) WHERE PaperNum = @paperNum";
            await using var getMaxCmd = new SqlCommand(getMaxItemSql, conn, tx);
            getMaxCmd.Parameters.AddWithValue("@paperNum", req.PaperNum);
            var maxItem = Convert.ToInt32(await getMaxCmd.ExecuteScalarAsync());
            var newItem = maxItem + 1;

            // 插入新資料列
            string insertSql;
            if (req.TableType?.ToLower() == "source")
            {
                insertSql = $@"
                    INSERT INTO [{tableName}] (PaperNum, Item, CashAmount, BankAmount, BillAmount, OtherAmount, CashAmountOg, BankAmountOg, BillAmountOg, OtherAmountOg)
                    VALUES (@paperNum, @item, 0, 0, 0, 0, 0, 0, 0, 0)";
            }
            else if (req.TableType?.ToLower() == "bill")
            {
                insertSql = $@"
                    INSERT INTO [{tableName}] (PaperNum, Item, BillId, Amount, PayBankId, PayAccountId, RecvBankId, RecvAccountId, BillAccId, BillTypeId, DueDate, Inhibit, ParaLine, Title)
                    VALUES (@paperNum, @item, '', 0, '', '', '', '', '', '', NULL, 0, 0, 0)";
            }
            else if (req.TableType?.ToLower() == "otheracc")
            {
                insertSql = $@"
                    INSERT INTO [{tableName}] (PaperNum, Item, AccId, SubAccId, Amount, AmountOg)
                    VALUES (@paperNum, @item, '', '', 0, 0)";
            }
            else if (req.TableType?.ToLower() == "strikepost")
            {
                insertSql = $@"
                    INSERT INTO [{tableName}] (PaperNum, Item, PostAmount, PostBankId, PostAccountId, PostAccId, PostSubAccId)
                    VALUES (@paperNum, @item, 0, '', '', '', '')";
            }
            else
            {
                return Ok(new { ok = false, error = "無效的表格類型" });
            }

            await using var insertCmd = new SqlCommand(insertSql, conn, tx);
            insertCmd.Parameters.AddWithValue("@paperNum", req.PaperNum);
            insertCmd.Parameters.AddWithValue("@item", newItem);

            await insertCmd.ExecuteNonQueryAsync();
            await tx.CommitAsync();

            return Ok(new { ok = true, newItem });
        }
        catch (Exception ex)
        {
            if (tx != null) { try { await tx.RollbackAsync(); } catch { } }
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 刪除一筆資料列
    /// POST /api/AdvanceDtl/DeleteRow
    /// </summary>
    [HttpPost("DeleteRow")]
    public async Task<IActionResult> DeleteRow([FromBody] DeleteRowRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.PaperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        if (!_moduleConfigs.TryGetValue(req.ModuleType ?? "advance", out var cfg))
            return Ok(new { ok = false, error = "無效的模組類型" });

        await using var conn = new SqlConnection(_cs);
        SqlTransaction? tx = null;

        try
        {
            await conn.OpenAsync();
            tx = (SqlTransaction)await conn.BeginTransactionAsync();

            // 根據 tableType 決定要操作的表格
            string tableName = req.TableType?.ToLower() switch
            {
                "source" => cfg.SourceTable,
                "bill" => cfg.BillTable,
                "otheracc" => cfg.OtherAccDtlTable,
                "strikepost" => cfg.StrikePostTable,
                _ => cfg.SourceTable
            };

            if (string.IsNullOrWhiteSpace(tableName))
                return Ok(new { ok = false, error = "無效的表格類型" });

            // 刪除指定資料列
            var deleteSql = $"DELETE FROM [{tableName}] WHERE PaperNum = @paperNum AND Item = @item";
            await using var deleteCmd = new SqlCommand(deleteSql, conn, tx);
            deleteCmd.Parameters.AddWithValue("@paperNum", req.PaperNum);
            deleteCmd.Parameters.AddWithValue("@item", req.Item);

            var affected = await deleteCmd.ExecuteNonQueryAsync();
            if (affected == 0)
            {
                await tx.RollbackAsync();
                return Ok(new { ok = false, error = "找不到要刪除的資料" });
            }

            // 重新排序 Item 編號
            var reorderSql = $@"
                ;WITH CTE AS (
                    SELECT Item, ROW_NUMBER() OVER (ORDER BY Item) AS NewItem
                    FROM [{tableName}]
                    WHERE PaperNum = @paperNum
                )
                UPDATE [{tableName}]
                SET Item = CTE.NewItem
                FROM [{tableName}] t
                INNER JOIN CTE ON t.PaperNum = @paperNum AND t.Item = CTE.Item";

            await using var reorderCmd = new SqlCommand(reorderSql, conn, tx);
            reorderCmd.Parameters.AddWithValue("@paperNum", req.PaperNum);
            await reorderCmd.ExecuteNonQueryAsync();

            await tx.CommitAsync();

            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            if (tx != null) { try { await tx.RollbackAsync(); } catch { } }
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 儲存多筆資料列
    /// POST /api/AdvanceDtl/SaveRows
    /// </summary>
    [HttpPost("SaveRows")]
    public async Task<IActionResult> SaveRows([FromBody] SaveRowsRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.PaperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        if (!_moduleConfigs.TryGetValue(req.ModuleType ?? "advance", out var cfg))
            return Ok(new { ok = false, error = "無效的模組類型" });

        if (req.Rows == null || req.Rows.Count == 0)
            return Ok(new { ok = true, message = "沒有資料需要儲存" });

        await using var conn = new SqlConnection(_cs);
        SqlTransaction? tx = null;

        try
        {
            await conn.OpenAsync();
            tx = (SqlTransaction)await conn.BeginTransactionAsync();

            // 根據 tableType 決定要操作的表格和更新邏輯
            string tableName = req.TableType?.ToLower() switch
            {
                "source" => cfg.SourceTable,
                "bill" => cfg.BillTable,
                "otheracc" => cfg.OtherAccDtlTable,
                "strikepost" => cfg.StrikePostTable,
                _ => cfg.SourceTable
            };

            if (string.IsNullOrWhiteSpace(tableName))
                return Ok(new { ok = false, error = "無效的表格類型" });

            foreach (var row in req.Rows)
            {
                var item = GetIntValue(row, "Item");
                if (item == 0) continue;

                string updateSql;

                if (req.TableType?.ToLower() == "source")
                {
                    // 根據 mode 決定更新哪些欄位
                    if (req.Mode == "og")
                    {
                        updateSql = $@"
                            UPDATE [{tableName}] SET
                                CashAmountOg = @CashAmountOg,
                                BankAmountOg = @BankAmountOg,
                                BillAmountOg = @BillAmountOg,
                                OtherAmountOg = @OtherAmountOg
                            WHERE PaperNum = @PaperNum AND Item = @Item";

                        await using var cmd = new SqlCommand(updateSql, conn, tx);
                        cmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);
                        cmd.Parameters.AddWithValue("@Item", item);
                        cmd.Parameters.AddWithValue("@CashAmountOg", GetDecimalValue(row, "CashAmountOg"));
                        cmd.Parameters.AddWithValue("@BankAmountOg", GetDecimalValue(row, "BankAmountOg"));
                        cmd.Parameters.AddWithValue("@BillAmountOg", GetDecimalValue(row, "BillAmountOg"));
                        cmd.Parameters.AddWithValue("@OtherAmountOg", GetDecimalValue(row, "OtherAmountOg"));
                        await cmd.ExecuteNonQueryAsync();
                    }
                    else
                    {
                        updateSql = $@"
                            UPDATE [{tableName}] SET
                                CashAmount = @CashAmount,
                                BankAmount = @BankAmount,
                                BillAmount = @BillAmount,
                                OtherAmount = @OtherAmount
                            WHERE PaperNum = @PaperNum AND Item = @Item";

                        await using var cmd = new SqlCommand(updateSql, conn, tx);
                        cmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);
                        cmd.Parameters.AddWithValue("@Item", item);
                        cmd.Parameters.AddWithValue("@CashAmount", GetDecimalValue(row, "CashAmount"));
                        cmd.Parameters.AddWithValue("@BankAmount", GetDecimalValue(row, "BankAmount"));
                        cmd.Parameters.AddWithValue("@BillAmount", GetDecimalValue(row, "BillAmount"));
                        cmd.Parameters.AddWithValue("@OtherAmount", GetDecimalValue(row, "OtherAmount"));
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                else if (req.TableType?.ToLower() == "bill")
                {
                    updateSql = $@"
                        UPDATE [{tableName}] SET
                            BillId = @BillId,
                            Amount = @Amount,
                            PayBankId = @PayBankId,
                            PayAccountId = @PayAccountId,
                            RecvBankId = @RecvBankId,
                            RecvAccountId = @RecvAccountId,
                            BillAccId = @BillAccId,
                            BillTypeId = @BillTypeId,
                            DueDate = @DueDate,
                            Inhibit = @Inhibit,
                            ParaLine = @ParaLine,
                            Title = @Title
                        WHERE PaperNum = @PaperNum AND Item = @Item";

                    await using var cmd = new SqlCommand(updateSql, conn, tx);
                    cmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);
                    cmd.Parameters.AddWithValue("@Item", item);
                    cmd.Parameters.AddWithValue("@BillId", GetStringValue(row, "BillId"));
                    cmd.Parameters.AddWithValue("@Amount", GetDecimalValue(row, "Amount"));
                    cmd.Parameters.AddWithValue("@PayBankId", GetStringValue(row, "PayBankId"));
                    cmd.Parameters.AddWithValue("@PayAccountId", GetStringValue(row, "PayAccountId"));
                    cmd.Parameters.AddWithValue("@RecvBankId", GetStringValue(row, "RecvBankId"));
                    cmd.Parameters.AddWithValue("@RecvAccountId", GetStringValue(row, "RecvAccountId"));
                    cmd.Parameters.AddWithValue("@BillAccId", GetStringValue(row, "BillAccId"));
                    cmd.Parameters.AddWithValue("@BillTypeId", GetStringValue(row, "BillTypeId"));
                    cmd.Parameters.AddWithValue("@DueDate", GetDateValue(row, "DueDate"));
                    cmd.Parameters.AddWithValue("@Inhibit", GetBoolValue(row, "Inhibit"));
                    cmd.Parameters.AddWithValue("@ParaLine", GetBoolValue(row, "ParaLine"));
                    cmd.Parameters.AddWithValue("@Title", GetBoolValue(row, "Title"));
                    await cmd.ExecuteNonQueryAsync();
                }
                else if (req.TableType?.ToLower() == "otheracc")
                {
                    updateSql = $@"
                        UPDATE [{tableName}] SET
                            AccId = @AccId,
                            SubAccId = @SubAccId,
                            Amount = @Amount,
                            AmountOg = @AmountOg
                        WHERE PaperNum = @PaperNum AND Item = @Item";

                    await using var cmd = new SqlCommand(updateSql, conn, tx);
                    cmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);
                    cmd.Parameters.AddWithValue("@Item", item);
                    cmd.Parameters.AddWithValue("@AccId", GetStringValue(row, "AccId"));
                    cmd.Parameters.AddWithValue("@SubAccId", GetStringValue(row, "SubAccId"));
                    cmd.Parameters.AddWithValue("@Amount", GetDecimalValue(row, "Amount"));
                    cmd.Parameters.AddWithValue("@AmountOg", GetDecimalValue(row, "AmountOg"));
                    await cmd.ExecuteNonQueryAsync();
                }
                else if (req.TableType?.ToLower() == "strikepost")
                {
                    updateSql = $@"
                        UPDATE [{tableName}] SET
                            PostAmount = @PostAmount,
                            PostBankId = @PostBankId,
                            PostAccountId = @PostAccountId,
                            PostAccId = @PostAccId,
                            PostSubAccId = @PostSubAccId
                        WHERE PaperNum = @PaperNum AND Item = @Item";

                    await using var cmd = new SqlCommand(updateSql, conn, tx);
                    cmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);
                    cmd.Parameters.AddWithValue("@Item", item);
                    cmd.Parameters.AddWithValue("@PostAmount", GetDecimalValue(row, "PostAmount"));
                    cmd.Parameters.AddWithValue("@PostBankId", GetStringValue(row, "PostBankId"));
                    cmd.Parameters.AddWithValue("@PostAccountId", GetStringValue(row, "PostAccountId"));
                    cmd.Parameters.AddWithValue("@PostAccId", GetStringValue(row, "PostAccId"));
                    cmd.Parameters.AddWithValue("@PostSubAccId", GetStringValue(row, "PostSubAccId"));
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            await tx.CommitAsync();
            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            if (tx != null) { try { await tx.RollbackAsync(); } catch { } }
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    // Helper: 從 Dictionary 取得 string 值
    private static string GetStringValue(Dictionary<string, object?> row, string key)
    {
        if (!row.TryGetValue(key, out var val) || val == null)
            return "";

        if (val is JsonElement je)
        {
            if (je.ValueKind == JsonValueKind.String)
                return je.GetString() ?? "";
            return je.ToString();
        }

        return val.ToString() ?? "";
    }

    // Helper: 從 Dictionary 取得 bool 值
    private static bool GetBoolValue(Dictionary<string, object?> row, string key)
    {
        if (!row.TryGetValue(key, out var val) || val == null)
            return false;

        if (val is JsonElement je)
        {
            if (je.ValueKind == JsonValueKind.True) return true;
            if (je.ValueKind == JsonValueKind.False) return false;
            if (je.ValueKind == JsonValueKind.String)
                return je.GetString()?.ToLower() == "true";
            if (je.ValueKind == JsonValueKind.Number)
                return je.GetInt32() != 0;
            return false;
        }

        if (val is bool b) return b;
        if (bool.TryParse(val.ToString(), out var result)) return result;
        return false;
    }

    // Helper: 從 Dictionary 取得 DateTime 值
    private static object GetDateValue(Dictionary<string, object?> row, string key)
    {
        if (!row.TryGetValue(key, out var val) || val == null)
            return DBNull.Value;

        string? dateStr = null;

        if (val is JsonElement je)
        {
            if (je.ValueKind == JsonValueKind.String)
                dateStr = je.GetString();
            else if (je.ValueKind == JsonValueKind.Null)
                return DBNull.Value;
        }
        else
        {
            dateStr = val.ToString();
        }

        if (string.IsNullOrWhiteSpace(dateStr))
            return DBNull.Value;

        if (DateTime.TryParse(dateStr, out var dt))
            return dt;

        return DBNull.Value;
    }

    // Request Models
    public record ModuleRequest(string? ModuleType, string? PaperNum);
    public record SaveSourceRequest(string? ModuleType, string? PaperNum, List<Dictionary<string, object?>>? Rows);
    public record SaveBankRequest(string? ModuleType, string? PaperNum, string? BankId, string? AccountId);
    public record InsertRowRequest(string? ModuleType, string? PaperNum, string? TableType, string? Mode);
    public record DeleteRowRequest(string? ModuleType, string? PaperNum, string? TableType, int Item);
    public record SaveRowsRequest(string? ModuleType, string? PaperNum, string? TableType, string? Mode, List<Dictionary<string, object?>>? Rows);
}