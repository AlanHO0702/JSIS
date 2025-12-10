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

        public string ItemId => "EMO00004";
        public string PageTitle => "工程資料維護";
        public string TableName { get; private set; } = DataTable;
        public string DictTableName { get; private set; } = DictTable;
        public int PageNumber { get; private set; } = 1;
        public int PageSize { get; private set; } = 50;
        public int TotalCount { get; private set; }
        public List<Dictionary<string, object?>> Items { get; private set; } = new();
        public List<CURdTableField> FieldDictList { get; private set; } = new();
        public List<CURdTableField> TableFields { get; private set; } = new();
        public List<string> KeyFields { get; private set; } = new();

        public async Task OnGetAsync([FromQuery(Name = "pageIndex")] int pageIndex = 1, int pageSize = 50)
        {
            PageNumber = pageIndex <= 0 ? 1 : pageIndex;
            PageSize = pageSize <= 0 ? 50 : pageSize;
            ViewData["Title"] = PageTitle;

            FieldDictList = await LoadFieldDictAsync(DictTableName);
            await ApplyLangDisplaySizeAsync(DictTableName, FieldDictList);
            TableFields = FieldDictList
                .Where(f => f.Visible == 1)
                .OrderBy(f => f.SerialNum ?? 0)
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

            var orderBy = await GetDefaultOrderByAsync(TableName);

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
            ViewData["OrderBy"] = orderBy;
            ViewData["QueryStringRaw"] = Request.QueryString.Value ?? string.Empty;
            try
            {
                ViewData["OCXLookups"] = _dictService.GetOCXLookups(DictTableName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetOCXLookups failed for {Table}", DictTableName);
            }

            ViewData["CustomButtons"] = BuildCustomButtonsHtml();
        }

        private HtmlString BuildCustomButtonsHtml()
        {
            const string btn = "<button type='button' class='btn btn-outline-secondary btn-sm' data-custom-btn='1' data-button-name='openDetail'>詳細</button>";
            return new HtmlString(btn);
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
                    TableName = dictTableName,
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
    }
}
