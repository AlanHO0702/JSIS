using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using PcbErpApi.Data;
using PcbErpApi.Models;
using PcbErpApi.Services;

namespace PcbErpApi.Pages.SPO
{
    public class SPO00003Model : PageModel
    {
        private readonly PcbErpContext _context;
        private readonly PaginationService _pagedService;
        private readonly ITableDictionaryService _dictService;

        public SPO00003Model(PcbErpContext context, PaginationService pagedService, ITableDictionaryService dictService)
        {
            _context = context;
            _pagedService = pagedService;
            _dictService = dictService;
        }

        public string PageTitle => "SPO00003 銷售訂單查詢";
        public string? DictTableName { get; set; }
        public string? ParamTableName { get; set; }
        [BindProperty(SupportsGet = true, Name = "spId")]
        public int SpId { get; set; }
        public Dictionary<string, object?> DefaultQueryValues { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, string> ParamNameByField { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, string> FieldNameByParam { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        [BindProperty(SupportsGet = true)]
        public string? PaperNum { get; set; }
        [BindProperty(SupportsGet = true)]
        public string? CustomerId { get; set; }
        [BindProperty(SupportsGet = true)]
        public string? CustPONum { get; set; }
        [BindProperty(SupportsGet = true)]
        public string? PartNum { get; set; }
        [BindProperty(SupportsGet = true)]
        public DateTime? PaperDateFrom { get; set; }
        [BindProperty(SupportsGet = true)]
        public DateTime? PaperDateTo { get; set; }

        [BindProperty(SupportsGet = true, Name = "page")]
        public int PageNumber { get; set; } = 1;
        [BindProperty(SupportsGet = true, Name = "pageSize")]
        public int PageSize { get; set; } = 50;

        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public List<Dictionary<string, object?>> Items { get; set; } = new();

        public List<CURdTableField> QueryDictFields { get; set; } = new();
        public List<CURdTableField> GridDictFields { get; set; } = new();
        public string? DebugExecText { get; set; }
        public List<(string Name, object? Value)> DebugParams { get; set; } = new();
        public string CurrentUserId { get; set; } = "";
        public string CurrentUseId { get; set; } = "";

        public async Task OnGetAsync()
        {
            var conn = _context.Database.GetDbConnection();
            var opened = conn.State != ConnectionState.Open;
            if (opened) await _context.Database.OpenConnectionAsync();
            try
            {
                const string itemId = "SPO00003";

                var item = await _context.CurdSysItems.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.ItemId == itemId);

                var className = (item?.ClassName ?? "").Replace(".dll", "", StringComparison.OrdinalIgnoreCase);
                if (string.IsNullOrWhiteSpace(className))
                    className = "SPOdOrderInq";

                ParamTableName = ResolveParamTableName(className);

                QueryDictFields = LoadDictSafe(ParamTableName);
                DictTableName = className;
                GridDictFields = LoadDictSafe(DictTableName);
                (ParamNameByField, FieldNameByParam) = await LoadParamMapAsync(conn, ParamTableName);

                PageNumber = PageNumber <= 0 ? 1 : PageNumber;
                PageSize = PageSize <= 0 ? 50 : Math.Min(PageSize, 500);

                string paramStr;
                if (SpId <= 0)
                {
                    var start = await CallInqStartAsync(conn, itemId);
                    paramStr = start.ParamStr;
                    SpId = start.SpId;
                    if (!string.IsNullOrWhiteSpace(start.TableName))
                    {
                        DictTableName = start.TableName;
                        GridDictFields = LoadDictSafe(DictTableName);
                    }
                }
                else
                {
                    paramStr = BuildParamStrFromDict(className, ParamTableName);
                }

                DefaultQueryValues = await LoadDefaultParamRowAsync(conn, SpId, item?.ItemId ?? itemId);
                // Expand defaults with FieldName -> ParamName mapping for easier lookup
                foreach (var kv in ParamNameByField)
                {
                    var field = kv.Key;
                    var param = kv.Value;
                    if (!DefaultQueryValues.ContainsKey(field) && DefaultQueryValues.TryGetValue(param, out var v))
                        DefaultQueryValues[field] = v;
                }
                var rows = await ExecInqAsync(conn, paramStr, SpId, item, DefaultQueryValues);

                TotalCount = rows.Count;
                TotalPages = Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));

                Items = rows
                    .Skip((PageNumber - 1) * PageSize)
                    .Take(PageSize)
                    .ToList();

                CurrentUserId = ResolveUserId();
                CurrentUseId = ResolveUseId();
            }
            finally
            {
                if (opened) await _context.Database.CloseConnectionAsync();
            }
        }

