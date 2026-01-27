using System.Data;
using System.Text;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;

namespace PcbErpApi.Pages.MPH
{
    public class MPH00052Model : PageModel
    {
        private readonly PcbErpContext _ctx;
        private readonly ITableDictionaryService _dictService;
        private readonly ILogger<MPH00052Model> _logger;

        private const string DefaultItemId = "MPH00052";

        public MPH00052Model(PcbErpContext ctx, ITableDictionaryService dictService, ILogger<MPH00052Model> logger)
        {
            _ctx = ctx;
            _dictService = dictService;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public string? ItemId { get; set; } = DefaultItemId;

        [BindProperty(SupportsGet = true)]
        public string? Customer { get; set; }

        [BindProperty(SupportsGet = true)]
        public string CustomerOp { get; set; } = "=";

        [BindProperty(SupportsGet = true)]
        public string? Part { get; set; }

        [BindProperty(SupportsGet = true)]
        public string PartOp { get; set; } = "like";

        [BindProperty(SupportsGet = true)]
        public string Effect { get; set; } = "valid";

        [BindProperty(SupportsGet = true, Name = "pageIndex")]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true, Name = "pageSize")]
        public int PageSize { get; set; } = 50;

        public string? ItemName { get; private set; }
        public string PageTitle => string.IsNullOrWhiteSpace(ItemName) ? ItemId ?? string.Empty : $"{ItemId} {ItemName}";
        public string DictTableName { get; private set; } = string.Empty;
        public string TableName { get; private set; } = string.Empty;
        public string OrderBy { get; private set; } = string.Empty;
        public string? LoadError { get; private set; }
        public int ModeNum { get; private set; }

        public List<Dictionary<string, object?>> Items { get; private set; } = new();
        public List<CURdTableField> FieldDictList { get; private set; } = new();
        public List<CURdTableField> TableFields { get; private set; } = new();
        public List<QueryFieldDef> QueryFields { get; private set; } = new();
        public List<LookupItem> Customers { get; private set; } = new();
        public List<LookupItem> Parts { get; private set; } = new();
        public int TotalCount { get; private set; }

