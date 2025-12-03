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
using WebRazor.Models;

namespace PcbErpApi.Pages.CUR
{
    public class MasterDetailModel : PageModel
    {
        private readonly PcbErpContext _ctx;
        private readonly ILogger<MasterDetailModel> _logger;

        public MasterDetailModel(PcbErpContext ctx, ILogger<MasterDetailModel> logger)
        {
            _ctx = ctx;
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
                CustomButtons = await LoadCustomButtonsAsync(ItemId);
                if (CustomButtons.Count > 0)
                    CustomButtonsHtml = BuildCustomButtonsHtml(CustomButtons);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Load custom buttons failed for {ItemId}", ItemId);
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

        private async Task<(bool hasCaptionE, bool hasHintE, string chkCol)> DetectButtonSchemaAsync(SqlConnection conn)
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
            var chkCol = cols.Contains("ChkCanUpdate") ? "ChkCanUpdate"
                       : cols.Contains("ChkCanbUpdate") ? "ChkCanbUpdate"
                       : "ChkCanUpdate";
            return (hasCapE, hasHintE, chkCol);
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

                sb.Append("<button type='button' class='btn btn-outline-secondary btn-sm' data-custom-btn='1'");
                sb.Append(" data-button-name='").Append(System.Net.WebUtility.HtmlEncode(b.ButtonName)).Append('\'');
                sb.Append(" data-item-id='").Append(System.Net.WebUtility.HtmlEncode(b.ItemId ?? string.Empty)).Append('\'');
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
            public int? bVisible { get; set; }
            public int? ChkCanUpdate { get; set; }
            public int? bNeedNum { get; set; }
            public int? DesignType { get; set; }
        }
    }
}
