using Microsoft.AspNetCore.Mvc;
using PcbErpApi.Data;
using PcbErpApi.Models;
using PcbErpApi.Helpers;
using static PcbErpApi.Helpers.DynamicQueryHelper;
using System.Text.Json.Serialization;

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

            // 過濾掉 page/pageSize
            var filterConditions = filters
                .Where(x => !string.IsNullOrEmpty(x.Value) &&
                            x.Field.ToLower() != "page" &&
                            x.Field.ToLower() != "pagesize" &&
                            !x.Field.StartsWith("__"))
                .Select(x => new QueryParam
                {
                    Field = x.Field,
                    Op = ParseOp(x.Op),
                    Value = x.Value
                }).ToList();

            // 取得分頁參數
            var page = filters.FirstOrDefault(x => x.Field.ToLower() == "page")?.Value ?? "1";
            var pageSize = filters.FirstOrDefault(x => x.Field.ToLower() == "pagesize")?.Value ?? "50";
            if (!int.TryParse(page, out int pageNumber)) pageNumber = 1;
            if (!int.TryParse(pageSize, out int pageSizeNumber)) pageSizeNumber = 50;

            // 1️⃣ 找到 DbSet 屬性
            var dbSetProp = _context.GetType().GetProperties()
                .FirstOrDefault(p => p.Name.ToLower() == tableName);
            if (dbSetProp == null)
                return BadRequest($"找不到 Table {tableName}");

            // 抓 IQueryable<T>
            var entityType = dbSetProp.PropertyType.GenericTypeArguments.First();
            var query = (IQueryable)dbSetProp.GetValue(_context);

            // 2️⃣ 動態套用 where (ApplyDynamicWhere<T>)
            var whereMethod = typeof(DynamicQueryHelper)
                .GetMethod(nameof(DynamicQueryHelper.ApplyDynamicWhere))
                .MakeGenericMethod(entityType);

            query = (IQueryable)whereMethod.Invoke(null, new object[] { query, filterConditions });

            // 3️⃣ 找排序欄位
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

            // 4️⃣ 分頁查詢 (GetPagedAsync<T>)
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

            // 5️⃣ Lookup 與欄位定義
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

            // 6️⃣ 格式化輸出
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

            // 7️⃣ Lookup Map
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

    }
}
