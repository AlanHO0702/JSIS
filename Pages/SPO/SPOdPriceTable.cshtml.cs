using System.Data;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;
using WebRazor.Models;

namespace PcbErpApi.Pages.SPO
{
    public class SPO00013Model : PageModel
    {
        private readonly PcbErpContext _ctx;
        private readonly ILogger<SPO00013Model> _logger;

        public SPO00013Model(PcbErpContext ctx, ILogger<SPO00013Model> logger)
        {
            _ctx = ctx;
            _logger = logger;
        }

        public string ItemId { get; private set; } = "SPO00013";
        public string? ItemName { get; private set; }
        public MasterDetailConfig? Config { get; private set; }
        public string? LoadError { get; private set; }

        public List<CURdTableField> FieldDictList { get; private set; } = new();
        public List<CustomButtonRow> CustomButtons { get; private set; } = new();
        public HtmlString CustomButtonsHtml { get; private set; } = HtmlString.Empty;

        public async Task<IActionResult> OnGetAsync()
        {
            var loadResult = await BuildConfigAsync();
            if (loadResult.Config == null)
            {
                LoadError = loadResult.Error;
                ViewData["Title"] = ItemId;
                return Page();
            }

            ItemName = loadResult.ItemName;
            Config = loadResult.Config;
            LoadError = loadResult.Error;

            ViewData["Title"] = string.IsNullOrWhiteSpace(ItemName) ? ItemId : $"{ItemId} {ItemName}";
            var dictName = Config?.MasterDict ?? Config?.MasterTable ?? string.Empty;
            ViewData["DictTableName"] = dictName;

            if (!string.IsNullOrWhiteSpace(dictName))
            {
                FieldDictList = await _ctx.CURdTableFields
                    .AsNoTracking()
                    .Where(x => x.TableName == dictName)
                    .OrderBy(x => x.SerialNum)
                    .ToListAsync();
            }

            try
            {
                CustomButtons = await LoadCustomButtonsAsync(ItemId);
                if (CustomButtons.Count > 0)
                    CustomButtonsHtml = BuildCustomButtonsHtml(CustomButtons);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Load custom buttons failed for {ItemId}", ItemId);
            }

            return Page();
        }

