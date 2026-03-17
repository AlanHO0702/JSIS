using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PcbErpApi.Data;
using PcbErpApi.Models;
using PcbErpApi.Services;

namespace PcbErpApi.Pages.EMOdProdLayer
{
    [IgnoreAntiforgeryToken]
    public class DetailModel : PageModel
    {
        private readonly PcbErpContext _ctx;
        private readonly ITableDictionaryService _dictService;
        private readonly ILogger<DetailModel> _logger;
        private readonly IBreadcrumbService _breadcrumbService;

        private const string MasterDict  = "EMOdProdLayer";
        private const string MasterTable = "EMOdProdLayer";

        public DetailModel(
            PcbErpContext ctx,
            ITableDictionaryService dictService,
            ILogger<DetailModel> logger,
            IBreadcrumbService breadcrumbService)
        {
            _ctx = ctx;
            _dictService = dictService;
            _logger = logger;
            _breadcrumbService = breadcrumbService;
        }

        public List<KeyValuePair<string, string>> MasterKeyValues { get; private set; } = new();
        public List<DetailTab> ExtraTabs { get; private set; } = new();
        public List<MasterTab> MasterTabs { get; private set; } = new();
        public List<ItemCustButtonRow> CustomButtons { get; private set; } = new();
        public string? ActionRailPartial { get; private set; }
        public string? MasterApi { get; private set; }

        [FromQuery(Name = "mode")]
        public string? Mode { get; set; }

        [FromQuery(Name = "itemId")]
        public string? ItemId { get; set; }

        public bool IsViewOnly =>
            !string.IsNullOrEmpty(Mode)
                ? string.Equals(Mode, "view", StringComparison.OrdinalIgnoreCase)
                : false;

        public string PageTitle => IsViewOnly ? "層別資料查詢" : "EMO00059 層別資料維護";

