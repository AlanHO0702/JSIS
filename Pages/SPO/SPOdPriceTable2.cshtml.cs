using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PcbErpApi.Data;
using PcbErpApi.Models;

namespace PcbErpApi.Pages.SPO
{
    public class SPOdPriceTable2Model : PageModel
    {
        private readonly PcbErpContext _ctx;
        private readonly ITableDictionaryService _dictService;
        private readonly ILogger<SPOdPriceTable2Model> _logger;

        private const string DictTable = "SPOdPriceTable2";

        public SPOdPriceTable2Model(PcbErpContext ctx, ITableDictionaryService dictService, ILogger<SPOdPriceTable2Model> logger)
        {
            _ctx = ctx;
            _dictService = dictService;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public string? CustomerId { get; set; }

        [BindProperty(SupportsGet = true, Name = "modal")]
        public string? Modal { get; set; }

        public bool IsModal =>
            string.Equals(Modal, "1", StringComparison.OrdinalIgnoreCase)
            || string.Equals(Modal, "true", StringComparison.OrdinalIgnoreCase);

        [BindProperty(SupportsGet = true, Name = "pageIndex")]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true, Name = "pageSize")]
        public int PageSize { get; set; } = 50;

        public string PageTitle => "價格表";
        public string DictTableName { get; private set; } = DictTable;
        public string TableName { get; private set; } = DictTable;
        public int TotalCount { get; private set; }
        public List<Dictionary<string, object?>> Items { get; private set; } = new();
        public List<CURdTableField> FieldDictList { get; private set; } = new();
        public List<CURdTableField> TableFields { get; private set; } = new();
        public string? LoadError { get; private set; }

        public async Task<IActionResult> OnGetDataAsync(string? sortBy = null, string? sortDir = null, int pageIndex = 1, int pageSize = 50)
        {
            try
            {
                FieldDictList = await LoadFieldDictAsync(DictTableName);
                TableName = await ResolveRealTableNameAsync(DictTableName) ?? DictTableName;

                if (string.IsNullOrWhiteSpace(CustomerId))
                {
                    return new JsonResult(new
                    {
                        success = true,
                        items = new List<Dictionary<string, object?>>(),
                        totalCount = 0,
                        pageNumber = pageIndex,
                        pageSize = pageSize,
                        sortBy,
                        sortDir
                    });
                }

                var filterParams = new List<SqlParameter>();
                var filter = BuildFilter(filterParams);
                var orderBy = BuildOrderByClause(sortBy, sortDir) ?? await GetDefaultOrderByAsync(TableName);

                var totalCount = await CountRowsAsync(TableName, filter, filterParams);
                var items = await LoadRowsAsync(TableName, filter, orderBy, pageIndex, pageSize, filterParams);

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
                _logger.LogError(ex, "Load SPOdPriceTable2 data failed (AJAX)");
                return new JsonResult(new { success = false, error = ex.Message });
            }
        }

        public async Task OnGetAsync([FromQuery(Name = "pageIndex")] int pageIndex = 1, int pageSize = 50, string? sortBy = null, string? sortDir = null)
        {
            PageNumber = pageIndex <= 0 ? 1 : pageIndex;
            PageSize = pageSize <= 0 ? 50 : pageSize;
            ViewData["Title"] = PageTitle;
            ViewData["SortBy"] = sortBy;
            ViewData["SortDir"] = sortDir;

            try
            {
                FieldDictList = await LoadFieldDictAsync(DictTableName);
                await ApplyLangDisplaySizeAsync(DictTableName, FieldDictList);
                TableFields = FieldDictList
                    .Where(f => f.Visible == 1)
                    .OrderBy(f => f.SerialNum ?? 0)
                    .ToList();

                TableName = await ResolveRealTableNameAsync(DictTableName) ?? DictTableName;

                if (string.IsNullOrWhiteSpace(CustomerId))
                {
                    LoadError = "未指定客戶代碼";
                    Items = new();
                    TotalCount = 0;
                }
                else
                {
                    var filterParams = new List<SqlParameter>();
                    var filter = BuildFilter(filterParams);
                    var orderBy = BuildOrderByClause(sortBy, sortDir) ?? await GetDefaultOrderByAsync(TableName);

                    TotalCount = await CountRowsAsync(TableName, filter, filterParams);
                    Items = await LoadRowsAsync(TableName, filter, orderBy, PageNumber, PageSize, filterParams);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Load SPOdPriceTable2 page failed");
                LoadError = ex.Message;
                Items = new();
                TotalCount = 0;
            }

            ViewData["DictTableName"] = DictTableName;
            ViewData["FieldDictList"] = FieldDictList;
            ViewData["Fields"] = TableFields;
            ViewData["QueryStringRaw"] = Request.QueryString.Value ?? string.Empty;
            try
            {
                ViewData["OCXLookups"] = _dictService.GetOCXLookups(DictTableName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetOCXLookups failed for {Table}", DictTableName);
            }
        }

        private string BuildFilter(List<SqlParameter> parameters)
        {
            var parts = new List<string> { "t0.IsIn = 0" };
            if (!string.IsNullOrWhiteSpace(CustomerId))
            {
                var pName = $"@p{parameters.Count}";
                parts.Add($"t0.CompanyId = {pName}");
                parameters.Add(new SqlParameter(pName, CustomerId));
            }

            if (parts.Count == 0) return string.Empty;
            return "WHERE " + string.Join(" AND ", parts);
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
                return await LoadFieldDictFallbackAsync(dictTableName);
            }
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
                foreach (var col in columns)
                {
                    var val = rd[col];
                    row[col] = val == DBNull.Value ? null : val;
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
                sql.Append(' ').Append(where);

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

        private string? BuildOrderByClause(string? sortBy, string? sortDir)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                return null;

            var validFieldName = System.Text.RegularExpressions.Regex.IsMatch(sortBy, @"^[a-zA-Z_][a-zA-Z0-9_]*$");
            if (!validFieldName)
                return null;

            var actualFieldName = GetActualFieldNameForSort(sortBy);
            if (string.IsNullOrWhiteSpace(actualFieldName))
                return null;

            var direction = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC";
            return $"[{actualFieldName}] {direction}";
        }

        private string? GetActualFieldNameForSort(string fieldName)
        {
            if (fieldName.EndsWith("Name", StringComparison.OrdinalIgnoreCase))
            {
                var baseFieldName = fieldName.Substring(0, fieldName.Length - 4);
                var baseField = FieldDictList?.FirstOrDefault(f =>
                    string.Equals(f.FieldName, baseFieldName, StringComparison.OrdinalIgnoreCase));
                if (baseField != null)
                    return baseFieldName;
            }

            return fieldName;
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

        private async Task<string?> ResolveRealTableNameAsync(string dictTableName)
        {
            var cs = GetConnStr();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            const string sql = @"
SELECT TOP 1 ISNULL(NULLIF(RealTableName,''), TableName) AS ActualName
  FROM CURdTableName WITH (NOLOCK)
 WHERE TableName = @tbl";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@tbl", dictTableName ?? string.Empty);
            var result = await cmd.ExecuteScalarAsync();
            if (result == null || result == DBNull.Value)
                return null;

            return result.ToString();
        }

        private List<SqlParameter> CloneParams(List<SqlParameter> source)
        {
            var list = new List<SqlParameter>();
            foreach (var p in source)
            {
                var clone = new SqlParameter(p.ParameterName, p.SqlDbType)
                {
                    Value = p.Value ?? DBNull.Value
                };
                list.Add(clone);
            }
            return list;
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
                    TableName = dictTableName ?? string.Empty,
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
    }
}