        public async Task<IActionResult> OnGetExportAsync()
        {
            var conn = _context.Database.GetDbConnection();
            var opened = conn.State != ConnectionState.Open;
            if (opened) await _context.Database.OpenConnectionAsync();
            try
            {
                const string itemId = "SPO00003";

                var item = await _context.CurdSysItems.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.ItemId == itemId);

                var className = (item?.ClassName ?? "").Replace(".dll", "", StringComparison.OrdinalIgnoreCase);
                if (string.IsNullOrWhiteSpace(className))
                    className = "SPOdOrderInq";

                ParamTableName = ResolveParamTableName(className);
                QueryDictFields = LoadDictSafe(ParamTableName);
                DictTableName = className;
                GridDictFields = LoadDictSafe(DictTableName);
                (ParamNameByField, FieldNameByParam) = await LoadParamMapAsync(conn, ParamTableName);

                string paramStr;
                if (SpId <= 0)
                {
                    var start = await CallInqStartAsync(conn, itemId);
                    paramStr = start.ParamStr;
                    SpId = start.SpId;
                    if (!string.IsNullOrWhiteSpace(start.TableName))
                    {
                        DictTableName = start.TableName;
                        GridDictFields = LoadDictSafe(DictTableName);
                    }
                }
                else
                {
                    paramStr = BuildParamStrFromDict(className, ParamTableName);
                }

                DefaultQueryValues = await LoadDefaultParamRowAsync(conn, SpId, item?.ItemId ?? itemId);
                foreach (var kv in ParamNameByField)
                {
                    var field = kv.Key;
                    var param = kv.Value;
                    if (!DefaultQueryValues.ContainsKey(field) && DefaultQueryValues.TryGetValue(param, out var v))
                        DefaultQueryValues[field] = v;
                }

                var rows = await ExecInqAsync(conn, paramStr, SpId, item, DefaultQueryValues);
                var fields = (GridDictFields ?? new List<CURdTableField>())
                    .Where(f => (f.Visible ?? 1) != 0)
                    .OrderBy(f => f.SerialNum ?? int.MaxValue)
                    .ToList();

                using var wb = new XLWorkbook();
                var ws = wb.AddWorksheet("SPO00003");
                for (var c = 0; c < fields.Count; c++)
                {
                    ws.Cell(1, c + 1).Value = fields[c].DisplayLabel ?? fields[c].FieldName;
                    ws.Cell(1, c + 1).Style.Font.Bold = true;
                }

                for (var r = 0; r < rows.Count; r++)
                {
                    var row = rows[r];
                    for (var c = 0; c < fields.Count; c++)
                    {
                        var field = fields[c].FieldName ?? string.Empty;
                        if (string.IsNullOrWhiteSpace(field)) continue;
                        row.TryGetValue(field, out var val);
                        ws.Cell(r + 2, c + 1).Value = val == null
                            ? default(XLCellValue)
                            : XLCellValue.FromObject(val);
                    }
                }

                try
                {
                    ws.Columns().AdjustToContents();
                }
                catch
                {
                    // Fallback: estimate widths from header/data text length.
                    for (var c = 0; c < fields.Count; c++)
                    {
                        var header = fields[c].DisplayLabel ?? fields[c].FieldName ?? string.Empty;
                        var maxLen = header.Length;
                        var field = fields[c].FieldName ?? string.Empty;
                        if (!string.IsNullOrWhiteSpace(field))
                        {
                            for (var r = 0; r < rows.Count; r++)
                            {
                                rows[r].TryGetValue(field, out var v);
                                var s = v?.ToString() ?? string.Empty;
                                if (s.Length > maxLen) maxLen = s.Length;
                            }
                        }
                        var width = Math.Min(60, Math.Max(10, maxLen + 2));
                        ws.Column(c + 1).Width = width;
                    }
                }

                var fileName = $"SPO00003_{DateTime.Now:yyyyMMdd}.xlsx";
                using var stream = new MemoryStream();
                wb.SaveAs(stream);
                stream.Position = 0;
                return File(stream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName);
            }
            finally
            {
                if (opened) await _context.Database.CloseConnectionAsync();
            }
        }