        private async Task<MasterDetailLoadResult> BuildConfigAsync()
        {
            var result = new MasterDetailLoadResult { ItemId = ItemId };

            const string masterTableName = "AJNdCustomer";
            const string detailTableName = "SPOdPriceTable";
            const string mdKeyRaw = "CompanyId";
            const string locateKeysRaw = "CompanyId;PartNum;UOM;Qnty";
            const string masterOrderBy = "CompanyId";
            const string detailOrderBy = "CompanyId,PartNum,UOM,Qnty,QuoteDate desc";

            try
            {
                var item = await _ctx.CurdSysItems.AsNoTracking()
                    .SingleOrDefaultAsync(x => x.ItemId == ItemId);

                if (item is null)
                {
                    result.Error = $"Item {ItemId} not found.";
                    result.IsNotFound = true;
                    return result;
                }

                var connStr = GetConnStr(_ctx);
                var masterMeta = await GetTableMetaAsync(connStr, masterTableName);
                var detailMeta = await GetTableMetaAsync(connStr, detailTableName);

                var cfg = new MasterDetailConfig
                {
                    DomId = "md_spo00013",
                    MasterTitle = masterMeta?.DisplayLabel ?? masterTableName,
                    DetailTitle = detailMeta?.DisplayLabel ?? detailTableName,
                    MasterTable = masterMeta?.RealTableName ?? masterTableName,
                    DetailTable = detailMeta?.RealTableName ?? detailTableName,
                    MasterDict = masterTableName,
                    DetailDict = detailTableName,
                    KeyMap = BuildKeyMap(mdKeyRaw),
                    DetailKeyFields = Split(locateKeysRaw),
                    MasterTop = 500
                };

                cfg.MasterOrderBy = string.IsNullOrWhiteSpace(masterOrderBy)
                    ? await GetDefaultOrderByAsync(connStr, cfg.MasterTable)
                    : NormalizeOrderBy(masterOrderBy);

                cfg.DetailOrderBy = string.IsNullOrWhiteSpace(detailOrderBy)
                    ? await GetDefaultOrderByAsync(connStr, cfg.DetailTable)
                    : NormalizeOrderBy(detailOrderBy);

                cfg.DetailApi = string.Empty; // 使用預設 ByKeys 並帶入 KeyMap

                result.Config = cfg;
                result.ItemName = item.ItemName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Load config failed for {ItemId}", ItemId);
                result.Error = ex.Message;
            }

            return result;
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

        private static HtmlString BuildCustomButtonsHtml(IEnumerable<CustomButtonRow> rows)
        {
            if (rows == null) return HtmlString.Empty;
            var sb = new System.Text.StringBuilder();

            foreach (var b in rows)
            {
                if (!b.bVisible.HasValue || b.bVisible.Value != 1) continue;
                if (string.IsNullOrWhiteSpace(b.ButtonName)) continue;

                var caption = string.IsNullOrWhiteSpace(b.CustCaption) ? b.ButtonName : b.CustCaption;
                var hint = b.CustHint ?? string.Empty;

                sb.Append("<button type='button' class='btn btn-outline-secondary btn-sm me-1' data-custom-btn='1'");
                sb.Append(" data-button-name='").Append(System.Net.WebUtility.HtmlEncode(b.ButtonName)).Append('\'');
                sb.Append(" data-item-id='").Append(System.Net.WebUtility.HtmlEncode(b.ItemId ?? string.Empty)).Append('\'');
                sb.Append(" title='").Append(System.Net.WebUtility.HtmlEncode(hint)).Append("'>");
                sb.Append("<i class='bi bi-gear me-1'></i>").Append(System.Net.WebUtility.HtmlEncode(caption));
                sb.Append("</button>");
            }

            return new HtmlString(sb.ToString());
        }

        private static int? TryToInt(object? o)
        {
            if (o == null || o == DBNull.Value) return null;
            return int.TryParse(o.ToString(), out var n) ? n : null;
        }

        private static string GetConnStr(PcbErpContext ctx)
        {
            var cs = ctx.Database.GetConnectionString();
            if (string.IsNullOrWhiteSpace(cs))
                throw new InvalidOperationException("Connection string is not configured.");
            return cs;
        }

        private static string? NormalizeOrderBy(string? raw)
        {
            var s = (raw ?? string.Empty)
                .Replace('*', ' ')
                .Replace('+', ' ')
                .Trim();
            if (string.IsNullOrWhiteSpace(s)) return null;
            while (s.Contains("  "))
                s = s.Replace("  ", " ");
            return s;
        }

        private static async Task<TableMeta?> GetTableMetaAsync(string connStr, string dictTableName)
        {
            await using var conn = new SqlConnection(connStr);
            await conn.OpenAsync();

            const string sql = @"
SELECT TOP 1 
       ISNULL(NULLIF(RealTableName,''), TableName) AS RealTableName,
       ISNULL(NULLIF(DisplayLabel,''), TableName) AS DisplayLabel
  FROM CURdTableName WITH (NOLOCK)
 WHERE TableName = @tbl";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@tbl", dictTableName ?? string.Empty);

            await using var rd = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow);
            if (!await rd.ReadAsync()) return null;

            return new TableMeta
            {
                RealTableName = rd["RealTableName"] == DBNull.Value ? null : rd["RealTableName"]?.ToString(),
                DisplayLabel = rd["DisplayLabel"] == DBNull.Value ? null : rd["DisplayLabel"]?.ToString()
            };
        }

        private static async Task<string> GetDefaultOrderByAsync(string connStr, string tableName)
        {
            await using var conn = new SqlConnection(connStr);
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

        private static KeyMap[] BuildKeyMap(string? mdKey)
        {
            var keys = Split(mdKey);
            if (keys.Length == 0) return Array.Empty<KeyMap>();
            return keys.Select(k =>
            {
                var parts = k.Split(new[] { ':', '=' }, StringSplitOptions.RemoveEmptyEntries);
                var master = parts.Length > 0 ? parts[0].Trim() : k;
                var detail = parts.Length > 1 ? parts[1].Trim() : master;
                master = string.IsNullOrWhiteSpace(master) ? k : master;
                detail = string.IsNullOrWhiteSpace(detail) ? master : detail;
                return new KeyMap(master, detail);
            }).ToArray();
        }

        private static string[] Split(string? raw)
        {
            return (raw ?? string.Empty)
                .Split(new[] { ';', ',', '|' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToArray();
        }

        private sealed class TableMeta
        {
            public string? RealTableName { get; set; }
            public string? DisplayLabel { get; set; }
        }

        private sealed class MasterDetailLoadResult
        {
            public string ItemId { get; set; } = string.Empty;
            public string? ItemName { get; set; }
            public MasterDetailConfig? Config { get; set; }
            public string? Error { get; set; }
            public bool IsNotFound { get; set; }
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
