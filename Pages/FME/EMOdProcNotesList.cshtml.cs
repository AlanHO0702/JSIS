using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;

namespace PcbErpApi.Pages.FME;

/// <summary>
/// 製程常用備註表 PageModel
/// Table 設定與欄位辭典皆動態讀取自 CURdOCXTableSetUp / CURdTableName / CURdTableField
/// </summary>
public class EMOdProcNotesListModel : PageModel
{
    private const string SysItemId = "EMO00042";

    private readonly PcbErpContext _ctx;
    private readonly ILogger<EMOdProcNotesListModel> _logger;

    public EMOdProcNotesListModel(PcbErpContext ctx, ILogger<EMOdProcNotesListModel> logger)
    {
        _ctx = ctx;
        _logger = logger;
    }

    public string UserId { get; set; } = "admin";

    /// <summary>主表卡片標題 (CURdTableName.DisplayLabel)</summary>
    public string MasterTitle { get; set; } = "製程清單";

    /// <summary>明細表卡片標題 (CURdTableName.DisplayLabel)</summary>
    public string DetailTitle { get; set; } = "常用備注";

    /// <summary>主表實際查詢資料表 (CURdTableName.RealTableName)</summary>
    public string MasterTable { get; set; } = "EMOdProcInfo";

    /// <summary>明細表實際查詢資料表 (CURdTableName.RealTableName 或 TableName)</summary>
    public string DetailTable { get; set; } = "EMOdProcNotesList";

    /// <summary>主表欄位辭典 FieldName → DisplayLabel</summary>
    public Dictionary<string, string> MasterFields { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>明細表欄位辭典 FieldName → DisplayLabel</summary>
    public Dictionary<string, string> DetailFields { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public async Task OnGetAsync()
    {
        UserId = HttpContext.Session.GetString("UserId") ?? "admin";

        var cs = _ctx.Database.GetConnectionString();
        if (string.IsNullOrWhiteSpace(cs)) return;

        await using var conn = new SqlConnection(cs);
        await conn.OpenAsync();

        await LoadTableConfigAsync(conn);
    }

    private async Task LoadTableConfigAsync(SqlConnection conn)
    {
        // ① 讀 CURdOCXTableSetUp → 取得 MASTER1 / DETAIL1 的辭典表名
        string masterDictTable = "EMOdProcInfoShort";
        string detailDictTable = "EMOdProcNotesList";

        try
        {
            const string sqlSetup = @"
                SELECT TableKind, TableName
                FROM CURdOCXTableSetUp WITH (NOLOCK)
                WHERE ItemId = @itemId
                  AND TableKind IN ('MASTER1','DETAIL1')";

            await using var cmd = new SqlCommand(sqlSetup, conn);
            cmd.Parameters.AddWithValue("@itemId", SysItemId);
            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                var kind = rd["TableKind"]?.ToString() ?? "";
                var tbl  = rd["TableName"]?.ToString() ?? "";
                if (string.IsNullOrWhiteSpace(tbl)) continue;
                if (kind.Equals("MASTER1", StringComparison.OrdinalIgnoreCase)) masterDictTable = tbl;
                else if (kind.Equals("DETAIL1", StringComparison.OrdinalIgnoreCase)) detailDictTable = tbl;
            }
        }
        catch (Exception ex) { _logger.LogError(ex, "LoadTableConfig step1 failed"); }

        // ② 讀 CURdTableName → 取得顯示名稱 & 實際資料表
        try
        {
            const string sqlMeta = @"
                SELECT TableName,
                       ISNULL(NULLIF(DisplayLabel,''), TableName)   AS DisplayLabel,
                       ISNULL(NULLIF(RealTableName,''), TableName)  AS RealTableName
                FROM CURdTableName WITH (NOLOCK)
                WHERE TableName IN (@master, @detail)";

            await using var cmd = new SqlCommand(sqlMeta, conn);
            cmd.Parameters.AddWithValue("@master", masterDictTable);
            cmd.Parameters.AddWithValue("@detail", detailDictTable);
            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                var tbl   = rd["TableName"]?.ToString() ?? "";
                var label = rd["DisplayLabel"]?.ToString() ?? "";
                var real  = rd["RealTableName"]?.ToString() ?? "";
                if (tbl.Equals(masterDictTable, StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.IsNullOrWhiteSpace(label)) MasterTitle = label;
                    if (!string.IsNullOrWhiteSpace(real))  MasterTable = real;
                }
                else if (tbl.Equals(detailDictTable, StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.IsNullOrWhiteSpace(label)) DetailTitle = label;
                    if (!string.IsNullOrWhiteSpace(real))  DetailTable = real;
                }
            }
        }
        catch (Exception ex) { _logger.LogError(ex, "LoadTableConfig step2 failed"); }

        // ③ 讀 CURdTableField → 欄位辭典（兩張表一次查回）
        try
        {
            const string sqlFields = @"
                SELECT TableName, FieldName, DisplayLabel
                FROM CURdTableField WITH (NOLOCK)
                WHERE TableName IN (@master, @detail)
                  AND DisplayLabel IS NOT NULL
                  AND DisplayLabel <> ''";

            await using var cmd = new SqlCommand(sqlFields, conn);
            cmd.Parameters.AddWithValue("@master", masterDictTable);
            cmd.Parameters.AddWithValue("@detail", detailDictTable);
            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                var tbl   = rd["TableName"]?.ToString() ?? "";
                var field = rd["FieldName"]?.ToString() ?? "";
                var label = rd["DisplayLabel"]?.ToString() ?? "";
                if (string.IsNullOrWhiteSpace(field) || string.IsNullOrWhiteSpace(label)) continue;

                if (tbl.Equals(masterDictTable, StringComparison.OrdinalIgnoreCase))
                    MasterFields[field] = label;
                else if (tbl.Equals(detailDictTable, StringComparison.OrdinalIgnoreCase))
                    DetailFields[field] = label;
            }
        }
        catch (Exception ex) { _logger.LogError(ex, "LoadTableConfig step3 failed"); }
    }

    /// <summary>
    /// 取得欄位的辭典顯示名稱，找不到時回傳 fallback。
    /// </summary>
    public string Field(Dictionary<string, string> dict, string fieldName, string fallback)
        => dict.TryGetValue(fieldName, out var label) && !string.IsNullOrWhiteSpace(label)
            ? label
            : fallback;
}
