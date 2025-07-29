// /Pages/Shared/TableListModel.cs
using Microsoft.AspNetCore.Mvc; // 導入 MVC 相關功能
using Microsoft.AspNetCore.Mvc.RazorPages; // 導入 Razor Pages
using PcbErpApi.Helpers;
using PcbErpApi.Models; // 導入專案模型
using System.Net.Http.Json; // 提供 HttpClient JSON 擴充
using System.Reflection; // 反射功能

public abstract class TableListModel<T> : PageModel where T : class, new() // 泛型抽象類別，限制 T 為可實例化的類別
{ // 開始類別定義
    protected readonly HttpClient _httpClient; // 供 API 呼叫的 HttpClient
    protected readonly ITableDictionaryService _dictService; // 取得欄位設定的服務

    public TableListModel(IHttpClientFactory httpClientFactory, ITableDictionaryService dictService) // 建構子注入依賴
    { // 建構子開始
        _httpClient = httpClientFactory.CreateClient(); // 建立 HttpClient 實例
        _dictService = dictService; // 儲存欄位字典服務
    } // 建構子結束

    public Dictionary<string, Dictionary<string, string>> LookupDisplayMap { get; set; } = new();

    public List<T> Items { get; set; } = new(); // 目前頁面的資料集合
    public int PageSize { get; set; } = 50; // 每頁筆數預設為 50
    public int PageNumber { get; set; } = 1; // 目前頁碼，預設 1
    public int TotalCount { get; set; } // 總資料筆數
    public int TotalPages => (int)Math.Ceiling((TotalCount) / (double)PageSize); // 計算總頁數
    public List<CURdTableField> FieldDictList { get; set; } // 欄位定義清單
    public List<TableFieldViewModel> TableFields { get; set; } = new(); // 供表格顯示的欄位

    public abstract string TableName { get; } // 對應資料表名稱
    public virtual string ApiPagedUrl => $"/api/{TableName}/paged"; // 取得分頁 API 路徑

    public virtual async Task OnGetAsync([FromQuery(Name = "page")] int? page) // GET 處理方法
    { // 方法開始
        PageNumber = page ?? 1; // 若參數為空則使用預設頁碼
        var baseUrl = $"{Request.Scheme}://{Request.Host}"; // 組成 API 基底網址
        var apiUrl = $"{baseUrl}{ApiPagedUrl}?page={PageNumber}&pageSize={PageSize}"; // 完整 API 路徑
        var resp = await _httpClient.GetFromJsonAsync<ApiResult>(apiUrl); // 呼叫 API 並解析回傳

        Items = resp?.data ?? new List<T>(); // API 回傳的資料集合
        TotalCount = resp?.totalCount ?? 0; // API 回傳的總筆數

        FieldDictList = _dictService.GetFieldDict(TableName, typeof(T)); // 取得欄位字典資料
        TableFields = FieldDictList
            .Where(x => x.Visible == 1)
            .OrderBy(x => x.SerialNum)
            .GroupBy(x => x.FieldName) // <<<<<<<<<<<<<<<<<<< 去除重複 FieldName
            .Select(g => {
                var x = g.First(); // 同名只取一個
                return new TableFieldViewModel
                {
                    FieldName = x.FieldName,
                    DisplayLabel = x.DisplayLabel,
                    SerialNum = x.SerialNum ?? 0,
                    Visible = x.Visible == 1,
                    iShowWhere = x.iShowWhere,
                    DataType = x.DataType,
                    FormatStr = x.FormatStr,
                    LookupResultField= x.LookupResultField
                };
            }).ToList();

        ViewData["Fields"] = TableFields;

        var lookupMaps = _dictService.GetOCXLookups(TableName);

            // 如果只用 PaperNum 當 key
            LookupDisplayMap = LookupDisplayHelper.BuildLookupDisplayMap(
                Items,
                lookupMaps,
                item => typeof(T).GetProperty("PaperNum")?.GetValue(item)?.ToString() ?? ""
            );

            // 或者複合主鍵（如 PaperNum + Item）:
            // LookupDisplayMap = LookupDisplayHelper.BuildLookupDisplayMap(
            //     Items,
            //     lookupMaps,
            //     item => $"{typeof(T).GetProperty("PaperNum")?.GetValue(item)}_{typeof(T).GetProperty("Item")?.GetValue(item)}"
            // );

            ViewData["LookupDisplayMap"] = LookupDisplayMap;


    } // 方法結束

    public class ApiResult // 對應 API 回傳格式
    { // 開始類別
        public int totalCount { get; set; } // 總筆數欄位
        public List<T>? data { get; set; } // 資料內容
    } // 結束類別

    



}
