using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PcbErpApi.Data;
using PcbErpApi.Models;

namespace PcbErpApi.Pages.CCS
{
    public class CC000054Model : PageModel
    {
        private readonly PcbErpContext _ctx;
        private readonly ITableDictionaryService _dictService;
        private readonly ILogger<CC000054Model> _logger;

        private const string DefaultDictTable = "AJNdCompany_1";
        private const string DataTable = "AJNdCompany";

        public CC000054Model(PcbErpContext ctx, ITableDictionaryService dictService, ILogger<CC000054Model> logger)
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
        public bool ApplySystemFilter { get; private set; } = true;
        public bool ApplySubSystemFilter { get; private set; }
        public bool CanAdd { get; private set; }
        public bool CanDelete { get; private set; }
        public bool CanReview { get; private set; }
        public bool CanBackReview { get; private set; }
        public bool CanTurnFormal { get; private set; }
        public bool CanDrop { get; private set; }

        public string PageTitle => string.IsNullOrWhiteSpace(ItemName) ? ItemId : $"{ItemId} {ItemName}";
        public string TableName { get; private set; } = DataTable;
        public string DictTableName { get; private set; } = DefaultDictTable;
        public int PageNumber { get; private set; } = 1;
        public int PageSize { get; private set; } = 50;
        public int TotalCount { get; private set; }
        public List<Dictionary<string, object?>> Items { get; private set; } = new();
        public List<CURdTableField> FieldDictList { get; private set; } = new();
        public List<CURdTableField> TableFields { get; private set; } = new();
        public List<string> KeyFields { get; private set; } = new();
        public List<QueryFieldViewModel> QueryFields { get; private set; } = new();

        public async Task<IActionResult> OnGetAsync(
            [FromRoute] string? itemId,
            [FromRoute(Name = "systemId")] int? systemIdRoute,
            [FromQuery(Name = "systemId")] int? systemIdQuery,
            [FromQuery(Name = "pageIndex")] int pageIndex = 1,
            int pageSize = 50,
            string? sortBy = null,
            string? sortDir = null)
        {
            ItemId = string.IsNullOrWhiteSpace(itemId) ? ItemId : itemId.Trim();

            var item = await _ctx.CurdSysItems.AsNoTracking()
                .Where(x => x.ItemId == ItemId)
                .Select(x => new
                {
                    x.ItemName,
                    x.PowerType,
                    x.PaperType,
                    x.BtnInq,
                    x.BtnToExcel,
                    x.BtnUpdate,
                    x.BtnAdd,
                    x.BtnDelete,
                    x.BtnVoid,
                    x.BtnExam,
                    x.BtnRejExam
                })
                .FirstOrDefaultAsync();

            if (item == null)
                return NotFound($"Item {ItemId} not found.");

            ItemName = item.ItemName ?? string.Empty;
            PowerType = item.PowerType;
            PaperType = item.PaperType;

            var requestedSystem = NormalizeSystemId(systemIdQuery ?? systemIdRoute);
            SystemId = ResolveSystemId(PowerType, requestedSystem);
            TableId = ResolveTableId(SystemId);
            SubSystemId = ResolveSubSystemId(PaperType);
            ApplySystemFilter = ShouldApplySystemFilter(PowerType);
            ApplySubSystemFilter = ShouldApplySubSystemFilter(PowerType, PaperType);

            DictTableName = ResolveDictTableName(TableId);
            TableName = DataTable;
            PageNumber = pageIndex <= 0 ? 1 : pageIndex;
            PageSize = pageSize <= 0 ? 50 : pageSize;

            var canUpdateByFlag = GetFlag(item.BtnUpdate, true);
            var canAddByFlag = GetFlag(item.BtnAdd, true) || canUpdateByFlag;
            CanAdd = CanAddOrDelete(PowerType) && canAddByFlag;
            var canDeleteByFlag = GetFlag(item.BtnDelete, true) || GetFlag(item.BtnVoid, true);
            CanDelete = CanAddOrDelete(PowerType) && canDeleteByFlag;
            CanReview = GetFlag(item.BtnExam, false);
            CanBackReview = GetFlag(item.BtnRejExam, false);
            CanTurnFormal = PowerType == 9;
            CanDrop = PowerType == 101;

            ViewData["Title"] = PageTitle;
            ViewData["SystemId"] = SystemId;
            ViewData["SubSystemId"] = SubSystemId;
            ViewData["PowerType"] = PowerType;
            ViewData["PaperType"] = PaperType;

            FieldDictList = await LoadFieldDictAsync(DictTableName);
            if (FieldDictList.Count == 0 && !string.Equals(DictTableName, DefaultDictTable, StringComparison.OrdinalIgnoreCase))
            {
                DictTableName = DefaultDictTable;
                FieldDictList = await LoadFieldDictAsync(DictTableName);
            }

            await ApplyLangDisplaySizeAsync(DictTableName, FieldDictList);
            TableFields = FieldDictList
                .Where(f => f.Visible == 1)
                .OrderBy(f => f.SerialNum ?? 0)
                .ToList();

            KeyFields = FieldDictList
                .Where(f => (f.PK ?? 0) == 1)
                .Select(f => f.FieldName)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .ToList();
            if (KeyFields.Count == 0)
            {
                if (FieldDictList.Any(f => string.Equals(f.FieldName, "CompanyId", StringComparison.OrdinalIgnoreCase)))
                    KeyFields.Add("CompanyId");
            }

            QueryFields = _ctx.CURdPaperSelected
                .Where(x => x.TableName == TableName && x.IVisible == 1)
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

            var orderBy = BuildOrderByClause(sortBy, sortDir, FieldDictList)
                ?? await GetDefaultOrderByAsync(TableName);
            var filterParams = new List<SqlParameter>();
            var filterSql = BuildFilterFromQuery(FieldDictList, Request.Query, filterParams);
            if (ApplySystemFilter)
                AppendSystemFilter(filterParams, SystemId, SubSystemId, ApplySubSystemFilter, ref filterSql);

            try
            {
                TotalCount = await CountRowsAsync(TableName, filterSql, filterParams);
                Items = await LoadRowsAsync(TableName, filterSql, orderBy, PageNumber, PageSize, filterParams);
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrWhiteSpace(sortBy))
                {
                    try
                    {
                        orderBy = await GetDefaultOrderByAsync(TableName);
                        TotalCount = await CountRowsAsync(TableName, filterSql, filterParams);
                        Items = await LoadRowsAsync(TableName, filterSql, orderBy, PageNumber, PageSize, filterParams);
                        sortBy = null;
                        sortDir = null;
                    }
                    catch (Exception ex2)
                    {
                        _logger.LogError(ex2, "Load AJNdCompany data failed (fallback) for {ItemId}", ItemId);
                        Items = new();
                        TotalCount = 0;
                        ViewData["LoadError"] = ex2.Message;
                    }
                }
                else
                {
                    _logger.LogError(ex, "Load AJNdCompany data failed for {ItemId}", ItemId);
                    Items = new();
                    TotalCount = 0;
                    ViewData["LoadError"] = ex.Message;
                }
            }

