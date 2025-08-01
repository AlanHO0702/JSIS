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
    public virtual string ApiPagedUrl => $"/api/{TableName}/paged"; // 取得分頁 API 路徑

    public virtual async Task OnGetAsync()
    {
        // 預設頁碼/每頁
        PageNumber = int.TryParse(Request.Query["page"], out var pg) ? pg : 1;
        PageSize = int.TryParse(Request.Query["pageSize"], out var ps) ? ps : 50;

        // 取得查詢欄位設定（cache/service 取最快）
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

        // 組查詢參數
        var queryList = new List<string>();
        foreach (var field in QueryFields)
        {
            var val = Request.Query[field.ColumnName].ToString();
            if (!string.IsNullOrWhiteSpace(val))
            {
                queryList.Add($"{field.ColumnName}={Uri.EscapeDataString(val)}");
            }
        }

        var queryStr = string.Join("&", queryList);
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var apiUrl = $"{baseUrl}{ApiPagedUrl}?page={PageNumber}&pageSize={PageSize}";
        if (!string.IsNullOrWhiteSpace(queryStr))
            apiUrl += "&" + queryStr;

        var resp = await _httpClient.GetFromJsonAsync<ApiResult>(apiUrl);

        Items = resp?.data ?? new List<T>();
        TotalCount = resp?.totalCount ?? 0;

        // 欄位、lookup設定
        FieldDictList = _dictService.GetFieldDict(TableName, typeof(T));
        TableFields = FieldDictList
            .Where(x => x.Visible == 1)
            .OrderBy(x => x.SerialNum)
            .GroupBy(x => x.FieldName)
            .Select(g => {
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

        var lookupMaps = _dictService.GetOCXLookups(TableName);
        LookupDisplayMap = LookupDisplayHelper.BuildLookupDisplayMap(
            Items,
            lookupMaps,
            item => typeof(T).GetProperty("PaperNum")?.GetValue(item)?.ToString() ?? ""
        );
        ViewData["LookupDisplayMap"] = LookupDisplayMap;
    }
    public virtual async Task<IActionResult> OnPostAsync()
    {
        foreach (var k in Request.Form.Keys)
        {
            _logger.LogInformation($"{k} = {Request.Form[k]}");
        }
        // 1. 取得分頁參數
        PageNumber = int.TryParse(Request.Form["page"], out var pg) ? pg : 1;
        PageSize = int.TryParse(Request.Form["pageSize"], out var ps) ? ps : 50;

        // 2. 取得查詢欄位設定（通常這不會變動，cache/service 較快）
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

        // 3. 組查詢參數物件（改用 POST）
                // 4. 呼叫 API（POST）
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var apiUrl = $"{baseUrl}/api/{TableName}/pagedQuery";
        var filters = new List<object>();
        foreach (var field in QueryFields)
        {
            var val = Request.Form[field.ColumnName].ToString();
            var op = Request.Form["Cond_" + field.ColumnName].ToString();
            if (!string.IsNullOrWhiteSpace(val))
            {
                filters.Add(new {
                    Field = field.ColumnName,
                    Op = op,
                    Value = val
                });
            }
        }
        filters.Add(new { Field = "page", Op = "", Value = PageNumber.ToString() });
        filters.Add(new { Field = "pageSize", Op = "", Value = PageSize.ToString() });
        var queryObj = new { filters = filters };
        var resp = await _httpClient.PostAsJsonAsync(apiUrl, queryObj);
        var result = await resp.Content.ReadFromJsonAsync<ApiResult>();

        Items = result?.data ?? new List<T>();
        TotalCount = result?.totalCount ?? 0;

        // 5. 欄位、lookup設定
        FieldDictList = _dictService.GetFieldDict(TableName, typeof(T));
        TableFields = FieldDictList
            .Where(x => x.Visible == 1)
            .OrderBy(x => x.SerialNum)
            .GroupBy(x => x.FieldName)
            .Select(g => {
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

        var lookupMaps = _dictService.GetOCXLookups(TableName);
        LookupDisplayMap = LookupDisplayHelper.BuildLookupDisplayMap(
            Items,
            lookupMaps,
            item => typeof(T).GetProperty("PaperNum")?.GetValue(item)?.ToString() ?? ""
        );
        ViewData["LookupDisplayMap"] = LookupDisplayMap;

        return Page();
    }


    public class ApiResult // 對應 API 回傳格式
    { // 開始類別
        public int totalCount { get; set; } // 總筆數欄位
        public List<T>? data { get; set; } // 資料內容
    } // 結束類別
    

}
