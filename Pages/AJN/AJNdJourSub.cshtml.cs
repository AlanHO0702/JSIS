using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PcbErpApi.Data;
using PcbErpApi.Models;
using static SpodOrdersModel;

/// <summary>
/// SPOdOrderSub 單身頁面的 PageModel，繼承共用樣板 TableDetailModel
/// </summary>
public class AJNdJourSubModel : TableDetailModel<AjndJourSub>
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
    public AJNdJourSubModel(IHttpClientFactory httpClientFactory, PcbErpContext context,ITableDictionaryService dictService)
        : base(httpClientFactory, context, dictService) { }

    #endregion

    #region 覆寫樣板所需屬性

    // 單身資料表名稱
    public override string TableName => "AJNdJourSub";

    // 單身 API 相對路徑
    public override string ApiDetailUrl => "/api/AJNdJourSub";

    // 單頭資料表名稱
    public override string HeaderTableName => "AJNdJourMain";

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
            $"{baseUrl}/api/AJNdJourMain/{PaperNum}"
        );

        // Step 1.5：轉換 HeaderData 中的 JsonElement 成實際型別
        if (HeaderData != null)
        {
            var keys = HeaderData.Keys.ToList();
            foreach (var key in keys)
            {
                if (HeaderData[key] is JsonElement je)
                {
                    object? realValue = null;
                    switch (je.ValueKind)
                    {
                        case JsonValueKind.String:
                            if (DateTime.TryParse(je.GetString(), out var dt))
                                realValue = dt;
                            else
                                realValue = je.GetString();
                            break;
                        case JsonValueKind.Number:
                            if (je.TryGetInt32(out var i32))
                                realValue = i32;
                            else if (je.TryGetInt64(out var i64))
                                realValue = i64;
                            else if (je.TryGetDecimal(out var dec))
                                realValue = dec;
                            else
                                realValue = je.GetDouble();
                            break;
                        case JsonValueKind.True:
                        case JsonValueKind.False:
                            realValue = je.GetBoolean();
                            break;
                        case JsonValueKind.Null:
                            realValue = null;
                            break;
                        default:
                            realValue = je.ToString();
                            break;
                    }

                    HeaderData[key] = realValue; // 替換成轉換後的值
                }
            }

            // ✅【這裡加手動預設值補值，只補沒資料的欄位】
            if (!HeaderData.ContainsKey("RateToNT") || HeaderData["RateToNT"] == null)
                HeaderData["RateToNT"] = 1;
         
        }

        // Step 2：取得單頭欄位設定（僅取 Visible 的欄位）
        var headerFieldDicts = _dictService.GetFieldDict("AJNdJourMain", typeof(AjndJourMain));
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
                iShowWhere = x.iShowWhere ?? 0,
                DataType = x.DataType,
                FormatStr = x.FormatStr,
                LookupTable = x.LookupTable,
                LookupKeyField = x.LookupKeyField,
                LookupResultField = x.LookupResultField  
            }).ToList();

        // Step 3：呼叫樣板方法 FetchDataAsync 載入單身資料與 lookup map
        await FetchDataAsync(PaperNum);

        // Step 4：單頭區的 lookup 資料轉換（顯示用名稱）
        var headerLookupMaps = _dictService.GetOCXLookups("AJNdJourMain");

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
