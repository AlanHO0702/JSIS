using System.Data;
using Microsoft.Data.SqlClient;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using System.Collections.Concurrent;

[ApiController]
[Route("api/[controller]")]
public class OrderHeaderApiController : ControllerBase
{
    private readonly PcbErpContext _context;
    private readonly string _connStr;

    public OrderHeaderApiController(PcbErpContext context, IConfiguration config)
    {
        _context = context;
        _connStr = config.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    // ===== 通用表結構快取 =====
    private sealed class TableMeta
    {
        public Dictionary<string, string> Types { get; init; }      // 欄位 => 資料型別（INFORMATION_SCHEMA.DATA_TYPE）
        public Dictionary<string, bool> Nullable { get; init; }     // 欄位 => 是否可空
        public HashSet<string> Columns { get; init; }               // 欄位集合
    }

    private static readonly ConcurrentDictionary<string, TableMeta> _metaCache =
        new(StringComparer.OrdinalIgnoreCase);

    private static readonly Regex SafeIdent =
        new(@"^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.Compiled);

    private TableMeta EnsureMeta(string table)
    {
        if (string.IsNullOrWhiteSpace(table))
            throw new ArgumentException("表名不可為空");

        table = table.Trim();

        return _metaCache.GetOrAdd(table, t =>
        {
            var types = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var nullable = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            var cols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            using var conn = new SqlConnection(_connStr);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_NAME = @t";
            cmd.Parameters.AddWithValue("@t", t);
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                var col = rd.GetString(0);
                var typ = rd.GetString(1);
                var isNullable = rd.GetString(2);

                types[col] = typ;
                nullable[col] = isNullable.Equals("YES", StringComparison.OrdinalIgnoreCase);
                cols.Add(col);
            }

            if (cols.Count == 0)
                throw new ArgumentException($"表不存在或無欄位：{t}");

            return new TableMeta { Types = types, Nullable = nullable, Columns = cols };
        });
    }

    private static void GuardIdentifier(string name, TableMeta? meta = null)
    {
        if (string.IsNullOrWhiteSpace(name) || !SafeIdent.IsMatch(name))
            throw new ArgumentException($"非法識別名稱：{name}");
        if (meta != null && !meta.Columns.Contains(name))
            throw new ArgumentException($"欄位不存在：{name}");
    }

    private static string? GetMeta(Dictionary<string, object> body, string key)
    {
        if (!body.TryGetValue(key, out var v)) return null;
        body.Remove(key);
        if (v is JsonElement je && je.ValueKind == JsonValueKind.String) return je.GetString();
        return v?.ToString();
    }

    private async Task<string> ResolveRealTableNameAsync(string dictTable)
    {
        var table = (dictTable ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(table)) return table;

        var realTable = await _context.CurdTableNames
            .AsNoTracking()
            .Where(x => x.TableName.ToLower() == table.ToLower())
            .Select(x => string.IsNullOrWhiteSpace(x.RealTableName) ? x.TableName : x.RealTableName)
            .FirstOrDefaultAsync();

        return string.IsNullOrWhiteSpace(realTable) ? table : realTable.Trim();
    }

    private static string Esc(string id) => $"[{id.Replace("]", "]]")}]";

    private static string EscTableName(string raw)
    {
        var s = (raw ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(s)) throw new ArgumentException("table required");

        var parts = s.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var part in parts)
        {
            if (!SafeIdent.IsMatch(part))
                throw new ArgumentException($"非法識別名稱：{raw}");
        }
        return string.Join(".", parts.Select(Esc));
    }

    [HttpGet("GetHeaderRow")]
    public async Task<IActionResult> GetHeaderRow([FromQuery] string table, [FromQuery] string paperNum)
    {
        if (string.IsNullOrWhiteSpace(table))
            return BadRequest("table 必須指定");
        if (string.IsNullOrWhiteSpace(paperNum))
            return Ok(null);

        var resolved = await ResolveRealTableNameAsync(table);
        string safeTable;
        try
        {
            safeTable = EscTableName(resolved);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        await using (var cmd = new SqlCommand(
            $"SELECT TOP 1 * FROM {safeTable} WHERE [PaperNum]=@paperNum", conn))
        {
            cmd.Parameters.AddWithValue("@paperNum", paperNum);
            await using var rd = await cmd.ExecuteReaderAsync();
            if (await rd.ReadAsync())
            {
                for (int i = 0; i < rd.FieldCount; i++)
                {
                    var name = rd.GetName(i);
                    row[name] = rd.IsDBNull(i) ? null : rd.GetValue(i);
                }
            }
        }

        return row.Count == 0 ? Ok(null) : Ok(row);
    }

    // ====== 儲存（單頭固定 PaperNum；單身固定 PaperNum + Item） ======
    [HttpPost("SaveOrderHeader")]
    public async Task<IActionResult> SaveOrderHeader([FromBody] Dictionary<string, object> body)
    {
        // 只收表名（關鍵欄位固定）
        string? headerTable = GetMeta(body, "__headerTable");            // 必填
        string? detailTable = GetMeta(body, "__detailTable");            // 可省略（不傳就不處理明細）

        if (string.IsNullOrWhiteSpace(headerTable))
            return BadRequest("__headerTable 必填");

        const string headerKey = "PaperNum";
        const string detailFk = "PaperNum";
        const string detailPk = "Item";   // int 自動遞增

        headerTable = await ResolveRealTableNameAsync(headerTable);
        if (!string.IsNullOrWhiteSpace(detailTable))
            detailTable = await ResolveRealTableNameAsync(detailTable);

        // 取出 Details（若有）
        List<Dictionary<string, object>> details = new List<Dictionary<string, object>>();
        if (body.TryGetValue("Details", out var detailsObj) && detailsObj is JsonElement je && je.ValueKind == JsonValueKind.Array)
        {
            var deserialized = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(je.GetRawText());
            if (deserialized != null)
                details = deserialized;
        }
        body.Remove("Details");

        // 取單頭 PaperNum
        if (!body.TryGetValue(headerKey, out var keyObj))
            return BadRequest($"找不到主鍵欄位：{headerKey}");

        // 表結構
        GuardIdentifier(headerTable);
        var headerMeta = EnsureMeta(headerTable);
        GuardIdentifier(headerKey, headerMeta);
        var keyDbType = headerMeta.Types[headerKey];
        var keyValue = ConvertJsonToDbType(keyObj, keyDbType);
        if (keyValue == null) return BadRequest("主鍵值不可為空");

        TableMeta? detailMeta = null;
        if (!string.IsNullOrWhiteSpace(detailTable))
        {
            GuardIdentifier(detailTable);
            detailMeta = EnsureMeta(detailTable);
            GuardIdentifier(detailFk, detailMeta);
            GuardIdentifier(detailPk, detailMeta);
            Console.WriteLine($"DetailTable = {detailTable}");
            Console.WriteLine("Columns = " + string.Join(",", detailMeta.Columns));
        }

        using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();
        using var tran = conn.BeginTransaction();

        try
        {
            // 讀目前單頭
            var dbRow = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            using (var cmd = new SqlCommand(
                $"SELECT * FROM [{headerTable}] WHERE [{headerKey}]=@__key", conn, tran))
            {
                cmd.Parameters.Add(MakeTypedParam("@__key", keyValue, keyDbType));
                using var rd = await cmd.ExecuteReaderAsync();
                if (!await rd.ReadAsync())
                    return NotFound("找不到此單號");
                for (int i = 0; i < rd.FieldCount; i++)
                    dbRow[rd.GetName(i)] = rd.GetValue(i);
            }

            // 比對差異 → UPDATE 單頭
            var updateFields = new List<string>();
            var updateValues = new Dictionary<string, object>();
            foreach (var (k, v) in body)
            {
                if (k.Equals(headerKey, StringComparison.OrdinalIgnoreCase)) continue;
                if (!headerMeta.Types.ContainsKey(k)) continue;

                var dbType = headerMeta.Types[k].ToLowerInvariant();
                var newVal = ConvertJsonToDbType(v, dbType);
                var dbVal = dbRow.TryGetValue(k, out var dv) ? (dv is DBNull ? null : dv) : null;

                bool diff = (newVal, dbVal) switch
                {
                    (null, null) => false,
                    (null, _) or (_, null) => true,
                    (DateTime a, DateTime b) => a != b,
                    _ => !newVal.Equals(dbVal)
                };
                if (!diff) continue;
                if (newVal == null && !headerMeta.Nullable[k]) continue;

                updateFields.Add(k);
                updateValues[k] = newVal ?? DBNull.Value;
            }

            if (updateFields.Count > 0)
            {
                var setClause = string.Join(", ", updateFields.Select(f => $"[{f}]=@{f}"));
                using var u = new SqlCommand(
                    $"UPDATE [{headerTable}] SET {setClause} WHERE [{headerKey}]=@__key", conn, tran);
                foreach (var f in updateFields)
                    u.Parameters.Add(MakeTypedParam("@" + f, updateValues[f], headerMeta.Types[f]));
                u.Parameters.Add(MakeTypedParam("@__key", keyValue, keyDbType));
                await u.ExecuteNonQueryAsync();
            }

            // 明細 CRUD（可省略）
            if (details != null && details.Count > 0 && detailMeta != null)
            {
                // 讀取目前明細（用 Item 當 key）
                var currentSubs = new Dictionary<int, Dictionary<string, object>>();
                using (var cmd = new SqlCommand(
                    $"SELECT * FROM [{detailTable}] WHERE [{detailFk}]=@fk", conn, tran))
                {
                    cmd.Parameters.Add(MakeTypedParam("@fk", keyValue, keyDbType));
                    using var rd = await cmd.ExecuteReaderAsync();
                    while (await rd.ReadAsync())
                    {
                        var row = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                        for (int i = 0; i < rd.FieldCount; i++)
                            row[rd.GetName(i)] = rd.GetValue(i);
                        var item = Convert.ToInt32(row[detailPk]);
                        currentSubs[item] = row;
                    }
                }

                async Task<int> NextItemAsync()
                {
                    using var c = new SqlCommand(
                        $"SELECT ISNULL(MAX([{detailPk}]),0)+1 FROM [{detailTable}] WHERE [{detailFk}]=@fk",
                        conn, tran);
                    c.Parameters.Add(MakeTypedParam("@fk", keyValue, keyDbType));
                    var v = await c.ExecuteScalarAsync();
                    return Convert.ToInt32(v);
                }

                foreach (var d in details)
                {
                    var state = d.TryGetValue("__state", out var s) ? s?.ToString()?.ToLower() : "modified";
                    d.Remove("__state");

                    // 外鍵固定帶入
                    d[detailFk] = keyValue;

                    if (state == "deleted")
                    {
                        if (!d.TryGetValue(detailPk, out var pkObj)) continue;
                        var pk = ToInt(pkObj);
                        if (pk == null) continue;

                        using var del = new SqlCommand(
                            $"DELETE FROM [{detailTable}] WHERE [{detailFk}]=@fk AND [{detailPk}]=@pk",
                            conn, tran);
                        del.Parameters.Add(MakeTypedParam("@fk", keyValue, keyDbType));
                        del.Parameters.Add(MakeTypedParam("@pk", pk.Value, detailMeta.Types[detailPk]));
                        await del.ExecuteNonQueryAsync();
                        continue;
                    }

                    // 確保有 Item，沒有就自動遞增
                    int? pkMaybe = d.TryGetValue(detailPk, out var pkObj2) ? ToInt(pkObj2) : null;
                    if (pkMaybe is null || pkMaybe <= 0)
                    {
                        var newItem = await NextItemAsync();
                        d[detailPk] = newItem;
                        pkMaybe = newItem;
                    }
                    var itemKey = pkMaybe.Value;
                    var exists = currentSubs.ContainsKey(itemKey);

                    if (!exists)
                    {
                        var cols = new List<string>();
                        var pars = new List<string>();
                        var cmd = new SqlCommand() { Connection = conn, Transaction = tran };

                        foreach (var kv in d)
                        {
                            if (!detailMeta.Types.ContainsKey(kv.Key)) continue;
                            var dbType = detailMeta.Types[kv.Key].ToLowerInvariant();
                            var val = ConvertJsonToDbType(kv.Value, dbType);
                            if (val == null && !detailMeta.Nullable[kv.Key]) continue;

                            cols.Add($"[{kv.Key}]");
                            pars.Add("@" + kv.Key);
                            cmd.Parameters.Add(MakeTypedParam("@" + kv.Key, val ?? DBNull.Value, dbType));
                        }

                        if (cols.Count > 0)
                        {
                            cmd.CommandText =
                                $"INSERT INTO [{detailTable}]({string.Join(",", cols)}) VALUES({string.Join(",", pars)})";
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                    else
                    {
                        var dbRowSub = currentSubs[itemKey];
                        var uFields = new List<string>();
                        var cmd = new SqlCommand() { Connection = conn, Transaction = tran };

                        foreach (var kv in d)
                        {
                            if (!detailMeta.Types.ContainsKey(kv.Key)) continue;
                            if (kv.Key.Equals(detailFk, StringComparison.OrdinalIgnoreCase) ||
                                kv.Key.Equals(detailPk, StringComparison.OrdinalIgnoreCase)) continue;

                            var dbType = detailMeta.Types[kv.Key].ToLowerInvariant();
                            var newVal = ConvertJsonToDbType(kv.Value, dbType);
                            var dbVal = dbRowSub.TryGetValue(kv.Key, out var v) ? (v is DBNull ? null : v) : null;

                            bool diff = (newVal, dbVal) switch
                            {
                                (null, null) => false,
                                (null, _) or (_, null) => true,
                                (DateTime a, DateTime b) => a != b,
                                _ => !newVal.Equals(dbVal)
                            };
                            if (!diff) continue;
                            if (newVal == null && !detailMeta.Nullable[kv.Key]) continue;

                            uFields.Add($"[{kv.Key}]=@{kv.Key}");
                            cmd.Parameters.Add(MakeTypedParam("@" + kv.Key, newVal ?? DBNull.Value, dbType));
                        }

                        if (uFields.Count > 0)
                        {
                            cmd.CommandText =
                                $"UPDATE [{detailTable}] SET {string.Join(",", uFields)} WHERE [{detailFk}]=@fk AND [{detailPk}]=@pk";
                            cmd.Parameters.Add(MakeTypedParam("@fk", keyValue, keyDbType));
                            cmd.Parameters.Add(MakeTypedParam("@pk", itemKey, detailMeta.Types[detailPk]));
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }
            }

            // 回傳最新資料
            var latestMain = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            using (var cmd = new SqlCommand(
                $"SELECT * FROM [{headerTable}] WHERE [{headerKey}]=@__key", conn, tran))
            {
                cmd.Parameters.Add(MakeTypedParam("@__key", keyValue, keyDbType));
                using var rd = await cmd.ExecuteReaderAsync();
                if (await rd.ReadAsync())
                {
                    for (int i = 0; i < rd.FieldCount; i++)
                        latestMain[rd.GetName(i)] = rd.IsDBNull(i) ? null : rd.GetValue(i);
                }
            }

            var latestSubs = new List<Dictionary<string, object>>();
            if (!string.IsNullOrWhiteSpace(detailTable) && detailMeta != null)
            {
                using var cmd = new SqlCommand(
                    $"SELECT * FROM [{detailTable}] WHERE [{detailFk}]=@fk ORDER BY [{detailPk}]",
                    conn, tran);
                cmd.Parameters.Add(MakeTypedParam("@fk", keyValue, keyDbType));
                using var rd = await cmd.ExecuteReaderAsync();
                while (await rd.ReadAsync())
                {
                    var row = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                    for (int i = 0; i < rd.FieldCount; i++)
                        row[rd.GetName(i)] = rd.IsDBNull(i) ? null : rd.GetValue(i);
                    latestSubs.Add(row);
                }
            }

            tran.Commit();
            return Ok(new { updated = true, data = latestMain, details = latestSubs });
        }
        catch
        {
            tran.Rollback();
            throw;
        }
    }

    // ===== 參數型別綁定（保險再轉一次） =====
    private SqlParameter MakeTypedParam(string name, object value, string dbType)
    {
        dbType = (dbType ?? "").ToLowerInvariant();

        if (value is JsonElement je)
            value = ConvertJsonToDbType(je, dbType);

        var p = new SqlParameter(name, value ?? DBNull.Value);
        switch (dbType)
        {
            case "int":
            case "smallint":
            case "tinyint":
                p.SqlDbType = SqlDbType.Int; break;
            case "bigint":
                p.SqlDbType = SqlDbType.BigInt; break;
            case "decimal":
            case "numeric":
            case "money":
            case "smallmoney":
                p.SqlDbType = SqlDbType.Decimal; break; // 可視需要再設 Precision/Scale
            case "bit":
                p.SqlDbType = SqlDbType.Bit; break;
            default:
                if (dbType.Contains("date"))
                    p.SqlDbType = SqlDbType.DateTime;
                break;
        }
        return p;
    }

    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    private object ConvertJsonToDbType(object newVal, string dbType)
    {
        if (newVal == null) return null;
        dbType = (dbType ?? "").ToLowerInvariant();

        if (newVal is JsonElement je)
        {
            if (je.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined) return null;
            string s = je.ValueKind switch
            {
                JsonValueKind.String => je.GetString(),
                JsonValueKind.Number => je.ToString(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                _ => je.ToString()
            };
            return ParseStringForDbType(s, dbType);
        }

        if (newVal is string str) return ParseStringForDbType(str, dbType);
        return newVal; // 已是強型別（int/decimal/datetime…）
    }

    private static bool IsStringDbType(string dbType)
    {
        if (string.IsNullOrWhiteSpace(dbType)) return false;
        dbType = dbType.ToLowerInvariant();
        return dbType is "char" or "nchar" or "varchar" or "nvarchar" or "text" or "ntext";
    }

    private object ParseStringForDbType(string s, string dbType)
    {
        if (string.IsNullOrWhiteSpace(s))
            return IsStringDbType(dbType) ? "" : null;
        s = s.Trim().Replace(",", ""); // 去掉千分位

        switch (dbType)
        {
            case "int":
            case "smallint":
            case "tinyint":
                return int.TryParse(s, NumberStyles.Any, Inv, out var i) ? i : null;
            case "bigint":
                return long.TryParse(s, NumberStyles.Any, Inv, out var l) ? l : null;
            case "decimal":
            case "numeric":
            case "money":
            case "smallmoney":
                if (decimal.TryParse(s, NumberStyles.Any, Inv, out var d)) return d;
                if (decimal.TryParse(s, out d)) return d; // 退而求其次（本地文化）
                return null;
            case "bit":
                if (s == "1") return true;
                if (s == "0") return false;
                return bool.TryParse(s, out var b) ? b : null;
            default:
                if (dbType.Contains("date"))
                    return DateTime.TryParse(s, out var dt) ? dt : null;
                return s; // 文字型
        }
    }

    private int? ToInt(object v)
    {
        var o = ConvertJsonToDbType(v, "int");
        return o is int i ? i : (int?)null;
    }

    private static string UnwrapDefault(string raw)
    {
        var s = (raw ?? string.Empty).Trim();
        while (s.StartsWith("(", StringComparison.Ordinal) && s.EndsWith(")", StringComparison.Ordinal) && s.Length >= 2)
        {
            s = s.Substring(1, s.Length - 2).Trim();
        }
        return s;
    }

    private object? ConvertSqlDefault(string raw, string dbType)
    {
        var s = UnwrapDefault(raw);
        if (string.IsNullOrWhiteSpace(s)) return null;
        if (string.Equals(s, "null", StringComparison.OrdinalIgnoreCase)) return null;

        if (s.StartsWith("N'", StringComparison.OrdinalIgnoreCase) || s.StartsWith("'", StringComparison.Ordinal))
        {
            var t = s.StartsWith("N'", StringComparison.OrdinalIgnoreCase) ? s.Substring(1) : s;
            if (t.StartsWith("'", StringComparison.Ordinal) && t.EndsWith("'", StringComparison.Ordinal) && t.Length >= 2)
            {
                var inner = t.Substring(1, t.Length - 2).Replace("''", "'");
                return inner;
            }
            return t.Trim('\'');
        }

        if (string.Equals(s, "getdate()", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(s, "sysdatetime()", StringComparison.OrdinalIgnoreCase))
            return DateTime.Now;
        if (string.Equals(s, "getutcdate()", StringComparison.OrdinalIgnoreCase))
            return DateTime.UtcNow;
        if (string.Equals(s, "newid()", StringComparison.OrdinalIgnoreCase))
            return Guid.NewGuid().ToString();

        return ParseStringForDbType(s, dbType);
    }

    private static object DefaultForType(string dbType)
    {
        var t = (dbType ?? "").ToLowerInvariant();
        return t switch
        {
            "int" or "smallint" or "tinyint" or "bigint" => 0,
            "decimal" or "numeric" or "money" or "smallmoney" or "float" or "real" => 0,
            "bit" => 0,
            var x when x.Contains("date") => DateTime.Now,
            _ => string.Empty
        };
    }

    public class AddDetailRowReq
    {
        public string DetailTable { get; set; } // ex: "SpodOrderSub"
        public string PaperNum { get; set; }    // 目前單號
    }

    private static readonly Regex _safeName = new(@"^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.Compiled);
    private static string SafeTable(string t)
    {
        if (string.IsNullOrWhiteSpace(t) || !_safeName.IsMatch(t))
            throw new ArgumentException("illegal table name");
        return t;
    }

    // 取表欄位型別/可空性（簡易快取）
    private static readonly Dictionary<string, (Dictionary<string, string> types, Dictionary<string, bool> nullable)> _tableMeta
        = new(StringComparer.OrdinalIgnoreCase);

    private (Dictionary<string, string> types, Dictionary<string, bool> nullable) GetTableMeta(string table)
    {
        if (_tableMeta.TryGetValue(table, out var m)) return m;

        var types = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var nullable = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        using var conn = new SqlConnection(_connStr);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_NAME = @t";
        cmd.Parameters.AddWithValue("@t", table);

        using var rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            types[rd.GetString(0)] = rd.GetString(1);
            nullable[rd.GetString(0)] = rd.GetString(2).Equals("YES", StringComparison.OrdinalIgnoreCase);
        }

        _tableMeta[table] = (types, nullable);
        return (types, nullable);
    }

    private static readonly Dictionary<string, (Dictionary<string, string> types, Dictionary<string, bool> nullable, Dictionary<string, string?> defaults)> _tableMetaWithDefaults
        = new(StringComparer.OrdinalIgnoreCase);

    private (Dictionary<string, string> types, Dictionary<string, bool> nullable, Dictionary<string, string?> defaults) GetTableMetaWithDefaults(string table)
    {
        if (_tableMetaWithDefaults.TryGetValue(table, out var m)) return m;

        var types = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var nullable = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        var defaults = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        using var conn = new SqlConnection(_connStr);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, COLUMN_DEFAULT
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_NAME = @t";
        cmd.Parameters.AddWithValue("@t", table);

        using var rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            var col = rd.GetString(0);
            types[col] = rd.GetString(1);
            nullable[col] = rd.GetString(2).Equals("YES", StringComparison.OrdinalIgnoreCase);
            defaults[col] = rd.IsDBNull(3) ? null : rd.GetString(3);
        }

        _tableMetaWithDefaults[table] = (types, nullable, defaults);
        return (types, nullable, defaults);
    }

    [HttpGet("DetailDefaults")]
    public IActionResult DetailDefaults([FromQuery] string table)
    {
        if (string.IsNullOrWhiteSpace(table))
            return BadRequest("table required");

        var t = SafeTable(table.Trim());
        var (types, nullable, defaultsRaw) = GetTableMetaWithDefaults(t);

        var tableDefaults = new Dictionary<string, Dictionary<string, object>>(StringComparer.OrdinalIgnoreCase)
        {
            ["AJNdJourSub"] = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                ["AccId"] = "1101",
                ["SubAccId"] = "01",
                ["IsD"] = "1"
            },
            ["SPOdOrderSub"] = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                ["PartNum"] = "----",
                ["qnty"] = 0,
                ["UnitPrice"] = 0,
                ["SubTotal"] = 0
            }
        };

        var result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var kv in types)
        {
            var col = kv.Key;
            if (col.Equals("PaperNum", StringComparison.OrdinalIgnoreCase) ||
                col.Equals("Item", StringComparison.OrdinalIgnoreCase))
                continue;

            if (defaultsRaw.TryGetValue(col, out var raw) && !string.IsNullOrWhiteSpace(raw))
            {
                result[col] = ConvertSqlDefault(raw, kv.Value);
                continue;
            }

            if (!nullable.TryGetValue(col, out var isNullable) || !isNullable)
            {
                result[col] = DefaultForType(kv.Value);
            }
        }

        if (tableDefaults.TryGetValue(t, out var cfg))
        {
            foreach (var kv in cfg)
            {
                if (!types.TryGetValue(kv.Key, out var dbType)) continue;
                result[kv.Key] = ConvertJsonToDbType(kv.Value, dbType);
            }
        }

        return Ok(new { ok = true, defaults = result });
    }

    [HttpPost("AddDetailRow")]
    public async Task<IActionResult> AddDetailRow([FromBody] AddDetailRowReq req)
    {
        if (req == null || string.IsNullOrWhiteSpace(req.PaperNum))
            return BadRequest("paperNum required");
        if (string.IsNullOrWhiteSpace(req.DetailTable))
            return BadRequest("detailTable required");

        var table = SafeTable(req.DetailTable.Trim());
        var (types, nullable) = GetTableMeta(table);

        if (!types.ContainsKey("PaperNum") || !types.ContainsKey("Item"))
            return BadRequest("Detail table must have PaperNum & Item");

        // 🧩 1️⃣ 預設值設定區（依表名設定）
        var tableDefaults = new Dictionary<string, Dictionary<string, object>>(StringComparer.OrdinalIgnoreCase)
        {
            // 🔹 傳票明細
            ["AJNdJourSub"] = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                ["AccId"] = "1101",       // 預設借方科目
                ["SubAccId"] = "01",       // 預設借方科目
                ["IsD"] = "1"          // 借貸別：1=借方, 2=貸方

            },

            // 🔹 銷貨單明細
            ["SPOdOrderSub"] = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                ["PartNum"] = "----",
                ["qnty"] = 0,
                ["UnitPrice"] = 0,
                ["SubTotal"] = 0
            }
        };

        // 取當前表的自訂預設設定（若無則空字典）
        var defaults = tableDefaults.TryGetValue(table, out var cfg)
            ? cfg
            : new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();
        using var tran = conn.BeginTransaction();

        try
        {
            // 2️⃣ 取下一個項次
            int nextItem;
            using (var c = new SqlCommand(
                $@"SELECT ISNULL(MAX([Item]),0)+1 FROM [{table}] WHERE [PaperNum]=@PaperNum",
                conn, tran))
            {
                c.Parameters.Add(MakeTypedParam("@PaperNum", req.PaperNum, types["PaperNum"]));
                var v = await c.ExecuteScalarAsync();
                nextItem = Convert.ToInt32(v);
            }

            // 3️⃣ 動態組 INSERT 欄位
            var cols = new List<string> { "PaperNum", "Item" };
            var pars = new List<string> { "@PaperNum", "@Item" };
            using var ins = new SqlCommand() { Connection = conn, Transaction = tran };
            ins.Parameters.Add(MakeTypedParam("@PaperNum", req.PaperNum, types["PaperNum"]));
            ins.Parameters.Add(MakeTypedParam("@Item", nextItem, types["Item"]));

            // 4️⃣ 自動補 NOT NULL 欄位的預設值
            foreach (var col in types.Keys)
            {
                if (cols.Contains(col, StringComparer.OrdinalIgnoreCase)) continue;
                if (!defaults.ContainsKey(col) && nullable[col]) continue;

                object val = null;

                // 1️⃣ 優先使用自訂預設表中的設定值
                if (defaults.TryGetValue(col, out var preset))
                    val = ConvertJsonToDbType(preset, types[col]);

                // 2️⃣ 若沒有自訂設定則依型別給通用預設值
                val ??= types[col].ToLowerInvariant() switch
                {
                    "int" or "smallint" or "tinyint" or "bigint" => 0,
                    "decimal" or "numeric" or "money" or "smallmoney" => 0m,
                    "bit" => false,
                    var t when t.Contains("date") => DateTime.Now,
                    _ => "----"
                };

                cols.Add(col);
                pars.Add("@" + col);
                ins.Parameters.Add(MakeTypedParam("@" + col, val, types[col]));
            }

            ins.CommandText = $"INSERT INTO [{table}]({string.Join(",", cols.Select(c => $"[{c}]"))}) VALUES({string.Join(",", pars)})";
            await ins.ExecuteNonQueryAsync();

            // 5️⃣ 抓回整列
            var row = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            using (var s = new SqlCommand(
                $@"SELECT * FROM [{table}] WHERE [PaperNum]=@PaperNum AND [Item]=@Item",
                conn, tran))
            {
                s.Parameters.Add(MakeTypedParam("@PaperNum", req.PaperNum, types["PaperNum"]));
                s.Parameters.Add(MakeTypedParam("@Item", nextItem, types["Item"]));
                using var rd = await s.ExecuteReaderAsync();
                if (await rd.ReadAsync())
                {
                    for (int i = 0; i < rd.FieldCount; i++)
                        row[rd.GetName(i)] = rd.GetValue(i);
                }
            }

            tran.Commit();
            return Ok(new { ok = true, row });
        }
        catch (Exception ex)
        {
            tran.Rollback();
            return BadRequest(new { ok = false, error = ex.Message });
        }
    }


    
    // DELETE /api/OrderDetailApi/DeleteRow?table=SpodOrderSub&paperNum=...&item=...
    [HttpDelete("DeleteRow")]
    public async Task<IActionResult> DeleteRow([FromQuery] string table, [FromQuery] string paperNum, [FromQuery] int item)
    {
        if (string.IsNullOrWhiteSpace(table) || string.IsNullOrWhiteSpace(paperNum))
            return BadRequest("table/paperNum 不可為空");

        using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();
        var sql = $"DELETE FROM {table} WHERE PaperNum=@PaperNum AND Item=@Item";
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@PaperNum", paperNum);
        cmd.Parameters.AddWithValue("@Item", item);
        var n = await cmd.ExecuteNonQueryAsync();
        return Ok(new { deleted = n });
    }


}
