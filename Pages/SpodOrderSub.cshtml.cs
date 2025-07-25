using Microsoft.AspNetCore.Mvc.RazorPages;
using PcbErpApi.Models;
using static SpodOrdersModel;

/// <summary>
/// SPOdOrderSub 單身頁面的 PageModel，繼承共用樣板 TableDetailModel
/// </summary>
public class SpodOrderSubModel : TableDetailModel<SpodOrderSub>
{
    #region 單頭/單身資料屬性

    // 單頭資料（API 回傳 Dictionary 格式）
    public Dictionary<string, object>? HeaderData { get; set; }

    // 單頭欄位設定清單（含位置與顯示設定）
    public List<TableFieldViewModel>? HeaderTableFields { get; set; }

    // 單頭欄位的 lookup 對應資料（每個 PaperNum 對應一份對照表）
    public Dictionary<string, Dictionary<string, string>> HeaderLookupDisplayMap { get; set; } = new();

    #endregion

    #region 建構子

    /// <summary>
    /// 注入 HttpClient 與欄位服務
    /// </summary>
    public SpodOrderSubModel(IHttpClientFactory httpClientFactory, ITableDictionaryService dictService)
        : base(httpClientFactory, dictService) { }

    #endregion

    #region 覆寫樣板所需屬性

    // 單身資料表名稱
    public override string TableName => "SPOdOrderSub";

    // 單身 API 相對路徑
    public override string ApiDetailUrl => "/api/SpodOrderSub";

    // 單頭資料表名稱
    public override string HeaderTableName => "SPOdOrderMain";

    #endregion

    #region 主要頁面載入邏輯

    /// <summary>
    /// Razor Page 的 OnGet，載入單頭與單身資料，並產生 lookup 對照表
    /// </summary>
    public async Task OnGetAsync(string PaperNum)
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";

        // Step 1：取得單頭資料
        HeaderData = await _httpClient.GetFromJsonAsync<Dictionary<string, object>>(
            $"{baseUrl}/api/SPOdOrderMain/{PaperNum}"
        );

        // Step 2：取得單頭欄位設定（僅取 Visible 的欄位）
        var headerFieldDicts = _dictService.GetFieldDict("SPOdOrderMain", typeof(SpodOrderMain));
        HeaderTableFields = headerFieldDicts
            .Where(x => x.Visible == 1)
            .OrderBy(x => x.SerialNum)
            .Select(x => new TableFieldViewModel
            {
                FieldName = x.FieldName,
                DisplayLabel = x.DisplayLabel,
                SerialNum = x.SerialNum ?? 0,
                Visible = true,
                iFieldWidth = x.iFieldWidth ?? 160,
                iFieldHeight = x.iFieldHeight ?? 22,
                iFieldTop = x.iFieldTop ?? 0,
                iFieldLeft = x.iFieldLeft ?? 0,
                iShowWhere = x.iShowWhere ?? 0
            }).ToList();

        // Step 3：呼叫樣板方法 FetchDataAsync 載入單身資料與 lookup map
        await FetchDataAsync(PaperNum);

        // Step 4：單頭區的 lookup 資料轉換（顯示用名稱）
        var headerLookupMaps = _dictService.GetOCXLookups("SPOdOrderMain");

        // 建立單頭欄位的 lookup 顯示字典（以 PaperNum 作為 key）
        var headerKey = PaperNum;
        HeaderLookupDisplayMap[headerKey] = new Dictionary<string, string>();

        // 將每個欄位的實際值轉為對應的顯示名稱
        foreach (var map in headerLookupMaps)
        {
            if (HeaderData != null && HeaderData.TryGetValue(map.KeySelfName, out var keyValueObj))
            {
                var keyValue = keyValueObj?.ToString();
                if (!string.IsNullOrEmpty(keyValue) &&
                    map.LookupValues.TryGetValue(keyValue, out var display))
                {
                    HeaderLookupDisplayMap[headerKey][map.FieldName] = display;
                }
            }
        }

        // 將 lookup map 提供給前端 Razor View 使用
        ViewData["HeaderLookupDisplayMap"] = HeaderLookupDisplayMap;
        ViewData["HeaderLookupMap"] = HeaderLookupDisplayMap.ContainsKey(headerKey)
            ? HeaderLookupDisplayMap[headerKey]
            : new Dictionary<string, string>();
    }

    #endregion
}
