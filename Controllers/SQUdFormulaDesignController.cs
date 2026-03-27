using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

[ApiController]
[Route("api/[controller]")]
public class SQUdFormulaDesignController : ControllerBase
{
    private readonly string _connStr;

    public SQUdFormulaDesignController(IConfiguration config)
    {
        _connStr = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    /// <summary>載入因素資料 (FormulaText, FactorType, DataType, ProcCode, ProcCount)</summary>
    [HttpGet("Factor")]
    public async Task<IActionResult> GetFactor([FromQuery] string sysId, [FromQuery] string factorId)
    {
        if (string.IsNullOrWhiteSpace(sysId) || string.IsNullOrWhiteSpace(factorId))
            return BadRequest(new { ok = false, error = "sysId 和 factorId 為必填" });

        var tableName = $"{sysId.Trim()}dFactor";
        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        // 驗證資料表存在
        await using var chk = new SqlCommand(
            "SELECT COUNT(*) FROM sys.objects WHERE name=@t AND type IN ('U','V')", conn);
        chk.Parameters.AddWithValue("@t", tableName);
        if ((int)(await chk.ExecuteScalarAsync())! == 0)
            return NotFound(new { ok = false, error = $"Table '{tableName}' not found." });

        var sql = $"SELECT FactorType, DataType, ProcCode, ProcCount, "
                + $"FormulaText=CONVERT(NVARCHAR(4000), FormulaText) "
                + $"FROM [{tableName}] (NOLOCK) WHERE FactorId=@fid";
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@fid", factorId.Trim());
        await using var rd = await cmd.ExecuteReaderAsync();

        if (!await rd.ReadAsync())
            return Ok(new { ok = true, found = false });

        return Ok(new
        {
            ok = true,
            found = true,
            factorType = rd["FactorType"]?.ToString()?.Trim() ?? "",
            dataType = rd["DataType"]?.ToString()?.Trim() ?? "",
            procCode = rd["ProcCode"]?.ToString()?.Trim() ?? "",
            procCount = rd["ProcCount"]?.ToString()?.Trim() ?? "",
            formulaText = rd["FormulaText"]?.ToString() ?? ""
        });
    }

    /// <summary>載入樹狀資料來源項目 (SQUdTableDictionary)</summary>
    [HttpGet("TreeData")]
    public async Task<IActionResult> TreeData([FromQuery] int powerType = 103, [FromQuery] string? systemId = null)
    {
        var list = new List<object>();
        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        var sql = string.IsNullOrWhiteSpace(systemId)
            ? "EXEC SQUdTableDictionary @PowerType"
            : "EXEC SQUdTableDictionary @PowerType, @SystemId";

        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@PowerType", powerType);
        if (!string.IsNullOrWhiteSpace(systemId))
            cmd.Parameters.AddWithValue("@SystemId", systemId.Trim());

        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            list.Add(new
            {
                itemId = (rd["ItemId"] ?? "").ToString()?.Trim(),
                itemName = (rd["ItemName"] ?? "").ToString()?.Trim(),
                levelNo = rd["LevelNo"] is DBNull ? 0 : Convert.ToInt32(rd["LevelNo"]),
                superId = (rd["SuperId"] ?? "").ToString()?.Trim()
            });
        }
        return Ok(list);
    }

