using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Helpers;
using PcbErpApi.Models;
using System.Net.Http.Json;
using System.Reflection;

/// <summary>
/// 提供表單詳情頁的共用樣板邏輯，泛型 T 為對應的資料模型
/// </summary>
public abstract class TableDetailModel<T> : PageModel where T : class, new()
{
    #region Services (建構注入)
    protected readonly HttpClient _httpClient;
    protected readonly ITableDictionaryService _dictService;
    #endregion

    #region 屬性定義

    // 主檔唯一鍵，例如 PaperNum
    public string MasterKey { get; set; }

    // 單身資料清單
    public List<T> Items { get; set; } = new();

    // 從 CURdTableField 取回的欄位設定 (包含所有欄位)
    public List<CURdTableField> FieldDictList { get; set; }

    // 單身欄位呈現設定（僅 Visible=1 且有排序）
    public List<TableFieldViewModel> TableFields { get; set; } = new();

    // 單頭資料（以 Dictionary 儲存）
    public Dictionary<string, object> HeaderData { get; set; }

    // 單頭欄位設定（含欄位位置資訊）
    public List<TableFieldViewModel> HeaderTableFields { get; set; }

    // 分頁用的標籤頁資訊 (TabNum + 標題)
    public List<(int TabNum, string TabTitle)> FieldTabs { get; set; } = new();

    // Lookup 資料轉換用的 Map
    public Dictionary<string, Dictionary<string, string>> LookupDisplayMap { get; set; } = new();

    #endregion

    #region 抽象屬性 (由子類實作)

    // API 資料來源的相對路徑
    public abstract string ApiDetailUrl { get; }

    // 資料所屬的 Table 名稱（會用來抓欄位設定）
    public abstract string TableName { get; }

    // 單頭對應的 Table 名稱（抓 header lookup 用）
    public abstract string HeaderTableName { get; }

    #endregion

    #region 可覆寫屬性

    // 如果 API 要附加參數可以覆寫這個字典
    public virtual Dictionary<string, string>? ApiQueryParameters => null;

    // 預設的主鍵欄位名稱
    public virtual string GetDefaultMasterKeyName() => "PaperNum";
    public List<QueryFieldViewModel> QueryFields { get; set; } = new();
    private readonly PcbErpContext _context;

    #endregion

    #region 建構子

    public TableDetailModel(IHttpClientFactory httpClientFactory, PcbErpContext context, ITableDictionaryService dictService)
    {
        _httpClient = httpClientFactory.CreateClient();
        _context = context;
        _dictService = dictService;
    }

    #endregion

    #region 資料載入邏輯

    /// <summary>
    /// 載入單身資料、欄位設定、Lookup 對照表等
    /// </summary>
    protected async Task FetchDataAsync(string masterKey)
    {
        MasterKey = masterKey;

        // 拼接 API URL
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var url = $"{baseUrl}{ApiDetailUrl}";

        // 若有額外查詢參數則帶入
        if (ApiQueryParameters != null)
        {
            var query = string.Join("&", ApiQueryParameters.Select(p => $"{p.Key}={p.Value}"));
            url = $"{url}?{query}";
        }
        else if (!string.IsNullOrEmpty(masterKey))
        {
            url = $"{url}?{GetDefaultMasterKeyName()}={masterKey}";
        }

        // 呼叫 API 取得單身資料
        Items = await _httpClient.GetFromJsonAsync<List<T>>(url) ?? new();

        // 取得欄位設定
        FieldDictList = _dictService.GetFieldDict(TableName, typeof(T));

        // 單身欄位（僅取 Visible=1，並依 SerialNum 排序）
        TableFields = FieldDictList
            .Where(x => x.Visible == 1)
            .OrderBy(x => x.SerialNum)
            .Select(x => new TableFieldViewModel
            {
                FieldName = x.FieldName,
                DisplayLabel = x.DisplayLabel,
                SerialNum = x.SerialNum ?? 0,
                Visible = true,
                LookupResultField = x.LookupResultField,
                DataType = x.DataType,
                FormatStr = x.FormatStr,
                ComboStyle = x.ComboStyle
            }).ToList();

        // 分頁資訊：根據 iShowWhere 分群
        FieldTabs = FieldDictList
            .Where(x => x.Visible == 1 && (x.iShowWhere ?? 0) >= 0)
            .Select(x => x.iShowWhere ?? 0)
            .Distinct()
            .OrderBy(x => x)
            .Select(i => (i, i == 0 ? "主頁" : $"分頁{i}"))
            .ToList();

        // 單頭欄位設定（含位置資訊）
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
                iShowWhere = x.iShowWhere,
                DataType = x.DataType,
                FormatStr = x.FormatStr,
                LookupTable = x.LookupTable,
                LookupKeyField = x.LookupKeyField,
                LookupResultField = x.LookupResultField,
                ComboStyle = x.ComboStyle
            }).ToList();

        // Lookup Map 資料 (單身 + 單頭)
        var lookupMaps = _dictService.GetOCXLookups(TableName);
        var headerLookupMaps = _dictService.GetOCXLookups(HeaderTableName);

        // 單身 lookup 值轉換（用 PaperNum_Item 作為 key）
        LookupDisplayMap = LookupDisplayHelper.BuildLookupDisplayMap(
            Items,
            lookupMaps,
            item => $"{typeof(T).GetProperty("PaperNum")?.GetValue(item)}_{typeof(T).GetProperty("Item")?.GetValue(item)}"
        );
        ViewData["LookupDisplayMap"] = LookupDisplayMap;

        // 單頭 lookup 值轉換
        var headerLookupDict = LookupDisplayHelper.BuildHeaderLookupMap(HeaderData, headerLookupMaps);
        ViewData["HeaderLookupMap"] = headerLookupDict;

        var headerDisplayLabel = await _context.CurdTableNames
            .AsNoTracking()
            .Where(x => x.TableName == HeaderTableName)
            .Select(x => string.IsNullOrWhiteSpace(x.DisplayLabel) ? x.TableName : x.DisplayLabel)
            .FirstOrDefaultAsync();
        ViewData["HeaderTableDisplayLabel"] = string.IsNullOrWhiteSpace(headerDisplayLabel)
            ? HeaderTableName
            : headerDisplayLabel;
        

        // 取得查詢欄位設定（cache/service 取最快）
        QueryFields = _context.CURdPaperSelected
            .Where(x => x.TableName == HeaderTableName && x.IVisible == 1)
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
    }

    #endregion
}
