using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class CustomButtonApiController : ControllerBase
{
    private readonly string _connStr;
    public CustomButtonApiController(IConfiguration config)
    {
        _connStr = config.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    // ===== helpers =====
    private static int? TryToInt(object o)
    {
        if (o == null || o is DBNull) return null;
        return int.TryParse(o.ToString(), out var n) ? n : null;
    }
    private static object DbNull(object? v) => v ?? DBNull.Value;

    private sealed class ButtonSchema
    {
        public HashSet<string> Cols { get; init; } = new(StringComparer.OrdinalIgnoreCase);
        public bool HasCaptionE => Cols.Contains("CustCaptionE");
        public bool HasHintE => Cols.Contains("CustHintE");
        public string ChkCanUpdateCol =>
            Cols.Contains("ChkCanUpdate") ? "ChkCanUpdate"
            : Cols.Contains("ChkCanbUpdate") ? "ChkCanbUpdate"
            : "ChkCanUpdate";

        public bool Has(string col) => Cols.Contains(col);
        public string NVarExpr(string col) => Has(col) ? col : $"CAST('' AS nvarchar(1)) AS {col}";
        public string IntExpr(string col, int fallback = 0) => Has(col) ? col : $"CAST({fallback} AS int) AS {col}";
    }

    private async Task<ButtonSchema> DetectSchema(SqlConnection conn, SqlTransaction? tx = null)
    {
        var sql = @"
SELECT name FROM sys.columns WHERE object_id = OBJECT_ID('dbo.CURdOCXItemCustButton')";
        var cols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        await using (var cmd = new SqlCommand(sql, conn, tx))
        using (var rd = await cmd.ExecuteReaderAsync())
        {
            while (await rd.ReadAsync())
                cols.Add(rd.GetString(0));
        }

        return new ButtonSchema { Cols = cols };
    }

    // ===== DTO =====
    public class CustomButtonRow
    {
        public string? ItemId { get; set; }
        public int? SerialNum { get; set; }
        public string? ButtonName { get; set; }

        public string? CustCaption { get; set; }
        public string? CustCaptionE { get; set; }
        public string? CustHint { get; set; }
        public string? CustHintE { get; set; }

        public string? OCXName { get; set; }
        public string? CoClassName { get; set; }
        public string? SpName { get; set; }

        public int? bVisible { get; set; }
        public int? ChkCanUpdate { get; set; }
        public int? bNeedNum { get; set; }
        public int? DesignType { get; set; }
        public int? bSpHasResult { get; set; }
        public string? SearchTemplate { get; set; }
        public string? MultiSelectDD { get; set; }
        public string? ExecSpName { get; set; }
        public int? AllowSelCount { get; set; }
        public int? ReplaceExists { get; set; }
        public string? DialogCaption { get; set; }
        public int? bTranByGlobalId { get; set; }
        public string? PrintSQL { get; set; }
        public string? PrintSp { get; set; }
        public string? PrintRptName { get; set; }
        public string? GetPohtoSQL { get; set; }
        public string? NewPohtoName { get; set; }
        public int? iMultiSelUseDtl { get; set; }
        public string? MultiSelDtlSQL { get; set; }
        public string? MultiSelDtlDDName { get; set; }
        public string? MultiSelDtlKeyName { get; set; }
        public int? iEditGridType { get; set; }
        public string? EditGridTableKind { get; set; }
        public string? EditGridTableField { get; set; }
        public int? NeediFlag { get; set; }
        public int? NeediSeqNum { get; set; }
    }

    // ===== GET =====
    // GET /api/CustomButtonApi/ByItem/{itemId}
    [HttpGet("ByItem/{itemId}")]
    public async Task<IActionResult> GetByItem(string itemId)
    {
        var list = new List<CustomButtonRow>();
        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        var schema = await DetectSchema(conn);

        var select = $@"
SELECT ItemId, SerialNum, ButtonName,
       CustCaption,
       {schema.NVarExpr("CustCaptionE")},
       CustHint,
       {schema.NVarExpr("CustHintE")},
       OCXName, CoClassName, SpName,
       bVisible, {schema.ChkCanUpdateCol} AS ChkCanUpdate, bNeedNum, DesignType,
       {schema.IntExpr("bSpHasResult")},
       {schema.NVarExpr("SearchTemplate")},
       {schema.NVarExpr("MultiSelectDD")},
       {schema.NVarExpr("ExecSpName")},
       {schema.IntExpr("AllowSelCount")},
       {schema.IntExpr("ReplaceExists")},
       {schema.NVarExpr("DialogCaption")},
       {schema.IntExpr("bTranByGlobalId")},
       {schema.NVarExpr("PrintSQL")},
       {schema.NVarExpr("PrintSp")},
       {schema.NVarExpr("PrintRptName")},
       {schema.NVarExpr("GetPohtoSQL")},
       {schema.NVarExpr("NewPohtoName")},
       {schema.IntExpr("iMultiSelUseDtl")},
       {schema.NVarExpr("MultiSelDtlSQL")},
       {schema.NVarExpr("MultiSelDtlDDName")},
       {schema.NVarExpr("MultiSelDtlKeyName")},
       {schema.IntExpr("iEditGridType")},
       {schema.NVarExpr("EditGridTableKind")},
       {schema.NVarExpr("EditGridTableField")},
       {schema.IntExpr("NeediFlag")},
       {schema.IntExpr("NeediSeqNum")}
FROM CURdOCXItemCustButton WITH (NOLOCK)
WHERE ItemId = @itemId
ORDER BY SerialNum, ButtonName;";

        await using var cmd = new SqlCommand(select, conn);
        cmd.Parameters.AddWithValue("@itemId", itemId ?? string.Empty);

        using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            list.Add(new CustomButtonRow
            {
                ItemId = rd["ItemId"]?.ToString()!,
                SerialNum = TryToInt(rd["SerialNum"]),
                ButtonName = rd["ButtonName"]?.ToString() ?? string.Empty,
                CustCaption = rd["CustCaption"]?.ToString() ?? string.Empty,
                CustCaptionE = rd["CustCaptionE"]?.ToString() ?? string.Empty, // 若欄位不存在，為空字串
                CustHint = rd["CustHint"]?.ToString() ?? string.Empty,
                CustHintE = rd["CustHintE"]?.ToString() ?? string.Empty,    // 若欄位不存在，為空字串
                OCXName = rd["OCXName"]?.ToString() ?? string.Empty,
                CoClassName = rd["CoClassName"]?.ToString() ?? string.Empty,
                SpName = rd["SpName"]?.ToString() ?? string.Empty,
                bVisible = TryToInt(rd["bVisible"]),
                ChkCanUpdate = TryToInt(rd["ChkCanUpdate"]),
                bNeedNum = TryToInt(rd["bNeedNum"]),
                DesignType = TryToInt(rd["DesignType"]),
                bSpHasResult = TryToInt(rd["bSpHasResult"]),
                SearchTemplate = rd["SearchTemplate"]?.ToString() ?? string.Empty,
                MultiSelectDD = rd["MultiSelectDD"]?.ToString() ?? string.Empty,
                ExecSpName = rd["ExecSpName"]?.ToString() ?? string.Empty,
                AllowSelCount = TryToInt(rd["AllowSelCount"]),
                ReplaceExists = TryToInt(rd["ReplaceExists"]),
                DialogCaption = rd["DialogCaption"]?.ToString() ?? string.Empty,
                bTranByGlobalId = TryToInt(rd["bTranByGlobalId"]),
                PrintSQL = rd["PrintSQL"]?.ToString() ?? string.Empty,
                PrintSp = rd["PrintSp"]?.ToString() ?? string.Empty,
                PrintRptName = rd["PrintRptName"]?.ToString() ?? string.Empty,
                GetPohtoSQL = rd["GetPohtoSQL"]?.ToString() ?? string.Empty,
                NewPohtoName = rd["NewPohtoName"]?.ToString() ?? string.Empty,
                iMultiSelUseDtl = TryToInt(rd["iMultiSelUseDtl"]),
                MultiSelDtlSQL = rd["MultiSelDtlSQL"]?.ToString() ?? string.Empty,
                MultiSelDtlDDName = rd["MultiSelDtlDDName"]?.ToString() ?? string.Empty,
                MultiSelDtlKeyName = rd["MultiSelDtlKeyName"]?.ToString() ?? string.Empty,
                iEditGridType = TryToInt(rd["iEditGridType"]),
                EditGridTableKind = rd["EditGridTableKind"]?.ToString() ?? string.Empty,
                EditGridTableField = rd["EditGridTableField"]?.ToString() ?? string.Empty,
                NeediFlag = TryToInt(rd["NeediFlag"]),
                NeediSeqNum = TryToInt(rd["NeediSeqNum"])
            });
        }

        return Ok(list);
    }

    // ===== POST：同 ItemId 先刪後增 =====
    // POST /api/CustomButtonApi/Save
    [HttpPost("Save")]
    public async Task<IActionResult> Save([FromBody] List<CustomButtonRow> rows)
    {
        if (rows == null || rows.Count == 0)
            return BadRequest(new { success = false, message = "no payload" });

        var itemId = rows[0].ItemId?.Trim();
        if (string.IsNullOrEmpty(itemId))
            return BadRequest(new { success = false, message = "ItemId required" });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();
        await using var tx = await conn.BeginTransactionAsync(IsolationLevel.ReadCommitted);

        try
        {
            var schema = await DetectSchema(conn, (SqlTransaction)tx);

            // 刪現有
            var del = new SqlCommand(@"DELETE FROM CURdOCXItemCustButton WHERE ItemId=@itemId;", conn, (SqlTransaction)tx);
            del.Parameters.AddWithValue("@itemId", itemId);
            await del.ExecuteNonQueryAsync();

            // 動態 INSERT 欄位
            var cols = new List<string>{
                "ItemId","SerialNum","ButtonName",
                "CustCaption","CustHint","OCXName","CoClassName","SpName",
                "bVisible", schema.ChkCanUpdateCol, "bNeedNum","DesignType"
            };
            if (schema.HasCaptionE) cols.Insert(4, "CustCaptionE"); // 放在 CustCaption 後
            if (schema.HasHintE) cols.Insert(schema.HasCaptionE ? 6 : 5, "CustHintE"); // 放在 CustHint 後

            // 擴充欄位（依 schema 動態加入）
            var extStr = new[]{"SearchTemplate","MultiSelectDD","ExecSpName","DialogCaption",
                "PrintSQL","PrintSp","PrintRptName","GetPohtoSQL","NewPohtoName",
                "MultiSelDtlSQL","MultiSelDtlDDName","MultiSelDtlKeyName",
                "EditGridTableKind","EditGridTableField"};
            var extInt = new[]{"bSpHasResult","AllowSelCount","ReplaceExists","bTranByGlobalId",
                "iMultiSelUseDtl","iEditGridType","NeediFlag","NeediSeqNum"};
            foreach (var c in extStr) { if (schema.Has(c)) cols.Add(c); }
            foreach (var c in extInt) { if (schema.Has(c)) cols.Add(c); }

            var pCols = new List<string>();
            foreach (var c in cols) pCols.Add("@" + c);

            var insertSql = $"INSERT INTO CURdOCXItemCustButton({string.Join(",", cols)}) VALUES({string.Join(",", pCols)});";
            var ins = new SqlCommand(insertSql, conn, (SqlTransaction)tx);

            // 建參數
            void Add(string name, SqlDbType type, int size = 0)
            {
                if (size > 0) ins.Parameters.Add("@" + name, type, size);
                else ins.Parameters.Add("@" + name, type);
            }

            Add("ItemId", SqlDbType.VarChar, 16);
            Add("SerialNum", SqlDbType.Int);
            Add("ButtonName", SqlDbType.VarChar, 64);
            Add("CustCaption", SqlDbType.NVarChar, 200);
            if (schema.HasCaptionE) Add("CustCaptionE", SqlDbType.NVarChar, 200);
            Add("CustHint", SqlDbType.NVarChar, -1);
            if (schema.HasHintE) Add("CustHintE", SqlDbType.NVarChar, -1);
            Add("OCXName", SqlDbType.VarChar, 128);
            Add("CoClassName", SqlDbType.VarChar, 128);
            Add("SpName", SqlDbType.VarChar, 128);
            Add("bVisible", SqlDbType.Int);
            Add(schema.ChkCanUpdateCol, SqlDbType.Int);
            Add("bNeedNum", SqlDbType.Int);
            Add("DesignType", SqlDbType.Int);
            // 擴充 string 參數
            foreach (var c in extStr) { if (schema.Has(c)) Add(c, SqlDbType.NVarChar, -1); }
            // 擴充 int 參數
            foreach (var c in extInt) { if (schema.Has(c)) Add(c, SqlDbType.Int); }

            foreach (var r in rows)
            {
                ins.Parameters["@ItemId"].Value = r.ItemId ?? itemId;
                ins.Parameters["@SerialNum"].Value = (object?)r.SerialNum ?? DBNull.Value;
                ins.Parameters["@ButtonName"].Value = r.ButtonName ?? "";

                ins.Parameters["@CustCaption"].Value = DbNull(r.CustCaption);
                if (schema.HasCaptionE) ins.Parameters["@CustCaptionE"].Value = DbNull(r.CustCaptionE);

                ins.Parameters["@CustHint"].Value = DbNull(r.CustHint);
                if (schema.HasHintE) ins.Parameters["@CustHintE"].Value = DbNull(r.CustHintE);

                ins.Parameters["@OCXName"].Value = DbNull(r.OCXName);
                ins.Parameters["@CoClassName"].Value = DbNull(r.CoClassName);
                ins.Parameters["@SpName"].Value = DbNull(r.SpName);

                ins.Parameters["@bVisible"].Value = (object?)r.bVisible ?? 0;
                ins.Parameters["@" + schema.ChkCanUpdateCol].Value = (object?)r.ChkCanUpdate ?? 0;
                ins.Parameters["@bNeedNum"].Value = (object?)r.bNeedNum ?? 0;
                ins.Parameters["@DesignType"].Value = r.DesignType ?? 0;

                // 擴充 string 欄位賦值（資料庫不允許 NULL，預設空字串）
                if (schema.Has("SearchTemplate")) ins.Parameters["@SearchTemplate"].Value = r.SearchTemplate ?? "";
                if (schema.Has("MultiSelectDD")) ins.Parameters["@MultiSelectDD"].Value = r.MultiSelectDD ?? "";
                if (schema.Has("ExecSpName")) ins.Parameters["@ExecSpName"].Value = r.ExecSpName ?? "";
                if (schema.Has("DialogCaption")) ins.Parameters["@DialogCaption"].Value = r.DialogCaption ?? "";
                if (schema.Has("PrintSQL")) ins.Parameters["@PrintSQL"].Value = r.PrintSQL ?? "";
                if (schema.Has("PrintSp")) ins.Parameters["@PrintSp"].Value = r.PrintSp ?? "";
                if (schema.Has("PrintRptName")) ins.Parameters["@PrintRptName"].Value = r.PrintRptName ?? "";
                if (schema.Has("GetPohtoSQL")) ins.Parameters["@GetPohtoSQL"].Value = r.GetPohtoSQL ?? "";
                if (schema.Has("NewPohtoName")) ins.Parameters["@NewPohtoName"].Value = r.NewPohtoName ?? "";
                if (schema.Has("MultiSelDtlSQL")) ins.Parameters["@MultiSelDtlSQL"].Value = r.MultiSelDtlSQL ?? "";
                if (schema.Has("MultiSelDtlDDName")) ins.Parameters["@MultiSelDtlDDName"].Value = r.MultiSelDtlDDName ?? "";
                if (schema.Has("MultiSelDtlKeyName")) ins.Parameters["@MultiSelDtlKeyName"].Value = r.MultiSelDtlKeyName ?? "";
                if (schema.Has("EditGridTableKind")) ins.Parameters["@EditGridTableKind"].Value = r.EditGridTableKind ?? "";
                if (schema.Has("EditGridTableField")) ins.Parameters["@EditGridTableField"].Value = r.EditGridTableField ?? "";
                // 擴充 int 欄位賦值（資料庫不允許 NULL，預設 0）
                if (schema.Has("bSpHasResult")) ins.Parameters["@bSpHasResult"].Value = r.bSpHasResult ?? 0;
                if (schema.Has("AllowSelCount")) ins.Parameters["@AllowSelCount"].Value = r.AllowSelCount ?? 0;
                if (schema.Has("ReplaceExists")) ins.Parameters["@ReplaceExists"].Value = r.ReplaceExists ?? 0;
                if (schema.Has("bTranByGlobalId")) ins.Parameters["@bTranByGlobalId"].Value = r.bTranByGlobalId ?? 0;
                if (schema.Has("iMultiSelUseDtl")) ins.Parameters["@iMultiSelUseDtl"].Value = r.iMultiSelUseDtl ?? 0;
                if (schema.Has("iEditGridType")) ins.Parameters["@iEditGridType"].Value = r.iEditGridType ?? 0;
                if (schema.Has("NeediFlag")) ins.Parameters["@NeediFlag"].Value = r.NeediFlag ?? 0;
                if (schema.Has("NeediSeqNum")) ins.Parameters["@NeediSeqNum"].Value = r.NeediSeqNum ?? 0;

                await ins.ExecuteNonQueryAsync();
            }

            await tx.CommitAsync();
            return Ok(new { success = true, count = rows.Count });
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    // POST /api/CustomButtonApi/SaveSearchParam
    [HttpPost("SaveSearchParam")]
    public async Task<IActionResult> SaveSearchParam([FromBody] SaveSearchParamDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.ItemId)
            || string.IsNullOrWhiteSpace(dto.ButtonName)
            || string.IsNullOrWhiteSpace(dto.ParamName))
            return BadRequest(new { success = false, message = "ItemId, ButtonName, ParamName required" });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        var sql = @"
UPDATE CURdOCXSearchParams
   SET CommandText = @CommandText,
       DefaultType = @DefaultType,
       DefaultValue = @DefaultValue,
       SuperId = @SuperId
 WHERE ItemId = @ItemId
   AND ButtonName = @ButtonName
   AND ParamName = @ParamName;";

        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@ItemId", dto.ItemId);
        cmd.Parameters.AddWithValue("@ButtonName", dto.ButtonName);
        cmd.Parameters.AddWithValue("@ParamName", dto.ParamName);
        cmd.Parameters.AddWithValue("@CommandText", (object?)dto.CommandText ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@DefaultType", (object?)dto.DefaultType ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@DefaultValue", (object?)dto.DefaultValue ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@SuperId", (object?)dto.SuperId ?? DBNull.Value);

        var affected = await cmd.ExecuteNonQueryAsync();
        if (affected == 0)
            return NotFound(new { success = false, message = "找不到該參數列" });

        return Ok(new { success = true, count = affected });
    }

    // POST /api/CustomButtonApi/FetchParams
    [HttpPost("FetchParams")]
    public async Task<IActionResult> FetchParams([FromBody] FetchParamsDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto?.ItemId) || string.IsNullOrWhiteSpace(dto.ButtonName) || string.IsNullOrWhiteSpace(dto.ObjectName))
            return BadRequest(new { success = false, message = "ItemId, ButtonName, ObjectName required" });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand("CURdOCXSearchParamsGet", conn);
        cmd.CommandType = System.Data.CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@ItemId", dto.ItemId);
        cmd.Parameters.AddWithValue("@ButtonName", dto.ButtonName);
        cmd.Parameters.AddWithValue("@ObjectName", dto.ObjectName);
        await cmd.ExecuteNonQueryAsync();

        return Ok(new { success = true });
    }

    public class FetchParamsDto
    {
        public string? ItemId { get; set; }
        public string? ButtonName { get; set; }
        public string? ObjectName { get; set; }
    }

    // POST /api/CustomButtonApi/SaveSearchParams (批次)
    [HttpPost("SaveSearchParams")]
    public async Task<IActionResult> SaveSearchParams([FromBody] SaveSearchParamsBatchDto batch)
    {
        if (batch == null || string.IsNullOrWhiteSpace(batch.ItemId)
            || string.IsNullOrWhiteSpace(batch.ButtonName)
            || batch.Rows == null)
            return BadRequest(new { success = false, message = "ItemId, ButtonName, Rows required" });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();
        await using var tx = await conn.BeginTransactionAsync(IsolationLevel.ReadCommitted);
        try
        {
            // 刪除該按鈕的所有 SearchParams
            var del = new SqlCommand(
                "DELETE FROM CURdOCXSearchParams WHERE ItemId=@id AND ButtonName=@btn;",
                conn, (SqlTransaction)tx);
            del.Parameters.AddWithValue("@id", batch.ItemId);
            del.Parameters.AddWithValue("@btn", batch.ButtonName);
            await del.ExecuteNonQueryAsync();

            if (batch.Rows.Count > 0)
            {
                // 偵測欄位是否存在
                var colSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                {
                    var schCmd = new SqlCommand(
                        "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='CURdOCXSearchParams'",
                        conn, (SqlTransaction)tx);
                    using var srd = await schCmd.ExecuteReaderAsync();
                    while (await srd.ReadAsync()) colSet.Add(srd.GetString(0));
                }

                var baseCols = new List<string>{
                    "ItemId","ButtonName","ParamName","DisplayName","CommandText",
                    "DefaultValue","DefaultType","TableKind","EditMask","SuperId",
                    "ParamSN","ParamValue","ParamType","iReadOnly","iVisible"
                };
                var extCols = new[]{"ControlType","iForInput","iDateTime",
                    "DisplayNameCN","DisplayNameEN","DisplayNameJP","DisplayNameTH"};
                foreach (var c in extCols) { if (colSet.Contains(c)) baseCols.Add(c); }

                var pList = baseCols.Select(c => "@" + c).ToList();
                var insertSql = $"INSERT INTO CURdOCXSearchParams({string.Join(",", baseCols)}) VALUES({string.Join(",", pList)});";

                var ins = new SqlCommand(insertSql, conn, (SqlTransaction)tx);
                foreach (var c in baseCols)
                {
                    var p = ins.Parameters.Add("@" + c, SqlDbType.NVarChar, -1);
                    // int 欄位用 Int
                    if (c is "DefaultType" or "ParamSN" or "ParamType" or "ControlType"
                        or "iReadOnly" or "iVisible" or "iForInput" or "iDateTime")
                        ins.Parameters["@" + c].SqlDbType = SqlDbType.Int;
                }

                foreach (var r in batch.Rows)
                {
                    ins.Parameters["@ItemId"].Value = batch.ItemId;
                    ins.Parameters["@ButtonName"].Value = batch.ButtonName;
                    ins.Parameters["@ParamName"].Value = r.ParamName ?? "";
                    ins.Parameters["@DisplayName"].Value = r.DisplayName ?? "";
                    ins.Parameters["@CommandText"].Value = r.CommandText ?? "";
                    ins.Parameters["@DefaultValue"].Value = r.DefaultValue ?? "";
                    ins.Parameters["@DefaultType"].Value = r.DefaultType ?? 0;
                    ins.Parameters["@TableKind"].Value = r.TableKind ?? "";
                    ins.Parameters["@EditMask"].Value = r.EditMask ?? "";
                    ins.Parameters["@SuperId"].Value = r.SuperId ?? "";
                    ins.Parameters["@ParamSN"].Value = r.ParamSN ?? 0;
                    ins.Parameters["@ParamValue"].Value = r.ParamValue ?? "";
                    ins.Parameters["@ParamType"].Value = r.ParamType ?? 0;
                    ins.Parameters["@iReadOnly"].Value = r.iReadOnly ?? 0;
                    ins.Parameters["@iVisible"].Value = r.iVisible ?? 0;
                    if (colSet.Contains("ControlType")) ins.Parameters["@ControlType"].Value = r.ControlType ?? (object)DBNull.Value;
                    if (colSet.Contains("iForInput")) ins.Parameters["@iForInput"].Value = r.iForInput ?? 0;
                    if (colSet.Contains("iDateTime")) ins.Parameters["@iDateTime"].Value = r.iDateTime ?? 0;
                    if (colSet.Contains("DisplayNameCN")) ins.Parameters["@DisplayNameCN"].Value = r.DisplayNameCN ?? "";
                    if (colSet.Contains("DisplayNameEN")) ins.Parameters["@DisplayNameEN"].Value = r.DisplayNameEN ?? "";
                    if (colSet.Contains("DisplayNameJP")) ins.Parameters["@DisplayNameJP"].Value = r.DisplayNameJP ?? "";
                    if (colSet.Contains("DisplayNameTH")) ins.Parameters["@DisplayNameTH"].Value = r.DisplayNameTH ?? "";
                    await ins.ExecuteNonQueryAsync();
                }
            }

            await tx.CommitAsync();
            return Ok(new { success = true, count = batch.Rows.Count });
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    public class SaveSearchParamsBatchDto
    {
        public string? ItemId { get; set; }
        public string? ButtonName { get; set; }
        public List<SearchParamRow> Rows { get; set; } = new();
    }

    // ====== DTOs ======
    public class SaveSearchParamDto
    {
        public string? ItemId { get; set; }
        public string? ButtonName { get; set; }
        public string? ParamName { get; set; }
        public string? CommandText { get; set; }
        public string? DefaultValue { get; set; }
        public string? SuperId { get; set; }
        public int? DefaultType { get; set; }
    }

public class SearchParamRow {
    public string? ItemId { get; set; }
    public string? ButtonName { get; set; }
    public string? ParamName { get; set; }
    public string? DisplayName { get; set; }
    public int?    ControlType { get; set; }
    public string? CommandText { get; set; }
    public string? DefaultValue { get; set; }
    public int?    DefaultType { get; set; }
    public string? TableKind { get; set; }
    public string? EditMask { get; set; }
    public string? SuperId { get; set; }
    public int?    ParamSN { get; set; }
    public string? ParamValue { get; set; }
    public int?    ParamType { get; set; }
    public string? ParamTypeName { get; set; }
    public int?    iReadOnly { get; set; }
    public int?    iVisible { get; set; }
    public int?    iForInput { get; set; }
    public int?    iDateTime { get; set; }
    public string? DisplayNameCN { get; set; }
    public string? DisplayNameEN { get; set; }
    public string? DisplayNameJP { get; set; }
    public string? DisplayNameTH { get; set; }
}

public class InsKeyRow {
    public string? ItemId { get; set; }
    public string? ButtonName { get; set; }
    public string? KeyFieldName { get; set; }
    public int?    SeqNum { get; set; }
    public int?    PositionType { get; set; }
    public string? PositionTypeName { get; set; }
}

public class CallSpParamRow {
    public string? ItemId { get; set; }
    public string? ButtonName { get; set; }
    public int?    SeqNum { get; set; }
    public string? TableKind { get; set; }
    public string? ParamFieldName { get; set; }
    public int?    ParamType { get; set; }
}

public class TranParamRow {
    public string? ItemId { get; set; }
    public string? ButtonName { get; set; }
    public int?    SeqNum { get; set; }
    public string? TableKind { get; set; }
    public string? ParamFieldName { get; set; }
    public int?    ParamType { get; set; }
}

public class ButtonDetailDto {
    public string ItemId { get; set; }
    public string ButtonName { get; set; }
    public List<SearchParamRow> SearchParams { get; set; } = new();
    public List<InsKeyRow> InsertKeys { get; set; } = new();
    public List<CallSpParamRow> CallSpParams { get; set; } = new();
    public List<TranParamRow> TranParams { get; set; } = new();
}

// GET /api/CustomButtonApi/Detail/{itemId}/{buttonName}
[HttpGet("Detail/{itemId}/{buttonName}")]
public async Task<IActionResult> GetDetail(string itemId, string buttonName)
{
    await using var conn = new SqlConnection(_connStr);
    await conn.OpenAsync();

    var dto = new ButtonDetailDto { ItemId = itemId, ButtonName = buttonName };

    // CURdOCXSearchParams
    try
    {
    var cmd1 = new SqlCommand(@"
SELECT s.ItemId, s.ButtonName, s.ParamName, s.DisplayName, s.ControlType, s.CommandText,
       s.DefaultValue, s.DefaultType, s.TableKind, s.EditMask, s.SuperId, s.ParamSN, s.ParamValue,
       s.ParamType, t.ParamTypeName,
       s.iReadOnly, s.iVisible, s.iForInput, s.iDateTime,
       s.DisplayNameCN, s.DisplayNameEN, s.DisplayNameJP, s.DisplayNameTH
FROM CURdOCXSearchParams s WITH (NOLOCK)
LEFT JOIN CURdOCXCusBtnParamType t WITH (NOLOCK)
       ON s.ParamType = t.ParamType
WHERE ItemId=@itemId AND ButtonName=@btn
ORDER BY ParamSN, ParamName;", conn);
    cmd1.Parameters.AddWithValue("@itemId", itemId ?? "");
    cmd1.Parameters.AddWithValue("@btn", buttonName ?? "");
    using (var rd = await cmd1.ExecuteReaderAsync())
    {
        while (await rd.ReadAsync())
        {
            dto.SearchParams.Add(new SearchParamRow{
                ItemId = rd["ItemId"]?.ToString(),
                ButtonName = rd["ButtonName"]?.ToString(),
                ParamName = rd["ParamName"]?.ToString(),
                DisplayName = rd["DisplayName"]?.ToString(),
                ControlType = TryToInt(rd["ControlType"]),
                CommandText = rd["CommandText"]?.ToString(),
                DefaultValue = rd["DefaultValue"]?.ToString(),
                DefaultType = TryToInt(rd["DefaultType"]),
                TableKind = rd["TableKind"]?.ToString(),
                EditMask = rd["EditMask"]?.ToString(),
                SuperId = rd["SuperId"]?.ToString(),
                ParamSN = TryToInt(rd["ParamSN"]),
                ParamValue = rd["ParamValue"]?.ToString(),
                ParamType = TryToInt(rd["ParamType"]),
                ParamTypeName = rd["ParamTypeName"]?.ToString(),
                iReadOnly = TryToInt(rd["iReadOnly"]),
                iVisible = TryToInt(rd["iVisible"]),
                iForInput = TryToInt(rd["iForInput"]),
                iDateTime = TryToInt(rd["iDateTime"]),
                DisplayNameCN = rd["DisplayNameCN"]?.ToString(),
                DisplayNameEN = rd["DisplayNameEN"]?.ToString(),
                DisplayNameJP = rd["DisplayNameJP"]?.ToString(),
                DisplayNameTH = rd["DisplayNameTH"]?.ToString(),
            });
        }
    }
    }
    catch (SqlException)
    {
        // 舊版資料庫欄位不足時，退回基本欄位查詢
        var fallback = new SqlCommand(@"
SELECT ItemId, ButtonName, ParamName, DisplayName, ControlType, CommandText,
       DefaultValue, DefaultType, TableKind, EditMask, SuperId, ParamSN, ParamValue,
       ParamType, iReadOnly, iVisible
  FROM CURdOCXSearchParams WITH (NOLOCK)
 WHERE ItemId=@itemId AND ButtonName=@btn
 ORDER BY ParamSN, ParamName;", conn);
        fallback.Parameters.AddWithValue("@itemId", itemId ?? "");
        fallback.Parameters.AddWithValue("@btn", buttonName ?? "");
        using (var rd = await fallback.ExecuteReaderAsync())
        {
            while (await rd.ReadAsync())
            {
                dto.SearchParams.Add(new SearchParamRow
                {
                    ItemId = rd["ItemId"]?.ToString(),
                    ButtonName = rd["ButtonName"]?.ToString(),
                    ParamName = rd["ParamName"]?.ToString(),
                    DisplayName = rd["DisplayName"]?.ToString(),
                    ControlType = TryToInt(rd["ControlType"]),
                    CommandText = rd["CommandText"]?.ToString(),
                    DefaultValue = rd["DefaultValue"]?.ToString(),
                    DefaultType = TryToInt(rd["DefaultType"]),
                    TableKind = rd["TableKind"]?.ToString(),
                    EditMask = rd["EditMask"]?.ToString(),
                    SuperId = rd["SuperId"]?.ToString(),
                    ParamSN = TryToInt(rd["ParamSN"]),
                    ParamValue = rd["ParamValue"]?.ToString(),
                    ParamType = TryToInt(rd["ParamType"]),
                    iReadOnly = TryToInt(rd["iReadOnly"]),
                    iVisible = TryToInt(rd["iVisible"])
                });
            }
        }
    }

    // CURdOCXItmCusBtnInsKey
    try
    {
    var cmd2 = new SqlCommand(@"
SELECT k.ItemId, k.ButtonName, k.KeyFieldName, k.SeqNum, k.PositionType,
       p.PositionTypeName
FROM CURdOCXItmCusBtnInsKey k WITH (NOLOCK)
LEFT JOIN CURdOCXCusBtnPositionType p WITH (NOLOCK)
       ON k.PositionType = p.PositionType
WHERE k.ItemId=@itemId AND k.ButtonName=@btn
ORDER BY k.SeqNum, k.KeyFieldName;", conn);
    cmd2.Parameters.AddWithValue("@itemId", itemId ?? "");
    cmd2.Parameters.AddWithValue("@btn", buttonName ?? "");
    using (var rd = await cmd2.ExecuteReaderAsync())
    {
        while (await rd.ReadAsync())
        {
            dto.InsertKeys.Add(new InsKeyRow{
                ItemId = rd["ItemId"]?.ToString(),
                ButtonName = rd["ButtonName"]?.ToString(),
                KeyFieldName = rd["KeyFieldName"]?.ToString(),
                SeqNum = TryToInt(rd["SeqNum"]),
                PositionType = TryToInt(rd["PositionType"]),
                PositionTypeName = rd["PositionTypeName"]?.ToString(),
            });
        }
    }
    }
    catch (SqlException)
    {
        var fallback = new SqlCommand(@"
SELECT ItemId, ButtonName, KeyFieldName, SeqNum, PositionType
FROM CURdOCXItmCusBtnInsKey WITH (NOLOCK)
WHERE ItemId=@itemId AND ButtonName=@btn
ORDER BY SeqNum, KeyFieldName;", conn);
        fallback.Parameters.AddWithValue("@itemId", itemId ?? "");
        fallback.Parameters.AddWithValue("@btn", buttonName ?? "");
        using (var rd = await fallback.ExecuteReaderAsync())
        {
            while (await rd.ReadAsync())
            {
                dto.InsertKeys.Add(new InsKeyRow
                {
                    ItemId = rd["ItemId"]?.ToString(),
                    ButtonName = rd["ButtonName"]?.ToString(),
                    KeyFieldName = rd["KeyFieldName"]?.ToString(),
                    SeqNum = TryToInt(rd["SeqNum"]),
                    PositionType = TryToInt(rd["PositionType"]),
                    PositionTypeName = string.Empty
                });
            }
        }
    }

    // CURdOCXItmCusBtnParam (呼叫 SP 參數)
    try
    {
    var cmd3 = new SqlCommand(@"
SELECT ItemId, ButtonName, SeqNum, TableKind, ParamFieldName, ParamType
FROM CURdOCXItmCusBtnParam WITH (NOLOCK)
WHERE ItemId=@itemId AND ButtonName=@btn
ORDER BY SeqNum, ParamFieldName;", conn);
    cmd3.Parameters.AddWithValue("@itemId", itemId ?? "");
    cmd3.Parameters.AddWithValue("@btn", buttonName ?? "");
    using (var rd = await cmd3.ExecuteReaderAsync())
    {
        while (await rd.ReadAsync())
        {
            dto.CallSpParams.Add(new CallSpParamRow{
                ItemId = rd["ItemId"]?.ToString(),
                ButtonName = rd["ButtonName"]?.ToString(),
                SeqNum = TryToInt(rd["SeqNum"]),
                TableKind = rd["TableKind"]?.ToString(),
                ParamFieldName = rd["ParamFieldName"]?.ToString(),
                ParamType = TryToInt(rd["ParamType"])
            });
        }
    }
    }
    catch (SqlException)
    {
        // 某些舊版 DB 可能不存在此資料表，忽略並回空清單
    }

    // CURdOCXItmCusBtnTranPm (傳送資料參數)
    try
    {
    var cmd4 = new SqlCommand(@"
SELECT ItemId, ButtonName, SeqNum, TableKind, ParamFieldName, ParamType
FROM CURdOCXItmCusBtnTranPm WITH (NOLOCK)
WHERE ItemId=@itemId AND ButtonName=@btn
ORDER BY SeqNum, ParamFieldName;", conn);
    cmd4.Parameters.AddWithValue("@itemId", itemId ?? "");
    cmd4.Parameters.AddWithValue("@btn", buttonName ?? "");
    using (var rd = await cmd4.ExecuteReaderAsync())
    {
        while (await rd.ReadAsync())
        {
            dto.TranParams.Add(new TranParamRow{
                ItemId = rd["ItemId"]?.ToString(),
                ButtonName = rd["ButtonName"]?.ToString(),
                SeqNum = TryToInt(rd["SeqNum"]),
                TableKind = rd["TableKind"]?.ToString(),
                ParamFieldName = rd["ParamFieldName"]?.ToString(),
                ParamType = TryToInt(rd["ParamType"])
            });
        }
    }
    }
    catch (SqlException)
    {
        // 某些舊版 DB 可能不存在此資料表，忽略並回空清單
    }

    return Ok(dto);
}



}
