using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using PcbErpApi.Data;
using PcbErpApi.Models;
using WebRazor.Models;

namespace PcbErpApi.Pages.EMOdProdInfo
{
    public class DetailModel : PageModel
    {
        private readonly PcbErpContext _ctx;
        private readonly ITableDictionaryService _dictService;
        private readonly ILogger<DetailModel> _logger;

        private const string MasterDict = "EMOdProdInfo";
        private const string MasterTable = "EMOdProdInfo";

        public DetailModel(PcbErpContext ctx, ITableDictionaryService dictService, ILogger<DetailModel> logger)
        {
            _ctx = ctx;
            _dictService = dictService;
            _logger = logger;
        }

        public MasterDetailConfig? Config { get; private set; }
        public List<KeyValuePair<string, string>> MasterKeyValues { get; private set; } = new();
        public List<DetailTab> ExtraTabs { get; private set; } = new();
        public List<MasterTab> MasterTabs { get; private set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            ViewData["Title"] = "工程資料維護 - 詳細";

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
                MasterTabs = BuildMasterTabs();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Build EMOdProdInfo detail config failed");
                return BadRequest($"初始化失敗: {ex.Message}");
            }

            return Page();
        }

        private async Task<List<CURdTableField>> LoadFieldDictAsync(string dictTableName)
        {
            try
            {
                return _dictService.GetFieldDict(dictTableName, typeof(EmodProdInfo));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "GetFieldDict failed for {Table}", dictTableName);
                return new List<CURdTableField>();
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
                // 板材尺寸明細圖
                new DetailTab(
                    "boardsize",
                    "板材尺寸明細圖",
                    "EMOdProdPOP",
                    "EMOdProdPOP",
                    new [] { "PartNum", "Revision" }
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
                // 暫停記錄
                new DetailTab(
                    "haltlog",
                    "暫停記錄",
                    "EMOdProdLog",
                    "EMOdProdLog",
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
                    "EMOdPressMatDtl",
                    "EMOdPressMatDtl",
                    new [] { "PartNum", "Revision" }
                ),
                // 途程內容
                new DetailTab(
                    "layerroute",
                    "途程內容",
                    "EMOdLayerRoute",
                    "EMOdLayerRoute",
                    new [] { "PartNum", "Revision" }
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

        private List<MasterTab> BuildMasterTabs()
        {
            return new List<MasterTab>
            {
                new MasterTab("substrate", "基板/尺寸/板厚", new[]
                {
                    "TmpBomid", "DoType", "CustomerPartNum", "CustomerSname", "NumOfLayer",
                    "Materialq", "ProDstyle", "Press", "PressP", "PressM",
                    "NetThickLow", "NetThickUpp", "RftNetThick", "RftNetThickM", "RftNetThickP",
                    "Usage", "UnitArea"
                }),
                new MasterTab("imaging", "影像轉移", new[]
                {
                    // Original fields
                    "PatternNum", "LineWid", "MinSmd",
                    // Added fields
                    "AnnularRanMinIn", "AnnularWorkMinIn", "AnnularNonIn", "AnnularRanMinOut", "AnnularWorkMinOut", "AnnularNonOut",
                    "NetOpticAvgC", "NetOpticMinC", "NetOpticMaxC", "NetOpticNonC",
                    "NetOpticAvgS", "NetOpticMinS", "NetOpticMaxS", "NetOpticNonS"
                }),
                new MasterTab("plating", "電鍍", new[]
                {
                    "GoldArea", "GoldThick", "GoldMin", "GoldMax", "GoldAvg", "GoldRequest", "GoldRequestMin", "GoldRequestMax", "GoldRequestAvg",
                    "NickelThick", "NickelMin", "NickelMax", "NickelAvg", "NiRequest", "NickelRequestMin", "NickelRequestMax", "NickelRequestAvg",
                    "CuholeMin", "CuholeAvg", "CuviaMax", "CusurfaceMax", "ThickCuMin", "ThickCuMax", "ThickCuAvg",
                    "TinThick", "NetTin", "NetTinMin", "NetTinMax", "NetTinAvg",
                    "ChGoldThick", "ChSilverThick", "ChTinThick"
                }),
                new MasterTab("marking", "標記", new[]
                {
                    "ULMarkFace", "MarkCycle", "MarkCdate", "MarkBlue", "Ulmark94V", "Ullayer",
                    "MarkPlace", "MadeIn"
                }),
                new MasterTab("surface", "表面處理", new[]
                {
                    // Original fields
                    "Osseal", "Entekdepth", "EntekdepthMax", "Entekchk",
                    // Added fields
                    "Passivation", "PrintCarbon", "Strip", "Hardness", "OilThick"
                }),
                new MasterTab("solder", "防焊", new[]
                {
                    "LsmColor", "LsmFace", "LsmMaker", "LsmViahole", "LsmColorB", "LsmIsIn", "LsmModel", "MinLsm", "Film"
                }),
                new MasterTab("legend", "文印", new[]
                {
                    // Original fields
                    "CharColor", "CharFace", "CharColorB", "CharColorC", "CharMark", "LetterHl",
                    // Added fields
                    "BarCodeColorT", "BarCodeColorB", "BarCodeThick", "BarCodeSize", "WordNum"
                }),
                new MasterTab("profile", "成型", new[]
                {
                    "MachWay", "DieType", "DieSeq", "Pinvcu", "VcutDist", "VcutCross", "VcutRip", "VcutDisM", "VcutWid",
                    "Pinslash", "PinslashA", "PinslashB", "GfslashAngA", "GfslashAngB", "GfslashHa", "GfslashHb",
                    "Cnc", "Cncrequest", "Cncmin", "Cncmax", "VcutAngleAvg", "VcutAngleMin", "VcutAngleMax",
                    "VcutRelicAvg", "VcutRelicMin", "VcutRelicMax", "VcutSum"
                }),
                new MasterTab("test", "測試", new[]
                {
                    "TestManuFac", "HoleCheck", "SpecDemandCont",
                    "TinChk", "NetTinChk", "GoldChk", "NickelChk", "GoldRequestChk", "NickelRequestChk",
                    "SoftGoldReqChk", "SoftNickelReqChk", "ChsilverChk", "ChTinChk", "VcutDisMchk",
                    "Entekchk", "HardGoldChk",
                    "StatusChk1", "StatusChk2", "StatusChk3", "StatusChk4", "StatusChk5", "StatusChk6", "StatusChk7", "StatusChk8"
                }),
                new MasterTab("cam", "CAM注意事項", new[]
                {
                    "ProdNotes", "OssealNotes", "ProdHints", "CmapPath", "SmapPath", "SpecDemandCont", "Cam", "Designer", "HaltNotes"
                })
            };
        }

        public record DetailTab(string Key, string Title, string TableName, string DictName, IEnumerable<string> KeyFields, bool IsForm = false, Dictionary<string, string>? ExtraKeys = null);
        public record MasterTab(string Key, string Title, IEnumerable<string> FieldNames);
    }
}