            ViewData["DictTableName"] = DictTableName;
            ViewData["FieldDictList"] = FieldDictList;
            ViewData["Fields"] = TableFields;
            ViewData["KeyFields"] = KeyFields;
            ViewData["OrderBy"] = orderBy;
            ViewData["SortBy"] = sortBy ?? string.Empty;
            ViewData["SortDir"] = sortDir ?? string.Empty;
            ViewData["QueryFields"] = QueryFields;
            ViewData["QueryStringRaw"] = Request.QueryString.Value ?? string.Empty;
            var customButtons = await LoadCustomButtonsAsync(ItemId);
            ViewData["CustomButtonMeta"] = customButtons;
            ViewData["CustomButtons"] = BuildCustomButtonsHtml(customButtons);
            ViewData["ToolbarButtonVisibility"] = BuildToolbarButtonVisibility(item);
            ViewData["EnableRowDelete"] = false;
            ViewData["AllowDeleteInView"] = false;
            ViewData["CCSCanAdd"] = CanAdd;
            ViewData["CCSCanDelete"] = CanDelete;
            ViewData["CCSCanReview"] = CanReview;
            ViewData["CCSCanBackReview"] = CanBackReview;
            ViewData["CCSCanTurnFormal"] = CanTurnFormal;
            ViewData["CCSCanDrop"] = CanDrop;

            try
            {
                ViewData["OCXLookups"] = _dictService.GetOCXLookups(DictTableName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetOCXLookups failed for {Table}", DictTableName);
            }

            return Page();
        }

