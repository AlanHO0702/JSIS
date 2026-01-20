using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class CustomButtonExecController : ControllerBase
{
    private readonly string _connStr;

    public CustomButtonExecController(IConfiguration config)
    {
        _connStr = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    public class RunRequest
    {
        public string ItemId { get; set; } = "";
        public string ButtonName { get; set; } = "";
        public string? PaperNum { get; set; }
        public string? TableName { get; set; }
        public string? PaperMode { get; set; }
        public string? UserId { get; set; }
        public string? UseId { get; set; }
        public string? GlobalId { get; set; }
        public int? OpKind { get; set; }
        public bool? AcceptConfirm { get; set; }
    }

    [HttpPost("Run")]
    public async Task<IActionResult> Run([FromBody] RunRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.ItemId) || string.IsNullOrWhiteSpace(req.ButtonName))
            return BadRequest(new { ok = false, error = "ItemId/ButtonName 為必填" });

        var paperNum = (req.PaperNum ?? string.Empty).Trim();
        var paperMode = (req.PaperMode ?? "BROWSE").Trim().ToUpperInvariant();
        var userId = string.IsNullOrWhiteSpace(req.UserId) ? "admin" : req.UserId.Trim();
        var useId = string.IsNullOrWhiteSpace(req.UseId) ? "A001" : req.UseId.Trim();
        var opKind = req.OpKind ?? 0;

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        var info = await LoadButtonInfoAsync(conn, req.ItemId, req.ButtonName, req.GlobalId ?? "");
        if (info.Count == 0)
            return BadRequest(new { ok = false, error = "找不到按鈕設定" });

        var needInEdit = GetInt(info, "bNeedInEdit");
        if (needInEdit == 1 && paperMode != "UPDATE")
            return Ok(new { ok = false, error = "必須在「編輯中」才可使用此功能" });

        var chkCanUpdate = GetInt(info, "ChkCanbUpdate");
        if (chkCanUpdate == 1)
        {
            var canUpdate = await LoadCanUpdateAsync(conn, userId, useId, req.ItemId);
            if (canUpdate == 0)
                return Ok(new { ok = false, error = "您沒有編輯的權限" });
        }

        var needNum = GetInt(info, "bNeedNum");
        if (needNum == 1 && string.IsNullOrWhiteSpace(paperNum))
            return Ok(new { ok = false, error = "沒有「單據號碼」可供操作" });

        var chkStatus = GetInt(info, "ChkStatus");
        var isUpdateMoney = GetInt(info, "IsUpdateMoney");
        if (opKind == 1 && chkStatus == 1)
        {
            if (string.IsNullOrWhiteSpace(req.TableName))
                return Ok(new { ok = false, error = "未提供單據資料表" });

            var finished = await LoadFinishedAsync(conn, req.TableName!, paperNum);
            if (finished.HasValue && finished.Value != 0 && finished.Value != 3)
            {
                if (isUpdateMoney == 0)
                    return Ok(new { ok = false, error = "只有「作業中」及「審核中」的單據才可使用此功能" });
                if (finished.Value == 2)
                    return Ok(new { ok = false, error = "單據「已作廢」，不可使用此功能" });
            }
        }

        var needConfirm = GetInt(info, "iNeedConfirmBefExec");
        var confirmText = GetString(info, "sConfirmBefExec");
        if (needConfirm == 1 && !req.AcceptConfirm.GetValueOrDefault())
        {
            var caption = GetString(info, "CustCaption");
            if (string.IsNullOrWhiteSpace(confirmText) && !string.IsNullOrWhiteSpace(caption))
                confirmText = $"確認要「{caption}」嗎？";
            return Ok(new { ok = false, needConfirm = true, message = confirmText });
        }

        var beforeSql = GetString(info, "BeforeRunSQL");
        if (!string.IsNullOrWhiteSpace(beforeSql))
        {
            if (!string.IsNullOrWhiteSpace(req.TableName) && !string.IsNullOrWhiteSpace(paperNum))
            {
                var row = await LoadRowAsync(conn, req.TableName!, paperNum);
                if (row.Count > 0)
                    beforeSql = ReplaceSqlTokens(beforeSql, row);
            }
            beforeSql = ReplaceSqlTokens(beforeSql, req.TableName, paperNum);
            await ExecSqlAsync(conn, beforeSql);
        }

        var designType = GetInt(info, "DesignType");
        if (designType != 3)
            return Ok(new { ok = false, error = "此按鈕目前僅支援呼叫 SP" });

        var spNameRaw = GetString(info, "ExecSpName");
        if (string.IsNullOrWhiteSpace(spNameRaw))
            spNameRaw = GetString(info, "SpName");
        if (string.IsNullOrWhiteSpace(spNameRaw))
            return Ok(new { ok = false, error = "找不到 SP 名稱" });

        var paramDefs = await LoadButtonParamsAsync(conn, req.ItemId, req.ButtonName);
        var tableMap = await LoadTableMapAsync(conn, req.ItemId);
        var systemId = await LoadSystemIdAsync(conn, req.ItemId);
        var spHasResult = GetInt(info, "bSpHasResult");

        var placeholders = paramDefs.Count == 0
            ? string.Empty
            : " " + string.Join(", ", Enumerable.Range(1, paramDefs.Count).Select(i => $"@p{i}"));

        await using var cmd = new SqlCommand($"EXEC {QuoteIdentifier(spNameRaw)}{placeholders}", conn)
        {
            CommandType = CommandType.Text,
            CommandTimeout = 180
        };

        for (var i = 0; i < paramDefs.Count; i++)
        {
            var p = paramDefs[i];
            var value = await ResolveParamValueAsync(conn, tableMap, paperNum, p, systemId, userId, useId);
            cmd.Parameters.AddWithValue($"@p{i + 1}", value ?? DBNull.Value);
        }

        if (spHasResult == 1)
        {
            await using var rd = await cmd.ExecuteReaderAsync();
            if (await rd.ReadAsync())
            {
                var msg = rd.GetValue(0)?.ToString() ?? string.Empty;
                if (msg.StartsWith("ERROR:", StringComparison.OrdinalIgnoreCase) ||
                    msg.StartsWith("ABORT:", StringComparison.OrdinalIgnoreCase))
                    return Ok(new { ok = false, error = msg.Substring(6) });
                return Ok(new { ok = true, message = msg });
            }
            return Ok(new { ok = true });
        }

        var affected = await cmd.ExecuteNonQueryAsync();
        return Ok(new { ok = true, rowsAffected = affected });
    }

    private static async Task<Dictionary<string, object?>> LoadButtonInfoAsync(SqlConnection conn, string itemId, string buttonName, string globalId)
    {
        var result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        await using var cmd = new SqlCommand("exec CURdCustButtonInfoGet @ItemId, @ButtonName, @GlobalId", conn);
        cmd.Parameters.AddWithValue("@ItemId", itemId);
        cmd.Parameters.AddWithValue("@ButtonName", buttonName);
        cmd.Parameters.AddWithValue("@GlobalId", globalId ?? string.Empty);
        await using var rd = await cmd.ExecuteReaderAsync();
        if (!await rd.ReadAsync()) return result;
        for (var i = 0; i < rd.FieldCount; i++)
        {
            var name = rd.GetName(i);
            if (string.IsNullOrWhiteSpace(name)) continue;
            result[name] = rd.IsDBNull(i) ? null : rd.GetValue(i);
        }
        return result;
    }

    private static int GetInt(Dictionary<string, object?> row, string key)
    {
        if (!row.TryGetValue(key, out var obj) || obj == null) return 0;
        return int.TryParse(obj.ToString(), out var v) ? v : 0;
    }

    private static string GetString(Dictionary<string, object?> row, string key)
    {
        if (!row.TryGetValue(key, out var obj) || obj == null) return string.Empty;
        return obj.ToString() ?? string.Empty;
    }

    private static async Task<int> LoadCanUpdateAsync(SqlConnection conn, string userId, string useId, string itemId)
    {
        const string sql = "exec CURdGetUserSysItems @Blank1, @UserId, @Blank2, @UseId, @Zero, @ItemId";
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Blank1", "");
        cmd.Parameters.AddWithValue("@UserId", userId ?? "");
        cmd.Parameters.AddWithValue("@Blank2", "");
        cmd.Parameters.AddWithValue("@UseId", useId ?? "");
        cmd.Parameters.AddWithValue("@Zero", 0);
        cmd.Parameters.AddWithValue("@ItemId", itemId ?? "");
        await using var rd = await cmd.ExecuteReaderAsync();
        if (!await rd.ReadAsync()) return 0;
        var obj = rd["bUpdate"];
        return obj == DBNull.Value ? 0 : Convert.ToInt32(obj);
    }

    private static async Task<int?> LoadFinishedAsync(SqlConnection conn, string tableName, string paperNum)
    {
        if (string.IsNullOrWhiteSpace(tableName) || string.IsNullOrWhiteSpace(paperNum)) return null;
        if (!Regex.IsMatch(tableName, "^[A-Za-z0-9_]+$")) return null;
        var sql = $"SELECT TOP 1 Finished FROM [{tableName}] WITH (NOLOCK) WHERE PaperNum = @PaperNum";
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@PaperNum", paperNum);
        var obj = await cmd.ExecuteScalarAsync();
        if (obj == null || obj == DBNull.Value) return null;
        return int.TryParse(obj.ToString(), out var v) ? v : null;
    }

    private static string ReplaceSqlTokens(string sql, string? tableName, string paperNum)
    {
        if (string.IsNullOrWhiteSpace(sql)) return sql;
        var result = sql.Replace("@PaperNum", ToSqlLiteral(paperNum), StringComparison.OrdinalIgnoreCase);
        if (!string.IsNullOrWhiteSpace(tableName))
            result = result.Replace("@PaperId", ToSqlLiteral(tableName), StringComparison.OrdinalIgnoreCase);
        return result;
    }

    private static string ReplaceSqlTokens(string sql, Dictionary<string, object?> row)
    {
        if (string.IsNullOrWhiteSpace(sql)) return sql;
        foreach (var kv in row)
        {
            if (string.IsNullOrWhiteSpace(kv.Key)) continue;
            sql = Regex.Replace(
                sql,
                "@" + Regex.Escape(kv.Key),
                ToSqlLiteral(kv.Value),
                RegexOptions.IgnoreCase);
        }
        return sql;
    }

    private static async Task<Dictionary<string, object?>> LoadRowAsync(SqlConnection conn, string tableName, string paperNum)
    {
        var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(tableName) || string.IsNullOrWhiteSpace(paperNum)) return row;
        if (!Regex.IsMatch(tableName, "^[A-Za-z0-9_]+$")) return row;
        var sql = $"SELECT TOP 1 * FROM [{tableName}] WITH (NOLOCK) WHERE PaperNum = @PaperNum";
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@PaperNum", paperNum);
        await using var rd = await cmd.ExecuteReaderAsync();
        if (!await rd.ReadAsync()) return row;
        for (var i = 0; i < rd.FieldCount; i++)
        {
            var name = rd.GetName(i);
            if (string.IsNullOrWhiteSpace(name)) continue;
            row[name] = rd.IsDBNull(i) ? null : rd.GetValue(i);
        }
        return row;
    }

    private static string ToSqlLiteral(object? value)
    {
        if (value == null || value == DBNull.Value) return "NULL";
        if (value is bool b) return b ? "1" : "0";
        if (value is byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal)
            return Convert.ToString(value, CultureInfo.InvariantCulture) ?? "0";
        if (value is DateTime dt)
            return $"'{dt:yyyy/MM/dd HH:mm:ss}'";
        var s = value.ToString() ?? "";
        return $"'{s.Replace("'", "''")}'";
    }

    private static async Task ExecSqlAsync(SqlConnection conn, string sql)
    {
        await using var cmd = new SqlCommand(sql, conn);
        await cmd.ExecuteNonQueryAsync();
    }

    private sealed record ButtonParamRow(int SeqNum, int? ParamType, string? TableKind, string? ParamFieldName);
    private sealed record TableKindMap(string Kind, string TableName);

    private static string QuoteIdentifier(string name)
    {
        var n = (name ?? string.Empty).Trim();
        var parts = n.Split('.', StringSplitOptions.RemoveEmptyEntries);
        foreach (var p in parts)
        {
            if (!Regex.IsMatch(p, "^[A-Za-z_][A-Za-z0-9_]*$"))
                throw new InvalidOperationException("非法的資料表或欄位名稱");
        }
        return string.Join(".", parts.Select(p => $"[{p}]"));
    }

    private async Task<List<ButtonParamRow>> LoadButtonParamsAsync(SqlConnection conn, string itemId, string buttonName)
    {
        const string sql = @"
SELECT SeqNum, ParamType, TableKind, ParamFieldName
  FROM CURdOCXItmCusBtnParam WITH (NOLOCK)
 WHERE ItemId = @itemId AND ButtonName = @buttonName
 ORDER BY SeqNum, Seq;";
        var list = new List<ButtonParamRow>();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@itemId", itemId);
        cmd.Parameters.AddWithValue("@buttonName", buttonName);
        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            list.Add(new ButtonParamRow(
                SeqNum: Convert.ToInt32(rd["SeqNum"] ?? 0),
                ParamType: rd["ParamType"] == DBNull.Value ? null : Convert.ToInt32(rd["ParamType"]),
                TableKind: rd["TableKind"]?.ToString(),
                ParamFieldName: rd["ParamFieldName"]?.ToString()
            ));
        }
        return list;
    }

    private async Task<Dictionary<string, string>> LoadTableMapAsync(SqlConnection conn, string itemId)
    {
        const string sql = @"
SELECT TableKind, TableName
  FROM CURdOCXTableSetUp WITH (NOLOCK)
 WHERE ItemId = @itemId;";
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@itemId", itemId);
        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            var kind = (rd["TableKind"]?.ToString() ?? "").Trim();
            var table = rd["TableName"]?.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(kind) || string.IsNullOrWhiteSpace(table)) continue;
            if (!map.ContainsKey(kind)) map[kind] = table;
        }
        return map;
    }

    private async Task<string?> LoadSystemIdAsync(SqlConnection conn, string itemId)
    {
        const string sql = "SELECT TOP 1 SystemId FROM CURdSysItems WITH (NOLOCK) WHERE ItemId = @itemId;";
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@itemId", itemId);
        var obj = await cmd.ExecuteScalarAsync();
        return obj == null || obj == DBNull.Value ? null : obj.ToString();
    }

    private async Task<object?> ResolveParamValueAsync(
        SqlConnection conn,
        Dictionary<string, string> tableMap,
        string paperNum,
        ButtonParamRow p,
        string? systemId,
        string userId,
        string useId)
    {
        var paramType = p.ParamType ?? 0;
        switch (paramType)
        {
            case 0:
            {
                var tableKind = (p.TableKind ?? "Master1").Trim();
                if (string.IsNullOrWhiteSpace(tableKind)) tableKind = "Master1";
                var tableName = ResolveTableName(tableMap, tableKind);
                if (string.IsNullOrWhiteSpace(tableName) || string.IsNullOrWhiteSpace(p.ParamFieldName)) return null;
                return await ReadFieldValueAsync(conn, tableName, p.ParamFieldName!, paperNum);
            }
            case 1:
                return p.ParamFieldName ?? string.Empty;
            case 2:
                return string.IsNullOrWhiteSpace(userId) ? null : userId;
            case 3:
                return useId;
            case 4:
                return systemId ?? string.Empty;
            case 5:
                return paperNum;
            default:
                return null;
        }
    }

    private static string? ResolveTableName(Dictionary<string, string> tableMap, string tableKind)
    {
        if (tableMap.TryGetValue(tableKind, out var name)) return name;

        var k = tableKind.Trim();
        if (k.StartsWith("Master", StringComparison.OrdinalIgnoreCase))
        {
            var master = tableMap.FirstOrDefault(x => x.Key.Contains("Master", StringComparison.OrdinalIgnoreCase));
            return string.IsNullOrWhiteSpace(master.Value) ? null : master.Value;
        }

        if (k.StartsWith("Detail", StringComparison.OrdinalIgnoreCase))
        {
            var digit = new string(k.SkipWhile(c => !char.IsDigit(c)).ToArray());
            if (!string.IsNullOrWhiteSpace(digit))
            {
                var match = tableMap.FirstOrDefault(x => x.Key.Equals($"Detail{digit}", StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrWhiteSpace(match.Value)) return match.Value;
            }
            var firstDetail = tableMap.FirstOrDefault(x => x.Key.StartsWith("Detail", StringComparison.OrdinalIgnoreCase));
            return string.IsNullOrWhiteSpace(firstDetail.Value) ? null : firstDetail.Value;
        }

        return null;
    }

    private async Task<object?> ReadFieldValueAsync(SqlConnection conn, string tableName, string fieldName, string paperNum)
    {
        var actualTable = await ResolveRealTableNameAsync(conn, tableName) ?? tableName;
        var safeTable = QuoteIdentifier(actualTable);
        var safeField = QuoteIdentifier(fieldName);
        if (string.IsNullOrWhiteSpace(safeTable) || string.IsNullOrWhiteSpace(safeField)) return null;

        var sql = $"SELECT TOP 1 {safeField} FROM {safeTable} WITH (NOLOCK) WHERE [PaperNum] = @paperNum";
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@paperNum", paperNum ?? string.Empty);
        var obj = await cmd.ExecuteScalarAsync();
        return obj == DBNull.Value ? null : obj;
    }

    private async Task<string?> ResolveRealTableNameAsync(SqlConnection conn, string dictTableName)
    {
        const string sql = @"
SELECT TOP 1 ISNULL(NULLIF(RealTableName,''), TableName) AS ActualName
  FROM CURdTableName WITH (NOLOCK)
 WHERE TableName = @tbl;";
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@tbl", dictTableName ?? string.Empty);
        var obj = await cmd.ExecuteScalarAsync();
        return obj == null || obj == DBNull.Value ? null : obj.ToString();
    }
}
