using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;

namespace PcbErpApi.Pages.DynamicTemplate
{
    public class Paper3LModel : PageModel
    {
        private readonly PcbErpContext _ctx;
        private readonly ITableDictionaryService _dictService;
        private readonly HttpClient _http;

        public Paper3LModel(PcbErpContext ctx, ITableDictionaryService dictService, IHttpClientFactory httpClientFactory)
        {
            _ctx = ctx;
            _dictService = dictService;
            _http = httpClientFactory.CreateClient("MyApiClient");
        }

        public string ItemId { get; private set; } = string.Empty;
        public string ItemName { get; private set; } = string.Empty;
        public string DictTableName { get; private set; } = string.Empty;
        public string MasterTable { get; private set; } = string.Empty;
        public string DetailRouteTemplate => $"/DynamicTemplate/Paper3L/{ItemId}/{{PaperNum}}";
        public string PageTitle { get; private set; } = string.Empty;
        public int PageNumber { get; private set; } = 1;
        public int PageSize { get; private set; } = 50;
        public int TotalCount { get; private set; }
        public int TotalPages => (int)Math.Ceiling((TotalCount) / (double)PageSize);

        public List<Dictionary<string, object?>> Items { get; private set; } = new();
        public List<TableFieldViewModel> TableFields { get; private set; } = new();
        public List<QueryFieldViewModel> QueryFields { get; private set; } = new();
        public List<CURdTableField> FieldDictList { get; private set; } = new();
        public Dictionary<string, Dictionary<string, string>> LookupDisplayMap { get; private set; } = new();

