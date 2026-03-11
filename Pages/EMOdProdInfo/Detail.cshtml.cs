using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PcbErpApi.Data;
using PcbErpApi.Models;
using PcbErpApi.Services;
using WebRazor.Models;

namespace PcbErpApi.Pages.EMOdProdInfo
{
    [IgnoreAntiforgeryToken]
    public class DetailModel : PageModel
    {
        private readonly PcbErpContext _ctx;
        private readonly ITableDictionaryService _dictService;
        private readonly ILogger<DetailModel> _logger;
        private readonly Services.AuditImageService _auditImageService;
        private readonly IWebHostEnvironment? _env;
        private readonly IBreadcrumbService _breadcrumbService;

        private const string MasterDict = "EMOdProdInfo";
        private const string MasterTable = "EMOdProdInfo";

        public DetailModel(
            PcbErpContext ctx,
            ITableDictionaryService dictService,
            ILogger<DetailModel> logger,
            Services.AuditImageService auditImageService,
            IBreadcrumbService breadcrumbService,
            IWebHostEnvironment? env = null)
        {
            _ctx = ctx;
            _dictService = dictService;
            _logger = logger;
            _auditImageService = auditImageService;
            _breadcrumbService = breadcrumbService;
            _env = env;
        }

        public MasterDetailConfig? Config { get; private set; }
        public List<KeyValuePair<string, string>> MasterKeyValues { get; private set; } = new();
        public List<DetailTab> ExtraTabs { get; private set; } = new();
        public List<MasterTab> MasterTabs { get; private set; } = new();
        public string? LayerPressApi { get; private set; }
        public string? SubmitHistoryApi { get; private set; }
        public List<ItemCustButtonRow> CustomButtons { get; private set; } = new();
        public string? ActionRailPartial { get; private set; }
        public string HeaderTableName => MasterTable;

        [FromQuery(Name = "mode")]
        public string? Mode { get; set; }

        [FromQuery(Name = "itemId")]
        public string? ItemId { get; set; }

        // 判斷是否為查詢模式
        public bool IsViewOnly
        {
            get
            {
                // 優先順序1: 明確的 mode 參數
                if (!string.IsNullOrEmpty(Mode))
                    return string.Equals(Mode, "view", StringComparison.OrdinalIgnoreCase);

                // 優先順序2: 根據 ItemId 判斷（查詢作業代碼）
                if (ItemId == "EMO00018")
                    return true;

                // 預設為維護模式
                return false;
            }
        }

        public string PageTitle => IsViewOnly ? "EMO00018 工程資料查詢" : "EMO00004 工程資料維護";

        public async Task<IActionResult> OnGetAsync()
        {
            ViewData["Title"] = PageTitle;

            // 麵包屑：用目前的 ItemId（EMO00004 或 EMO00018）查 SuperId
            try
            {
                var currentItemId = ItemId ?? (IsViewOnly ? "EMO00018" : "EMO00004");
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

                // 主鍵：PartNum + Revision
                var masterKeys = ExtractKeys(masterDict);
                if (masterKeys.Count == 0)
                {
                    masterKeys.Add("PartNum");
                    masterKeys.Add("Revision");
                }

                MasterKeyValues = CollectKeyValues(masterKeys);

                Config = new MasterDetailConfig
                {
                    DomId = "md_emodprodinfo",
                    MasterTable = MasterTable,
                    MasterDict = MasterDict,
                    MasterTitle = "工程資料主檔",
                    MasterTop = 1
                };

                if (MasterKeyValues.Count > 0)
                {
                    Config.MasterApi = BuildByKeysApi(MasterTable, MasterKeyValues);
                }

                ExtraTabs = BuildExtraTabs();

                var itemId = IsViewOnly ? "EMO00018" : "EMO00004";
                var tabCaptions = await LoadMasterTabCaptionsAsync(itemId);
                var fieldLangs = await LoadFieldLangsAsync(MasterDict);
                MasterTabs = BuildMasterTabs(masterDict, tabCaptions, fieldLangs);

                // 層別資料 API（EMOdProdLayer）- 用於左側層別清單
                if (MasterKeyValues.Count > 0)
                {
                    LayerPressApi = BuildByKeysApi("EMOdProdLayer", MasterKeyValues);
                }

                // 送審歷史資料（使用 CURdTableField 的記錄）
                // 暫時使用內部資料欄位：作業、使用者、時間
                SubmitHistoryApi = null; // 將在前端從 masterData 取得

                // 加載自定义按钮（左侧 Action Rail）
                try
                {
                    CustomButtons = await LoadCustomButtonsAsync(itemId);
                    ActionRailPartial = (CustomButtons?.Count ?? 0) > 0
                        ? "~/Pages/Shared/_ActionRail.DynamicButtons.cshtml"
                        : "~/Pages/Shared/_ActionRail.Empty.cshtml";

                    var logicPartial = ResolveActionRailLogicPartial(itemId);
                    if (!string.IsNullOrWhiteSpace(logicPartial))
                        ViewData["ActionRailLogicPartial"] = logicPartial;
                }
                catch (Exception btnEx)
                {
                    _logger.LogWarning(btnEx, "Failed to load custom buttons");
                    CustomButtons = new List<ItemCustButtonRow>();
                    ActionRailPartial = "~/Pages/Shared/_ActionRail.Empty.cshtml";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Build EMOdProdInfo detail config failed");
                return BadRequest($"初始化失敗: {ex.Message}");
            }

            return Page();
        }

        private Task<List<CURdTableField>> LoadFieldDictAsync(string dictTableName)
        {
            try
            {
                return Task.FromResult(_dictService.GetFieldDict(dictTableName, typeof(EmodProdInfo)));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "GetFieldDict failed for {Table}", dictTableName);
                return Task.FromResult(new List<CURdTableField>());
            }
        }

