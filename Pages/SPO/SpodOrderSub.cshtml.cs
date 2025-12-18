using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using PcbErpApi.Data;
using PcbErpApi.Models;
using static SpodOrdersModel;

/// <summary>
/// SPOdOrderSub 單身頁面的 PageModel，繼承共用樣板 TableDetailModel
/// </summary>
public class SpodOrderSubModel : TableDetailModel<SpodOrderSub>
{
    private const string DynamicItemId = "SA000002"; // 銷售訂單 ItemId（導向動態單據樣板）
    private readonly PcbErpContext _ctx;

    #region 單頭/單身資料屬性

    // 單頭資料（API 回傳 Dictionary 格式）
    public new Dictionary<string, object>? HeaderData { get; set; }

    // 單頭欄位設定清單（含位置與顯示設定）
    public new List<TableFieldViewModel>? HeaderTableFields { get; set; }

    // 單頭欄位的 lookup 對應資料（每個 PaperNum 對應一份對照表）
    public Dictionary<string, Dictionary<string, string>> HeaderLookupDisplayMap { get; set; } = new();

    #endregion

    #region 建構子

    /// <summary>
    /// 注入 HttpClient 與欄位服務
    /// </summary>
    public SpodOrderSubModel(IHttpClientFactory httpClientFactory, PcbErpContext context,ITableDictionaryService dictService)
        : base(httpClientFactory, context, dictService)
    {
        _ctx = context;
    }

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
    public async Task<IActionResult> OnGetAsync(string PaperNum)
    {
        // 銷售訂單頁改走動態單據樣板（支援 DETAIL2+ 多頁籤），自訂按鈕由 PaperDetailModel 依 itemId 套用
        if (!string.IsNullOrWhiteSpace(PaperNum))
            return Redirect($"/DynamicTemplate/Paper/{DynamicItemId}/{Uri.EscapeDataString(PaperNum)}");

        var baseUrl = $"{Request.Scheme}://{Request.Host}";

        // Step 1：取得單頭資料
        HeaderData = await _httpClient.GetFromJsonAsync<Dictionary<string, object>>(
            $"{baseUrl}/api/SPOdOrderMain/{PaperNum}"
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

        // Step 6：若有設定 DETAIL2+，於單身下方顯示多頁籤（僅顯示，不影響原本單身編輯/儲存）
        await BuildDetailTabsAsync(PaperNum);

        return Page();
    }

    #endregion

    private async Task BuildDetailTabsAsync(string paperNum)
    {
        ViewData["PaperNum"] = paperNum ?? string.Empty;
        ViewData["MultiTabAllowEdit"] = false;

        var resolvedItemId = await ResolveItemIdForMultiDetailAsync();
        if (string.IsNullOrWhiteSpace(resolvedItemId))
        {
            ViewData["Tabs"] = Array.Empty<object>();
            ViewData["TabFieldDicts"] = new Dictionary<string, Dictionary<string, string>>();
            ViewData["MultiTabDebug"] = $"resolveItemId=empty; detailTable={TableName}; masterTable={HeaderTableName}";
            return;
        }

        var setups = await _ctx.CurdOcxtableSetUp.AsNoTracking()
            .Where(x => x.ItemId == resolvedItemId)
            .ToListAsync();

        var details = setups
            .Where(x => (x.TableKind ?? string.Empty).StartsWith("Detail", StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => ExtractOrderIndex(x.TableKind))
            .ThenBy(x => x.TableKind)
            .ThenBy(x => x.TableName)
            .ToList();

        if (details.Count == 0)
        {
            ViewData["Tabs"] = Array.Empty<object>();
            ViewData["TabFieldDicts"] = new Dictionary<string, Dictionary<string, string>>();
            ViewData["MultiTabDebug"] = $"itemId={resolvedItemId}; detailSetups=0; rawSetups={setups.Count}";
            return;
        }

        // 只顯示 DETAIL2+（DETAIL1 就是本頁原本的 SPOdOrderSub 單身表格）
        var beforeFilterCount = details.Count;
        details = details
            .Where(d => !string.Equals(d.TableName, TableName, StringComparison.OrdinalIgnoreCase))
            .Where(d => ExtractOrderIndex(d.TableKind) != 1)
            .ToList();

        if (details.Count == 0)
        {
            ViewData["Tabs"] = Array.Empty<object>();
            ViewData["TabFieldDicts"] = new Dictionary<string, Dictionary<string, string>>();
            ViewData["MultiTabDebug"] = $"itemId={resolvedItemId}; detailSetups={beforeFilterCount}; afterFilter=0; note=only DETAIL1 found";
            return;
        }

        var tabs = new List<object>(details.Count);
        var tabFieldDicts = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < details.Count; i++)
        {
            var d = details[i];
            var dictTable = (d.TableName ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(dictTable)) continue;

            var n = ExtractOrderIndex(d.TableKind);
            var tabId = (n != int.MaxValue) ? $"d{n}" : $"d{i + 2}";

            var title = await ResolveDisplayLabelAsync(dictTable) ?? dictTable;
            var apiUrl = $"/api/DynamicTable/ByPaperNum?table={Uri.EscapeDataString(dictTable)}";

            tabs.Add(new { Id = tabId, Title = title, ApiUrl = apiUrl, DictTable = dictTable });

            var fields = _dictService.GetFieldDict(dictTable, typeof(object));
            tabFieldDicts[tabId] = fields
                .Where(f => f.Visible == 1)
                .GroupBy(f => f.FieldName ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .Where(g => !string.IsNullOrWhiteSpace(g.Key))
                .ToDictionary(g => g.Key, g => g.First().DisplayLabel ?? g.Key, StringComparer.OrdinalIgnoreCase);
        }

        ViewData["Tabs"] = tabs.ToArray();
        ViewData["TabFieldDicts"] = tabFieldDicts;
        ViewData["MultiTabDebug"] = $"itemId={resolvedItemId}; detailSetups={beforeFilterCount}; afterFilter={tabs.Count}";
    }

    private async Task<string?> ResolveItemIdForMultiDetailAsync()
    {
        // 優先用目前單身表（DETAIL1）反查 ItemId，比硬編 ItemId 更可靠
        var detailTableLower = (TableName ?? string.Empty).Trim().ToLowerInvariant();
        if (!string.IsNullOrWhiteSpace(detailTableLower))
        {
            var rows = await _ctx.CurdOcxtableSetUp.AsNoTracking()
                .Where(x => (x.TableName ?? string.Empty).ToLower() == detailTableLower)
                .Select(x => new { x.ItemId, x.TableKind })
                .ToListAsync();

            var hit = rows.FirstOrDefault(r => IsDetail1(r.TableKind))?.ItemId
                      ?? rows.FirstOrDefault()?.ItemId;
            if (!string.IsNullOrWhiteSpace(hit)) return hit.Trim();
        }

        // fallback: 用單頭表反查（MASTER1）
        var masterTableLower = (HeaderTableName ?? string.Empty).Trim().ToLowerInvariant();
        if (!string.IsNullOrWhiteSpace(masterTableLower))
        {
            var rows = await _ctx.CurdOcxtableSetUp.AsNoTracking()
                .Where(x => (x.TableName ?? string.Empty).ToLower() == masterTableLower)
                .Select(x => new { x.ItemId, x.TableKind })
                .ToListAsync();

            var hit = rows.FirstOrDefault(r => IsMaster1(r.TableKind))?.ItemId
                      ?? rows.FirstOrDefault()?.ItemId;
            if (!string.IsNullOrWhiteSpace(hit)) return hit.Trim();
        }

        // 最後再回到既有預設（舊版銷售訂單 ItemId）
        return "SA000002";
    }

    private static bool IsDetail1(string? tableKind)
    {
        var s = (tableKind ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(s)) return false;
        if (!s.Contains("DETAIL", StringComparison.OrdinalIgnoreCase)) return false;
        return ExtractOrderIndex(s) == 1;
    }

    private static bool IsMaster1(string? tableKind)
    {
        var s = (tableKind ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(s)) return false;
        if (!s.Contains("MASTER", StringComparison.OrdinalIgnoreCase)) return false;
        return ExtractOrderIndex(s) == 1;
    }

    private static int ExtractOrderIndex(string? tableKind)
    {
        if (string.IsNullOrWhiteSpace(tableKind)) return int.MaxValue;
        var m = System.Text.RegularExpressions.Regex.Match(tableKind, "(\\d+)$");
        return m.Success && int.TryParse(m.Groups[1].Value, out var n) ? n : int.MaxValue;
    }

    private async Task<string?> ResolveDisplayLabelAsync(string dictTableName)
    {
        var cs = _ctx.Database.GetConnectionString();
        if (string.IsNullOrWhiteSpace(cs)) return null;

        await using var conn = new SqlConnection(cs);
        await conn.OpenAsync();

        const string sql = @"
SELECT TOP 1 ISNULL(NULLIF(DisplayLabel,''), TableName) AS DisplayLabel
  FROM CURdTableName WITH (NOLOCK)
 WHERE TableName = @tbl";

        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@tbl", dictTableName ?? string.Empty);
        var result = await cmd.ExecuteScalarAsync();
        return result == null || result == DBNull.Value ? null : result.ToString();
    }
}
