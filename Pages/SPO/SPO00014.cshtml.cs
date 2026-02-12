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
using PcbErpApi.Models;
using PcbErpApi.Data;

namespace PcbErpApi.Pages.SPO
{
    public class SPO00014Model : PageModel
    {
        private readonly PcbErpContext _context;

        public SPO00014Model(PcbErpContext context)
        {
            _context = context;
        }

        public string PageTitle => "SPO00014 客戶退貨單查詢";

        [BindProperty(SupportsGet = true, Name = "page")]
        public int PageNumber { get; set; } = 1;
        [BindProperty(SupportsGet = true, Name = "pageSize")]
        public int PageSize { get; set; } = 50;

        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public List<Dictionary<string, object?>> Items { get; set; } = new();
        public List<CURdTableField> FieldDictList { get; set; } = new();
        public string CurrentUserId { get; set; } = "";
        public string CurrentUseId { get; set; } = "";

        private static readonly string[] FallbackFields = new[]
        {
            "PaperNum","PaperDate","CustomerId","ShortName","FinishedName","Item","PartNum",
            "UOMQnty","SourNum","SourItem","Treatment","Defects","ChangedName","ActionName",
            "Qnty","PaperId"
        };

        private static readonly HashSet<string> AllowedOps = new(StringComparer.OrdinalIgnoreCase)
        {
            "=", ">=", "<=", "like"
        };

        private static readonly HashSet<string> ReservedQueryKeys = new(StringComparer.OrdinalIgnoreCase)
        {
            "page", "pageSize", "pageIndex", "sortBy", "sortDir", "__RequestVerificationToken"
        };

        private static readonly Regex TrailingDigits = new(@"\d+$", RegexOptions.Compiled);
        private static readonly object AllowedColumnsLock = new();
        private static HashSet<string>? _cachedAllowedColumns;

        public async Task OnGetAsync([FromQuery(Name = "sortBy")] string? sortBy = null, [FromQuery(Name = "sortDir")] string? sortDir = null)
        {
            PageNumber = PageNumber <= 0 ? 1 : PageNumber;
            PageSize = PageSize <= 0 ? 50 : Math.Min(PageSize, 500);

            var conn = _context.Database.GetDbConnection();
            var opened = conn.State != ConnectionState.Open;
            if (opened) await _context.Database.OpenConnectionAsync();
            try
            {
                var allowed = await GetAllowedColumnsAsync(conn);
                var orderBy = BuildOrderBy(sortBy, sortDir, allowed);
                var (whereSql, parameters) = BuildWhere(Request.Query, allowed);

                TotalCount = await CountRowsAsync(conn, whereSql, parameters);
                TotalPages = Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));

                Items = await LoadRowsAsync(conn, whereSql, orderBy, PageNumber, PageSize, parameters);
                FieldDictList = await LoadFieldDictAsync(conn, "SPOdV_MPSRejectInq");

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
                var allowed = await GetAllowedColumnsAsync(conn);
                var (whereSql, parameters) = BuildWhere(Request.Query, allowed);
                var rows = await LoadRowsAllAsync(conn, whereSql, "order by t0.PartNum, t0.PaperNum", parameters);

                var columns = new (string Header, string Field)[]
                {
                    ("單據編號","PaperNum"),
                    ("單據日期","PaperDate"),
                    ("客戶代碼","CustomerId"),
                    ("客戶名稱","ShortName"),
                    ("狀態","FinishedName"),
                    ("項次","Item"),
                    ("品號","PartNum"),
                    ("UOMQty","UOMQnty"),
                    ("來源單號","SourNum"),
                    ("來源項次","SourItem"),
                    ("處置","Treatment"),
                    ("不良現象","Defects"),
                    ("更換原因","ChangedName"),
                    ("動作","ActionName"),
                    ("數量","Qnty"),
                    ("單據別","PaperId")
                };

                using var wb = new XLWorkbook();
                var ws = wb.AddWorksheet("SPO00014");
                for (var c = 0; c < columns.Length; c++)
                {
                    ws.Cell(1, c + 1).Value = columns[c].Header;
                    ws.Cell(1, c + 1).Style.Font.Bold = true;
                }

