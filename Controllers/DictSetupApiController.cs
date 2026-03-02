using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class DictSetupApiController : ControllerBase
{
    private readonly string _connStr;
    private readonly PcbErpContext _ctx;
    public DictSetupApiController(IConfiguration config, PcbErpContext ctx)
    {
        _connStr = config.GetConnectionString("DefaultConnection") 
            ?? throw new ArgumentNullException("DefaultConnection string is missing in configuration.");
        _ctx = ctx;
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
        public string? FilterSQL { get; set; }
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
             ORDER BY
                CASE
                    WHEN TableKind LIKE 'Master%' THEN 1
                    WHEN TableKind LIKE 'Detail%' THEN 2
                    WHEN TableKind LIKE 'SubDetail%' THEN 3
                    ELSE 4
                END,
                TableKind,
                TableName;", conn);

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

    // ===== A-2. TableName 詳細資訊（CURdTableName）=====
    public class TableNameRow
    {
        public string TableName { get; set; }
        public string DisplayLabel { get; set; }
        public string TableNote { get; set; }
        public int? SerialNum { get; set; }
        public int? TableType { get; set; }
        public int? LevelNo { get; set; }
        public string SystemId { get; set; }
        public string SuperId { get; set; }
        public string RealTableName { get; set; }
        public string OrderByField { get; set; }
        public string DisplayLabelCn { get; set; }
        public string DisplayLabelEn { get; set; }
        public string DisplayLabelJp { get; set; }
        public string DisplayLabelTh { get; set; }
        public string LogKeildFieldName { get; set; }
    }

    // GET /api/DictSetupApi/TableName/{tableName}
    [HttpGet("TableName/{tableName}")]
    public async Task<IActionResult> GetTableName(string tableName)
    {
        if (string.IsNullOrWhiteSpace(tableName))
            return BadRequest(new { success = false, message = "tableName is required" });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        var cmd = new SqlCommand(@"
            SELECT TableName, DisplayLabel, TableNote, SerialNum, TableType, LevelNo,
                   SystemId, SuperId, RealTableName, OrderByField,
                   DisplayLabelCn, DisplayLabelEn, DisplayLabelJp, DisplayLabelTh, LogKeildFieldName
              FROM CURdTableName WITH (NOLOCK)
             WHERE TableName = @tableName;", conn);

        cmd.Parameters.AddWithValue("@tableName", tableName);

        using var rd = await cmd.ExecuteReaderAsync();
        if (await rd.ReadAsync())
        {
            return Ok(new TableNameRow
            {
                TableName = rd["TableName"]?.ToString() ?? string.Empty,
                DisplayLabel = rd["DisplayLabel"]?.ToString() ?? string.Empty,
                TableNote = rd["TableNote"]?.ToString() ?? string.Empty,
                SerialNum = TryToInt(rd["SerialNum"]),
                TableType = TryToInt(rd["TableType"]),
                LevelNo = TryToInt(rd["LevelNo"]),
                SystemId = rd["SystemId"]?.ToString() ?? string.Empty,
                SuperId = rd["SuperId"]?.ToString() ?? string.Empty,
                RealTableName = rd["RealTableName"]?.ToString() ?? string.Empty,
                OrderByField = rd["OrderByField"]?.ToString() ?? string.Empty,
                DisplayLabelCn = rd["DisplayLabelCn"]?.ToString() ?? string.Empty,
                DisplayLabelEn = rd["DisplayLabelEn"]?.ToString() ?? string.Empty,
                DisplayLabelJp = rd["DisplayLabelJp"]?.ToString() ?? string.Empty,
                DisplayLabelTh = rd["DisplayLabelTh"]?.ToString() ?? string.Empty,
                LogKeildFieldName = rd["LogKeildFieldName"]?.ToString() ?? string.Empty
            });
        }

        return NotFound(new { success = false, message = "TableName not found" });
    }

    // POST /api/DictSetupApi/TableName/Update
    [HttpPost("TableName/Update")]
    public async Task<IActionResult> UpdateTableName([FromBody] TableNameRow input)
    {
        if (input == null || string.IsNullOrWhiteSpace(input.TableName))
            return BadRequest(new { success = false, message = "Invalid input" });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        var cmd = new SqlCommand(@"
UPDATE CURdTableName
   SET DisplayLabel = @DisplayLabel,
       TableNote = @TableNote,
       SerialNum = @SerialNum,
       TableType = @TableType,
       LevelNo = @LevelNo,
       SystemId = @SystemId,
       SuperId = @SuperId,
       RealTableName = @RealTableName,
       OrderByField = @OrderByField,
       DisplayLabelCn = @DisplayLabelCn,
       DisplayLabelEn = @DisplayLabelEn,
       DisplayLabelJp = @DisplayLabelJp,
       DisplayLabelTh = @DisplayLabelTh,
       LogKeildFieldName = @LogKeildFieldName
 WHERE TableName = @TableName;", conn);

        cmd.Parameters.AddWithValue("@TableName", input.TableName);
        cmd.Parameters.AddWithValue("@DisplayLabel", (object?)input.DisplayLabel ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@TableNote", (object?)input.TableNote ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@SerialNum", (object?)input.SerialNum ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@TableType", (object?)input.TableType ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@LevelNo", (object?)input.LevelNo ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@SystemId", (object?)input.SystemId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@SuperId", (object?)input.SuperId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@RealTableName", (object?)input.RealTableName ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@OrderByField", (object?)input.OrderByField ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@DisplayLabelCn", (object?)input.DisplayLabelCn ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@DisplayLabelEn", (object?)input.DisplayLabelEn ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@DisplayLabelJp", (object?)input.DisplayLabelJp ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@DisplayLabelTh", (object?)input.DisplayLabelTh ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@LogKeildFieldName", (object?)input.LogKeildFieldName ?? DBNull.Value);

        var affected = await cmd.ExecuteNonQueryAsync();

        return Ok(new { success = affected > 0, affected });
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

    // ===== D. �欰�p�ɿ�ܿ��j�p (CURdTableField.DisplaySize) =====
    public class FieldWidthInput
    {
        public string TableName { get; set; } = "";
        public string FieldName { get; set; } = "";
        public int WidthPx { get; set; }
        public string LanguageId { get; set; } = "TW";
    }

    // POST /api/DictSetupApi/FieldWidth/Save
    [HttpPost("FieldWidth/Save")]
    public async Task<IActionResult> SaveFieldWidth([FromBody] FieldWidthInput input)
    {
        if (string.IsNullOrWhiteSpace(input.TableName) || string.IsNullOrWhiteSpace(input.FieldName))
            return BadRequest("TableName and FieldName are required.");

        var displaySize = Math.Max(1, (int)Math.Round(input.WidthPx / 10.0, MidpointRounding.AwayFromZero));

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();
        var cmd = new SqlCommand(@"
UPDATE CURdTableField
   SET DisplaySize = @DisplaySize
 WHERE TableName = @TableName AND FieldName = @FieldName;", conn);
        cmd.Parameters.AddWithValue("@DisplaySize", displaySize);
        cmd.Parameters.AddWithValue("@TableName", input.TableName);
        cmd.Parameters.AddWithValue("@FieldName", input.FieldName);

        var affected = await cmd.ExecuteNonQueryAsync();
        // �t�ⶰ�y�ШѼ�: �w�] TW, �|�Ϊ̥�J
        var langCmd = new SqlCommand(@"
UPDATE CURdTableFieldLang
   SET DisplaySize = @DisplaySize
 WHERE TableName = @TableName AND FieldName = @FieldName AND LanguageId = @LangId;", conn);
        langCmd.Parameters.AddWithValue("@DisplaySize", displaySize);
        langCmd.Parameters.AddWithValue("@TableName", input.TableName);
        langCmd.Parameters.AddWithValue("@FieldName", input.FieldName);
        langCmd.Parameters.AddWithValue("@LangId", string.IsNullOrWhiteSpace(input.LanguageId) ? "TW" : input.LanguageId);
        var langAffected = await langCmd.ExecuteNonQueryAsync();

        return Ok(new { success = affected > 0 || langAffected > 0, affected, langAffected, displaySize });
    }

    // ===== C. 報表設定（CURdPaperPaper）=====
    public class PaperRow
    {
        public string? PaperId { get; set; }
        public int? SerialNum { get; set; }
        public string? ItemName { get; set; }
        public int? Enabled { get; set; }
        public string? Notes { get; set; }
        public string? ClassName { get; set; }
        public string? ObjectName { get; set; }
        public int? LinkType { get; set; }
        public int? DisplayType { get; set; }
        public int? OutputType { get; set; }
        public int? ShowTitle { get; set; }
        public int? ShowTree { get; set; }
        public int? TableIndex { get; set; }
        public int? ItemCount { get; set; }
        public string? PrintItemId { get; set; }
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

    // ===== E. ���d�ߪ��줸�]�w (CURdOCXPaperSelOtherGet) =====
    public class QueryFieldRow
    {
        public string ColumnName { get; set; } = "";
        public string ColumnCaption { get; set; } = "";
        public int? DataType { get; set; }
        public int? ControlType { get; set; }
        public string? DefaultValue { get; set; }
        public string? DefaultEqual { get; set; }
        public string? CommandText { get; set; }
        public int? IsCommonQuery { get; set; }
        public int? SortOrder { get; set; }
        public int? DefaultType { get; set; }
    }

    // GET /api/DictSetupApi/QueryFields?itemId=...&table=...&lang=TW
    [HttpGet("QueryFields")]
    public async Task<IActionResult> GetQueryFields([FromQuery] string itemId, [FromQuery] string table, [FromQuery] string lang = "TW")
    {
        if (string.IsNullOrWhiteSpace(itemId) || string.IsNullOrWhiteSpace(table))
            return BadRequest("itemId and table are required.");

        var list = new List<QueryFieldRow>();
        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        using var cmd = new SqlCommand("exec CURdOCXPaperSelOtherGet @p0,@p1,@p2", conn);
        cmd.CommandType = System.Data.CommandType.Text;
        cmd.Parameters.AddWithValue("@p0", itemId);
        cmd.Parameters.AddWithValue("@p1", table);
        cmd.Parameters.AddWithValue("@p2", string.IsNullOrWhiteSpace(lang) ? "TW" : lang);

        using (var rd = await cmd.ExecuteReaderAsync())
        {
            // 檢查欄位是否存在
            var hasIsCommonQuery = false;
            var hasSortOrder = false;
            var hasDefaultType = false;
            try
            {
                for (int i = 0; i < rd.FieldCount; i++)
                {
                    var fieldName = rd.GetName(i);
                    if (fieldName.Equals("IsCommonQuery", StringComparison.OrdinalIgnoreCase))
                        hasIsCommonQuery = true;
                    if (fieldName.Equals("SortOrder", StringComparison.OrdinalIgnoreCase))
                        hasSortOrder = true;
                    if (fieldName.Equals("DefaultType", StringComparison.OrdinalIgnoreCase))
                        hasDefaultType = true;
                }
            }
            catch { }

            while (await rd.ReadAsync())
            {
                // 只在欄位存在時才讀取
                int? isCommonQuery = null;
                int? sortOrder = null;
                int? defaultType = null;

                if (hasIsCommonQuery)
                {
                    try { isCommonQuery = TryToInt(rd["IsCommonQuery"]); } catch { }
                }

                if (hasSortOrder)
                {
                    try { sortOrder = TryToInt(rd["SortOrder"]); } catch { }
                }
                if (hasDefaultType)
                {
                    try { defaultType = TryToInt(rd["DefaultType"]); } catch { }
                }

                list.Add(new QueryFieldRow
                {
                    ColumnName = rd["ColumnName"]?.ToString() ?? "",
                    ColumnCaption = rd["ColumnCaption"]?.ToString() ?? rd["old_ColumnCaption"]?.ToString() ?? "",
                    DataType = TryToInt(rd["DataType"]),
                    ControlType = TryToInt(rd["ControlType"]),
                    DefaultValue = rd["DefaultValue"]?.ToString(),
                    DefaultEqual = rd["DefaultEqual"]?.ToString(),
                    CommandText = rd["CommandText"]?.ToString(),
                    IsCommonQuery = isCommonQuery,
                    SortOrder = sortOrder,
                    DefaultType = defaultType
                });
            }
        }

        foreach (var row in list)
        {
            row.DefaultValue = await ResolveDefaultValueAsync(conn, row.DefaultValue, row.DefaultType);
        }

        return Ok(list);
    }

    private static async Task<string?> ResolveDefaultValueAsync(SqlConnection conn, string? defaultValue, int? defaultType)
    {
        if (defaultType != 1 || string.IsNullOrWhiteSpace(defaultValue))
            return defaultValue;

        var sql = defaultValue.Trim();
        if (!sql.StartsWith("select", StringComparison.OrdinalIgnoreCase))
            return defaultValue;

        await using var cmd = new SqlCommand(sql, conn);
        cmd.CommandType = System.Data.CommandType.Text;
        var result = await cmd.ExecuteScalarAsync();
        return result == null || result == DBNull.Value ? "" : result.ToString();
    }

    public class InqMustRow
    {
        public string FieldMust { get; set; } = "";
        public string ShowError { get; set; } = "";
    }

    public class QueryEqualRow
    {
        public string Value { get; set; } = "";
        public string Text { get; set; } = "";
    }

    [HttpGet("QueryEquals")]
    public async Task<IActionResult> GetQueryEquals([FromQuery] int all = 0)
    {
        var list = new List<QueryEqualRow>();
        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        var view = all == 1 ? "CURdV_EqualALL" : "CURdV_Equal";
        var sql = $"SELECT sEqual FROM {view} WITH (NOLOCK) ORDER BY iOrderby";

        await using var cmd = new SqlCommand(sql, conn);
        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            var eq = rd["sEqual"]?.ToString() ?? "";
            if (string.IsNullOrWhiteSpace(eq)) continue;
            list.Add(new QueryEqualRow { Value = eq, Text = eq });
        }

        if (list.Count == 0)
        {
            list.AddRange(new[]
            {
                new QueryEqualRow { Value = "=", Text = "=" },
                new QueryEqualRow { Value = ">=", Text = ">=" },
                new QueryEqualRow { Value = "<=", Text = "<=" },
                new QueryEqualRow { Value = "like", Text = "like" }
            });
        }

        return Ok(list);
    }

    public class PaperSelectedRow
    {
        public string PaperId { get; set; } = "";
        public string TableName { get; set; } = "";
        public string? AliasName { get; set; }
        public string ColumnName { get; set; } = "";
        public string ColumnCaption { get; set; } = "";
        public int DataType { get; set; }
        public int SortOrder { get; set; }
        public string? DefaultValue { get; set; }
        public string DefaultEqual { get; set; } = "";
        public int? ControlType { get; set; }
        public string? CommandText { get; set; }
        public int? DefaultType { get; set; }
        public string? EditMask { get; set; }
        public string? SuperId { get; set; }
        public string? ParamValue { get; set; }
        public int? ParamType { get; set; }
        public int? IReadOnly { get; set; }
        public int? IVisible { get; set; }
        public string? TableKind { get; set; }
    }

    public class PaperSelectedPayload
    {
        public string ItemId { get; set; } = "";
        public string Table { get; set; } = "";
        public List<PaperSelectedRow> Items { get; set; } = new();
    }

    [HttpGet("PaperSelectedList")]
    public async Task<IActionResult> GetPaperSelected([FromQuery] string itemId, [FromQuery] string table, [FromQuery] string? equal = null, [FromQuery] string lang = "TW", [FromQuery] int resolveDefault = 0)
    {
        if (string.IsNullOrWhiteSpace(itemId) || string.IsNullOrWhiteSpace(table))
            return BadRequest("itemId and table are required.");

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        var dictTable = table.Trim();
        var realTable = await ResolveRealTableNameAsync(conn, dictTable) ?? dictTable;
        var paperId = await ResolvePaperIdAsync(conn, itemId, dictTable, realTable) ?? realTable;

        var selectedRows = await _ctx.CURdPaperSelected
            .AsNoTracking()
            .Where(x => x.PaperId == paperId)
            .OrderBy(x => x.SortOrder)
            .ToListAsync();

        if (selectedRows.Count == 0)
        {
            selectedRows = await _ctx.CURdPaperSelected
                .AsNoTracking()
                .Where(x => x.TableName == dictTable)
                .OrderBy(x => x.SortOrder)
                .ToListAsync();
        }

        var allFields = await LoadPaperDictSelectAsync(conn, itemId, equal, lang);

        var selected = selectedRows.Select(x => new PaperSelectedRow
        {
            PaperId = x.PaperId,
            TableName = x.TableName,
            AliasName = x.AliasName,
            ColumnName = x.ColumnName,
            ColumnCaption = x.ColumnCaption,
            DataType = x.DataType,
            SortOrder = x.SortOrder,
            DefaultValue = x.DefaultValue,
            DefaultEqual = x.DefaultEqual,
            ControlType = x.ControlType,
            CommandText = x.CommandText,
            DefaultType = x.DefaultType,
            EditMask = x.EditMask,
            SuperId = x.SuperId,
            ParamValue = x.ParamValue,
            ParamType = x.ParamType,
            IReadOnly = x.IReadOnly,
            IVisible = x.IVisible,
            TableKind = x.TableKind
        }).ToList();

        foreach (var row in selected)
        {
            if (row.DefaultType == 0 &&
                row.ColumnName.Equals("FinishQnty", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(row.DefaultValue, "0", StringComparison.OrdinalIgnoreCase))
            {
                row.DefaultValue = null;
            }
        }

        if (resolveDefault == 1)
        {
            foreach (var row in selected)
            {
                row.DefaultValue = await ResolveDefaultValueAsync(conn, row.DefaultValue, row.DefaultType);
            }
        }

        return Ok(new
        {
            paperId,
            dictTable,
            selected,
            allFields
        });
    }

    [HttpPost("PaperSelected")]
    public async Task<IActionResult> SavePaperSelected([FromBody] PaperSelectedPayload payload)
    {
        if (payload == null || string.IsNullOrWhiteSpace(payload.ItemId) || string.IsNullOrWhiteSpace(payload.Table))
            return BadRequest("itemId and table are required.");

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        var dictTable = payload.Table.Trim();
        var realTable = await ResolveRealTableNameAsync(conn, dictTable) ?? dictTable;
        var paperId = await ResolvePaperIdAsync(conn, payload.ItemId, dictTable, realTable) ?? realTable;

        await using var tx = await conn.BeginTransactionAsync();
        try
        {
            await using (var del = new SqlCommand("DELETE FROM CURdPaperSelected WHERE PaperId = @paperId", conn, (SqlTransaction)tx))
            {
                del.Parameters.AddWithValue("@paperId", paperId);
                await del.ExecuteNonQueryAsync();
            }

            var insertSql = @"
INSERT INTO CURdPaperSelected
    (PaperId, TableName, AliasName, ColumnName, ColumnCaption, DataType, SortOrder,
     DefaultValue, DefaultEqual, ControlType, CommandText, DefaultType, EditMask,
     SuperId, ParamValue, ParamType, iReadOnly, iVisible, TableKind)
VALUES
    (@PaperId, @TableName, @AliasName, @ColumnName, @ColumnCaption, @DataType, @SortOrder,
     @DefaultValue, @DefaultEqual, @ControlType, @CommandText, @DefaultType, @EditMask,
     @SuperId, @ParamValue, @ParamType, @IReadOnly, @IVisible, @TableKind);";

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var order = 1;
            foreach (var item in payload.Items ?? new())
            {
                if (string.IsNullOrWhiteSpace(item.ColumnName)) continue;
                var key = $"{item.TableName}|{item.ColumnName}|{item.DefaultEqual}";
                if (!seen.Add(key)) continue;

                await using var ins = new SqlCommand(insertSql, conn, (SqlTransaction)tx);
                ins.Parameters.AddWithValue("@PaperId", paperId);
                ins.Parameters.AddWithValue("@TableName", string.IsNullOrWhiteSpace(item.TableName) ? dictTable : item.TableName);
                ins.Parameters.AddWithValue("@AliasName", (object?)item.AliasName ?? DBNull.Value);
                ins.Parameters.AddWithValue("@ColumnName", item.ColumnName);
                ins.Parameters.AddWithValue("@ColumnCaption", item.ColumnCaption ?? item.ColumnName);
                ins.Parameters.AddWithValue("@DataType", item.DataType);
                ins.Parameters.AddWithValue("@SortOrder", item.SortOrder > 0 ? item.SortOrder : order++);
                object? defaultValueParam = string.IsNullOrWhiteSpace(item.DefaultValue)
                    ? DBNull.Value
                    : item.DefaultValue;
                ins.Parameters.AddWithValue("@DefaultValue", defaultValueParam);
                ins.Parameters.AddWithValue("@DefaultEqual", string.IsNullOrWhiteSpace(item.DefaultEqual) ? "=" : item.DefaultEqual);
                ins.Parameters.AddWithValue("@ControlType", (object?)item.ControlType ?? DBNull.Value);
                ins.Parameters.AddWithValue("@CommandText", (object?)item.CommandText ?? DBNull.Value);
                ins.Parameters.AddWithValue("@DefaultType", (object?)item.DefaultType ?? DBNull.Value);
                ins.Parameters.AddWithValue("@EditMask", (object?)item.EditMask ?? DBNull.Value);
                ins.Parameters.AddWithValue("@SuperId", (object?)item.SuperId ?? DBNull.Value);
                ins.Parameters.AddWithValue("@ParamValue", (object?)item.ParamValue ?? DBNull.Value);
                ins.Parameters.AddWithValue("@ParamType", (object?)item.ParamType ?? DBNull.Value);
                ins.Parameters.AddWithValue("@IReadOnly", (object?)item.IReadOnly ?? DBNull.Value);
                ins.Parameters.AddWithValue("@IVisible", (object?)item.IVisible ?? 1);
                ins.Parameters.AddWithValue("@TableKind", (object?)item.TableKind ?? DBNull.Value);
                await ins.ExecuteNonQueryAsync();

                // 覆寫 trigger 可能改掉的 DefaultValue（例如空白被改成 0）
                await using (var upd = new SqlCommand(@"
UPDATE CURdPaperSelected
   SET DefaultValue = @DefaultValue
 WHERE PaperId = @PaperId
   AND TableName = @TableName
   AND ColumnName = @ColumnName
   AND DefaultEqual = @DefaultEqual;", conn, (SqlTransaction)tx))
                {
                    upd.Parameters.AddWithValue("@DefaultValue", defaultValueParam);
                    upd.Parameters.AddWithValue("@PaperId", paperId);
                    upd.Parameters.AddWithValue("@TableName", string.IsNullOrWhiteSpace(item.TableName) ? dictTable : item.TableName);
                    upd.Parameters.AddWithValue("@ColumnName", item.ColumnName);
                    upd.Parameters.AddWithValue("@DefaultEqual", string.IsNullOrWhiteSpace(item.DefaultEqual) ? "=" : item.DefaultEqual);
                    await upd.ExecuteNonQueryAsync();
                }
            }

            await tx.CommitAsync();
            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return StatusCode(500, new { ok = false, error = ex.Message });
        }
    }

    [HttpGet("QueryFieldOptions")]
    public async Task<IActionResult> GetQueryFieldOptions(
        [FromQuery] string itemId,
        [FromQuery] string table,
        [FromQuery] string column,
        [FromQuery] string? superValue = null)
    {
        if (string.IsNullOrWhiteSpace(itemId) || string.IsNullOrWhiteSpace(table) || string.IsNullOrWhiteSpace(column))
            return BadRequest("itemId, table, column are required.");

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        var dictTable = table.Trim();
        var realTable = await ResolveRealTableNameAsync(conn, dictTable) ?? dictTable;
        var paperId = await ResolvePaperIdAsync(conn, itemId, dictTable, realTable) ?? realTable;

        var cmdText = await LoadQueryCommandTextAsync(conn, paperId, dictTable, column);
        if (string.IsNullOrWhiteSpace(cmdText)) return NotFound();

        var raw = cmdText.Trim();
        if (raw.Contains("@@@@@", StringComparison.Ordinal))
        {
            if (string.IsNullOrWhiteSpace(superValue))
                return Ok(Array.Empty<object>());
            raw = raw.Replace("@@@@@", "@p0", StringComparison.Ordinal);
        }
        if (!raw.StartsWith("select", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Only SELECT is allowed");

        await using var cmd = new SqlCommand(raw, conn);
        if (raw.Contains("@p0", StringComparison.Ordinal))
        {
            if (DateTime.TryParse(superValue, out var dt))
                cmd.Parameters.AddWithValue("@p0", dt);
            else
                cmd.Parameters.AddWithValue("@p0", superValue ?? string.Empty);
        }

        var list = new List<object>();
        await using var reader = await cmd.ExecuteReaderAsync();

        var ordValue = -1;
        var ordText = -1;
        for (int i = 0; i < reader.FieldCount; i++)
        {
            var name = reader.GetName(i);
            if (ordValue == -1 && name.Equals("value", StringComparison.OrdinalIgnoreCase)) ordValue = i;
            if (ordText == -1 && name.Equals("text", StringComparison.OrdinalIgnoreCase)) ordText = i;
            if (ordValue == -1 && name.Equals("item", StringComparison.OrdinalIgnoreCase)) ordValue = i;
            if (ordText == -1 && name.Equals("itemname", StringComparison.OrdinalIgnoreCase)) ordText = i;
        }
        if (ordValue == -1 && reader.FieldCount > 0) ordValue = 0;
        if (ordText == -1 && reader.FieldCount > 1) ordText = 1;
        if (ordText == -1) ordText = ordValue;

        while (await reader.ReadAsync())
        {
            var value = reader.IsDBNull(ordValue) ? "" : reader.GetValue(ordValue)?.ToString() ?? "";
            var text = reader.IsDBNull(ordText) ? "" : reader.GetValue(ordText)?.ToString() ?? "";
            list.Add(new { value, text });
        }

        return Ok(list);
    }

    private static async Task<string?> ResolveRealTableNameAsync(SqlConnection conn, string dictTableName)
    {
        const string sql = @"
SELECT TOP 1 ISNULL(NULLIF(RealTableName,''), TableName) AS ActualName
  FROM CURdTableName WITH (NOLOCK)
 WHERE TableName = @tbl";

        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@tbl", dictTableName ?? string.Empty);
        var result = await cmd.ExecuteScalarAsync();
        return result == null || result == DBNull.Value ? null : result.ToString();
    }

    private static async Task<string?> ResolvePaperIdAsync(SqlConnection conn, string itemId, string dictTable, string realTable)
    {
        if (!string.IsNullOrWhiteSpace(itemId))
        {
            await using (var cmdMaster = new SqlCommand(@"
SELECT TOP 1 TableName
  FROM CURdOCXTableSetUp WITH (NOLOCK)
 WHERE ItemId = @itemId
   AND (TableKind = 'Master1' OR TableKind LIKE 'Master%')
 ORDER BY TableKind;", conn))
            {
                cmdMaster.Parameters.AddWithValue("@itemId", itemId);
                var masterObj = await cmdMaster.ExecuteScalarAsync();
                if (masterObj != null && masterObj != DBNull.Value)
                {
                    var dict = masterObj.ToString();
                    var real = await ResolveRealTableNameAsync(conn, dict ?? "");
                    if (!string.IsNullOrWhiteSpace(real)) return real;
                }
            }

            await using var cmd = new SqlCommand(@"
SELECT TOP 1 PaperId
  FROM CURdSysItems WITH (NOLOCK)
 WHERE ItemId = @itemId;", conn);
            cmd.Parameters.AddWithValue("@itemId", itemId ?? string.Empty);
            var paperIdObj = await cmd.ExecuteScalarAsync();
            if (paperIdObj != null && paperIdObj != DBNull.Value)
                return paperIdObj.ToString();
        }

        return string.IsNullOrWhiteSpace(realTable) ? dictTable : realTable;
    }

    private static async Task<string?> LoadQueryCommandTextAsync(SqlConnection conn, string paperId, string dictTable, string column)
    {
        const string sql = @"
SELECT TOP 1 CommandText
  FROM CURdPaperSelected WITH (NOLOCK)
 WHERE (PaperId = @paperId OR TableName = @dictTable)
   AND ColumnName = @col;";
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@paperId", paperId ?? string.Empty);
        cmd.Parameters.AddWithValue("@dictTable", dictTable ?? string.Empty);
        cmd.Parameters.AddWithValue("@col", column ?? string.Empty);
        var obj = await cmd.ExecuteScalarAsync();
        return obj == null || obj == DBNull.Value ? null : obj.ToString();
    }

    private static async Task<List<PaperSelectedRow>> LoadPaperDictSelectAsync(
        SqlConnection conn,
        string itemId,
        string? equal,
        string lang)
    {
        var list = new List<PaperSelectedRow>();
        var eq = string.IsNullOrWhiteSpace(equal) ? "" : equal.Trim();
        await using var cmd = new SqlCommand("exec CURdOCXPaperDictSelect @ItemId, @Equal, 0, @LanguageId", conn);
        cmd.Parameters.AddWithValue("@ItemId", itemId ?? string.Empty);
        cmd.Parameters.AddWithValue("@Equal", eq ?? string.Empty);
        cmd.Parameters.AddWithValue("@LanguageId", string.IsNullOrWhiteSpace(lang) ? "TW" : lang);

        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            list.Add(new PaperSelectedRow
            {
                PaperId = rd["PaperId"]?.ToString() ?? "",
                TableName = rd["TableName"]?.ToString() ?? "",
                AliasName = rd["AliasName"]?.ToString(),
                ColumnName = rd["ColumnName"]?.ToString() ?? "",
                ColumnCaption = rd["ColumnCaption"]?.ToString() ?? "",
                DataType = TryToInt(rd["DataType"]) ?? 0,
                SortOrder = TryToInt(rd["SortOrder"]) ?? 0,
                DefaultEqual = rd["DefaultEqual"]?.ToString() ?? ""
            });
        }
        return list;
    }

    // GET /api/DictSetupApi/InqMust?paperId=...
    [HttpGet("InqMust")]
    public async Task<IActionResult> GetInqMust([FromQuery] string paperId)
    {
        if (string.IsNullOrWhiteSpace(paperId))
            return BadRequest("paperId is required.");

        var list = new List<InqMustRow>();
        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        using var cmd = new SqlCommand("exec CURdInqSelectedGetMust @PaperId", conn);
        cmd.Parameters.AddWithValue("@PaperId", paperId);
        using var rd = await cmd.ExecuteReaderAsync();

        while (await rd.ReadAsync())
        {
            list.Add(new InqMustRow
            {
                FieldMust = rd["sFieldMust"]?.ToString() ?? "",
                ShowError = rd["sShowError"]?.ToString() ?? ""
            });
        }

        return Ok(list);
    }

    // GET /api/DictSetupApi/GetItemIdByTableName?tableName=xxx
    [HttpGet("GetItemIdByTableName")]
    public async Task<IActionResult> GetItemIdByTableName([FromQuery] string tableName)
    {
        if (string.IsNullOrWhiteSpace(tableName))
        {
            return BadRequest(new { error = "tableName is required" });
        }

        try
        {
            await using var conn = new SqlConnection(_connStr);
            await conn.OpenAsync();

            var sql = @"
                SELECT TOP 1 ItemId
                FROM CURdOCXTableSetUp WITH (NOLOCK)
                WHERE TableName = @TableName";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@TableName", tableName);

            var result = await cmd.ExecuteScalarAsync();

            if (result != null && result != DBNull.Value)
            {
                return Ok(new { itemId = result.ToString() });
            }

            // 如果找不到，返回原始的 tableName 作為 fallback
            return Ok(new { itemId = tableName });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // ===== C. 欄位設定（用於 F3 對辭典和唯讀控制）=====
    public class FieldSettingRow
    {
        public string FieldName { get; set; }
        public string DisplayLabel { get; set; }
        public int? ReadOnly { get; set; }
        public string OCXLKTableName { get; set; }
        public string OCXLKResultName { get; set; }
        public string DataType { get; set; }
    }

    // GET /api/DictSetupApi/FieldSettings/{tableName}
    [HttpGet("FieldSettings/{tableName}")]
    public async Task<IActionResult> GetFieldSettings(string tableName)
    {
        if (string.IsNullOrWhiteSpace(tableName))
            return BadRequest(new { success = false, message = "tableName is required" });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        var cmd = new SqlCommand(@"
            SELECT FieldName, DisplayLabel, ReadOnly, OCXLKTableName, OCXLKResultName, DataType
              FROM CURdTableField WITH (NOLOCK)
             WHERE LOWER(REPLACE(TableName, 'dbo.', '')) = LOWER(REPLACE(@tableName, 'dbo.', ''))
             ORDER BY SerialNum;", conn);

        cmd.Parameters.AddWithValue("@tableName", tableName);

        var list = new List<FieldSettingRow>();
        using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            list.Add(new FieldSettingRow
            {
                FieldName = rd["FieldName"]?.ToString() ?? string.Empty,
                DisplayLabel = rd["DisplayLabel"]?.ToString() ?? string.Empty,
                ReadOnly = TryToInt(rd["ReadOnly"]),
                OCXLKTableName = rd["OCXLKTableName"]?.ToString() ?? string.Empty,
                OCXLKResultName = rd["OCXLKResultName"]?.ToString() ?? string.Empty,
                DataType = rd["DataType"]?.ToString() ?? string.Empty
            });
        }

        return Ok(list);
    }

    // GET /api/DictSetupApi/LookupData/{tableName}/{resultField}
    [HttpGet("LookupData/{tableName}/{resultField}")]
    public async Task<IActionResult> GetLookupData(string tableName, string resultField)
    {
        if (string.IsNullOrWhiteSpace(tableName) || string.IsNullOrWhiteSpace(resultField))
            return BadRequest(new { success = false, message = "tableName and resultField are required" });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        // 動態查詢對辭典資料表
        var cmd = new SqlCommand($@"
            SELECT TOP 1000 [{resultField}]
              FROM [{tableName}] WITH (NOLOCK)
             WHERE [{resultField}] IS NOT NULL
             ORDER BY [{resultField}];", conn);

        var list = new List<Dictionary<string, object>>();
        using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            var row = new Dictionary<string, object>();
            for (int i = 0; i < rd.FieldCount; i++)
            {
                row[rd.GetName(i)] = rd.GetValue(i) ?? DBNull.Value;
            }
            list.Add(row);
        }

        return Ok(list);
    }

    // ===== F. 系統按鈕設定（CURdOCXItemSysButton）=====
    public class SysButtonRow
    {
        public string ItemId { get; set; }
        public string ButtonName { get; set; }
        public int? bVisiable { get; set; }
        public string CustCaption { get; set; }
        public string CustHint { get; set; }
        public int? bShowMsg { get; set; }
        public string sCustMsg { get; set; }
        public int? bShowMsgAft { get; set; }
        public string sCustMsgAft { get; set; }
        public string CustCaptionCN { get; set; }
        public string CustCaptionEN { get; set; }
        public string CustCaptionJP { get; set; }
        public string CustCaptionTH { get; set; }
        public string CustHintCN { get; set; }
        public string CustHintEN { get; set; }
        public string CustHintJP { get; set; }
        public string CustHintTH { get; set; }
        public int? SerialNum { get; set; }
    }

    // GET /api/DictSetupApi/SysButton/ByItem/{itemId}
    [HttpGet("SysButton/ByItem/{itemId}")]
    public async Task<IActionResult> GetSysButtonsByItem(string itemId)
    {
        var list = new List<SysButtonRow>();
        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        var cmd = new SqlCommand(@"
            SELECT ItemId, ButtonName, bVisiable, CustCaption, CustHint,
                   bShowMsg, sCustMsg, bShowMsgAft, sCustMsgAft,
                   CustCaptionCN, CustCaptionEN, CustCaptionJP, CustCaptionTH,
                   CustHintCN, CustHintEN, CustHintJP, CustHintTH, SerialNum
              FROM CURdOCXItemSysButton WITH (NOLOCK)
             WHERE ItemId = @itemId
             ORDER BY SerialNum, ButtonName;", conn);

        cmd.Parameters.AddWithValue("@itemId", (object?)itemId ?? string.Empty);

        using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            list.Add(new SysButtonRow
            {
                ItemId = rd["ItemId"]?.ToString() ?? string.Empty,
                ButtonName = rd["ButtonName"]?.ToString() ?? string.Empty,
                bVisiable = TryToInt(rd["bVisiable"]),
                CustCaption = rd["CustCaption"]?.ToString() ?? string.Empty,
                CustHint = rd["CustHint"]?.ToString() ?? string.Empty,
                bShowMsg = TryToInt(rd["bShowMsg"]),
                sCustMsg = rd["sCustMsg"]?.ToString() ?? string.Empty,
                bShowMsgAft = TryToInt(rd["bShowMsgAft"]),
                sCustMsgAft = rd["sCustMsgAft"]?.ToString() ?? string.Empty,
                CustCaptionCN = rd["CustCaptionCN"]?.ToString() ?? string.Empty,
                CustCaptionEN = rd["CustCaptionEN"]?.ToString() ?? string.Empty,
                CustCaptionJP = rd["CustCaptionJP"]?.ToString() ?? string.Empty,
                CustCaptionTH = rd["CustCaptionTH"]?.ToString() ?? string.Empty,
                CustHintCN = rd["CustHintCN"]?.ToString() ?? string.Empty,
                CustHintEN = rd["CustHintEN"]?.ToString() ?? string.Empty,
                CustHintJP = rd["CustHintJP"]?.ToString() ?? string.Empty,
                CustHintTH = rd["CustHintTH"]?.ToString() ?? string.Empty,
                SerialNum = TryToInt(rd["SerialNum"])
            });
        }

        return Ok(list);
    }

    // POST /api/DictSetupApi/SysButton/Save
    [HttpPost("SysButton/Save")]
    public async Task<IActionResult> SaveSysButtons([FromBody] List<SysButtonRow> list)
    {
        if (list == null || list.Count == 0)
            return BadRequest(new { success = false, message = "no payload" });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        foreach (var x in list)
        {
            var cmd = new SqlCommand(@"
UPDATE CURdOCXItemSysButton
   SET bVisiable = @bVisiable,
       CustCaption = @CustCaption,
       CustHint = @CustHint,
       bShowMsg = @bShowMsg,
       sCustMsg = @sCustMsg,
       bShowMsgAft = @bShowMsgAft,
       sCustMsgAft = @sCustMsgAft,
       CustCaptionCN = @CustCaptionCN,
       CustCaptionEN = @CustCaptionEN,
       CustCaptionJP = @CustCaptionJP,
       CustCaptionTH = @CustCaptionTH,
       CustHintCN = @CustHintCN,
       CustHintEN = @CustHintEN,
       CustHintJP = @CustHintJP,
       CustHintTH = @CustHintTH,
       SerialNum = @SerialNum
 WHERE ItemId = @ItemId AND ButtonName = @ButtonName;", conn);

            cmd.Parameters.AddWithValue("@ItemId", x.ItemId ?? "");
            cmd.Parameters.AddWithValue("@ButtonName", x.ButtonName ?? "");
            cmd.Parameters.AddWithValue("@bVisiable", (object?)x.bVisiable ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CustCaption", (object?)x.CustCaption ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CustHint", (object?)x.CustHint ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@bShowMsg", (object?)x.bShowMsg ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@sCustMsg", (object?)x.sCustMsg ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@bShowMsgAft", (object?)x.bShowMsgAft ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@sCustMsgAft", (object?)x.sCustMsgAft ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CustCaptionCN", (object?)x.CustCaptionCN ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CustCaptionEN", (object?)x.CustCaptionEN ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CustCaptionJP", (object?)x.CustCaptionJP ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CustCaptionTH", (object?)x.CustCaptionTH ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CustHintCN", (object?)x.CustHintCN ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CustHintEN", (object?)x.CustHintEN ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CustHintJP", (object?)x.CustHintJP ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CustHintTH", (object?)x.CustHintTH ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@SerialNum", (object?)x.SerialNum ?? DBNull.Value);

            var affected = await cmd.ExecuteNonQueryAsync();
            if (affected == 0)
            {
                var ins = new SqlCommand(@"
INSERT INTO CURdOCXItemSysButton
(
    ItemId, ButtonName, bVisiable, CustCaption, CustHint, bShowMsg, sCustMsg, bShowMsgAft, sCustMsgAft,
    CustCaptionCN, CustCaptionEN, CustCaptionJP, CustCaptionTH,
    CustHintCN, CustHintEN, CustHintJP, CustHintTH, SerialNum
)
VALUES
(
    @ItemId, @ButtonName, @bVisiable, @CustCaption, @CustHint, @bShowMsg, @sCustMsg, @bShowMsgAft, @sCustMsgAft,
    @CustCaptionCN, @CustCaptionEN, @CustCaptionJP, @CustCaptionTH,
    @CustHintCN, @CustHintEN, @CustHintJP, @CustHintTH, @SerialNum
);", conn);

                ins.Parameters.AddWithValue("@ItemId", x.ItemId ?? "");
                ins.Parameters.AddWithValue("@ButtonName", x.ButtonName ?? "");
                ins.Parameters.AddWithValue("@bVisiable", (object?)x.bVisiable ?? DBNull.Value);
                ins.Parameters.AddWithValue("@CustCaption", (object?)x.CustCaption ?? DBNull.Value);
                ins.Parameters.AddWithValue("@CustHint", (object?)x.CustHint ?? DBNull.Value);
                ins.Parameters.AddWithValue("@bShowMsg", (object?)x.bShowMsg ?? DBNull.Value);
                ins.Parameters.AddWithValue("@sCustMsg", (object?)x.sCustMsg ?? DBNull.Value);
                ins.Parameters.AddWithValue("@bShowMsgAft", (object?)x.bShowMsgAft ?? DBNull.Value);
                ins.Parameters.AddWithValue("@sCustMsgAft", (object?)x.sCustMsgAft ?? DBNull.Value);
                ins.Parameters.AddWithValue("@CustCaptionCN", (object?)x.CustCaptionCN ?? DBNull.Value);
                ins.Parameters.AddWithValue("@CustCaptionEN", (object?)x.CustCaptionEN ?? DBNull.Value);
                ins.Parameters.AddWithValue("@CustCaptionJP", (object?)x.CustCaptionJP ?? DBNull.Value);
                ins.Parameters.AddWithValue("@CustCaptionTH", (object?)x.CustCaptionTH ?? DBNull.Value);
                ins.Parameters.AddWithValue("@CustHintCN", (object?)x.CustHintCN ?? DBNull.Value);
                ins.Parameters.AddWithValue("@CustHintEN", (object?)x.CustHintEN ?? DBNull.Value);
                ins.Parameters.AddWithValue("@CustHintJP", (object?)x.CustHintJP ?? DBNull.Value);
                ins.Parameters.AddWithValue("@CustHintTH", (object?)x.CustHintTH ?? DBNull.Value);
                ins.Parameters.AddWithValue("@SerialNum", (object?)x.SerialNum ?? DBNull.Value);
                await ins.ExecuteNonQueryAsync();
            }
        }

        return Ok(new { success = true, count = list.Count });
    }

    // POST /api/DictSetupApi/SysButton/Import/{itemId}
    [HttpPost("SysButton/Import/{itemId}")]
    public async Task<IActionResult> ImportSysButton(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
            return BadRequest(new { success = false, message = "itemId is required" });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();
        var cmd = new SqlCommand("exec CURdOCXImportSysButton @ItemId", conn);
        cmd.Parameters.AddWithValue("@ItemId", itemId);
        await cmd.ExecuteNonQueryAsync();

        return Ok(new { success = true });
    }

    // ===== G. 其他規則設定（CURdOCXItemOtherRule）=====
    public class ItemOtherRuleRow
    {
        public string ItemId { get; set; } = string.Empty;
        public string RuleId { get; set; } = string.Empty;
        public string Lk_RuleName { get; set; } = string.Empty;
        public string DLLValue { get; set; } = string.Empty;
    }

    public class OtherRuleOptionRow
    {
        public string RuleId { get; set; } = string.Empty;
        public string RuleName { get; set; } = string.Empty;
    }

    // GET /api/DictSetupApi/OtherRule/ByItem/{itemId}
    [HttpGet("OtherRule/ByItem/{itemId}")]
    public async Task<IActionResult> GetOtherRuleByItem(string itemId)
    {
        var list = new List<ItemOtherRuleRow>();
        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        var cmd = new SqlCommand(@"
SELECT a.ItemId, a.RuleId, ISNULL(b.RuleName,'') AS Lk_RuleName, a.DLLValue
  FROM CURdOCXItemOtherRule a WITH (NOLOCK)
  LEFT JOIN CURdOCXOtherRule b WITH (NOLOCK) ON a.RuleId = b.RuleId
 WHERE a.ItemId = @itemId
 ORDER BY a.RuleId;", conn);
        cmd.Parameters.AddWithValue("@itemId", itemId ?? string.Empty);

        using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            list.Add(new ItemOtherRuleRow
            {
                ItemId = rd["ItemId"]?.ToString() ?? string.Empty,
                RuleId = rd["RuleId"]?.ToString() ?? string.Empty,
                Lk_RuleName = rd["Lk_RuleName"]?.ToString() ?? string.Empty,
                DLLValue = rd["DLLValue"]?.ToString() ?? string.Empty
            });
        }
        return Ok(list);
    }

    // GET /api/DictSetupApi/OtherRule/Options
    [HttpGet("OtherRule/Options")]
    public async Task<IActionResult> GetOtherRuleOptions()
    {
        var list = new List<OtherRuleOptionRow>();
        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();
        var cmd = new SqlCommand(@"
SELECT RuleId, RuleName
  FROM CURdOCXOtherRule WITH (NOLOCK)
 ORDER BY RuleId;", conn);
        using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            list.Add(new OtherRuleOptionRow
            {
                RuleId = rd["RuleId"]?.ToString() ?? string.Empty,
                RuleName = rd["RuleName"]?.ToString() ?? string.Empty
            });
        }
        return Ok(list);
    }

    // POST /api/DictSetupApi/OtherRule/Save
    [HttpPost("OtherRule/Save")]
    public async Task<IActionResult> SaveOtherRules([FromBody] List<ItemOtherRuleRow> rows, [FromQuery] string? itemId = null)
    {
        if (rows == null) rows = new List<ItemOtherRuleRow>();
        var targetItemId = (itemId ?? rows.Select(x => x.ItemId?.Trim()).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(targetItemId))
            return BadRequest(new { success = false, message = "ItemId required" });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();
        await using var tx = await conn.BeginTransactionAsync();
        try
        {
            var del = new SqlCommand("DELETE FROM CURdOCXItemOtherRule WHERE ItemId = @itemId;", conn, (SqlTransaction)tx);
            del.Parameters.AddWithValue("@itemId", targetItemId);
            await del.ExecuteNonQueryAsync();

            var validRows = rows
                .Where(x => !string.IsNullOrWhiteSpace(x.RuleId))
                .GroupBy(x => x.RuleId.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(g => g.Last())
                .ToList();

            foreach (var x in validRows)
            {
                var ins = new SqlCommand(@"
INSERT INTO CURdOCXItemOtherRule (ItemId, RuleId, DLLValue)
VALUES (@itemId, @ruleId, @dllValue);", conn, (SqlTransaction)tx);
                ins.Parameters.AddWithValue("@itemId", targetItemId);
                ins.Parameters.AddWithValue("@ruleId", x.RuleId?.Trim() ?? string.Empty);
                ins.Parameters.AddWithValue("@dllValue", (object?)x.DLLValue ?? DBNull.Value);
                await ins.ExecuteNonQueryAsync();
            }

            await tx.CommitAsync();
            return Ok(new { success = true, count = validRows.Count });
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }
}