        public async Task<IActionResult> OnGetDataAsync(
            [FromRoute] string? itemId,
            [FromRoute(Name = "systemId")] int? systemIdRoute,
            [FromQuery(Name = "systemId")] int? systemIdQuery,
            [FromQuery(Name = "pageIndex")] int pageIndex = 1,
            int pageSize = 50,
            string? sortBy = null,
            string? sortDir = null)
        {
            ItemId = string.IsNullOrWhiteSpace(itemId) ? ItemId : itemId.Trim();
            PageNumber = pageIndex <= 0 ? 1 : pageIndex;
            PageSize = pageSize <= 0 ? 50 : pageSize;

            var item = await _ctx.CurdSysItems.AsNoTracking()
                .Where(x => x.ItemId == ItemId)
                .Select(x => new { x.PowerType, x.PaperType })
                .FirstOrDefaultAsync();
            if (item == null)
            {
                return new JsonResult(new { success = false, error = $"Item {ItemId} not found." });
            }

            PowerType = item.PowerType;
            PaperType = item.PaperType;

            var requestedSystem = NormalizeSystemId(systemIdQuery ?? systemIdRoute);
            SystemId = ResolveSystemId(PowerType, requestedSystem);
            TableId = ResolveTableId(SystemId);
            SubSystemId = ResolveSubSystemId(PaperType);
            ApplySystemFilter = ShouldApplySystemFilter(PowerType);
            ApplySubSystemFilter = ShouldApplySubSystemFilter(PowerType, PaperType);

            DictTableName = ResolveDictTableName(TableId);
            TableName = DataTable;
            FieldDictList = await LoadFieldDictAsync(DictTableName);
            if (FieldDictList.Count == 0 && !string.Equals(DictTableName, DefaultDictTable, StringComparison.OrdinalIgnoreCase))
            {
                DictTableName = DefaultDictTable;
                FieldDictList = await LoadFieldDictAsync(DictTableName);
            }
            await ApplyLangDisplaySizeAsync(DictTableName, FieldDictList);

            var orderBy = BuildOrderByClause(sortBy, sortDir, FieldDictList)
                ?? await GetDefaultOrderByAsync(TableName);
            var filterParams = new List<SqlParameter>();
            var filterSql = BuildFilterFromQuery(FieldDictList, Request.Query, filterParams);
            if (ApplySystemFilter)
                AppendSystemFilter(filterParams, SystemId, SubSystemId, ApplySubSystemFilter, ref filterSql);

            try
            {
                var totalCount = await CountRowsAsync(TableName, filterSql, filterParams);
                var itemsData = await LoadRowsAsync(TableName, filterSql, orderBy, PageNumber, PageSize, filterParams);
                var hasQueryParams = HasQueryParams(Request.Query, FieldDictList);
                return new JsonResult(new
                {
                    success = true,
                    items = itemsData,
                    totalCount,
                    pageNumber = PageNumber,
                    pageSize = PageSize,
                    sortBy,
                    sortDir,
                    openNoRecord = false,
                    hasQueryParams
                });
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrWhiteSpace(sortBy))
                {
                    try
                    {
                        var fallbackOrder = await GetDefaultOrderByAsync(TableName);
                        var totalCount = await CountRowsAsync(TableName, filterSql, filterParams);
                        var itemsData = await LoadRowsAsync(TableName, filterSql, fallbackOrder, PageNumber, PageSize, filterParams);
                        var hasQueryParams = HasQueryParams(Request.Query, FieldDictList);
                        return new JsonResult(new
                        {
                            success = true,
                            items = itemsData,
                            totalCount,
                            pageNumber = PageNumber,
                            pageSize = PageSize,
                            sortBy = "",
                            sortDir = "",
                            openNoRecord = false,
                            hasQueryParams
                        });
                    }
                    catch (Exception ex2)
                    {
                        _logger.LogError(ex2, "Load AJNdCompany data failed (AJAX fallback) for {ItemId}", ItemId);
                    }
                }

                _logger.LogError(ex, "Load AJNdCompany data failed (AJAX) for {ItemId}", ItemId);
                return new JsonResult(new { success = false, error = ex.Message });
            }
        }

