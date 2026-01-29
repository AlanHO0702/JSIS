using System.Collections.Generic;
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
using PcbErpApi.Services;
using WebRazor.Models;

namespace PcbErpApi.Pages.CUR
{
    public class MasterDetailModel : PageModel
    {
        private readonly PcbErpContext _ctx;
        private readonly IBreadcrumbService _breadcrumbService;
        private readonly ILogger<MasterDetailModel> _logger;

        public MasterDetailModel(PcbErpContext ctx, IBreadcrumbService breadcrumbService, ILogger<MasterDetailModel> logger)
        {
            _ctx = ctx;
            _breadcrumbService = breadcrumbService;
            _logger = logger;
        }

        public string ItemId { get; private set; } = string.Empty;
        public string? ItemName { get; private set; }
        public MasterDetailConfig? Config { get; private set; }
        public string? LoadError { get; private set; }
        public List<CURdTableField> FieldDictList { get; private set; } = new();
        public List<CustomButtonRow> CustomButtons { get; private set; } = new();
        public HtmlString CustomButtonsHtml { get; private set; } = HtmlString.Empty;

        public async Task<IActionResult> OnGetAsync(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
                return NotFound("itemId is required.");

            var result = await MasterDetailConfigLoader.LoadAsync(_ctx, _logger, itemId);
            if (result.Config == null)
            {
                return NotFound(result.Error ?? "Master-detail config not found.");
            }

            ItemId = result.ItemId;
            ItemName = result.ItemName;
            Config = result.Config;
            LoadError = result.Error;

            ViewData["Title"] = string.IsNullOrWhiteSpace(ItemName) ? ItemId : $"{ItemId}{ItemName}";
            ViewData["DictTableName"] = Config?.MasterDict ?? Config?.MasterTable;

            try
            {
                var superId = await _ctx.CurdSysItems.AsNoTracking()
                    .Where(x => x.ItemId == ItemId)
                    .Select(x => x.SuperId)
                    .SingleOrDefaultAsync();

                if (!string.IsNullOrWhiteSpace(superId))
                    ViewData["Breadcrumbs"] = await _breadcrumbService.BuildBreadcrumbsAsync(superId);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Build breadcrumbs failed for {ItemId}", ItemId);
            }

            try
            {
                CustomButtons = await LoadCustomButtonsAsync(ItemId);
                if (CustomButtons.Count > 0)
                    CustomButtonsHtml = BuildCustomButtonsHtml(CustomButtons);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Load custom buttons failed for {ItemId}", ItemId);
            }

            try
            {
                var toolbarVisibility = await LoadToolbarButtonVisibilityAsync(ItemId);
                if (toolbarVisibility.Count > 0)
                    ViewData["ToolbarButtonVisibility"] = toolbarVisibility;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Load toolbar button visibility failed for {ItemId}", ItemId);
            }

            return Page();
        }

