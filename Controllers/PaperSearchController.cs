using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PaperSearchController : ControllerBase
{
    private readonly string _cs;

    public PaperSearchController(IConfiguration cfg)
    {
        _cs = cfg.GetConnectionString("DefaultConnection")
              ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    public class SearchParamRow
    {
        public string ItemId { get; set; } = string.Empty;
        public string ButtonName { get; set; } = string.Empty;
        public string ParamName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int? ControlType { get; set; }
        public string? CommandText { get; set; }
        public string? DefaultValue { get; set; }
        public int? DefaultType { get; set; }
        public string EditMask { get; set; } = string.Empty;
        public string SuperId { get; set; } = string.Empty;
        public int? ParamSN { get; set; }
        public string? ParamValue { get; set; }
        public int? ParamType { get; set; }
        public int? iReadOnly { get; set; }
        public int? iVisible { get; set; }
    }

    public class InsKeyRow
    {
        public string ItemId { get; set; } = string.Empty;
        public string ButtonName { get; set; } = string.Empty;
        public string KeyFieldName { get; set; } = string.Empty;
        public int? SeqNum { get; set; }
        public int? PositionType { get; set; }
    }

    public class ButtonConfigDto
    {
        public string ItemId { get; set; } = string.Empty;
        public string ButtonName { get; set; } = string.Empty;
        public int? DesignType { get; set; }
        public string SpName { get; set; } = string.Empty;
        public string ExecSpName { get; set; } = string.Empty;
        public string SearchTemplate { get; set; } = string.Empty;
        public string MultiSelectDd { get; set; } = string.Empty;
        public int? ReplaceExists { get; set; }
        public string DialogCaption { get; set; } = string.Empty;
        public int? AllowSelCount { get; set; }
        public List<SearchParamRow> SearchParams { get; set; } = new();
        public List<InsKeyRow> InsertKeys { get; set; } = new();
    }

    public class PaperSearchRequest
    {
        public string ItemId { get; set; } = string.Empty;
        public string ButtonName { get; set; } = string.Empty;
        public string PaperNum { get; set; } = string.Empty;
        public Dictionary<string, object>? Params { get; set; }
    }

    public class PaperSearchConfirmRequest
    {
        public string ItemId { get; set; } = string.Empty;
        public string ButtonName { get; set; } = string.Empty;
        public string PaperNum { get; set; } = string.Empty;
        public bool Replace { get; set; }
        public Dictionary<string, object>? Inputs { get; set; }
        public List<Dictionary<string, object>> Rows { get; set; } = new();
    }

    [HttpGet("Config")]
    public async Task<IActionResult> Config([FromQuery] string itemId, [FromQuery] string buttonName)
    {
        if (string.IsNullOrWhiteSpace(itemId) || string.IsNullOrWhiteSpace(buttonName))
            return BadRequest(new { ok = false, error = "ItemId/ButtonName 為必填" });

        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync();

        var cfg = await LoadButtonConfigAsync(conn, itemId, buttonName);
        if (cfg == null)
            return NotFound(new { ok = false, error = "找不到按鈕設定" });

        cfg.SearchParams = await LoadSearchParamsAsync(conn, itemId, buttonName);
        cfg.InsertKeys = await LoadInsertKeysAsync(conn, itemId, buttonName);
        return Ok(cfg);
    }

    [HttpGet("ParamOptions")]
    public async Task<IActionResult> ParamOptions([FromQuery] string itemId, [FromQuery] string buttonName, [FromQuery] string paramName)
    {
        if (string.IsNullOrWhiteSpace(itemId) || string.IsNullOrWhiteSpace(buttonName) || string.IsNullOrWhiteSpace(paramName))
            return BadRequest("ItemId/ButtonName/ParamName required");

        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync();

        var sql = await LoadParamCommandTextAsync(conn, itemId, buttonName, paramName);
        if (string.IsNullOrWhiteSpace(sql))
            return NotFound();

        var raw = sql.Trim();
        if (!raw.StartsWith("select", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Only SELECT is allowed");

        await using var cmd = new SqlCommand(raw, conn);
        await using var reader = await cmd.ExecuteReaderAsync();

        var ordValue = -1;
        var ordText = -1;
        for (int i = 0; i < reader.FieldCount; i++)
        {
            var name = reader.GetName(i);
            if (ordValue == -1 && name.Equals("value", StringComparison.OrdinalIgnoreCase)) ordValue = i;
            if (ordText == -1 && name.Equals("text", StringComparison.OrdinalIgnoreCase)) ordText = i;
            if (ordValue == -1 && name.Equals("item", StringComparison.OrdinalIgnoreCase)) ordValue = i;
            if (ordText == -1 && name.Equals("itemname", StringComparison.OrdinalIgnoreCase)) ordText = i;
        }
        if (ordValue == -1 && reader.FieldCount > 0) ordValue = 0;
        if (ordText == -1 && reader.FieldCount > 1) ordText = 1;
        if (ordText == -1) ordText = ordValue;

        var list = new List<object>();
        while (await reader.ReadAsync())
        {
            var value = reader.IsDBNull(ordValue) ? "" : reader.GetValue(ordValue)?.ToString() ?? "";
            var text = reader.IsDBNull(ordText) ? "" : reader.GetValue(ordText)?.ToString() ?? "";
            list.Add(new { value, text });
        }
        return Ok(list);
    }

    [HttpPost("Search")]
    public async Task<IActionResult> Search([FromBody] PaperSearchRequest req, [FromQuery] int debug = 0)
    {
        if (string.IsNullOrWhiteSpace(req.ItemId) || string.IsNullOrWhiteSpace(req.ButtonName))
            return BadRequest(new { ok = false, error = "ItemId/ButtonName 為必填" });

        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync();

        var cfg = await LoadButtonConfigAsync(conn, req.ItemId, req.ButtonName);
        if (cfg == null)
            return BadRequest(new { ok = false, error = "找不到按鈕設定" });

        if (string.IsNullOrWhiteSpace(cfg.SpName))
            return BadRequest(new { ok = false, error = "找不到 SP 名稱" });

        var spName = ValidateProcName(cfg.SpName);
        var searchParams = await LoadSearchParamsAsync(conn, req.ItemId, req.ButtonName);
        var defaultMap = await LoadSearchDefaultMapAsync(conn, req.ItemId, req.ButtonName);
        var inputs = NormalizeInputs(req.Params);
        var systemId = await LoadSystemIdAsync(conn, req.ItemId);
        var (userId, useId) = await LoadUserContextAsync(conn);

        var paramValues = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var p in searchParams)
        {
            var paramName = NormalizeParamName(p.ParamName);
            if (string.IsNullOrWhiteSpace(paramName)) continue;
            var value = ResolveSearchParamValue(p, inputs, req.PaperNum, systemId, userId, useId, defaultMap);
            paramValues[paramName] = value;
        }

        await using var cmd = new SqlCommand(spName, conn)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 180
        };

        SqlCommandBuilder.DeriveParameters(cmd);
        foreach (SqlParameter spParam in cmd.Parameters)
        {
            if (spParam.Direction == ParameterDirection.ReturnValue) continue;
            if (paramValues.TryGetValue(spParam.ParameterName, out var val))
                spParam.Value = CoerceParamValue(spParam, val);
            else
                spParam.Value = spParam.Value ?? DBNull.Value;
        }

        var rows = new List<Dictionary<string, object?>>();
        await using var rdr = await cmd.ExecuteReaderAsync();
        while (await rdr.ReadAsync())
        {
            var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < rdr.FieldCount; i++)
                row[rdr.GetName(i)] = rdr.IsDBNull(i) ? null : rdr.GetValue(i);
            rows.Add(row);
        }

        if (debug == 1)
        {
            var echo = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            foreach (SqlParameter spParam in cmd.Parameters)
            {
                if (spParam.Direction == ParameterDirection.ReturnValue) continue;
                echo[spParam.ParameterName] = spParam.Value == DBNull.Value ? null : spParam.Value;
            }
            return Ok(new { ok = true, data = rows, debugParams = echo });
        }

        return Ok(new { ok = true, data = rows });
    }

    [HttpPost("Confirm")]
    public async Task<IActionResult> Confirm([FromBody] PaperSearchConfirmRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.ItemId) || string.IsNullOrWhiteSpace(req.ButtonName))
            return BadRequest(new { ok = false, error = "ItemId/ButtonName 為必填" });
        if (req.Rows == null || req.Rows.Count == 0)
            return BadRequest(new { ok = false, error = "沒有要匯入的資料" });

        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync();
        await using var tx = await conn.BeginTransactionAsync();

        try
        {
            var cfg = await LoadButtonConfigAsync(conn, req.ItemId, req.ButtonName, (SqlTransaction)tx);
            if (cfg == null)
                return BadRequest(new { ok = false, error = "找不到按鈕設定" });

            if (string.IsNullOrWhiteSpace(cfg.ExecSpName))
                return BadRequest(new { ok = false, error = "找不到 ExecSpName" });

            var spName = ValidateProcName(cfg.ExecSpName);
            var keys = await LoadInsertKeysAsync(conn, req.ItemId, req.ButtonName, (SqlTransaction)tx);
            var inputs = NormalizeInputs(req.Inputs);
            var systemId = await LoadSystemIdAsync(conn, req.ItemId, (SqlTransaction)tx);
            var (userId, useId) = await LoadUserContextAsync(conn, (SqlTransaction)tx);
            var allowReplace = (cfg.ReplaceExists ?? 0) != 0;

            for (int i = 0; i < req.Rows.Count; i++)
            {
                var row = req.Rows[i];
                var placeholders = keys.Count == 0
                    ? string.Empty
                    : " " + string.Join(", ", Enumerable.Range(1, keys.Count).Select(n => $"@p{n}"));
                await using var cmd = new SqlCommand($"EXEC {spName}{placeholders}", conn, (SqlTransaction)tx)
                {
                    CommandType = CommandType.Text,
                    CommandTimeout = 180
                };

                for (var k = 0; k < keys.Count; k++)
                {
                    var key = keys[k];
                    var value = ResolveInsertValue(key, row, inputs, req.PaperNum, i, req.Rows.Count, req.Replace, allowReplace, systemId, userId, useId);
                    cmd.Parameters.AddWithValue($"@p{k + 1}", value ?? DBNull.Value);
                }

                await cmd.ExecuteNonQueryAsync();
            }

            await tx.CommitAsync();
            return Ok(new { ok = true, rows = req.Rows.Count });
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return StatusCode(500, new { ok = false, error = ex.GetBaseException().Message });
        }
    }

    private static string NormalizeParamName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return string.Empty;
        return name.StartsWith("@", StringComparison.Ordinal) ? name : "@" + name.Trim();
    }

    private static string NormalizeInputKey(string key)
    {
        return (key ?? string.Empty).Trim().TrimStart('@').ToLowerInvariant();
    }

    private static Dictionary<string, object> NormalizeInputs(Dictionary<string, object>? raw)
    {
        var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        if (raw == null) return dict;
        foreach (var kv in raw)
        {
            var key = NormalizeInputKey(kv.Key);
            if (string.IsNullOrWhiteSpace(key)) continue;
            dict[key] = kv.Value;
        }
        return dict;
    }

    private static object? GetInput(Dictionary<string, object> inputs, string paramName)
    {
        var key = NormalizeInputKey(paramName);
        if (inputs.TryGetValue(key, out var val))
            return ToDbValue(val);
        return null;
    }

    private static object? ResolveSearchParamValue(
        SearchParamRow p,
        Dictionary<string, object> inputs,
        string paperNum,
        string? systemId,
        string userId,
        string useId,
        Dictionary<string, string?>? defaultMap)
    {
        var pt = p.ParamType ?? 0;
        switch (pt)
        {
            case 0:
            case 6:
            {
                var inputVal = GetInput(inputs, p.ParamName);
                if (inputVal is string s && string.IsNullOrWhiteSpace(s)) inputVal = null;
                if (inputVal != null) return inputVal;
                if (TryGetDefaultValue(defaultMap, p.ParamName, out var defFromList))
                    return defFromList;
                if (!string.IsNullOrWhiteSpace(p.DefaultValue)) return p.DefaultValue;
                if (!string.IsNullOrWhiteSpace(p.ParamValue)) return p.ParamValue;
                if (p.DefaultValue != null) return p.DefaultValue;
                if (p.ParamValue != null) return p.ParamValue;
                var name = (p.ParamName ?? string.Empty).Trim().ToLowerInvariant();
                if (name == "@includemat") return 1;
                if (name == "@calc") return 0;
                if (name == "@qntyover0") return 255;
                return string.Empty;
            }
            case 1:
                if (TryGetDefaultValue(defaultMap, p.ParamName, out var defVal))
                    return defVal;
                return p.ParamValue ?? p.DefaultValue ?? string.Empty;
            case 2:
                return string.IsNullOrWhiteSpace(userId) ? null : userId;
            case 3:
                return string.IsNullOrWhiteSpace(useId) ? null : useId;
            case 4:
                return systemId ?? string.Empty;
            case 5:
                return string.IsNullOrWhiteSpace(paperNum) ? null : paperNum;
            case 9:
                return Guid.NewGuid().ToString();
            default:
                return GetInput(inputs, p.ParamName);
        }
    }

    private static object? ResolveInsertValue(
        InsKeyRow key,
        Dictionary<string, object> row,
        Dictionary<string, object> inputs,
        string paperNum,
        int index,
        int total,
        bool replaceFlag,
        bool allowReplace,
        string? systemId,
        string userId,
        string useId)
    {
        var keyName = NormalizeInputKey(key.KeyFieldName);
        var pt = key.PositionType ?? 0;
        switch (pt)
        {
            case 0:
            {
                if (keyName == "dllpapernum") return paperNum;
                return GetRowValue(row, key.KeyFieldName);
            }
            case 1:
                return string.IsNullOrWhiteSpace(userId) ? null : userId;
            case 3:
                return string.IsNullOrWhiteSpace(useId) ? null : useId;
            case 4:
                return systemId ?? string.Empty;
            case 5:
                if (keyName == "papernum" || keyName == "dllpapernum") return paperNum;
                return string.IsNullOrWhiteSpace(paperNum) ? null : paperNum;
            case 6:
            case 11:
            {
                var val = GetInput(inputs, key.KeyFieldName);
                if (val is string s && string.IsNullOrWhiteSpace(s)) val = null;
                if (val != null) return val;
                if (keyName == "papernum" || keyName == "dllpapernum") return paperNum;
                return null;
            }
            case 7:
                return (index == 0 || index == total - 1) ? 1 : 0;
            case 8:
            {
                var rowVal = GetRowValue(row, key.KeyFieldName);
                if (rowVal is string s && string.IsNullOrWhiteSpace(s)) rowVal = null;
                return rowVal ?? (index + 1);
            }
            case 9:
                return (allowReplace && replaceFlag && index == 0) ? 1 : 0;
            case 10:
                return total;
            default:
                return GetRowValue(row, key.KeyFieldName);
        }
    }

    private static object? GetRowValue(Dictionary<string, object> row, string keyField)
    {
        if (row == null) return null;
        var key = NormalizeInputKey(keyField);
        foreach (var kv in row)
        {
            if (NormalizeInputKey(kv.Key) == key)
                return ToDbValue(kv.Value);
        }
        return null;
    }

    private async Task<(string UserId, string UseId)> LoadUserContextAsync(SqlConnection conn, SqlTransaction? tx = null)
    {
        var userId = string.Empty;
        var useId = string.Empty;

        var jwtHeader = Request?.Headers["X-JWTID"].ToString();
        if (!string.IsNullOrWhiteSpace(jwtHeader) && Guid.TryParse(jwtHeader, out var jwtId))
        {
            await using var cmd = new SqlCommand(
                "SELECT UserId FROM CURdUserOnline WITH (NOLOCK) WHERE JwtId = @jwtId", conn, tx);
            cmd.Parameters.AddWithValue("@jwtId", jwtId);
            userId = (await cmd.ExecuteScalarAsync())?.ToString() ?? string.Empty;
        }

        if (string.IsNullOrWhiteSpace(userId))
            userId = User?.Identity?.Name ?? string.Empty;

        userId = userId.Trim();
        if (!string.IsNullOrWhiteSpace(userId))
        {
            await using var cmd = new SqlCommand(
                "SELECT UseId FROM CURdUsers WITH (NOLOCK) WHERE UserId = @userId", conn, tx);
            cmd.Parameters.AddWithValue("@userId", userId);
            useId = (await cmd.ExecuteScalarAsync())?.ToString() ?? string.Empty;
        }

        if (string.IsNullOrWhiteSpace(useId))
        {
            var claim =
                User?.Claims?.FirstOrDefault(c => string.Equals(c.Type, "UseId", StringComparison.OrdinalIgnoreCase))
                ?? User?.Claims?.FirstOrDefault(c => string.Equals(c.Type, "useid", StringComparison.OrdinalIgnoreCase));
            useId = claim?.Value?.Trim() ?? string.Empty;
        }

        if (string.IsNullOrWhiteSpace(useId)) useId = "A001";

        return (userId, useId);
    }

    private static int? TryToInt(object? o)
    {
        if (o == null || o == DBNull.Value) return null;
        return int.TryParse(o.ToString(), out var n) ? n : null;
    }

    private static string ValidateProcName(string name)
    {
        var n = (name ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(n))
            throw new InvalidOperationException("Invalid SP name");

        var parts = n.Split('.', StringSplitOptions.RemoveEmptyEntries);
        foreach (var p in parts)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(p, "^[A-Za-z_][A-Za-z0-9_]*$"))
                throw new InvalidOperationException("Invalid SP name");
        }
        return n;
    }

    private async Task<ButtonConfigDto?> LoadButtonConfigAsync(SqlConnection conn, string itemId, string buttonName, SqlTransaction? tx = null)
    {
        var sql = @"
SELECT TOP 1 ItemId, ButtonName, ISNULL(DesignType,0) AS DesignType,
       SpName, ExecSpName, SearchTemplate, MultiSelectDD,
       ReplaceExists, DialogCaption, AllowSelCount
  FROM CURdOCXItemCustButton WITH (NOLOCK)
 WHERE ItemId=@itemId AND ButtonName=@btn;";

        await using var cmd = new SqlCommand(sql, conn, tx);
        cmd.Parameters.AddWithValue("@itemId", itemId ?? string.Empty);
        cmd.Parameters.AddWithValue("@btn", buttonName ?? string.Empty);

        await using var rd = await cmd.ExecuteReaderAsync();
        if (!await rd.ReadAsync()) return null;

        return new ButtonConfigDto
        {
            ItemId = rd["ItemId"]?.ToString() ?? string.Empty,
            ButtonName = rd["ButtonName"]?.ToString() ?? string.Empty,
            DesignType = TryToInt(rd["DesignType"]),
            SpName = rd["SpName"]?.ToString() ?? string.Empty,
            ExecSpName = rd["ExecSpName"]?.ToString() ?? string.Empty,
            SearchTemplate = rd["SearchTemplate"]?.ToString() ?? string.Empty,
            MultiSelectDd = rd["MultiSelectDD"]?.ToString() ?? string.Empty,
            ReplaceExists = TryToInt(rd["ReplaceExists"]),
            DialogCaption = rd["DialogCaption"]?.ToString() ?? string.Empty,
            AllowSelCount = TryToInt(rd["AllowSelCount"])
        };
    }

    private async Task<List<SearchParamRow>> LoadSearchParamsAsync(SqlConnection conn, string itemId, string buttonName, SqlTransaction? tx = null)
    {
        var list = new List<SearchParamRow>();
        var cmd = new SqlCommand(@"
SELECT ItemId, ButtonName, ParamName, DisplayName, ControlType, CommandText,
       DefaultValue, DefaultType, EditMask, SuperId, ParamSN, ParamValue,
       ParamType, iReadOnly, iVisible
  FROM CURdOCXSearchParams WITH (NOLOCK)
 WHERE ItemId=@itemId AND ButtonName=@btn
 ORDER BY ParamSN, ParamName;", conn, tx);
        cmd.Parameters.AddWithValue("@itemId", itemId ?? string.Empty);
        cmd.Parameters.AddWithValue("@btn", buttonName ?? string.Empty);
        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            list.Add(new SearchParamRow
            {
                ItemId = rd["ItemId"]?.ToString() ?? string.Empty,
                ButtonName = rd["ButtonName"]?.ToString() ?? string.Empty,
                ParamName = rd["ParamName"]?.ToString() ?? string.Empty,
                DisplayName = rd["DisplayName"]?.ToString() ?? string.Empty,
                ControlType = TryToInt(rd["ControlType"]),
                CommandText = rd["CommandText"] == DBNull.Value ? null : rd["CommandText"]?.ToString(),
                DefaultValue = rd["DefaultValue"] == DBNull.Value ? null : rd["DefaultValue"]?.ToString(),
                DefaultType = TryToInt(rd["DefaultType"]),
                EditMask = rd["EditMask"]?.ToString() ?? string.Empty,
                SuperId = rd["SuperId"]?.ToString() ?? string.Empty,
                ParamSN = TryToInt(rd["ParamSN"]),
                ParamValue = rd["ParamValue"] == DBNull.Value ? null : rd["ParamValue"]?.ToString(),
                ParamType = TryToInt(rd["ParamType"]),
                iReadOnly = TryToInt(rd["iReadOnly"]),
                iVisible = TryToInt(rd["iVisible"])
            });
        }
        return list;
    }

    private async Task<List<InsKeyRow>> LoadInsertKeysAsync(SqlConnection conn, string itemId, string buttonName, SqlTransaction? tx = null)
    {
        var list = new List<InsKeyRow>();
        var cmd = new SqlCommand(@"
SELECT ItemId, ButtonName, KeyFieldName, SeqNum, PositionType
  FROM CURdOCXItmCusBtnInsKey WITH (NOLOCK)
 WHERE ItemId=@itemId AND ButtonName=@btn
 ORDER BY SeqNum, KeyFieldName;", conn, tx);
        cmd.Parameters.AddWithValue("@itemId", itemId ?? string.Empty);
        cmd.Parameters.AddWithValue("@btn", buttonName ?? string.Empty);
        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            list.Add(new InsKeyRow
            {
                ItemId = rd["ItemId"]?.ToString() ?? string.Empty,
                ButtonName = rd["ButtonName"]?.ToString() ?? string.Empty,
                KeyFieldName = rd["KeyFieldName"]?.ToString() ?? string.Empty,
                SeqNum = TryToInt(rd["SeqNum"]),
                PositionType = TryToInt(rd["PositionType"])
            });
        }
        return list;
    }

    private async Task<string?> LoadParamCommandTextAsync(SqlConnection conn, string itemId, string buttonName, string paramName)
    {
        var cmd = new SqlCommand(@"
SELECT TOP 1 CommandText
  FROM CURdOCXSearchParams WITH (NOLOCK)
 WHERE ItemId=@itemId AND ButtonName=@btn AND ParamName=@param;", conn);
        cmd.Parameters.AddWithValue("@itemId", itemId ?? string.Empty);
        cmd.Parameters.AddWithValue("@btn", buttonName ?? string.Empty);
        cmd.Parameters.AddWithValue("@param", paramName ?? string.Empty);
        var obj = await cmd.ExecuteScalarAsync();
        return obj == null || obj == DBNull.Value ? null : obj.ToString();
    }

    private async Task<string?> LoadSystemIdAsync(SqlConnection conn, string itemId, SqlTransaction? tx = null)
    {
        var cmd = new SqlCommand("SELECT TOP 1 SystemId FROM CURdSysItems WITH (NOLOCK) WHERE ItemId=@itemId;", conn, tx);
        cmd.Parameters.AddWithValue("@itemId", itemId ?? string.Empty);
        var obj = await cmd.ExecuteScalarAsync();
        return obj == null || obj == DBNull.Value ? null : obj.ToString();
    }

    private static object? ToDbValue(object? v)
    {
        if (v == null) return null;
        if (v is JsonElement je) return FromJson(je);
        return v;
    }

    private async Task<Dictionary<string, string?>?> LoadSearchDefaultMapAsync(SqlConnection conn, string itemId, string buttonName)
    {
        var map = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        await using var cmd = new SqlCommand("exec CURdOCXPaperSearchParamList @p0, @p1, @p2", conn);
        cmd.Parameters.AddWithValue("@p0", itemId ?? string.Empty);
        cmd.Parameters.AddWithValue("@p1", buttonName ?? string.Empty);
        cmd.Parameters.AddWithValue("@p2", string.Empty);

        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            var field = rd["FieldName"]?.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(field)) continue;
            var val = rd["sInqValue"] == DBNull.Value ? null : rd["sInqValue"]?.ToString();
            map[NormalizeInputKey(field)] = val;
        }
        return map;
    }

    private static bool TryGetDefaultValue(Dictionary<string, string?>? defaults, string paramName, out string? value)
    {
        value = null;
        if (defaults == null) return false;
        var key = NormalizeInputKey(paramName);
        if (defaults.TryGetValue(key, out var v))
        {
            value = v;
            return true;
        }
        return false;
    }

    private static object? CoerceParamValue(SqlParameter spParam, object? value)
    {
        if (value == null) return DBNull.Value;
        if (value is DBNull) return DBNull.Value;

        if (value is string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                if (IsStringType(spParam)) return string.Empty;
                if (IsIntType(spParam) && ShouldBlankIntAsZero(spParam)) return 0;
                return DBNull.Value;
            }
            if (IsIntType(spParam) && int.TryParse(s, out var i)) return i;
            if (IsIntType(spParam)) return DBNull.Value;
            if (IsLongType(spParam) && long.TryParse(s, out var l)) return l;
            if (IsLongType(spParam)) return DBNull.Value;
            if (IsDecimalType(spParam) && decimal.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var d))
                return d;
            if (IsDecimalType(spParam)) return DBNull.Value;
            if (IsFloatType(spParam) && double.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var f))
                return f;
            if (IsFloatType(spParam)) return DBNull.Value;
            if (IsBitType(spParam) && TryParseBool(s, out var b)) return b ? 1 : 0;
            if (IsBitType(spParam)) return DBNull.Value;
            if (IsDateType(spParam) && DateTime.TryParse(s, out var dt)) return dt;
            if (IsDateType(spParam)) return DBNull.Value;
            if (IsGuidType(spParam) && Guid.TryParse(s, out var g)) return g;
            if (IsGuidType(spParam)) return DBNull.Value;
            return s;
        }

        return value;
    }

    private static bool IsStringType(SqlParameter p)
    {
        return p.SqlDbType is SqlDbType.NVarChar or SqlDbType.VarChar or SqlDbType.NChar or SqlDbType.Char
            or SqlDbType.NText or SqlDbType.Text;
    }

    private static bool IsIntType(SqlParameter p)
    {
        return p.SqlDbType is SqlDbType.Int or SqlDbType.SmallInt or SqlDbType.TinyInt;
    }

    private static bool IsLongType(SqlParameter p)
    {
        return p.SqlDbType is SqlDbType.BigInt;
    }

    private static bool IsDecimalType(SqlParameter p)
    {
        return p.SqlDbType is SqlDbType.Decimal or SqlDbType.Money or SqlDbType.SmallMoney;
    }

    private static bool IsFloatType(SqlParameter p)
    {
        return p.SqlDbType is SqlDbType.Float or SqlDbType.Real;
    }

    private static bool IsBitType(SqlParameter p)
    {
        return p.SqlDbType is SqlDbType.Bit;
    }

    private static bool IsDateType(SqlParameter p)
    {
        return p.SqlDbType is SqlDbType.Date or SqlDbType.DateTime or SqlDbType.DateTime2 or SqlDbType.SmallDateTime or SqlDbType.DateTimeOffset;
    }

    private static bool IsGuidType(SqlParameter p)
    {
        return p.SqlDbType is SqlDbType.UniqueIdentifier;
    }

    private static bool ShouldBlankIntAsZero(SqlParameter p)
    {
        var name = (p.ParameterName ?? string.Empty).Trim().TrimStart('@').ToLowerInvariant();
        return name == "moneycode" || name == "outnotin";
    }

    private static bool TryParseBool(string s, out bool b)
    {
        if (bool.TryParse(s, out b)) return true;
        if (int.TryParse(s, out var n)) { b = n != 0; return true; }
        if (string.Equals(s, "Y", StringComparison.OrdinalIgnoreCase)) { b = true; return true; }
        if (string.Equals(s, "N", StringComparison.OrdinalIgnoreCase)) { b = false; return true; }
        b = false;
        return false;
    }

    private static object? FromJson(JsonElement je)
    {
        return je.ValueKind switch
        {
            JsonValueKind.Null or JsonValueKind.Undefined => null,
            JsonValueKind.String => je.GetString(),
            JsonValueKind.Number => je.TryGetInt64(out var l) ? l :
                                   je.TryGetDecimal(out var d) ? d :
                                   je.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => je.GetRawText()
        };
    }
}
