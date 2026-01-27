using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PcbErpApi.Data;
using PcbErpApi.Models;
using PcbErpApi.Helpers;

namespace PcbErpApi.Pages.EMOdProdInfo
{
    public class IndexModel : PageModel
    {
        private readonly PcbErpContext _ctx;
        private readonly ITableDictionaryService _dictService;
        private readonly ILogger<IndexModel> _logger;

        private const string DictTable = "EMOdProdInfo";
        private const string DataTable = "EMOdProdInfo";

        public IndexModel(PcbErpContext ctx, ITableDictionaryService dictService, ILogger<IndexModel> logger)
        {
            _ctx = ctx;
            _dictService = dictService;
            _logger = logger;
        }

        [FromQuery(Name = "mode")]
        public string? Mode { get; set; }

        [FromQuery(Name = "itemId")]
        public string? RequestedItemId { get; set; }

        // 判斷是否為查詢模式
        public bool IsViewOnly
        {
            get
            {
                // 優先順序1: 明確的 mode 參數
                if (!string.IsNullOrEmpty(Mode))
                    return string.Equals(Mode, "view", StringComparison.OrdinalIgnoreCase);

                // 優先順序2: 根據 ItemId 判斷（查詢作業代碼）
                if (RequestedItemId == "EMO00018")
                    return true;

                // 預設為維護模式
                return false;
            }
        }

        public string ItemId => IsViewOnly ? "EMO00018" : "EMO00004";
        public string PageTitle => IsViewOnly ? "EMO00018 工程資料查詢" : "EMO00004 工程資料維護";
        public string TableName { get; private set; } = DataTable;
        public string DictTableName { get; private set; } = DictTable;
        public int PageNumber { get; private set; } = 1;
        public int PageSize { get; private set; } = 50;
        public int TotalCount { get; private set; }
        public int TotalPages => (int)Math.Ceiling((TotalCount) / (double)PageSize);
        public List<Dictionary<string, object?>> Items { get; private set; } = new();
        public List<CURdTableField> FieldDictList { get; private set; } = new();
        public List<TableFieldViewModel> TableFields { get; private set; } = new();
        public List<QueryFieldViewModel> QueryFields { get; private set; } = new();
        public List<string> KeyFields { get; private set; } = new();

