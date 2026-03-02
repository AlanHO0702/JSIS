using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using System.Data;
using System.Text;

/// <summary>
/// ABE00013 - 銀行匯款單 自訂按鈕 API 控制器
/// 對應 Delphi: AccountPayDLL.pas
/// 功能:
///   - btnC2 (SendRPT): 轉出報表 — 執行 SP 回傳 Grid 資料
///   - btnC3 (SendTXT): 轉出匯款檔 — 執行 SP 回傳 TXT/DAT 下載
///   - btnC8 (SendTXT2): 郵局匯款檔 — 執行 ABEdBankTranGetData_TCI 回傳 TXT 下載
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ABE00013Controller : ControllerBase
{
    private readonly string _cs;

    public ABE00013Controller(IConfiguration cfg, PcbErpContext db)
    {
        _cs = cfg.GetConnectionString("DefaultConnection")
              ?? db?.Database.GetDbConnection().ConnectionString
              ?? throw new InvalidOperationException("找不到連線字串");
    }

    /// <summary>
    /// 檢查單據完成狀態 (Finished 必須為 1 或 4 才可轉出)
    /// GET /api/ABE00013/CheckFinished?paperNum=xxx
    /// </summary>
    [HttpGet("CheckFinished")]
    public async Task<IActionResult> CheckFinished([FromQuery] string paperNum)
    {
        if (string.IsNullOrWhiteSpace(paperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync();

        const string sql = "SELECT Finished FROM APRdAccountPayMain WITH (NOLOCK) WHERE PaperNum = @PN";
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@PN", paperNum);

        var result = await cmd.ExecuteScalarAsync();
        if (result == null || result == DBNull.Value)
            return Ok(new { ok = false, error = "找不到此單號" });

        var finished = Convert.ToInt32(result);
        if (finished != 1 && finished != 4)
            return Ok(new { ok = false, error = "完成狀態不符，無法轉出!!" });

        return Ok(new { ok = true, finished });
    }

    /// <summary>
    /// 取得 AccountPay 的 SP 名稱 (對應 Delphi: CURdSysParams → AccountPaySP)
    /// GET /api/ABE00013/GetSpName
    /// </summary>
    [HttpGet("GetSpName")]
    public async Task<IActionResult> GetSpName()
    {
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync();

        const string sql = @"
            SELECT Value FROM CURdSysParams WITH (NOLOCK)
            WHERE SystemId = 'APR' AND ParamId = 'AccountPaySP'";
        await using var cmd = new SqlCommand(sql, conn);

        var result = await cmd.ExecuteScalarAsync();
        var spName = result?.ToString()?.Trim();
        if (string.IsNullOrEmpty(spName))
            return Ok(new { ok = false, error = "尚未設定電匯SP名稱，請至系統參數(APR/AccountPaySP)進行設定!!" });

        return Ok(new { ok = true, spName });
    }

    /// <summary>
    /// btnC2 (SendRPT): 執行報表 SP，回傳 Grid 資料 (欄位 + 列)
    /// POST /api/ABE00013/ExecReport
    /// </summary>
    [HttpPost("ExecReport")]
    public async Task<IActionResult> ExecReport([FromBody] ExecRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.PaperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        try
        {
            // 取得 SP 名稱
            var spName = await GetAccountPaySpName();
            if (spName == null)
                return Ok(new { ok = false, error = "尚未設定電匯SP名稱，請至系統參數(APR/AccountPaySP)進行設定!!" });

            await using var conn = new SqlConnection(_cs);
            await conn.OpenAsync();

            // 對應 Delphi: exec {SPName} '{PaperNum}', 1
            // SP 參數: @PaperNum cvPaperNum(16), @IsDetail int
            await using var cmd = new SqlCommand(spName, conn)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 180
            };
            cmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);
            cmd.Parameters.AddWithValue("@IsDetail", 1);

            var dt = new DataTable();
            using (var da = new SqlDataAdapter(cmd))
            {
                da.Fill(dt);
            }

            var columns = dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();
            var rows = dt.Rows.Cast<DataRow>()
                .Select(r => dt.Columns.Cast<DataColumn>()
                    .ToDictionary(c => c.ColumnName, c =>
                        r[c] == DBNull.Value ? null : r[c]))
                .ToList();

            return Ok(new { ok = true, columns, rows });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// btnC3 (SendTXT): 執行 SP 取得匯款檔資料，回傳 TXT 下載
    /// POST /api/ABE00013/DownloadTxt
    /// </summary>
    [HttpPost("DownloadTxt")]
    public async Task<IActionResult> DownloadTxt([FromBody] ExecRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.PaperNum))
            return BadRequest(new { ok = false, error = "沒有單號" });

        try
        {
            // 取得 SP 名稱
            var spName = await GetAccountPaySpName();
            if (spName == null)
                return BadRequest(new { ok = false, error = "尚未設定電匯SP名稱，請至系統參數(APR/AccountPaySP)進行設定!!" });

            // 判斷是否為 TCI 客戶 (決定副檔名和最後一行是否加換行)
            var isTci = await IsTciCustomer();
            var ext = isTci ? "dat" : "txt";

            await using var conn = new SqlConnection(_cs);
            await conn.OpenAsync();

            // 對應 Delphi: exec {SPName} '{PaperNum}', 0
            // SP 參數: @PaperNum cvPaperNum(16), @IsDetail int
            await using var cmd = new SqlCommand(spName, conn)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 180
            };
            cmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);
            cmd.Parameters.AddWithValue("@IsDetail", 0);

            var lines = new List<string>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                lines.Add(reader["Data"]?.ToString() ?? "");
            }

            // 對應 Delphi 邏輯: TCI 最後一行用 Writeln，非 TCI 最後一行用 Write (不加換行)
            var sb = new StringBuilder();
            for (int i = 0; i < lines.Count; i++)
            {
                if (i < lines.Count - 1)
                    sb.AppendLine(lines[i]);
                else
                {
                    if (isTci)
                        sb.AppendLine(lines[i]);
                    else
                        sb.Append(lines[i]);
                }
            }

            var bytes = Encoding.Default.GetBytes(sb.ToString());
            return File(bytes, "application/octet-stream", $"AccountPay_{req.PaperNum}.{ext}");
        }
        catch (Exception ex)
        {
            return BadRequest(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// btnC8 (SendTXT2): 郵局匯款檔 — 執行 ABEdBankTranGetData_TCI 回傳 TXT 下載
    /// POST /api/ABE00013/DownloadPostTxt
    /// </summary>
    [HttpPost("DownloadPostTxt")]
    public async Task<IActionResult> DownloadPostTxt([FromBody] ExecRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.PaperNum))
            return BadRequest(new { ok = false, error = "沒有單號" });

        try
        {
            await using var conn = new SqlConnection(_cs);
            await conn.OpenAsync();

            // 對應 Delphi: exec ABEdBankTranGetData_TCI '{PaperNum}'
            await using var cmd = new SqlCommand("ABEdBankTranGetData_TCI", conn)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 180
            };
            cmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);

            var lines = new List<string>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                lines.Add(reader["Data"]?.ToString() ?? "");
            }

            // 對應 Delphi: 每行都用 Writeln
            var content = string.Join("\r\n", lines);
            if (lines.Count > 0) content += "\r\n";

            var bytes = Encoding.Default.GetBytes(content);
            return File(bytes, "application/octet-stream", $"PostTransfer_{req.PaperNum}.txt");
        }
        catch (Exception ex)
        {
            return BadRequest(new { ok = false, error = ex.Message });
        }
    }

    // ── 私有方法 ──

    private async Task<string?> GetAccountPaySpName()
    {
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync();

        const string sql = @"
            SELECT Value FROM CURdSysParams WITH (NOLOCK)
            WHERE SystemId = 'APR' AND ParamId = 'AccountPaySP'";
        await using var cmd = new SqlCommand(sql, conn);

        var result = await cmd.ExecuteScalarAsync();
        var spName = result?.ToString()?.Trim();
        return string.IsNullOrEmpty(spName) ? null : spName;
    }

    private async Task<bool> IsTciCustomer()
    {
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync();

        const string sql = @"
            SELECT Value FROM CURdSysParams WITH (NOLOCK)
            WHERE SystemId = 'EMO' AND ParamId = 'CusId'";
        await using var cmd = new SqlCommand(sql, conn);

        var result = await cmd.ExecuteScalarAsync();
        return string.Equals(result?.ToString()?.Trim(), "TCI", StringComparison.OrdinalIgnoreCase);
    }

    public class ExecRequest
    {
        public string PaperNum { get; set; } = "";
    }
}
