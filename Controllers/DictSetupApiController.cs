using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class DictSetupApiController : ControllerBase
{
    private readonly string _connStr;
    public DictSetupApiController(IConfiguration config)
    {
        _connStr = config.GetConnectionString("DefaultConnection") 
            ?? throw new ArgumentNullException("DefaultConnection string is missing in configuration.");
    }

    // ===== 共用 =====
    private static int? TryToInt(object o)
    {
        if (o == null || o is DBNull) return null;
        return int.TryParse(o.ToString(), out var n) ? n : null;
    }

    // ===== A. 表格設定（CURdOCXTableSetUp）=====
    public class DictSetupRow
    {
        public string TableName { get; set; }
        public string TableKind { get; set; }
        public string MDKey { get; set; }
        public string LocateKeys { get; set; }
        public string OrderByField { get; set; }
        public string FilterSQL { get; set; }
        public string RunSQLAfterAdd { get; set; }
    }

    // GET /api/DictSetupApi/Table/ByItem/{itemId}
    [HttpGet("Table/ByItem/{itemId}")]
    public async Task<IActionResult> GetByItem(string itemId)
    {
        var list = new List<DictSetupRow>();
        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        var cmd = new SqlCommand(@"
            SELECT TableName, TableKind, MDKey, LocateKeys, OrderByField, FilterSQL, RunSQLAfterAdd
              FROM CURdOCXTableSetUp WITH (NOLOCK)
             WHERE ItemId = @itemId
             ORDER BY TableKind, TableName;", conn);

        cmd.Parameters.AddWithValue("@itemId", (object?)itemId ?? string.Empty);

        using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            list.Add(new DictSetupRow
            {
                TableName      = rd["TableName"]?.ToString() ?? string.Empty,
                TableKind      = rd["TableKind"]?.ToString() ?? string.Empty,
                MDKey          = rd["MDKey"]?.ToString()!,
                LocateKeys     = rd["LocateKeys"]?.ToString() ?? string.Empty,
                OrderByField   = rd["OrderByField"]?.ToString() ?? string.Empty,
                FilterSQL      = rd["FilterSQL"] == DBNull.Value ? null : rd["FilterSQL"]?.ToString(),
                RunSQLAfterAdd = rd["RunSQLAfterAdd"]?.ToString() ?? string.Empty
            });
        }

        return Ok(list);
    }

    public class DocxSetupUpdateInput
    {
        public string ItemId { get; set; }        // key
        public string TableName { get; set; }     // key
        public int? IsUpdateMoney { get; set; }   // 停用，但 DB 不允許 NULL -> 預設 0
        public string TableKind { get; set; }
        public int? FixColCount { get; set; }
        public string MDKey { get; set; }
        public string LocateKeys { get; set; }
        public string OrderByField { get; set; }
        public string FilterSQL { get; set; }
        public string RunSQLAfterAdd { get; set; }
    }

    // POST /api/DictSetupApi/Table/UpdateByItem
    [HttpPost("Table/UpdateByItem")]
    public async Task<IActionResult> UpdateByItem([FromBody] List<DocxSetupUpdateInput> list)
    {
        if (list == null || list.Count == 0)
            return BadRequest(new { success = false, message = "no payload" });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        foreach (var x in list)
        {
            var cmd = new SqlCommand(@"
UPDATE CURdOCXTableSetUp
   SET IsUpdateMoney  = @IsUpdateMoney,
       TableKind      = @TableKind,
       FixColCount    = @FixColCount,
       MDKey          = @MDKey,
       LocateKeys     = @LocateKeys,
       OrderByField   = @OrderByField,
       FilterSQL      = @FilterSQL,
       RunSQLAfterAdd = @RunSQLAfterAdd
 WHERE ItemId = @ItemId AND TableName = @TableName;", conn);

            cmd.Parameters.AddWithValue("@ItemId", x.ItemId ?? "");
            cmd.Parameters.AddWithValue("@TableName", x.TableName ?? "");
            cmd.Parameters.AddWithValue("@IsUpdateMoney", (object?)x.IsUpdateMoney ?? 0);
            cmd.Parameters.AddWithValue("@TableKind", (object?)x.TableKind ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@FixColCount", (object?)x.FixColCount ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MDKey", (object?)x.MDKey ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@LocateKeys", (object?)x.LocateKeys ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@OrderByField", (object?)x.OrderByField ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@FilterSQL", (object?)x.FilterSQL ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@RunSQLAfterAdd", (object?)x.RunSQLAfterAdd ?? DBNull.Value);

            await cmd.ExecuteNonQueryAsync();
        }

        return Ok(new { success = true, count = list.Count });
    }

    // ===== B. 自訂按鈕（CURdOCXItemCustButton）=====
    public class CustButtonRow
    {
        public string ButtonName { get; set; }
        public string CustCaption { get; set; }
        public string CustHint { get; set; }
        public string OCXName { get; set; }
        public string CoClassName { get; set; }
        public int? ChkCanbUpdate { get; set; }
        public int? ChkStatus { get; set; }
        public int? bVisible { get; set; }
        public int? bNeedNum { get; set; }
        public int? bNeedInEdit { get; set; }
        public int? DesignType { get; set; }
        public string SpName { get; set; }
        public int? bSpHasResult { get; set; }
        public string SearchTemplate { get; set; }
        public string MultiSelectDD { get; set; }
        public string ExecSpName { get; set; }
        public int? AllowSelCount { get; set; }
        public int? ReplaceExists { get; set; }
        public string DialogCaption { get; set; }
        public int? SerialNum { get; set; }
    }

    // GET /api/DictSetupApi/Button/ByItem/{itemId}
    [HttpGet("Button/ByItem/{itemId}")]
    public async Task<IActionResult> GetButtonsByItem(string itemId)
    {
        var list = new List<CustButtonRow>();
        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        var cmd = new SqlCommand(@"
        SELECT ButtonName, CustCaption, CustHint, OCXName, CoClassName,
               ChkCanbUpdate, ChkStatus, bVisible, bNeedNum, bNeedInEdit,
               DesignType, SpName, bSpHasResult, SearchTemplate, MultiSelectDD,
               ExecSpName, AllowSelCount, ReplaceExists, DialogCaption, SerialNum
          FROM CURdOCXItemCustButton WITH (NOLOCK)
         WHERE ItemId = @itemId
         ORDER BY SerialNum, ButtonName;", conn);

        cmd.Parameters.AddWithValue("@itemId", (object?)itemId ?? string.Empty);

        using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            list.Add(new CustButtonRow
            {
                ButtonName     = rd["ButtonName"]?.ToString() ?? string.Empty,
                CustCaption    = rd["CustCaption"]?.ToString() ?? string.Empty,
                CustHint       = rd["CustHint"]?.ToString() ?? string.Empty,
                OCXName        = rd["OCXName"]?.ToString() ?? string.Empty,
                CoClassName    = rd["CoClassName"]?.ToString() ?? string.Empty,
                ChkCanbUpdate  = TryToInt(rd["ChkCanbUpdate"]),
                ChkStatus      = TryToInt(rd["ChkStatus"]),
                bVisible       = TryToInt(rd["bVisible"]),
                bNeedNum       = TryToInt(rd["bNeedNum"]),
                bNeedInEdit    = TryToInt(rd["bNeedInEdit"]),
                DesignType     = TryToInt(rd["DesignType"]),
                SpName         = rd["SpName"]?.ToString() ?? string.Empty,
                bSpHasResult   = TryToInt(rd["bSpHasResult"]),
                SearchTemplate = rd["SearchTemplate"]?.ToString() ?? string.Empty,
                MultiSelectDD  = rd["MultiSelectDD"]?.ToString() ?? string.Empty,
                ExecSpName     = rd["ExecSpName"]?.ToString() ?? string.Empty,
                AllowSelCount  = TryToInt(rd["AllowSelCount"]),
                ReplaceExists  = TryToInt(rd["ReplaceExists"]),
                DialogCaption  = rd["DialogCaption"]?.ToString() ?? string.Empty,
                SerialNum      = TryToInt(rd["SerialNum"])
            });
        }

        return Ok(list);
    }

    // ===== C. 報表設定（CURdPaperPaper）=====
    public class PaperRow
    {
        public string PaperId { get; set; }
        public int? SerialNum { get; set; }
        public string ItemName { get; set; }
        public int? Enabled { get; set; }
        public string Notes { get; set; }
        public string ClassName { get; set; }
        public string ObjectName { get; set; }
        public int? LinkType { get; set; }
        public int? DisplayType { get; set; }
        public int? OutputType { get; set; }
        public int? ShowTitle { get; set; }
        public int? ShowTree { get; set; }
        public int? TableIndex { get; set; }
        public int? ItemCount { get; set; }
        public string PrintItemId { get; set; }
    }

    // GET /api/DictSetupApi/Report/{paperId}
    [HttpGet("Report/{paperId}")]
    public async Task<IActionResult> GetByPaperId(string paperId)
    {
        var list = new List<PaperRow>();
        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        var cmd = new SqlCommand(@"
            SELECT PaperId, SerialNum, ItemName, Enabled, Notes,
                   ClassName, ObjectName,
                   LinkType, DisplayType, OutputType,
                   ShowTitle, ShowTree, TableIndex, ItemCount, PrintItemId
              FROM CURdPaperPaper WITH (NOLOCK)
             WHERE PaperId = @paperId
             ORDER BY SerialNum;", conn);

        cmd.Parameters.AddWithValue("@paperId", paperId ?? string.Empty);

        using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            list.Add(new PaperRow
            {
                PaperId     = rd["PaperId"]?.ToString(),
                SerialNum   = TryToInt(rd["SerialNum"]),
                ItemName    = rd["ItemName"]?.ToString(),
                Enabled     = TryToInt(rd["Enabled"]),
                Notes       = rd["Notes"]?.ToString(),
                ClassName   = rd["ClassName"]?.ToString(),
                ObjectName  = rd["ObjectName"]?.ToString(),
                LinkType    = TryToInt(rd["LinkType"]),
                DisplayType = TryToInt(rd["DisplayType"]),
                OutputType  = TryToInt(rd["OutputType"]),
                ShowTitle   = TryToInt(rd["ShowTitle"]),
                ShowTree    = TryToInt(rd["ShowTree"]),
                TableIndex  = TryToInt(rd["TableIndex"]),
                ItemCount   = TryToInt(rd["ItemCount"]),
                PrintItemId = rd["PrintItemId"]?.ToString()
            });
        }
        return Ok(list);
    }

    public class PaperRowUpdateInput : PaperRow { }

    // POST /api/DictSetupApi/Report/UpdateRows
    [HttpPost("Report/UpdateRows")]
    public async Task<IActionResult> UpdateReportRows([FromBody] List<PaperRowUpdateInput> list)
    {
        if (list == null || list.Count == 0)
            return BadRequest(new { success = false, message = "no payload" });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        foreach (var x in list)
        {
            var cmd = new SqlCommand(@"
UPDATE CURdPaperPaper
   SET ItemName    = @ItemName,
       Enabled     = @Enabled,
       Notes       = @Notes,
       ClassName   = @ClassName,
        ObjectName  = @ObjectName,
       LinkType    = @LinkType,
       DisplayType = @DisplayType,
       OutputType  = @OutputType,
       ShowTitle   = @ShowTitle,
       ShowTree    = @ShowTree,
       TableIndex  = @TableIndex,
       ItemCount   = @ItemCount,
       PrintItemId = @PrintItemId
 WHERE PaperId = @PaperId AND SerialNum = @SerialNum;", conn);

            cmd.Parameters.AddWithValue("@PaperId", x.PaperId ?? "");
            cmd.Parameters.AddWithValue("@SerialNum", (object?)x.SerialNum ?? 0);
            cmd.Parameters.AddWithValue("@ItemName", (object?)x.ItemName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Enabled", (object?)x.Enabled ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Notes", (object?)x.Notes ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ClassName", (object?)x.ClassName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ObjectName", (object?)x.ObjectName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@LinkType", (object?)x.LinkType ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DisplayType", (object?)x.DisplayType ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@OutputType", (object?)x.OutputType ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ShowTitle", (object?)x.ShowTitle ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ShowTree", (object?)x.ShowTree ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@TableIndex", (object?)x.TableIndex ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ItemCount", (object?)x.ItemCount ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PrintItemId", (object?)x.PrintItemId ?? DBNull.Value);

            await cmd.ExecuteNonQueryAsync();
        }

        return Ok(new { success = true, count = list.Count });
    }
}
