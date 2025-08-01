// /Pages/Shared/TableListModel.cs
using Microsoft.AspNetCore.Mvc; // 導入 MVC 相關功能
using Microsoft.AspNetCore.Mvc.RazorPages; // 導入 Razor Pages
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Helpers;
using PcbErpApi.Models; // 導入專案模型
using System.Net.Http.Json; // 提供 HttpClient JSON 擴充
using System.Reflection;
using System.Text;
using System.Text.Json;
using static PcbErpApi.Helpers.DynamicQueryHelper; // 反射功能

public abstract class TableListModel<T> : PageModel where T : class, new() // 泛型抽象類別，限制 T 為可實例化的類別
{ // 開始類別定義
    protected readonly HttpClient _httpClient; // 供 API 呼叫的 HttpClient
    protected readonly ITableDictionaryService _dictService; // 取得欄位設定的服務
    private readonly PcbErpContext _context;
    private readonly ILogger<TableListModel<T>> _logger;

    public TableListModel(IHttpClientFactory httpClientFactory, ITableDictionaryService dictService, PcbErpContext context, ILogger<TableListModel<T>> logger) // 建構子注入依賴
    { // 建構子開始
        _httpClient = httpClientFactory.CreateClient("MyApiClient"); // 建立 HttpClient 實例
        _dictService = dictService; // 儲存欄位字典服務
        _context = context;
        _logger = logger;
    } // 建構子結束

    public Dictionary<string, Dictionary<string, string>> LookupDisplayMap { get; set; } = new();

    public List<T> Items { get; set; } = new List<T>();
    public List<QueryFieldViewModel> QueryFields { get; set; } = new();
    public int PageSize { get; set; } = 50; // 每頁筆數預設為 50
    public int PageNumber { get; set; } = 1; // 目前頁碼，預設 1
    public int TotalCount { get; set; } // 總資料筆數
    public int TotalPages => (int)Math.Ceiling((TotalCount) / (double)PageSize); // 計算總頁數
    public List<CURdTableField> FieldDictList { get; set; } // 欄位定義清單
    public List<TableFieldViewModel> TableFields { get; set; } = new(); // 供表格顯示的欄位

    public abstract string TableName { get; } // 對應資料表名稱
    public virtual string ApiPagedUrl => $"/api/{TableName}"; // 取得分頁 API 路徑

    public virtual async Task OnGetAsync(string paperNum, int? page, int? pageSize)
    {
        PageNumber = page ?? 1;
        PageSize = pageSize ?? 50;

        var filters = new List<QueryParam>
        {
            new QueryParam { Field = "PaperNum", Op = QueryOp.Contains, Value = paperNum ?? "" }
        };
        // 1. 取得查詢欄位設定（本地資料庫查詢，仍然可以保留）
        QueryFields = _context.CURdPaperSelected
            .Where(x => x.TableName == TableName && x.IVisible == 1)
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

        // 2. 取得欄位定義（本地字典服務）
        FieldDictList = _dictService.GetFieldDict(TableName, typeof(T));
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
                    Visible = x.Visible == 1,
                    iShowWhere = x.iShowWhere,
                    DataType = x.DataType,
                    FormatStr = x.FormatStr,
                    LookupResultField = x.LookupResultField
                };
            }).ToList();
        ViewData["Fields"] = TableFields;

        // 3. 呼叫 API 取得分頁資料
            var requestObj = new
            {
                filters = new List<object>
                {
                    new { Field = "PaperNum", Op = "Contains", Value = paperNum ?? "" },
                    new { Field = "page", Op = "Equal", Value = PageNumber.ToString() },
                    new { Field = "pageSize", Op = "Equal", Value = PageSize.ToString() }
                }
            };

        var resp = await _httpClient.PostAsJsonAsync(ApiPagedUrl + "/pagedQuery", requestObj);

        if (resp.IsSuccessStatusCode)
        {
            var json = await resp.Content.ReadAsStringAsync();
            // 用 System.Text.Json 反序列化成 ApiResult<T>
            var result = JsonSerializer.Deserialize<ApiResult>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Items = result?.data ?? new List<T>();
            TotalCount = result?.totalCount ?? 0;
        }
        else
        {
            // 如果 API 呼叫失敗，可以處理錯誤或直接查空資料
            Items = new List<T>();
            TotalCount = 0;
        }

        // 4. 產生 Lookup 顯示資料（本地字典服務）
        var lookupMaps = _dictService.GetOCXLookups(TableName);
        LookupDisplayMap = LookupDisplayHelper.BuildLookupDisplayMap(
            Items,
            lookupMaps,
            item => typeof(T).GetProperty("PaperNum")?.GetValue(item)?.ToString() ?? ""
        );
        ViewData["LookupDisplayMap"] = LookupDisplayMap;

        // 5. 設定分頁資料
        ViewData["PaginationVm"] = new PaginationModel
        {
            PageNumber = PageNumber,
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize),
            RouteUrl = $"/{TableName}"
        };
    }


    public class ApiResult // 對應 API 回傳格式
    { // 開始類別
        public int totalCount { get; set; } // 總筆數欄位
        public List<T>? data { get; set; } // 資料內容
    } // 結束類別
    

}