        private HtmlString BuildCustomButtonsHtml(IEnumerable<CustomButtonRow> rows)
        {
            var parts = new List<string>();
            if (CanAdd)
            {
                parts.Add("<button type='button' class='btn toolbar-btn' id='btnAddCustomer'><i class='bi bi-plus-lg'></i>新增</button>");
            }
            parts.Add("<button type='button' class='btn toolbar-btn' data-custom-btn='1' data-button-name='deletePaper'><i class='bi bi-trash3'></i>刪除</button>");
            parts.Add("<button type='button' class='btn toolbar-btn' data-custom-btn='1' data-button-name='showLog'><i class='bi bi-clock-history'></i>記錄</button>");
            parts.Add("<button type='button' class='btn toolbar-btn' data-custom-btn='1' data-button-name='showSystem'><i class='bi bi-person-badge'></i>檢視身分</button>");
            parts.Add("<button type='button' class='btn toolbar-btn' data-custom-btn='1' data-button-name='openDetail'><i class='bi bi-list-ul'></i>明細</button>");
            foreach (var b in rows ?? Enumerable.Empty<CustomButtonRow>())
            {
                if ((b.bVisible ?? 1) == 0) continue;
                var btnName = b.ButtonName ?? string.Empty;
                if (string.IsNullOrWhiteSpace(btnName)) continue;
                if (string.Equals(btnName, "btnC1", StringComparison.OrdinalIgnoreCase)) continue;
                if (string.Equals(btnName, "showSystem", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(btnName, "openDetail", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(btnName, "deletePaper", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(btnName, "rejectPaper", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(btnName, "showLog", StringComparison.OrdinalIgnoreCase))
                    continue;
                var caption = string.IsNullOrWhiteSpace(b.CustCaption) ? btnName : b.CustCaption;
                if (caption.Contains("檢查統編", StringComparison.OrdinalIgnoreCase)) continue;
                var hint = b.CustHint ?? string.Empty;
                parts.Add($"<button type='button' class='btn toolbar-btn' data-custom-btn='1' data-button-name='{System.Net.WebUtility.HtmlEncode(btnName)}' title='{System.Net.WebUtility.HtmlEncode(hint)}'>{System.Net.WebUtility.HtmlEncode(caption)}</button>");
            }
            return new HtmlString(string.Join("", parts));
        }

        private static Dictionary<string, bool> BuildToolbarButtonVisibility(dynamic item)
        {
            var map = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase)
            {
                ["btnInq"] = GetFlag(item?.BtnInq, true),
                ["btnToExcel"] = GetFlag(item?.BtnToExcel, true),
                ["btnUpdate"] = true
            };
            return map;
        }

        private async Task<List<CustomButtonRow>> LoadCustomButtonsAsync(string itemId)
        {
            var list = new List<CustomButtonRow>();
            if (string.IsNullOrWhiteSpace(itemId)) return list;

            var cs = GetConnStr();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();
            const string sql = @"
SELECT *
  FROM CURdOCXItemCustButton WITH (NOLOCK)
 WHERE ItemId = @itemId
 ORDER BY SerialNum, ButtonName;";
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@itemId", itemId);
            await using var rd = await cmd.ExecuteReaderAsync();

            var cols = Enumerable.Range(0, rd.FieldCount)
                .ToDictionary(i => rd.GetName(i), i => i, StringComparer.OrdinalIgnoreCase);
            string S(string name) => cols.TryGetValue(name, out var i) && !rd.IsDBNull(i) ? rd.GetValue(i)?.ToString() ?? string.Empty : string.Empty;
            int? I(string name)
            {
                if (!cols.TryGetValue(name, out var i) || rd.IsDBNull(i)) return null;
                return int.TryParse(rd.GetValue(i)?.ToString(), out var n) ? n : null;
            }

            while (await rd.ReadAsync())
            {
                list.Add(new CustomButtonRow
                {
                    ItemId = S("ItemId"),
                    SerialNum = I("SerialNum"),
                    ButtonName = S("ButtonName"),
                    CustCaption = S("CustCaption"),
                    CustHint = S("CustHint"),
                    OCXName = S("OCXName"),
                    CoClassName = S("CoClassName"),
                    SpName = S("SpName"),
                    ExecSpName = S("ExecSpName"),
                    SearchTemplate = S("SearchTemplate"),
                    MultiSelectDD = S("MultiSelectDD"),
                    ReplaceExists = I("ReplaceExists"),
                    DialogCaption = S("DialogCaption"),
                    AllowSelCount = I("AllowSelCount"),
                    bVisible = I("bVisible"),
                    ChkCanUpdate = I("ChkCanUpdate"),
                    bNeedNum = I("bNeedNum"),
                    bNeedInEdit = I("bNeedInEdit"),
                    ChkStatus = I("ChkStatus"),
                    bSpHasResult = I("bSpHasResult"),
                    IsUpdateMoney = I("IsUpdateMoney"),
                    iNeedConfirmBefExec = I("iNeedConfirmBefExec"),
                    sConfirmBefExec = S("sConfirmBefExec"),
                    DesignType = I("DesignType")
                });
            }

            return list;
        }

        private sealed class CustomButtonRow
        {
            public string ItemId { get; set; } = string.Empty;
            public int? SerialNum { get; set; }
            public string ButtonName { get; set; } = string.Empty;
            public string CustCaption { get; set; } = string.Empty;
            public string CustHint { get; set; } = string.Empty;
            public string OCXName { get; set; } = string.Empty;
            public string CoClassName { get; set; } = string.Empty;
            public string SpName { get; set; } = string.Empty;
            public string ExecSpName { get; set; } = string.Empty;
            public string SearchTemplate { get; set; } = string.Empty;
            public string MultiSelectDD { get; set; } = string.Empty;
            public int? ReplaceExists { get; set; }
            public string DialogCaption { get; set; } = string.Empty;
            public int? AllowSelCount { get; set; }
            public int? bVisible { get; set; }
            public int? ChkCanUpdate { get; set; }
            public int? bNeedNum { get; set; }
            public int? bNeedInEdit { get; set; }
            public int? ChkStatus { get; set; }
            public int? bSpHasResult { get; set; }
            public int? IsUpdateMoney { get; set; }
            public int? iNeedConfirmBefExec { get; set; }
            public string sConfirmBefExec { get; set; } = string.Empty;
            public int? DesignType { get; set; }
        }

        private static bool GetFlag(int? flag, bool defaultValue)
        {
            if (!flag.HasValue) return defaultValue;
            return flag.Value != 0;
        }

        private async Task<List<Dictionary<string, object?>>> LoadRowsAsync(string tableName, string? filter, string? orderBy, int page, int pageSize, List<SqlParameter>? filterParams)
        {
            var list = new List<Dictionary<string, object?>>();
            var sql = BuildSelectSql(tableName, filter, orderBy, page, pageSize);

            var cs = GetConnStr();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            await using var cmd = new SqlCommand(sql, conn);
            if (filterParams != null && filterParams.Count > 0)
                cmd.Parameters.AddRange(CloneParams(filterParams).ToArray());
            await using var rd = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess);
            var columns = Enumerable.Range(0, rd.FieldCount).Select(rd.GetName).ToList();

            while (await rd.ReadAsync())
            {
                var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < columns.Count; i++)
                {
                    row[columns[i]] = rd.IsDBNull(i) ? null : rd.GetValue(i);
                }
                list.Add(row);
            }

            return list;
        }

        private async Task<int> CountRowsAsync(string tableName, string? filter, List<SqlParameter>? filterParams)
        {
            var cs = GetConnStr();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            var where = (filter ?? string.Empty).Trim();
            var sql = new StringBuilder($"SELECT COUNT(1) FROM [{tableName}] t0");
            if (!string.IsNullOrWhiteSpace(where))
            {
                sql.Append(' ').Append(where);
            }

            await using var cmd = new SqlCommand(sql.ToString(), conn);
            if (filterParams != null && filterParams.Count > 0)
                cmd.Parameters.AddRange(CloneParams(filterParams).ToArray());
            var obj = await cmd.ExecuteScalarAsync();
            if (obj == null || obj == DBNull.Value) return 0;
            return Convert.ToInt32(obj);
        }

        private static string BuildSelectSql(string tableName, string? filter, string? orderBy, int page, int pageSize)
        {
            var sb = new StringBuilder($"SELECT * FROM [{tableName}] t0");
            var where = (filter ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(where))
            {
                sb.Append(' ').Append(where);
            }

            var order = (orderBy ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(order))
                order = "1";

            sb.Append(" ORDER BY ").Append(order);
            sb.Append(" OFFSET ").Append((page - 1) * pageSize).Append(" ROWS FETCH NEXT ").Append(pageSize).Append(" ROWS ONLY");

            return sb.ToString();
        }

        private static string? BuildOrderByClause(string? sortBy, string? sortDir, IEnumerable<CURdTableField> fields)
        {
            if (string.IsNullOrWhiteSpace(sortBy)) return null;
            var target = fields?.FirstOrDefault(f =>
                !string.IsNullOrWhiteSpace(f.FieldName)
                && string.Equals(f.FieldName, sortBy, StringComparison.OrdinalIgnoreCase));
            if (target == null || string.IsNullOrWhiteSpace(target.FieldName)) return null;
            var dir = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC";
            return $"[{target.FieldName}] {dir}";
        }

        private async Task<string> GetDefaultOrderByAsync(string tableName)
        {
            var cs = GetConnStr();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            const string sql = @"
SELECT TOP 1 c.name
  FROM sys.columns c
  JOIN sys.tables t ON t.object_id = c.object_id
 WHERE t.name = @tbl
 ORDER BY c.column_id";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@tbl", tableName);
            var result = await cmd.ExecuteScalarAsync();
            var col = result?.ToString();
            return string.IsNullOrWhiteSpace(col) ? "1" : $"[{col}]";
        }

