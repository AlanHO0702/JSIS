using System.Data;
using System.Linq;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using PcbErpApi.Data;
using PcbErpApi.Helpers;
using PcbErpApi.Models;

namespace PcbErpApi.Pages.DynamicTemplate
{
    public class Paper3LDetailModel : PageModel
    {
        private readonly PcbErpContext _ctx;
        private readonly ITableDictionaryService _dictService;
        private readonly HttpClient _http;
        private readonly IWebHostEnvironment _env;

        public Paper3LDetailModel(PcbErpContext ctx, ITableDictionaryService dictService, IHttpClientFactory httpClientFactory, IWebHostEnvironment env)
        {
            _ctx = ctx;
            _dictService = dictService;
            _http = httpClientFactory.CreateClient("MyApiClient");
            _env = env;
        }

        public string ItemId { get; private set; } = string.Empty;
        public string ItemName { get; private set; } = string.Empty;
        public string PaperNum { get; private set; } = string.Empty;
        public string MasterTable { get; private set; } = string.Empty;
        public string DetailTable { get; private set; } = string.Empty;
        public string MasterDictTable { get; private set; } = string.Empty;
        public string DetailDictTable { get; private set; } = string.Empty;

        // _DetailHeaderSection / custom buttons 會用 ViewData["DictTableName"] 當作「單身表」辨識
        public string DictTableName => string.IsNullOrWhiteSpace(DetailDictTable) ? MasterDictTable : DetailDictTable;

        public string DetailRouteTemplate => $"/DynamicTemplate/Paper3L/{ItemId}/{{PaperNum}}";
        public string PageTitle { get; private set; } = "單據動態樣板(三階)";
        public string? ReportSpName { get; private set; }
        public string? ActionRailPartial { get; private set; }

        public List<CURdTableField> FieldDictList { get; private set; } = new();
        public List<TableFieldViewModel> HeaderTableFields { get; private set; } = new();
        public Dictionary<string, object> HeaderData { get; private set; } = new();
        public Dictionary<string, string> HeaderLookupMap { get; private set; } = new();
        public List<QueryFieldViewModel> QueryFields { get; private set; } = new();
        public List<ItemCustButtonRow> CustomButtons { get; private set; } = new();

        public string HeaderTableName => MasterDictTable;
        public string TableName => MasterTable;

        public SubDetailClientConfig? SubDetail1 { get; private set; }