                for (var r = 0; r < rows.Count; r++)
                {
                    var row = rows[r];
                    for (var c = 0; c < columns.Length; c++)
                    {
                        var field = columns[c].Field;
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
                    for (var c = 0; c < columns.Length; c++)
                    {
                        var header = columns[c].Header ?? string.Empty;
                        var maxLen = header.Length;
                        var field = columns[c].Field ?? string.Empty;
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

                var fileName = $"SPO00014_{DateTime.Now:yyyyMMdd}.xlsx";
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

        private async Task<List<CURdTableField>> LoadFieldDictAsync(DbConnection conn, string tableName)
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

        private static (string WhereSql, List<DbParameter> Params) BuildWhere(Microsoft.AspNetCore.Http.IQueryCollection query, HashSet<string> allowed)
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
                var op = NormalizeOp(opRaw, "=");
                var paramName = $"@p{idx++}";

                if (string.Equals(op, "like", StringComparison.OrdinalIgnoreCase))
                {
                    if (!val.Contains('%')) val = $"%{val}%";
                    where += $" and t0.[{field}] like {paramName}";
                }
                else
                {
                    where += $" and t0.[{field}] {op} {paramName}";
                }

                list.Add(MakeParam(paramName, val));
            }

            return (where, list);
        }

        private static bool TryResolveField(string key, HashSet<string> allowed, out string field)
        {
            if (allowed.Contains(key))
            {
                field = key;
                return true;
            }

            var baseKey = TrailingDigits.Replace(key, "");
            if (!string.IsNullOrWhiteSpace(baseKey) && allowed.Contains(baseKey))
            {
                field = baseKey;
                return true;
            }

            field = "";
            return false;
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

        private static async Task<int> CountRowsAsync(DbConnection conn, string whereSql, List<DbParameter> parameters)
        {
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = $"select count(1) from SPOdV_MPSRejectInq t0 {whereSql}";
            AddParams(cmd, parameters);
            var result = await cmd.ExecuteScalarAsync();
            return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
        }

        private static async Task<List<Dictionary<string, object?>>> LoadRowsAsync(DbConnection conn, string whereSql, string orderBy, int page, int pageSize, List<DbParameter> parameters)
        {
            var list = new List<Dictionary<string, object?>>();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = $@"
select t0.PaperNum,t0.PaperDate,t0.CustomerId,t0.ShortName,t0.FinishedName,
       t0.Item,t0.PartNum,t0.UOMQnty,t0.SourNum,t0.SourItem,t0.Treatment,
       t0.Defects,t0.ChangedName,t0.ActionName,t0.Qnty,t0.PaperId
  from SPOdV_MPSRejectInq t0
  {whereSql}
  {orderBy}
  offset @__skip rows fetch next @__take rows only";

            AddParams(cmd, parameters);
            var pSkip = cmd.CreateParameter();
            pSkip.ParameterName = "@__skip";
            pSkip.Value = (page - 1) * pageSize;
            cmd.Parameters.Add(pSkip);
            var pTake = cmd.CreateParameter();
            pTake.ParameterName = "@__take";
            pTake.Value = pageSize;
            cmd.Parameters.Add(pTake);

            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < rd.FieldCount; i++)
                {
                    var key = rd.GetName(i);
                    row[key] = rd.IsDBNull(i) ? null : rd.GetValue(i);
                }
                list.Add(row);
            }
            return list;
        }

        private static async Task<List<Dictionary<string, object?>>> LoadRowsAllAsync(DbConnection conn, string whereSql, string orderBy, List<DbParameter> parameters)
        {
            var list = new List<Dictionary<string, object?>>();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = $@"
select t0.PaperNum,t0.PaperDate,t0.CustomerId,t0.ShortName,t0.FinishedName,
       t0.Item,t0.PartNum,t0.UOMQnty,t0.SourNum,t0.SourItem,t0.Treatment,
       t0.Defects,t0.ChangedName,t0.ActionName,t0.Qnty,t0.PaperId
  from SPOdV_MPSRejectInq t0
  {whereSql}
  {orderBy}";

            AddParams(cmd, parameters);

            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < rd.FieldCount; i++)
                {
                    var key = rd.GetName(i);
                    row[key] = rd.IsDBNull(i) ? null : rd.GetValue(i);
                }
                list.Add(row);
            }
            return list;
        }

        private static async Task<HashSet<string>> GetAllowedColumnsAsync(DbConnection conn)
        {
            if (_cachedAllowedColumns != null) return _cachedAllowedColumns;

            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = "select COLUMN_NAME from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = 'SPOdV_MPSRejectInq'";
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
                // ignore, fallback below
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
    }
}