        private async Task<List<CURdTableField>> LoadFieldDictAsync(string dictTableName)
        {
            try
            {
                return _dictService.GetFieldDict(dictTableName, typeof(object));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetFieldDict failed for {Table}", dictTableName);
                return await LoadFieldDictFallbackAsync(dictTableName);
            }
        }

        private async Task<List<CURdTableField>> LoadFieldDictFallbackAsync(string dictTableName)
        {
            var list = new List<CURdTableField>();
            var cs = GetConnStr();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            const string sql = @"
SELECT FieldName, DisplayLabel, SerialNum, Visible, DataType, DisplaySize, ReadOnly, PK
  FROM CURdTableField WITH (NOLOCK)
 WHERE TableName = @tbl
 ORDER BY SerialNum, FieldName;";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@tbl", dictTableName ?? string.Empty);

            await using var rd = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);
            while (await rd.ReadAsync())
            {
                list.Add(new CURdTableField
                {
                    TableName = dictTableName ?? string.Empty,
                    FieldName = rd["FieldName"]?.ToString() ?? string.Empty,
                    DisplayLabel = rd["DisplayLabel"] as string,
                    SerialNum = rd["SerialNum"] == DBNull.Value ? null : Convert.ToInt32(rd["SerialNum"]),
                    Visible = rd["Visible"] == DBNull.Value ? null : Convert.ToInt32(rd["Visible"]),
                    DataType = rd["DataType"] as string,
                    DisplaySize = rd["DisplaySize"] == DBNull.Value ? null : Convert.ToInt32(rd["DisplaySize"]),
                    ReadOnly = rd["ReadOnly"] == DBNull.Value ? null : Convert.ToInt32(rd["ReadOnly"]),
                    PK = rd["PK"] == DBNull.Value ? null : Convert.ToInt32(rd["PK"])
                });
            }

            return list;
        }

        private async Task ApplyLangDisplaySizeAsync(string dictTableName, List<CURdTableField> fields)
        {
            if (fields == null || fields.Count == 0) return;
            var cs = GetConnStr();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            const string sql = @"
SELECT FieldName, DisplaySize
  FROM CURdTableFieldLang WITH (NOLOCK)
 WHERE TableName = @tbl AND LanguageId = 'TW'";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@tbl", dictTableName ?? string.Empty);

            var langSize = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                var fn = rd["FieldName"]?.ToString();
                if (string.IsNullOrWhiteSpace(fn)) continue;
                if (rd["DisplaySize"] != DBNull.Value)
                    langSize[fn] = Convert.ToInt32(rd["DisplaySize"]);
            }

            if (langSize.Count == 0) return;

            foreach (var f in fields)
            {
                if (f.FieldName != null && langSize.TryGetValue(f.FieldName, out var sz))
                {
                    f.DisplaySize = sz;
                }
            }
        }

        private string BuildFilterFromQuery(IEnumerable<CURdTableField> fields, IQueryCollection query, List<SqlParameter> parameters)
        {
            var dict = fields?
                .Where(f => !string.IsNullOrWhiteSpace(f.FieldName))
                .ToDictionary(f => f.FieldName!, StringComparer.OrdinalIgnoreCase)
                ?? new Dictionary<string, CURdTableField>(StringComparer.OrdinalIgnoreCase);

            var parts = new List<string>();
            AppendCompanySelectFilter(parts, query, parameters, SystemId);
            foreach (var kv in query)
            {
                var key = kv.Key ?? string.Empty;
                if (string.IsNullOrWhiteSpace(key)) continue;
                if (key.Equals("pageIndex", StringComparison.OrdinalIgnoreCase)) continue;
                if (key.Equals("page", StringComparison.OrdinalIgnoreCase)) continue;
                if (key.Equals("pageSize", StringComparison.OrdinalIgnoreCase)) continue;
                if (key.Equals("systemId", StringComparison.OrdinalIgnoreCase)) continue;
                if (key.Equals("sortBy", StringComparison.OrdinalIgnoreCase)) continue;
                if (key.Equals("sortDir", StringComparison.OrdinalIgnoreCase)) continue;
                if (key.Equals("handler", StringComparison.OrdinalIgnoreCase)) continue;
                if (key.Equals("_sgall", StringComparison.OrdinalIgnoreCase)) continue;
                if (key.StartsWith("cs_", StringComparison.OrdinalIgnoreCase)) continue;
                if (key.StartsWith("_", StringComparison.OrdinalIgnoreCase)) continue;
                if (!dict.TryGetValue(key, out var f)) continue;

                var val = kv.Value.ToString();
                if (string.IsNullOrWhiteSpace(val)) continue;

                var paramName = $"@p{parameters.Count}";
                var dataType = f.DataType?.ToLowerInvariant() ?? string.Empty;
                var isText = string.IsNullOrEmpty(dataType)
                    || dataType.Contains("char")
                    || dataType.Contains("text")
                    || dataType.Contains("nchar")
                    || dataType.Contains("varchar");
                var op = isText ? "LIKE" : "=";
                var pVal = isText ? $"%{val}%" : val;

                parts.Add($"t0.[{f.FieldName}] {op} {paramName}");
                parameters.Add(new SqlParameter(paramName, pVal));
            }

            if (parts.Count == 0) return string.Empty;
            return "WHERE " + string.Join(" AND ", parts);
        }

        private static void AppendCompanySelectFilter(List<string> parts, IQueryCollection query, List<SqlParameter> parameters, int systemId)
        {
            static string Get(IQueryCollection q, string key) => (q[key].ToString() ?? string.Empty).Trim();
            static void AddLike(List<string> p, List<SqlParameter> ps, string field, string value)
            {
                if (string.IsNullOrWhiteSpace(value)) return;
                var pn = $"@p{ps.Count}";
                p.Add($"t0.[{field}] LIKE {pn}");
                ps.Add(new SqlParameter(pn, $"%{value}%"));
            }

            var companyLike = Get(query, "cs_companyLike");
            if (!string.IsNullOrWhiteSpace(companyLike))
            {
                var pn = $"@p{parameters.Count}";
                parts.Add($"t0.[CompanyId] LIKE {pn}");
                parameters.Add(new SqlParameter(pn, $"%{companyLike}%"));
            }

            var companyStart = Get(query, "cs_companyStart");
            if (!string.IsNullOrWhiteSpace(companyStart))
            {
                var pn = $"@p{parameters.Count}";
                parts.Add($"t0.[CompanyId] >= {pn}");
                parameters.Add(new SqlParameter(pn, companyStart));
            }

            var companyEnd = Get(query, "cs_companyEnd");
            if (!string.IsNullOrWhiteSpace(companyEnd))
            {
                var pn = $"@p{parameters.Count}";
                parts.Add($"t0.[CompanyId] <= {pn}");
                parameters.Add(new SqlParameter(pn, companyEnd));
            }

            AddLike(parts, parameters, "ShortName", Get(query, "cs_shortName"));
            AddLike(parts, parameters, "UniFormId", Get(query, "cs_uniFormId"));
            AddLike(parts, parameters, "CompanyName", Get(query, "cs_companyName"));
            AddLike(parts, parameters, "CompanyAddr", Get(query, "cs_companyAddr"));
            AddLike(parts, parameters, "BnsItem", Get(query, "cs_bnsItem"));

            var phone = Get(query, "cs_phone");
            if (!string.IsNullOrWhiteSpace(phone))
            {
                var cond = Get(query, "cs_phoneCond");
                var pn = $"@p{parameters.Count}";
                var keyword = string.Equals(cond, "1", StringComparison.OrdinalIgnoreCase)
                    ? $"%{phone}%"
                    : $"{phone}%";
                parts.Add($"(t0.[Phone1] LIKE {pn} OR t0.[Phone2] LIKE {pn})");
                parameters.Add(new SqlParameter(pn, keyword));
            }

            var fax = Get(query, "cs_fax");
            if (!string.IsNullOrWhiteSpace(fax))
            {
                var cond = Get(query, "cs_faxCond");
                var pn = $"@p{parameters.Count}";
                var keyword = string.Equals(cond, "1", StringComparison.OrdinalIgnoreCase)
                    ? $"%{fax}%"
                    : $"{fax}%";
                parts.Add($"(t0.[Fax1] LIKE {pn} OR t0.[Fax2] LIKE {pn})");
                parameters.Add(new SqlParameter(pn, keyword));
            }

            var subClass = Get(query, "cs_subClass");
            if (!string.IsNullOrWhiteSpace(subClass) && systemId >= 1 && systemId <= 9)
            {
                var field = $"CustomerSubClass{systemId}";
                var pn = $"@p{parameters.Count}";
                parts.Add($"t0.[{field}] = {pn}");
                parameters.Add(new SqlParameter(pn, subClass));
            }

            var salesId = Get(query, "cs_salesId");
            if (!string.IsNullOrWhiteSpace(salesId))
            {
                var pn = $"@p{parameters.Count}";
                parts.Add($"t0.[SalesId] = {pn}");
                parameters.Add(new SqlParameter(pn, salesId));
            }
        }

        private static bool HasQueryParams(IQueryCollection query, IEnumerable<CURdTableField> fields)
        {
            var validFields = fields?
                .Where(f => !string.IsNullOrWhiteSpace(f.FieldName))
                .Select(f => f.FieldName!)
                .ToHashSet(StringComparer.OrdinalIgnoreCase)
                ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var kv in query)
            {
                var key = kv.Key ?? string.Empty;
                if (string.IsNullOrWhiteSpace(key)) continue;
                if (IsControlQueryKey(key)) continue;
                if (key.StartsWith("cs_", StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.IsNullOrWhiteSpace(kv.Value.ToString())) return true;
                    continue;
                }
                if (!validFields.Contains(key)) continue;
                if (!string.IsNullOrWhiteSpace(kv.Value.ToString())) return true;
            }
            return false;
        }

        private static bool IsControlQueryKey(string key)
        {
            if (key.Equals("pageIndex", StringComparison.OrdinalIgnoreCase)) return true;
            if (key.Equals("page", StringComparison.OrdinalIgnoreCase)) return true;
            if (key.Equals("pageSize", StringComparison.OrdinalIgnoreCase)) return true;
            if (key.Equals("systemId", StringComparison.OrdinalIgnoreCase)) return true;
            if (key.Equals("sortBy", StringComparison.OrdinalIgnoreCase)) return true;
            if (key.Equals("sortDir", StringComparison.OrdinalIgnoreCase)) return true;
            if (key.Equals("handler", StringComparison.OrdinalIgnoreCase)) return true;
            if (key.Equals("_sgall", StringComparison.OrdinalIgnoreCase)) return true;
            if (key.StartsWith("_", StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        private string GetConnStr()
        {
            var cs = _ctx.Database.GetConnectionString();
            if (string.IsNullOrWhiteSpace(cs))
                throw new InvalidOperationException("Connection string is not configured.");
            return cs;
        }

        private static IEnumerable<SqlParameter> CloneParams(IEnumerable<SqlParameter> source)
        {
            foreach (var p in source)
            {
                var clone = new SqlParameter(p.ParameterName, p.Value)
                {
                    DbType = p.DbType,
                    Direction = p.Direction
                };
                yield return clone;
            }
        }

        private static int NormalizeSystemId(int? id)
        {
            if (id.HasValue && id.Value > 0) return id.Value;
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

        private static string ResolveDictTableName(int tableId)
        {
            if (tableId <= 0) return DefaultDictTable;
            return $"AJNdCompany_{tableId}";
        }

        private static bool ShouldApplySystemFilter(int? powerType)
        {
            if (!powerType.HasValue) return true;
            return powerType.Value != 101;
        }

        private static bool ShouldApplySubSystemFilter(int? powerType, int? paperType)
        {
            if (!paperType.HasValue || paperType.Value == 255) return false;
            if (!powerType.HasValue) return false;

            var pt = powerType.Value;
            return (pt <= 100 || pt == 103 || pt == 104) && pt != 101;
        }

        private static bool CanAddOrDelete(int? powerType)
        {
            if (!powerType.HasValue) return true;
            var pt = powerType.Value;
            if (pt == 101 || pt == 103 || pt == 104) return false;
            return true;
        }

        private static void AppendSystemFilter(List<SqlParameter> parameters, int systemId, int subSystemId, bool withSubSystem, ref string filterSql)
        {
            const string head = "EXISTS (SELECT 1 FROM AJNdCompanySystem s WHERE s.CompanyId = t0.CompanyId";
            var clause = new StringBuilder(head)
                .Append(" AND s.SystemId = @sysId");
            parameters ??= new List<SqlParameter>();
            parameters.Add(new SqlParameter("@sysId", systemId));

            if (withSubSystem)
            {
                clause.Append(" AND s.SubSystemId = @subSystemId");
                parameters.Add(new SqlParameter("@subSystemId", subSystemId));
            }

            clause.Append(')');
            var finalClause = clause.ToString();

            var where = (filterSql ?? string.Empty).Trim();
            if (where.Length == 0)
            {
                filterSql = "WHERE " + finalClause;
            }
            else if (where.StartsWith("WHERE", StringComparison.OrdinalIgnoreCase))
            {
                filterSql = where + " AND " + finalClause;
            }
            else
            {
                filterSql = "WHERE " + where + " AND " + finalClause;
            }
        }
    }
}