        // AJAX API：只回傳資料 JSON
        public async Task<IActionResult> OnGetDataAsync(string? sortBy = null, string? sortDir = null, int pageIndex = 1, int pageSize = 50)
        {
            try
            {
                // 載入欄位字典以便處理 Lookup 欄位
                FieldDictList = await LoadFieldDictAsync(DictTableName);

                var orderBy = BuildOrderByClause(sortBy, sortDir) ?? await GetDefaultOrderByAsync(TableName);
                var totalCount = await CountRowsAsync(TableName, string.Empty, null);
                var items = await LoadRowsAsync(TableName, string.Empty, orderBy, pageIndex, pageSize, null);

                return new JsonResult(new
                {
                    success = true,
                    items = items,
                    totalCount = totalCount,
                    pageNumber = pageIndex,
                    pageSize = pageSize,
                    sortBy = sortBy,
                    sortDir = sortDir
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Load EMOdProdInfo data failed (AJAX)");
                return new JsonResult(new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }

        public async Task OnGetAsync(int page = 1, int pageIndex = 1, int pageSize = 50, string? sortBy = null, string? sortDir = null)
        {
            // 支援 page 和 pageIndex 兩種參數名稱
            var effectivePage = page > 1 ? page : pageIndex;
            PageNumber = effectivePage <= 0 ? 1 : effectivePage;
            PageSize = pageSize <= 0 ? 50 : pageSize;
            ViewData["Title"] = PageTitle;
            ViewData["SortBy"] = sortBy;
            ViewData["SortDir"] = sortDir;
            ViewData["IsViewOnly"] = IsViewOnly;

            FieldDictList = await LoadFieldDictAsync(DictTableName);
            await ApplyLangDisplaySizeAsync(DictTableName, FieldDictList);

            // 建立查詢欄位列表
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

            // 主鍵設定：PartNum + Revision
            KeyFields = FieldDictList
                .Where(f => (f.PK ?? 0) == 1)
                .Select(f => f.FieldName)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .ToList();
            if (KeyFields.Count == 0)
            {
                // fallback：若辭典未標記 PK，手動設定 PartNum + Revision
                KeyFields.Add("PartNum");
                KeyFields.Add("Revision");
            }

            var orderBy = BuildOrderByClause(sortBy, sortDir) ?? await GetDefaultOrderByAsync(TableName);

            try
            {
                TotalCount = await CountRowsAsync(TableName, string.Empty, null);
                Items = await LoadRowsAsync(TableName, string.Empty, orderBy, PageNumber, PageSize, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Load EMOdProdInfo data failed");
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
            ViewData["KeyFieldName"] = "PartNum";
            ViewData["PagedQueryUrl"] = "/api/DynamicTable/PagedQuery";
            ViewData["AddApiUrl"] = $"/api/{TableName}";
            ViewData["PaginationVm"] = new PaginationModel
            {
                PageNumber = PageNumber,
                TotalPages = TotalPages,
                RouteUrl = Url.Page("/EMOdProdInfo/Index") ?? "/EMOdProdInfo"
            };
            try
            {
                ViewData["OCXLookups"] = _dictService.GetOCXLookups(DictTableName);
                var lookupMaps = _dictService.GetOCXLookups(DictTableName);
                ViewData["LookupDisplayMap"] = LookupDisplayHelper.BuildLookupDisplayMap(
                    Items,
                    lookupMaps,
                    item => GetDictValue(item, "PartNum")
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetOCXLookups failed for {Table}", DictTableName);
                ViewData["LookupDisplayMap"] = new Dictionary<string, Dictionary<string, string>>();
            }

            // ViewData["CustomButtons"] = BuildCustomButtonsHtml(); // 已移除「詳細」按鈕
        }

        // private HtmlString BuildCustomButtonsHtml()
        // {
        //     const string btn = "<button type='button' class='btn btn-outline-secondary btn-sm' data-custom-btn='1' data-button-name='openDetail'>詳細</button>";
        //     return new HtmlString(btn);
        // }

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

                // 為虛擬欄位 UserName 填入 Designer 的值（顯示代碼）
                if (row.ContainsKey("Designer"))
                {
                    row["UserName"] = row["Designer"];
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

        private string? BuildOrderByClause(string? sortBy, string? sortDir)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                return null;

            // 防止 SQL Injection：只允許合法的欄位名稱
            var validFieldName = System.Text.RegularExpressions.Regex.IsMatch(sortBy, @"^[a-zA-Z_][a-zA-Z0-9_]*$");
            if (!validFieldName)
                return null;

            // 檢查是否為 Lookup 欄位，如果是則對應到實際欄位
            var actualFieldName = GetActualFieldNameForSort(sortBy);
            if (string.IsNullOrWhiteSpace(actualFieldName))
                return null;

            var direction = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC";
            return $"[{actualFieldName}] {direction}";
        }

        private string? GetActualFieldNameForSort(string fieldName)
        {
            // 常見模式：StatusName -> Status, CustomerName -> Customer
            // 如果欄位名稱以 "Name" 結尾，先嘗試找到對應的基礎欄位
            if (fieldName.EndsWith("Name", StringComparison.OrdinalIgnoreCase))
            {
                var baseFieldName = fieldName.Substring(0, fieldName.Length - 4);

                // 檢查這個基礎欄位是否存在於辭典中
                var baseField = FieldDictList?.FirstOrDefault(f =>
                    string.Equals(f.FieldName, baseFieldName, StringComparison.OrdinalIgnoreCase));

                if (baseField != null)
                    return baseFieldName;
            }

            // 如果不是 lookup 欄位，或找不到對應的基礎欄位，就用原始名稱
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

        private async Task<List<CURdTableField>> LoadFieldDictAsync(string dictTableName)
        {
            try
            {
                return _dictService.GetFieldDict(dictTableName, typeof(EmodProdInfo));
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
                var clone = new SqlParameter(p.ParameterName, p.Value);
                clone.DbType = p.DbType;
                clone.Direction = p.Direction;
                yield return clone;
            }
        }

        private static string GetDictValue(Dictionary<string, object?> dict, string field)
        {
            if (dict == null) return string.Empty;
            var hit = dict.FirstOrDefault(kv => kv.Key.Equals(field, StringComparison.OrdinalIgnoreCase));
            return hit.Equals(default(KeyValuePair<string, object?>)) ? string.Empty : hit.Value?.ToString() ?? string.Empty;
        }

        private static int GetDataTypeCode(string? dataType)
        {
            if (string.IsNullOrWhiteSpace(dataType)) return 0;
            var dt = dataType.ToLowerInvariant();

            // 1 = Date/DateTime
            if (dt.Contains("date") || dt.Contains("time")) return 1;

            // 2 = Number
            if (dt.Contains("int") || dt.Contains("decimal") || dt.Contains("numeric") ||
                dt.Contains("float") || dt.Contains("money") || dt.Contains("number")) return 2;

            // 0 = String (default)
            return 0;
        }
    }
}
