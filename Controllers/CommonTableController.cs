using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using System.Data;
using System.Data.Common;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Data.SqlClient; // ★ 指定 SQL Server 參數型別用

namespace PcbErpApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CommonTableController : ControllerBase
    {
        private readonly PcbErpContext _context;
        private readonly ILogger<CommonTableController> _logger;
        private readonly string _connStr; // ★ 新增

        public CommonTableController(PcbErpContext context, ILogger<CommonTableController> logger,IConfiguration config)
        {
            _context = context;
            _logger = logger;
                    // 優先用 appsettings 內的 DefaultConnection；沒有就向 EF 要
            _connStr = config.GetConnectionString("DefaultConnection")
                   ?? _context.Database.GetConnectionString()
                   ?? throw new InvalidOperationException("Missing connection string.");
        }

        public class SaveTableChangesRequest
        {
            public string TableName { get; set; } = "";
            public JsonArray Data { get; set; } = new();
            public List<string>? KeyFields { get; set; } // 可選提示鍵
        }

        [HttpPost]
        public async Task<IActionResult> SaveTableChanges([FromBody] SaveTableChangesRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.TableName))
                return BadRequest("TableName 不可為空");
            if (req.Data is null || req.Data.Count == 0)
                return BadRequest("沒有任何資料要更新");

            var conn = _context.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open) await conn.OpenAsync();
            using var tx = await conn.BeginTransactionAsync();

            try
            {
                var (columns, updatable, pkCols) = await LoadTableSchemaAsync(conn, tx, req.TableName);
                var uniqueIndexSets = await LoadUniqueIndexSetsAsync(conn, tx, req.TableName);

                var hintedKeys = new HashSet<string>(
                    (req.KeyFields ?? Enumerable.Empty<string>())
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Where(s => columns.ContainsKey(s)),
                    StringComparer.OrdinalIgnoreCase
                );

                var results = new List<object>();

                foreach (var node in req.Data)
                {
                    if (node is not JsonObject row) continue;

                    var rowColNames = new HashSet<string>(row.Select(kv => kv.Key), StringComparer.OrdinalIgnoreCase);
                    List<string>? keyNames = null;

                    if (hintedKeys.Count > 0 && hintedKeys.All(k => rowColNames.Contains(k)))
                        keyNames = hintedKeys.ToList();
                    if (keyNames is null && pkCols.Count > 0 && pkCols.All(k => rowColNames.Contains(k)))
                        keyNames = pkCols.ToList();
                    if (keyNames is null && uniqueIndexSets.Count > 0)
                    {
                        var hit = uniqueIndexSets.FirstOrDefault(set => set.All(k => rowColNames.Contains(k)));
                        if (hit != null) keyNames = hit.ToList();
                    }
                    if (keyNames is null || keyNames.Count == 0)
                    {
                        results.Add(new { ok = false, reason = "找不到可用的鍵欄位（請帶齊主鍵或唯一索引欄位）", row });
                        continue;
                    }

                    // __delete 標記支援刪除
                    bool isDelete = false;
                    if (row.TryGetPropertyValue("__delete", out var delNode))
                    {
                        if (delNode is JsonValue jv)
                        {
                            if (jv.TryGetValue(out bool b)) isDelete = b;
                            else if (jv.TryGetValue(out string? s))
                                isDelete = string.Equals(s, "true", StringComparison.OrdinalIgnoreCase) || s == "1";
                        }
                    }

                    // ★ setPairs 改成攜帶欄位資訊，方便之後決定參數型別
                    var setPairs = new List<(ColumnInfo Col, object? Value)>();
                    var keyPairs = new List<(string Name, object? Value)>();
                    var skipped  = new List<string>();

                    foreach (var (name, jv) in row)
                    {
                        if (!columns.ContainsKey(name)) continue;
                        var col = columns[name];

                        // === Binary / RowVersion 特別處理 ===
                        if (col.IsBinary || col.IsRowVersion)
                        {
                            var (shouldSet, isNull, bytes) = ConvertBinaryForUpdate(jv);
                            if (!shouldSet) { skipped.Add(col.Name); continue; }          // 空字串 → 跳過，不更新
                            setPairs.Add((col, isNull ? DBNull.Value : bytes));
                            continue;
                        }

                        // 其他型別
                        var val = ConvertJsonToDbValue(jv, col.DbType);

                        if (keyNames.Any(k => string.Equals(k, name, StringComparison.OrdinalIgnoreCase)))
                            keyPairs.Add((col.Name, val));
                        else if (updatable.Contains(col.Name, StringComparer.OrdinalIgnoreCase))
                            setPairs.Add((col, val));
                    }

                    if (keyPairs.Count != keyNames.Count || keyPairs.Any(k => IsNullOrEmptyDbValue(k.Value)))
                    {
                        results.Add(new { ok = false, reason = "鍵欄位不完整或為空", row, keys = keyNames });
                        continue;
                    }

                    if (isDelete)
                    {
                        var delWhereSql = string.Join(" AND ", keyPairs.Select(p => $"[{p.Name}] = @K_{p.Name}"));
                        var delSql = $"DELETE FROM [{req.TableName}] WHERE {delWhereSql}";
                        using var delCmd = conn.CreateCommand();
                        delCmd.Transaction = (System.Data.Common.DbTransaction)tx;
                        delCmd.CommandText = delSql;
                        foreach (var kp in keyPairs)
                        {
                            if (columns.TryGetValue(kp.Name, out var colInfo))
                                AddTypedParameter(delCmd, $"@K_{kp.Name}", kp.Value, colInfo);
                            else
                                AddTypedParameter(delCmd, $"@K_{kp.Name}", kp.Value, null);
                        }
                        var delAffected = await delCmd.ExecuteNonQueryAsync();
                        results.Add(new { ok = delAffected > 0, affected = delAffected, deleted = true, sql = delSql });
                        continue;
                    }

                    if (setPairs.Count == 0)
                    {
                        var insertSeen0 = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        var insertPairs0 = new List<(ColumnInfo Col, object? Value)>();
                        foreach (var kp in keyPairs)
                        {
                            if (columns.TryGetValue(kp.Name, out var col) && insertSeen0.Add(col.Name))
                                insertPairs0.Add((col, kp.Value));
                        }
                        if (insertPairs0.Count > 0)
                        {
                            var colsSql0 = string.Join(", ", insertPairs0.Select(p => $"[{p.Col.Name}]"));
                            var valsSql0 = string.Join(", ", insertPairs0.Select(p => $"@I_{p.Col.Name}"));
                            var insertSql0 = $"INSERT INTO [{req.TableName}] ({colsSql0}) VALUES ({valsSql0})";
                            using var insertCmd0 = conn.CreateCommand();
                            insertCmd0.Transaction = (System.Data.Common.DbTransaction)tx;
                            insertCmd0.CommandText = insertSql0;
                            foreach (var p in insertPairs0)
                                AddTypedParameter(insertCmd0, $"@I_{p.Col.Name}", p.Value, p.Col);
                            var insAffected = await insertCmd0.ExecuteNonQueryAsync();
                            results.Add(new { ok = insAffected > 0, affected = insAffected, sql = insertSql0, inserted = true, skipped });
                            continue;
                        }
                    }
                    if (setPairs.Count == 0)
                    {
                        results.Add(new { ok = true, affected = 0, skip = (skipped.Count > 0 ? $"略過欄位: {string.Join(",", skipped)}" : "無可更新欄位") });
                        continue;
                    }

                    var setSql   = string.Join(", ", setPairs.Select(p => $"[{p.Col.Name}] = @{p.Col.Name}"));
                    var whereSql = string.Join(" AND ", keyPairs.Select(p => $"[{p.Name}] = @K_{p.Name}"));
                    var sql      = $"UPDATE [{req.TableName}] SET {setSql} WHERE {whereSql}";

                    using var cmd = conn.CreateCommand();
                    cmd.Transaction = (System.Data.Common.DbTransaction)tx;
                    cmd.CommandText = sql;

                    // Set 參數（★ binary 指定 SqlDbType）
                    foreach (var p in setPairs)
                        AddTypedParameter(cmd, $"@{p.Col.Name}", p.Value, p.Col);

                    // Key 參數（★ 也需要根據欄位型別轉換）
                    foreach (var p in keyPairs)
                    {
                        if (columns.TryGetValue(p.Name, out var colInfo))
                            AddTypedParameter(cmd, $"@K_{p.Name}", p.Value, colInfo);
                        else
                            AddTypedParameter(cmd, $"@K_{p.Name}", p.Value, null);
                    }

                    var affected = await cmd.ExecuteNonQueryAsync();
                    // 若找不到資料，嘗試 INSERT 新增
                    if (affected == 0)
                    {
                        var insertSeen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        var insertPairs = new List<(ColumnInfo Col, object? Value)>();
                        foreach (var kp in keyPairs)
                        {
                            if (columns.TryGetValue(kp.Name, out var col) && insertSeen.Add(col.Name))
                                insertPairs.Add((col, kp.Value));
                        }
                        foreach (var sp in setPairs)
                        {
                            if (insertSeen.Add(sp.Col.Name))
                                insertPairs.Add(sp);
                        }

                        if (insertPairs.Count > 0)
                        {
                            var colsSql = string.Join(", ", insertPairs.Select(p => $"[{p.Col.Name}]"));
                            var valsSql = string.Join(", ", insertPairs.Select(p => $"@I_{p.Col.Name}"));
                            var insertSql = $"INSERT INTO [{req.TableName}] ({colsSql}) VALUES ({valsSql})";
                            using var insertCmd = conn.CreateCommand();
                            insertCmd.Transaction = (System.Data.Common.DbTransaction)tx;
                            insertCmd.CommandText = insertSql;
                            foreach (var p in insertPairs)
                                AddTypedParameter(insertCmd, $"@I_{p.Col.Name}", p.Value, p.Col);

                            affected = await insertCmd.ExecuteNonQueryAsync();
                            results.Add(new { ok = affected > 0, affected, sql = insertSql, inserted = true, skipped });
                            continue;
                        }
                    }

                    results.Add(new { ok = affected > 0, affected, sql, skipped });
                }

                await tx.CommitAsync();
                return Ok(new { success = true, results });
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _logger.LogError(ex, "SaveTableChanges 失敗");
                return StatusCode(500, $"更新失敗：{ex.Message}");
            }
        }

        // ===== Schema =====
        private sealed record ColumnInfo(string Name, string DbType, bool IsIdentity, bool IsComputed, bool IsRowVersion, bool IsBinary);

        private async Task<(Dictionary<string, ColumnInfo> columns, List<string> updatable, List<string> pkCols)>
        LoadTableSchemaAsync(DbConnection conn, IDbTransaction tx, string table)
        {
            var columns   = new Dictionary<string, ColumnInfo>(StringComparer.OrdinalIgnoreCase);
            var updatable = new List<string>();
            var pkCols    = new List<string>();

            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = (System.Data.Common.DbTransaction)tx;
                cmd.CommandText = @"
SELECT c.name AS ColName,
       t.name AS DbType,
       c.is_identity,
       c.is_computed,
       c.is_rowguidcol AS is_rowversion
FROM sys.columns c
JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.[object_id] = OBJECT_ID(@tbl)";
                var p = cmd.CreateParameter();
                p.ParameterName = "@tbl";
                p.Value = table;
                cmd.Parameters.Add(p);

                using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                {
                    var name = r.GetString(0);
                    var dbt  = r.GetString(1);
                    var isId = r.GetBoolean(2);
                    var isCmp= r.GetBoolean(3);
                    var isRv = r.GetBoolean(4);
                    var isBin = IsBinaryType(dbt);

                    var info = new ColumnInfo(name, dbt, isId, isCmp, isRv, isBin);
                    columns[name] = info;

                    // 白名單：排除 identity / computed / rowversion / binary / text(ntext)
                    if (!(isId || isCmp || isRv) && !isBin && !IsLargeTextType(dbt))
                        updatable.Add(name);
                }
            }

            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = (System.Data.Common.DbTransaction)tx;
                cmd.CommandText = @"
