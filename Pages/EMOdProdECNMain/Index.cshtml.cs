using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PcbErpApi.Data;
using PcbErpApi.Helpers;
using PcbErpApi.Models;
using PcbErpApi.Services;

namespace PcbErpApi.Pages.EMOdProdECNMain
{
    public class IndexModel : PageModel
    {
        private readonly PcbErpContext _ctx;
        private readonly ITableDictionaryService _dictService;
        private readonly ILogger<IndexModel> _logger;
        private readonly IBreadcrumbService _breadcrumbService;

        private const string DictTable = "EMOdProdECNMain";
        private const string DataTable = "EMOdProdECNMain";

        public IndexModel(PcbErpContext ctx, ITableDictionaryService dictService,
            ILogger<IndexModel> logger, IBreadcrumbService breadcrumbService)
        {
            _ctx = ctx;
            _dictService = dictService;
            _logger = logger;
            _breadcrumbService = breadcrumbService;
        }

        [FromQuery(Name = "mode")]
        public string? Mode { get; set; }

        [FromQuery(Name = "itemId")]
        public string? RequestedItemId { get; set; }

        public bool IsViewOnly =>
            !string.IsNullOrEmpty(Mode)
                && string.Equals(Mode, "view", StringComparison.OrdinalIgnoreCase);

