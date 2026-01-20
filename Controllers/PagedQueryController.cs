using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using PcbErpApi.Data;
using PcbErpApi.Models;
using PcbErpApi.Helpers;
using static PcbErpApi.Helpers.DynamicQueryHelper;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Reflection;

namespace PcbErpApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PagedQueryController : ControllerBase
    {
        private readonly PcbErpContext _context;
        private readonly PaginationService _pagedService;
        private readonly ILogger<PagedQueryController> _logger;

        public PagedQueryController(PcbErpContext context, PaginationService pagedService, ILogger<PagedQueryController> logger)
        {
            _context = context;
            _pagedService = pagedService;
            _logger = logger;
        }

        // 共用 DTO
        public class QueryFilterRequest
        {
            public string Table { get; set; }
            public List<QueryParamDto> filters { get; set; } = new();
        }

        public class QueryParamDto
        {
            [JsonPropertyName("Field")]
            public string Field { get; set; }
            [JsonPropertyName("Op")]
            public string Op { get; set; }
            [JsonPropertyName("Value")]
            public string Value { get; set; }
        }

        [HttpPost("PagedQuery")]
        public async Task<IActionResult> PagedQuery([FromBody] QueryFilterRequest request)
        {
            var dictTableName = request?.Table?.Trim();
            var filters = request?.filters ?? new List<QueryParamDto>();

            if (string.IsNullOrEmpty(dictTableName))
                return BadRequest("Table 必須指定");

            if (!filters.Any())
                filters.Add(new QueryParamDto { Field = "PaperNum", Op = "Contains", Value = "" });

            // 先取分頁參數 ─ 不需要 entityType
            var page = filters.FirstOrDefault(x => x.Field.Equals("page", StringComparison.OrdinalIgnoreCase))?.Value ?? "1";
            var pageSize = filters.FirstOrDefault(x => x.Field.Equals("pageSize", StringComparison.OrdinalIgnoreCase))?.Value ?? "50";
            if (!int.TryParse(page, out int pageNumber)) pageNumber = 1;
            if (!int.TryParse(pageSize, out int pageSizeNumber)) pageSizeNumber = 50;

            // 1) 取 DbSet / entityType / query
            // 先嘗試用原始名稱查詢 CURdTableName
            var realTableName = await _context.CurdTableNames
                .AsNoTracking()
                .Where(x => x.TableName.ToLower() == dictTableName.ToLower())
                .Select(x => string.IsNullOrWhiteSpace(x.RealTableName) ? x.TableName : x.RealTableName)
                .FirstOrDefaultAsync();

            // 若找不到，嘗試用移除底線的版本查詢
            if (string.IsNullOrWhiteSpace(realTableName))
            {
                var normalizedName = dictTableName.Replace("_", "");
                realTableName = await _context.CurdTableNames
                    .AsNoTracking()
                    .Where(x => x.TableName.ToLower().Replace("_", "") == normalizedName.ToLower())
                    .Select(x => string.IsNullOrWhiteSpace(x.RealTableName) ? x.TableName : x.RealTableName)
                    .FirstOrDefaultAsync();
            }

            if (string.IsNullOrWhiteSpace(realTableName))
                realTableName = dictTableName;

            // 將 View 名稱對應到 DbSet 屬性名稱（移除底線和特殊字元）
            var dbSetName = MapViewNameToDbSetName(realTableName);
            var dbSetProp = _context.GetType().GetProperties()
                .FirstOrDefault(p => p.Name.Equals(dbSetName, StringComparison.OrdinalIgnoreCase));

            // fallback 1: 若實體表找不到，試試原始字典表名稱
            if (dbSetProp == null && !realTableName.Equals(dictTableName, StringComparison.OrdinalIgnoreCase))
            {
                dbSetName = MapViewNameToDbSetName(dictTableName);
                dbSetProp = _context.GetType().GetProperties()
                    .FirstOrDefault(p => p.Name.Equals(dbSetName, StringComparison.OrdinalIgnoreCase));
            }

            // fallback 2: 直接嘗試移除底線的版本
            if (dbSetProp == null)
            {
                dbSetName = dictTableName.Replace("_", "");
                dbSetProp = _context.GetType().GetProperties()
                    .FirstOrDefault(p => p.Name.Equals(dbSetName, StringComparison.OrdinalIgnoreCase));
            }

            // ★ fallback 3: 若仍找不到 DbSet，改用 Raw SQL 查詢
            if (dbSetProp == null)
            {
                return await PagedQueryRawSqlAsync(dictTableName, realTableName, filters, pageNumber, pageSizeNumber);
            }

            var entityType = dbSetProp.PropertyType.GenericTypeArguments.First();
            var query = (IQueryable)dbSetProp.GetValue(_context);

            // 2) 在這裡才用 entityType 來建立 filterConditions（映射別名 → 真欄位名）
            var filterConditions = filters
                .Where(x => !string.IsNullOrEmpty(x.Value)
                        && !x.Field.Equals("page", StringComparison.OrdinalIgnoreCase)
                        && !x.Field.Equals("pageSize", StringComparison.OrdinalIgnoreCase)
                        && !(x.Field?.StartsWith("__") ?? false))
                .Select(x =>
                {
                    var realField = NormalizeFieldToEntity(x.Field, entityType); // ★ 用到 entityType
                    return new QueryParam
                    {
                        Field = realField,
                        Op = ParseOp(x.Op),
                        Value = x.Value
                    };
                })
                .ToList();

            // 3) 套 where
            var whereMethod = typeof(DynamicQueryHelper)
                .GetMethod(nameof(DynamicQueryHelper.ApplyDynamicWhere))
                .MakeGenericMethod(entityType);

            query = (IQueryable)whereMethod.Invoke(null, new object[] { query, filterConditions });

            // 4) 排序
            var orderField = entityType.GetProperty("PaperDate") != null ? "PaperDate"
                        : entityType.GetProperty("Item") != null ? "Item"
                        : entityType.GetProperty("PaperNum") != null ? "PaperNum"
                        : null;

            if (orderField != null)
            {
                var orderMethod = typeof(PagedQueryController)
                    .GetMethod(nameof(OrderByField))
                    .MakeGenericMethod(entityType);

                query = (IQueryable)orderMethod.Invoke(null, new object[] { query, orderField, true });
            }

            // 5) 分頁
            var pagedMethod = typeof(PaginationService)
                .GetMethod(nameof(PaginationService.GetPagedAsync))
                .MakeGenericMethod(entityType);

            var task = (Task)pagedMethod.Invoke(_pagedService, new object[] { query, pageNumber, pageSizeNumber });
            await task.ConfigureAwait(false);

            var resultProp = task.GetType().GetProperty("Result");
            var result = resultProp.GetValue(task);
            var dataProp = result.GetType().GetProperty("Data");
            var data = (IEnumerable<object>)dataProp.GetValue(result);
            var totalCount = (int)result.GetType().GetProperty("TotalCount").GetValue(result);

            // 6) lookup 與欄位
            var tableDictService = new TableDictionaryService(_context);
            var lookupMaps = tableDictService.GetOCXLookups(dictTableName);

            var fields = _context.CURdTableFields
                .Where(f => f.TableName == dictTableName)
                .Select(f => new TableFieldViewModel
                {
                    FieldName = f.FieldName,
                    DataType = f.DataType,
                    FormatStr = f.FormatStr
                })
                .ToList();

            var formattedData = data.Select(item =>
            {
                var dict = new Dictionary<string, object>();
                foreach (var field in fields)
                {
                    var rawValue = GetValue(item, field.FieldName);
                    dict[field.FieldName] = FormatHelper.FormatValue(rawValue, field.DataType, field.FormatStr);
                }
                return dict;
            }).ToList();

            var lookupMapData = LookupDisplayHelper.BuildLookupDisplayMap(
                data, lookupMaps, item =>
                {
                    var prop = item.GetType().GetProperty("PaperNum");
                    return prop?.GetValue(item)?.ToString()?.Trim() ?? "";
                });

            return Ok(new { totalCount, data = formattedData, lookupMapData });
        }

        /// <summary>
        /// 當找不到 DbSet 時，使用 Raw SQL 查詢（類似 DynamicTableController 的實現）
        /// </summary>
        private async Task<IActionResult> PagedQueryRawSqlAsync(
            string dictTableName,
            string realTableName,
            List<QueryParamDto> filters,
            int pageNumber,
            int pageSizeNumber)
        {
            // 基本白名單，防止 injection
            if (!Regex.IsMatch(realTableName, @"^[A-Za-z0-9_]+$"))
                return BadRequest("Table 名稱不合法");

            // 欄位白名單
            var fieldList = await _context.CURdTableFields
                .AsNoTracking()
                .Where(f => f.TableName.ToLower() == dictTableName.ToLower()
                         || f.TableName.ToLower() == realTableName.ToLower())
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
                s = Regex.Replace(s, @"(?:_?(From|To))$", "", RegexOptions.IgnoreCase);
                s = Regex.Replace(s, @"[_\-]?\d+$", "", RegexOptions.IgnoreCase);
                return s;
            }

            string? ResolveFieldName(string rawField)
            {
                if (string.IsNullOrWhiteSpace(rawField)) return null;
                if (fieldMap.TryGetValue(rawField.Trim(), out var exact)) return exact;
                var norm = NormalizeFilterField(rawField);
                if (!string.Equals(norm, rawField, StringComparison.OrdinalIgnoreCase)
                    && fieldMap.TryGetValue(norm, out var n2))
                    return n2;
                var alt = fieldMap.Keys.FirstOrDefault(k => k.Equals(norm, StringComparison.OrdinalIgnoreCase));
                return string.IsNullOrWhiteSpace(alt) ? null : alt;
            }

            static string NormalizeOp(string op)
            {
                if (string.IsNullOrWhiteSpace(op)) return "=";
                return op.ToLowerInvariant() switch
                {
                    "contains" or "like" => "LIKE",
                    "eq" or "=" or "==" => "=",
                    "ne" or "!=" or "<>" => "<>",
                    "gt" or ">" => ">",
                    "ge" or ">=" => ">=",
                    "lt" or "<" => "<",
                    "le" or "<=" => "<=",
                    _ => "="
                };
            }

            var conditions = new List<string>();
            var parameters = new List<SqlParameter>();
            int pIndex = 0;

            foreach (var f in filters ?? new())
            {
                if (string.Equals(f.Field, "page", StringComparison.OrdinalIgnoreCase)) continue;
                if (string.Equals(f.Field, "pageSize", StringComparison.OrdinalIgnoreCase)) continue;
                if (string.IsNullOrWhiteSpace(f.Field) || string.IsNullOrWhiteSpace(f.Value)) continue;
                if (f.Field?.StartsWith("__") ?? false) continue;

                var realField = ResolveFieldName(f.Field);
                if (string.IsNullOrWhiteSpace(realField)) continue;

                var op = NormalizeOp(f.Op);
                var paramName = $"@p{pIndex++}";
                if (op == "LIKE")
                    parameters.Add(new SqlParameter(paramName, $"%{f.Value}%"));
                else
                    parameters.Add(new SqlParameter(paramName, f.Value));
                conditions.Add($"[{realField}] {op} {paramName}");
            }

            var whereSql = conditions.Count > 0 ? $"WHERE {string.Join(" AND ", conditions)}" : "";

            // 決定排序欄位
            var orderSql =
                fieldSet.Contains("PaperDate")
                    ? (fieldSet.Contains("PaperNum") ? "[PaperDate] DESC, [PaperNum] DESC" : "[PaperDate] DESC")
                    : fieldSet.Contains("Item") ? "[Item]"
                    : fieldSet.Contains("PaperNum") ? "[PaperNum]"
                    : "1";

            try
            {
                var sqlPaged = new StringBuilder();
                sqlPaged.Append($"SELECT * FROM [{realTableName}] t0 WITH (NOLOCK) {whereSql} ");
                sqlPaged.Append($"ORDER BY {orderSql} ");
                sqlPaged.Append($"OFFSET {(pageNumber - 1) * pageSizeNumber} ROWS FETCH NEXT {pageSizeNumber} ROWS ONLY;");

                var sqlCount = $"SELECT COUNT(1) FROM [{realTableName}] t0 WITH (NOLOCK) {whereSql};";

                var result = new List<Dictionary<string, object?>>();
                int totalCount = 0;

                var cs = _context.Database.GetConnectionString();
                await using var conn = new SqlConnection(cs);
                await conn.OpenAsync();

                // count
                await using (var cmd = new SqlCommand(sqlCount, conn))
                {
                    foreach (var p in parameters) cmd.Parameters.Add(new SqlParameter(p.ParameterName, p.Value));
                    var obj = await cmd.ExecuteScalarAsync();
                    totalCount = obj != null && obj != DBNull.Value ? Convert.ToInt32(obj) : 0;
                }

                // data
                await using (var cmd = new SqlCommand(sqlPaged.ToString(), conn))
                {
                    foreach (var p in parameters) cmd.Parameters.Add(new SqlParameter(p.ParameterName, p.Value));
                    await using var rd = await cmd.ExecuteReaderAsync();
                    while (await rd.ReadAsync())
                    {
                        var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                        for (var i = 0; i < rd.FieldCount; i++)
                        {
                            var name = rd.GetName(i);
                            row[name] = rd.IsDBNull(i) ? null : rd.GetValue(i);
                        }
                        result.Add(row);
                    }
                }

                // lookup 與欄位
                var tableDictService = new TableDictionaryService(_context);
                var lookupMaps = tableDictService.GetOCXLookups(dictTableName);

                var fields = _context.CURdTableFields
                    .Where(f => f.TableName.ToLower() == dictTableName.ToLower()
                             || f.TableName.ToLower() == realTableName.ToLower())
                    .Select(f => new TableFieldViewModel
                    {
                        FieldName = f.FieldName,
                        DataType = f.DataType,
                        FormatStr = f.FormatStr
                    })
                    .ToList();

                var formattedData = result.Select(row =>
                {
                    var dict = new Dictionary<string, object?>();
                    foreach (var field in fields)
                    {
                        if (row.TryGetValue(field.FieldName, out var rawValue))
                            dict[field.FieldName] = FormatHelper.FormatValue(rawValue, field.DataType, field.FormatStr);
                    }
                    // 也把不在 fields 裡的欄位加進去（例如 PaperNum）
                    foreach (var kv in row)
                    {
                        if (!dict.ContainsKey(kv.Key))
                            dict[kv.Key] = kv.Value;
                    }
                    return dict;
                }).ToList();

                var lookupMapData = LookupDisplayHelper.BuildLookupDisplayMap(
                    result, lookupMaps, item =>
                    {
                        if (item is Dictionary<string, object?> d && d.TryGetValue("PaperNum", out var pn))
                            return pn?.ToString()?.Trim() ?? "";
                        return "";
                    });

                return Ok(new { totalCount, data = formattedData, lookupMapData });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PagedQueryRawSqlAsync error for table {Table}", realTableName);
                return StatusCode(500, new { ok = false, message = ex.Message });
            }
        }

        private object GetValue(object item, string fieldName)
        {
            if (item is IDictionary<string, object> dict && dict.TryGetValue(fieldName, out var val))
                return val;

            var prop = item.GetType().GetProperty(fieldName);
            return prop != null ? prop.GetValue(item) : null;
        }

        public static IQueryable<T> OrderByField<T>(IQueryable<T> source, string field, bool desc = true)
        {
            var param = System.Linq.Expressions.Expression.Parameter(typeof(T), "x");
            var property = System.Linq.Expressions.Expression.PropertyOrField(param, field);
            var lambda = System.Linq.Expressions.Expression.Lambda(property, param);
            var method = desc ? "OrderByDescending" : "OrderBy";
            var types = new Type[] { typeof(T), property.Type };
            var mce = System.Linq.Expressions.Expression.Call(typeof(Queryable), method, types, source.Expression, lambda);
            return source.Provider.CreateQuery<T>(mce);
        }

        private static string NormalizeFieldToEntity(string field, Type entityType)
        {
            if (string.IsNullOrWhiteSpace(field)) return field;

            // 先試精確對應（忽略大小寫）
            var prop = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                .FirstOrDefault(p => p.Name.Equals(field, StringComparison.OrdinalIgnoreCase));
            if (prop != null) return prop.Name;

            // 允許 UI 別名：去尾碼（如 CustomerId5、CustomerId_2、CustomerIdTo、CustomerIdFrom）
            var core = Regex.Replace(field, @"(?:_?(From|To))$", "", RegexOptions.IgnoreCase);
            core = Regex.Replace(core, @"[_\-]?\d+$", "");   // 去掉最後的數字尾碼或 _2、-3

            prop = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                            .FirstOrDefault(p => p.Name.Equals(core, StringComparison.OrdinalIgnoreCase));
            return prop?.Name ?? field; // 找不到就原樣返回（讓你看 log）
        }

        /// <summary>
        /// 將資料庫 View 名稱對應到 DbSet 屬性名稱
        /// 例如: FMEdV_ProcNIS_ToStd -> FmedVProcNisToStd
        /// </summary>
        private static string MapViewNameToDbSetName(string viewName)
        {
            if (string.IsNullOrWhiteSpace(viewName))
                return viewName;

            // 移除所有底線和特殊字元，保留字母和數字
            var cleanName = Regex.Replace(viewName, @"[_\-]", "");

            return cleanName;
        }

    }


}
