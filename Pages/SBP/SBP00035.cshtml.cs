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

namespace PcbErpApi.Pages.SBP
{
    public class SBP00035Model : PageModel
    {
        private readonly PcbErpContext _context;
        private readonly ITableDictionaryService _dictService;

        public SBP00035Model(PcbErpContext context, ITableDictionaryService dictService)
        {
            _context = context;
            _dictService = dictService;
        }

        public string PageTitle => "SBP00035 缺料查詢2-生管";
        public string DictTableName { get; set; } = "FMEdLessMatInq2";

        [BindProperty(SupportsGet = true, Name = "page")]
        public int PageNumber { get; set; } = 1;
        [BindProperty(SupportsGet = true, Name = "pageSize")]
        public int PageSize { get; set; } = 50;

        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public List<Dictionary<string, object?>> Items { get; set; } = new();
        public List<CURdTableField> GridDictFields { get; set; } = new();
        public List<(string Value, string Label)> MatClassOptions { get; set; } = new();
        public List<(string Value, string Label)> POTypeOptions { get; set; } = new();
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
                await LoadPOTypeOptionsAsync(conn);

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
                var ws = wb.AddWorksheet("SBP00035");
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
                    $"SBP00035_{DateTime.Now:yyyyMMdd}.xlsx");
            }
            finally
            {
                if (opened) await _context.Database.CloseConnectionAsync();
            }
        }

        private async Task LoadMatClassOptionsAsync(System.Data.Common.DbConnection conn)
        {
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "select MatClass, ClassName from MINdMatClass (nolock) order by MatClass";
            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
                MatClassOptions.Add((rd["MatClass"]?.ToString() ?? "", rd["ClassName"]?.ToString() ?? ""));
        }

        private async Task LoadPOTypeOptionsAsync(System.Data.Common.DbConnection conn)
        {
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "select POType, POTypeName from FMEdPOType (nolock) order by POType";
            try
            {
                await using var rd = await cmd.ExecuteReaderAsync();
                while (await rd.ReadAsync())
                    POTypeOptions.Add((rd["POType"]?.ToString() ?? "", rd["POTypeName"]?.ToString() ?? ""));
            }
            catch { }
        }

        private async Task<List<Dictionary<string, object?>>> ExecInqAsync(System.Data.Common.DbConnection conn)
        {
            var partNum = Request.Query["PartNum"].ToString().Trim();
            var matName = Request.Query["MatName"].ToString().Trim();
            var matClass = Request.Query["MatClass"].ToString().Trim();
            var inqDate = Request.Query["InqDate"].ToString().Trim();
            var poTypeStr = Request.Query["POType"].ToString().Trim();
            var nplRatioStr = Request.Query["NPLratio"].ToString().Trim();

            var date = string.IsNullOrEmpty(inqDate) ? "1900/01/01" : inqDate.Replace("-", "/");
            var poType = string.IsNullOrEmpty(poTypeStr) ? "255" : poTypeStr;
            var nplRatio = string.IsNullOrEmpty(nplRatioStr) ? "0" : nplRatioStr;
            var useId = ResolveUseId();

            await using var cmd = conn.CreateCommand();
            // exec MINdLookMatInfoDtl_MUT PartNum, iInqKind, InqDate, MatClass, MB, UseId, 0, '', '', 1, MatName, 0, '', 0, 0, '', '', POType, NPLratio
            cmd.CommandText = "exec MINdLookMatInfoDtl_MUT @PartNum, @iInqKind, @InqDate, @MatClass, @MB, @UseId, @p7, @p8, @p9, @p10, @MatName, @p12, @p13, @p14, @p15, @p16, @p17, @POType, @NPLratio";
            cmd.CommandType = CommandType.Text;

            void AddParam(string name, object? value)
            {
                var p = cmd.CreateParameter();
                p.ParameterName = name;
                p.Value = value ?? DBNull.Value;
                cmd.Parameters.Add(p);
            }

            AddParam("@PartNum", partNum);
            AddParam("@iInqKind", 4);
            AddParam("@InqDate", date);
            AddParam("@MatClass", matClass);
            AddParam("@MB", 255);
            AddParam("@UseId", useId);
            AddParam("@p7", 0);
            AddParam("@p8", "");
            AddParam("@p9", "");
            AddParam("@p10", 1);
            AddParam("@MatName", matName);
            AddParam("@p12", 0);
            AddParam("@p13", "");
            AddParam("@p14", 0);
            AddParam("@p15", 0);
            AddParam("@p16", "");
            AddParam("@p17", "");
            AddParam("@POType", poType);
            AddParam("@NPLratio", nplRatio);

            DebugExecText = $"exec MINdLookMatInfoDtl_MUT '{partNum}', 4, '{date}', '{matClass}', 255, '{useId}', 0, '', '', 1, '{matName}', 0, '', 0, 0, '', '', '{poType}', '{nplRatio}'";

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
