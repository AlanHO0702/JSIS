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
            var tableName = request?.Table?.Trim();
            var filters = request?.filters ?? new List<QueryParamDto>();

            if (string.IsNullOrEmpty(tableName))
                return BadRequest("Table 必須指定");

            if (!filters.Any())
                filters.Add(new QueryParamDto { Field = "PaperNum", Op = "Contains", Value = "" });

            // 過濾掉 page/pageSize
            var filterConditions = filters
                .Where(x => !string.IsNullOrEmpty(x.Value) && x.Field.ToLower() != "page" && x.Field.ToLower() != "pagesize" && !x.Field.StartsWith("__"))
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

            switch (tableName?.ToLower())//注意資料表要換成小寫對應
            {
                case "spodordermain":
                    {
                        IQueryable<SpodOrderMain> query = _context.SpodOrderMain.AsQueryable();
                        query = query.ApplyDynamicWhere(filterConditions);

                        var result = await _pagedService.GetPagedAsync(
                            query.OrderByDescending(x => x.PaperDate), pageNumber, pageSizeNumber);

                        var tableDictService = new TableDictionaryService(_context);
                        var lookupMaps = tableDictService.GetOCXLookups(tableName);

                        var lookupMapData = LookupDisplayHelper.BuildLookupDisplayMap(
                            result.Data, lookupMaps, (SpodOrderMain item) => item.PaperNum?.Trim() ?? "");

                        return Ok(new { totalCount = result.TotalCount, data = result.Data, lookupMapData });
                    }
                case "spodordersub":
                    {
                        IQueryable<SpodOrderSub> query = _context.SpodOrderSub.AsQueryable();
                        query = query.ApplyDynamicWhere(filterConditions);

                        var result = await _pagedService.GetPagedAsync(
                            query.OrderByDescending(x => x.Item), pageNumber, pageSizeNumber);

                        var tableDictService = new TableDictionaryService(_context);
                        var lookupMaps = tableDictService.GetOCXLookups(tableName);

                        var lookupMapData = LookupDisplayHelper.BuildLookupDisplayMap(
                            result.Data, lookupMaps, (SpodOrderSub item) => item.PaperNum?.Trim() ?? "");

                        return Ok(new { totalCount = result.TotalCount, data = result.Data, lookupMapData });
                    }
                case "ajndjourmain":
                    {
                        IQueryable<AjndJourMain> query = _context.AjndJourMain.AsQueryable();
                        query = query.ApplyDynamicWhere(filterConditions);

                        var result = await _pagedService.GetPagedAsync(
                            query.OrderByDescending(x => x.PaperDate), pageNumber, pageSizeNumber);

                        var tableDictService = new TableDictionaryService(_context);
                        var lookupMaps = tableDictService.GetOCXLookups(tableName);

                        var lookupMapData = LookupDisplayHelper.BuildLookupDisplayMap(
                            result.Data, lookupMaps, (AjndJourMain item) => item.PaperNum?.Trim() ?? "");

                        return Ok(new { totalCount = result.TotalCount, data = result.Data, lookupMapData });
                    }
                case "ajndjoursub":
                    {
                        IQueryable<AjndJourSub> query = _context.AjndJourSub.AsQueryable();
                        query = query.ApplyDynamicWhere(filterConditions);

                        var result = await _pagedService.GetPagedAsync(
                            query.OrderByDescending(x => x.Item), pageNumber, pageSizeNumber);

                        var tableDictService = new TableDictionaryService(_context);
                        var lookupMaps = tableDictService.GetOCXLookups(tableName);

                        var lookupMapData = LookupDisplayHelper.BuildLookupDisplayMap(
                            result.Data, lookupMaps, (AjndJourSub item) => item.PaperNum?.Trim() ?? "");

                        return Ok(new { totalCount = result.TotalCount, data = result.Data, lookupMapData });
                    }    
                // 依需求再加其他表
                default:
                    return BadRequest("不支援的 Table");
            }
        }


        // 排序泛型
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