        public async Task<IActionResult> OnGetAsync(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
                return NotFound("itemId is required.");

            ItemId = itemId;
            PageNumber = int.TryParse(Request.Query["page"], out var pg) ? pg : 1;
            PageSize = int.TryParse(Request.Query["pageSize"], out var ps) ? ps : 50;

            var sysItem = await _ctx.CurdSysItems.AsNoTracking()
                .Where(x => x.ItemId == itemId)
                .Select(x => new { x.ItemName, x.ItemType, x.Ocxtemplete })
                .FirstOrDefaultAsync();

            if (sysItem == null)
                return NotFound($"Item {itemId} not found.");

            var ocx = (sysItem.Ocxtemplete ?? string.Empty).Trim();
            if (sysItem.ItemType != 6 || !ocx.Equals("JSdPaper3LDLL.dll", StringComparison.OrdinalIgnoreCase))
                return NotFound($"Item {itemId} is not a JSdPaper3LDLL paper(3-level) item.");

            ItemName = sysItem.ItemName ?? string.Empty;

            var setupList = await _ctx.CurdOcxtableSetUp.AsNoTracking()
                .Where(x => x.ItemId == itemId)
                .ToListAsync();

            var master = setupList
                .Where(x => (x.TableKind ?? "").Contains("Master", StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => x.TableKind)
                .FirstOrDefault()
                ?? setupList.FirstOrDefault();

            if (master == null)
                return NotFound($"No table setup found for item {itemId}.");

            DictTableName = master.TableName ?? "";
            MasterTable = await ResolveRealTableNameAsync(DictTableName) ?? DictTableName;

            // 不顯示英文表頭
            PageTitle = string.Empty;

            QueryFields = _ctx.CURdPaperSelected
                .Where(x => x.TableName == DictTableName && x.IVisible == 1)
                .OrderBy(x => x.SortOrder)
                .Select(x => new QueryFieldViewModel
                {
                    ColumnName = x.ColumnName,
                    ColumnCaption = x.ColumnCaption,
                    DataType = x.DataType,
                    ControlType = x.ControlType ?? 0,
                    EditMask = x.EditMask,
                    DefaultValue = x.DefaultValue,
                    DefaultEqual = x.DefaultEqual,
                    SortOrder = x.SortOrder
                })
                .ToList();

            ViewData["QueryFields"] = QueryFields;
            ViewData["PagedQueryUrl"] = "/api/DynamicTable/PagedQuery";

            await FetchListByDynamicTableAsync();

            FieldDictList = _dictService.GetFieldDict(DictTableName, typeof(object));

            TableFields = FieldDictList
                .Where(x => x.Visible == 1)
                .OrderBy(x => x.SerialNum)
                .GroupBy(x => x.FieldName)
                .Select(g =>
                {
                    var x = g.First();
                    return new TableFieldViewModel
                    {
                        FieldName = x.FieldName,
                        DisplayLabel = x.DisplayLabel,
                        SerialNum = x.SerialNum ?? 0,
                        Visible = true,
                        LookupResultField = x.LookupResultField,
                        LookupCond1Field = x.LookupCond1Field,
                        LookupCond1ResultField = x.LookupCond1ResultField,
                        LookupCond2Field = x.LookupCond2Field,
                        LookupCond2ResultField = x.LookupCond2ResultField,
                        DataType = x.DataType,
                        FormatStr = x.FormatStr,
                        iFieldWidth = x.iFieldWidth,
                        DisplaySize = x.DisplaySize
                    };
                })
                .ToList();

            ViewData["Fields"] = TableFields;
            ViewData["KeyFieldName"] = "PaperNum";
            ViewData["LookupDisplayMap"] = LookupDisplayMap ?? new();

            return Page();
        }

        private async Task FetchListByDynamicTableAsync()
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var url = $"{baseUrl}/api/DynamicTable/PagedQuery";

            var filters = new List<FilterItem>();
            foreach (var q in QueryFields)
            {
                if (string.IsNullOrWhiteSpace(q.ColumnName)) continue;
                var val = Request.Query[q.ColumnName].ToString();
                if (string.IsNullOrWhiteSpace(val)) continue;
                filters.Add(new FilterItem { Field = q.ColumnName, Op = q.DefaultEqual ?? "", Value = val });
            }
            filters.Add(new FilterItem { Field = "page", Op = "", Value = PageNumber.ToString() });
            filters.Add(new FilterItem { Field = "pageSize", Op = "", Value = PageSize.ToString() });

            var payload = new { table = DictTableName, filters };
            var json = JsonSerializer.Serialize(payload);
            var req = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            try
            {
                var apiResp = await _http.SendAsync(req);
                if (!apiResp.IsSuccessStatusCode)
                {
                    ViewData["LoadError"] = $"查詢 API 失敗：{(int)apiResp.StatusCode} {apiResp.ReasonPhrase}";
                    Items = new();
                    TotalCount = 0;
                    LookupDisplayMap = new();
                    return;
                }

                var resp = await apiResp.Content.ReadFromJsonAsync<PagedQueryResult>();
                Items = resp?.data ?? new();
                TotalCount = resp?.totalCount ?? 0;
                LookupDisplayMap = resp?.lookupMapData ?? new();
            }
            catch (HttpRequestException ex)
            {
                ViewData["LoadError"] = $"查詢 API 例外：{ex.Message}";
                Items = new();
                TotalCount = 0;
                LookupDisplayMap = new();
            }
        }

        private async Task<string?> ResolveRealTableNameAsync(string dictTableName)
        {
            var cs = _ctx.Database.GetConnectionString();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            const string sql = @"
SELECT TOP 1 ISNULL(NULLIF(RealTableName,''), TableName) AS ActualName
  FROM CURdTableName WITH (NOLOCK)
 WHERE TableName = @tbl";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@tbl", dictTableName ?? string.Empty);

            var result = await cmd.ExecuteScalarAsync();
            return result == null || result == DBNull.Value ? null : result.ToString();
        }

        public class PagedQueryResult
        {
            [JsonPropertyName("totalCount")]
            public int totalCount { get; set; }

            [JsonPropertyName("data")]
            public List<Dictionary<string, object?>>? data { get; set; }

            [JsonPropertyName("lookupMapData")]
            public Dictionary<string, Dictionary<string, string>>? lookupMapData { get; set; }
        }

        public class FilterItem
        {
            public string Field { get; set; } = "";
            public string Op { get; set; } = "";
            public string Value { get; set; } = "";
        }
    }
}