    /// <summary>載入 FactorType 下拉選項</summary>
    [HttpGet("FactorTypes")]
    public async Task<IActionResult> FactorTypes()
    {
        var list = new List<object>();
        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(
            "SELECT t2.FactorType, t2.FactorTypeName FROM dbo.SQUdFactorType t2(NOLOCK)", conn);
        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            list.Add(new
            {
                value = rd["FactorType"]?.ToString()?.Trim(),
                text = rd["FactorTypeName"]?.ToString()?.Trim()
            });
        }
        return Ok(list);
    }

    /// <summary>載入 DataType 下拉選項</summary>
    [HttpGet("DataTypes")]
    public IActionResult DataTypes()
    {
        return Ok(new[]
        {
            new { value = "0", text = "ISNULL=NULL" },
            new { value = "1", text = "ISNULL=0" },
            new { value = "2", text = "ISNULL=空白" }
        });
    }

    /// <summary>載入 ProcCount 下拉選項</summary>
    [HttpGet("ProcCounts")]
    public IActionResult ProcCounts()
    {
        return Ok(new[]
        {
            new { value = "0", text = "非途程展開" },
            new { value = "1", text = "乘以(*)製程次數" }
        });
    }

    /// <summary>載入 ProcCode 下拉選項</summary>
    [HttpGet("ProcCodes")]
    public async Task<IActionResult> ProcCodes()
    {
        var list = new List<object>();
        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(
            "SELECT ProcCode, ProcName FROM dbo.EMOdProcInfo(NOLOCK) ORDER BY ProcCode", conn);
        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            list.Add(new
            {
                value = rd["ProcCode"]?.ToString()?.Trim(),
                text = $"{rd["ProcCode"]?.ToString()?.Trim()} - {rd["ProcName"]?.ToString()?.Trim()}"
            });
        }
        return Ok(list);
    }

    /// <summary>儲存公式文字及相關欄位</summary>
    [HttpPost("Save")]
    public async Task<IActionResult> Save([FromBody] SaveRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.SysId) || string.IsNullOrWhiteSpace(req.FactorId))
            return BadRequest(new { ok = false, error = "sysId 和 factorId 為必填" });

        var tableName = $"{req.SysId.Trim()}dFactor";
        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        var sql = $"UPDATE [{tableName}] SET FormulaText=@formula, FactorType=@ftype, DataType=@dtype, "
                + $"ProcCount=@pcount, UserId=@userId";

        if (req.SysId.Trim().Equals("SQU", StringComparison.OrdinalIgnoreCase))
            sql += ", ProcCode=@pcode";

        sql += " WHERE FactorId=@fid";

        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@formula", req.FormulaText ?? "");
        cmd.Parameters.AddWithValue("@ftype", req.FactorType ?? "");
        cmd.Parameters.AddWithValue("@dtype", req.DataType ?? "");
        cmd.Parameters.AddWithValue("@pcount", req.ProcCount ?? "");
        cmd.Parameters.AddWithValue("@userId", req.UserId ?? "");
        cmd.Parameters.AddWithValue("@fid", req.FactorId.Trim());

        if (req.SysId.Trim().Equals("SQU", StringComparison.OrdinalIgnoreCase))
            cmd.Parameters.AddWithValue("@pcode", req.ProcCode ?? "");

        try
        {
            var affected = await cmd.ExecuteNonQueryAsync();
            return Ok(new { ok = true, affected });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>取得按鈕參數 (CURdOCXItmCusBtnParam)，回傳 SeqNum → { tableKind, paramFieldName, paramType }</summary>
    [HttpGet("ButtonParams")]
    public async Task<IActionResult> ButtonParams([FromQuery] string itemId, [FromQuery] string buttonName)
    {
        if (string.IsNullOrWhiteSpace(itemId) || string.IsNullOrWhiteSpace(buttonName))
            return BadRequest(new { ok = false, error = "itemId 和 buttonName 為必填" });

        var list = new List<object>();
        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(@"
            SELECT SeqNum, TableKind, ParamFieldName, ParamType
              FROM CURdOCXItmCusBtnParam WITH (NOLOCK)
             WHERE ItemId=@id AND ButtonName=@btn
             ORDER BY SeqNum", conn);
        cmd.Parameters.AddWithValue("@id", itemId.Trim());
        cmd.Parameters.AddWithValue("@btn", buttonName.Trim());
        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            list.Add(new
            {
                seqNum = rd["SeqNum"] is DBNull ? 0 : Convert.ToInt32(rd["SeqNum"]),
                tableKind = rd["TableKind"]?.ToString()?.Trim() ?? "",
                paramFieldName = rd["ParamFieldName"]?.ToString()?.Trim() ?? "",
                paramType = rd["ParamType"] is DBNull ? 0 : Convert.ToInt32(rd["ParamType"])
            });
        }
        return Ok(list);
    }

    /// <summary>查詢產品層別 (EMOdProdLayer)，依料號+版本篩選</summary>
    [HttpGet("ProdLayers")]
    public async Task<IActionResult> ProdLayers([FromQuery] string partNum, [FromQuery] string revision)
    {
        var list = new List<object>();
        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand(@"
            SELECT DISTINCT LayerId, LayerName
              FROM dbo.EMOdProdLayer WITH (NOLOCK)
             WHERE PartNum = @pn AND Revision = @rev
             ORDER BY LayerId", conn);
        cmd.Parameters.AddWithValue("@pn", (partNum ?? "").Trim());
        cmd.Parameters.AddWithValue("@rev", (revision ?? "").Trim());

        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            list.Add(new
            {
                value = rd["LayerId"]?.ToString()?.Trim(),
                text = rd["LayerName"]?.ToString()?.Trim()
            });
        }
        return Ok(list);
    }

    /// <summary>試算：呼叫 SQUdQUComputeId_TestCal SP</summary>
    [HttpGet("TestCalc")]
    public async Task<IActionResult> TestCalc(
        [FromQuery] string factorId,
        [FromQuery] string partNum,
        [FromQuery] string revision,
        [FromQuery] string layerId,
        [FromQuery] int factorType = 0,
        [FromQuery] string sysId = "")
    {
        if (string.IsNullOrWhiteSpace(factorId))
            return BadRequest(new { ok = false, error = "factorId 為必填" });

        var list = new List<object>();
        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand("SQUdQUComputeId_TestCal", conn)
        {
            CommandType = System.Data.CommandType.StoredProcedure,
            CommandTimeout = 60
        };
        cmd.Parameters.AddWithValue("@FactorId", (factorId ?? "").Trim());
        cmd.Parameters.AddWithValue("@PartNum", (partNum ?? "").Trim());
        cmd.Parameters.AddWithValue("@Revision", (revision ?? "").Trim());
        cmd.Parameters.AddWithValue("@LayerId", (layerId ?? "").Trim());
        cmd.Parameters.AddWithValue("@POP", 0);
        cmd.Parameters.AddWithValue("@FactorType", factorType);
        cmd.Parameters.AddWithValue("@SysId", (sysId ?? "").Trim());

        try
        {
            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                // 讀取所有欄位（SP 回傳 Item, LevelNo, SuperId, Caption, Rs 等）
                var row = new Dictionary<string, object?>();
                for (int i = 0; i < rd.FieldCount; i++)
                {
                    var name = rd.GetName(i);
                    row[name] = rd.IsDBNull(i) ? null : rd.GetValue(i)?.ToString()?.Trim();
                }
                list.Add(row);
            }
            return Ok(new { ok = true, data = list });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    public class SaveRequest
    {
        public string SysId { get; set; } = "";
        public string FactorId { get; set; } = "";
        public string? FormulaText { get; set; }
        public string? FactorType { get; set; }
        public string? DataType { get; set; }
        public string? ProcCode { get; set; }
        public string? ProcCount { get; set; }
        public string? UserId { get; set; }
    }
}
