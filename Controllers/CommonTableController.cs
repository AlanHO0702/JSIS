using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Data.SqlClient; // ???? SQL Server ????

namespace PcbErpApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CommonTableController : ControllerBase
    {
        private readonly PcbErpContext _context;
        private readonly ILogger<CommonTableController> _logger;
        private readonly string _connStr; // ???啣?

        public CommonTableController(PcbErpContext context, ILogger<CommonTableController> logger,IConfiguration config)
        {
            _context = context;
            _logger = logger;
                    // ?芸???appsettings ?抒? DefaultConnection嚗??停??EF 閬?
            _connStr = config.GetConnectionString("DefaultConnection")
                   ?? _context.Database.GetConnectionString()
                   ?? throw new InvalidOperationException("Missing connection string.");
        }

        public class SaveTableChangesRequest
        {
            public string TableName { get; set; } = "";
            public JsonArray Data { get; set; } = new();
            public List<string>? KeyFields { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> SaveTableChanges([FromBody] SaveTableChangesRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.TableName))
                return BadRequest("TableName is required.");
            if (req.Data is null || req.Data.Count == 0)
                return BadRequest("No data to update.");
            var actualTable = await ResolveActualTableNameAsync(req.TableName);

            var conn = _context.Database.GetDbConnection()
                      ?? throw new InvalidOperationException("DbConnection ?芸?憪?");
            if (conn.State != ConnectionState.Open) await conn.OpenAsync();
            var txRaw = await conn.BeginTransactionAsync();
            if (txRaw == null) throw new InvalidOperationException("Transaction ?⊥?撱箇?");
            using var tx = txRaw;

            try
            {
                var (columns, updatable, pkCols) = await LoadTableSchemaAsync(conn, tx, actualTable);
                var uniqueIndexSets = await LoadUniqueIndexSetsAsync(conn, tx, actualTable);

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
                        results.Add(new { ok = false, reason = "No valid key fields found for update.", row });
                        continue;
                    }

                    // __delete 璅??舀?芷
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

                    // 分類欄位到 keyPairs / setPairs
                    var setPairs = new List<(ColumnInfo Col, object? Value)>();
                    var keyPairs = new List<(ColumnInfo Col, object? Value)>();
                    var skipped  = new List<string>();

                    // ★ 解析 __originalKeys（前端修改了 key 欄位時會帶原始值）
                    Dictionary<string, object?>? originalKeyValues = null;
                    if (row.TryGetPropertyValue("__originalKeys", out var okNode) && okNode is JsonObject okObj)
                    {
                        originalKeyValues = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                        foreach (var (okName, okVal) in okObj)
                        {
                            if (columns.ContainsKey(okName))
                                originalKeyValues[okName] = ConvertJsonToDbValue(okVal, columns[okName].DbType);
                        }
                    }

                    foreach (var (name, jv) in row)
                    {
                        if (name == "__originalKeys") continue;
                        if (!columns.ContainsKey(name)) continue;
                        var col = columns[name];

                        // === Binary / RowVersion 特殊處理 ===
                        if (col.IsBinary || col.IsRowVersion)
                        {
                            var (shouldSet, isNull, bytes) = ConvertBinaryForUpdate(jv);
                            if (!shouldSet) { skipped.Add(col.Name); continue; }
                            setPairs.Add((col, isNull ? DBNull.Value : bytes));
                            continue;
                        }

                        var val = ConvertJsonToDbValue(jv, col.DbType);

                        if (keyNames.Any(k => string.Equals(k, name, StringComparison.OrdinalIgnoreCase)))
                        {
                            // ★ 如果有 __originalKeys，用原始值做 WHERE，新值放 SET
                            if (originalKeyValues != null && originalKeyValues.TryGetValue(name, out var origVal))
                            {
                                keyPairs.Add((col, origVal));   // 原始值 → WHERE
                                setPairs.Add((col, val));       // 新值 → SET
                            }
                            else
                            {
                                keyPairs.Add((col, val));
                            }
                        }
                        else if (updatable.Contains(col.Name, StringComparer.OrdinalIgnoreCase))
                            setPairs.Add((col, val));
                    }
                    if (keyPairs.Count != keyNames.Count || keyPairs.Any(k => IsNullOrEmptyDbValue(k.Value)))
                    {
                        results.Add(new { ok = false, reason = "Key fields missing or empty.", row, keys = keyNames });
                        continue;
                    }

                    if (isDelete)
                    {
                        var delWhereSql = string.Join(" AND ", keyPairs.Select(p => $"[{p.Col.Name}] = @K_{p.Col.Name}"));
                        var delSql = $"DELETE FROM [{actualTable}] WHERE {delWhereSql}";
                        using var delCmd = conn.CreateCommand();
                        delCmd.Transaction = (System.Data.Common.DbTransaction)tx;
                        delCmd.CommandText = delSql;
                        foreach (var kp in keyPairs)
                        {
                            AddTypedParameter(delCmd, $"@K_{kp.Col.Name}", kp.Value, kp.Col);
                        }
                        var delAffected = await delCmd.ExecuteNonQueryAsync();
                        results.Add(new { ok = delAffected > 0, affected = delAffected, deleted = true, sql = delSql });
                        continue;
                    }

                    if (setPairs.Count == 0)
                    {
                        // ★ 先檢查資料是否已存在，避免既有資料被重複 INSERT
                        var checkWhereSql0 = string.Join(" AND ", keyPairs.Select(p => $"[{p.Col.Name}] = @K_{p.Col.Name}"));
                        var checkSql0 = $"SELECT COUNT(*) FROM [{actualTable}] WHERE {checkWhereSql0}";
                        using var checkCmd0 = conn.CreateCommand();
                        checkCmd0.Transaction = (System.Data.Common.DbTransaction)tx;
                        checkCmd0.CommandText = checkSql0;
                        foreach (var kp in keyPairs)
                            AddTypedParameter(checkCmd0, $"@K_{kp.Col.Name}", kp.Value, kp.Col);
                        var existsCount = (int)(await checkCmd0.ExecuteScalarAsync() ?? 0);
                        if (existsCount > 0)
                        {
                            // 資料已存在且沒有可更新的欄位 → 跳過（不新增重複資料）
                            results.Add(new { ok = true, affected = 0, skip = "Row already exists, no updatable changes" });
                            continue;
                        }

                        var insertSeen0 = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        var insertPairs0 = new List<(ColumnInfo Col, object? Value)>();
                        foreach (var kp in keyPairs)
                        {
                            if (insertSeen0.Add(kp.Col.Name))
                                insertPairs0.Add((kp.Col, kp.Value));
                        }
                        if (insertPairs0.Count > 0)
                        {
                            var colsSql0 = string.Join(", ", insertPairs0.Select(p => $"[{p.Col.Name}]"));
                            var valsSql0 = string.Join(", ", insertPairs0.Select(p => $"@I_{p.Col.Name}"));
                            var insertSql0 = $"INSERT INTO [{actualTable}] ({colsSql0}) VALUES ({valsSql0})";
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
                        results.Add(new { ok = true, affected = 0, skip = (skipped.Count > 0 ? $"Skipped: {string.Join(",", skipped)}" : "No changes to apply") });
                        continue;
                    }

                    var setSql   = string.Join(", ", setPairs.Select(p => $"[{p.Col.Name}] = @{p.Col.Name}"));
                    var whereSql = string.Join(" AND ", keyPairs.Select(p => $"[{p.Col.Name}] = @K_{p.Col.Name}"));

                    // ★ 先檢查資料是否已存在，避免對不存在的資料執行 UPDATE 而誤觸空的 UPDATE trigger
                    bool rowExists;
                    {
                        var existsSql = $"SELECT COUNT(*) FROM [{actualTable}] WHERE {whereSql}";
                        using var existsCmd = conn.CreateCommand();
                        existsCmd.Transaction = (System.Data.Common.DbTransaction)tx;
                        existsCmd.CommandText = existsSql;
                        foreach (var kp in keyPairs)
                            AddTypedParameter(existsCmd, $"@K_{kp.Col.Name}", kp.Value, kp.Col);
                        rowExists = (int)(await existsCmd.ExecuteScalarAsync() ?? 0) > 0;
                    }

                    var affected = 0;
                    var sql = $"UPDATE [{actualTable}] SET {setSql} WHERE {whereSql}";

                    if (rowExists)
                    {
                        using var cmd = conn.CreateCommand();
                        cmd.Transaction = (System.Data.Common.DbTransaction)tx;
                        cmd.CommandText = sql;

                        foreach (var p in setPairs)
                            AddTypedParameter(cmd, $"@{p.Col.Name}", p.Value, p.Col);
                        foreach (var p in keyPairs)
                            AddTypedParameter(cmd, $"@K_{p.Col.Name}", p.Value, p.Col);

                        affected = await cmd.ExecuteNonQueryAsync();
                    }

                    // 資料不存在 → 直接 INSERT（不經過 UPDATE，避免觸發空的 UPDATE trigger）
                    if (!rowExists)
                    {
                        var insertSeen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        var insertPairs = new List<(ColumnInfo Col, object? Value)>();
                        foreach (var kp in keyPairs)
                        {
                            if (insertSeen.Add(kp.Col.Name))
                                insertPairs.Add((kp.Col, kp.Value));
                        }
                        foreach (var sp in setPairs)
                        {
                            if (!insertSeen.Add(sp.Col.Name)) continue;
                            var isKey = keyNames?.Contains(sp.Col.Name, StringComparer.OrdinalIgnoreCase) ?? false;
                            if (!isKey && (sp.Value == null || sp.Value == DBNull.Value || (sp.Value is string s && string.IsNullOrEmpty(s))))
                                continue;
                            insertPairs.Add(sp);
                        }

                        if (insertPairs.Count > 0)
                        {
                            var colsSql = string.Join(", ", insertPairs.Select(p => $"[{p.Col.Name}]"));
                            var valsSql = string.Join(", ", insertPairs.Select(p => $"@I_{p.Col.Name}"));
                            var insertSql = $"INSERT INTO [{actualTable}] ({colsSql}) VALUES ({valsSql})";
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
                _logger.LogError(ex, "SaveTableChanges failed");

                // 從 SqlException 取第一個錯誤訊息（通常是 RAISERROR 自訂的內容），
                // 過濾掉系統自動附加的「交易在觸發程序中結束。已中止批次。」等訊息
                var userMsg = ex.Message;
                if (ex is SqlException sqlEx && sqlEx.Errors.Count > 0)
                {
                    var customErrors = sqlEx.Errors
                        .Cast<SqlError>()
                        .Where(e => e.Class >= 11 && e.Class <= 16)
                        .Select(e => e.Message)
                        .Where(m => !m.Contains("交易在觸發程序中結束"))
                        .ToList();
                    if (customErrors.Count > 0)
                        userMsg = string.Join("\n", customErrors);
                }

                return StatusCode(500, new { success = false, message = userMsg });
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

                    // ?賢??殷?? identity / computed / rowversion / binary / text(ntext)
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

        // ===== ?????=====

        // ??靘?雿??亙遣蝡??賂?binary ?? SqlDbType嚗征?賢?銝脰? NULL嚗???澈?頧??詨潘?
        private static void AddTypedParameter(DbCommand cmd, string name, object? value, ColumnInfo? col)
        {
            var dbType = col?.DbType ?? string.Empty;
            var normVal = col != null
                ? ConvertValueByDbType(value, dbType)
                : NormalizeValue(value);

            if (cmd is SqlCommand sc)
            {
                if (col != null && col.IsBinary)
                {
                    var p = sc.Parameters.Add(name, MapSqlDbType(dbType));
                    p.Value = normVal;
                    return;
                }
                // ?嗡??韏圈?閮?
                var sp = sc.Parameters.AddWithValue(name, normVal ?? DBNull.Value);
                return;
            }

            // ?嗡?鞈?摨?provider嚗???砍???
            var prm = cmd.CreateParameter();
            prm.ParameterName = name;
            prm.Value = normVal ?? DBNull.Value;
            cmd.Parameters.Add(prm);
        }

        private static object NormalizeValue(object? value, string? dbType = null)
        {
            if (value == null || value == DBNull.Value) return DBNull.Value;

            if (value is string s)
            {
                return string.IsNullOrWhiteSpace(s) ? DBNull.Value : s;
            }

            return value;
        }

        private static SqlDbType MapSqlDbType(string dbType)
        {
            if (string.IsNullOrWhiteSpace(dbType)) return SqlDbType.VarBinary;
            var t = dbType.ToLowerInvariant();
            return t switch
            {
                "image"     => SqlDbType.Image,
                "varbinary" => SqlDbType.VarBinary,
                "binary"    => SqlDbType.Binary,
                _           => SqlDbType.VarBinary // fallback
            };
        }

        private static readonly Dictionary<string, string> AliasBaseTypeMap = new(StringComparer.OrdinalIgnoreCase)
        {
            // char / varchar family
            ["caccid"] = "char",
            ["caccountid"] = "varchar",
            ["ccompanyid"] = "varchar",
            ["ccustpartnum"] = "varchar",
            ["cdefectid"] = "char",
            ["cdepartid"] = "char",
            ["cempid"] = "char",
            ["cfilmid"] = "char",
            ["charempid"] = "char",
            ["chrempid"] = "char",
            ["chrmattcode"] = "char",
            ["chrmattgauge"] = "varchar",
            ["chrmattname"] = "varchar",
            ["chrpartnum"] = "char",
            ["chrrevision"] = "char",
            ["clayerid"] = "char",
            ["clotnum"] = "varchar",
            ["cmid"] = "char",
            ["cname"] = "varchar",
            ["cnotes"] = "varchar",
            ["cpaperid"] = "varchar",
            ["cpapernum"] = "varchar",
            ["cpartnum"] = "char",
            ["cproccode"] = "char",
            ["cprojectid"] = "varchar",
            ["crevision"] = "char",
            ["csid"] = "char",
            ["csubaccid"] = "varchar",
            ["cuserid"] = "varchar",
            ["cvaccid"] = "varchar",
            ["cvaccountid"] = "varchar",
            ["cvcompanyid"] = "varchar",
            ["cvcustpartnum"] = "varchar",
            ["cvdecqnty"] = "decimal",
            ["cvdecmoney"] = "decimal",
            ["cvdefectid"] = "varchar",
            ["cvdepartid"] = "varchar",
            ["cvempid"] = "varchar",
            ["cvlayerid"] = "varchar",
            ["cvlotnum"] = "varchar",
            ["cvmid"] = "varchar",
            ["cvmotherissuenum"] = "varchar",
            ["cvname"] = "varchar",
            ["cvnotes"] = "varchar",
            ["cvpaperid"] = "varchar",
            ["cvpapernum"] = "varchar",
            ["cvpartnum"] = "varchar",
            ["cvpartnum120"] = "varchar",
            ["cvproccode"] = "varchar",
            ["cvprojectid"] = "varchar",
            ["cvrevision"] = "varchar",
            ["cvsid"] = "varchar",
            ["cvsubaccid"] = "varchar",
            ["cvuseid"] = "varchar",
            ["cvuserid"] = "varchar",
        };

        private static string ResolveDbBaseType(string dbType)
        {
            if (string.IsNullOrWhiteSpace(dbType)) return string.Empty;
            var key = dbType.Trim().ToLowerInvariant();
            return AliasBaseTypeMap.TryGetValue(key, out var baseType) ? baseType : key;
        }

        private static object? ConvertJsonToDbValue(JsonNode? node, string dbType)
        {
            if (node is null) return DBNull.Value;
            if (node.GetValueKind() == JsonValueKind.Null) return DBNull.Value;

            var s = node.ToString();
            string t = ResolveDbBaseType(dbType);

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

            return s; // ?嗡??嗅?銝?
        }

        // ??Binary 銝?嚗kip(蝛箏?銝??ull?ytes
        private static (bool shouldSet, bool isNull, byte[]? bytes) ConvertBinaryForUpdate(JsonNode? node)
        {
            if (node is null || node.GetValueKind() == JsonValueKind.Null)
                return (true, true, null); // 撖怠 NULL

            var s = node.ToString().Trim();
            if (string.IsNullOrEmpty(s))
                return (false, false, null); // 蝛箏?銝???銝??

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
                return (false, false, null); // ?澆?銝?????銝??
            }
        }

        private static bool IsNullOrEmptyDbValue(object? v)
            => v is null || v == DBNull.Value || (v is string s && string.IsNullOrWhiteSpace(s));

        private static bool IsBinaryType(string dbType)
        {
            var t = dbType.ToLowerInvariant();
            return t is "image" or "varbinary" or "binary" or "rowversion" or "timestamp";
        }

        private static object? ConvertValueByDbType(object? value, string dbType)
        {
            if (value == null || value == DBNull.Value) return DBNull.Value;
            var s = value.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(s)) return DBNull.Value;

            var t = ResolveDbBaseType(dbType);

            // ??憿??湔???銝莎??踹?隤方??詨?
            if (t.Contains("char") || t.Contains("text"))
                return s;

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

            return s;
        }

        private static bool IsLargeTextType(string dbType)
        {
            var t = dbType.ToLowerInvariant();
            return t is "text" or "ntext";
        }
        // Paged(table, page, pageSize, orderBy?, orderDir?)
        [HttpGet]
        public async Task<IActionResult> Paged(
            [FromQuery] string table,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] string? orderBy = null,
            [FromQuery] string? orderDir = "DESC",
            [FromQuery] int? mb = null)
        {
            if (string.IsNullOrWhiteSpace(table)) return BadRequest("table is required");
        var actualTable = await ResolveActualTableNameAsync(table);

            var tblOk = await TableExistsAsync(actualTable);
            if (!tblOk) return NotFound($"Table '{table}' not found.");

            var whereSql = string.Empty;
            if (mb.HasValue && await ColumnExistsAsync(actualTable, "MB"))
            {
                whereSql = " WHERE [MB] = @mb";
            }

            var totalCount = 0;
            await using (var cntConn = new SqlConnection(_connStr))
            {
                await cntConn.OpenAsync();
                await using var cntCmd = new SqlCommand($"SELECT COUNT(1) FROM [{actualTable}]{whereSql}", cntConn);
                if (whereSql.Length > 0)
                    cntCmd.Parameters.AddWithValue("@mb", mb!.Value);
                totalCount = Convert.ToInt32(await cntCmd.ExecuteScalarAsync());
            }

            var orderSql = string.IsNullOrWhiteSpace(orderBy)
                ? await BuildDefaultOrderBySqlAsync(actualTable, orderDir)
                : await BuildOrderBySqlAsync(actualTable, orderBy, orderDir);

            if (pageSize <= 0)
            {
                var allDt = new DataTable();
                await using var connAll = new SqlConnection(_connStr);
                await connAll.OpenAsync();
                await using (var cmdAll = new SqlCommand($"SELECT * FROM [{actualTable}]{whereSql}{orderSql}", connAll))
                {
                    if (whereSql.Length > 0)
                        cmdAll.Parameters.AddWithValue("@mb", mb!.Value);
                    await using var rd = await cmdAll.ExecuteReaderAsync();
                    allDt.Load(rd);
                }
                var allRows = ToDictList(allDt);
                await EnrichRowsWithOcxLookupAsync(table, actualTable, allRows);
                return Ok(new { totalCount, data = allRows });
            }

            if (string.IsNullOrWhiteSpace(orderSql))
                orderSql = " ORDER BY (SELECT 1)";

            var offset = Math.Max(0, (page - 1) * pageSize);
            var sql = $"SELECT * FROM [{actualTable}]{whereSql}{orderSql} OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";

            var dt = new DataTable();
            await using var conn = new SqlConnection(_connStr);
            await conn.OpenAsync();
            await using (var cmd = new SqlCommand(sql, conn))
            {
                if (whereSql.Length > 0)
                    cmd.Parameters.AddWithValue("@mb", mb!.Value);
                cmd.Parameters.AddWithValue("@offset", offset);
                cmd.Parameters.AddWithValue("@pageSize", pageSize);
                await using var rd = await cmd.ExecuteReaderAsync();
                dt.Load(rd);
            }
            var rows = ToDictList(dt);
            await EnrichRowsWithOcxLookupAsync(table, actualTable, rows);
            return Ok(new { totalCount, data = rows });
        }


       // ?Ｘ?嚗opRows(table, top, orderBy?, orderDir?)
    [HttpGet]
    public async Task<IActionResult> TopRows([FromQuery] string table, [FromQuery] int top = 100, [FromQuery] string? orderBy = null, [FromQuery] string? orderDir = "ASC")
    {
        if (string.IsNullOrWhiteSpace(table)) return BadRequest("table is required");
        var actualTable = await ResolveActualTableNameAsync(table);

        var tblOk = await TableExistsAsync(actualTable);
        if (!tblOk) return NotFound($"Table '{table}' not found.");

        var orderSql = await BuildOrderBySqlAsync(actualTable, orderBy, orderDir);

        var sql = $"SELECT TOP (@top) * FROM [{actualTable}]{orderSql}";
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
        var rows = ToDictList(dt);
        return Ok(rows);
    }

    // ?啣?嚗yKeys(table, keyNames[], keyValues[]) ??憭?= 璇辣 (AND)
    [HttpGet]
    public async Task<IActionResult> ByKeys([FromQuery] string table, [FromQuery] string[] keyNames, [FromQuery] string[] keyValues, [FromQuery] string? orderBy = null, [FromQuery] string? orderDir = "ASC")
    {
        if (string.IsNullOrWhiteSpace(table)) return BadRequest("table is required");
        var actualTable = await ResolveActualTableNameAsync(table);
        if (keyNames == null || keyValues == null || keyNames.Length == 0 || keyNames.Length != keyValues.Length)
            return BadRequest("keyNames and keyValues must be same length and not empty.");

        // 撽?table
        var tblOk = await TableExistsAsync(actualTable);
        if (!tblOk) return NotFound($"Table '{table}' not found.");

        // 撽?column & 蝯?隞?
        var whereParts = new List<string>();
        var parameters = new List<SqlParameter>();
        for (int i = 0; i < keyNames.Length; i++)
        {
            var col = keyNames[i];
            var val = keyValues[i];

            var colOk = await ColumnExistsAsync(actualTable, col);
            if (!colOk) return BadRequest($"Column '{col}' not found in '{table}'.");

            var p = new SqlParameter($"@p{i}", (object?)val ?? DBNull.Value);
            parameters.Add(p);
            whereParts.Add($"[{col}] = @p{i}");
        }

        var whereSql = string.Join(" AND ", whereParts);
        var orderSql = await BuildOrderBySqlAsync(actualTable, orderBy, orderDir);
        var sql = $"SELECT * FROM [{actualTable}] WHERE {whereSql}{orderSql}";

        var dt = new DataTable();
        await using var conn = (SqlConnection)_context.Database.GetDbConnection();
        await conn.OpenAsync();
        await using (var cmd = new SqlCommand(sql, conn))
        {
            parameters.ForEach(p => cmd.Parameters.Add(p));
            await using var rd = await cmd.ExecuteReaderAsync();
            dt.Load(rd);
        }
        var rows = ToDictList(dt);
        await EnrichRowsWithOcxLookupAsync(table, actualTable, rows);
        return Ok(rows);
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

    private Task EnrichRowsWithOcxLookupAsync(string dictTableName, string actualTableName, List<Dictionary<string, object?>> rows)
    {
        if (rows == null || rows.Count == 0) return Task.CompletedTask;

        try
        {
            var dictService = new TableDictionaryService(_context);
            var lookupMaps = dictService.GetOCXLookups(dictTableName);
            if (lookupMaps.Count == 0 && !string.Equals(dictTableName, actualTableName, StringComparison.OrdinalIgnoreCase))
                lookupMaps = dictService.GetOCXLookups(actualTableName);
            if (lookupMaps.Count == 0 && dictTableName.StartsWith("MGN_", StringComparison.OrdinalIgnoreCase))
                lookupMaps = dictService.GetOCXLookups(dictTableName.Substring(4));
            if (lookupMaps.Count == 0 && !dictTableName.StartsWith("MGN_", StringComparison.OrdinalIgnoreCase))
                lookupMaps = dictService.GetOCXLookups("MGN_" + dictTableName);
            if (lookupMaps.Count == 0) return Task.CompletedTask;

            static string ToKey(object? v) => v == null || v == DBNull.Value ? "" : v.ToString()?.Trim() ?? "";

            foreach (var row in rows)
            {
                foreach (var map in lookupMaps)
                {
                    if (map == null || string.IsNullOrWhiteSpace(map.FieldName)) continue;
                    if (row.ContainsKey(map.FieldName)) continue;

                    var key = "";
                    if (!string.IsNullOrWhiteSpace(map.KeyFieldName) && row.TryGetValue(map.KeyFieldName, out var keyFieldVal))
                        key = ToKey(keyFieldVal);
                    if (string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(map.KeySelfName) && row.TryGetValue(map.KeySelfName, out var keySelfVal))
                        key = ToKey(keySelfVal);
                    if (string.IsNullOrWhiteSpace(key) && row.TryGetValue(map.FieldName, out var rawVal))
                        key = ToKey(rawVal);

                    var display = "";
                    if (!string.IsNullOrWhiteSpace(key) && map.LookupValues != null && map.LookupValues.TryGetValue(key, out var dv) && dv != null)
                        display = dv;

                    row[map.FieldName] = display;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "CommonTable: enrich OCX lookup failed for {Dict}/{Actual}", dictTableName, actualTableName);
        }
        return Task.CompletedTask;
    }

    private async Task<string> BuildOrderBySqlAsync(string table, string? orderByRaw, string? defaultDir = "ASC")
    {
        if (string.IsNullOrWhiteSpace(orderByRaw)) return "";

        // ?迂??? * ??+ 隞?”蝛箇嚗?憒?RateDate*desc
        var normalized = orderByRaw
            .Replace('*', ' ')
            .Replace('+', ' ');

        var parts = normalized
            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim())
            .Where(p => !string.IsNullOrWhiteSpace(p));

        var list = new List<string>();
        foreach (var part in parts)
        {
            var tokens = part.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 0) continue;

            var col = tokens[0].Trim('[', ']');
            if (string.IsNullOrWhiteSpace(col)) continue;

            var dirToken = tokens.Skip(1).FirstOrDefault();
            var dir = string.Equals(dirToken, "DESC", StringComparison.OrdinalIgnoreCase)
                ? "DESC"
                : string.Equals(dirToken, "ASC", StringComparison.OrdinalIgnoreCase) ? "ASC"
                : (string.IsNullOrWhiteSpace(defaultDir) ? "ASC" : defaultDir);

            try
            {
                var colOk = await ColumnExistsAsync(table, col);
                if (!colOk) continue;
                list.Add($"[{col}] {dir}");
            }
            catch
            {
                // ?⊥?甈?/???蝑?憿????仿???嚗??500
                return "";
            }
        }

        return list.Count == 0 ? "" : " ORDER BY " + string.Join(", ", list);
    }
    private async Task<string> BuildDefaultOrderBySqlAsync(string table, string? defaultDir = "DESC")
    {
        var dir = string.Equals(defaultDir, "ASC", StringComparison.OrdinalIgnoreCase) ? "ASC" : "DESC";
        var cols = new List<string>();
        if (await ColumnExistsAsync(table, "PartNum")) cols.Add("PartNum");
        if (await ColumnExistsAsync(table, "Revision")) cols.Add("Revision");
        if (cols.Count == 0) return "";
        return " ORDER BY " + string.Join(", ", cols.Select(c => $"[{c}] {dir}"));
    }


    private static string CleanTableName(string s)
    {
        return (s ?? "")
            .Trim()
            .Trim('[', ']')
            .Replace("dbo.", "", StringComparison.OrdinalIgnoreCase);
    }

    private async Task<string?> ResolveRealTableNameAsync(string dictTableName)
    {
        const string sql = @"
SELECT TOP 1 ISNULL(NULLIF(RealTableName,''), TableName) AS ActualName
  FROM CURdTableName WITH (NOLOCK)
 WHERE TableName = @tbl";

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@tbl", dictTableName ?? string.Empty);
        var result = await cmd.ExecuteScalarAsync();
        return result == null || result == DBNull.Value ? null : result.ToString();
    }

    private async Task<string> ResolveActualTableNameAsync(string dictTableName)
    {
        var clean = CleanTableName(dictTableName);
        var real = await ResolveRealTableNameAsync(clean);
        var candidate = CleanTableName(string.IsNullOrWhiteSpace(real) ? clean : real);
        if (await TableExistsAsync(candidate)) return candidate;
        if (candidate.StartsWith("MGN_", StringComparison.OrdinalIgnoreCase))
        {
            var trimmed = candidate.Substring(4);
            if (await TableExistsAsync(trimmed)) return trimmed;
        }
        return candidate;
    }

    // 新增：Query(table, top, orderBy?, orderDir?, 其他欄位參數) → 支援動態查詢條件
    [HttpGet]
    public async Task<IActionResult> Query([FromQuery] string table, [FromQuery] int top = 200, [FromQuery] string? orderBy = null, [FromQuery] string? orderDir = "ASC")
    {
        if (string.IsNullOrWhiteSpace(table)) return BadRequest("table is required");

        var tblOk = await TableExistsAsync(table);
        if (!tblOk) return NotFound($"Table '{table}' not found.");

        // 收集其他查詢參數（排除 table, top, orderBy, orderDir）
        var excludeKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "table", "top", "orderBy", "orderDir" };
        var filterParams = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var qp in Request.Query)
        {
            if (excludeKeys.Contains(qp.Key)) continue;
            var val = qp.Value.ToString();
            if (!string.IsNullOrWhiteSpace(val))
            {
                filterParams[qp.Key] = val;
            }
        }

        var whereParts = new List<string>();
        var parameters = new List<SqlParameter> { new SqlParameter("@top", top) };
        int paramIdx = 0;

        foreach (var kv in filterParams)
        {
            var col = kv.Key;
            var val = kv.Value;

            var colOk = await ColumnExistsAsync(table, col);
            if (!colOk) continue; // 忽略不存在的欄位

            var pName = $"@p{paramIdx++}";
            // 支援模糊查詢（如果值包含 %）
            if (val.Contains('%'))
            {
                whereParts.Add($"[{col}] LIKE {pName}");
                parameters.Add(new SqlParameter(pName, val));
            }
            else
            {
                // 預設使用 LIKE '%value%' 模糊查詢
                whereParts.Add($"[{col}] LIKE {pName}");
                parameters.Add(new SqlParameter(pName, $"%{val}%"));
            }
        }

        var whereSql = whereParts.Count > 0 ? " WHERE " + string.Join(" AND ", whereParts) : "";
        var orderSql = await BuildOrderBySqlAsync(table, orderBy, orderDir);
        var sql = $"SELECT TOP (@top) * FROM [{table}]{whereSql}{orderSql}";

        var dt = new DataTable();
        await using var conn = (SqlConnection)_context.Database.GetDbConnection();
        await conn.OpenAsync();
        await using (var cmd = new SqlCommand(sql, conn))
        {
            parameters.ForEach(p => cmd.Parameters.Add(p));
            await using var rd = await cmd.ExecuteReaderAsync();
            dt.Load(rd);
        }
        var rows = ToDictList(dt);
        return Ok(rows);
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteByKeys(
        [FromQuery] string table,
        [FromQuery] string[] keyNames,
        [FromQuery] string[] keyValues)
    {
        if (string.IsNullOrWhiteSpace(table)) return BadRequest("table is required.");
        if (keyNames == null || keyValues == null || keyNames.Length == 0 || keyNames.Length != keyValues.Length)
            return BadRequest("keyNames and keyValues must be non-empty and equal length.");

        var tblOk = await TableExistsAsync(table);
        if (!tblOk) return NotFound($"Table '{table}' not found.");

        foreach (var k in keyNames)
            if (!await ColumnExistsAsync(table, k))
                return BadRequest($"Column '{k}' not found in table '{table}'.");

        var whereParts = keyNames.Select((k, i) => $"[{k}] = @p{i}").ToList();
        var sql = $"DELETE FROM [{table}] WHERE {string.Join(" AND ", whereParts)}";

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        for (int i = 0; i < keyNames.Length; i++)
            cmd.Parameters.AddWithValue($"@p{i}", (object?)keyValues[i] ?? DBNull.Value);

        var affected = await cmd.ExecuteNonQueryAsync();
        return Ok(new { success = true, affected });
    }

    private Task<bool> TableExistsAsync(string name)
    {
        const string sql = @"
SELECT 1 FROM sys.objects WHERE name = @n AND type IN ('U','V')
UNION ALL
SELECT 1 FROM sys.synonyms WHERE name = @n";
        return ExecExistsAsync(sql, new SqlParameter("@n", name ?? string.Empty));
    }

    private Task<bool> ColumnExistsAsync(string table, string column)
    {
        const string sql = @"
SELECT 1
  FROM sys.columns c
  JOIN sys.objects o ON o.object_id = c.object_id AND o.type IN ('U','V')
 WHERE o.name = @t AND c.name = @c
UNION ALL
SELECT 1
  FROM sys.synonyms s
  CROSS APPLY (SELECT OBJECT_ID(s.base_object_name) AS obj_id) b
  JOIN sys.columns c ON c.object_id = b.obj_id
 WHERE s.name = @t AND c.name = @c";
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
