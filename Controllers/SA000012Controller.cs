using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

/// <summary>
/// 客戶收款發票沖銷明細維護 API 控制器
/// 對應 Delphi: StrikeDtlDLL.pas (TfrmSPOdStrikeDtlDLL)
/// 主表: SPOdStrikeMain, 來源表: SPOdStrikeSourceInv
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class SA000012Controller : ControllerBase
{
    private readonly string _cs;

    public SA000012Controller(IConfiguration cfg)
    {
        _cs = cfg.GetConnectionString("DefaultConnection")
              ?? throw new InvalidOperationException("找不到連線字串");
    }

    // ────────────────────────────────────────────────────────────────
    // GET /api/SA000012/Prepare?paperNum=xxx
    // 對應 Delphi: exec SPOdStrikePrepare :PaperNum
    // ────────────────────────────────────────────────────────────────
    [HttpGet("Prepare")]
    public async Task<IActionResult> Prepare([FromQuery] string paperNum)
    {
        if (string.IsNullOrWhiteSpace(paperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();
            // 對應 Delphi: Open (SP 可能回傳結果集，用 reader 確保相容)
            await using var cmd = new SqlCommand($"exec SPOdStrikePrepare '{paperNum.Replace("'", "''")}'", conn)
            {
                CommandTimeout = 120
            };
            await using var reader = await cmd.ExecuteReaderAsync();
            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    // ────────────────────────────────────────────────────────────────
    // GET /api/SA000012/GetMaster?paperNum=xxx
    // 對應 Delphi: Select * from SPOdStrikeMain where PaperNum=:PaperNum
    // ────────────────────────────────────────────────────────────────
    [HttpGet("GetMaster")]
    public async Task<IActionResult> GetMaster([FromQuery] string paperNum)
    {
        if (string.IsNullOrWhiteSpace(paperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();
            const string sql = @"
                SELECT PaperNum, PaperDate, Status, Finished,
                       CustomerId, MoneyCode, RateToNT,
                       AdvanceAmountOg, OverAmountOg, Notes
                FROM SPOdStrikeMain WITH (NOLOCK)
                WHERE PaperNum = @p";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@p", paperNum);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return Ok(new { ok = false, error = "找不到主檔資料" });

            var result = new Dictionary<string, object?>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                var name  = reader.GetName(i);
                var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                if (value is DateTime dt)
                    result[name] = dt.ToString("yyyy-MM-dd");
                else
                    result[name] = value;
            }

            return Ok(new { ok = true, data = result });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    // ────────────────────────────────────────────────────────────────
    // GET /api/SA000012/GetShowSum?paperNum=xxx
    // 對應 Delphi: exec SPOdStrikeShowSum :PaperNum  → 回傳 OgSumStr
    // ────────────────────────────────────────────────────────────────
    [HttpGet("GetShowSum")]
    public async Task<IActionResult> GetShowSum([FromQuery] string paperNum)
    {
        if (string.IsNullOrWhiteSpace(paperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();
            await using var cmd = new SqlCommand("SPOdStrikeShowSum", conn)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 120
            };
            cmd.Parameters.AddWithValue("@PaperNum", paperNum);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return Ok(new { ok = true, ogSumStr = "" });

            var ogSumStr = reader.IsDBNull(reader.GetOrdinal("OgSumStr"))
                ? ""
                : reader["OgSumStr"]?.ToString() ?? "";

            return Ok(new { ok = true, ogSumStr });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    // ────────────────────────────────────────────────────────────────
    // GET /api/SA000012/GetSourceData?paperNum=xxx
    // 對應 Delphi: select * from SPOdStrikeSourceInv where PaperNum=:PaperNum order by SortSerial
    // ────────────────────────────────────────────────────────────────
    [HttpGet("GetSourceData")]
    public async Task<IActionResult> GetSourceData([FromQuery] string paperNum)
    {
        if (string.IsNullOrWhiteSpace(paperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();
            const string sql = @"
                SELECT *
                FROM SPOdStrikeSourceInv WITH (NOLOCK)
                WHERE PaperNum = @p
                ORDER BY SortSerial";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@p", paperNum);

            var rows = new List<Dictionary<string, object?>>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object?>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var name  = reader.GetName(i);
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

    // ────────────────────────────────────────────────────────────────
    // GET /api/SA000012/GetAllAmount?paperNum=xxx
    // 對應 Delphi: exec SPOdStrikeCalcAllAmount :PaperNum
    // ────────────────────────────────────────────────────────────────
    [HttpGet("GetAllAmount")]
    public async Task<IActionResult> GetAllAmount([FromQuery] string paperNum)
    {
        if (string.IsNullOrWhiteSpace(paperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();
            await using var cmd = new SqlCommand("SPOdStrikeCalcAllAmount", conn)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 120
            };
            cmd.Parameters.AddWithValue("@PaperNum", paperNum);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return Ok(new { ok = true, data = new Dictionary<string, object>() });

            var amounts = new Dictionary<string, object?>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                var name = reader.GetName(i);
                amounts[name] = reader.IsDBNull(i) ? (object?)0.0 : reader.GetValue(i);
            }

            return Ok(new { ok = true, data = amounts });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    // ────────────────────────────────────────────────────────────────
    // POST /api/SA000012/AutoCalc
    // 對應 Delphi: exec SPOdStrikeNewAutoCalc :PaperNum, :SourNum, :Item, :FieldName, :SourceType
    // ────────────────────────────────────────────────────────────────
    public class AutoCalcRequest
    {
        public string PaperNum   { get; set; } = "";
        public string SourNum    { get; set; } = "";
        public int    Item       { get; set; }
        public string FieldName  { get; set; } = "";
        public int    SourceType { get; set; }   // iPage (tab index, 0 = 帳款)
    }

    [HttpPost("AutoCalc")]
    public async Task<IActionResult> AutoCalc([FromBody] AutoCalcRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.PaperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();
            // 對應 Delphi: 'exec SPOdStrikeNewAutoCalc ''' + sPaperNum + ''',''' + SourNum + ''',' + IntToStr(Item) + ',''' + SourceField + ''',' + IntToStr(iPage)
            var sql = $"exec SPOdStrikeNewAutoCalc '{req.PaperNum.Replace("'", "''")}','{req.SourNum.Replace("'", "''")}',{req.Item},'{req.FieldName.Replace("'", "''")}',{req.SourceType}";
            await using var cmd = new SqlCommand(sql, conn) { CommandTimeout = 120 };
            await using var reader = await cmd.ExecuteReaderAsync();

            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    // ────────────────────────────────────────────────────────────────
    // POST /api/SA000012/SerialChange
    // 對應 Delphi: exec SPOdStrikeSerialChg :TableName, :PaperNum, :SortSerial, :Direction
    //   Direction: 1=上移, 0=下移
    // ────────────────────────────────────────────────────────────────
    public class SerialChangeRequest
    {
        public string PaperNum   { get; set; } = "";
        public int    SortSerial { get; set; }
        public int    Direction  { get; set; }   // 1=上移, 0=下移
    }

    [HttpPost("SerialChange")]
    public async Task<IActionResult> SerialChange([FromBody] SerialChangeRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.PaperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();
            // 對應 Delphi: 'exec SPOdStrikeSerialChg ''SPOdStrikeSourceInv'', ''' + sPaperNum + ''', ' + IntToStr(iItem) + ', ' + IntToStr(iDirection)
            var sql = $"exec SPOdStrikeSerialChg 'SPOdStrikeSourceInv','{req.PaperNum.Replace("'", "''")}',{req.SortSerial},{req.Direction}";
            await using var cmd = new SqlCommand(sql, conn) { CommandTimeout = 120 };
            await cmd.ExecuteNonQueryAsync();

            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    // ────────────────────────────────────────────────────────────────
    // POST /api/SA000012/Confirm
    // 對應 Delphi btnOKClick:
    //   1. exec SPOdStrikeCheck :PaperNum
    //   2. 若 Finished=0 or 3 → exec SPOdStrikeSubInsSum :PaperNum
    // ────────────────────────────────────────────────────────────────
    public class ConfirmRequest
    {
        public string PaperNum { get; set; } = "";
    }

    [HttpPost("Confirm")]
    public async Task<IActionResult> Confirm([FromBody] ConfirmRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.PaperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            // 1. 執行 SPOdStrikeCheck
            var checkSql = $"exec SPOdStrikeCheck '{req.PaperNum.Replace("'", "''")}'";
            await using var checkCmd = new SqlCommand(checkSql, conn) { CommandTimeout = 120 };
            await using (var checkReader = await checkCmd.ExecuteReaderAsync()) { }

            // 2. 取得 Finished 欄位
            int finished = 0;
            await using (var masterCmd = new SqlCommand(
                "SELECT Finished FROM SPOdStrikeMain WITH (NOLOCK) WHERE PaperNum = @p", conn))
            {
                masterCmd.Parameters.AddWithValue("@p", req.PaperNum);
                var val = await masterCmd.ExecuteScalarAsync();
                if (val != null && val != DBNull.Value)
                    finished = Convert.ToInt32(val);
            }

            // 3. 若 Finished=0 or 3, 執行 SPOdStrikeSubInsSum
            if (finished == 0 || finished == 3)
            {
                var sumSql = $"exec SPOdStrikeSubInsSum '{req.PaperNum.Replace("'", "''")}'";
                await using var sumCmd    = new SqlCommand(sumSql, conn) { CommandTimeout = 120 };
                await using var sumReader = await sumCmd.ExecuteReaderAsync();
            }

            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    // ────────────────────────────────────────────────────────────────
    // POST /api/SA000012/UpdateField
    // 直接更新 SPOdStrikeSourceInv 的單一欄位值（儲存使用者輸入，再呼叫 AutoCalc）
    // ────────────────────────────────────────────────────────────────
    public class UpdateFieldRequest
    {
        public string PaperNum  { get; set; } = "";
        public string SourNum   { get; set; } = "";
        public int    Item      { get; set; }
        public string FieldName { get; set; } = "";
        public string Value     { get; set; } = "";
    }

    [HttpPost("UpdateField")]
    public async Task<IActionResult> UpdateField([FromBody] UpdateFieldRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.PaperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        // 防 SQL injection：欄位名只允許字母、數字、底線
        if (!System.Text.RegularExpressions.Regex.IsMatch(req.FieldName ?? "", @"^[A-Za-z_][A-Za-z0-9_]*$"))
            return Ok(new { ok = false, error = "欄位名稱格式不合法" });

        // 嘗試將 Value 解析為數值（金額欄位），不是數值就當字串存
        object paramValue;
        if (decimal.TryParse(req.Value, System.Globalization.NumberStyles.Any,
                             System.Globalization.CultureInfo.InvariantCulture, out var dec))
            paramValue = dec;
        else
            paramValue = req.Value ?? "";

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();
            var sql = $@"
                UPDATE SPOdStrikeSourceInv
                SET [{req.FieldName}] = @val
                WHERE PaperNum = @pn AND SourNum = @sourNum AND Item = @item";

            await using var cmd = new SqlCommand(sql, conn) { CommandTimeout = 60 };
            cmd.Parameters.AddWithValue("@val",     paramValue);
            cmd.Parameters.AddWithValue("@pn",      req.PaperNum);
            cmd.Parameters.AddWithValue("@sourNum",  req.SourNum);
            cmd.Parameters.AddWithValue("@item",     req.Item);
            await cmd.ExecuteNonQueryAsync();

            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    // ────────────────────────────────────────────────────────────────
    // GET /api/SA000012/GetFieldDefs
    // 從資料辭典 CURdTableField 取得 SPOdStrikeSourceInv 的欄位定義
    // 供前端動態建立 Grid 欄位標題、寬度、唯讀性、格式、顏色條件
    // ────────────────────────────────────────────────────────────────
    [HttpGet("GetFieldDefs")]
    public async Task<IActionResult> GetFieldDefs()
    {
        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();
            const string sql = @"
                SELECT FieldName,
                       ISNULL(DisplayLabel, FieldName)  AS DisplayLabel,
                       ISNULL(DisplaySize,  80)          AS DisplaySize,
                       ISNULL(SerialNum,    9999)        AS SerialNum,
                       ISNULL(ReadOnly,     0)           AS ReadOnly,
                       ISNULL(IsMoneyField, 0)           AS IsMoneyField,
                       ISNULL(Visible,      1)           AS Visible,
                       ISNULL(bFooter,      0)           AS bFooter,
                       HightLightRed,
                       HightLightNavy,
                       FormatStr,
                       EditColor
                FROM CURdTableField WITH (NOLOCK)
                WHERE TableName = 'SPOdStrikeSourceInv'
                ORDER BY ISNULL(SerialNum, 9999), FieldName";

            await using var cmd = new SqlCommand(sql, conn);
            var fields = new List<Dictionary<string, object?>>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                fields.Add(new Dictionary<string, object?>
                {
                    ["fieldName"]     = reader["FieldName"]?.ToString()?.Trim() ?? "",
                    ["displayLabel"]  = reader["DisplayLabel"]?.ToString()?.Trim() ?? "",
                    ["displaySize"]   = reader.IsDBNull(reader.GetOrdinal("DisplaySize")) ? 80 : Convert.ToInt32(reader["DisplaySize"]),
                    ["readOnly"]      = reader.IsDBNull(reader.GetOrdinal("ReadOnly"))    ? 1  : Convert.ToInt32(reader["ReadOnly"]),
                    ["isMoneyField"]  = reader.IsDBNull(reader.GetOrdinal("IsMoneyField"))? 0  : Convert.ToInt32(reader["IsMoneyField"]),
                    ["visible"]       = reader.IsDBNull(reader.GetOrdinal("Visible"))     ? 1  : Convert.ToInt32(reader["Visible"]),
                    ["bFooter"]       = reader.IsDBNull(reader.GetOrdinal("bFooter"))     ? 0  : Convert.ToInt32(reader["bFooter"]),
                    ["hightLightRed"] = reader.IsDBNull(reader.GetOrdinal("HightLightRed"))   ? null : reader["HightLightRed"]?.ToString()?.Trim(),
                    ["hightLightNavy"]= reader.IsDBNull(reader.GetOrdinal("HightLightNavy"))  ? null : reader["HightLightNavy"]?.ToString()?.Trim(),
                    ["formatStr"]     = reader.IsDBNull(reader.GetOrdinal("FormatStr"))   ? null : reader["FormatStr"]?.ToString()?.Trim(),
                    ["editColor"]     = reader.IsDBNull(reader.GetOrdinal("EditColor"))   ? null : reader["EditColor"]?.ToString()?.Trim()
                });
            }

            return Ok(new { ok = true, fields });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    // ────────────────────────────────────────────────────────────────
    // POST /api/SA000012/UpdateMaster
    // 更新主表 SPOdStrikeMain 的轉預收金額 / 超收金額
    // ────────────────────────────────────────────────────────────────
    public record UpdateMasterRequest(string PaperNum, decimal AdvanceAmountOg, decimal OverAmountOg);

    [HttpPost("UpdateMaster")]
    public async Task<IActionResult> UpdateMaster([FromBody] UpdateMasterRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.PaperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();
            const string sql = @"
                UPDATE SPOdStrikeMain
                SET    AdvanceAmountOg = @adv,
                       OverAmountOg    = @over
                WHERE  PaperNum = @p";
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@adv",  req.AdvanceAmountOg);
            cmd.Parameters.AddWithValue("@over", req.OverAmountOg);
            cmd.Parameters.AddWithValue("@p",    req.PaperNum);
            await cmd.ExecuteNonQueryAsync();
            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }
}