        public string ItemId => "EMO00019";
        public string PageTitle => "EMO00019 工程變更審核";
        public string TableName { get; private set; } = DataTable;
        public string DictTableName { get; private set; } = DictTable;
        public int PageNumber { get; private set; } = 1;
        public int PageSize { get; private set; } = 50;
        public int TotalCount { get; private set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public List<Dictionary<string, object?>> Items { get; private set; } = new();
        public List<CURdTableField> FieldDictList { get; private set; } = new();
        public List<TableFieldViewModel> TableFields { get; private set; } = new();
        public List<QueryFieldViewModel> QueryFields { get; private set; } = new();
        public List<string> KeyFields { get; private set; } = new();

        // AJAX
        public async Task<IActionResult> OnGetDataAsync(
            string? sortBy = null, string? sortDir = null,
            int pageIndex = 1, int pageSize = 50)
        {
            try
            {
                FieldDictList = await LoadFieldDictAsync(DictTableName);
                var orderBy = BuildOrderByClause(sortBy, sortDir)
                              ?? await GetDefaultOrderByAsync(DataTable);
                var totalCount = await CountRowsAsync(DataTable, string.Empty, null);
                var items = await LoadRowsAsync(DataTable, string.Empty, orderBy, pageIndex, pageSize, null);

                return new JsonResult(new
                {
                    success = true,
                    items,
                    totalCount,
                    pageNumber = pageIndex,
                    pageSize,
                    sortBy,
                    sortDir
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Load EMOdProdECNMain data failed (AJAX)");
                return new JsonResult(new { success = false, error = ex.Message });
            }
        }

        public async Task OnGetAsync(
            int page = 1, int pageIndex = 1, int pageSize = 50,
            string? sortBy = null, string? sortDir = null)
        {
            var effectivePage = page > 1 ? page : pageIndex;
            PageNumber = effectivePage <= 0 ? 1 : effectivePage;
            PageSize = pageSize <= 0 ? 50 : pageSize;
            ViewData["Title"] = PageTitle;
            ViewData["SortBy"] = sortBy;
            ViewData["SortDir"] = sortDir;
            ViewData["IsViewOnly"] = IsViewOnly;

            // 麵包屑
            var sysItem = await _ctx.CurdSysItems
                .Where(x => x.ItemId == "EMO00019")
                .FirstOrDefaultAsync();
            if (sysItem != null)
                ViewData["Breadcrumbs"] = await _breadcrumbService.BuildBreadcrumbsAsync(sysItem.SuperId);

            FieldDictList = await LoadFieldDictAsync(DictTableName);
            await ApplyLangDisplaySizeAsync(DictTableName, FieldDictList);

            QueryFields = FieldDictList
                .Where(f => f.iShowWhere == 1)
                .OrderBy(f => f.SerialNum ?? 0)
                .Select(f => new QueryFieldViewModel
                {
                    ColumnName = f.FieldName ?? "",
                    ColumnCaption = f.DisplayLabel ?? f.FieldName ?? "",
                    DataType = GetDataTypeCode(f.DataType),
                    ControlType = 0,
                    DefaultEqual = "like",
                    SortOrder = f.SerialNum
                })
                .ToList();

            TableFields = FieldDictList
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
                })
                .ToList();

            // 主鍵：PaperNum
            KeyFields = FieldDictList
                .Where(f => (f.PK ?? 0) == 1)
                .Select(f => f.FieldName)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .ToList();
            if (KeyFields.Count == 0)
                KeyFields.Add("PaperNum");

            var orderBy = BuildOrderByClause(sortBy, sortDir)
                          ?? "PaperDate DESC, PaperNum DESC";

            try
            {
                TotalCount = await CountRowsAsync(DataTable, string.Empty, null);
                Items = await LoadRowsAsync(DataTable, string.Empty, orderBy, PageNumber, PageSize, null);
                EnrichWithOcxLookups(Items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Load EMOdProdECNMain data failed");
                Items = new();
                TotalCount = 0;
                ViewData["LoadError"] = ex.Message;
            }

            ViewData["DictTableName"] = DictTableName;
            ViewData["FieldDictList"] = FieldDictList;
            ViewData["Fields"] = TableFields;
            ViewData["KeyFields"] = KeyFields;
            ViewData["QueryFields"] = QueryFields;
            ViewData["OrderBy"] = orderBy;
            ViewData["QueryStringRaw"] = Request.QueryString.Value ?? string.Empty;
            ViewData["KeyFieldName"] = "PaperNum";
            ViewData["PagedQueryUrl"] = "/api/DynamicTable/PagedQuery";
            ViewData["AddApiUrl"] = $"/api/DynamicTable/AddPaper/{TableName}";
            ViewData["PaginationVm"] = new PaginationModel
            {
                PageNumber = PageNumber,
                TotalPages = TotalPages,
                RouteUrl = Url.Page("/EMOdProdECNMain/Index") ?? "/EMOdProdECNMain"
            };
            try
            {
                ViewData["OCXLookups"] = _dictService.GetOCXLookups(DictTableName);
                var lookupMaps = _dictService.GetOCXLookups(DictTableName);
                ViewData["LookupDisplayMap"] = LookupDisplayHelper.BuildLookupDisplayMap(
                    Items,
                    lookupMaps,
                    item => GetDictValue(item, "PaperNum")
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetOCXLookups failed for {Table}", DictTableName);
                ViewData["LookupDisplayMap"] = new Dictionary<string, Dictionary<string, string>>();
            }
        }

        // ── private helpers ────────────────────────────────────────────────

        private async Task<List<Dictionary<string, object?>>> LoadRowsAsync(
            string tableName, string? filter, string? orderBy,
            int page, int pageSize, List<SqlParameter>? filterParams)
        {
            var list = new List<Dictionary<string, object?>>();
            var sql = BuildSelectSql(tableName, filter, orderBy, page, pageSize);
            var cs = GetConnStr();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(sql, conn);
            if (filterParams?.Count > 0)
                cmd.Parameters.AddRange(CloneParams(filterParams).ToArray());
            await using var rd = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess);
            var columns = Enumerable.Range(0, rd.FieldCount).Select(rd.GetName).ToList();
            while (await rd.ReadAsync())
            {
                var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < columns.Count; i++)
                    row[columns[i]] = rd.IsDBNull(i) ? null : rd.GetValue(i);
                list.Add(row);
            }
            return list;
        }

        private async Task<int> CountRowsAsync(
            string tableName, string? filter, List<SqlParameter>? filterParams)
        {
            var cs = GetConnStr();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();
            var where = (filter ?? string.Empty).Trim();
            var sql = new StringBuilder($"SELECT COUNT(1) FROM [{tableName}] t0");
            if (!string.IsNullOrWhiteSpace(where)) sql.Append(' ').Append(where);
            await using var cmd = new SqlCommand(sql.ToString(), conn);
            if (filterParams?.Count > 0)
                cmd.Parameters.AddRange(CloneParams(filterParams).ToArray());
            var obj = await cmd.ExecuteScalarAsync();
            return obj == null || obj == DBNull.Value ? 0 : Convert.ToInt32(obj);
        }

        private static string BuildSelectSql(
            string tableName, string? filter, string? orderBy, int page, int pageSize)
        {
            var sb = new StringBuilder($"SELECT * FROM [{tableName}] t0");
            var where = (filter ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(where)) sb.Append(' ').Append(where);
            var order = (orderBy ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(order)) order = "1";
            sb.Append(" ORDER BY ").Append(order)
              .Append(" OFFSET ").Append((page - 1) * pageSize)
              .Append(" ROWS FETCH NEXT ").Append(pageSize).Append(" ROWS ONLY");
            return sb.ToString();
        }

        private string? BuildOrderByClause(string? sortBy, string? sortDir)
        {
            if (string.IsNullOrWhiteSpace(sortBy)) return null;
            if (!Regex.IsMatch(sortBy, @"^[a-zA-Z_][a-zA-Z0-9_]*$")) return null;
            var direction = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase)
                ? "DESC" : "ASC";
            return $"[{sortBy}] {direction}";
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
                return _dictService.GetFieldDict(dictTableName, typeof(EmodProdEcnMain));
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
            foreach (var f in fields)
            {
                if (f.FieldName != null && langSize.TryGetValue(f.FieldName, out var sz))
                    f.DisplaySize = sz;
            }
        }