        private static List<string> ExtractKeys(IEnumerable<CURdTableField> fields)
        {
            return fields
                .Where(f => (f.PK ?? 0) == 1)
                .Select(f => f.FieldName)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .ToList();
        }

        private List<KeyValuePair<string, string>> CollectKeyValues(IEnumerable<string> masterKeys)
        {
            var list = new List<KeyValuePair<string, string>>();
            foreach (var k in masterKeys)
            {
                if (Request.Query.TryGetValue(k, out var v) && !string.IsNullOrWhiteSpace(v))
                {
                    list.Add(new KeyValuePair<string, string>(k, v.ToString()));
                }
            }
            // fallback：確保有 PartNum 和 Revision
            if (!list.Any(kv => kv.Key.Equals("PartNum", StringComparison.OrdinalIgnoreCase)))
            {
                if (Request.Query.TryGetValue("PartNum", out var pn) && !string.IsNullOrWhiteSpace(pn))
                    list.Add(new KeyValuePair<string, string>("PartNum", pn.ToString()));
            }
            if (!list.Any(kv => kv.Key.Equals("Revision", StringComparison.OrdinalIgnoreCase)))
            {
                if (Request.Query.TryGetValue("Revision", out var rev) && !string.IsNullOrWhiteSpace(rev))
                    list.Add(new KeyValuePair<string, string>("Revision", rev.ToString()));
            }
            return list;
        }

        private static string BuildByKeysApi(string table, IEnumerable<KeyValuePair<string, string>> keyValues)
        {
            var qs = string.Concat(keyValues.Select(kv =>
                $"&keyNames={Uri.EscapeDataString(kv.Key)}&keyValues={Uri.EscapeDataString(kv.Value ?? string.Empty)}"));
            return $"/api/CommonTable/ByKeys?table={Uri.EscapeDataString(table)}{qs}";
        }

        private List<DetailTab> BuildExtraTabs()
        {
            return new List<DetailTab>
            {
                // 壓合明細/替代料
                new DetailTab(
                    "layerpress",
                    "壓合明細/替代料",
                    "EMOdLayerPress",
                    "EMOdLayerPress",
                    new [] { "PartNum", "Revision" }
                ),
                // 板材尺寸明細圖（POP 為各列識別碼，必須列入 key 否則 UPDATE 會更新全部列）
                new DetailTab(
                    "boardsize",
                    "板材尺寸明細圖",
                    "EMOdProdPOP",
                    "EMOdProdPOP",
                    new [] { "PartNum", "Revision", "POP" }
                ),
                // 裁板/排版圖
                new DetailTab(
                    "cutlayout",
                    "裁板/排版圖",
                    "EMOdProdMills",
                    "EMOdProdMills",
                    new [] { "PartNum", "Revision" }
                ),
                // 混裝明細圖
                new DetailTab(
                    "mixdetail",
                    "混裝明細檔",
                    "EMOdProdMixedDtl",
                    "EMOdProdMixedDtl",
                    new [] { "PartNum", "Revision" }
                ),
                // 暫停記錄（暫停發料備註）
                new DetailTab(
                    "haltlog",
                    "暫停記錄",
                    "EMOdVProdHoldMemo",
                    "EMOdVProdHoldMemo",
                    new [] { "PartNum", "Revision" }
                ),
                // 修改記錄
                new DetailTab(
                    "modifylog",
                    "修改記錄",
                    "EMOdNotesLog",
                    "EMOdNotesLog",
                    new [] { "PartNum", "Revision" }
                ),
                // EC記錄
                new DetailTab(
                    "ecnlog",
                    "ECN記錄",
                    "EMOdProdECNLog",
                    "EMOdProdECNLog",
                    new [] { "PartNum", "Revision" }
                ),
                // 併板明細圖
                new DetailTab(
                    "mergedetail",
                    "併板明細檔",
                    "EMOdPartMerge",
                    "EMOdPartMerge",
                    new [] { "PartNum", "Revision" }
                ),
                // 壓合方式
                new DetailTab(
                    "pressmethod",
                    "壓合方式",
                    "EMOdProdTier",
                    "EMOdProdTier",
                    new [] { "PartNum", "Revision", "LayerId" }
                ),
                // 途程內容
                new DetailTab(
                    "layerroute",
                    "途程內容",
                    "EMOdLayerRoute",
                    "EMOdLayerRoute",
                    new [] { "PartNum", "Revision", "LayerId" }
                ),
                // 產品工程圖
                new DetailTab(
                    "prodmap",
                    "產品工程圖",
                    "EMOdProdMap",
                    "EMOdProdMap",
                    new [] { "PartNum", "Revision" }
                )
            };
        }

