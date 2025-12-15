using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PcbErpApi.Data;
using PcbErpApi.Models;

namespace PcbErpApi.Pages.CUR
{
    public class MultiGridModel : PageModel
    {
        private readonly PcbErpContext _ctx;
        private readonly ITableDictionaryService _dictService;
        private readonly ILogger<MultiGridModel> _logger;

        public MultiGridModel(PcbErpContext ctx, ITableDictionaryService dictService, ILogger<MultiGridModel> logger)
        {
            _ctx = ctx;
            _dictService = dictService;
            _logger = logger;
        }

        public string ItemId { get; private set; } = string.Empty;
        public string? ItemName { get; private set; }
        public string PageTitle => string.IsNullOrWhiteSpace(ItemName) ? ItemId : $"{ItemId}{ItemName}";
        public List<GridTabViewModel> Tabs { get; private set; } = new();
        public string ActiveTabKey { get; private set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(string itemId, string? tab = null)
        {
            if (string.IsNullOrWhiteSpace(itemId))
                return NotFound("itemId is required");

            var item = await _ctx.CurdSysItems.AsNoTracking()
                .SingleOrDefaultAsync(x => x.ItemId == itemId);

            if (item is null)
                return NotFound($"Item {itemId} not found.");
            //確認這個 Item 是 JSdGridDLL.dll 且 ItemType = 6
            if (item.ItemType != 6 || !string.Equals(item.Ocxtemplete, "JSdGridDLL.dll", StringComparison.OrdinalIgnoreCase))
                return NotFound($"Item {itemId} is not a JSdGridDLL multi-grid item.");


            ItemId = item.ItemId;
            ItemName = item.ItemName;
            ViewData["Title"] = PageTitle;
            //查這個 Item 底下要顯示哪些表
            var setups = await _ctx.CurdOcxtableSetUp.AsNoTracking()
                .Where(x => x.ItemId == itemId)
                .OrderBy(x => x.TableKind)
                .ThenBy(x => x.TableName)
                .ToListAsync();//把上面這個查詢「真正送去資料庫執行」，然後把結果裝成 List<CurdOcxtableSetUp>。Async 代表是「非同步」版本，所以前面要 await。

            if (setups.Count == 0)
                return NotFound($"CURdOCXTableSetUp not found for item {itemId}.");

            var seenKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            int idx = 0;
            foreach (var setup in setups)
            {
                var tableMeta = await GetTableMetaAsync(setup.TableName);
                var tabKey = BuildTabKey(setup.TableKind, setup.TableName, idx, seenKeys);  // 產生 Tab 的 key（供 URL/前端用）
                var tabLabel = tableMeta?.DisplayLabel ?? (string.IsNullOrWhiteSpace(setup.TableKind) ? setup.TableName : setup.TableKind);
                var pageNumber = ResolvePageIndex(tabKey);
                var pageSize = ResolvePageSize(tabKey);
                var dictTableName = setup.TableName;
                var realTableName = tableMeta?.RealName ?? dictTableName;

                var tabVm = new GridTabViewModel(tabKey, tabLabel, dictTableName, realTableName)
                {
                    TableKind = setup.TableKind ?? string.Empty,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                try
                {   // 1) 讀欄位字典（顯示哪些欄位、順序…）
                    tabVm.FieldDictList = await LoadFieldDictAsync(dictTableName);
                    await ApplyLangDisplaySizeAsync(dictTableName, tabVm.FieldDictList);
                    tabVm.TableFields = tabVm.FieldDictList
                        .Where(f => f.Visible == 1)
                        .OrderBy(f => f.SerialNum ?? 0)
                        .ToList();
                     // 2) 排序欄位
                    tabVm.OrderBy = string.IsNullOrWhiteSpace(setup.OrderByField)
                        ? await GetDefaultOrderByAsync(tabVm.TableName)
                        : setup.OrderByField;
                    // 3) 查詢欄位設定（CURdOCXPaperSelOtherGet）
                    tabVm.QueryFields = await LoadQueryFieldsAsync(itemId, tabVm.TableName, "TW");
                    var filterParams = new List<SqlParameter>();
                    var filterSql = BuildFilterSql(tabVm.QueryFields, Request.Query, filterParams);
                    var combinedFilter = CombineFilter(setup.FilterSql, filterSql);

                    tabVm.TotalCount = await CountRowsAsync(tabVm.TableName, combinedFilter, filterParams);
                    tabVm.Items = await LoadRowsAsync(tabVm.TableName, combinedFilter, tabVm.OrderBy, tabVm.PageNumber, tabVm.PageSize, filterParams);
                    tabVm.KeyFields = BuildKeyFields(setup.Mdkey, setup.LocateKeys);
                    tabVm.OcxLookups = await GetLookupMapAsync(dictTableName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Load multi-grid tab failed for {ItemId} table {Table}", itemId, dictTableName);
                    tabVm.Items = new();
                    tabVm.TotalCount = 0;
                    ViewData["LoadError"] = ex.Message;
                }

                Tabs.Add(tabVm);
                idx++;
            }

            ActiveTabKey = DetermineActiveTab(tab, Tabs);
            return Page();
        }

        private string BuildTabKey(string? tableKind, string tableName, int index, HashSet<string> seen)
        {
            var baseKey = string.IsNullOrWhiteSpace(tableKind) ? tableName : tableKind;
            baseKey = baseKey?.Trim() ?? $"tab{index + 1}";
            if (string.IsNullOrWhiteSpace(baseKey))
                baseKey = $"tab{index + 1}";

            var safe = Regex.Replace(baseKey, "[^a-zA-Z0-9_-]", "_");
            if (string.IsNullOrWhiteSpace(safe))
                safe = $"tab{index + 1}";

            var finalKey = safe;
            int seq = 1;
            while (seen.Contains(finalKey))
            {
                finalKey = $"{safe}_{seq++}";
            }
            seen.Add(finalKey);
            return finalKey;
        }

        private string DetermineActiveTab(string? requestTab, List<GridTabViewModel> tabs)
        {
            if (!string.IsNullOrWhiteSpace(requestTab) && tabs.Any(t => string.Equals(t.TabKey, requestTab, StringComparison.OrdinalIgnoreCase)))
                return tabs.First(t => string.Equals(t.TabKey, requestTab, StringComparison.OrdinalIgnoreCase)).TabKey;

            return tabs.FirstOrDefault()?.TabKey ?? string.Empty;
        }

        private int ResolvePageIndex(string tabKey)
        {
            var altKey = $"pageIndex_{tabKey}";
            if (Request.Query.TryGetValue(altKey, out var altVals) && int.TryParse(altVals.ToString(), out var altPage) && altPage > 0)
                return altPage;

            if (Request.Query.TryGetValue("pageIndex", out var pageVals) && int.TryParse(pageVals.ToString(), out var page) && page > 0)
                return page;

            if (Request.Query.TryGetValue("page", out var legacyVals) && int.TryParse(legacyVals.ToString(), out var legacy) && legacy > 0)
                return legacy;

            return 1;
        }

        private int ResolvePageSize(string tabKey)
        {
            var altKey = $"pageSize_{tabKey}";
            if (Request.Query.TryGetValue(altKey, out var altVals) && int.TryParse(altVals.ToString(), out var altSize) && altSize > 0)
                return altSize;

            if (Request.Query.TryGetValue("pageSize", out var pageSizeVals) && int.TryParse(pageSizeVals.ToString(), out var size) && size > 0)
                return size;

            return 50;
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

        private async Task<Dictionary<string, TableDictionaryService.OCXLookupMap>> GetLookupMapAsync(string dictTableName)
        {
            var result = new Dictionary<string, TableDictionaryService.OCXLookupMap>(StringComparer.OrdinalIgnoreCase);
            try
            {
                var lookups = _dictService.GetOCXLookups(dictTableName);
                foreach (var lk in lookups)
                {
                    if (!string.IsNullOrWhiteSpace(lk.FieldName))
                        result[lk.FieldName] = lk;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetOCXLookups failed for {Table}", dictTableName);
            }

            return result;
        }

        private async Task<TableMeta?> GetTableMetaAsync(string dictTableName)
        {
            var cs = GetConnStr();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
SELECT TOP 1 
       ISNULL(NULLIF(RealTableName,''), TableName) AS ActualName,
       ISNULL(NULLIF(DisplayLabel,''), TableName) AS DisplayLabel
  FROM CURdTableName WITH (NOLOCK)
 WHERE TableName = @tbl";
            var p = cmd.CreateParameter();
            p.ParameterName = "@tbl";
            p.Value = dictTableName ?? string.Empty;
            cmd.Parameters.Add(p);

            await using var rd = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow);
            if (!await rd.ReadAsync()) return null;

            var realName = rd["ActualName"] == DBNull.Value ? null : rd["ActualName"]?.ToString();
            var displayLabel = rd["DisplayLabel"] == DBNull.Value ? null : rd["DisplayLabel"]?.ToString();

            return new TableMeta
            {
                RealName = realName,
                DisplayLabel = displayLabel
            };
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

        private static List<string> BuildKeyFields(string? mdKey, string? locateKeys)
        {
            var raw = string.IsNullOrWhiteSpace(mdKey) ? locateKeys : mdKey;
            if (string.IsNullOrWhiteSpace(raw)) return new List<string>();

            return raw
                .Split(new[] { ';', ',', '|' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();
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

        public class QueryFieldDef
        {
            public string ColumnName { get; set; } = "";
            public string ColumnCaption { get; set; } = "";
            public int? DataType { get; set; }
            public int? ControlType { get; set; }
            public string? DefaultValue { get; set; }
            public string? DefaultEqual { get; set; }
            public string? CommandText { get; set; }
        }

        private async Task<List<QueryFieldDef>> LoadQueryFieldsAsync(string itemId, string tableName, string lang)
        {
            var list = new List<QueryFieldDef>();
            var cs = GetConnStr();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            using var cmd = new SqlCommand("exec CURdOCXPaperSelOtherGet @p0,@p1,@p2", conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@p0", itemId);
            cmd.Parameters.AddWithValue("@p1", tableName);
            cmd.Parameters.AddWithValue("@p2", lang ?? "TW");

            using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                list.Add(new QueryFieldDef
                {
                    ColumnName = rd["ColumnName"]?.ToString() ?? "",
                    ColumnCaption = rd["ColumnCaption"]?.ToString() ?? rd["old_ColumnCaption"]?.ToString() ?? "",
                    DataType = TryToInt(rd["DataType"]),
                    ControlType = TryToInt(rd["ControlType"]),
                    DefaultValue = rd["DefaultValue"]?.ToString(),
                    DefaultEqual = rd["DefaultEqual"]?.ToString(),
                    CommandText = rd["CommandText"]?.ToString()
                });
            }
            return list;
        }

        private static int? TryToInt(object? o)
        {
            if (o == null || o == DBNull.Value) return null;
            return int.TryParse(o.ToString(), out var n) ? n : null;
        }

        private string BuildFilterSql(List<QueryFieldDef> defs, IQueryCollection query, List<SqlParameter> parameters)
        {
            if (defs == null || defs.Count == 0) return string.Empty;
            var parts = new List<string>();
            int idx = 0;

            foreach (var def in defs)
            {
                if (string.IsNullOrWhiteSpace(def.ColumnName)) continue;
                var key = def.ColumnName;
                var val = query[key].ToString();
                if (string.IsNullOrWhiteSpace(val)) continue;

                var paramName = $"@p{idx++}";
                string op = ResolveOperator(def.DefaultEqual);
                if (op.Equals("LIKE", StringComparison.OrdinalIgnoreCase))
                {
                    val = $"%{val}%";
                }
                var colSql = $"t0.[{def.ColumnName}]";
                parts.Add($"{colSql} {op} {paramName}");
                parameters.Add(new SqlParameter(paramName, val));
            }

            if (parts.Count == 0) return string.Empty;
            return string.Join(" AND ", parts);
        }

        private static string ResolveOperator(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "=";
            var s = raw.Trim().ToLowerInvariant();
            if (s.Contains("like")) return "LIKE";
            if (s.Contains(">=")) return ">=";
            if (s.Contains("<=")) return "<=";
            if (s.Contains("<>")) return "<>";
            if (s.Contains(">")) return ">";
            if (s.Contains("<")) return "<";
            return "=";
        }

        private static string CombineFilter(string? baseFilter, string? extra)
        {
            var f1 = NormalizeFilter(baseFilter);
            var f2 = NormalizeFilter(extra);
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(f1)) parts.Add(f1);
            if (!string.IsNullOrWhiteSpace(f2)) parts.Add(f2);
            if (parts.Count == 0) return string.Empty;
            return "WHERE " + string.Join(" AND ", parts);
        }

        private static IEnumerable<SqlParameter> CloneParams(IEnumerable<SqlParameter> source)
        {
            foreach (var p in source)
            {
                var clone = new SqlParameter(p.ParameterName, p.Value);
                clone.DbType = p.DbType;
                clone.Direction = p.Direction;
                yield return clone;
            }
        }

        private static string NormalizeFilter(string? raw)
        {
            var s = (raw ?? "").Trim();
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            if (s.StartsWith("where", StringComparison.OrdinalIgnoreCase))
                s = s.Substring(5).Trim();
            if (s.StartsWith("and", StringComparison.OrdinalIgnoreCase))
                s = s.Substring(3).Trim();
            if (s.StartsWith("or", StringComparison.OrdinalIgnoreCase))
                s = s.Substring(2).Trim();
            return s.Trim();
        }

        private string GetConnStr()
        {
            var cs = _ctx.Database.GetConnectionString();
            if (string.IsNullOrWhiteSpace(cs))
                throw new InvalidOperationException("Connection string is not configured.");
            return cs;
        }

        private async Task<List<CURdTableField>> LoadFieldDictFallbackAsync(string dictTableName)
        {
            var list = new List<CURdTableField>();
            var cs = GetConnStr();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            const string sql = @"
SELECT FieldName, DisplayLabel, SerialNum, Visible, DataType, DisplaySize, ReadOnly
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
                    TableName = dictTableName,
                    FieldName = rd["FieldName"]?.ToString() ?? string.Empty,
                    DisplayLabel = rd["DisplayLabel"] as string,
                    SerialNum = rd["SerialNum"] == DBNull.Value ? null : Convert.ToInt32(rd["SerialNum"]),
                    Visible = rd["Visible"] == DBNull.Value ? null : Convert.ToInt32(rd["Visible"]),
                    DataType = rd["DataType"] as string,
                    DisplaySize = rd["DisplaySize"] == DBNull.Value ? null : Convert.ToInt32(rd["DisplaySize"]),
                    ReadOnly = rd["ReadOnly"] == DBNull.Value ? null : Convert.ToInt32(rd["ReadOnly"])
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

        public class GridTabViewModel
        {
            public GridTabViewModel(string tabKey, string tabLabel, string dictTableName, string tableName)
            {
                TabKey = tabKey;
                TabLabel = tabLabel;
                DictTableName = dictTableName;
                TableName = tableName;
            }

            public string TabKey { get; }
            public string TabLabel { get; }
            public string DictTableName { get; set; }
            public string TableName { get; set; }
            public string TableKind { get; set; } = string.Empty;
            public int PageNumber { get; set; } = 1;
            public int PageSize { get; set; } = 50;
            public int TotalCount { get; set; }
            public string? OrderBy { get; set; }
            public List<Dictionary<string, object?>> Items { get; set; } = new();
            public List<CURdTableField> FieldDictList { get; set; } = new();
            public List<CURdTableField> TableFields { get; set; } = new();
            public List<QueryFieldDef> QueryFields { get; set; } = new();
            public List<string> KeyFields { get; set; } = new();
            public Dictionary<string, TableDictionaryService.OCXLookupMap> OcxLookups { get; set; } = new(StringComparer.OrdinalIgnoreCase);
            public string DisplayName => string.IsNullOrWhiteSpace(TabLabel) ? DictTableName : TabLabel;
            public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)Math.Max(1, PageSize)));
        }

        private class TableMeta
        {
            public string? RealName { get; set; }
            public string? DisplayLabel { get; set; }
        }
    }
}