        public async Task<IActionResult> OnGetDataAsync(string? sortBy = null, string? sortDir = null, int pageIndex = 1, int pageSize = 50)
        {
            try
            {
                ItemId = string.IsNullOrWhiteSpace(ItemId) ? DefaultItemId : ItemId;
                pageIndex = pageIndex <= 0 ? 1 : pageIndex;
                pageSize = pageSize <= 0 ? 50 : pageSize;

                var item = await _ctx.CurdSysItems.AsNoTracking()
                    .SingleOrDefaultAsync(x => x.ItemId == ItemId);
                if (item is null)
                {
                    return new JsonResult(new { success = false, error = $"Item {ItemId} not found." });
                }

                var setup = await _ctx.CurdOcxtableSetUp.AsNoTracking()
                    .Where(x => x.ItemId == ItemId)
                    .OrderBy(x => x.TableKind)
                    .FirstOrDefaultAsync();
                if (setup == null)
                {
                    return new JsonResult(new { success = false, error = $"CURdOCXTableSetUp not found for item {ItemId}." });
                }

                DictTableName = setup.TableName;
                TableName = await ResolveRealTableNameAsync(DictTableName) ?? DictTableName;

                try
                {
                    FieldDictList = _dictService.GetFieldDict(DictTableName, typeof(object));
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "GetFieldDict failed, fallback to direct load for {Table}", DictTableName);
                    FieldDictList = await LoadFieldDictFallbackAsync(DictTableName);
                }

                QueryFields = await LoadQueryFieldsAsync(ItemId, DictTableName, "TW");
                ModeNum = await LoadModeAsync(ItemId);

                var filterParams = new List<SqlParameter>();
                var baseFilter = BuildFilterSql(QueryFields, Request.Query, filterParams);
                var customFilter = BuildExtraFilter(filterParams);
                var combinedFilter = CombineFilters(setup.FilterSql, baseFilter, customFilter);

                var orderBy = BuildOrderByClause(sortBy, sortDir)
                    ?? (string.IsNullOrWhiteSpace(setup.OrderByField)
                        ? await GetDefaultOrderByAsync(TableName)
                        : setup.OrderByField);

                var totalCount = await CountRowsAsync(TableName, combinedFilter, filterParams);
                var items = await LoadRowsAsync(TableName, combinedFilter, orderBy, pageIndex, pageSize, filterParams);

                var hasQueryParams = HasQueryParams(Request.Query);
                return new JsonResult(new
                {
                    success = true,
                    items,
                    totalCount,
                    pageNumber = pageIndex,
                    pageSize,
                    sortBy,
                    sortDir,
                    openNoRecord = false,
                    hasQueryParams
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Load MPH00052 data failed (AJAX)");
                return new JsonResult(new { success = false, error = ex.Message });
            }
        }

        public async Task<IActionResult> OnGetExportAsync()
        {
            ItemId = string.IsNullOrWhiteSpace(ItemId) ? DefaultItemId : ItemId;

            var item = await _ctx.CurdSysItems.AsNoTracking()
                .SingleOrDefaultAsync(x => x.ItemId == ItemId);
            if (item is null)
                return NotFound($"Item {ItemId} not found.");

            var setup = await _ctx.CurdOcxtableSetUp.AsNoTracking()
                .Where(x => x.ItemId == ItemId)
                .OrderBy(x => x.TableKind)
                .FirstOrDefaultAsync();
            if (setup == null)
                return NotFound($"CURdOCXTableSetUp not found for item {ItemId}.");

            DictTableName = setup.TableName;
            TableName = await ResolveRealTableNameAsync(DictTableName) ?? DictTableName;

            try
            {
                FieldDictList = _dictService.GetFieldDict(DictTableName, typeof(object));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "GetFieldDict failed, fallback to direct load for {Table}", DictTableName);
                FieldDictList = await LoadFieldDictFallbackAsync(DictTableName);
            }

            await ApplyLangDisplaySizeAsync(DictTableName, FieldDictList);
            TableFields = FieldDictList
                .Where(f => f.Visible == 1)
                .OrderBy(f => f.SerialNum ?? 0)
                .ToList();

            ModeNum = await LoadModeAsync(ItemId);
            QueryFields = await LoadQueryFieldsAsync(ItemId, DictTableName, "TW");

            var filterParams = new List<SqlParameter>();
            var baseFilter = BuildFilterSql(QueryFields, Request.Query, filterParams);
            var customFilter = BuildExtraFilter(filterParams);
            var combinedFilter = CombineFilters(setup.FilterSql, baseFilter, customFilter);

            var orderBy = string.IsNullOrWhiteSpace(setup.OrderByField)
                ? await GetDefaultOrderByAsync(TableName)
                : setup.OrderByField;

            var rows = await LoadAllRowsAsync(TableName, combinedFilter, orderBy, filterParams);
            var lookupMeta = await LoadLookupMetaAsync(DictTableName);

            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("PriceTable");

            for (var c = 0; c < TableFields.Count; c++)
            {
                ws.Cell(1, c + 1).Value = TableFields[c].DisplayLabel ?? TableFields[c].FieldName;
                ws.Cell(1, c + 1).Style.Font.Bold = true;
            }

            for (var r = 0; r < rows.Count; r++)
            {
                var row = rows[r];
                for (var c = 0; c < TableFields.Count; c++)
                {
                    var field = TableFields[c].FieldName ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(field)) continue;
                    row.TryGetValue(field, out var val);
                    var displayVal = ResolveLookupDisplay(field, row, val, lookupMeta);
                    ws.Cell(r + 2, c + 1).Value = displayVal == null
                        ? default(XLCellValue)
                        : XLCellValue.FromObject(displayVal);
                }
            }

            ws.Columns().AdjustToContents();

