using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PcbErpApi.Data;
using PcbErpApi.Models;
using WebRazor.Models;

namespace PcbErpApi.Pages.CCS
{
    public class CC000054DetailModel : PageModel
    {
        private readonly PcbErpContext _ctx;
        private readonly ITableDictionaryService _dictService;
        private readonly ILogger<CC000054DetailModel> _logger;

        private const string DefaultMasterDict = "AJNdCompany_1";
        private const string MasterTable = "AJNdCompany";
        private const string DefaultSystemDict = "AJNdCompanySystemView_1";
        private const string DetailTable = "AJNdCompanySystem";

        public CC000054DetailModel(PcbErpContext ctx, ITableDictionaryService dictService, ILogger<CC000054DetailModel> logger)
        {
            _ctx = ctx;
            _dictService = dictService;
            _logger = logger;
        }

        public string ItemId { get; private set; } = "CC000054";
        public string ItemName { get; private set; } = string.Empty;
        public int? PowerType { get; private set; }
        public int? PaperType { get; private set; }
        public int TableId { get; private set; } = 1;
        public int SystemId { get; private set; } = 1;
        public int SubSystemId { get; private set; }
        public string PageTitle => string.IsNullOrWhiteSpace(ItemName) ? ItemId : $"{ItemId} {ItemName}";
        public string MasterDictName { get; private set; } = DefaultMasterDict;
        public string SystemDictName { get; private set; } = DefaultSystemDict;

        public MasterDetailConfig? Config { get; private set; }
        public List<KeyValuePair<string, string>> MasterKeyValues { get; private set; } = new();
        public List<DetailTab> ExtraTabs { get; private set; } = new();

        public async Task<IActionResult> OnGetAsync([FromRoute] string? itemId)
        {
            ItemId = string.IsNullOrWhiteSpace(itemId) ? ItemId : itemId.Trim();

            var item = await _ctx.CurdSysItems.AsNoTracking()
                .Where(x => x.ItemId == ItemId)
                .Select(x => new
                {
                    x.ItemName,
                    x.PowerType,
                    x.PaperType
                })
                .FirstOrDefaultAsync();

            if (item == null)
                return NotFound($"Item {ItemId} not found.");

            ItemName = item.ItemName ?? string.Empty;
            PowerType = item.PowerType;
            PaperType = item.PaperType;

            var systemIdQuery = ParseSystemId(Request.Query["systemId"]);
            SystemId = ResolveSystemId(PowerType, systemIdQuery);
            TableId = ResolveTableId(SystemId);
            SubSystemId = ResolveSubSystemId(PaperType);

            var masterDictName = ResolveMasterDictName(TableId);
            var systemDictName = ResolveSystemDictName(TableId);

            ViewData["Title"] = PageTitle;

            try
            {
                var masterDict = await LoadFieldDictAsync(masterDictName);
                if (masterDict.Count == 0 && !string.Equals(masterDictName, DefaultMasterDict, StringComparison.OrdinalIgnoreCase))
                {
                    masterDictName = DefaultMasterDict;
                    masterDict = await LoadFieldDictAsync(masterDictName);
                }

                var detailDict = await LoadFieldDictAsync(systemDictName);
                if (detailDict.Count == 0 && !string.Equals(systemDictName, DefaultSystemDict, StringComparison.OrdinalIgnoreCase))
                {
                    systemDictName = DefaultSystemDict;
                    detailDict = await LoadFieldDictAsync(systemDictName);
                }

                MasterDictName = masterDictName;
                SystemDictName = systemDictName;

                var masterKeys = ExtractKeys(masterDict);
                if (masterKeys.Count == 0) masterKeys.Add("CompanyId");
                var detailKeys = ExtractKeys(detailDict);
                var keyMap = BuildKeyMap(masterKeys, detailKeys);

                MasterKeyValues = CollectKeyValues(masterKeys);

                Config = new MasterDetailConfig
                {
                    DomId = "md_cc000054",
                    MasterTable = MasterTable,
                    MasterDict = masterDictName,
                    DetailTable = DetailTable,
                    DetailDict = systemDictName,
                    MasterTitle = string.IsNullOrWhiteSpace(ItemName) ? "客戶主檔" : ItemName,
                    DetailTitle = "系統資料",
                    KeyMap = keyMap.ToArray(),
                    DetailKeyFields = detailKeys.ToArray(),
                    MasterTop = 300
                };

                if (MasterKeyValues.Count > 0)
                {
                    Config.MasterApi = BuildByKeysApi(MasterTable, MasterKeyValues);

                    var detailKeyValues = keyMap.Count > 0
                        ? MapToDetailKeys(MasterKeyValues, keyMap)
                        : MapBySameName(detailKeys, MasterKeyValues);

                    if (detailKeys.Any(k => string.Equals(k, "SystemId", StringComparison.OrdinalIgnoreCase)))
                    {
                        detailKeyValues.RemoveAll(kv => kv.Key.Equals("SystemId", StringComparison.OrdinalIgnoreCase));
                        detailKeyValues.Add(new KeyValuePair<string, string>("SystemId", SystemId.ToString()));
                    }
                    if (SubSystemId > 0 && detailKeys.Any(k => string.Equals(k, "SubSystemId", StringComparison.OrdinalIgnoreCase)))
                    {
                        detailKeyValues.RemoveAll(kv => kv.Key.Equals("SubSystemId", StringComparison.OrdinalIgnoreCase));
                        detailKeyValues.Add(new KeyValuePair<string, string>("SubSystemId", SubSystemId.ToString()));
                    }

                    if (detailKeyValues.Count > 0)
                        Config.DetailApi = BuildByKeysApi(DetailTable, detailKeyValues);
                }

                ExtraTabs = BuildExtraTabs();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Build CC000054 detail config failed for {ItemId}", ItemId);
                return BadRequest($"初始化畫面失敗: {ex.Message}");
            }

            return Page();
        }

