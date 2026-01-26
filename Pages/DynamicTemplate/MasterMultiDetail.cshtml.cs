using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PcbErpApi.Data;
using PcbErpApi.Services;
using WebRazor.Models;

namespace PcbErpApi.Pages.CUR
{
    public class MasterMultiDetailModel : PageModel
    {
        private readonly PcbErpContext _ctx;
        private readonly IBreadcrumbService _breadcrumbService;
        private readonly ILogger<MasterMultiDetailModel> _logger;
        private readonly IConfiguration _configuration;

        public MasterMultiDetailModel(PcbErpContext ctx, IBreadcrumbService breadcrumbService, ILogger<MasterMultiDetailModel> logger, IConfiguration configuration)
        {
            _ctx = ctx;
            _breadcrumbService = breadcrumbService;
            _logger = logger;
            _configuration = configuration;
        }

        public string ItemId { get; private set; } = string.Empty;
        public string? ItemName { get; private set; }
        public MasterMultiDetailConfig? Config { get; private set; }
        public string? LoadError { get; private set; }
        public HtmlString CustomButtonsHtml { get; private set; } = HtmlString.Empty;

        public async Task<IActionResult> OnGetAsync(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
                return NotFound("itemId is required.");

            var result = await MasterMultiDetailConfigLoader.LoadAsync(_ctx, _logger, itemId);
            ItemId = result.ItemId;
            ItemName = result.ItemName;
            Config = result.Config;
            LoadError = result.Error;

            if (Config == null)
                return NotFound(result.Error ?? "Master-multi-detail config not found.");

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

            // 加載自訂按鈕
            try
            {
                var customButtons = await LoadCustomButtonsAsync(ItemId);
                if (customButtons.Count > 0)
                    CustomButtonsHtml = BuildCustomButtonsHtml(customButtons);
            }
            catch (System.Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load custom buttons for {ItemId}", ItemId);
            }

            ViewData["DictTableName"] = Config.MasterDict ?? Config.MasterTable;
            return Page();
        }

        private async Task<List<CustomButtonRow>> LoadCustomButtonsAsync(string itemId)
        {
            var list = new List<CustomButtonRow>();
            if (string.IsNullOrWhiteSpace(itemId)) return list;

            var connStr = _configuration.GetConnectionString("DefaultConnection");
            await using var conn = new SqlConnection(connStr);
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
            cmd.Parameters.AddWithValue("@itemId", itemId);

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

                sb.Append("<button type='button' class='mmd-custom-btn' data-custom-btn='1'");
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

        private static int? TryToInt(object? val)
        {
            if (val == null || val == System.DBNull.Value) return null;
            if (val is int i) return i;
            if (int.TryParse(val.ToString(), out var r)) return r;
            return null;
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
}