            var fileName = $"{TableName}_{DateTime.Now:yyyyMMdd}.xlsx";
            using var stream = new MemoryStream();
            wb.SaveAs(stream);
            stream.Position = 0;
            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        public async Task<IActionResult> OnGetAsync()
        {
            ItemId = string.IsNullOrWhiteSpace(ItemId) ? DefaultItemId : ItemId;
            PageNumber = PageNumber <= 0 ? 1 : PageNumber;
            PageSize = PageSize <= 0 ? 50 : PageSize;

            var item = await _ctx.CurdSysItems.AsNoTracking()
                .SingleOrDefaultAsync(x => x.ItemId == ItemId);

            if (item is null)
            {
                LoadError = $"Item {ItemId} not found.";
                return Page();
            }

            ItemName = item.ItemName;
            ViewData["Title"] = PageTitle;

            var setup = await _ctx.CurdOcxtableSetUp.AsNoTracking()
                .Where(x => x.ItemId == ItemId)
                .OrderBy(x => x.TableKind)
                .FirstOrDefaultAsync();

            if (setup == null)
            {
                LoadError = $"CURdOCXTableSetUp not found for item {ItemId}.";
                return Page();
            }

            DictTableName = setup.TableName;
            TableName = await ResolveRealTableNameAsync(DictTableName) ?? DictTableName;
            OrderBy = string.IsNullOrWhiteSpace(setup.OrderByField)
                ? await GetDefaultOrderByAsync(TableName)
                : setup.OrderByField;

            try
            {
                FieldDictList = _dictService.GetFieldDict(DictTableName, typeof(object));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "GetFieldDict failed, fallback to direct load for {Table}", DictTableName);
                FieldDictList = await LoadFieldDictFallbackAsync(DictTableName);
            }

            await ApplyLangDisplaySizeAsync(DictTableName, FieldDictList);
            var keyFields = await LoadPrimaryKeyColumnsAsync(TableName);
            if (keyFields.Count > 0)
                ViewData["KeyFields"] = keyFields;

            var keyFieldSet = new HashSet<string>(keyFields ?? new List<string>(), StringComparer.OrdinalIgnoreCase);
            TableFields = FieldDictList
                .Where(f => f.Visible == 1 || (!string.IsNullOrWhiteSpace(f.FieldName) && keyFieldSet.Contains(f.FieldName)))
                .OrderBy(f => f.SerialNum ?? 0)
                .ToList();

            QueryFields = await LoadQueryFieldsAsync(ItemId, DictTableName, "TW");
            ViewData["DictTableName"] = DictTableName;
            ViewData["FieldDictList"] = FieldDictList;
            ViewData["Fields"] = TableFields;
            ViewData["OrderBy"] = OrderBy;
            ViewData["QueryStringRaw"] = Request.QueryString.Value ?? "";

            try
            {
                ViewData["OCXLookups"] = _dictService.GetOCXLookups(DictTableName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetOCXLookups failed for {Table}", DictTableName);
            }

            ModeNum = await LoadModeAsync(ItemId);
            await LoadLookupsAsync(ModeNum);

            var filterParams = new List<SqlParameter>();
            var baseFilter = BuildFilterSql(QueryFields, Request.Query, filterParams);
            var customFilter = BuildExtraFilter(filterParams);
            var combinedFilter = CombineFilters(setup.FilterSql, baseFilter, customFilter);

            try
            {
                TotalCount = await CountRowsAsync(TableName, combinedFilter, filterParams);
                Items = await LoadRowsAsync(TableName, combinedFilter, OrderBy, PageNumber, PageSize, filterParams);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Load rows failed for {ItemId}/{Table}", ItemId, TableName);
                LoadError = ex.Message;
                Items = new();
                TotalCount = 0;
            }

            try
            {
                var customButtons = await LoadCustomButtonsAsync(ItemId);
                if (customButtons.Count > 0)
                {
                    ViewData["CustomButtons"] = BuildCustomButtonsHtml(customButtons);
                    ViewData["CustomButtonMeta"] = customButtons;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Load custom buttons failed for {ItemId}", ItemId);
            }

            return Page();
        }

        private async Task<List<string>> LoadPrimaryKeyColumnsAsync(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName)) return new List<string>();

            async Task<List<string>> QueryAsync(string objectName)
            {
                var list = new List<string>();
                var cs = GetConnStr();
                await using var conn = new SqlConnection(cs);
                await conn.OpenAsync();
                const string sql = @"
SELECT col.name
  FROM sys.indexes i
  JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
  JOIN sys.columns col ON col.object_id = ic.object_id AND col.column_id = ic.column_id
 WHERE i.object_id = OBJECT_ID(@tbl) AND i.is_primary_key = 1
 ORDER BY ic.key_ordinal;";
                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@tbl", objectName);
                await using var rd = await cmd.ExecuteReaderAsync();
                while (await rd.ReadAsync())
                    list.Add(rd.GetString(0));
                return list;
            }