        private Task<List<CURdTableField>> LoadFieldDictAsync(string dictTableName)
        {
            try
            {
                return Task.FromResult(_dictService.GetFieldDict(dictTableName, typeof(object)));
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

        private List<KeyMap> BuildKeyMap(List<string> masterKeys, List<string> detailKeys)
        {
            var list = new List<KeyMap>();
            foreach (var dk in detailKeys)
            {
                var mk = masterKeys.FirstOrDefault(m => string.Equals(m, dk, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrWhiteSpace(mk))
                {
                    list.Add(new KeyMap(mk, dk));
                }
            }

            if (list.Count == 0 && masterKeys.Count > 0 && detailKeys.Count > 0)
            {
                list.Add(new KeyMap(masterKeys[0], detailKeys[0]));
            }

            return list;
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

            if (!list.Any(kv => kv.Key.Equals("CompanyId", StringComparison.OrdinalIgnoreCase)))
            {
                if (Request.Query.TryGetValue("CompanyId", out var cid) && !string.IsNullOrWhiteSpace(cid))
                    list.Add(new KeyValuePair<string, string>("CompanyId", cid.ToString()));
            }
            return list;
        }

        private static List<KeyValuePair<string, string>> MapToDetailKeys(IEnumerable<KeyValuePair<string, string>> masterValues, IEnumerable<KeyMap> map)
        {
            var list = new List<KeyValuePair<string, string>>();
            foreach (var km in map)
            {
                var val = masterValues.FirstOrDefault(m => string.Equals(m.Key, km.Master, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrWhiteSpace(val.Key))
                {
                    list.Add(new KeyValuePair<string, string>(km.Detail, val.Value));
                }
            }
            return list;
        }

        private static List<KeyValuePair<string, string>> MapBySameName(IEnumerable<string> detailKeys, IEnumerable<KeyValuePair<string, string>> masterValues)
        {
            var list = new List<KeyValuePair<string, string>>();
            var masterMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in masterValues)
            {
                if (!masterMap.ContainsKey(kv.Key))
                    masterMap[kv.Key] = kv.Value;
            }

            foreach (var dk in detailKeys ?? Array.Empty<string>())
            {
                if (masterMap.TryGetValue(dk, out var v) && !string.IsNullOrWhiteSpace(v))
                {
                    list.Add(new KeyValuePair<string, string>(dk, v));
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

        private List<DetailTab> BuildExtraTabs()
        {
            var tabs = new List<DetailTab>();
            var pt = PowerType ?? 0;
            var showSystemTab = pt != 101 && pt != 104;
            var showCreditTabs = pt == 103 || pt == 104;
            var showCustomerTabs = ShouldShowCustomerTabs(pt, SystemId);

            if (showSystemTab)
            {
                var extraKeys = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["SystemId"] = SystemId.ToString()
                };
                if (SubSystemId > 0)
                    extraKeys["SubSystemId"] = SubSystemId.ToString();

                tabs.Add(new DetailTab(
                    "systemview",
                    "系統資料",
                    "AJNdCompanySystem",
                    SystemDictName,
                    new[] { "CompanyId", "SystemId", "SubSystemId" },
                    true,
                    extraKeys));
            }

            tabs.Add(new DetailTab("assistant", "聯絡人", "AJNdCompanyAssistant", "AJNdCompanyAssistant", new[] { "CompanyId", "SerialNum" }));
            tabs.Add(new DetailTab("outaddr", "出貨地址", "AJNdCompanyOutAddr", "AJNdCompanyOutAddr", new[] { "CompanyId", "SerialNum" }));

            if (showCustomerTabs)
            {
                tabs.Add(new DetailTab("bank", "銀行資料", "AJNdCompanyBank", "AJNdCompanyBank", new[] { "CompanyId", "SerialNum" }));
                tabs.Add(new DetailTab("bankcredit", "銀行授信", "AJNdCompanyBankCredit", "AJNdCompanyBankCredit", new[] { "CompanyId", "SerialNum", "CreditSerial" }));
            }

            if (showCreditTabs)
            {
                tabs.Add(new DetailTab("creditline", "授信額度", "AJNdCompanyCreditLine", "AJNdCompanyCreditLine", new[] { "CompanyId", "SerialNum" }));
                tabs.Add(new DetailTab("bankassure", "銀行保證函", "AJNdCompanyBankAssure", "AJNdCompanyBankAssure", new[] { "CompanyId", "RecYear", "RecMonth" }));
            }

            if (showCustomerTabs)
            {
                tabs.Add(new DetailTab("ship", "銷貨資料", "AJNdCompanyShip", "AJNdCompanyShip", new[] { "CompanyId" }, true));
                tabs.Add(new DetailTab("forwarder", "Forwarder", "AJNdCompanyForwarder", "AJNdCompanyForwarder", new[] { "CompanyId", "SerialNum" }));
            }

            if (TryGetAvlTable(pt, SystemId, showCustomerTabs, out var avlTable, out var avlTitle))
                tabs.Add(new DetailTab("avl", avlTitle, avlTable, avlTable, new[] { "CompanyId" }));

            if (SystemId == 1 && showCustomerTabs)
                tabs.Add(new DetailTab("quota", "客戶要求", "AJNdCompanyQuota", "AJNdCompanyQuota", new[] { "CompanyId", "NumId" }));

            if (SystemId == 8)
                tabs.Add(new DetailTab("car", "車輛明細檔", "AJNdCompanyRecycleCar", "AJNdCompanyRecycleCar", new[] { "CompanyId", "SerialNum" }));

            if (SystemId > 1 && SystemId < 9 && SystemId != 7)
                tabs.Add(new DetailTab("attach", "附檔", "AJNdCompanyAttach", "AJNdCompanyAttach", new[] { "CompanyId", "SerialNum" }));

            if (showCustomerTabs)
                tabs.Add(new DetailTab("xout", "XOut 設定", "AJNdCompanyXOut", "AJNdCompanyXOut", new[] { "CompanyId", "SerialNum" }));

            return tabs;
        }

        private static bool ShouldShowCustomerTabs(int powerType, int systemId)
        {
            if (powerType <= 100)
                return systemId == 1;
            if (powerType == 101)
                return false;
            if (powerType == 103 || powerType == 104)
                return true;
            return true;
        }

        private static bool TryGetAvlTable(int powerType, int systemId, bool showCustomerTabs, out string tableName, out string title)
        {
            tableName = string.Empty;
            title = string.Empty;

            if (powerType == 103)
                return false;

            if (systemId == 1)
            {
                if (!showCustomerTabs) return false;
                tableName = "SPOdPriceTable_CCS";
                title = "客戶品號";
                return true;
            }
            if (systemId == 2)
            {
                tableName = "MPHdPriceTable_CCS";
                title = "物料AVL";
                return true;
            }
            if (systemId == 8)
            {
                tableName = "MPHdPriceRecycleTable_CCS";
                title = "廢棄物料號";
                return true;
            }

            return false;
        }

        public record DetailTab(string Key, string Title, string TableName, string DictName, IEnumerable<string> KeyFields, bool IsForm = false, Dictionary<string, string>? ExtraKeys = null);

        private static int ParseSystemId(Microsoft.Extensions.Primitives.StringValues val)
        {
            foreach (var v in val)
            {
                if (int.TryParse(v, out var n) && n > 0) return n;
            }
            return 0;
        }

        private static int ResolveSystemId(int? powerType, int requestedSystem)
        {
            if (requestedSystem > 0) return requestedSystem;
            if (!powerType.HasValue) return 1;

            var pt = powerType.Value;
            if (pt <= 100)
            {
                if (pt <= 50) return Math.Max(1, pt);
                return Math.Max(1, pt - 50);
            }
            if (pt == 101) return 1;
            if (pt == 103 || pt == 104) return 1;
            return 1;
        }

        private static int ResolveTableId(int systemId)
        {
            if (systemId > 0 && systemId < 10) return systemId;
            return 1;
        }

        private static int ResolveSubSystemId(int? paperType)
        {
            if (!paperType.HasValue) return 0;
            if (paperType.Value == 255) return 0;
            return Math.Max(0, paperType.Value);
        }

        private static string ResolveMasterDictName(int tableId)
        {
            if (tableId <= 0) return DefaultMasterDict;
            return $"AJNdCompany_{tableId}";
        }

        private static string ResolveSystemDictName(int tableId)
        {
            if (tableId <= 0) return DefaultSystemDict;
            return $"AJNdCompanySystemView_{tableId}";
        }
    }
}
