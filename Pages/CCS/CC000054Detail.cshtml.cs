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

namespace PcbErpApi.Pages.CCS
{
    public class CC000054DetailModel : PageModel
    {
        private readonly PcbErpContext _ctx;
        private readonly ITableDictionaryService _dictService;
        private readonly ILogger<CC000054DetailModel> _logger;

        private const string MasterDict = "AJNdCompany_1";
        private const string MasterTable = "AJNdCompany";
        private const string DetailDict = "AJNdCompanySystemView_1";
        private const string DetailTable = "AJNdCompanySystem";

        public CC000054DetailModel(PcbErpContext ctx, ITableDictionaryService dictService, ILogger<CC000054DetailModel> logger)
        {
            _ctx = ctx;
            _dictService = dictService;
            _logger = logger;
        }

        public MasterDetailConfig? Config { get; private set; }
        public List<KeyValuePair<string, string>> MasterKeyValues { get; private set; } = new();
        public List<DetailTab> ExtraTabs { get; private set; } = new();
        public int SystemId { get; private set; } = 1;

        public async Task<IActionResult> OnGetAsync()
        {
            ViewData["Title"] = "客戶主檔_業務 - 明細";
            SystemId = ParseSystemId(Request.Query["systemId"]);

            try
            {
                var masterDict = await LoadFieldDictAsync(MasterDict);
                var detailDict = await LoadFieldDictAsync(DetailDict);

                var masterKeys = ExtractKeys(masterDict);
                if (masterKeys.Count == 0) masterKeys.Add("CompanyId");
                var detailKeys = ExtractKeys(detailDict);
                var keyMap = BuildKeyMap(masterKeys, detailKeys);

                MasterKeyValues = CollectKeyValues(masterKeys);

                Config = new MasterDetailConfig
                {
                    DomId = "md_cc000054",
                    MasterTable = MasterTable,
                    MasterDict = MasterDict,
                    DetailTable = DetailTable,
                    DetailDict = DetailDict,
                    MasterTitle = "客戶主檔",
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

                    if (detailKeyValues.Count > 0)
                        Config.DetailApi = BuildByKeysApi(DetailTable, detailKeyValues);
                }

                ExtraTabs = BuildExtraTabs();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Build CC000054 detail config failed");
                return BadRequest($"初始化失敗: {ex.Message}");
            }

            return Page();
        }

        private async Task<List<CURdTableField>> LoadFieldDictAsync(string dictTableName)
        {
            try
            {
                return _dictService.GetFieldDict(dictTableName, typeof(object));
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
            // fallback: 即便辭典沒標 PK，也優先取 CompanyId
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
            return new List<DetailTab>
            {
                new DetailTab(
                    "systemview",
                    "系統資料",
                    "AJNdCompanySystem",
                    "AJNdCompanySystemView_1",
                    new [] { "CompanyId" },
                    true,
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["SystemId"] = SystemId.ToString(),
                        ["SubSystemId"] = "0"
                    }),
                new DetailTab("assistant", "聯絡人", "AJNdCompanyAssistant", "AJNdCompanyAssistant", new [] { "CompanyId", "SerialNum" }),
                new DetailTab("outaddr", "出貨地址", "AJNdCompanyOutAddr", "AJNdCompanyOutAddr", new [] { "CompanyId", "SerialNum" }),
                new DetailTab("bank", "銀行資料", "AJNdCompanyBank", "AJNdCompanyBank", new [] { "CompanyId", "SerialNum" }),
                new DetailTab("bankcredit", "銀行授信", "AJNdCompanyBankCredit", "AJNdCompanyBankCredit", new [] { "CompanyId", "CreditSerial" }),
                new DetailTab("xout", "XOut 設定", "AJNdCompanyXOut", "AJNdCompanyXOut", new [] { "CompanyId", "SerialNum" })
            };
        }

        public record DetailTab(string Key, string Title, string TableName, string DictName, IEnumerable<string> KeyFields, bool IsForm = false, Dictionary<string, string>? ExtraKeys = null);

        private static int ParseSystemId(Microsoft.Extensions.Primitives.StringValues val)
        {
            foreach (var v in val)
            {
                if (int.TryParse(v, out var n) && n > 0) return n;
            }
            return 1;
        }
    }
}
