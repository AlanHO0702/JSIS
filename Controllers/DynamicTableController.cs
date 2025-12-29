using System.Data;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;
using PcbErpApi.Helpers;

namespace PcbErpApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DynamicTableController : ControllerBase
    {
        private readonly PcbErpContext _ctx;
        private readonly ILogger<DynamicTableController> _logger;

        public DynamicTableController(PcbErpContext ctx, ILogger<DynamicTableController> logger)
        {
            _ctx = ctx;
            _logger = logger;
        }

        public class FilterItem
        {
            [JsonPropertyName("Field")] public string Field { get; set; } = "";
            [JsonPropertyName("Op")]    public string Op { get; set; } = "=";
            [JsonPropertyName("Value")] public string Value { get; set; } = "";
        }

        public class QueryRequest
        {
            public string Table { get; set; } = "";
            public List<FilterItem> Filters { get; set; } = new();
        }

        [HttpPost("AddPaper/{table}")]
        public async Task<IActionResult> AddPaper(string table)
        {
            if (string.IsNullOrWhiteSpace(table))
                return BadRequest("Table 必須指定");
            if (!Regex.IsMatch(table, @"^[A-Za-z0-9_]+$"))
                return BadRequest("Table 名稱不合法");

            var dictTable = table.Trim();

            var realTable = await _ctx.CurdTableNames
                .AsNoTracking()
                .Where(x => x.TableName.ToLower() == dictTable.ToLower())
                .Select(x => string.IsNullOrWhiteSpace(x.RealTableName) ? x.TableName : x.RealTableName)
                .FirstOrDefaultAsync();
            if (string.IsNullOrWhiteSpace(realTable))
                realTable = dictTable;

            var cs = _ctx.Database.GetConnectionString();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            // 1) 推導 PaperNum 規則：prefix + yymm(4) + seq
            string? lastPaperNum;
            await using (var cmd = new SqlCommand(
                $"SELECT TOP 1 [PaperNum] FROM [{realTable}] WITH (NOLOCK) WHERE [PaperNum] IS NOT NULL ORDER BY [PaperNum] DESC",
                conn))
            {
                var obj = await cmd.ExecuteScalarAsync();
                lastPaperNum = obj == null || obj == DBNull.Value ? null : obj.ToString();
            }

            if (string.IsNullOrWhiteSpace(lastPaperNum))
                return BadRequest($"無法新增：[{realTable}] 找不到既有 PaperNum 以推導單號規則");

            var m = Regex.Match(lastPaperNum.Trim(), @"^(?<prefix>.*?)(?<yymm>\d{4})(?<seq>\d+)$");
            if (!m.Success)
                return BadRequest($"無法新增：PaperNum 格式不支援 ({lastPaperNum})");

            var prefix = m.Groups["prefix"].Value;
            var seqLen = m.Groups["seq"].Value.Length;
            var yymm = DateTime.Now.ToString("yyMM");
            var monthPrefix = prefix + yymm;

            string? lastThisMonth;
            await using (var cmd = new SqlCommand(
                $"SELECT TOP 1 [PaperNum] FROM [{realTable}] WITH (NOLOCK) WHERE [PaperNum] LIKE @p + '%' ORDER BY [PaperNum] DESC",
                conn))
            {
                cmd.Parameters.AddWithValue("@p", monthPrefix);
                var obj = await cmd.ExecuteScalarAsync();
                lastThisMonth = obj == null || obj == DBNull.Value ? null : obj.ToString();
            }

            var nextSeq = 1;
            if (!string.IsNullOrWhiteSpace(lastThisMonth))
            {
                var m2 = Regex.Match(lastThisMonth.Trim(), @"^(?<prefix>.*?)(?<yymm>\d{4})(?<seq>\d+)$");
                if (m2.Success && int.TryParse(m2.Groups["seq"].Value, out var s))
                    nextSeq = s + 1;
            }
            var newPaperNum = monthPrefix + nextSeq.ToString("D" + seqLen);

            // 2) 取欄位中哪些是必填（NOT NULL 且無預設值）
            var cols = new List<(string name, string dataType, bool isNullable, bool hasDefault)>();
            const string metaSql = @"
SELECT c.COLUMN_NAME, c.DATA_TYPE, c.IS_NULLABLE, c.COLUMN_DEFAULT
  FROM INFORMATION_SCHEMA.COLUMNS c
 WHERE c.TABLE_NAME = @t;";
            await using (var cmd = new SqlCommand(metaSql, conn))
            {
                cmd.Parameters.AddWithValue("@t", realTable);
                await using var rd = await cmd.ExecuteReaderAsync();
                while (await rd.ReadAsync())
                {
                    cols.Add((
                        rd.GetString(0),
                        rd.GetString(1),
                        rd.GetString(2).Equals("YES", StringComparison.OrdinalIgnoreCase),
                        !rd.IsDBNull(3)
                    ));
                }
            }

            var colMap = cols.ToDictionary(x => x.name, x => x, StringComparer.OrdinalIgnoreCase);
            if (!colMap.ContainsKey("PaperNum"))
                return BadRequest($"無法新增：[{realTable}] 沒有 PaperNum 欄位");

            var insertCols = new List<string> { "PaperNum" };
            var insertParams = new List<SqlParameter> { new SqlParameter("@PaperNum", newPaperNum) };

            void AddDefaultIfNeeded(string name, object val)
            {
                if (!colMap.TryGetValue(name, out var meta)) return;
                if (meta.isNullable || meta.hasDefault) return;
                if (insertCols.Contains(meta.name, StringComparer.OrdinalIgnoreCase)) return;
                insertCols.Add(meta.name);
                insertParams.Add(new SqlParameter("@" + meta.name, val));
            }

            AddDefaultIfNeeded("PaperDate", DateTime.Now);
            AddDefaultIfNeeded("BuildDate", DateTime.Now);

            foreach (var c in cols)
            {
                if (insertCols.Contains(c.name, StringComparer.OrdinalIgnoreCase)) continue;
                if (c.isNullable || c.hasDefault) continue;
                if (c.name.Equals("PaperNum", StringComparison.OrdinalIgnoreCase)) continue;

                object val = c.dataType.ToLowerInvariant() switch
                {
                    "int" or "smallint" or "tinyint" or "bigint" => 0,
                    "decimal" or "numeric" or "money" or "smallmoney" or "float" or "real" => 0,
                    "bit" => 0,
                    "datetime" or "smalldatetime" or "date" or "datetime2" => DateTime.Now,
                    _ => ""
                };
                insertCols.Add(c.name);
                insertParams.Add(new SqlParameter("@" + c.name, val ?? DBNull.Value));
            }

            var colsSql = string.Join(",", insertCols.Select(c => $"[{c}]"));
            var valsSql = string.Join(",", insertCols.Select(c => "@" + c));
            var insertSql = $"INSERT INTO [{realTable}] ({colsSql}) VALUES ({valsSql});";

            try
            {
                await using var cmd = new SqlCommand(insertSql, conn);
                cmd.Parameters.AddRange(CloneParams(insertParams).ToArray());
                await cmd.ExecuteNonQueryAsync();
                return Ok(new { paperNum = newPaperNum });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AddPaper failed for {Table}({RealTable})", dictTable, realTable);
                return BadRequest($"新增失敗: {ex.Message}");
            }
        }

        [HttpPost("PagedQuery")]
        public async Task<IActionResult> PagedQuery([FromBody] QueryRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Table))
                return BadRequest("Table 必須指定");

            // 基本白名單，防止 injection
            if (!System.Text.RegularExpressions.Regex.IsMatch(req.Table, @"^[A-Za-z0-9_]+$"))
                return BadRequest("Table 名稱不合法");

            var dictTable = req.Table.Trim();

            // 取 RealTableName
            // 取得實體表名（有 RealTableName 則用，否則 fallback 字典表名）
            var realTable = await _ctx.CurdTableNames
                .AsNoTracking()
                .Where(x => x.TableName.ToLower() == dictTable.ToLower())
                .Select(x => string.IsNullOrWhiteSpace(x.RealTableName) ? x.TableName : x.RealTableName)
                .FirstOrDefaultAsync();
            if (string.IsNullOrWhiteSpace(realTable))
                realTable = dictTable;

            // 欄位白名單
            var fieldList = await _ctx.CURdTableFields
                .AsNoTracking()
                .Where(f => f.TableName.ToLower() == dictTable.ToLower())
                .Select(f => f.FieldName)
                .ToListAsync();

            var fieldSet = new HashSet<string>(fieldList, StringComparer.OrdinalIgnoreCase);
            var fieldMap = fieldList
                .Where(f => !string.IsNullOrWhiteSpace(f))
                .GroupBy(f => f, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            static string NormalizeFilterField(string field)
            {
                if (string.IsNullOrWhiteSpace(field)) return field;
                var s = field.Trim();
                // CustomerId_2 / CustomerId2 / CustomerIdFrom / CustomerIdTo
                s = Regex.Replace(s, @"(?:_?(From|To))$", "", RegexOptions.IgnoreCase);
                // 去掉最後的數字尾碼或 _2、-3
                s = Regex.Replace(s, @"[_\-]?\d+$", "", RegexOptions.IgnoreCase);
                return s;
            }

            string? ResolveFieldName(string rawField)
            {
                if (string.IsNullOrWhiteSpace(rawField)) return null;
                // 1) exact
                if (fieldMap.TryGetValue(rawField.Trim(), out var exact)) return exact;
                // 2) normalized
                var norm = NormalizeFilterField(rawField);
                if (!string.Equals(norm, rawField, StringComparison.OrdinalIgnoreCase)
                    && fieldMap.TryGetValue(norm, out var n2))
                    return n2;
                // 3) try case-insensitive match after normalize (for weird spacing)
                var alt = fieldMap.Keys.FirstOrDefault(k => k.Equals(norm, StringComparison.OrdinalIgnoreCase));
                return string.IsNullOrWhiteSpace(alt) ? null : alt;
            }

            int page = 1, pageSize = 50;
            var conditions = new List<string>();
            var parameters = new List<SqlParameter>();
            int pIndex = 0;

            foreach (var f in req.Filters ?? new())
            {
                if (string.Equals(f.Field, "page", StringComparison.OrdinalIgnoreCase))
                {
                    int.TryParse(f.Value, out page);
                    continue;
                }
                if (string.Equals(f.Field, "pageSize", StringComparison.OrdinalIgnoreCase))
                {
                    int.TryParse(f.Value, out pageSize);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(f.Field) || string.IsNullOrWhiteSpace(f.Value))
                    continue;
                var realField = ResolveFieldName(f.Field);
                if (string.IsNullOrWhiteSpace(realField))
                    continue; // 忽略未知欄位/別名

                var op = NormalizeOp(f.Op);
                var paramName = $"@p{pIndex++}";
                if (op == "LIKE")
                    parameters.Add(new SqlParameter(paramName, $"%{f.Value}%"));
                else
                    parameters.Add(new SqlParameter(paramName, f.Value));
                conditions.Add($"[{realField}] {op} {paramName}");
            }

            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 1000) pageSize = 50;

            var whereSql = conditions.Count > 0 ? $"WHERE {string.Join(" AND ", conditions)}" : "";
            var orderSql =
                fieldSet.Contains("PaperDate")
                    ? (fieldSet.Contains("PaperNum")
                        ? "[PaperDate] DESC, [PaperNum] DESC"
                        : "[PaperDate] DESC")
                    : fieldSet.Contains("Item")
                        ? "[Item]"
                        : fieldSet.Contains("PaperNum")
                            ? "[PaperNum]"
                            : "1";

            try
            {
                var sqlPaged = new StringBuilder();
                sqlPaged.Append($"SELECT * FROM [{realTable}] WITH (NOLOCK) {whereSql} ");
                sqlPaged.Append($"ORDER BY {orderSql} ");
                sqlPaged.Append($"OFFSET {(page - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY;");

                var sqlCount = $"SELECT COUNT(1) FROM [{realTable}] WITH (NOLOCK) {whereSql};";

                var result = new List<Dictionary<string, object?>>();
                int totalCount = 0;

                var cs = _ctx.Database.GetConnectionString();
                await using var conn = new SqlConnection(cs);
                await conn.OpenAsync();

                // count
                await using (var cmd = new SqlCommand(sqlCount, conn))
                {
                    cmd.Parameters.AddRange(CloneParams(parameters).ToArray());
                    var obj = await cmd.ExecuteScalarAsync();
                    totalCount = obj == null || obj == DBNull.Value ? 0 : Convert.ToInt32(obj);
                }

                // data
                await using (var cmd = new SqlCommand(sqlPaged.ToString(), conn))
                {
                    cmd.Parameters.AddRange(CloneParams(parameters).ToArray());
                    await using var rd = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);
                    while (await rd.ReadAsync())
                    {
                        var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                        for (int i = 0; i < rd.FieldCount; i++)
                        {
                            dict[rd.GetName(i)] = rd.IsDBNull(i) ? null : rd.GetValue(i);
                        }
                        result.Add(dict);
                    }
                }

                // lookup（失敗不要影響主要資料回傳）
                Dictionary<string, Dictionary<string, string>> lookupMapData = new();
                try
                {
                    var tableDictService = new TableDictionaryService(_ctx);
                    var lookupMaps = tableDictService.GetOCXLookups(dictTable);

                    // 1) 補上 OCX Lookup 的「非實體顯示欄位」（第三階子明細會用到）
                    if (lookupMaps.Count > 0)
                    {
                        foreach (var row in result)
                        {
                            foreach (var map in lookupMaps)
                            {
                                if (map == null || string.IsNullOrWhiteSpace(map.FieldName)) continue;

                                // 若實體欄位本來就存在，避免覆寫
                                if (row.ContainsKey(map.FieldName)) continue;

                                static string ToKey(object? v) => v == null || v == DBNull.Value ? "" : v.ToString()?.Trim() ?? "";

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

                                // 即使沒找到，也補空字串，避免前端因第一筆缺值而不產生欄位
                                row[map.FieldName] = display;
                            }
                        }
                    }

                    // 2) 舊版回傳 lookupMapData（其他頁面可能仍在用）
                    lookupMapData = LookupDisplayHelper.BuildLookupDisplayMap(
                        result,
                        lookupMaps,
                        item => item.TryGetValue("PaperNum", out var v) ? v?.ToString() ?? "" : ""
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Build lookup map failed for {Table}", dictTable);
                    lookupMapData = new();
                }

                return Ok(new { totalCount, data = result, lookupMapData });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DynamicTable query failed for {Table}", dictTable);
                return BadRequest($"查詢 {dictTable} 失敗: {ex.Message}");
            }
        }

        // 給多頁籤明細（_MultiTabDetail）使用：依 PaperNum 取回該字典表的所有列
        // 回傳格式：[{ col1:..., col2:... }, ...]
        [HttpGet("ByPaperNum")]
        public async Task<IActionResult> ByPaperNum([FromQuery] string table, [FromQuery] string paperNum, [FromQuery] int top = 9999)
        {
            if (string.IsNullOrWhiteSpace(table))
                return BadRequest("table 必須指定");
            if (!Regex.IsMatch(table, @"^[A-Za-z0-9_]+$"))
                return BadRequest("table 名稱不合法");
            if (string.IsNullOrWhiteSpace(paperNum))
                return Ok(Array.Empty<Dictionary<string, object?>>());

            top = Math.Clamp(top, 1, 20000);
            var dictTable = table.Trim();

            var realTable = await _ctx.CurdTableNames
                .AsNoTracking()
                .Where(x => x.TableName.ToLower() == dictTable.ToLower())
                .Select(x => string.IsNullOrWhiteSpace(x.RealTableName) ? x.TableName : x.RealTableName)
                .FirstOrDefaultAsync();
            if (string.IsNullOrWhiteSpace(realTable))
                realTable = dictTable;

            var fieldList = await _ctx.CURdTableFields
                .AsNoTracking()
                .Where(f => f.TableName.ToLower() == dictTable.ToLower())
                .Select(f => f.FieldName)
                .ToListAsync();

            var cols = fieldList
                .Where(f => !string.IsNullOrWhiteSpace(f))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            // 若該表沒有辭典欄位設定，避免直接 SELECT *（風險較高）→ 回傳空陣列
            if (cols.Count == 0)
                return Ok(Array.Empty<Dictionary<string, object?>>());

            var cs = _ctx.Database.GetConnectionString();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            static string NormalizeSqlName(string raw)
            {
                var s = (raw ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(s)) return string.Empty;
                s = s.Replace("[", string.Empty).Replace("]", string.Empty).Trim();
                return s;
            }

            static string Esc(string id) => $"[{id.Replace("]", "]]")}]";

            static string EscTableName(string raw)
            {
                var s = NormalizeSqlName(raw);
                if (string.IsNullOrWhiteSpace(s)) return string.Empty;
                var parts = s.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                return parts.Length switch
                {
                    1 => Esc(parts[0]),
                    _ => string.Join(".", parts.Select(Esc))
                };
            }

            static async Task<HashSet<string>> GetExistingColumnsAsync(SqlConnection conn, string rawTable)
            {
                var tbl = NormalizeSqlName(rawTable);
                if (string.IsNullOrWhiteSpace(tbl)) return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                async Task<HashSet<string>> QueryAsync(string objectIdName)
                {
                    var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    const string sql = "SELECT name FROM sys.columns WHERE object_id = OBJECT_ID(@t)";
                    await using var cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@t", objectIdName);
                    await using var rd = await cmd.ExecuteReaderAsync();
                    while (await rd.ReadAsync())
                    {
                        var n = rd.GetString(0);
                        if (!string.IsNullOrWhiteSpace(n)) set.Add(n);
                    }
                    return set;
                }

                // 先用原始名稱（可能含 schema），若查不到再 fallback dbo.<name>
                var set1 = await QueryAsync(tbl);
                if (set1.Count > 0) return set1;

                if (!tbl.Contains('.', StringComparison.OrdinalIgnoreCase))
                {
                    var dboSet = await QueryAsync("dbo." + tbl);
                    if (dboSet.Count > 0) return dboSet;
                }

                return set1;
            }

            var existingCols = await GetExistingColumnsAsync(conn, realTable);
            if (existingCols.Count == 0)
                return Ok(Array.Empty<Dictionary<string, object?>>());

            if (!existingCols.Contains("PaperNum"))
                return BadRequest($"ByPaperNum 失敗: 資料表 {realTable} 缺少欄位 PaperNum");

            // 字典欄位可能包含不存在於實體表的欄位（例如顯示用/舊欄位），需先過濾
            var safeCols = cols.Where(c => existingCols.Contains(c)).ToList();
            if (safeCols.Count == 0)
                return Ok(Array.Empty<Dictionary<string, object?>>());

            var selectCols = string.Join(",", safeCols.Select(Esc));
            var orderBy = safeCols.Any(c => c.Equals("Item", StringComparison.OrdinalIgnoreCase)) ? Esc("Item") : Esc("PaperNum");

            var sql = $@"
