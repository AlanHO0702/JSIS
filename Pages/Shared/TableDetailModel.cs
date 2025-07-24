using Microsoft.AspNetCore.Mvc.RazorPages;
using PcbErpApi.Helpers;
using PcbErpApi.Models;
using System.Net.Http.Json;
using System.Reflection;

public abstract class TableDetailModel<T> : PageModel where T : class, new()
{
    protected readonly HttpClient _httpClient;
    protected readonly ITableDictionaryService _dictService;

    public Dictionary<string, Dictionary<string, string>> LookupDisplayMap { get; set; } = new();



    public TableDetailModel(IHttpClientFactory httpClientFactory, ITableDictionaryService dictService)
    {
        _httpClient = httpClientFactory.CreateClient();
        _dictService = dictService;
    }

    public List<T> Items { get; set; } = new();
    public List<CURdTableField> FieldDictList { get; set; }
    public List<TableFieldViewModel> TableFields { get; set; } = new();
    public Dictionary<string, object> HeaderData { get; set; }
    public List<TableFieldViewModel> HeaderTableFields { get; set; }
    public List<(int TabNum, string TabTitle)> FieldTabs { get; set; } = new();


    public abstract string ApiDetailUrl { get; }
    public virtual Dictionary<string, string>? ApiQueryParameters => null;
    public string MasterKey { get; set; }
    public abstract string TableName { get; }

    // ⚠️ 這裡才是 protected method，寫在類別最外層
    protected async Task FetchDataAsync(string masterKey)
    {
        MasterKey = masterKey;

        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var url = $"{baseUrl}{ApiDetailUrl}";

        if (ApiQueryParameters != null)
        {
            var query = string.Join("&", ApiQueryParameters.Select(p => $"{p.Key}={p.Value}"));
            url = $"{url}?{query}";
        }
        else if (!string.IsNullOrEmpty(masterKey))
        {
            url = $"{url}?{GetDefaultMasterKeyName()}={masterKey}";
        }

        Items = await _httpClient.GetFromJsonAsync<List<T>>(url) ?? new();

        FieldDictList = _dictService.GetFieldDict(TableName, typeof(T));
        TableFields = FieldDictList
            .Where(x => x.Visible == 1)
            .OrderBy(x => x.SerialNum)
            .Select(x => new TableFieldViewModel
            {
                FieldName = x.FieldName,
                DisplayLabel = x.DisplayLabel,
                SerialNum = x.SerialNum ?? 0,
                Visible = x.Visible == 1
            }).ToList();

        FieldTabs = FieldDictList
        .Where(x => x.Visible == 1 && (x.iShowWhere ?? 0) >= 0)
        .Select(x => x.iShowWhere ?? 0)
        .Distinct()
        .OrderBy(x => x)
        .Select(i => (i, i == 0 ? "主頁" : $"分頁{i}"))
        .ToList();

        HeaderTableFields = FieldDictList
        .Where(x => x.Visible == 1)
        .OrderBy(x => x.SerialNum)
        .Select(x => new TableFieldViewModel
        {
            FieldName = x.FieldName,
            DisplayLabel = x.DisplayLabel,
            iFieldWidth = x.iFieldWidth,
            iFieldHeight = x.iFieldHeight,
            iFieldTop = x.iFieldTop,
            iFieldLeft = x.iFieldLeft,
            iShowWhere = x.iShowWhere
        }).ToList();

        var lookupMaps = _dictService.GetOCXLookups(TableName);

       // --- 明細 Mapping（舊的）---
           foreach (var item in Items)
            {
                // 取 PaperNum、Item
                var paperNum = typeof(T).GetProperty("PaperNum")?.GetValue(item)?.ToString();
                var itemNo   = typeof(T).GetProperty("Item")?.GetValue(item)?.ToString();

                var rowKey = $"{paperNum}_{itemNo}";  // ← 改成這個
                if (string.IsNullOrEmpty(rowKey)) continue;
                if (!LookupDisplayMap.ContainsKey(rowKey))
                    LookupDisplayMap[rowKey] = new Dictionary<string, string>();

                foreach (var map in lookupMaps)
                {
                    var fieldProp = typeof(T).GetProperty(map.KeySelfName);
                    var keyValue = fieldProp?.GetValue(item)?.ToString();
                    if (!string.IsNullOrEmpty(keyValue) && map.LookupValues.TryGetValue(keyValue, out var display))
                    {
                        LookupDisplayMap[rowKey][map.FieldName] = display;
                    }
                }
            }

        // --- 新增：單頭（HeaderData）Mapping ---
        var headerLookupMaps = _dictService.GetOCXLookups("SPOdOrderMain");
        var headerLookupDict = LookupDisplayHelper.BuildHeaderLookupMap(HeaderData, headerLookupMaps);
        // ➜ 給 View 用
        ViewData["LookupDisplayMap"] = LookupDisplayMap; 
    }

    public virtual string GetDefaultMasterKeyName() => "PaperNum";
}