        public async Task<IActionResult> OnGetAsync(string itemId, string paperNum)
        {
            if (string.IsNullOrWhiteSpace(itemId) || string.IsNullOrWhiteSpace(paperNum))
                return NotFound("itemId and paperNum are required.");

            ItemId = itemId;
            PaperNum = paperNum;

            var sysItem = await _ctx.CurdSysItems.AsNoTracking()
                .Where(x => x.ItemId == itemId)
                .Select(x => new { x.ItemId, x.ItemName, x.ItemType, x.Ocxtemplete })
                .FirstOrDefaultAsync();

            if (sysItem == null)
                return NotFound($"Item {itemId} not found.");

            var ocx = (sysItem.Ocxtemplete ?? string.Empty).Trim();
            if (sysItem.ItemType != 6 || !ocx.Equals("JSdPaper3LDLL.dll", StringComparison.OrdinalIgnoreCase))
                return NotFound($"Item {itemId} is not a JSdPaper3LDLL paper(3-level) item.");

            ItemName = sysItem.ItemName ?? string.Empty;

            var setupList = await _ctx.CurdOcxtableSetUp.AsNoTracking()
                .Where(x => x.ItemId == itemId)
                .ToListAsync();

            var master = setupList
                .Where(x => (x.TableKind ?? "").Contains("Master", StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => x.TableKind)
                .FirstOrDefault()
                ?? setupList.FirstOrDefault();

            if (master == null)
                return NotFound($"No master table for item {itemId}.");

            var details = setupList
                .Where(x => (x.TableKind ?? "").StartsWith("Detail", StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => ExtractOrderIndex(x.TableKind))
                .ThenBy(x => x.TableKind)
                .ThenBy(x => x.TableName)
                .ToList();

            if (details.Count == 0)
                return NotFound($"No detail table found for item {itemId}.");

            // ★ 三階：多抓一個 SubDetail1（對應第二階的 DETAIL1）
            var subDetail1Setup = setupList.FirstOrDefault(x =>
                string.Equals((x.TableKind ?? string.Empty).Trim(), "SubDetail1", StringComparison.OrdinalIgnoreCase)
                || (x.TableKind ?? string.Empty).StartsWith("SubDetail", StringComparison.OrdinalIgnoreCase));

            MasterDictTable = master.TableName ?? "";
            DetailDictTable = details[0].TableName ?? "";

            MasterTable = await ResolveRealTableNameAsync(MasterDictTable) ?? MasterDictTable;
            DetailTable = await ResolveRealTableNameAsync(DetailDictTable) ?? DetailDictTable;

            ReportSpName = master.RunSqlafterAdd;
            var display = master.TableName ?? itemId;
            PageTitle = $"{display}（{paperNum}）";
            ViewData["HeaderTableDisplayLabel"] = await ResolveDisplayLabelAsync(MasterDictTable) ?? MasterDictTable;

            // 1) Header data
            HeaderData = await LoadHeaderAsync(MasterDictTable, PaperNum);

            // 2) Multi-detail tabs (DETAIL1/DETAIL2/...)
            ViewData["PaperNum"] = PaperNum;
            ViewData["MultiTabAllowEdit"] = true;

            var tabs = new List<object>(details.Count);
            var tabFieldDicts = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

            for (var i = 0; i < details.Count; i++)
            {
                var d = details[i];
                var dictTable = d.TableName ?? string.Empty;
                if (string.IsNullOrWhiteSpace(dictTable)) continue;

                var tabId = $"d{i + 1}";
                var title = await ResolveDisplayLabelAsync(dictTable) ?? dictTable;
                var apiUrl = $"/api/DynamicTable/ByPaperNum?table={Uri.EscapeDataString(dictTable)}";

                tabs.Add(new { Id = tabId, Title = title, ApiUrl = apiUrl, DictTable = dictTable });

                var fields = _dictService.GetFieldDict(dictTable, typeof(object));
                tabFieldDicts[tabId] = fields
                    .Where(f => f.Visible == 1)
                    .OrderBy(f => f.SerialNum ?? 0)
                    .GroupBy(f => f.FieldName ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                    .Where(g => !string.IsNullOrWhiteSpace(g.Key))
                    .ToDictionary(g => g.Key, g => g.First().DisplayLabel ?? g.Key, StringComparer.OrdinalIgnoreCase);
            }

            ViewData["Tabs"] = tabs.ToArray();
            ViewData["TabFieldDicts"] = tabFieldDicts;

            // 3) Field dictionaries (header + modal)
            var headerFields = _dictService.GetFieldDict(master.TableName ?? "", typeof(object));
            FieldDictList = _dictService.GetFieldDict(MasterDictTable, typeof(object));

            HeaderTableFields = headerFields
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

            // 4) Header lookup maps（MultiTab 明細採前端即時載入，這裡只保留單頭 lookup）
            ViewData["LookupDisplayMap"] = new Dictionary<string, Dictionary<string, string>>();

            var headerLookup = _dictService.GetOCXLookups(master.TableName ?? "");
            HeaderLookupMap = LookupDisplayHelper.BuildHeaderLookupMap(
                HeaderData.ToDictionary(k => k.Key, v => v.Value),
                headerLookup);
            ViewData["HeaderLookupMap"] = HeaderLookupMap;

            // 5) Query fields (for search modal reuse)
            QueryFields = _ctx.CURdPaperSelected
                .Where(x => x.TableName == master.TableName && x.IVisible == 1)
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

            // 6) SubDetail1 (third level) - 依 MdKey 對應到第二階（預設用 DETAIL1 / tab d1）
            if (subDetail1Setup != null && !string.IsNullOrWhiteSpace(subDetail1Setup.TableName))
            {
                var dictTable = subDetail1Setup.TableName!.Trim();
                var title = await ResolveDisplayLabelAsync(dictTable) ?? dictTable;
                var keyMap = BuildKeyMap(subDetail1Setup.Mdkey);

                var fields = _dictService.GetFieldDict(dictTable, typeof(object))
                    .Where(f => f.Visible == 1)
                    .OrderBy(f => f.SerialNum ?? 0)
                    .GroupBy(f => f.FieldName ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                    .Where(g => !string.IsNullOrWhiteSpace(g.Key))
                    .Select(g => new SubDetailFieldDef(g.Key, g.First().DisplayLabel ?? g.Key))
                    .ToList();

                SubDetail1 = new SubDetailClientConfig
                {
                    Title = title,
                    DictTable = dictTable,
                    ParentTabId = "d1",
                    KeyMap = keyMap,
                    Fields = fields
                };

                ViewData["SubDetail1"] = SubDetail1;
            }

            // 7) Action rail：先不套用 DB 動態按鈕（Paper3L 以顯示為主）
            CustomButtons = await LoadCustomButtonsAsync(itemId);
            ActionRailPartial = (CustomButtons?.Count ?? 0) > 0
                ? "~/Pages/Shared/_ActionRail.DynamicButtons.cshtml"
                : "~/Pages/Shared/_ActionRail.Empty.cshtml";

            var logicPartial = ResolveActionRailLogicPartial(itemId);
            if (!string.IsNullOrWhiteSpace(logicPartial))
                ViewData["ActionRailLogicPartial"] = logicPartial;

            return Page();
        }

        private string? ResolveActionRailLogicPartial(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId)) return null;

            var fileName = $"{itemId.Trim()}.cshtml";
            var fullPath = Path.Combine(_env.ContentRootPath, "Pages", "CustomButton", fileName);
            if (System.IO.File.Exists(fullPath))
                return $"~/Pages/CustomButton/{fileName}";

            return null;
        }

        private static int ExtractOrderIndex(string? tableKind)
        {
            if (string.IsNullOrWhiteSpace(tableKind)) return int.MaxValue;
            var m = Regex.Match(tableKind, "(\\d+)$");
            return m.Success && int.TryParse(m.Groups[1].Value, out var n) ? n : int.MaxValue;
        }

        private static List<KeyMapPair> BuildKeyMap(string? mdKey)
        {
            var parts = (mdKey ?? string.Empty)
                .Split(new[] { ';', ',', '|' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            if (parts.Count == 0) return new List<KeyMapPair>();

            var list = new List<KeyMapPair>(parts.Count);
            foreach (var k in parts)
            {
                var segs = k.Split(new[] { ':', '=' }, StringSplitOptions.RemoveEmptyEntries);
                var parent = segs.Length > 0 ? segs[0].Trim() : k.Trim();
                var child = segs.Length > 1 ? segs[1].Trim() : parent;
                if (string.IsNullOrWhiteSpace(parent) || string.IsNullOrWhiteSpace(child)) continue;
                list.Add(new KeyMapPair(parent, child));
            }
            return list;
        }

        private async Task<string?> ResolveDisplayLabelAsync(string dictTableName)
        {
            var cs = _ctx.Database.GetConnectionString();
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

        private async Task<string?> ResolveRealTableNameAsync(string dictTableName)
        {
            var cs = _ctx.Database.GetConnectionString();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            const string sql = @"
SELECT TOP 1 ISNULL(NULLIF(RealTableName,''), TableName) AS ActualName
  FROM CURdTableName WITH (NOLOCK)
 WHERE TableName = @tbl";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@tbl", dictTableName ?? string.Empty);
            var result = await cmd.ExecuteScalarAsync();
            return result == null || result == DBNull.Value ? null : result.ToString();
        }

        private async Task<Dictionary<string, object>> LoadHeaderAsync(string tableName, string paperNum)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var url = $"{baseUrl}/api/DynamicTable/PagedQuery";
            var payload = new
            {
                table = tableName,
                filters = new[]
                {
                    new { Field = "PaperNum", Op = "=", Value = paperNum },
                    new { Field = "page", Op = "", Value = "1" },
                    new { Field = "pageSize", Op = "", Value = "1" }
                }
            };
            var resp = await _http.PostAsJsonAsync(url, payload);
            if (!resp.IsSuccessStatusCode)
                return new Dictionary<string, object>();
            var json = await resp.Content.ReadFromJsonAsync<DynamicTableResult>();
            var first = json?.data?.FirstOrDefault();
            return first != null ? first : new Dictionary<string, object>();
        }

        public class DynamicTableResult
        {
            public int totalCount { get; set; }
            public List<Dictionary<string, object?>>? data { get; set; }
        }

        public sealed record KeyMapPair(string ParentField, string ChildField);
        public sealed record SubDetailFieldDef(string FieldName, string DisplayLabel);

        public sealed class SubDetailClientConfig
        {
            public string Title { get; set; } = string.Empty;
            public string DictTable { get; set; } = string.Empty;
            public string ParentTabId { get; set; } = "d1";
            public List<KeyMapPair> KeyMap { get; set; } = new();
            public List<SubDetailFieldDef> Fields { get; set; } = new();
        }

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

        private async Task<List<ItemCustButtonRow>> LoadCustomButtonsAsync(string itemId)
        {
            var list = new List<ItemCustButtonRow>();
            var cs = _ctx.Database.GetConnectionString();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            const string sql = @"
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

            await using var rd = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);
            while (await rd.ReadAsync())
            {
                var visible = TryToInt(rd["bVisible"]);
                if (visible.HasValue && visible.Value == 0) continue;

                list.Add(new ItemCustButtonRow
                {
                    ItemId = rd["ItemId"]?.ToString() ?? string.Empty,
                    SerialNum = TryToInt(rd["SerialNum"]),
                    ButtonName = rd["ButtonName"]?.ToString() ?? string.Empty,
                    Caption = rd["CustCaption"]?.ToString() ?? string.Empty,
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
    }
}