        public async Task<IActionResult> OnGetAsync()
        {
            ViewData["Title"] = PageTitle;

            // 麵包屑
            try
            {
                var currentItemId = ItemId ?? (IsViewOnly ? "EMO00060" : "EMO00059");
                var sysItem = await _ctx.CurdSysItems.AsNoTracking()
                    .Where(x => x.ItemId == currentItemId)
                    .Select(x => new { x.SuperId })
                    .FirstOrDefaultAsync();
                if (!string.IsNullOrWhiteSpace(sysItem?.SuperId))
                    ViewData["Breadcrumbs"] = await _breadcrumbService.BuildBreadcrumbsAsync(sysItem.SuperId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Build breadcrumbs failed");
            }

            try
            {
                var masterDict = await LoadFieldDictAsync(MasterDict);

                // 主鍵：PartNum + Revision + LayerId
                var masterKeys = ExtractKeys(masterDict);
                if (masterKeys.Count == 0)
                {
                    masterKeys.Add("PartNum");
                    masterKeys.Add("Revision");
                    masterKeys.Add("LayerId");
                }

                MasterKeyValues = CollectKeyValues(masterKeys);

                if (MasterKeyValues.Count > 0)
                    MasterApi = BuildByKeysApi(MasterTable, MasterKeyValues);

                var itemId = IsViewOnly ? "EMO00060" : "EMO00059";

                // Detail 頁籤（由 DB 動態產生）
                ExtraTabs = await BuildExtraTabsAsync(itemId);
                var tabCaptions = await LoadMasterTabCaptionsAsync(itemId);
                var fieldLangs  = await LoadFieldLangsAsync(MasterDict);
                MasterTabs = BuildMasterTabs(masterDict, tabCaptions, fieldLangs);

                // Action Rail 自訂按鈕
                try
                {
                    CustomButtons = await LoadCustomButtonsAsync(itemId);
                    ActionRailPartial = (CustomButtons?.Count ?? 0) > 0
                        ? "~/Pages/Shared/_ActionRail.DynamicButtons.cshtml"
                        : "~/Pages/Shared/_ActionRail.Empty.cshtml";
                }
                catch (Exception btnEx)
                {
                    _logger.LogWarning(btnEx, "Failed to load custom buttons");
                    CustomButtons   = new List<ItemCustButtonRow>();
                    ActionRailPartial = "~/Pages/Shared/_ActionRail.Empty.cshtml";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Build EMOdProdLayer detail config failed");
                return BadRequest($"初始化失敗: {ex.Message}");
            }

            return Page();
        }

        // ── Detail 頁籤定義（從 DB CURdOCXTableSetUp 動態產生）─────────────
        private async Task<List<DetailTab>> BuildExtraTabsAsync(string itemId)
        {
            var tabs = new List<DetailTab>();
            var cs = _ctx.Database.GetConnectionString()!;
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            // 讀取 Detail1~N 的資料表設定
            const string sql = @"
SELECT s.TableName, s.TableKind, s.LocateKeys, s.OrderByField,
       ISNULL(NULLIF(n.DisplayLabel,''), s.TableName) AS DisplayLabel
  FROM CURdOCXTableSetUp s WITH (NOLOCK)
  LEFT JOIN CURdTableName n WITH (NOLOCK) ON n.TableName = s.TableName
 WHERE s.ItemId = @itemId
   AND s.TableKind LIKE 'Detail%'
 ORDER BY s.TableKind";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@itemId", itemId);
            await using var rd = await cmd.ExecuteReaderAsync();

            var seq = 0;
            while (await rd.ReadAsync())
            {
                var tableName   = rd["TableName"]?.ToString() ?? "";
                var displayLabel = rd["DisplayLabel"]?.ToString() ?? tableName;
                var locateKeys  = rd["LocateKeys"]?.ToString() ?? "";
                if (string.IsNullOrWhiteSpace(tableName)) continue;

                // 解析主鍵欄位（格式：PartNum;Revision;LayerId 或 PartNum;Revision;LayerId;...）
                var keyFields = locateKeys
                    .Split(new[] { ';', ',', '|' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(k => k.Trim())
                    .Where(k => !string.IsNullOrWhiteSpace(k))
                    .ToArray();
                if (keyFields.Length == 0)
                    keyFields = new[] { "PartNum", "Revision", "LayerId" };

                tabs.Add(new DetailTab(
                    $"detail{++seq}",
                    displayLabel,
                    tableName,
                    tableName,
                    keyFields
                ));
            }

            return tabs;
        }

        // ── Master 頁籤（由 DB CURdOCXItemOtherRule 動態產生）───────────────
        private async Task<Dictionary<int, string>> LoadMasterTabCaptionsAsync(string itemId)
        {
            var result = new Dictionary<int, string>();

            var master1Label = _ctx.CurdTableNames
                .Where(t => t.TableName == MasterTable)
                .Select(t => t.DisplayLabel)
                .FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(master1Label))
                result[1] = master1Label;

            var cs = _ctx.Database.GetConnectionString();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();
            const string sql = @"
SELECT RuleId, DLLValue
  FROM CURdOCXItemOtherRule WITH (NOLOCK)
 WHERE ItemId = @itemId
   AND RuleId LIKE 'PaperMasTb%Caption'";
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@itemId", itemId);
            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                var ruleId  = rd["RuleId"]?.ToString() ?? "";
                var caption = rd["DLLValue"]?.ToString() ?? "";
                if (string.IsNullOrWhiteSpace(caption)) continue;
                var numStr = ruleId.Replace("PaperMasTb", "").Replace("Caption", "");
                if (int.TryParse(numStr, out var n))
                    result[n] = caption;
            }
            return result;
        }

        private Task<List<CurdTableFieldLang>> LoadFieldLangsAsync(string tableName)
        {
            var tname = tableName.ToLower();
            var langs = _ctx.CurdTableFieldLangs
                .Where(l => l.LanguageId == "TW"
                    && l.TableName != null
                    && (l.TableName.ToLower() == tname
                        || l.TableName.ToLower().Replace("dbo.", "") == tname))
                .ToList();
            return Task.FromResult(langs);
        }

        private List<MasterTab> BuildMasterTabs(
            IEnumerable<CURdTableField> dict,
            Dictionary<int, string> tabCaptions,
            List<CurdTableFieldLang> fieldLangs)
        {
            var dictList = dict.Where(f => !string.IsNullOrWhiteSpace(f.FieldName)).ToList();
            var langShowWhere = fieldLangs
                .Where(l => l.IShowWhere >= 1 && !string.IsNullOrWhiteSpace(l.FieldName))
                .ToDictionary(l => l.FieldName!, l => l.IShowWhere, StringComparer.OrdinalIgnoreCase);

            var fieldShowWhere = dictList
                .Where(f => (f.Visible ?? 1) == 1)
                .Select(f =>
                {
                    var sw = langShowWhere.TryGetValue(f.FieldName!, out var v) ? v
                             : (f.iShowWhere ?? 0);
                    return (FieldName: f.FieldName!, ShowWhere: sw, SerialNum: f.SerialNum ?? 9999);
                })
                .Where(x => x.ShowWhere >= 1)
                .GroupBy(x => x.ShowWhere)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(x => x.SerialNum).Select(x => x.FieldName).ToList()
                );

            var tabs = new List<MasterTab>();
            foreach (var kv in tabCaptions.OrderBy(kv => kv.Key))
            {
                fieldShowWhere.TryGetValue(kv.Key, out var fields);
                tabs.Add(new MasterTab($"spec{kv.Key}", kv.Value, fields ?? new List<string>(), kv.Key));
            }
            return tabs;
        }

