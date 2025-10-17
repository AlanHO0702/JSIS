using Microsoft.AspNetCore.Mvc;
using PcbErpApi.Data;
using PcbErpApi.Models;
using PcbErpApi.Helpers;
using static PcbErpApi.Helpers.DynamicQueryHelper;
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
            var tableName = request?.Table?.Trim().ToLower();
            var filters = request?.filters ?? new List<QueryParamDto>();

            if (string.IsNullOrEmpty(tableName))
                return BadRequest("Table 必須指定");

            if (!filters.Any())
                filters.Add(new QueryParamDto { Field = "PaperNum", Op = "Contains", Value = "" });

            // 先取分頁參數 ─ 不需要 entityType
            var page = filters.FirstOrDefault(x => x.Field.Equals("page", StringComparison.OrdinalIgnoreCase))?.Value ?? "1";
            var pageSize = filters.FirstOrDefault(x => x.Field.Equals("pageSize", StringComparison.OrdinalIgnoreCase))?.Value ?? "50";
            if (!int.TryParse(page, out int pageNumber)) pageNumber = 1;
            if (!int.TryParse(pageSize, out int pageSizeNumber)) pageSizeNumber = 50;

            // 1) 取 DbSet / entityType / query
            var dbSetProp = _context.GetType().GetProperties()
                .FirstOrDefault(p => p.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase));
            if (dbSetProp == null)
                return BadRequest($"找不到 Table {tableName}");

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
            var lookupMaps = tableDictService.GetOCXLookups(tableName);

            var fields = _context.CURdTableFields
                .Where(f => f.TableName.ToLower() == tableName)
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

    }
    
    
}