            var pk = await QueryAsync(tableName);
            if (pk.Count > 0) return pk;
            if (!tableName.Contains('.', StringComparison.OrdinalIgnoreCase))
            {
                pk = await QueryAsync("dbo." + tableName);
                if (pk.Count > 0) return pk;
            }
            return new List<string>();
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

        private async Task<List<Dictionary<string, object?>>> LoadAllRowsAsync(string tableName, string? filter, string? orderBy, List<SqlParameter>? filterParams)
        {
            var list = new List<Dictionary<string, object?>>();
            var sql = new StringBuilder($"SELECT * FROM [{tableName}] t0");
            var where = (filter ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(where))
            {
                sql.Append(' ').Append(where);
            }
            var order = (orderBy ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(order))
                order = "1";
            sql.Append(" ORDER BY ").Append(order);

            var cs = GetConnStr();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            await using var cmd = new SqlCommand(sql.ToString(), conn);
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

        private async Task<Dictionary<string, LookupMeta>> LoadLookupMetaAsync(string dictTableName)
        {
            var result = new Dictionary<string, LookupMeta>(StringComparer.OrdinalIgnoreCase);
            try
            {
                var lookups = _dictService.GetOCXLookups(dictTableName);
                foreach (var lk in lookups)
                {
                    var fieldName = (lk.FieldName as string ?? lk.FieldName?.ToString() ?? string.Empty).Trim();
                    if (string.IsNullOrWhiteSpace(fieldName)) continue;
                    var keyField = lk.KeyFieldName as string ?? lk.KeyFieldName?.ToString();
                    var keySelf = lk.KeySelfName as string ?? lk.KeySelfName?.ToString();
                    var map = BuildLookupMap(lk.LookupValues);
                    result[fieldName] = new LookupMeta(fieldName, keyField, keySelf, map);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "GetOCXLookups failed for {Table}", dictTableName);
            }
            return result;
        }

        private static Dictionary<string, string> BuildLookupMap(object? raw)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (raw is System.Collections.IDictionary dict)
            {
                foreach (var key in dict.Keys)
                {
                    var k = key?.ToString() ?? "";
                    if (string.IsNullOrWhiteSpace(k)) continue;
                    map[k] = dict[key]?.ToString() ?? "";
                }
            }
            return map;
        }

        private static object? ResolveLookupDisplay(string fieldName, Dictionary<string, object?> row, object? rawVal, Dictionary<string, LookupMeta> lookupMeta)
        {
            if (!lookupMeta.TryGetValue(fieldName, out var meta) || meta.Map.Count == 0)
                return rawVal;

            var lookupKey = rawVal?.ToString() ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(meta.KeyFieldName) && row.TryGetValue(meta.KeyFieldName, out var keyFieldVal) && keyFieldVal != null)
                lookupKey = keyFieldVal.ToString() ?? lookupKey;
            else if (!string.IsNullOrWhiteSpace(meta.KeySelfName) && row.TryGetValue(meta.KeySelfName, out var keySelfVal) && keySelfVal != null)
                lookupKey = keySelfVal.ToString() ?? lookupKey;

            if (!string.IsNullOrWhiteSpace(lookupKey) && meta.Map.TryGetValue(lookupKey, out var display))
                return display;