        // ── private helpers ───────────────────────────────────────────────

        private Task<List<CURdTableField>> LoadFieldDictAsync(string dictTableName)
        {
            try
            {
                return Task.FromResult(_dictService.GetFieldDict(dictTableName, typeof(EmodProdLayer)));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "GetFieldDict failed for {Table}", dictTableName);
                return Task.FromResult(new List<CURdTableField>());
            }
        }

        private static List<string> ExtractKeys(IEnumerable<CURdTableField> fields)
            => fields
                .Where(f => (f.PK ?? 0) == 1)
                .Select(f => f.FieldName)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .ToList();

        private List<KeyValuePair<string, string>> CollectKeyValues(IEnumerable<string> masterKeys)
        {
            var list = new List<KeyValuePair<string, string>>();
            foreach (var k in masterKeys)
            {
                if (Request.Query.TryGetValue(k, out var v) && !string.IsNullOrWhiteSpace(v))
                    list.Add(new KeyValuePair<string, string>(k, v.ToString()));
            }
            // fallback 確保三主鍵
            foreach (var key in new[] { "PartNum", "Revision", "LayerId" })
            {
                if (!list.Any(kv => kv.Key.Equals(key, StringComparison.OrdinalIgnoreCase)))
                {
                    if (Request.Query.TryGetValue(key, out var v) && !string.IsNullOrWhiteSpace(v))
                        list.Add(new KeyValuePair<string, string>(key, v.ToString()));
                }
            }
            return list;
        }

        private static string BuildByKeysApi(string table, IEnumerable<KeyValuePair<string, string>> keyValues)
        {
            var qs = string.Concat(keyValues.Select(kv =>
                $"&keyNames={Uri.EscapeDataString(kv.Key)}&keyValues={Uri.EscapeDataString(kv.Value ?? string.Empty)}"));
            return $"/api/CommonTable/ByKeys?table={Uri.EscapeDataString(table)}{qs}";
        }

        private async Task<List<ItemCustButtonRow>> LoadCustomButtonsAsync(string itemId)
        {
            var list = new List<ItemCustButtonRow>();
            var cs = _ctx.Database.GetConnectionString();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();
            const string sql = @"
SELECT ItemId, SerialNum, ButtonName, Caption, Hint, OCXName, CoClassName
  FROM CURdOCXItemCustButton WITH (NOLOCK)
 WHERE ItemId = @itemId
 ORDER BY SerialNum";
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@itemId", itemId);
            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                list.Add(new ItemCustButtonRow
                {
                    ItemId     = rd["ItemId"]?.ToString() ?? "",
                    SerialNum  = rd["SerialNum"] == DBNull.Value ? null : Convert.ToInt32(rd["SerialNum"]),
                    ButtonName = rd["ButtonName"]?.ToString() ?? "",
                    Caption    = rd["Caption"]?.ToString() ?? "",
                    Hint       = rd["Hint"]?.ToString() ?? "",
                    OCXName    = rd["OCXName"]?.ToString() ?? "",
                    CoClassName = rd["CoClassName"]?.ToString() ?? ""
                });
            }
            return list;
        }

        // ── Records / inner types ─────────────────────────────────────────
        public record DetailTab(
            string Key, string Title, string TableName, string DictName,
            IEnumerable<string> KeyFields, bool IsForm = false,
            Dictionary<string, string>? ExtraKeys = null);

        public record MasterTab(
            string Key, string Title, IEnumerable<string> FieldNames, int ShowWhere = 0);

        public class ItemCustButtonRow
        {
            public string ItemId      { get; set; } = string.Empty;
            public int?   SerialNum   { get; set; }
            public string ButtonName  { get; set; } = string.Empty;
            public string Caption     { get; set; } = string.Empty;
            public string Hint        { get; set; } = string.Empty;
            public string OCXName     { get; set; } = string.Empty;
            public string CoClassName { get; set; } = string.Empty;
        }
    }
}