        private async Task<List<CustomButtonRow>> LoadCustomButtonsAsync(string itemId)
        {
            var list = new List<CustomButtonRow>();
            var cs = _ctx.Database.GetConnectionString();
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
       {(schema.hasSearchTemplate ? "SearchTemplate" : "CAST('' AS nvarchar(1)) AS SearchTemplate")},
       {(schema.hasDialogCaption ? "DialogCaption" : "CAST('' AS nvarchar(1)) AS DialogCaption")},
       bVisible, {schema.chkCol} AS ChkCanUpdate, bNeedNum, DesignType,
       {(schema.hasNeedInEdit ? "bNeedInEdit" : "0 AS bNeedInEdit")},
       {(schema.hasMultiSelectDD ? "MultiSelectDD" : "CAST('' AS nvarchar(1)) AS MultiSelectDD")},
       {(schema.hasAllowSelCount ? "AllowSelCount" : "0 AS AllowSelCount")},
       {(schema.hasReplaceExists ? "ReplaceExists" : "0 AS ReplaceExists")}
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
                    SearchTemplate = rd["SearchTemplate"]?.ToString() ?? string.Empty,
                    DialogCaption = rd["DialogCaption"]?.ToString() ?? string.Empty,
                    bVisible = TryToInt(rd["bVisible"]),
                    ChkCanUpdate = TryToInt(rd["ChkCanUpdate"]),
                    bNeedNum = TryToInt(rd["bNeedNum"]),
                    DesignType = TryToInt(rd["DesignType"]),
                    bNeedInEdit = TryToInt(rd["bNeedInEdit"]),
                    MultiSelectDD = rd["MultiSelectDD"]?.ToString() ?? string.Empty,
                    AllowSelCount = TryToInt(rd["AllowSelCount"]),
                    ReplaceExists = TryToInt(rd["ReplaceExists"])
                });
            }

            return list;
        }

        private async Task<(bool hasCaptionE, bool hasHintE, bool hasSearchTemplate, bool hasDialogCaption, bool hasNeedInEdit, bool hasMultiSelectDD, bool hasAllowSelCount, bool hasReplaceExists, string chkCol)> DetectButtonSchemaAsync(SqlConnection conn)
        {
            var cols = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            const string sql = "SELECT name FROM sys.columns WHERE object_id = OBJECT_ID('dbo.CURdOCXItemCustButton')";

            await using (var cmd = new SqlCommand(sql, conn))
            await using (var rd = await cmd.ExecuteReaderAsync())
            {
                while (await rd.ReadAsync())
                    cols.Add(rd.GetString(0));
            }

            var hasCapE = cols.Contains("CustCaptionE");
            var hasHintE = cols.Contains("CustHintE");
            var hasSearchTemplate = cols.Contains("SearchTemplate");
            var hasDialogCaption = cols.Contains("DialogCaption");
            var hasNeedInEdit = cols.Contains("bNeedInEdit");
            var hasMultiSelectDD = cols.Contains("MultiSelectDD");
            var hasAllowSelCount = cols.Contains("AllowSelCount");
            var hasReplaceExists = cols.Contains("ReplaceExists");
            var chkCol = cols.Contains("ChkCanUpdate") ? "ChkCanUpdate"
                       : cols.Contains("ChkCanbUpdate") ? "ChkCanbUpdate"
                       : "ChkCanUpdate";
            return (hasCapE, hasHintE, hasSearchTemplate, hasDialogCaption, hasNeedInEdit, hasMultiSelectDD, hasAllowSelCount, hasReplaceExists, chkCol);
        }

        private static HtmlString BuildCustomButtonsHtml(IEnumerable<CustomButtonRow> rows)
        {
            if (rows == null) return HtmlString.Empty;
            var sb = new StringBuilder();

            foreach (var b in rows)
            {
                if (!b.bVisible.HasValue || b.bVisible.Value != 1) continue;
                if (string.IsNullOrWhiteSpace(b.ButtonName)) continue;

                var caption = string.IsNullOrWhiteSpace(b.CustCaption) ? b.ButtonName : b.CustCaption;
                var hint = b.CustHint ?? string.Empty;

                sb.Append("<button type='button' class='md-custom-btn' data-custom-btn='1'");
                sb.Append(" data-button-name='").Append(System.Net.WebUtility.HtmlEncode(b.ButtonName)).Append('\'');
                sb.Append(" data-item-id='").Append(System.Net.WebUtility.HtmlEncode(b.ItemId ?? string.Empty)).Append('\'');
                sb.Append(" data-search-template='").Append(System.Net.WebUtility.HtmlEncode(b.SearchTemplate ?? string.Empty)).Append('\'');
                sb.Append(" data-design-type='").Append(System.Net.WebUtility.HtmlEncode(b.DesignType?.ToString() ?? string.Empty)).Append('\'');
                sb.Append(" data-dialog-caption='").Append(System.Net.WebUtility.HtmlEncode(b.DialogCaption ?? string.Empty)).Append('\'');
                sb.Append(" data-b-need-in-edit='").Append(System.Net.WebUtility.HtmlEncode(b.bNeedInEdit?.ToString() ?? "0")).Append('\'');
                sb.Append(" data-multi-select-dd='").Append(System.Net.WebUtility.HtmlEncode(b.MultiSelectDD ?? string.Empty)).Append('\'');
                sb.Append(" data-allow-sel-count='").Append(System.Net.WebUtility.HtmlEncode(b.AllowSelCount?.ToString() ?? "0")).Append('\'');
                sb.Append(" data-replace-exists='").Append(System.Net.WebUtility.HtmlEncode(b.ReplaceExists?.ToString() ?? "0")).Append('\'');
                sb.Append(" title='").Append(System.Net.WebUtility.HtmlEncode(hint)).Append("'>");
                sb.Append(System.Net.WebUtility.HtmlEncode(caption));
                sb.Append("</button>");
            }

            return new HtmlString(sb.ToString());
        }

        private static int? TryToInt(object? o)
        {
            if (o == null || o == System.DBNull.Value) return null;
            return int.TryParse(o.ToString(), out var n) ? n : null;
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
            public string SearchTemplate { get; set; } = string.Empty;
            public string DialogCaption { get; set; } = string.Empty;
            public int? bVisible { get; set; }
            public int? ChkCanUpdate { get; set; }
            public int? bNeedNum { get; set; }
            public int? DesignType { get; set; }
            public int? bNeedInEdit { get; set; }
            public string MultiSelectDD { get; set; } = string.Empty;
            public int? AllowSelCount { get; set; }
            public int? ReplaceExists { get; set; }
        }

        private static async Task<(string? TableName, string? VisibleColumn)> ResolveToolbarButtonTableAsync(SqlConnection conn)
        {
            const string sql = @"
SELECT TOP 1 t.name AS TableName,
       c3.name AS VisibleColumn
  FROM sys.tables t
  JOIN sys.columns c1 ON c1.object_id = t.object_id AND c1.name = 'ItemId'
  JOIN sys.columns c2 ON c2.object_id = t.object_id AND c2.name = 'ButtonName'
  JOIN sys.columns c3 ON c3.object_id = t.object_id AND (c3.name = 'bVisiable' OR c3.name = 'bVisible')
 ORDER BY CASE WHEN c3.name = 'bVisiable' THEN 0 ELSE 1 END, t.name;";
            await using var cmd = new SqlCommand(sql, conn);
            await using var rd = await cmd.ExecuteReaderAsync();
            if (!await rd.ReadAsync()) return (null, null);
            return (rd["TableName"]?.ToString(), rd["VisibleColumn"]?.ToString());
        }

        private async Task<Dictionary<string, bool>> LoadToolbarButtonVisibilityAsync(string itemId)
        {
            var map = new Dictionary<string, bool>(System.StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(itemId)) return map;

            var cs = _ctx.Database.GetConnectionString();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            var (tableName, visibleColumn) = await ResolveToolbarButtonTableAsync(conn);
            if (string.IsNullOrWhiteSpace(tableName) || string.IsNullOrWhiteSpace(visibleColumn))
                return map;

            var sql = $@"
SELECT ButtonName, [{visibleColumn}] AS IsVisible
  FROM dbo.[{tableName}] WITH (NOLOCK)
 WHERE ItemId = @itemId";
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@itemId", itemId);
            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                var btn = rd["ButtonName"]?.ToString();
                if (string.IsNullOrWhiteSpace(btn)) continue;
                var raw = rd["IsVisible"];
                var visible = raw != null && raw != System.DBNull.Value && int.TryParse(raw.ToString(), out var n) && n == 1;
                map[btn] = visible;
            }

            return map;
        }
    }
}