SELECT col.name
FROM sys.indexes i
JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
JOIN sys.columns col       ON col.object_id = ic.object_id AND col.column_id = ic.column_id
WHERE i.object_id = OBJECT_ID(@tbl) AND i.is_primary_key = 1
ORDER BY ic.key_ordinal";
                var p = cmd.CreateParameter();
                p.ParameterName = "@tbl";
                p.Value = table;
                cmd.Parameters.Add(p);

                using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync()) pkCols.Add(r.GetString(0));
            }

            return (columns, updatable, pkCols);
        }

        private async Task<List<List<string>>> LoadUniqueIndexSetsAsync(DbConnection conn, IDbTransaction tx, string table)
        {
            var result = new List<List<string>>();
            using var cmd = conn.CreateCommand();
            cmd.Transaction = (System.Data.Common.DbTransaction)tx;
            cmd.CommandText = @"
WITH idx AS (
    SELECT i.index_id, i.name
    FROM sys.indexes i
    WHERE i.object_id = OBJECT_ID(@tbl)
      AND i.is_unique = 1
)
SELECT idx.index_id, ic.key_ordinal, c.name AS ColName
FROM idx
JOIN sys.index_columns ic ON ic.object_id = OBJECT_ID(@tbl) AND ic.index_id = idx.index_id
JOIN sys.columns c        ON c.object_id  = ic.object_id     AND c.column_id = ic.column_id
ORDER BY idx.index_id, ic.key_ordinal";
            var p = cmd.CreateParameter();
            p.ParameterName = "@tbl";
            p.Value = table;
            cmd.Parameters.Add(p);

            using var r = await cmd.ExecuteReaderAsync();
            int? current = null;
            List<string>? bucket = null;
            while (await r.ReadAsync())
            {
                var id = r.GetInt32(0);
                var col = r.GetString(2);
                if (current != id)
                {
                    if (bucket != null && bucket.Count > 0) result.Add(bucket);
                    current = id;
                    bucket  = new List<string>();
                }
                bucket!.Add(col);
            }
            if (bucket != null && bucket.Count > 0) result.Add(bucket);

            return result;
        }

        // ===== 參數與轉型 =====

        // ★ 依欄位型別建立參數（binary 指定 SqlDbType；空白字串轉 NULL；根據資料庫型別轉換數值）
        private static void AddTypedParameter(DbCommand cmd, string name, object? value, ColumnInfo? col)
        {
            var normVal = NormalizeValue(value, col?.DbType);

            if (cmd is SqlCommand sc)
            {
                if (col != null && col.IsBinary)
                {
                    var p = sc.Parameters.Add(name, MapSqlDbType(col.DbType));
                    p.Value = normVal;
                    return;
                }
                // 其他型別走預設
                var sp = sc.Parameters.AddWithValue(name, normVal);
                return;
            }

            // 其他資料庫 provider：退回一般參數
            var prm = cmd.CreateParameter();
            prm.ParameterName = name;
            prm.Value = normVal;
            cmd.Parameters.Add(prm);
        }

        private static object NormalizeValue(object? value, string? dbType = null)
        {
            if (value == null || value == DBNull.Value) return DBNull.Value;

            if (value is string s)
            {
                // 空白字串一律轉 NULL
                if (string.IsNullOrWhiteSpace(s)) return DBNull.Value;

                // ★ 根據資料庫型別決定如何轉換
                if (!string.IsNullOrWhiteSpace(dbType))
                {
                    var t = dbType.ToLowerInvariant();

                    // 數字型別：嘗試轉換，失敗則回傳 NULL
                    if (t is "int" or "bigint" or "smallint" or "tinyint")
                    {
                        if (long.TryParse(s, out var l)) return l;
                        return DBNull.Value; // 無法轉換就回傳 NULL
                    }
                    if (t.Contains("decimal") || t.Contains("numeric") || t.Contains("money"))
                    {
                        if (decimal.TryParse(s, out var d)) return d;
                        return DBNull.Value;
                    }
                    if (t is "float" or "real")
                    {
                        if (double.TryParse(s, out var f)) return f;
                        return DBNull.Value;
                    }
                    if (t is "bit")
                    {
                        if (bool.TryParse(s, out var b)) return b;
                        if (s == "0") return false;
                        if (s == "1") return true;
                        return DBNull.Value;
                    }
                    if (t.Contains("date") || t.Contains("time"))
                    {
                        if (DateTime.TryParse(s, out var dt)) return dt;
                        return DBNull.Value;
                    }
                }

                // 字串型別或無法判斷型別：直接回傳
                return s;
            }

            return value;
        }

        private static SqlDbType MapSqlDbType(string dbType)
        {
            var t = dbType.ToLowerInvariant();
            return t switch
            {
                "image"     => SqlDbType.Image,
                "varbinary" => SqlDbType.VarBinary,
                "binary"    => SqlDbType.Binary,
                _           => SqlDbType.VarBinary // fallback
            };
        }

        private static object? ConvertJsonToDbValue(JsonNode? node, string dbType)
        {
            if (node is null) return DBNull.Value;
            if (node.GetValueKind() == JsonValueKind.Null) return DBNull.Value;

            var s = node.ToString();
            string t = dbType.ToLowerInvariant();

            if (t is "int" or "bigint" or "smallint" or "tinyint")
                return long.TryParse(s, out var l) ? l : (object?)DBNull.Value;
            if (t.Contains("decimal") || t.Contains("numeric") || t.Contains("money"))
                return decimal.TryParse(s, out var d) ? d : (object?)DBNull.Value;
            if (t is "float" or "real")
                return double.TryParse(s, out var f) ? f : (object?)DBNull.Value;
            if (t is "bit")
                return bool.TryParse(s, out var b) ? b : (object?)DBNull.Value;
            if (t.Contains("date") || t.Contains("time"))
                return DateTime.TryParse(s, out var dt) ? dt : (object?)DBNull.Value;

            return s; // 其他當字串
        }

        // ★ Binary 三態：Skip(空字串)、Null、Bytes
        private static (bool shouldSet, bool isNull, byte[]? bytes) ConvertBinaryForUpdate(JsonNode? node)
        {
            if (node is null || node.GetValueKind() == JsonValueKind.Null)
                return (true, true, null); // 寫入 NULL

            var s = node.ToString().Trim();
            if (string.IsNullOrEmpty(s))
                return (false, false, null); // 空字串 → 不更新

            // data URI
            if (s.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            {
                var comma = s.IndexOf(',');
                if (comma > 0) s = s[(comma + 1)..];
            }

            // 0xHEX
            if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var hex = s[2..];
                    var bytes = Enumerable.Range(0, hex.Length / 2)
                        .Select(i => Convert.ToByte(hex.Substring(i * 2, 2), 16))
                        .ToArray();
                    return (true, false, bytes);
                }
                catch { return (false, false, null); }
            }

            // Base64
            try
            {
                var bytes = Convert.FromBase64String(s);
                return (true, false, bytes);
            }
            catch
            {
                return (false, false, null); // 格式不識別 → 不更新
            }
        }

        private static bool IsNullOrEmptyDbValue(object? v)
            => v is null || v == DBNull.Value || (v is string s && string.IsNullOrWhiteSpace(s));

        private static bool IsBinaryType(string dbType)
        {
            var t = dbType.ToLowerInvariant();
            return t is "image" or "varbinary" or "binary" or "rowversion" or "timestamp";
        }

        private static bool IsLargeTextType(string dbType)
        {
            var t = dbType.ToLowerInvariant();
            return t is "text" or "ntext";
        }

       // 既有：TopRows(table, top, orderBy?, orderDir?)
    [HttpGet]
    public async Task<IActionResult> TopRows([FromQuery] string table, [FromQuery] int top = 100, [FromQuery] string? orderBy = null, [FromQuery] string? orderDir = "ASC")
    {
        if (string.IsNullOrWhiteSpace(table)) return BadRequest("table is required");

        var tblOk = await TableExistsAsync(table);
        if (!tblOk) return NotFound($"Table '{table}' not found.");

        string orderSql = "";
        if (!string.IsNullOrWhiteSpace(orderBy))
        {
            var colOk = await ColumnExistsAsync(table, orderBy);
            if (colOk)
                orderSql = $" ORDER BY [{orderBy}] {(string.Equals(orderDir, "DESC", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC")}";
        }

        var sql = $"SELECT TOP (@top) * FROM [{table}]{orderSql}";
        var pTop = new SqlParameter("@top", top);

        var dt = new DataTable();
        await using var conn = (SqlConnection)_context.Database.GetDbConnection();
        await conn.OpenAsync();
        await using (var cmd = new SqlCommand(sql, conn))
        {
            cmd.Parameters.Add(pTop);
            await using var rd = await cmd.ExecuteReaderAsync();
            dt.Load(rd);
        }
        return Ok(ToDictList(dt));
    }

    // 新增：ByKeys(table, keyNames[], keyValues[]) → 多個 = 條件 (AND)
    [HttpGet]
    public async Task<IActionResult> ByKeys([FromQuery] string table, [FromQuery] string[] keyNames, [FromQuery] string[] keyValues)
    {
        if (string.IsNullOrWhiteSpace(table)) return BadRequest("table is required");
        if (keyNames == null || keyValues == null || keyNames.Length == 0 || keyNames.Length != keyValues.Length)
            return BadRequest("keyNames and keyValues must be same length and not empty.");

        // 驗 table
        var tblOk = await TableExistsAsync(table);
        if (!tblOk) return NotFound($"Table '{table}' not found.");

        // 驗 column & 組條件
        var whereParts = new List<string>();
        var parameters = new List<SqlParameter>();
        for (int i = 0; i < keyNames.Length; i++)
        {
            var col = keyNames[i];
            var val = keyValues[i];

            var colOk = await ColumnExistsAsync(table, col);
            if (!colOk) return BadRequest($"Column '{col}' not found in '{table}'.");

            var p = new SqlParameter($"@p{i}", (object?)val ?? DBNull.Value);
            parameters.Add(p);
            whereParts.Add($"[{col}] = @p{i}");
        }

        var whereSql = string.Join(" AND ", whereParts);
        var sql = $"SELECT * FROM [{table}] WHERE {whereSql}";

        var dt = new DataTable();
        await using var conn = (SqlConnection)_context.Database.GetDbConnection();
        await conn.OpenAsync();
        await using (var cmd = new SqlCommand(sql, conn))
        {
            parameters.ForEach(p => cmd.Parameters.Add(p));
            await using var rd = await cmd.ExecuteReaderAsync();
            dt.Load(rd);
        }
        return Ok(ToDictList(dt));
    }

    private static List<Dictionary<string, object?>> ToDictList(DataTable dt)
    {
        var list = new List<Dictionary<string, object?>>(dt.Rows.Count);
        foreach (DataRow r in dt.Rows)
        {
            var d = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            foreach (DataColumn c in dt.Columns)
                d[c.ColumnName] = r.IsNull(c) ? null : r[c];
            list.Add(d);
        }
        return list;
    }

    private Task<bool> TableExistsAsync(string name)
    {
        const string sql = "SELECT 1 FROM sys.objects WHERE name = @n AND type IN ('U','V')";
        return ExecExistsAsync(sql, new SqlParameter("@n", name ?? string.Empty));
    }

    private Task<bool> ColumnExistsAsync(string table, string column)
    {
        const string sql = @"
SELECT 1
  FROM sys.columns c
  JOIN sys.objects o ON o.object_id = c.object_id AND o.type IN ('U','V')
 WHERE o.name = @t AND c.name = @c";
        return ExecExistsAsync(sql,
            new SqlParameter("@t", table ?? string.Empty),
            new SqlParameter("@c", column ?? string.Empty));
    }

    private async Task<bool> ExecExistsAsync(string sql, params SqlParameter[] ps)
    {
        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        if (ps != null) cmd.Parameters.AddRange(ps);
        var obj = await cmd.ExecuteScalarAsync();
        return obj != null && obj != DBNull.Value;
    }
    }
}