            return rawVal;
        }

        private sealed record LookupMeta(string FieldName, string? KeyFieldName, string? KeySelfName, Dictionary<string, string> Map);

        private async Task<int> CountRowsAsync(string tableName, string? filter, List<SqlParameter>? filterParams)
        {
            var cs = GetConnStr();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            var sql = new StringBuilder($"SELECT COUNT(1) FROM [{tableName}] t0");
            var where = (filter ?? string.Empty).Trim();
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

        private string BuildExtraFilter(List<SqlParameter> parameters)
        {
            var parts = new List<string>();

            if (Effect.Equals("valid", StringComparison.OrdinalIgnoreCase))
            {
                parts.Add("((t0.EffectDate<=GETDATE()) OR (ISNULL(t0.EffectDate,'')=''))");
                parts.Add("((t0.DelDate>=GETDATE()) OR (ISNULL(t0.DelDate,'')=''))");
            }
            else if (Effect.Equals("invalid", StringComparison.OrdinalIgnoreCase))
            {
                parts.Add("((t0.EffectDate>=GETDATE()) OR (t0.DelDate<=GETDATE()))");
            }

            if (ModeNum == 1)
            {
                if (!string.IsNullOrWhiteSpace(Part))
                {
                    var op = ResolveOperator(PartOp);
                    var pName = $"@p{parameters.Count}";
                    parts.Add($"t0.MatName {op} {pName}");
                    parameters.Add(new SqlParameter(pName, BuildValue(op, Part)));
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(Customer))
                {
                    var op = ResolveOperator(CustomerOp);
                    var pName = $"@p{parameters.Count}";
                    parts.Add($"t0.CompanyId {op} {pName}");
                    parameters.Add(new SqlParameter(pName, BuildValue(op, Customer)));
                }
                if (!string.IsNullOrWhiteSpace(Part))
                {
                    var op = ResolveOperator(PartOp);
                    var pName = $"@p{parameters.Count}";
                    parts.Add($"t0.PartNum {op} {pName}");
                    parameters.Add(new SqlParameter(pName, BuildValue(op, Part)));
                }
            }

            return string.Join(" AND ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
        }

        private static object BuildValue(string op, string val)
        {
            if (op.Equals("LIKE", StringComparison.OrdinalIgnoreCase))
                return $"%{val}%";
            return val;
        }

        private async Task LoadLookupsAsync(int mode)
        {
            Customers.Clear();
            Parts.Clear();

            var cs = GetConnStr();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            if (mode != 1)
            {
                const string sqlCus = @"SELECT CompanyId, ShortName FROM dbo.AJNdMTLSupplier WITH (NOLOCK) ORDER BY CompanyId";
                await using (var cmd = new SqlCommand(sqlCus, conn))
                await using (var rd = await cmd.ExecuteReaderAsync())
                {
                    while (await rd.ReadAsync())
                    {
                        Customers.Add(new LookupItem(rd["CompanyId"]?.ToString() ?? string.Empty, rd["ShortName"]?.ToString() ?? string.Empty));
                    }
                }

                const string sqlPn = @"SELECT PartNum, MatName FROM dbo.MINdMatInfo WITH (NOLOCK) ORDER BY PartNum";
                await using (var cmd = new SqlCommand(sqlPn, conn))
                await using (var rd = await cmd.ExecuteReaderAsync())
                {
                    while (await rd.ReadAsync())
                    {
                        Parts.Add(new LookupItem(rd["PartNum"]?.ToString() ?? string.Empty, rd["MatName"]?.ToString() ?? string.Empty));
                    }
                }
            }
            else
            {
                const string sqlMat = @"SELECT MatName, PartNum FROM dbo.MINdMatInfo WITH (NOLOCK) WHERE MB=1 ORDER BY PartNum";
                await using var cmd = new SqlCommand(sqlMat, conn);
                await using var rd = await cmd.ExecuteReaderAsync();
                while (await rd.ReadAsync())
                {
                    Parts.Add(new LookupItem(rd["MatName"]?.ToString() ?? string.Empty, rd["PartNum"]?.ToString() ?? string.Empty));
                }
            }
        }

        private async Task<int> LoadModeAsync(string? itemId)
        {
            var cs = GetConnStr();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            const string sql = @"
SELECT DLLValue FROM CURdOCXItemOtherRule WITH (NOLOCK)
 WHERE ItemId=@item AND RuleId='ModeNum'";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@item", itemId ?? string.Empty);

            var obj = await cmd.ExecuteScalarAsync();
            return obj != null && int.TryParse(obj.ToString(), out var n) ? n : 0;
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
            cmd.Parameters.AddWithValue("@tbl", tableName ?? string.Empty);
            var result = await cmd.ExecuteScalarAsync();
            var col = result?.ToString();
            return string.IsNullOrWhiteSpace(col) ? "1" : $"[{col}]";
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

        private string BuildFilterSql(List<QueryFieldDef> defs, IQueryCollection query, List<SqlParameter> parameters)
        {
            if (defs == null || defs.Count == 0) return string.Empty;
            var parts = new List<string>();
            int idx = parameters.Count;

            foreach (var def in defs)
            {
                if (string.IsNullOrWhiteSpace(def.ColumnName)) continue;
                var key = def.ColumnName;
                var val = query[key].ToString();
                if (string.IsNullOrWhiteSpace(val)) continue;

                var paramName = $"@p{idx++}";
                var op = ResolveOperator(def.DefaultEqual);
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

        private static string CombineFilters(params string?[] filters)
        {
            var parts = new List<string>();
            foreach (var f in filters)
            {
                var s = NormalizeFilter(f);
                if (!string.IsNullOrWhiteSpace(s))
                    parts.Add(s);
            }

            if (parts.Count == 0) return string.Empty;
            return "WHERE " + string.Join(" AND ", parts);
        }

        private static string NormalizeFilter(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
            var s = raw.Trim();
            if (s.StartsWith("where", StringComparison.OrdinalIgnoreCase))
                s = s[5..].Trim();
            if (s.StartsWith("and", StringComparison.OrdinalIgnoreCase))
                s = s[3..].Trim();
            if (s.StartsWith("or", StringComparison.OrdinalIgnoreCase))
                s = s[2..].Trim();
            return s.Trim();
        }

        private string? BuildOrderByClause(string? sortBy, string? sortDir)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                return null;

            if (!System.Text.RegularExpressions.Regex.IsMatch(sortBy, @"^[a-zA-Z_][a-zA-Z0-9_]*$"))
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

        private static bool HasQueryParams(IQueryCollection query)
        {
            if (query == null || query.Count == 0) return false;
            foreach (var key in query.Keys)
            {
                if (string.IsNullOrWhiteSpace(key)) continue;
                if (key.Equals("handler", StringComparison.OrdinalIgnoreCase)) continue;
                if (key.Equals("pageIndex", StringComparison.OrdinalIgnoreCase)) continue;
                if (key.Equals("pageSize", StringComparison.OrdinalIgnoreCase)) continue;
                if (key.Equals("sortBy", StringComparison.OrdinalIgnoreCase)) continue;
                if (key.Equals("sortDir", StringComparison.OrdinalIgnoreCase)) continue;
                if (!string.IsNullOrWhiteSpace(query[key])) return true;
            }
            return false;
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

        public class CustomButtonRow
        {
            public string ItemId { get; set; } = string.Empty;
            public int? SerialNum { get; set; }
            public string ButtonName { get; set; } = string.Empty;
            public string CustCaption { get; set; } = string.Empty;
            public string CustCaptionE { get; set; } = string.Empty;
            public string CustHint { get; set; } = string.Empty;
            public string CustHintE { get; set; } = string.Empty;
            public string OCXName { get; set; } = string.Empty;
            public string CoClassName { get; set; } = string.Empty;
            public string SpName { get; set; } = string.Empty;
            public int? bVisible { get; set; }
            public int? ChkCanUpdate { get; set; }
            public int? bNeedNum { get; set; }
            public int? DesignType { get; set; }
        }

        private async Task<(bool hasCaptionE, bool hasHintE, string chkCol)> DetectButtonSchemaAsync(SqlConnection conn)
        {
            var cols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            const string sql = "SELECT name FROM sys.columns WHERE object_id = OBJECT_ID('dbo.CURdOCXItemCustButton')";

            await using (var cmd = new SqlCommand(sql, conn))
            await using (var rd = await cmd.ExecuteReaderAsync())
            {
                while (await rd.ReadAsync())
                    cols.Add(rd.GetString(0));
            }

            var hasCapE = cols.Contains("CustCaptionE");
            var hasHintE = cols.Contains("CustHintE");
            var chkCol = cols.Contains("ChkCanUpdate") ? "ChkCanUpdate"
                       : cols.Contains("ChkCanbUpdate") ? "ChkCanbUpdate"
                       : "ChkCanUpdate";
            return (hasCapE, hasHintE, chkCol);
        }

        private async Task<List<CustomButtonRow>> LoadCustomButtonsAsync(string? itemId)
        {
            var list = new List<CustomButtonRow>();
            var cs = GetConnStr();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            var schema = await DetectButtonSchemaAsync(conn);

            var sql = $@"
SELECT ItemId, SerialNum, ButtonName,
       CustCaption,
       {(schema.hasCaptionE ? "CustCaptionE" : "CAST('' AS nvarchar(1)) AS CustCaptionE")},
       CustHint,
       {(schema.hasHintE ? "CustHintE" : "CAST('' AS nvarchar(1)) AS CustHintE")},
       OCXName, CoClassName, SpName,
       bVisible, {schema.chkCol} AS ChkCanUpdate, bNeedNum, DesignType
  FROM CURdOCXItemCustButton WITH (NOLOCK)
 WHERE ItemId = @itemId
 ORDER BY SerialNum, ButtonName;";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@itemId", itemId ?? string.Empty);

            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                list.Add(new CustomButtonRow
                {
                    ItemId = rd["ItemId"]?.ToString() ?? string.Empty,
                    SerialNum = TryToInt(rd["SerialNum"]),
                    ButtonName = rd["ButtonName"]?.ToString() ?? string.Empty,
                    CustCaption = rd["CustCaption"]?.ToString() ?? string.Empty,
                    CustCaptionE = rd["CustCaptionE"]?.ToString() ?? string.Empty,
                    CustHint = rd["CustHint"]?.ToString() ?? string.Empty,
                    CustHintE = rd["CustHintE"]?.ToString() ?? string.Empty,
                    OCXName = rd["OCXName"]?.ToString() ?? string.Empty,
                    CoClassName = rd["CoClassName"]?.ToString() ?? string.Empty,
                    SpName = rd["SpName"]?.ToString() ?? string.Empty,
                    bVisible = TryToInt(rd["bVisible"]),
                    ChkCanUpdate = TryToInt(rd["ChkCanUpdate"]),
                    bNeedNum = TryToInt(rd["bNeedNum"]),
                    DesignType = TryToInt(rd["DesignType"])
                });
            }

            return list;
        }

        private static HtmlString BuildCustomButtonsHtml(IEnumerable<CustomButtonRow> rows)
        {
            if (rows == null) return HtmlString.Empty;
            var sb = new StringBuilder();

            foreach (var b in rows)
            {
                if (b.bVisible.HasValue && b.bVisible.Value == 0) continue;
                if (string.IsNullOrWhiteSpace(b.ButtonName)) continue;

                var caption = string.IsNullOrWhiteSpace(b.CustCaption) ? b.ButtonName : b.CustCaption;
                var hint = b.CustHint ?? string.Empty;

                sb.Append("<button type='button' class='btn btn-outline-secondary btn-sm' data-custom-btn='1'");
                sb.Append(" data-button-name='").Append(System.Net.WebUtility.HtmlEncode(b.ButtonName)).Append('\'');
                sb.Append(" data-item-id='").Append(System.Net.WebUtility.HtmlEncode(b.ItemId ?? string.Empty)).Append('\'');
                sb.Append(" title='").Append(System.Net.WebUtility.HtmlEncode(hint)).Append("'>");
                sb.Append(System.Net.WebUtility.HtmlEncode(caption));
                sb.Append("</button>");
            }

            return new HtmlString(sb.ToString());
        }

        private async Task<List<QueryFieldDef>> LoadQueryFieldsAsync(string? itemId, string? tableName, string? lang)
        {
            var list = new List<QueryFieldDef>();
            var cs = GetConnStr();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            using var cmd = new SqlCommand("exec CURdOCXPaperSelOtherGet @p0,@p1,@p2", conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@p0", itemId ?? string.Empty);
            cmd.Parameters.AddWithValue("@p1", tableName ?? string.Empty);
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

        public record LookupItem(string Value, string Label);

        public class QueryFieldDef
        {
            public string ColumnName { get; set; } = string.Empty;
            public string ColumnCaption { get; set; } = string.Empty;
            public int? DataType { get; set; }
            public int? ControlType { get; set; }
            public string? DefaultValue { get; set; }
            public string? DefaultEqual { get; set; }
            public string? CommandText { get; set; }
        }
    }
}

