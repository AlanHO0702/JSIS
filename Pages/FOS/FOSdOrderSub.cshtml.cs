using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using PcbErpApi.Data;
using PcbErpApi.Models;
using PcbErpApi.Helpers;

/// <summary>
/// FOSdOrderSub 單身頁面的 PageModel，繼承共用樣板 TableDetailModel
/// </summary>
public class FOSdOrderSubModel : TableDetailModel<FosdOrderSub>
{
    private const string DynamicItemId = "FOS00002"; // 委外出廠單 ItemId
    private readonly PcbErpContext _ctx;

    #region 單頭/單身資料屬性

    // 單頭資料（API 回傳 Dictionary 格式）
    public new Dictionary<string, object>? HeaderData { get; set; }

    // 單頭欄位設定清單（含位置與顯示設定）
    public new List<TableFieldViewModel>? HeaderTableFields { get; set; }

    // 單頭欄位的 lookup 對應資料
    public Dictionary<string, Dictionary<string, string>> HeaderLookupDisplayMap { get; set; } = new();

    // 自訂按鈕資料
    public List<ItemCustButtonRow> CustomButtons { get; set; } = new();

    // ItemId 供 ActionRail 使用
    public string ItemId => DynamicItemId;

    #endregion

    #region 建構子

    public FOSdOrderSubModel(IHttpClientFactory httpClientFactory, PcbErpContext context, ITableDictionaryService dictService)
        : base(httpClientFactory, context, dictService)
    {
        _ctx = context;
    }

    #endregion

    #region 覆寫樣板所需屬性

    // 單身資料表名稱
    public override string TableName => "FOSdOrderSub";

    // 單頭資料表名稱
    public override string HeaderTableName => "FOSdOrderMain";

    // 單身 API 相對路徑
    public override string ApiDetailUrl => "/api/FOSdOrderSub";

    #endregion

    #region OnGetAsync

    public async Task<IActionResult> OnGetAsync(string paperNum)
    {
        MasterKey = paperNum;

        // 1. 載入單身資料
        await FetchDataAsync(paperNum);

        // 2. 載入單頭資料
        await LoadHeaderDataAsync(paperNum);

        // 3. 載入自訂按鈕
        CustomButtons = await LoadCustomButtonsAsync(DynamicItemId);

        return Page();
    }

    #endregion

    #region 載入單頭資料