SELECT TOP (@top) {selectCols}
  FROM {EscTableName(realTable)} WITH (NOLOCK)
 WHERE {Esc("PaperNum")} = @paperNum
 ORDER BY {orderBy};";

                try
                {
                    await using var cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@top", top);
                    cmd.Parameters.AddWithValue("@paperNum", paperNum);

                    var list = new List<Dictionary<string, object?>>();
                    await using var rd = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess);
                    while (await rd.ReadAsync())
                    {
                        var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                        for (var i = 0; i < rd.FieldCount; i++)
                        {
                            var name = rd.GetName(i);
                            row[name] = rd.IsDBNull(i) ? null : rd.GetValue(i);
                        }
                        list.Add(row);
                    }

                    // 補上 OCX Lookup 的「非實體顯示欄位」（例如 MatName/StatusName）
                    // 讓前端 (_MultiTabDetail) 可直接顯示，不必依賴實體表欄位存在。
                    try
                    {
                        var tableDictService = new TableDictionaryService(_ctx);
                        var lookupMaps = tableDictService.GetOCXLookups(dictTable);
                        if (lookupMaps.Count > 0)
                        {
                            foreach (var row in list)
                            {
                                foreach (var map in lookupMaps)
                                {
                                    if (map == null || string.IsNullOrWhiteSpace(map.FieldName)) continue;

                                    // 若該顯示欄位本來就存在於實體表，前面 safeCols 已會選出，這裡不要覆寫
                                    if (existingCols.Contains(map.FieldName)) continue;

                                    static string ToKey(object? v) => v == null || v == DBNull.Value ? "" : v.ToString()?.Trim() ?? "";

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

                                    // 無論是否找到，都補一個 key，確保前端能產生欄位（避免第一列缺值導致欄位被吃掉）
                                    row[map.FieldName] = display;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "ByPaperNum: build OCX lookup display failed for {DictTable}", dictTable);
                    }

                    return Ok(list);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "ByPaperNum failed for {DictTable}({RealTable}) paperNum={PaperNum}", dictTable, realTable, paperNum);
                return BadRequest($"ByPaperNum 失敗: {ex.Message}");
            }
        }

        private static IEnumerable<SqlParameter> CloneParams(IEnumerable<SqlParameter> source)
        {
            foreach (var p in source)
            {
                var clone = new SqlParameter(p.ParameterName, p.Value ?? DBNull.Value);
                clone.SqlDbType = p.SqlDbType;
                clone.DbType = p.DbType;
                clone.Direction = p.Direction;
                clone.Size = p.Size;
                yield return clone;
            }
        }

        private static string NormalizeOp(string op)
        {
            if (string.IsNullOrWhiteSpace(op)) return "=";
            var s = op.Trim().ToUpperInvariant();
            return s switch
            {
                "CONTAINS" => "LIKE",
                "LIKE" => "LIKE",
                ">=" => ">=",
                "<=" => "<=",
                ">" => ">",
                "<" => "<",
                "<>" => "<>",
                "!=" => "<>",
                _ => "="
            };
        }
    }
}
