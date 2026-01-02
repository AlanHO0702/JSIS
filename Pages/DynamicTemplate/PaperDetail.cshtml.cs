using System.Text;
using System.Linq;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Data;
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
    public class PaperDetailModel : PageModel
    {
        private readonly PcbErpContext _ctx;
        private readonly ITableDictionaryService _dictService;
        private readonly HttpClient _http;
        private readonly IWebHostEnvironment _env;

        public PaperDetailModel(PcbErpContext ctx, ITableDictionaryService dictService, IHttpClientFactory httpClientFactory, IWebHostEnvironment env)
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
        // 這裡回傳 DETAIL1 的字典表名（而非 Master），避免像「清除單身」等功能抓錯表
        public string DictTableName => string.IsNullOrWhiteSpace(DetailDictTable) ? MasterDictTable : DetailDictTable;
        public string AddApiUrl => $"/api/{MasterTable}";
        public string DetailRouteTemplate => $"/DynamicTemplate/Paper/{ItemId}/{{PaperNum}}";
        public string PageTitle { get; private set; } = "單據動態樣板";
        public string? ReportSpName { get; private set; }
        public string? ActionRailPartial { get; private set; }

        public List<Dictionary<string, object?>> Items { get; private set; } = new();
        public List<TableFieldViewModel> TableFields { get; private set; } = new();
        public List<TableFieldViewModel> HeaderTableFields { get; private set; } = new();
        public List<CURdTableField> FieldDictList { get; private set; } = new();
        public Dictionary<string, object> HeaderData { get; private set; } = new();
        public Dictionary<string, Dictionary<string, string>> LookupDisplayMap { get; private set; } = new();
        public Dictionary<string, string> HeaderLookupMap { get; private set; } = new();
        public List<QueryFieldViewModel> QueryFields { get; private set; } = new();
        public string? DetailLoadError { get; private set; }
        public List<ItemCustButtonRow> CustomButtons { get; private set; } = new();

        public string HeaderTableName => MasterDictTable;
        public string TableName => MasterTable;

        public async Task<IActionResult> OnGetAsync(string itemId, string paperNum)
        {
            if (string.IsNullOrWhiteSpace(itemId) || string.IsNullOrWhiteSpace(paperNum))
                return NotFound("itemId and paperNum are required.");

            ItemId = itemId;
            PaperNum = paperNum;

            var sysItem = await _ctx.CurdSysItems.AsNoTracking()
                .Where(x => x.ItemId == itemId)
                .Select(x => new { x.ItemId, x.ItemName })
                .FirstOrDefaultAsync();
            ItemName = sysItem?.ItemName ?? string.Empty;

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

            MasterDictTable = master.TableName ?? "";
            // 保留既有單一 detail 的欄位/lookup 行為（取第一筆作為預設）
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
                    .GroupBy(f => f.FieldName ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                    .Where(g => !string.IsNullOrWhiteSpace(g.Key))
                    .ToDictionary(g => g.Key, g => g.First().DisplayLabel ?? g.Key, StringComparer.OrdinalIgnoreCase);
            }

            ViewData["Tabs"] = tabs.ToArray();
            ViewData["TabFieldDicts"] = tabFieldDicts;

            // 3) Field dictionaries
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
                    iFieldWidth = x.iFieldWidth,
                    DisplaySize = x.DisplaySize,
                    ComboStyle = x.ComboStyle
                }).ToList();

            // 4) Lookup maps（MultiTab 明細採前端即時載入，這裡只保留單頭 lookup）
            ViewData["LookupDisplayMap"] = new Dictionary<string, Dictionary<string, string>>();

            var headerLookup = _dictService.GetOCXLookups(master.TableName ?? "");
            HeaderLookupMap = LookupDisplayHelper.BuildHeaderLookupMap(
                HeaderData.ToDictionary(k => k.Key, v => v.Value)
                , headerLookup);
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

            // 6) Custom buttons (left action rail)
            CustomButtons = await LoadCustomButtonsAsync(itemId);
            ActionRailPartial = (CustomButtons?.Count ?? 0) > 0
                ? "~/Pages/Shared/_ActionRail.DynamicButtons.cshtml"
                : "~/Pages/Shared/_ActionRail.Empty.cshtml";

            var logicPartial = ResolveActionRailLogicPartial(itemId);
            if (!string.IsNullOrWhiteSpace(logicPartial))
                ViewData["ActionRailLogicPartial"] = logicPartial;

            // 載入 Inherited 資料夾下的自訂 Hook 邏輯（確認、退審、作廢等按鈕的前後處理）
            var inheritedPartial = ResolveInheritedActionPartial(itemId);
            if (!string.IsNullOrWhiteSpace(inheritedPartial))
                ViewData["InheritedActionPartial"] = inheritedPartial;

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

        private string? ResolveInheritedActionPartial(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId)) return null;

            var fileName = $"{itemId.Trim()}.cshtml";
            var fullPath = Path.Combine(_env.ContentRootPath, "Pages", "Inherited", fileName);
            if (System.IO.File.Exists(fullPath))
                return $"~/Pages/Inherited/{fileName}";

            return null;
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
            {
                var body = await resp.Content.ReadAsStringAsync();
                DetailLoadError ??= $"Header API {resp.StatusCode} {resp.ReasonPhrase}: {body}";
                return new Dictionary<string, object>();
            }
            var json = await resp.Content.ReadFromJsonAsync<DynamicTableResult>();
            var first = json?.data?.FirstOrDefault();
            return first != null ? first : new Dictionary<string, object>();
        }

        private async Task<List<Dictionary<string, object?>>> LoadDetailAsync(string tableName, string paperNum)
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
                    new { Field = "pageSize", Op = "", Value = "9999" }
                }
            };
            try
            {
                var resp = await _http.PostAsJsonAsync(url, payload);
                if (!resp.IsSuccessStatusCode)
                {
                    var body = await resp.Content.ReadAsStringAsync();
                    DetailLoadError = $"Detail API {resp.StatusCode} {resp.ReasonPhrase}: {body}";
                    return new List<Dictionary<string, object?>>();
                }
                var json = await resp.Content.ReadFromJsonAsync<DynamicTableResult>();
                return json?.data ?? new List<Dictionary<string, object?>>();
            }
            catch (HttpRequestException ex)
            {
                DetailLoadError = $"Detail API exception: {ex.Message}";
                return new List<Dictionary<string, object?>>();
            }
        }

        public class DynamicTableResult
        {
            public int totalCount { get; set; }
            public List<Dictionary<string, object?>>? data { get; set; }
            public Dictionary<string, Dictionary<string, string>>? lookupMapData { get; set; }
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

        private static string GetDictValue(Dictionary<string, object?> dict, string field)
        {
            if (dict == null) return string.Empty;
            var hit = dict.FirstOrDefault(kv => kv.Key.Equals(field, StringComparison.OrdinalIgnoreCase));
            return hit.Equals(default(KeyValuePair<string, object?>)) ? string.Empty : hit.Value?.ToString() ?? string.Empty;
        }

        private static int? TryToInt(object? o)
        {
            if (o == null || o == DBNull.Value) return null;
            return int.TryParse(o.ToString(), out var n) ? n : null;
        }
    }
}
