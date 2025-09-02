using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
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
        _connStr = config.GetConnectionString("DefaultConnection");
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

    private static void GuardIdentifier(string name, TableMeta meta = null)
    {
        if (string.IsNullOrWhiteSpace(name) || !SafeIdent.IsMatch(name))
            throw new ArgumentException($"非法識別名稱：{name}");
        if (meta != null && !meta.Columns.Contains(name))
            throw new ArgumentException($"欄位不存在：{name}");
    }

    private static string GetMeta(Dictionary<string, object> body, string key)
    {
        if (!body.TryGetValue(key, out var v)) return null;
        body.Remove(key);
        if (v is JsonElement je && je.ValueKind == JsonValueKind.String) return je.GetString();
        return v?.ToString();
    }

    // ====== 儲存（單頭固定 PaperNum；單身固定 PaperNum + Item） ======
    [HttpPost("SaveOrderHeader")]
    public async Task<IActionResult> SaveOrderHeader([FromBody] Dictionary<string, object> body)
    {
        // 只收表名（關鍵欄位固定）
        string headerTable = GetMeta(body, "__headerTable");            // 必填
        string detailTable = GetMeta(body, "__detailTable");            // 可省略（不傳就不處理明細）

        if (string.IsNullOrWhiteSpace(headerTable))
            return BadRequest("__headerTable 必填");

        const string headerKey = "PaperNum";
        const string detailFk  = "PaperNum";
        const string detailPk  = "Item";   // int 自動遞增

        // 取出 Details（若有）
        List<Dictionary<string, object>> details = null;
        if (body.TryGetValue("Details", out var detailsObj) && detailsObj is JsonElement je && je.ValueKind == JsonValueKind.Array)
            details = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(je.GetRawText());
        body.Remove("Details");

        // 取單頭 PaperNum
        if (!body.TryGetValue(headerKey, out var keyObj))
            return BadRequest($"找不到主鍵欄位：{headerKey}");

        // 表結構
        GuardIdentifier(headerTable);
        var headerMeta = EnsureMeta(headerTable);
        GuardIdentifier(headerKey, headerMeta);
        var keyDbType = headerMeta.Types[headerKey];
        var keyValue  = ConvertJsonToDbType(keyObj, keyDbType);
        if (keyValue == null) return BadRequest("主鍵值不可為空");

        TableMeta detailMeta = null;
        if (!string.IsNullOrWhiteSpace(detailTable))
        {
            GuardIdentifier(detailTable);
            detailMeta = EnsureMeta(detailTable);
            GuardIdentifier(detailFk, detailMeta);
            GuardIdentifier(detailPk, detailMeta);
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
                var dbVal  = dbRow.TryGetValue(k, out var dv) ? (dv is DBNull ? null : dv) : null;

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

                    if (state == "added" || !currentSubs.ContainsKey(itemKey))
                    {
                        var cols = new List<string>();
                        var pars = new List<string>();
                        var cmd = new SqlCommand() { Connection = conn, Transaction = tran };

                        foreach (var kv in d)
                        {
                            if (!detailMeta.Types.ContainsKey(kv.Key)) continue;
                            var dbType = detailMeta.Types[kv.Key].ToLowerInvariant();
                            var val    = ConvertJsonToDbType(kv.Value, dbType);
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
                            var dbVal  = dbRowSub.TryGetValue(kv.Key, out var v) ? (v is DBNull ? null : v) : null;

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
                        latestMain[rd.GetName(i)] = rd.GetValue(i);
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
                        row[rd.GetName(i)] = rd.GetValue(i);
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
                JsonValueKind.True   => "true",
                JsonValueKind.False  => "false",
                _ => je.ToString()
            };
            return ParseStringForDbType(s, dbType);
        }

        if (newVal is string str) return ParseStringForDbType(str, dbType);
        return newVal; // 已是強型別（int/decimal/datetime…）
    }

    private object ParseStringForDbType(string s, string dbType)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
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
}
