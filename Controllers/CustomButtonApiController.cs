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
        public string ItemId { get; set; }
        public int? SerialNum { get; set; }
        public string ButtonName { get; set; }

        public string CustCaption { get; set; }
        public string CustCaptionE { get; set; }   // 可能不存在，會回傳空字串
        public string CustHint { get; set; }
        public string CustHintE { get; set; }      // 可能不存在，會回傳空字串

        public string OCXName { get; set; }
        public string CoClassName { get; set; }
        public string SpName { get; set; }

        public int? bVisible { get; set; }
        public int? ChkCanUpdate { get; set; }   // 會對應到實際欄位（ChkCanUpdate 或 ChkCanbUpdate）
        public int? bNeedNum { get; set; }
        public int? DesignType { get; set; }
        public int? bSpHasResult { get; set; }
        public string SearchTemplate { get; set; }
        public string MultiSelectDD { get; set; }
        public string ExecSpName { get; set; }
        public int? AllowSelCount { get; set; }
        public int? ReplaceExists { get; set; }
        public string DialogCaption { get; set; }
        public int? bTranByGlobalId { get; set; }
        public string PrintSQL { get; set; }
        public string PrintSp { get; set; }
        public string PrintRptName { get; set; }
        public string GetPohtoSQL { get; set; }
        public string NewPohtoName { get; set; }
        public int? iMultiSelUseDtl { get; set; }
        public string MultiSelDtlSQL { get; set; }
        public string MultiSelDtlDDName { get; set; }
        public string MultiSelDtlKeyName { get; set; }
        public int? iEditGridType { get; set; }
        public string EditGridTableKind { get; set; }
        public string EditGridTableField { get; set; }
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
                ins.Parameters["@DesignType"].Value = DbNull(r.DesignType);

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
    
    // ====== DTOs ======
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
