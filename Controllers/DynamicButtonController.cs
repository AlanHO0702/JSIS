using System.Data;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

/// <summary>
/// 動態按鈕執行 Controller
/// 從 CURdOCXItemCustButton 讀取按鈕設定，動態執行 SP
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DynamicButtonController : ControllerBase
{
    private readonly string _connStr;

    public DynamicButtonController(IConfiguration config)
    {
        _connStr = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    #region Helper Methods

    private static object? ToClrValue(object? v)
    {
        if (v is null) return DBNull.Value;
        if (v is not JsonElement je) return v;

        return je.ValueKind switch
        {
            JsonValueKind.Null or JsonValueKind.Undefined => DBNull.Value,
            JsonValueKind.String => je.GetString(),
            JsonValueKind.Number => je.TryGetInt64(out var l) ? l :
                                    je.TryGetDecimal(out var d) ? d :
                                    je.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => je.GetRawText()
        };
    }

    private static int? TryToInt(object o)
    {
        if (o == null || o is DBNull) return null;
        return int.TryParse(o.ToString(), out var n) ? n : null;
    }

    #endregion

    #region DTOs

    public class ButtonExecuteRequest
    {
        public string ItemId { get; set; } = "";
        public string ButtonName { get; set; } = "";
        public Dictionary<string, object>? Parameters { get; set; }
    }

    public class ButtonConfig
    {
        public string ItemId { get; set; } = "";
        public string ButtonName { get; set; } = "";
        public string CustCaption { get; set; } = "";
        public string CustHint { get; set; } = "";
        public string SpName { get; set; } = "";
        public int? bVisible { get; set; }
        public int? SerialNum { get; set; }
        public int? DesignType { get; set; }
        public int? bNeedNum { get; set; }
        public int? ChkCanUpdate { get; set; }
    }

    public class SearchParamConfig
    {
        public string ParamName { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public int? ControlType { get; set; }
        public string? CommandText { get; set; }
        public string? DefaultValue { get; set; }
        public int? DefaultType { get; set; }
        public int? ParamSN { get; set; }
        public string? ParamValue { get; set; }
        public int? ParamType { get; set; }
        public int? iReadOnly { get; set; }
        public int? iVisible { get; set; }
    }

    #endregion

    #region API Endpoints

    /// <summary>
    /// 取得指定頁面的所有可見按鈕
    /// GET /api/DynamicButton/buttons/{itemId}
    /// </summary>
    [HttpGet("buttons/{itemId}")]
    public async Task<IActionResult> GetButtons(string itemId)
    {
        var list = new List<ButtonConfig>();

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        // 動態偵測欄位名稱 (ChkCanUpdate 或 ChkCanbUpdate)
        var chkCol = await DetectChkColumnName(conn);

        var sql = $@"
            SELECT ItemId, ButtonName, CustCaption, CustHint, SpName,
                   bVisible, SerialNum, DesignType, bNeedNum, {chkCol} AS ChkCanUpdate
            FROM CURdOCXItemCustButton WITH (NOLOCK)
            WHERE ItemId = @itemId AND ISNULL(bVisible, 0) = 1
            ORDER BY SerialNum, ButtonName";

        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@itemId", itemId ?? "");

        using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            list.Add(new ButtonConfig
            {
                ItemId = rd["ItemId"]?.ToString() ?? "",
                ButtonName = rd["ButtonName"]?.ToString() ?? "",
                CustCaption = rd["CustCaption"]?.ToString() ?? "",
                CustHint = rd["CustHint"]?.ToString() ?? "",
                SpName = rd["SpName"]?.ToString() ?? "",
                bVisible = TryToInt(rd["bVisible"]),
                SerialNum = TryToInt(rd["SerialNum"]),
                DesignType = TryToInt(rd["DesignType"]),
                bNeedNum = TryToInt(rd["bNeedNum"]),
                ChkCanUpdate = TryToInt(rd["ChkCanUpdate"])
            });
        }

        return Ok(list);
    }

    /// <summary>
    /// 取得按鈕的參數設定
    /// GET /api/DynamicButton/params/{itemId}/{buttonName}
    /// </summary>
    [HttpGet("params/{itemId}/{buttonName}")]
    public async Task<IActionResult> GetButtonParams(string itemId, string buttonName)
    {
        var list = new List<SearchParamConfig>();

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        var sql = @"
            SELECT ParamName, DisplayName, ControlType, CommandText,
                   DefaultValue, DefaultType, ParamSN, ParamValue,
                   ParamType, iReadOnly, iVisible
            FROM CURdOCXSearchParams WITH (NOLOCK)
            WHERE ItemId = @itemId AND ButtonName = @buttonName
            ORDER BY ParamSN, ParamName";

        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@itemId", itemId ?? "");
        cmd.Parameters.AddWithValue("@buttonName", buttonName ?? "");

        using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            list.Add(new SearchParamConfig
            {
                ParamName = rd["ParamName"]?.ToString() ?? "",
                DisplayName = rd["DisplayName"]?.ToString() ?? "",
                ControlType = TryToInt(rd["ControlType"]),
                CommandText = rd["CommandText"]?.ToString(),
                DefaultValue = rd["DefaultValue"]?.ToString(),
                DefaultType = TryToInt(rd["DefaultType"]),
                ParamSN = TryToInt(rd["ParamSN"]),
                ParamValue = rd["ParamValue"]?.ToString(),
                ParamType = TryToInt(rd["ParamType"]),
                iReadOnly = TryToInt(rd["iReadOnly"]),
                iVisible = TryToInt(rd["iVisible"])
            });
        }

        return Ok(list);
    }

    /// <summary>
    /// 執行按鈕的 SP
    /// POST /api/DynamicButton/execute
    /// </summary>
    [HttpPost("execute")]
    public async Task<IActionResult> Execute([FromBody] ButtonExecuteRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ItemId) || string.IsNullOrWhiteSpace(request.ButtonName))
        {
            return BadRequest(new { success = false, error = "ItemId 和 ButtonName 為必填" });
        }

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        // 1. 取得按鈕設定
        var btnConfig = await GetButtonConfig(conn, request.ItemId, request.ButtonName);
        if (btnConfig == null)
        {
            return NotFound(new { success = false, error = "找不到按鈕設定" });
        }

        if (string.IsNullOrWhiteSpace(btnConfig.SpName))
        {
            return BadRequest(new { success = false, error = "按鈕未設定 SP 名稱" });
        }

        // 2. 取得參數設定
        var paramConfigs = await GetParamConfigs(conn, request.ItemId, request.ButtonName);

        // 3. 執行 SP
        SqlTransaction? tx = null;
        try
        {
            tx = (SqlTransaction)await conn.BeginTransactionAsync();

            await using var cmd = new SqlCommand(btnConfig.SpName, conn, tx)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 120
            };

            // 組合參數
            var userParams = request.Parameters ?? new Dictionary<string, object>();
            foreach (var paramDef in paramConfigs)
            {
                object? paramValue = DBNull.Value;

                // 優先使用前端傳來的值
                if (userParams.TryGetValue(paramDef.ParamName, out var frontValue))
                {
                    paramValue = ToClrValue(frontValue);
                }
                // 其次使用預設值
                else if (!string.IsNullOrEmpty(paramDef.DefaultValue))
                {
                    paramValue = paramDef.DefaultValue;
                }
                // 再其次使用 ParamValue
                else if (!string.IsNullOrEmpty(paramDef.ParamValue))
                {
                    paramValue = paramDef.ParamValue;
                }

                cmd.Parameters.AddWithValue("@" + paramDef.ParamName, paramValue ?? DBNull.Value);
            }

            // 如果有前端傳來但不在設定中的參數，也加入（相容性）
            foreach (var kv in userParams)
            {
                var paramName = "@" + kv.Key;
                if (!cmd.Parameters.Contains(paramName))
                {
                    cmd.Parameters.AddWithValue(paramName, ToClrValue(kv.Value) ?? DBNull.Value);
                }
            }

            var affected = await cmd.ExecuteNonQueryAsync();
            await tx.CommitAsync();

            return Ok(new
            {
                success = true,
                rowsAffected = affected,
                message = $"執行成功"
            });
        }
        catch (Exception ex)
        {
            if (tx != null) { try { await tx.RollbackAsync(); } catch { } }
            return StatusCode(500, new { success = false, error = ex.GetBaseException().Message });
        }
    }

    /// <summary>
    /// 執行按鈕的 SP 並回傳結果集（適用於查詢型 SP）
    /// POST /api/DynamicButton/executeQuery
    /// </summary>
    [HttpPost("executeQuery")]
    public async Task<IActionResult> ExecuteQuery([FromBody] ButtonExecuteRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ItemId) || string.IsNullOrWhiteSpace(request.ButtonName))
        {
            return BadRequest(new { success = false, error = "ItemId 和 ButtonName 為必填" });
        }

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        // 1. 取得按鈕設定
        var btnConfig = await GetButtonConfig(conn, request.ItemId, request.ButtonName);
        if (btnConfig == null)
        {
            return NotFound(new { success = false, error = "找不到按鈕設定" });
        }

        if (string.IsNullOrWhiteSpace(btnConfig.SpName))
        {
            return BadRequest(new { success = false, error = "按鈕未設定 SP 名稱" });
        }

        // 2. 取得參數設定
        var paramConfigs = await GetParamConfigs(conn, request.ItemId, request.ButtonName);

        try
        {
            await using var cmd = new SqlCommand(btnConfig.SpName, conn)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 120
            };

            // 組合參數
            var userParams = request.Parameters ?? new Dictionary<string, object>();
            foreach (var paramDef in paramConfigs)
            {
                object? paramValue = DBNull.Value;

                if (userParams.TryGetValue(paramDef.ParamName, out var frontValue))
                {
                    paramValue = ToClrValue(frontValue);
                }
                else if (!string.IsNullOrEmpty(paramDef.DefaultValue))
                {
                    paramValue = paramDef.DefaultValue;
                }
                else if (!string.IsNullOrEmpty(paramDef.ParamValue))
                {
                    paramValue = paramDef.ParamValue;
                }

                cmd.Parameters.AddWithValue("@" + paramDef.ParamName, paramValue ?? DBNull.Value);
            }

            foreach (var kv in userParams)
            {
                var paramName = "@" + kv.Key;
                if (!cmd.Parameters.Contains(paramName))
                {
                    cmd.Parameters.AddWithValue(paramName, ToClrValue(kv.Value) ?? DBNull.Value);
                }
            }

            // 執行並讀取結果集
            var results = new List<Dictionary<string, object?>>();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object?>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var name = reader.GetName(i);
                    var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    row[name] = value;
                }
                results.Add(row);
            }

            return Ok(new
            {
                success = true,
                data = results,
                count = results.Count
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, error = ex.GetBaseException().Message });
        }
    }

    #endregion

    #region Private Methods

    private async Task<string> DetectChkColumnName(SqlConnection conn)
    {
        var sql = @"SELECT name FROM sys.columns
                    WHERE object_id = OBJECT_ID('dbo.CURdOCXItemCustButton')";
        var cols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        await using var cmd = new SqlCommand(sql, conn);
        using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            cols.Add(rd.GetString(0));
        }

        return cols.Contains("ChkCanUpdate") ? "ChkCanUpdate" : "ChkCanbUpdate";
    }

    private async Task<ButtonConfig?> GetButtonConfig(SqlConnection conn, string itemId, string buttonName)
    {
        var chkCol = await DetectChkColumnName(conn);

        var sql = $@"
            SELECT ItemId, ButtonName, CustCaption, CustHint, SpName,
                   bVisible, SerialNum, DesignType, bNeedNum, {chkCol} AS ChkCanUpdate
            FROM CURdOCXItemCustButton WITH (NOLOCK)
            WHERE ItemId = @itemId AND ButtonName = @buttonName";

        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@itemId", itemId);
        cmd.Parameters.AddWithValue("@buttonName", buttonName);

        using var rd = await cmd.ExecuteReaderAsync();
        if (await rd.ReadAsync())
        {
            return new ButtonConfig
            {
                ItemId = rd["ItemId"]?.ToString() ?? "",
                ButtonName = rd["ButtonName"]?.ToString() ?? "",
                CustCaption = rd["CustCaption"]?.ToString() ?? "",
                CustHint = rd["CustHint"]?.ToString() ?? "",
                SpName = rd["SpName"]?.ToString() ?? "",
                bVisible = TryToInt(rd["bVisible"]),
                SerialNum = TryToInt(rd["SerialNum"]),
                DesignType = TryToInt(rd["DesignType"]),
                bNeedNum = TryToInt(rd["bNeedNum"]),
                ChkCanUpdate = TryToInt(rd["ChkCanUpdate"])
            };
        }

        return null;
    }

    private async Task<List<SearchParamConfig>> GetParamConfigs(SqlConnection conn, string itemId, string buttonName)
    {
        var list = new List<SearchParamConfig>();

        var sql = @"
            SELECT ParamName, DisplayName, ControlType, CommandText,
                   DefaultValue, DefaultType, ParamSN, ParamValue,
                   ParamType, iReadOnly, iVisible
            FROM CURdOCXSearchParams WITH (NOLOCK)
            WHERE ItemId = @itemId AND ButtonName = @buttonName
            ORDER BY ParamSN, ParamName";

        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@itemId", itemId);
        cmd.Parameters.AddWithValue("@buttonName", buttonName);

        using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            list.Add(new SearchParamConfig
            {
                ParamName = rd["ParamName"]?.ToString() ?? "",
                DisplayName = rd["DisplayName"]?.ToString() ?? "",
                ControlType = TryToInt(rd["ControlType"]),
                CommandText = rd["CommandText"]?.ToString(),
                DefaultValue = rd["DefaultValue"]?.ToString(),
                DefaultType = TryToInt(rd["DefaultType"]),
                ParamSN = TryToInt(rd["ParamSN"]),
                ParamValue = rd["ParamValue"]?.ToString(),
                ParamType = TryToInt(rd["ParamType"]),
                iReadOnly = TryToInt(rd["iReadOnly"]),
                iVisible = TryToInt(rd["iVisible"])
            });
        }

        return list;
    }

    #endregion
}