        private List<CURdTableField> LoadDictSafe(string? tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName)) return new List<CURdTableField>();
            try
            {
                var list = _dictService.GetFieldDict(tableName, typeof(object));
                return list ?? new List<CURdTableField>();
            }
            catch
            {
                return new List<CURdTableField>();
            }
        }

        private string ResolveParamTableName(string className)
        {
            if (string.Equals(className, "SPOdOutInq", StringComparison.OrdinalIgnoreCase))
                return "SPOdMPSOutInqParams";
            if (string.Equals(className, "SPOdOutInvInq", StringComparison.OrdinalIgnoreCase))
                return "SPOdMPSOutInvInqParams";
            return $"{className}Params";
        }

        private async Task<(string ParamStr, int SpId, string TableName)> CallInqStartAsync(DbConnection conn, string itemId)
        {
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "exec SPOdInqStart @ItemId";
            var p = cmd.CreateParameter();
            p.ParameterName = "@ItemId";
            p.Value = itemId;
            cmd.Parameters.Add(p);

            await using var rd = await cmd.ExecuteReaderAsync();
            if (!await rd.ReadAsync())
                throw new InvalidOperationException("SPOdInqStart did not return data.");

            var paramStr = rd["ParamStr"]?.ToString() ?? "";
            var spId = rd["SPId"] == DBNull.Value ? 0 : Convert.ToInt32(rd["SPId"]);
            var tableName = rd["TableName"]?.ToString() ?? "";

            return (paramStr, spId, tableName);
        }

        private List<string> ParseParamNames(string paramStr, out string procName)
        {
            procName = "";
            var text = (paramStr ?? "").Trim();
            if (text.StartsWith("exec ", StringComparison.OrdinalIgnoreCase))
                text = text.Substring(5).Trim();

            var firstSpace = text.IndexOf(' ');
            if (firstSpace < 0)
            {
                procName = text.Trim();
                return new List<string>();
            }

            procName = text.Substring(0, firstSpace).Trim();
            var rawParams = text.Substring(firstSpace + 1).Trim();
            if (string.IsNullOrWhiteSpace(rawParams))
                return new List<string>();

            return rawParams
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(p => p.Trim().TrimStart(':').TrimStart('@'))
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToList();
        }

        private string BuildParamStrFromDict(string className, string? paramTableName)
        {
            if (string.IsNullOrWhiteSpace(className) || string.IsNullOrWhiteSpace(paramTableName))
                return $"exec {className}";

            var fields = LoadDictSafe(paramTableName)
                .Where(x => !string.IsNullOrWhiteSpace(x.FieldName) && x.FieldName.StartsWith("@"))
                .OrderBy(x => x.SerialNum ?? int.MaxValue)
                .Select(x => x.FieldName.TrimStart('@'))
                .ToList();

            var paramList = string.Join(",", fields.Select(f => ":" + f));
            return $"exec {className} {paramList}";
        }

        private async Task<Dictionary<string, object?>> LoadDefaultParamRowAsync(DbConnection conn, int spId, string itemId)
        {
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "select top 1 * from SPOdInqParams with (nolock) where SPId=@spid and ItemId=@itemId";
            var p1 = cmd.CreateParameter();
            p1.ParameterName = "@spid";
            p1.Value = spId;
            cmd.Parameters.Add(p1);
            var p2 = cmd.CreateParameter();
            p2.ParameterName = "@itemId";
            p2.Value = itemId;
            cmd.Parameters.Add(p2);

            await using var rd = await cmd.ExecuteReaderAsync();
            if (!await rd.ReadAsync()) return new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

            var map = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < rd.FieldCount; i++)
            {
                var key = rd.GetName(i);
                var val = rd.IsDBNull(i) ? null : rd.GetValue(i);
                map[key] = val;
            }
            return map;
        }

        private object? CoerceValue(string? raw, CURdTableField? field)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            var dt = (field?.DataType ?? "").ToLowerInvariant();

            if (dt.Contains("date"))
            {
                if (DateTime.TryParse(raw, out var d)) return d;
            }
            if (dt.Contains("int") || dt.Contains("smallint") || dt.Contains("tinyint") || dt.Contains("bigint"))
            {
                if (int.TryParse(raw, out var n)) return n;
            }
            if (dt.Contains("decimal") || dt.Contains("numeric") || dt.Contains("money") || dt.Contains("float") || dt.Contains("real"))
            {
                if (decimal.TryParse(raw, out var dec)) return dec;
            }

            return raw;
        }

        private async Task<List<Dictionary<string, object?>>> ExecInqAsync(DbConnection conn, string paramStr, int spId, CurdSysItem? item, Dictionary<string, object?> defaults)
        {
            var paramNames = ParseParamNames(paramStr, out var procName);
            if (string.IsNullOrWhiteSpace(procName))
                return new List<Dictionary<string, object?>>();

            var useId = ResolveUseId();
            var userId = ResolveUserId();
            var powerType = item?.PowerType ?? 0;

            var qFieldMap = (QueryDictFields ?? new List<CURdTableField>())
                .Where(f => !string.IsNullOrWhiteSpace(f.FieldName))
                .ToDictionary(f => f.FieldName.TrimStart('@'), f => f, StringComparer.OrdinalIgnoreCase);

            var paramValues = new List<(string Name, object? Value)>();
            foreach (var p in paramNames)
            {
                object? val = null;
                var qsVal = Request.Query[p].ToString();
                if (!string.IsNullOrWhiteSpace(qsVal) && qsVal.Contains(','))
                {
                    qsVal = qsVal.Split(',').LastOrDefault()?.Trim() ?? qsVal;
                }
                if (!string.IsNullOrWhiteSpace(qsVal))
                {
                    CURdTableField? f = null;
                    if (!qFieldMap.TryGetValue(p, out f) && FieldNameByParam.TryGetValue(p, out var fn))
                        qFieldMap.TryGetValue(fn, out f);
                    val = CoerceValue(qsVal, f);
                }
                else if (string.Equals(p, "SpId", StringComparison.OrdinalIgnoreCase))
                {
                    val = spId;
                }
                else if (string.Equals(p, "sUseId", StringComparison.OrdinalIgnoreCase))
                {
                    val = useId;
                }
                else if (string.Equals(p, "sUserId", StringComparison.OrdinalIgnoreCase))
                {
                    val = userId;
                }
                else if (string.Equals(p, "PowerType", StringComparison.OrdinalIgnoreCase))
                {
                    val = powerType;
                }
                else if (defaults.TryGetValue(p, out var dv))
                {
                    val = dv;
                }
                else if (FieldNameByParam.TryGetValue(p, out var fieldName)
                         && defaults.TryGetValue(fieldName, out var dv2))
                {
                    val = dv2;
                }

                if (val is string s && string.IsNullOrWhiteSpace(s))
                    val = null;

                // Delphi default behaviors (critical for getting rows)
                if (val == null)
                {
                    if (string.Equals(p, "iPost", StringComparison.OrdinalIgnoreCase)) val = 1;
                    else if (string.Equals(p, "Charge", StringComparison.OrdinalIgnoreCase)) val = 1;
                    else if (string.Equals(p, "OutNotIn", StringComparison.OrdinalIgnoreCase)) val = 255;
                    else if (string.Equals(p, "iNotFinish", StringComparison.OrdinalIgnoreCase)) val = 0;
                    else if (string.Equals(p, "iWaitAudit", StringComparison.OrdinalIgnoreCase)) val = 0;
                    else if (string.Equals(p, "iAct", StringComparison.OrdinalIgnoreCase)) val = 0;
                    else if (string.Equals(p, "iStop", StringComparison.OrdinalIgnoreCase)) val = 0;
                }

                // Always pass all parameters (Delphi does), allow NULL if no value/default.
                paramValues.Add((p, val));
            }

            var execText = $"exec {procName} {string.Join(", ", paramValues.Select(p => "@" + p.Name + "=@" + p.Name))}";
            DebugExecText = execText;
            DebugParams = paramValues.ToList();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = execText;
            cmd.CommandType = CommandType.Text;

            foreach (var p in paramValues)
            {
                var pp = cmd.CreateParameter();
                pp.ParameterName = "@" + p.Name;
                pp.Value = p.Value ?? DBNull.Value;
                cmd.Parameters.Add(pp);
            }

            var rows = new List<Dictionary<string, object?>>();
            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < rd.FieldCount; i++)
                {
                    var key = rd.GetName(i);
                    row[key] = rd.IsDBNull(i) ? null : rd.GetValue(i);
                }
                rows.Add(row);
            }
            return rows;
        }

        public string BuildPageUrl(int page)
        {
            var map = Request.Query.ToDictionary(k => k.Key, v => (string?)v.Value.ToString(), StringComparer.OrdinalIgnoreCase);
            map["page"] = page.ToString();
            map["pageSize"] = PageSize.ToString();
            return QueryString.Create(map).ToString();
        }

        private string ResolveUseId()
        {
            var claim = User?.Claims?.FirstOrDefault(c => string.Equals(c.Type, "UseId", StringComparison.OrdinalIgnoreCase))?.Value;
            var item = HttpContext.Items["UseId"]?.ToString();
            return string.IsNullOrWhiteSpace(claim)
                ? (string.IsNullOrWhiteSpace(item) ? "A001" : item)
                : claim;
        }

        private string ResolveUserId()
        {
            var claim = User?.Claims?.FirstOrDefault(c => string.Equals(c.Type, "UserId", StringComparison.OrdinalIgnoreCase))?.Value;
            var item = HttpContext.Items["UserId"]?.ToString();
            return string.IsNullOrWhiteSpace(claim)
                ? (string.IsNullOrWhiteSpace(item) ? "admin" : item)
                : claim;
        }

        private async Task<(Dictionary<string, string> ParamByField, Dictionary<string, string> FieldByParam)> LoadParamMapAsync(DbConnection conn, string? paramTableName)
        {
            var byField = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var byParam = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(paramTableName)) return (byField, byParam);

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "select ParamName, FieldName from SPOdInqParamsField with (nolock) where SPName=@sp";
            var p = cmd.CreateParameter();
            p.ParameterName = "@sp";
            p.Value = paramTableName;
            cmd.Parameters.Add(p);

            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                var param = rd["ParamName"]?.ToString()?.Trim();
                var field = rd["FieldName"]?.ToString()?.Trim();
                if (string.IsNullOrWhiteSpace(param) || string.IsNullOrWhiteSpace(field)) continue;
                byField[field.TrimStart('@')] = param.TrimStart('@');
                byParam[param.TrimStart('@')] = field.TrimStart('@');
            }
            return (byField, byParam);
        }

        public sealed class CustomerSelectRequest
        {
            public int SpId { get; set; }
            public List<string> CustomerIds { get; set; } = new();
        }

        public async Task<IActionResult> OnGetCustomerSelectedAsync(int spId)
        {
            if (spId <= 0)
                return new JsonResult(new { items = Array.Empty<string>() });

            var conn = _context.Database.GetDbConnection();
            var opened = conn.State != ConnectionState.Open;
            if (opened) await _context.Database.OpenConnectionAsync();
            try
            {
                var list = new List<string>();
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = "select Selected from SPOdInqTable with (nolock) where SpId=@spid and PaperId='SPOdOrderInq' order by Item";
                var p = cmd.CreateParameter();
                p.ParameterName = "@spid";
                p.Value = spId;
                cmd.Parameters.Add(p);

                await using var rd = await cmd.ExecuteReaderAsync();
                while (await rd.ReadAsync())
                {
                    var v = rd["Selected"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(v)) list.Add(v.Trim());
                }
                return new JsonResult(new { items = list });
            }
            finally
            {
                if (opened) await _context.Database.CloseConnectionAsync();
            }
        }

        public async Task<IActionResult> OnPostSaveCustomersAsync([FromBody] CustomerSelectRequest req)
        {
            if (req == null || req.SpId <= 0)
                return BadRequest("SpId is required.");

            var conn = _context.Database.GetDbConnection();
            var opened = conn.State != ConnectionState.Open;
            if (opened) await _context.Database.OpenConnectionAsync();
            try
            {
                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "delete SPOdInqTable where SpId=@spid and PaperId='SPOdOrderInq'";
                    var p = cmd.CreateParameter();
                    p.ParameterName = "@spid";
                    p.Value = req.SpId;
                    cmd.Parameters.Add(p);
                    await cmd.ExecuteNonQueryAsync();
                }

                var item = 1;
                foreach (var id in req.CustomerIds.Where(x => !string.IsNullOrWhiteSpace(x)))
                {
                    await using var cmd = conn.CreateCommand();
                    cmd.CommandText = "insert into SPOdInqTable(SPId,Item,PaperId,Selected) values (@spid,@item,'SPOdOrderInq',@sel)";
                    var p1 = cmd.CreateParameter();
                    p1.ParameterName = "@spid";
                    p1.Value = req.SpId;
                    cmd.Parameters.Add(p1);
                    var p2 = cmd.CreateParameter();
                    p2.ParameterName = "@item";
                    p2.Value = item++;
                    cmd.Parameters.Add(p2);
                    var p3 = cmd.CreateParameter();
                    p3.ParameterName = "@sel";
                    p3.Value = id.Trim();
                    cmd.Parameters.Add(p3);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            finally
            {
                if (opened) await _context.Database.CloseConnectionAsync();
            }

            return new JsonResult(new { ok = true });
        }

        public async Task<IActionResult> OnPostResetParamsAsync()
        {
            const string itemId = "SPO00003";
            var conn = _context.Database.GetDbConnection();
            var opened = conn.State != ConnectionState.Open;
            if (opened) await _context.Database.OpenConnectionAsync();
            try
            {
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = "exec SPOdImportInqParams @ItemId";
                var p = cmd.CreateParameter();
                p.ParameterName = "@ItemId";
                p.Value = itemId;
                cmd.Parameters.Add(p);
                await cmd.ExecuteNonQueryAsync();
            }
            finally
            {
                if (opened) await _context.Database.CloseConnectionAsync();
            }
            return new JsonResult(new { ok = true });
        }
    }
}