    private async Task LoadHeaderDataAsync(string paperNum)
    {
        try
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var headerApiUrl = $"{baseUrl}/api/FOSdOrderMain/{Uri.EscapeDataString(paperNum)}";

            var headerResp = await _httpClient.GetFromJsonAsync<Dictionary<string, object>>(headerApiUrl);
            HeaderData = headerResp ?? new Dictionary<string, object>();

            // 單頭欄位設定
            var headerFieldDict = _dictService.GetFieldDict("FOSdOrderMain", typeof(FosdOrderMain));

            HeaderTableFields = headerFieldDict
                .Where(f => f.Visible == 1)
                .OrderBy(f => f.SerialNum ?? 0)
                .Select(f => new TableFieldViewModel
                {
                    FieldName = f.FieldName,
                    DisplayLabel = f.DisplayLabel,
                    SerialNum = f.SerialNum ?? 0,
                    Visible = f.Visible == 1,
                    iShowWhere = f.iShowWhere,
                    DataType = f.DataType,
                    FormatStr = f.FormatStr,
                    LookupResultField = f.LookupResultField,
                    ReadOnly = f.ReadOnly,
                    DisplaySize = f.DisplaySize
                }).ToList();

            ViewData["HeaderData"] = HeaderData;
            ViewData["HeaderTableFields"] = HeaderTableFields;

            // Lookup 對應
            var headerLookups = _dictService.GetOCXLookups("FOSdOrderMain");
            if (HeaderData != null)
            {
                var headerList = new List<Dictionary<string, object>> { HeaderData };
                HeaderLookupDisplayMap = LookupDisplayHelper.BuildLookupDisplayMap(
                    headerList,
                    headerLookups,
                    item => item.TryGetValue("PaperNum", out var val) ? val?.ToString() ?? "" : ""
                );
            }
            ViewData["HeaderLookupDisplayMap"] = HeaderLookupDisplayMap;
        }
        catch (Exception ex)
        {
            ViewData["HeaderLoadError"] = ex.Message;
        }
    }

    #endregion

    #region 載入自訂按鈕

    private async Task<List<ItemCustButtonRow>> LoadCustomButtonsAsync(string itemId)
    {
        var list = new List<ItemCustButtonRow>();
        var cs = _ctx.Database.GetConnectionString();
        await using var conn = new SqlConnection(cs);
        await conn.OpenAsync();

        var sql = @"
SELECT ItemId, SerialNum, ButtonName,
       CustCaption, CustHint,
       bVisible, bNeedNum, bNeedInEdit, DesignType,
       OCXName, CoClassName, SpName, ExecSpName,
       SearchTemplate, MultiSelectDD, ReplaceExists, DialogCaption, AllowSelCount
  FROM CURdOCXItemCustButton WITH (NOLOCK)
 WHERE ItemId = @itemId
 ORDER BY SerialNum, ButtonName;";

        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@itemId", itemId ?? string.Empty);

        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            var visible = TryToInt(rd["bVisible"]);
            if (visible.HasValue && visible.Value != 1) continue;

            var buttonName = rd["ButtonName"]?.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(buttonName)) continue;

            var caption = rd["CustCaption"]?.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(caption)) caption = buttonName;

            list.Add(new ItemCustButtonRow
            {
                ItemId = rd["ItemId"]?.ToString() ?? string.Empty,
                SerialNum = TryToInt(rd["SerialNum"]),
                ButtonName = buttonName,
                Caption = caption,
                Hint = rd["CustHint"]?.ToString() ?? string.Empty,
                OCXName = rd["OCXName"]?.ToString() ?? string.Empty,
                CoClassName = rd["CoClassName"]?.ToString() ?? string.Empty,
                SpName = rd["SpName"]?.ToString() ?? string.Empty,
                ExecSpName = rd["ExecSpName"]?.ToString() ?? string.Empty,
                SearchTemplate = rd["SearchTemplate"]?.ToString() ?? string.Empty,
                MultiSelectDD = rd["MultiSelectDD"]?.ToString() ?? string.Empty,
                ReplaceExists = TryToInt(rd["ReplaceExists"]),
                DialogCaption = rd["DialogCaption"]?.ToString() ?? string.Empty,
                AllowSelCount = TryToInt(rd["AllowSelCount"]),
                bNeedNum = TryToInt(rd["bNeedNum"]),
                bNeedInEdit = TryToInt(rd["bNeedInEdit"]),
                DesignType = TryToInt(rd["DesignType"])
            });
        }

        return list;
    }

    private static int? TryToInt(object? val)
    {
        if (val == null || val == DBNull.Value) return null;
        if (val is int i) return i;
        if (int.TryParse(val.ToString(), out var n)) return n;
        return null;
    }

    #endregion

    #region ItemCustButtonRow class

    public class ItemCustButtonRow
    {
        public string ItemId { get; set; } = string.Empty;
        public int? SerialNum { get; set; }
        public string ButtonName { get; set; } = string.Empty;
        public string Caption { get; set; } = string.Empty;
        public string Hint { get; set; } = string.Empty;
        public string OCXName { get; set; } = string.Empty;
        public string CoClassName { get; set; } = string.Empty;
        public string SpName { get; set; } = string.Empty;
        public string ExecSpName { get; set; } = string.Empty;
        public string SearchTemplate { get; set; } = string.Empty;
        public string MultiSelectDD { get; set; } = string.Empty;
        public int? ReplaceExists { get; set; }
        public string DialogCaption { get; set; } = string.Empty;
        public int? AllowSelCount { get; set; }
        public int? bNeedNum { get; set; }
        public int? bNeedInEdit { get; set; }
        public int? DesignType { get; set; }
    }

    #endregion
}