        // 從 CURdOCXItemOtherRule 讀取規格頁籤名稱（PaperMasTb{N}Caption），Master1 從 CURdTableName 取
        private async Task<Dictionary<int, string>> LoadMasterTabCaptionsAsync(string itemId)
        {
            var result = new Dictionary<int, string>();

            // Master1 的名稱從 CURdTableName.DisplayLabel（TableName='EMOdProdInfo'）
            var master1Label = _ctx.CurdTableNames
                .Where(t => t.TableName == MasterTable)
                .Select(t => t.DisplayLabel)
                .FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(master1Label))
                result[1] = master1Label;

            // PaperMasTb2Caption ~ PaperMasTb11Caption 從 CURdOCXItemOtherRule
            var cs = _ctx.Database.GetConnectionString();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();
            var sql = @"
SELECT RuleId, DLLValue
FROM CURdOCXItemOtherRule WITH (NOLOCK)
WHERE ItemId = @itemId
  AND RuleId LIKE 'PaperMasTb%Caption'";
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@itemId", itemId);
            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                var ruleId = rd["RuleId"]?.ToString() ?? "";
                var caption = rd["DLLValue"]?.ToString() ?? "";
                if (string.IsNullOrWhiteSpace(caption)) continue;
                // RuleId 格式：PaperMasTb{N}Caption，解析出 N
                var numStr = ruleId.Replace("PaperMasTb", "").Replace("Caption", "");
                if (int.TryParse(numStr, out var n))
                    result[n] = caption;
            }
            return result;
        }

        // 從 CURdTableFieldLang 讀取欄位的 IShowWhere 對應（TW 語系）
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

        // 依 IShowWhere 動態建立規格頁籤，頁籤名稱來自 DB
        private List<MasterTab> BuildMasterTabs(
            IEnumerable<CURdTableField> dict,
            Dictionary<int, string> tabCaptions,
            List<CurdTableFieldLang> fieldLangs)
        {
            var dictList = dict.Where(f => !string.IsNullOrWhiteSpace(f.FieldName)).ToList();
            var dictMap = dictList.ToDictionary(f => f.FieldName!, f => f, StringComparer.OrdinalIgnoreCase);

            // Lang 表的 IShowWhere 對應（優先）
            var langShowWhere = fieldLangs
                .Where(l => l.IShowWhere >= 1 && !string.IsNullOrWhiteSpace(l.FieldName))
                .ToDictionary(l => l.FieldName!, l => l.IShowWhere, StringComparer.OrdinalIgnoreCase);

            // 合併：Lang 表有的用 Lang，否則 fallback 到主表 iShowWhere
            // 以欄位為單位，取得 (FieldName, ShowWhere, SerialNum)
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
                var n = kv.Key;
                fieldShowWhere.TryGetValue(n, out var fields);
                tabs.Add(new MasterTab($"spec{n}", kv.Value, fields ?? new List<string>(), n));
            }
            return tabs;
        }

        public record DetailTab(string Key, string Title, string TableName, string DictName, IEnumerable<string> KeyFields, bool IsForm = false, Dictionary<string, string>? ExtraKeys = null);
        public record MasterTab(string Key, string Title, IEnumerable<string> FieldNames, int ShowWhere = 0);

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

        private string? ResolveActionRailLogicPartial(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId) || _env == null) return null;

            try
            {
                var fileName = $"{itemId.Trim()}.cshtml";
                var fullPath = Path.Combine(_env.ContentRootPath, "Pages", "CustomButton", fileName);
                if (System.IO.File.Exists(fullPath))
                    return $"~/Pages/CustomButton/{fileName}";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to resolve action rail logic partial for {ItemId}", itemId);
            }

            return null;
        }

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

        private static int? TryToInt(object? o)
        {
            if (o == null || o == DBNull.Value) return null;
            return int.TryParse(o.ToString(), out var n) ? n : null;
        }

        /// <summary>
        /// 產生圖檔 POST Handler
        /// </summary>
        public async Task<IActionResult> OnPostGenerateImagesAsync(string? partNum, string? revision)
        {
            try
            {
                if (string.IsNullOrEmpty(partNum) || string.IsNullOrEmpty(revision))
                {
                    return new JsonResult(new
                    {
                        success = false,
                        message = "料號和版序不可為空"
                    });
                }

                var result = await _auditImageService.GenerateImagesForAuditAsync(
                    partNum,
                    revision
                );

                return new JsonResult(new
                {
                    success = result.Success,
                    message = result.Message,
                    images = result.GeneratedFiles.Select(f => new
                    {
                        type = f.Type,
                        fileName = f.FileName,
                        success = f.Success,
                        errorMessage = f.ErrorMessage
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "產生圖檔時發生錯誤");
                return new JsonResult(new
                {
                    success = false,
                    message = $"產生圖檔失敗: {ex.Message}"
                });
            }
        }
    }
}
