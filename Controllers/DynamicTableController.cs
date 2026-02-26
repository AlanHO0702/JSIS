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
using static TableDictionaryService;

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
            public string? ItemId { get; set; }
            public List<FilterItem> Filters { get; set; } = new();
            public bool SkipLookup { get; set; } = false;  // 前端已有快取時跳過 lookup 查詢
        }

        public class PaperTypeOption
        {
            public int PaperType { get; set; }
            public string? PaperTypeName { get; set; }
            public string? HeadFirst { get; set; }
            public int? PowerType { get; set; }
            public string? UpdateFieldName { get; set; }
            public string? UpdateValue { get; set; }
            public string? TradeId { get; set; }
        }

        public class AddPaperRequest
        {
            public string? ItemId { get; set; }
            public string? UserId { get; set; }
            public string? UseId { get; set; }
            public int? PaperType { get; set; }
            public string? PaperTypeName { get; set; }
            public string? HeadFirst { get; set; }
            public int? PowerType { get; set; }
            public string? UpdateFieldName { get; set; }
            public string? UpdateValue { get; set; }
            public string? TradeId { get; set; }
        }

        private static async Task<string?> ResolveRealTableNameAsync(SqlConnection conn, string? dictTableName)
        {
            if (string.IsNullOrWhiteSpace(dictTableName)) return null;
            await using var cmd = new SqlCommand(@"
SELECT TOP 1 ISNULL(NULLIF(RealTableName,''), TableName) AS ActualName
  FROM CURdTableName WITH (NOLOCK)
 WHERE TableName = @tbl;", conn);
            cmd.Parameters.AddWithValue("@tbl", dictTableName);
            var obj = await cmd.ExecuteScalarAsync();
            if (obj == null || obj == DBNull.Value) return dictTableName;
            return obj.ToString();
        }

        private static async Task<bool> HasColumnAsync(SqlConnection conn, string tableName, string columnName)
        {
            const string sql = @"
SELECT COUNT(1)
  FROM sys.columns c
  JOIN sys.objects o ON c.object_id = o.object_id
 WHERE o.type = 'U' AND o.name = @table AND c.name = @col;";
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@table", tableName);
            cmd.Parameters.AddWithValue("@col", columnName);
            var obj = await cmd.ExecuteScalarAsync();
            return obj != null && obj != DBNull.Value && Convert.ToInt32(obj) > 0;
        }

        private static async Task<string?> ResolvePaperIdAsync(SqlConnection conn, string? itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId)) return null;

            await using (var cmdMaster = new SqlCommand(@"
SELECT TOP 1 TableName
  FROM CURdOCXTableSetUp WITH (NOLOCK)
 WHERE ItemId = @itemId
   AND (TableKind = 'Master1' OR TableKind LIKE 'Master%')
 ORDER BY TableKind;", conn))
            {
                cmdMaster.Parameters.AddWithValue("@itemId", itemId);
                var masterObj = await cmdMaster.ExecuteScalarAsync();
                if (masterObj != null && masterObj != DBNull.Value)
                {
                    var dictTable = masterObj.ToString();
                    var real = await ResolveRealTableNameAsync(conn, dictTable);
                    if (!string.IsNullOrWhiteSpace(real)) return real;
                }
            }

            await using var cmd = new SqlCommand(@"
SELECT TOP 1 PaperId
  FROM CURdSysItems WITH (NOLOCK)
 WHERE ItemId = @itemId;", conn);
            cmd.Parameters.AddWithValue("@itemId", itemId);
            var paperIdObj = await cmd.ExecuteScalarAsync();
            if (paperIdObj == null || paperIdObj == DBNull.Value) return null;
            return paperIdObj.ToString();
        }

        [HttpGet("PaperTypes/{table}")]
        public async Task<IActionResult> GetPaperTypes(string table, [FromQuery] string? itemId = null)
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

            var resolvedPaperId = await ResolvePaperIdAsync(conn, itemId);
            var effectivePaperId = string.IsNullOrWhiteSpace(resolvedPaperId) ? realTable : resolvedPaperId;

            int selectType = 0;
            string? headFirst = null;
            await using (var cmdInfo = new SqlCommand(@"
SELECT TOP 1 SelectType, HeadFirst
  FROM CURdPaperInfo WITH (NOLOCK)
 WHERE LOWER(PaperId) = LOWER(@paperId) OR LOWER(PaperId) = LOWER(@dictTable);", conn))
            {
                cmdInfo.Parameters.AddWithValue("@paperId", effectivePaperId);
                cmdInfo.Parameters.AddWithValue("@dictTable", dictTable);
                await using var rd = await cmdInfo.ExecuteReaderAsync();
                if (await rd.ReadAsync())
                {
                    selectType = rd["SelectType"] == DBNull.Value ? 0 : Convert.ToInt32(rd["SelectType"]);
                    headFirst = rd["HeadFirst"]?.ToString();
                }
            }

            int? powerType = null;
            if (!string.IsNullOrWhiteSpace(itemId))
            {
                await using var cmdPower = new SqlCommand(@"
SELECT TOP 1 PowerType
  FROM CURdSysItems WITH (NOLOCK)
 WHERE ItemId = @itemId;", conn);
                cmdPower.Parameters.AddWithValue("@itemId", itemId);
                var obj = await cmdPower.ExecuteScalarAsync();
                if (obj != null && obj != DBNull.Value && int.TryParse(obj.ToString(), out var p))
                    powerType = p;
            }

            var list = new List<PaperTypeOption>();
            if (selectType == 1)
            {
                var hasTradeId = await HasColumnAsync(conn, "CURdPaperType", "TradeId");
                var sql = @"
SELECT PaperType, PaperTypeName, HeadFirst, PowerType, UpdateFieldName, UpdateValue, ";
                sql += hasTradeId ? "TradeId" : "CAST(NULL AS NVARCHAR(50)) AS TradeId";
                sql += @"
  FROM CURdPaperType WITH (NOLOCK)
 WHERE LOWER(PaperId) = LOWER(@paperId) OR LOWER(PaperId) = LOWER(@dictTable)";
                if (powerType.HasValue)
                    sql += " AND (PowerType = @powerType OR PowerType = -1)";
                sql += " ORDER BY PaperType;";

                await using var cmdTypes = new SqlCommand(sql, conn);
                cmdTypes.Parameters.AddWithValue("@paperId", effectivePaperId);
                cmdTypes.Parameters.AddWithValue("@dictTable", dictTable);
                if (powerType.HasValue)
                    cmdTypes.Parameters.AddWithValue("@powerType", powerType.Value);

                await using var rd = await cmdTypes.ExecuteReaderAsync();
                while (await rd.ReadAsync())
                {
                    list.Add(new PaperTypeOption
                    {
                        PaperType = rd["PaperType"] == DBNull.Value ? 0 : Convert.ToInt32(rd["PaperType"]),
                        PaperTypeName = rd["PaperTypeName"]?.ToString(),
                        HeadFirst = rd["HeadFirst"]?.ToString(),
                        PowerType = rd["PowerType"] == DBNull.Value ? null : Convert.ToInt32(rd["PowerType"]),
                        UpdateFieldName = rd["UpdateFieldName"]?.ToString(),
                        UpdateValue = rd["UpdateValue"]?.ToString(),
                        TradeId = rd["TradeId"]?.ToString()
                    });
                }
            }

            return Ok(new { selectType, types = list, headFirst, powerType, paperId = effectivePaperId });
        }

        [HttpPost("AddPaper/{table}")]
        public async Task<IActionResult> AddPaper(string table, [FromBody] AddPaperRequest? req)
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

            var itemId = req?.ItemId?.Trim();
            if (string.IsNullOrWhiteSpace(itemId))
            {
                var itemIds = await _ctx.CurdOcxtableSetUp
                    .AsNoTracking()
                    .Where(x =>
                        x.TableName == dictTable &&
                        !string.IsNullOrWhiteSpace(x.ItemId) &&
                        (x.TableKind == "Master1" ||
                         (x.TableKind ?? "").StartsWith("Master", StringComparison.OrdinalIgnoreCase)))
                    .Select(x => x.ItemId)
                    .Distinct()
                    .Take(2)
                    .ToListAsync();

                if (itemIds.Count == 1)
                    itemId = itemIds[0]?.Trim();
            }
            var userId = string.IsNullOrWhiteSpace(req?.UserId) ? "admin" : req!.UserId!.Trim();
            var useId = string.IsNullOrWhiteSpace(req?.UseId) ? "A001" : req!.UseId!.Trim();
            var resolvedPaperId = await ResolvePaperIdAsync(conn, itemId);
            var effectivePaperId = string.IsNullOrWhiteSpace(resolvedPaperId) ? realTable : resolvedPaperId;

            int selectType = 0;
            string? defaultHeadFirst = null;
            await using (var cmdInfo = new SqlCommand(@"
SELECT TOP 1 SelectType, HeadFirst
  FROM CURdPaperInfo WITH (NOLOCK)
 WHERE LOWER(PaperId) = LOWER(@paperId) OR LOWER(PaperId) = LOWER(@dictTable);", conn))
            {
                cmdInfo.Parameters.AddWithValue("@paperId", effectivePaperId);
                cmdInfo.Parameters.AddWithValue("@dictTable", dictTable);
                await using var rd = await cmdInfo.ExecuteReaderAsync();
                if (await rd.ReadAsync())
                {
                    selectType = rd["SelectType"] == DBNull.Value ? 0 : Convert.ToInt32(rd["SelectType"]);
                    defaultHeadFirst = rd["HeadFirst"]?.ToString();
                }
            }

            PaperTypeOption? selectedType = null;
            if (selectType == 1)
            {
                if (req?.PaperType == null)
                    return BadRequest("需要選擇單據類別");

                var hasTradeId = await HasColumnAsync(conn, "CURdPaperType", "TradeId");
                await using var cmdType = new SqlCommand(@"
SELECT TOP 1 PaperType, PaperTypeName, HeadFirst, PowerType, UpdateFieldName, UpdateValue, " +
                    (hasTradeId ? "TradeId" : "CAST(NULL AS NVARCHAR(50)) AS TradeId") + @"
  FROM CURdPaperType WITH (NOLOCK)
 WHERE (LOWER(PaperId) = LOWER(@paperId) OR LOWER(PaperId) = LOWER(@dictTable))
   AND PaperType = @paperType;", conn);
                cmdType.Parameters.AddWithValue("@paperId", effectivePaperId);
                cmdType.Parameters.AddWithValue("@dictTable", dictTable);
                cmdType.Parameters.AddWithValue("@paperType", req.PaperType.Value);
                await using var rd = await cmdType.ExecuteReaderAsync();
                if (await rd.ReadAsync())
                {
                    selectedType = new PaperTypeOption
                    {
                        PaperType = rd["PaperType"] == DBNull.Value ? 0 : Convert.ToInt32(rd["PaperType"]),
                        PaperTypeName = rd["PaperTypeName"]?.ToString(),
                        HeadFirst = rd["HeadFirst"]?.ToString(),
                        PowerType = rd["PowerType"] == DBNull.Value ? null : Convert.ToInt32(rd["PowerType"]),
                        UpdateFieldName = rd["UpdateFieldName"]?.ToString(),
                        UpdateValue = rd["UpdateValue"]?.ToString(),
                        TradeId = rd["TradeId"]?.ToString()
                    };
                }
                else
                {
                    return BadRequest("找不到對應的單據類別設定");
                }
            }
            else if (!string.IsNullOrWhiteSpace(itemId))
            {
                int? defaultPaperType = null;
                await using (var cmdItem = new SqlCommand(@"
SELECT TOP 1 PaperType
  FROM CURdSysItems WITH (NOLOCK)
 WHERE ItemId = @itemId;", conn))
                {
                    cmdItem.Parameters.AddWithValue("@itemId", itemId);
                    var obj = await cmdItem.ExecuteScalarAsync();
                    if (obj != null && obj != DBNull.Value && int.TryParse(obj.ToString(), out var p))
                        defaultPaperType = p;
                }

                if (defaultPaperType.HasValue)
                {
                    var hasTradeId = await HasColumnAsync(conn, "CURdPaperType", "TradeId");
                    await using var cmdType = new SqlCommand(@"
SELECT TOP 1 PaperType, PaperTypeName, HeadFirst, PowerType, UpdateFieldName, UpdateValue, " +
                        (hasTradeId ? "TradeId" : "CAST(NULL AS NVARCHAR(50)) AS TradeId") + @"
  FROM CURdPaperType WITH (NOLOCK)
 WHERE (LOWER(PaperId) = LOWER(@paperId) OR LOWER(PaperId) = LOWER(@dictTable))
   AND PaperType = @paperType;", conn);
                    cmdType.Parameters.AddWithValue("@paperId", effectivePaperId);
                    cmdType.Parameters.AddWithValue("@dictTable", dictTable);
                    cmdType.Parameters.AddWithValue("@paperType", defaultPaperType.Value);
                    await using var rd = await cmdType.ExecuteReaderAsync();
                    if (await rd.ReadAsync())
                    {
                        selectedType = new PaperTypeOption
                        {
                            PaperType = rd["PaperType"] == DBNull.Value ? 0 : Convert.ToInt32(rd["PaperType"]),
                            PaperTypeName = rd["PaperTypeName"]?.ToString(),
                            HeadFirst = rd["HeadFirst"]?.ToString(),
                            PowerType = rd["PowerType"] == DBNull.Value ? null : Convert.ToInt32(rd["PowerType"]),
                            UpdateFieldName = rd["UpdateFieldName"]?.ToString(),
                            UpdateValue = rd["UpdateValue"]?.ToString(),
                            TradeId = rd["TradeId"]?.ToString()
                        };
                    }
                }
            }

            var headFirst = selectedType?.HeadFirst ?? defaultHeadFirst ?? string.Empty;

            string? sDate = null;
            string? sNow = null;
            await using (var cmdDate = new SqlCommand("exec CURdGetServerDateTimeStr", conn))
            {
                await using var rd = await cmdDate.ExecuteReaderAsync();
                if (await rd.ReadAsync())
                {
                    sDate = rd["sDate"]?.ToString();
                    sNow = rd["sNow"]?.ToString();
                }
            }

            if (string.IsNullOrWhiteSpace(sDate) || string.IsNullOrWhiteSpace(sNow))
                return BadRequest("無法取得 Server 的日期時間");

            var useIdPrefix = string.IsNullOrWhiteSpace(useId) ? string.Empty : useId.Substring(0, 1);
            string? newPaperNum;
            await using (var cmdNum = new SqlCommand("exec CURdGetPaperNum @p0,@p1,@p2,@p3,@p4,@p5", conn))
            {
                cmdNum.Parameters.AddWithValue("@p0", realTable);
                cmdNum.Parameters.AddWithValue("@p1", string.Empty);
                cmdNum.Parameters.AddWithValue("@p2", useIdPrefix);
                cmdNum.Parameters.AddWithValue("@p3", sDate);
                cmdNum.Parameters.AddWithValue("@p4", headFirst);
                cmdNum.Parameters.AddWithValue("@p5", useId);
                var obj = await cmdNum.ExecuteScalarAsync();
                newPaperNum = obj == null || obj == DBNull.Value ? null : obj.ToString();
            }

            if (string.IsNullOrWhiteSpace(newPaperNum))
                return BadRequest("取單號失敗");

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

            var insertCols = new List<string>();
            var insertParams = new List<SqlParameter>();
            var values = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
            {
                ["PaperNum"] = newPaperNum,
                ["PaperDate"] = DateTime.TryParse(sDate, out var pd) ? pd : DateTime.Now,
                ["BuildDate"] = DateTime.TryParse(sNow, out var bd) ? bd : DateTime.Now,
                ["UserId"] = userId,
                ["UseId"] = useId,
                ["Status"] = 0,
                ["Finished"] = 0
            };

            if (selectedType != null)
            {
                values["dllPaperType"] = selectedType.PaperType;
                values["dllPaperTypeName"] = selectedType.PaperTypeName ?? string.Empty;
                values["dllHeadFirst"] = selectedType.HeadFirst ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(selectedType.UpdateFieldName))
                    values[selectedType.UpdateFieldName] = selectedType.UpdateValue ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(selectedType.TradeId))
                    values["TradeId"] = selectedType.TradeId;
            }
            else if (!string.IsNullOrWhiteSpace(defaultHeadFirst))
            {
                values["dllHeadFirst"] = defaultHeadFirst;
            }

            foreach (var kvp in values)
            {
                if (!colMap.TryGetValue(kvp.Key, out var meta)) continue;
                if (insertCols.Contains(meta.name, StringComparer.OrdinalIgnoreCase)) continue;
                insertCols.Add(meta.name);
                insertParams.Add(new SqlParameter("@" + meta.name, kvp.Value ?? DBNull.Value));
            }

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

                var runSql = string.Empty;
                if (!string.IsNullOrWhiteSpace(itemId))
                {
                    await using var cmdRun = new SqlCommand(@"
SELECT TOP 1 RunSQLAfterAdd
  FROM CURdOCXTableSetUp WITH (NOLOCK)
 WHERE ItemId = @itemId AND TableName = @table;", conn);
                    cmdRun.Parameters.AddWithValue("@itemId", itemId);
                    cmdRun.Parameters.AddWithValue("@table", dictTable);
                    var obj = await cmdRun.ExecuteScalarAsync();
                    runSql = obj == null || obj == DBNull.Value ? string.Empty : obj.ToString() ?? string.Empty;
                }
                if (string.IsNullOrWhiteSpace(runSql))
                {
                    await using var cmdRun = new SqlCommand(@"
SELECT TOP 1 RunSQLAfterAdd
  FROM CURdOCXTableSetUp WITH (NOLOCK)
 WHERE TableName = @table;", conn);
                    cmdRun.Parameters.AddWithValue("@table", dictTable);
                    var obj = await cmdRun.ExecuteScalarAsync();
                    runSql = obj == null || obj == DBNull.Value ? string.Empty : obj.ToString() ?? string.Empty;
                }

                if (!string.IsNullOrWhiteSpace(runSql))
                {
                    if (runSql.Contains("@PaperNum", StringComparison.OrdinalIgnoreCase))
                        runSql = runSql.Replace("@PaperNum", $"'{newPaperNum}'", StringComparison.OrdinalIgnoreCase);

                    var finalSql = $"{runSql} and t0.PaperNum='{newPaperNum}'";
                    await using var cmdAfter = new SqlCommand(finalSql, conn);
                    await cmdAfter.ExecuteNonQueryAsync();
                }

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

            var queryMeta = await LoadQueryFieldMetaAsync(req.ItemId, dictTable, realTable);
            var metaMap = queryMeta
                .GroupBy(x => NormalizeFilterField(x.ColumnName ?? ""), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var setupMap = await LoadTableSetupMapAsync(req.ItemId);

            int page = 1, pageSize = 50;
            var masterConditions = new List<string>();
            var detailGroups = new Dictionary<string, DetailGroup>(StringComparer.OrdinalIgnoreCase);
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

                var normKey = NormalizeFilterField(f.Field);
                metaMap.TryGetValue(normKey, out var meta);

                var isDetail = meta != null && IsDetailField(meta, dictTable);
                if (!isDetail)
                {
                    var realField = ResolveFieldName(f.Field);
                    if (string.IsNullOrWhiteSpace(realField))
                        continue; // 忽略未知欄位/別名

                    var op = NormalizeOp(string.IsNullOrWhiteSpace(f.Op) ? meta?.DefaultEqual : f.Op);
                    var masterCondition = BuildMasterConditionSql(realField, op, f.Value, parameters, ref pIndex);
                    if (!string.IsNullOrWhiteSpace(masterCondition))
                        masterConditions.Add(masterCondition);
                    continue;
                }

                var detailInfo = ResolveDetailInfo(meta!, setupMap, dictTable);
                if (detailInfo == null) continue;

                var detailField = await ResolveDetailFieldAsync(detailInfo.DictTable, f.Field);
                if (string.IsNullOrWhiteSpace(detailField)) continue;

                var opDetail = NormalizeOp(string.IsNullOrWhiteSpace(f.Op) ? meta?.DefaultEqual : f.Op);
                var detailCondition = BuildConditionSql(detailField, opDetail, f.Value, parameters, ref pIndex);
                if (string.IsNullOrWhiteSpace(detailCondition)) continue;

                var groupKey = $"{detailInfo.RealTable}|{detailInfo.MasterKey}|{detailInfo.DetailKey}";
                if (!detailGroups.TryGetValue(groupKey, out var group))
                {
                    group = new DetailGroup
                    {
                        RealTable = detailInfo.RealTable,
                        MasterKey = detailInfo.MasterKey,
                        DetailKey = detailInfo.DetailKey
                    };
                    detailGroups[groupKey] = group;
                }
                group.Conditions.Add(detailCondition);
            }

            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 1000) pageSize = 50;

            var detailConditions = detailGroups.Values
                .Where(g => g.Conditions.Count > 0)
                .Select(g =>
                    $"t0.[{g.MasterKey}] IN (SELECT DISTINCT [{g.DetailKey}] FROM [{g.RealTable}] t1 WITH (NOLOCK) WHERE 1=1 AND {string.Join(" AND ", g.Conditions)})")
                .ToList();

            var allConditions = masterConditions.Concat(detailConditions).ToList();
            var whereSql = allConditions.Count > 0 ? $"WHERE {string.Join(" AND ", allConditions)}" : "";
            string? filterSql = null;
            if (!string.IsNullOrWhiteSpace(req.ItemId))
            {
                filterSql = await ResolveMasterFilterSqlAsync(req.ItemId, dictTable);
            }
            if (!string.IsNullOrWhiteSpace(filterSql))
            {
                var extra = filterSql.Trim();
                if (extra.StartsWith("where ", StringComparison.OrdinalIgnoreCase))
                    extra = extra.Substring(5).Trim();
                if (!string.IsNullOrWhiteSpace(extra))
                {
                    var startsWithAndOr = Regex.IsMatch(extra, @"^(and|or)\b", RegexOptions.IgnoreCase);
                    if (string.IsNullOrWhiteSpace(whereSql))
                        whereSql = startsWithAndOr ? $"WHERE 1=1 {extra}" : $"WHERE {extra}";
                    else
                        whereSql = startsWithAndOr ? $"{whereSql} {extra}" : $"{whereSql} AND {extra}";
                }
            }
            // EMOdProdInfo 優先用 PartNum 排序（主鍵有索引），其他表用 PaperDate/PaperNum
            var orderSql =
                dictTable.Equals("EMOdProdInfo", StringComparison.OrdinalIgnoreCase) && fieldSet.Contains("PartNum")
                    ? "[PartNum]"
                    : fieldSet.Contains("PaperDate")
                        ? (fieldSet.Contains("PaperNum")
                            ? "[PaperDate] DESC, [PaperNum] DESC"
                            : "[PaperDate] DESC")
                        : fieldSet.Contains("Item")
                            ? "[Item]"
                            : fieldSet.Contains("PaperNum")
                                ? "[PaperNum]"
                                : fieldSet.Contains("PartNum")
                                    ? "[PartNum]"
                                    : "1";

            try
            {
                var sqlPaged = new StringBuilder();
                sqlPaged.Append($"SELECT * FROM [{realTable}] t0 WITH (NOLOCK) {whereSql} ");
                sqlPaged.Append($"ORDER BY {orderSql} ");
                sqlPaged.Append($"OFFSET {(page - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY;");

                var sqlCount = $"SELECT COUNT(1) FROM [{realTable}] t0 WITH (NOLOCK) {whereSql};";

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
                // 當前端已有快取 (SkipLookup=true) 時，跳過耗時的 lookup 查詢
                Dictionary<string, Dictionary<string, string>> lookupMapData = new();
                List<OCXLookupMap>? lookupMaps = null;

                if (!req.SkipLookup)
                {
                    try
                    {
                        var tableDictService = new TableDictionaryService(_ctx);
                        lookupMaps = tableDictService.GetOCXLookups(dictTable);

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
                            lookupMaps.Cast<dynamic>(),
                            item => item.TryGetValue("PaperNum", out var v) ? v?.ToString() ?? "" : ""
                        );
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Build lookup map failed for {Table}", dictTable);
                        lookupMapData = new();
                    }
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

        private sealed class QueryFieldMeta
        {
            public string ColumnName { get; set; } = "";
            public string? TableName { get; set; }
            public string? TableKind { get; set; }
            public string? AliasName { get; set; }
            public string? DefaultEqual { get; set; }
        }

        private sealed class TableSetupMeta
        {
            public string DictTable { get; set; } = "";
            public string RealTable { get; set; } = "";
            public string TableKind { get; set; } = "";
            public string? MdKey { get; set; }
            public string? LocateKeys { get; set; }
        }

        private sealed class DetailGroup
        {
            public string RealTable { get; set; } = "";
            public string MasterKey { get; set; } = "";
            public string DetailKey { get; set; } = "";
            public List<string> Conditions { get; } = new();
        }

        private static bool IsDetailField(QueryFieldMeta meta, string masterDictTable)
        {
            if (!string.IsNullOrWhiteSpace(meta.TableKind))
            {
                if (meta.TableKind.StartsWith("Detail", StringComparison.OrdinalIgnoreCase)) return true;
                if (meta.TableKind.StartsWith("SubDetail", StringComparison.OrdinalIgnoreCase)) return true;
            }
            if (!string.IsNullOrWhiteSpace(meta.TableName) &&
                !meta.TableName.Equals(masterDictTable, StringComparison.OrdinalIgnoreCase))
                return true;
            if (!string.IsNullOrWhiteSpace(meta.AliasName) &&
                !meta.AliasName.Equals("t0", StringComparison.OrdinalIgnoreCase))
                return true;
            return false;
        }

        private static string NormalizeFilterFieldName(string field)
        {
            if (string.IsNullOrWhiteSpace(field)) return field;
            var s = field.Trim();
            s = Regex.Replace(s, @"(?:_?(From|To))$", "", RegexOptions.IgnoreCase);
            s = Regex.Replace(s, @"[_\-]?\d+$", "", RegexOptions.IgnoreCase);
            return s;
        }

        private static string? GetFirstKey(string? mdKey, string? locateKeys)
        {
            var raw = string.IsNullOrWhiteSpace(mdKey) ? locateKeys : mdKey;
            if (string.IsNullOrWhiteSpace(raw)) return null;
            foreach (var part in raw.Split(new[] { ';', ',', '|'}, StringSplitOptions.RemoveEmptyEntries))
            {
                var s = part.Trim();
                if (!string.IsNullOrWhiteSpace(s)) return s;
            }
            return null;
        }

        private sealed class DetailInfo
        {
            public string DictTable { get; set; } = "";
            public string RealTable { get; set; } = "";
            public string MasterKey { get; set; } = "PaperNum";
            public string DetailKey { get; set; } = "PaperNum";
        }

        private DetailInfo? ResolveDetailInfo(QueryFieldMeta meta, Dictionary<string, TableSetupMeta> setupMap, string masterDictTable)
        {
            var detailDict = meta.TableName ?? "";
            var detailKind = meta.TableKind ?? "";

            if (string.IsNullOrWhiteSpace(detailDict) && !string.IsNullOrWhiteSpace(detailKind))
            {
                if (setupMap.TryGetValue(detailKind, out var setup))
                    detailDict = setup.DictTable;
            }

            if (string.IsNullOrWhiteSpace(detailDict)) return null;

            var masterSetup = setupMap.Values.FirstOrDefault(x =>
                x.TableKind.StartsWith("Master", StringComparison.OrdinalIgnoreCase));
            var masterKey = GetFirstKey(masterSetup?.MdKey, masterSetup?.LocateKeys) ?? "PaperNum";

            var detailSetup = setupMap.Values.FirstOrDefault(x =>
                x.TableKind.Equals(detailKind, StringComparison.OrdinalIgnoreCase)) ??
                setupMap.Values.FirstOrDefault(x =>
                    x.DictTable.Equals(detailDict, StringComparison.OrdinalIgnoreCase));

            var detailKey = GetFirstKey(detailSetup?.MdKey, detailSetup?.LocateKeys) ?? masterKey;
            var realTable = detailSetup?.RealTable ?? detailDict;

            return new DetailInfo
            {
                DictTable = detailDict,
                RealTable = realTable,
                MasterKey = masterKey,
                DetailKey = detailKey
            };
        }

        private async Task<List<QueryFieldMeta>> LoadQueryFieldMetaAsync(string? itemId, string dictTable, string realTable)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return await _ctx.CURdPaperSelected
                    .AsNoTracking()
                    .Where(x => x.TableName == dictTable)
                    .Select(x => new QueryFieldMeta
                    {
                        ColumnName = x.ColumnName,
                        TableName = x.TableName,
                        TableKind = x.TableKind,
                        AliasName = x.AliasName,
                        DefaultEqual = x.DefaultEqual
                    })
                    .ToListAsync();
            }

            var cs = _ctx.Database.GetConnectionString();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();
            var paperId = await ResolvePaperIdAsync(conn, itemId);
            if (string.IsNullOrWhiteSpace(paperId)) paperId = realTable;

            var list = await _ctx.CURdPaperSelected
                .AsNoTracking()
                .Where(x => x.PaperId == paperId)
                .Select(x => new QueryFieldMeta
                {
                    ColumnName = x.ColumnName,
                    TableName = x.TableName,
                    TableKind = x.TableKind,
                    AliasName = x.AliasName,
                    DefaultEqual = x.DefaultEqual
                })
                .ToListAsync();

            if (list.Count == 0)
            {
                list = await _ctx.CURdPaperSelected
                    .AsNoTracking()
                    .Where(x => x.TableName == dictTable)
                    .Select(x => new QueryFieldMeta
                    {
                        ColumnName = x.ColumnName,
                        TableName = x.TableName,
                        TableKind = x.TableKind,
                        AliasName = x.AliasName,
                        DefaultEqual = x.DefaultEqual
                    })
                    .ToListAsync();
            }

            return list;
        }

        private async Task<Dictionary<string, TableSetupMeta>> LoadTableSetupMapAsync(string? itemId)
        {
            var map = new Dictionary<string, TableSetupMeta>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(itemId)) return map;

            var setups = await _ctx.CurdOcxtableSetUp
                .AsNoTracking()
                .Where(x => x.ItemId == itemId)
                .ToListAsync();

            if (setups.Count == 0) return map;

            var tableNames = setups.Select(x => x.TableName).Where(t => !string.IsNullOrWhiteSpace(t)).Distinct().ToList();
            var tableNameSet = new HashSet<string>(tableNames, StringComparer.OrdinalIgnoreCase);
            var realTableRows = await _ctx.CurdTableNames
                .AsNoTracking()
                .Select(x => new { x.TableName, x.RealTableName })
                .ToListAsync();

            var realMap = realTableRows
                .Where(x => tableNameSet.Contains(x.TableName))
                .ToDictionary(
                x => x.TableName,
                x => string.IsNullOrWhiteSpace(x.RealTableName) ? x.TableName : x.RealTableName,
                StringComparer.OrdinalIgnoreCase);

            foreach (var s in setups)
            {
                if (string.IsNullOrWhiteSpace(s.TableKind) || string.IsNullOrWhiteSpace(s.TableName)) continue;
                if (map.ContainsKey(s.TableKind)) continue;
                map[s.TableKind] = new TableSetupMeta
                {
                    DictTable = s.TableName,
                    RealTable = realMap.TryGetValue(s.TableName, out var r) ? r : s.TableName,
                    TableKind = s.TableKind,
                    MdKey = s.Mdkey,
                    LocateKeys = s.LocateKeys
                };
            }

            return map;
        }

        private async Task<string?> ResolveMasterFilterSqlAsync(string itemId, string dictTable)
        {
            if (string.IsNullOrWhiteSpace(itemId) || string.IsNullOrWhiteSpace(dictTable))
                return null;

            var rows = await _ctx.CurdOcxtableSetUp
                .AsNoTracking()
                .Where(x => x.ItemId == itemId && x.TableName == dictTable)
                .Select(x => new { x.TableKind, x.FilterSql })
                .ToListAsync();

            if (rows.Count == 0) return null;

            var masters = rows
                .Where(x => !string.IsNullOrWhiteSpace(x.TableKind)
                    && x.TableKind.StartsWith("Master", StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => string.Equals(x.TableKind, "Master1", StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                .ThenBy(x => x.TableKind, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var source = masters.Count > 0
                ? masters
                : rows.OrderBy(x => x.TableKind ?? string.Empty, StringComparer.OrdinalIgnoreCase).ToList();

            var preferred = source.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.FilterSql));
            return preferred?.FilterSql ?? source.FirstOrDefault()?.FilterSql;
        }

        private async Task<string?> ResolveDetailFieldAsync(string dictTable, string rawField)
        {
            if (string.IsNullOrWhiteSpace(dictTable) || string.IsNullOrWhiteSpace(rawField)) return null;
            var fieldList = await _ctx.CURdTableFields
                .AsNoTracking()
                .Where(f => f.TableName == dictTable)
                .Select(f => f.FieldName)
                .ToListAsync();

            var fieldMap = fieldList
                .Where(f => !string.IsNullOrWhiteSpace(f))
                .GroupBy(f => f, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            if (fieldMap.TryGetValue(rawField.Trim(), out var exact)) return exact;
            var norm = NormalizeFilterFieldName(rawField);
            if (fieldMap.TryGetValue(norm, out var n2)) return n2;
            var alt = fieldMap.Keys.FirstOrDefault(k => k.Equals(norm, StringComparison.OrdinalIgnoreCase));
            return string.IsNullOrWhiteSpace(alt) ? null : alt;
        }

        private static string? BuildConditionSql(string field, string op, string value, List<SqlParameter> parameters, ref int pIndex)
        {
            if (string.IsNullOrWhiteSpace(field) || string.IsNullOrWhiteSpace(value)) return null;
            var cleanOp = NormalizeOp(op);
            var trimmed = value.Trim();

            if (cleanOp == "LIKE")
            {
                var paramName = $"@p{pIndex++}";
                parameters.Add(new SqlParameter(paramName, $"%{value}%"));
                return $"t1.[{field}] LIKE {paramName}";
            }

            if (cleanOp == "IN" || cleanOp == "NOT IN")
            {
                if ((trimmed.StartsWith("(") && trimmed.EndsWith(")")) ||
                    trimmed.StartsWith("select", StringComparison.OrdinalIgnoreCase))
                {
                    return $"t1.[{field}] {cleanOp} {trimmed}";
                }

                var parts = trimmed.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim())
                    .Where(p => !string.IsNullOrWhiteSpace(p))
                    .ToList();
                if (parts.Count == 0) return null;

                var paramNames = new List<string>();
                foreach (var part in parts)
                {
                    var pName = $"@p{pIndex++}";
                    parameters.Add(new SqlParameter(pName, part));
                    paramNames.Add(pName);
                }
                return $"t1.[{field}] {cleanOp} ({string.Join(",", paramNames)})";
            }

            var param = $"@p{pIndex++}";
            parameters.Add(new SqlParameter(param, value));
            return $"t1.[{field}] {cleanOp} {param}";
        }

        private static string? BuildMasterConditionSql(string field, string op, string value, List<SqlParameter> parameters, ref int pIndex)
        {
            if (string.IsNullOrWhiteSpace(field) || string.IsNullOrWhiteSpace(value)) return null;
            var cleanOp = NormalizeOp(op);
            var trimmed = value.Trim();

            if (cleanOp == "LIKE")
            {
                var paramName = $"@p{pIndex++}";
                parameters.Add(new SqlParameter(paramName, $"%{value}%"));
                return $"[{field}] LIKE {paramName}";
            }

            if (cleanOp == "IN" || cleanOp == "NOT IN")
            {
                if ((trimmed.StartsWith("(") && trimmed.EndsWith(")")) ||
                    trimmed.StartsWith("select", StringComparison.OrdinalIgnoreCase))
                {
                    return $"[{field}] {cleanOp} {trimmed}";
                }

                var parts = trimmed.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim())
                    .Where(p => !string.IsNullOrWhiteSpace(p))
                    .ToList();
                if (parts.Count == 0) return null;

                var paramNames = new List<string>();
                foreach (var part in parts)
                {
                    var pName = $"@p{pIndex++}";
                    parameters.Add(new SqlParameter(pName, part));
                    paramNames.Add(pName);
                }
                return $"[{field}] {cleanOp} ({string.Join(",", paramNames)})";
            }

            var param = $"@p{pIndex++}";
            parameters.Add(new SqlParameter(param, value));
            return $"[{field}] {cleanOp} {param}";
        }

        private static string NormalizeOp(string? op)
        {
            if (string.IsNullOrWhiteSpace(op)) return "=";
            var s = op.Trim().ToUpperInvariant();
            return s switch
            {
                "CONTAINS" => "LIKE",
                "LIKE" => "LIKE",
                "IN" => "IN",
                "NOT IN" => "NOT IN",
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
