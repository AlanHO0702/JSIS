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
    public async Task<IActionResult> GetPaperSelected([FromQuery] string itemId, [FromQuery] string table, [FromQuery] string? equal = null, [FromQuery] string lang = "TW")
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
                ins.Parameters.AddWithValue("@DefaultValue", (object?)item.DefaultValue ?? DBNull.Value);
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
}
