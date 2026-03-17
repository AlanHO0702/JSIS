using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;
using PcbErpApi.Services;

namespace PcbErpApi.Pages.FPE
{
    public class FPE00020Model : PageModel
    {
        private readonly PcbErpContext _context;
        private readonly ITableDictionaryService _dictService;

        public FPE00020Model(PcbErpContext context, ITableDictionaryService dictService)
        {
            _context = context;
            _dictService = dictService;
        }

        public string PageTitle => "FPE00020 品號資訊查詢";

        // 每個 tab 對應的 SP 名稱與 DB 字典表名
        public record TabInfo(string Label, string SpName, string DictTable);

        public List<TabInfo> Tabs { get; } = new()
        {
            new("製程現帳", "FMEdPartNumView_WIP",   "FMEdPartNumView_WIP"),
            new("製令單",   "FMEdPartNumView_Issue", "FMEdPartNumView_Issue"),
            new("過帳單",   "FMEdPartNumView_Pass",  "FMEdPartNumView_Pass"),
            new("入庫單",   "FMEdPartNumView_InFFG", "FMEdPartNumView_InFFG"),
        };

        [BindProperty(SupportsGet = true, Name = "Tab")]
        public int ActiveTab { get; set; } = 0;

        [BindProperty(SupportsGet = true, Name = "page")]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true, Name = "pageSize")]
        public int PageSize { get; set; } = 50;

        public string DictTableName => ActiveTab >= 0 && ActiveTab < Tabs.Count
            ? Tabs[ActiveTab].DictTable : Tabs[0].DictTable;

        public List<CURdTableField> GridDictFields { get; set; } = new();
        public string? DebugExecText { get; set; }
        public bool HasSubmitted => Request.Query["submitted"].ToString() == "1";

        public void OnGet()
        {
            if (ActiveTab < 0 || ActiveTab >= Tabs.Count) ActiveTab = 0;
            // 頁面初始載入只準備頁面結構，實際查詢由前端 AJAX 發起
        }

        private async Task<List<Dictionary<string, object?>>> ExecInqAsync(System.Data.Common.DbConnection conn)
        {
            var partNum  = Request.Query["PartNum"].ToString().Trim();
            var revision = Request.Query["Revision"].ToString().Trim();
            var dateCode = Request.Query["DateCode"].ToString().Trim();
            var fPaperDate = Request.Query["FPaperDate"].ToString().Trim();
            var ePaperDate = Request.Query["EPaperDate"].ToString().Trim();

            var spName = Tabs[ActiveTab].SpName;

            await using var cmd = conn.CreateCommand();
            cmd.CommandType = CommandType.Text;

            if (ActiveTab == 0)
            {
                // WIP: 只有 PartNum, Revision, DateCode
                cmd.CommandText = $"Exec {spName} @PartNum, @Revision, @DateCode";
                DebugExecText = $"Exec {spName} '{partNum}', '{revision}', '{dateCode}'";

                AddParam(cmd, "@PartNum",  partNum);
                AddParam(cmd, "@Revision", revision);
                AddParam(cmd, "@DateCode", dateCode);
            }
            else
            {
                // Issue/Pass/InFFG: 加上 FPaperDate, EPaperDate
                cmd.CommandText = $"Exec {spName} @PartNum, @Revision, @DateCode, @FPaperDate, @EPaperDate";
                DebugExecText = $"Exec {spName} '{partNum}', '{revision}', '{dateCode}', '{fPaperDate}', '{ePaperDate}'";

                AddParam(cmd, "@PartNum",    partNum);
                AddParam(cmd, "@Revision",   revision);
                AddParam(cmd, "@DateCode",   dateCode);
                AddParam(cmd, "@FPaperDate", string.IsNullOrEmpty(fPaperDate) ? "" : fPaperDate);
                AddParam(cmd, "@EPaperDate", string.IsNullOrEmpty(ePaperDate) ? "" : ePaperDate);
            }

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

        private static void AddParam(System.Data.Common.DbCommand cmd, string name, object? value)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }

        private List<CURdTableField> LoadDictSafe(string tableName)
        {
            try { return _dictService.GetFieldDict(tableName, typeof(object)) ?? new List<CURdTableField>(); }
            catch { return new List<CURdTableField>(); }
        }

        // AJAX handler：回傳欄位定義 + 資料列 JSON
        public async Task<IActionResult> OnGetQueryAsync()
        {
            if (ActiveTab < 0 || ActiveTab >= Tabs.Count) ActiveTab = 0;

            var dictFields = LoadDictSafe(Tabs[ActiveTab].DictTable)
                .Where(f => f.Visible == 1)
                .OrderBy(f => f.SerialNum ?? int.MaxValue)
                .Select(f => new
                {
                    fieldName  = f.FieldName ?? "",
                    label      = string.IsNullOrWhiteSpace(f.DisplayLabel) ? f.FieldName : f.DisplayLabel,
                    dataType   = f.DataType ?? "",
                    formatStr  = f.FormatStr ?? ""
                })
                .ToList();

            // 未帶查詢條件時只回傳欄位定義，不執行 SP
            if (!HasEffectiveQuery(Request.Query))
            {
                return new JsonResult(new { fields = dictFields, rows = Array.Empty<object>(), totalCount = 0 });
            }

            var conn = _context.Database.GetDbConnection();
            var opened = conn.State != ConnectionState.Open;
            if (opened) await _context.Database.OpenConnectionAsync();
            try
            {
                var rows = await ExecInqAsync(conn);
                // 將資料列序列化為簡單的 string?[][] 結構（依欄位順序）
                var fieldNames = dictFields.Select(f => f.fieldName).ToList();
                var serialized = rows.Select(row =>
                    fieldNames.Select(fn =>
                    {
                        row.TryGetValue(fn, out var v);
                        return v == null ? null : Convert.ToString(v);
                    }).ToArray()
                ).ToArray();

                return new JsonResult(new { fields = dictFields, rows = serialized, totalCount = rows.Count });
            }
            finally
            {
                if (opened) await _context.Database.CloseConnectionAsync();
            }
        }

        private static bool HasEffectiveQuery(Microsoft.AspNetCore.Http.IQueryCollection query)
        {
            return query["submitted"].ToString() == "1";
        }
    }
}
