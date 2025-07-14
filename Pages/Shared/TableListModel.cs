// /Pages/Shared/TableListModel.cs
using Microsoft.AspNetCore.Mvc; // 導入 MVC 相關功能
using Microsoft.AspNetCore.Mvc.RazorPages; // 導入 Razor Pages
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
        TableFields = FieldDictList // 轉換為顯示用欄位
            .Where(x => x.Visible == 1) // 只取可見欄位
            .OrderBy(x => x.SerialNum) // 依序號排序
            .Select(x => new TableFieldViewModel // 轉成 ViewModel
            { // 開始設定屬性
                FieldName = x.FieldName, // 欄位名稱
                DisplayLabel = x.DisplayLabel, // 顯示標籤
                SerialNum = x.SerialNum ?? 0, // 序號，若為空給 0
                Visible = x.Visible == 1 // 是否可見
            }).ToList(); // 形成清單
    } // 方法結束

    public class ApiResult // 對應 API 回傳格式
    { // 開始類別
        public int totalCount { get; set; } // 總筆數欄位
        public List<T>? data { get; set; } // 資料內容
    } // 結束類別

    



}
