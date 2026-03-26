using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;

namespace PcbErpApi.Pages.MPH
{
    public class MPH00018Model : PageModel
    {
        private const string ItemId = "MPH00018";
        private const string DictTable = "MPHdV_RejectInq";
        private readonly PcbErpContext _context;

        public MPH00018Model(PcbErpContext context)
        {
            _context = context;
        }

        public string PageTitle => "MPH00018 廠商退貨單查詢";

        [BindProperty(SupportsGet = true, Name = "page")]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true, Name = "pageSize")]
        public int PageSize { get; set; } = 50;

        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public List<Dictionary<string, object?>> Items { get; set; } = new();
        public List<CURdTableField> FieldDictList { get; set; } = new();
        public List<CurdPaperSelected> QueryFields { get; set; } = new();
        public string CurrentUserId { get; set; } = "";
        public string CurrentUseId { get; set; } = "";

        private static readonly string[] FallbackFields =
        {
            "PaperNum","PaperDate","CustomerId","ShortName","FinishedName","Item","PartNum",
            "UOMQnty","SourNum","SourItem","Treatment","Defects","ChangedName","ActionName",
            "Qnty","PaperId"
        };

        private static readonly HashSet<string> AllowedOps = new(StringComparer.OrdinalIgnoreCase)
        {
            "=",
            ">=",
            "<=",
            "like"
        };

        private static readonly HashSet<string> ReservedQueryKeys = new(StringComparer.OrdinalIgnoreCase)
        {
            "page",
            "pageSize",
            "pageIndex",
            "sortBy",
            "sortDir",
            "__RequestVerificationToken",
            "handler"
        };

        private static readonly Regex TrailingDigits = new(@"\d+$", RegexOptions.Compiled);
        private static readonly object AllowedColumnsLock = new();
        private static HashSet<string>? _cachedAllowedColumns;

        public async Task OnGetAsync([FromQuery(Name = "sortBy")] string? sortBy = null, [FromQuery(Name = "sortDir")] string? sortDir = null)
        {
            PageNumber = 1;
            PageSize = 0;

            var conn = _context.Database.GetDbConnection();
            var opened = conn.State != ConnectionState.Open;
            if (opened) await _context.Database.OpenConnectionAsync();
            try
            {
                QueryFields = await LoadQueryFieldsAsync();
                var allowed = await GetAllowedColumnsAsync(conn);
                var orderBy = BuildOrderBy(sortBy, sortDir, allowed);
                var hasQuery = HasEffectiveQuery(Request.Query);
                if (hasQuery)
                {
                    var (whereSql, parameters) = BuildWhere(Request.Query, allowed);
                    Items = await LoadRowsAllAsync(conn, whereSql, orderBy, parameters);
                    TotalCount = Items.Count;
                    TotalPages = 1;
                }
                else
                {
                    TotalCount = 0;
                    TotalPages = 1;
                    Items = new List<Dictionary<string, object?>>();
                }

                FieldDictList = await LoadFieldDictAsync(DictTable);
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
                QueryFields = await LoadQueryFieldsAsync();
                var allowed = await GetAllowedColumnsAsync(conn);
                var (whereSql, parameters) = BuildWhere(Request.Query, allowed);
                var rows = await LoadRowsAllAsync(conn, whereSql, "order by t0.PartNum, t0.PaperNum", parameters);
                var fields = await LoadFieldDictAsync(DictTable);
                var exportFields = fields
                    .Where(f => (f.Visible ?? 1) != 0)
                    .Where(f => !string.IsNullOrWhiteSpace(f.FieldName))
                    .OrderBy(f => f.SerialNum ?? int.MaxValue)
                    .ToList();

                if (exportFields.Count == 0 && rows.Count > 0)
                {
                    exportFields = rows[0].Keys
                        .Select((k, i) => new CURdTableField { FieldName = k, DisplayLabel = k, SerialNum = i })
                        .ToList();
                }

                using var wb = new XLWorkbook();
                var ws = wb.AddWorksheet(ItemId);
                for (var c = 0; c < exportFields.Count; c++)
                {
                    ws.Cell(1, c + 1).Value = exportFields[c].DisplayLabel ?? exportFields[c].FieldName;
                    ws.Cell(1, c + 1).Style.Font.Bold = true;
                }

                for (var r = 0; r < rows.Count; r++)
                {
                    var row = rows[r];
                    for (var c = 0; c < exportFields.Count; c++)
                    {
                        var field = exportFields[c].FieldName ?? string.Empty;
                        if (string.IsNullOrWhiteSpace(field)) continue;
                        row.TryGetValue(field, out var val);
                        ws.Cell(r + 2, c + 1).Value = val == null ? default(XLCellValue) : XLCellValue.FromObject(val);
                    }
                }

                try
                {
                    ws.Columns().AdjustToContents();
                }
                catch
                {
                    for (var c = 0; c < exportFields.Count; c++)
                    {
                        var header = exportFields[c].DisplayLabel ?? exportFields[c].FieldName ?? string.Empty;
                        var maxLen = header.Length;
                        var field = exportFields[c].FieldName ?? string.Empty;
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

                var fileName = $"{ItemId}_{DateTime.Now:yyyyMMdd}.xlsx";
                using var stream = new MemoryStream();
                wb.SaveAs(stream);
                stream.Position = 0;
                return File(
                    stream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName
                );
            }
            finally
            {
                if (opened) await _context.Database.CloseConnectionAsync();
            }
        }

        private async Task<List<CURdTableField>> LoadFieldDictAsync(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName)) return new List<CURdTableField>();
            try
            {
                return await _context.CURdTableFields
                    .AsNoTracking()
                    .Where(x => x.TableName == tableName)
                    .ToListAsync();
            }
            catch
            {
                return new List<CURdTableField>();
            }
        }

        private async Task<List<CurdPaperSelected>> LoadQueryFieldsAsync()
        {
            try
            {
                return await _context.CURdPaperSelected
                    .AsNoTracking()
                    .Where(x => x.TableName == DictTable && x.IVisible == 1)
                    .OrderBy(x => x.SortOrder)
                    .ToListAsync();
            }
            catch
            {
                return new List<CurdPaperSelected>();
            }
        }

        private (string WhereSql, List<DbParameter> Params) BuildWhere(Microsoft.AspNetCore.Http.IQueryCollection query, HashSet<string> allowed)
        {
            var where = "where 1=1";
            var list = new List<DbParameter>();
            var idx = 0;

            foreach (var key in query.Keys)
            {
                if (ReservedQueryKeys.Contains(key) || key.StartsWith("Cond_", StringComparison.OrdinalIgnoreCase))
                    continue;

                var val = query[key].ToString();
                if (string.IsNullOrWhiteSpace(val)) continue;
                if (!TryResolveField(key, allowed, out var field)) continue;

                var opRaw = query[$"Cond_{key}"].ToString();
                if (string.IsNullOrWhiteSpace(opRaw))
                {
                    opRaw = query[$"Cond_{field}"].ToString();
                }
                if (string.IsNullOrWhiteSpace(opRaw))
                {
                    opRaw = ResolveDefaultOpForKey(key, field);
                }

                var op = NormalizeOp(opRaw, "=");
                var paramName = $"@p{idx++}";

                if (string.Equals(op, "like", StringComparison.OrdinalIgnoreCase))
                {
                    var trimmed = val.Trim();
                    if (string.Equals(field, "PartNum", StringComparison.OrdinalIgnoreCase)
                        && !trimmed.Contains('%')
                        && !trimmed.Contains('_'))
                    {
                        var nextPrefix = BuildNextPrefix(trimmed);
                        var paramName2 = $"@p{idx++}";
                        where += $" and t0.[{field}] >= {paramName} and t0.[{field}] < {paramName2}";
                        list.Add(MakeParam(paramName, trimmed));
                        list.Add(MakeParam(paramName2, nextPrefix));
                        continue;
                    }

                    if (!trimmed.Contains('%') && !trimmed.Contains('_')) trimmed = $"%{trimmed}%";
                    where += $" and t0.[{field}] like {paramName}";
                    list.Add(MakeParam(paramName, trimmed));
                    continue;
                }

                where += $" and t0.[{field}] {op} {paramName}";
                list.Add(MakeParam(paramName, val));
            }

            return (where, list);
        }

        private string ResolveDefaultOpForKey(string key, string field)
        {
            var q = QueryFields ?? new List<CurdPaperSelected>();
            var row = q.FirstOrDefault(x =>
                string.Equals($"{x.ColumnName}{x.SortOrder}", key, StringComparison.OrdinalIgnoreCase));
            if (row == null)
            {
                row = q.FirstOrDefault(x => string.Equals(x.ColumnName, field, StringComparison.OrdinalIgnoreCase));
            }
            var op = row?.DefaultEqual;
            return string.IsNullOrWhiteSpace(op) ? "=" : op!;
        }

        private static string BuildNextPrefix(string text)
        {
            if (string.IsNullOrEmpty(text)) return char.ConvertFromUtf32(0x10FFFF);
            var chars = text.ToCharArray();
            for (var i = chars.Length - 1; i >= 0; i--)
            {
                if (chars[i] == char.MaxValue) continue;
                chars[i] = (char)(chars[i] + 1);
                return new string(chars, 0, i + 1);
            }

            return text + char.ConvertFromUtf32(0x10FFFF);
        }

        private static bool HasEffectiveQuery(Microsoft.AspNetCore.Http.IQueryCollection query)
        {
            foreach (var key in query.Keys)
            {
                if (string.IsNullOrWhiteSpace(key)) continue;
                if (ReservedQueryKeys.Contains(key) || key.StartsWith("Cond_", StringComparison.OrdinalIgnoreCase)) continue;
                if (!string.IsNullOrWhiteSpace(query[key].ToString())) return true;
            }
            return false;
        }

        private static bool TryResolveField(string key, HashSet<string> allowed, out string field)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                field = "";
                return false;
            }

            var candidates = new List<string>();
            var raw = key.Trim();
            candidates.Add(raw);

            var rawBase = TrailingDigits.Replace(raw, "");
            if (!string.IsNullOrWhiteSpace(rawBase)) candidates.Add(rawBase);

            var normalized = NormalizeColumnToken(raw);
            if (!string.IsNullOrWhiteSpace(normalized)) candidates.Add(normalized);

            var normalizedBase = TrailingDigits.Replace(normalized, "");
            if (!string.IsNullOrWhiteSpace(normalizedBase)) candidates.Add(normalizedBase);

            foreach (var candidate in candidates.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (allowed.Contains(candidate))
                {
                    field = allowed.First(x => string.Equals(x, candidate, StringComparison.OrdinalIgnoreCase));
                    return true;
                }
            }

            field = "";
            return false;
        }

        private static string NormalizeColumnToken(string token)
        {
            var s = (token ?? "").Trim();
            if (string.IsNullOrWhiteSpace(s)) return "";

            s = s.Replace("[", "").Replace("]", "");
            var dot = s.LastIndexOf('.');
            if (dot >= 0 && dot < s.Length - 1)
            {
                s = s[(dot + 1)..];
            }

            return s.Trim();
        }

        private static string NormalizeOp(string? op, string fallback)
        {
            if (string.IsNullOrWhiteSpace(op)) return fallback;
            var o = op.Trim();
            return AllowedOps.Contains(o) ? o : fallback;
        }

        private static DbParameter MakeParam(string name, object value)
        {
            var p = new Microsoft.Data.SqlClient.SqlParameter();
            p.ParameterName = name;
            p.Value = value;
            return p;
        }

        private static string BuildOrderBy(string? sortBy, string? sortDir, HashSet<string> allowed)
        {
            var field = (sortBy ?? "").Trim();
            if (!TryResolveField(field, allowed, out var resolved))
                return "order by t0.PartNum, t0.PaperNum";
            var dir = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase) ? "desc" : "asc";
            return $"order by t0.[{resolved}] {dir}, t0.PaperNum";
        }

        private static async Task<HashSet<string>> GetAllowedColumnsAsync(DbConnection conn)
        {
            if (_cachedAllowedColumns != null) return _cachedAllowedColumns;

            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = "select COLUMN_NAME from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = 'MPHdV_RejectInq'";
                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    if (!reader.IsDBNull(0))
                    {
                        var name = reader.GetString(0);
                        if (!string.IsNullOrWhiteSpace(name)) result.Add(name);
                    }
                }
            }
            catch
            {
                // ignore and use fallback
            }

            if (result.Count == 0)
            {
                result = new HashSet<string>(FallbackFields, StringComparer.OrdinalIgnoreCase);
            }

            lock (AllowedColumnsLock)
            {
                _cachedAllowedColumns ??= result;
            }

            return _cachedAllowedColumns;
        }

        private static void AddParams(DbCommand cmd, IEnumerable<DbParameter> parameters)
        {
            foreach (var p in parameters)
            {
                var cp = cmd.CreateParameter();
                cp.ParameterName = p.ParameterName;
                cp.Value = p.Value ?? DBNull.Value;
                cmd.Parameters.Add(cp);
            }
        }

        private static async Task<List<Dictionary<string, object?>>> LoadRowsAllAsync(
            DbConnection conn,
            string whereSql,
            string orderBy,
            List<DbParameter> parameters)
        {
            var list = new List<Dictionary<string, object?>>();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = $@"
select t0.*
  from MPHdV_RejectInq t0
  {whereSql}
  {orderBy}";

            AddParams(cmd, parameters);

            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                for (var i = 0; i < rd.FieldCount; i++)
                {
                    var key = rd.GetName(i);
                    row[key] = rd.IsDBNull(i) ? null : rd.GetValue(i);
                }
                list.Add(row);
            }

            return list;
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

        public async Task<IActionResult> OnPostResetParamsAsync()
        {
            var conn = _context.Database.GetDbConnection();
            var opened = conn.State != ConnectionState.Open;
            if (opened) await _context.Database.OpenConnectionAsync();
            try
            {
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = "exec SPOdImportInqParams @ItemId";
                var p = cmd.CreateParameter();
                p.ParameterName = "@ItemId";
                p.Value = ItemId;
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
