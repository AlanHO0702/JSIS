using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;
using PcbErpApi.Services;

namespace PcbErpApi.Pages.FME
{
    public class FME00014Model : PageModel
    {
        private readonly PcbErpContext _context;
        private readonly ITableDictionaryService _dictService;

        public FME00014Model(PcbErpContext context, ITableDictionaryService dictService)
        {
            _context = context;
            _dictService = dictService;
        }

        public string PageTitle => "FME00014 缺料查詢";
        public string DictTableName { get; set; } = "FMEdLessMatInq";

        [BindProperty(SupportsGet = true, Name = "page")]
        public int PageNumber { get; set; } = 1;
        [BindProperty(SupportsGet = true, Name = "pageSize")]
        public int PageSize { get; set; } = 50;

        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public List<Dictionary<string, object?>> Items { get; set; } = new();
        public List<CURdTableField> GridDictFields { get; set; } = new();
        public List<(string Value, string Label, int MB)> MatClassOptions { get; set; } = new();
        public string? DebugExecText { get; set; }

        public async Task OnGetAsync()
        {
            var conn = _context.Database.GetDbConnection();
            var opened = conn.State != ConnectionState.Open;
            if (opened) await _context.Database.OpenConnectionAsync();
            try
            {
                GridDictFields = LoadDictSafe(DictTableName);
                await LoadMatClassOptionsAsync(conn);

                if (HasEffectiveQuery(Request.Query))
                {
                    var rows = await ExecInqAsync(conn);
                    TotalCount = rows.Count;
                    TotalPages = Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
                    Items = rows.Skip((PageNumber - 1) * PageSize).Take(PageSize).ToList();
                }
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
                GridDictFields = LoadDictSafe(DictTableName);
                var rows = await ExecInqAsync(conn);
                var fields = GridDictFields.Where(f => (f.Visible ?? 1) != 0).OrderBy(f => f.SerialNum ?? int.MaxValue).ToList();

                using var wb = new XLWorkbook();
                var ws = wb.AddWorksheet("FME00014");
                for (var c = 0; c < fields.Count; c++)
                {
                    ws.Cell(1, c + 1).Value = fields[c].DisplayLabel ?? fields[c].FieldName;
                    ws.Cell(1, c + 1).Style.Font.Bold = true;
                }
                for (var r = 0; r < rows.Count; r++)
                {
                    for (var c = 0; c < fields.Count; c++)
                    {
                        var field = fields[c].FieldName ?? string.Empty;
                        if (string.IsNullOrWhiteSpace(field)) continue;
                        rows[r].TryGetValue(field, out var val);
                        ws.Cell(r + 2, c + 1).Value = val == null ? default(XLCellValue) : XLCellValue.FromObject(val);
                    }
                }
                try { ws.Columns().AdjustToContents(); } catch { }

                using var stream = new System.IO.MemoryStream();
                wb.SaveAs(stream);
                stream.Position = 0;
                return File(stream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"FME00014_{DateTime.Now:yyyyMMdd}.xlsx");
            }
            finally
            {
                if (opened) await _context.Database.CloseConnectionAsync();
            }
        }

        private async Task LoadMatClassOptionsAsync(System.Data.Common.DbConnection conn)
        {
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "select MatClass, ClassName, MB from MINdMatClass (nolock) order by MatClass";
            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                var mb = rd["MB"] is DBNull ? 255 : Convert.ToInt32(rd["MB"]);
                MatClassOptions.Add((rd["MatClass"]?.ToString() ?? "", rd["ClassName"]?.ToString() ?? "", mb));
            }
        }

        private async Task<List<Dictionary<string, object?>>> ExecInqAsync(System.Data.Common.DbConnection conn)
        {
            var partNum = Request.Query["PartNum"].ToString().Trim();
            var matName = Request.Query["MatName"].ToString().Trim();
            var matClass = Request.Query["MatClass"].ToString().Trim();
            var mbStr = Request.Query["MB"].ToString().Trim();
            var inqDateFrom = Request.Query["InqDateFrom"].ToString().Trim();
            var inqDateTo = Request.Query["InqDateTo"].ToString().Trim();
            var inqTypeStr = Request.Query["InqType"].ToString().Trim();

            var mb = (mbStr == "0" || mbStr == "1" || mbStr == "255") ? int.Parse(mbStr) : 255;
            var dateFrom = string.IsNullOrEmpty(inqDateFrom) ? "1900/01/01" : inqDateFrom.Replace("-", "/");
            var dateTo = string.IsNullOrEmpty(inqDateTo) ? "1900/01/01" : inqDateTo.Replace("-", "/");
            var inqType = int.TryParse(inqTypeStr, out var t) ? t : 0;
            var useId = ResolveUseId();

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "exec MINdLookMatInfoDtl_Std @PartNum, @iInqKind, @InqDate, @InqDate2, @MatClass, @MB, @UseId, @p8, @p9, @p10, @MatName, @p12, @InqType";
            cmd.CommandType = CommandType.Text;

            void AddParam(string name, object? value)
            {
                var p = cmd.CreateParameter();
                p.ParameterName = name;
                p.Value = value ?? DBNull.Value;
                cmd.Parameters.Add(p);
            }

            AddParam("@PartNum", partNum);
            AddParam("@iInqKind", 1);
            AddParam("@InqDate", dateFrom);
            AddParam("@InqDate2", dateTo);
            AddParam("@MatClass", matClass);
            AddParam("@MB", mb);
            AddParam("@UseId", useId);
            AddParam("@p8", 0);
            AddParam("@p9", "");
            AddParam("@p10", "");
            AddParam("@MatName", matName);
            AddParam("@p12", 0);
            AddParam("@InqType", inqType);

            DebugExecText = $"exec MINdLookMatInfoDtl_Std '{partNum}', 1, '{dateFrom}', '{dateTo}', '{matClass}', {mb}, '{useId}', 0, '', '', '{matName}', 0, {inqType}";

            var rows = new List<Dictionary<string, object?>>();
            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < rd.FieldCount; i++)
                    row[rd.GetName(i)] = rd.IsDBNull(i) ? null : rd.GetValue(i);
                rows.Add(row);
            }
            return rows;
        }

        private List<CURdTableField> LoadDictSafe(string tableName)
        {
            try { return _dictService.GetFieldDict(tableName, typeof(object)) ?? new List<CURdTableField>(); }
            catch { return new List<CURdTableField>(); }
        }

        public string BuildPageUrl(int page)
        {
            var map = Request.Query.ToDictionary(k => k.Key, v => (string?)v.Value.ToString(), StringComparer.OrdinalIgnoreCase);
            map["page"] = page.ToString();
            map["pageSize"] = PageSize.ToString();
            return QueryString.Create(map).ToString();
        }

        private static bool HasEffectiveQuery(Microsoft.AspNetCore.Http.IQueryCollection query)
        {
            var skip = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "page", "pageSize", "handler", "debug", "__RequestVerificationToken" };
            foreach (var key in query.Keys)
            {
                if (skip.Contains(key)) continue;
                if (!string.IsNullOrWhiteSpace(query[key].ToString())) return true;
            }
            return false;
        }

        private string ResolveUseId()
        {
            var claim = User?.Claims?.FirstOrDefault(c => string.Equals(c.Type, "UseId", StringComparison.OrdinalIgnoreCase))?.Value;
            var item = HttpContext.Items["UseId"]?.ToString();
            return string.IsNullOrWhiteSpace(claim) ? (string.IsNullOrWhiteSpace(item) ? "A001" : item) : claim;
        }
    }
}