        private static int GetDataTypeCode(string? dataType)
        {
            if (string.IsNullOrWhiteSpace(dataType)) return 0;
            var dt = dataType.ToLowerInvariant();
            if (dt.Contains("date") || dt.Contains("time")) return 1;
            if (dt.Contains("int") || dt.Contains("decimal") || dt.Contains("numeric") ||
                dt.Contains("float") || dt.Contains("money") || dt.Contains("number")) return 2;
            return 0;
        }

        private void EnrichWithOcxLookups(List<Dictionary<string, object?>> items)
        {
            if (items == null || items.Count == 0) return;
            try
            {
                var lookupMaps = _dictService.GetOCXLookups(DictTableName);
                if (lookupMaps.Count == 0) return;

                foreach (var row in items)
                {
                    foreach (var map in lookupMaps)
                    {
                        if (map == null || string.IsNullOrWhiteSpace(map.FieldName)) continue;
                        if (row.ContainsKey(map.FieldName)) continue;

                        var key = "";
                        if (!string.IsNullOrWhiteSpace(map.KeyFieldName)
                            && row.TryGetValue(map.KeyFieldName, out var kfv))
                            key = kfv?.ToString()?.Trim() ?? "";
                        if (string.IsNullOrWhiteSpace(key)
                            && !string.IsNullOrWhiteSpace(map.KeySelfName)
                            && row.TryGetValue(map.KeySelfName, out var ksv))
                            key = ksv?.ToString()?.Trim() ?? "";

                        if (!string.IsNullOrWhiteSpace(key)
                            && map.LookupValues != null
                            && map.LookupValues.TryGetValue(key, out var display))
                            row[map.FieldName] = display;
                        else
                            row[map.FieldName] = "";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "EnrichWithOcxLookups failed for {Table}", DictTableName);
            }
        }

        private static string? GetDictValue(Dictionary<string, object?> item, string key)
            => item.TryGetValue(key, out var v) ? v?.ToString() : null;

        private static IEnumerable<SqlParameter> CloneParams(IEnumerable<SqlParameter> src)
            => src.Select(p => new SqlParameter(p.ParameterName, p.Value));

        private string GetConnStr()
        {
            var cs = _ctx.Database.GetConnectionString();
            if (string.IsNullOrWhiteSpace(cs))
                throw new InvalidOperationException("Connection string is not configured.");
            return cs;
        }
    }
}
