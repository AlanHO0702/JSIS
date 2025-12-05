using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;

[Route("api/[controller]")]
[ApiController]
public class TableFieldLayoutController : ControllerBase
{
    private readonly PcbErpContext _context;
    private readonly string _connStr;

    public TableFieldLayoutController(PcbErpContext context, IConfiguration config)
    {
        _context = context;
        _connStr = config.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("DefaultConnection string is missing in configuration.");
    }

    // =========================================
    // 儲存 Header Layout
    // =========================================
    [HttpPost("SaveHeaderLayout")]
    public async Task<IActionResult> SaveHeaderLayout([FromBody] SaveHeaderLayoutRequest request)
    {
        if (request == null)
            return BadRequest("Invalid request format");

        var tableName = request.TableName;
        var layoutDict = request.LayoutUpdates
            .Select(x => new { x.FieldName, x.Width, x.Height, x.Top, x.Left, x.iShowWhere })
            .ToDictionary(x => x.FieldName.ToLower(), x => x);

        foreach (var layout in layoutDict.Values)
        {
            var entity = await _context.CURdTableFields
                .FirstOrDefaultAsync(f => f.TableName == tableName && f.FieldName.ToLower() == layout.FieldName.ToLower());

            if (entity != null)
            {
                bool skipPositionUpdate = layout.Top == 0 && layout.Left == 0
                    && (entity.iFieldTop ?? 0) != 0 && (entity.iFieldLeft ?? 0) != 0;

                if (!skipPositionUpdate)
                {
                    entity.iFieldTop = layout.Top;
                    entity.iFieldLeft = layout.Left;
                }

                entity.iFieldWidth = layout.Width;
                entity.iFieldHeight = layout.Height;

                await _context.Database.ExecuteSqlRawAsync(@"
                    UPDATE CURdTableField
                    SET  iFieldWidth = @Width, iFieldHeight = @Height,
                         iFieldTop = @Top, iFieldLeft = @Left,
                         iShowWhere = @iShowWhere
                    WHERE LOWER(FieldName) = @FieldName AND TableName = @TableName",
                    new[]
                    {
                        new SqlParameter("@Width", layout.Width),
                        new SqlParameter("@Height", layout.Height),
                        new SqlParameter("@FieldName", layout.FieldName),
                        new SqlParameter("@TableName", tableName),
                        new SqlParameter("@Top", layout.Top),
                        new SqlParameter("@Left", layout.Left),
                        new SqlParameter("@iShowWhere", layout.iShowWhere)
                    });
            }
        }

        return Ok();
    }

    // =========================================
    // 儲存欄位排序
    // =========================================
    [HttpPost("SaveSerialOrder")]
    public async Task<IActionResult> SaveSerialOrder([FromBody] SaveSerialOrderRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.TableName))
            return BadRequest("Invalid request");

        foreach (var field in request.FieldOrders)
        {
            await _context.Database.ExecuteSqlRawAsync(@"
                UPDATE CURdTableField
                SET SerialNum = @SerialNum
                WHERE LOWER(FieldName) = @FieldName AND TableName = @TableName",
                new[]
                {
                    new SqlParameter("@SerialNum", field.SerialNum),
                    new SqlParameter("@FieldName", field.FieldName),
                    new SqlParameter("@TableName", request.TableName)
                });
        }

        return Ok(new { success = true });
    }

    // =========================================
    // LookupData
    // =========================================
    [HttpGet("LookupData")]
    public async Task<IActionResult> GetLookupData(string table, string key, string result)
    {
        bool IsValidCol(string s)
            => s.Split(',').All(part => System.Text.RegularExpressions.Regex.IsMatch(part.Trim(), @"^[A-Za-z0-9_]+$"));

        if (!IsValidCol(key) || !IsValidCol(result))
            return BadRequest("Invalid column!");

        var resultFields = result.Split(',').Select(x => $"[{x.Trim()}]").ToArray();
        string selectResult = string.Join(", ", resultFields.Select((col, idx) => $"{col} as [result{idx}]"));
        var sql = $"SELECT [{key.Trim()}] as [key], {selectResult} FROM [{table.Trim()}]";

        var list = new List<Dictionary<string, object>>();
        using (var conn = new SqlConnection(_connStr))
        using (var cmd = new SqlCommand(sql, conn))
        {
            await conn.OpenAsync();
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var row = new Dictionary<string, object>();
                    row["key"] = reader["key"];
                    for (int i = 0; i < resultFields.Length; i++)
                        row[$"result{i}"] = reader[$"result{i}"];
                    list.Add(row);
                }
            }
        }
        return Ok(list);
    }

    // =========================================
    // 儲存 Detail Layout
    // =========================================
    [HttpPost("SaveDetailLayout")]
    public async Task<IActionResult> SaveDetailLayout([FromBody] DetailColWidthDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.tableName))
            return BadRequest("tableName is required");
        if (dto.cols == null || dto.cols.Count == 0)
            return Ok(new { ok = true, updated = 0 });

        static string Clean(string s) => (s ?? "")
            .Trim().Trim('[', ']')
            .Replace("dbo.", "", StringComparison.OrdinalIgnoreCase);

        var tname = Clean(dto.tableName).ToLowerInvariant();

        var items = dto.cols
            .Where(c => !string.IsNullOrWhiteSpace(c.fieldName))
            .Select(c => new { field = Clean(c.fieldName).ToLowerInvariant(), width = Math.Max(50, c.width) })
            .GroupBy(x => x.field)
            .Select(g => g.Last())
            .ToList();

        int updated = 0;

        const string sql = @"
UPDATE CURdTableField
SET iFieldWidth = @W, DisplaySize = @DS
WHERE LOWER(FieldName) = @F
  AND (
        LOWER(TableName) = @TN
     OR LOWER(REPLACE(TableName,'dbo.','')) = @TN
     )";

        const string sqlLang = @"
UPDATE CURdTableFieldLang
SET DisplaySize = @DS
WHERE LOWER(FieldName) = @F
  AND (
        LOWER(TableName) = @TN
     OR LOWER(REPLACE(TableName,'dbo.','')) = @TN
     )";

        foreach (var it in items)
        {
            // 以 px 寬度換算 DisplaySize（字數）：目前 1 字 = 10px 的策略
            var displaySize = Math.Max(1, (int)Math.Round(it.width / 10.0, MidpointRounding.AwayFromZero));

            var n = await _context.Database.ExecuteSqlRawAsync(
                sql,
                new SqlParameter("@W", it.width),
                new SqlParameter("@DS", displaySize),
                new SqlParameter("@F", it.field),
                new SqlParameter("@TN", tname)
            );
            // 同步更新語系表，確保 DisplaySize 也被帶入
            await _context.Database.ExecuteSqlRawAsync(
                sqlLang,
                new SqlParameter("@DS", displaySize),
                new SqlParameter("@F", it.field),
                new SqlParameter("@TN", tname)
            );

            updated += n;
        }

        return Ok(new { ok = true, updated, table = tname, fields = items.Select(i => new { i.field, i.width }) });
    }

    // =========================================
    // 取得 DictFields (帶語系, 唯一版本)
    // =========================================
    [HttpGet("DictFields")]
    public async Task<IActionResult> GetDictFields([FromQuery] string table, [FromQuery] string lang = "TW")
    {
        if (string.IsNullOrWhiteSpace(table))
            return BadRequest("table is required.");

        static string Clean(string s) => (s ?? "")
            .Trim().Trim('[', ']')
            .Replace("dbo.", "", StringComparison.OrdinalIgnoreCase)
            .ToLowerInvariant();

        var tname = Clean(table);
        var lname = string.IsNullOrWhiteSpace(lang) ? "TW" : lang.Trim();

        const string SQL = @"
SELECT
    f.FieldName,
    COALESCE(l.DisplayLabel, f.DisplayLabel, f.FieldName) AS DisplayLabel,
    COALESCE(l.DisplaySize, f.DisplaySize) AS DisplaySize,
    f.DataType,
    f.FormatStr,
    f.SerialNum,
    Visible = CASE WHEN ISNULL(f.Visible,1)=1 THEN 1 ELSE 0 END
FROM CURdTableField f WITH (NOLOCK)
LEFT JOIN CURdTableFieldLang l WITH (NOLOCK)
       ON l.TableName = f.TableName
      AND l.FieldName = f.FieldName
      AND l.LanguageId = @Lang
WHERE ISNULL(f.Visible,1)=1
  AND (LOWER(f.TableName)=@TN OR LOWER(REPLACE(f.TableName,'dbo.',''))=@TN)
ORDER BY CASE WHEN f.SerialNum IS NULL THEN 1 ELSE 0 END, f.SerialNum, f.FieldName;";

        var list = new List<DictFieldDto>();
        await using var cn = new SqlConnection(_connStr);
        await cn.OpenAsync();
        await using var cmd = new SqlCommand(SQL, cn);
        cmd.Parameters.Add(new SqlParameter("@TN", tname));
        cmd.Parameters.Add(new SqlParameter("@Lang", lname));
        using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            list.Add(new DictFieldDto {
                FieldName    = rd["FieldName"]?.ToString() ?? "",
                DisplayLabel = rd["DisplayLabel"]?.ToString() ?? "",
                DisplaySize  = rd["DisplaySize"] as int?,
                DataType     = rd["DataType"]?.ToString() ?? "",
                FormatStr    = rd["FormatStr"]?.ToString() ?? "",
                SerialNum    = rd["SerialNum"] as int?,
                Visible      = (rd["Visible"]?.ToString() ?? "1") == "1"
            });
        }

        return Ok(list);
    }

    // =========================================
    // DTOs
    // =========================================
    public class DetailColWidthDto
    {
        public string tableName { get; set; } = "";
        public List<ColItem> cols { get; set; } = new();
        public class ColItem { public string fieldName { get; set; } = ""; public int width { get; set; } }
    }

    public class SaveSerialOrderRequest
    {
        public string TableName { get; set; } = "";
        public List<FieldSerialUpdate> FieldOrders { get; set; } = new();
    }
    public class FieldSerialUpdate
    {
        public string FieldName { get; set; } = "";
        public int SerialNum { get; set; }
    }

    public class SaveHeaderLayoutRequest
    {
        public string TableName { get; set; } = "";
        public List<FieldLayoutUpdate> LayoutUpdates { get; set; } = new();
    }
    public class FieldLayoutUpdate
    {
        public string FieldName { get; set; } = "";
        public int Width { get; set; }
        public int Height { get; set; }
        public int Top { get; set; }
        public int Left { get; set; }
        public int iShowWhere { get; set; }
    }

    public class HeaderLayoutDto
    {
        public string FieldName { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int SerialNum { get; set; }
        public int Top { get; set; }
        public int Left { get; set; }
        public int iShowWhere { get; set; }
    }

    public class DictFieldDto
    {
        public string FieldName { get; set; } = "";
        public string DisplayLabel { get; set; } = "";
        public int? DisplaySize { get; set; }
        public string DataType { get; set; } = "";
        public string FormatStr { get; set; } = "";
        public int? SerialNum { get; set; }
        public bool Visible { get; set; }
    }

    [HttpGet("GetTableFields")]
    public Task<IActionResult> GetTableFields([FromQuery] string? tableName, [FromQuery] string? table, [FromQuery] string lang = "TW")
    {
        var t = (tableName ?? table ?? "").Trim();
    return GetTableFieldsFull(t, lang);
    }

    // 取辭典（完整欄位版本：含版面、查詢設定等）
    [HttpGet("GetTableFieldsFull")]
    public async Task<IActionResult> GetTableFieldsFull(
        [FromQuery] string? table,
        [FromQuery] string? tableName,
        [FromQuery] string lang = "TW")
    {
         var t = (table ?? tableName ?? "").Trim();
        if (string.IsNullOrWhiteSpace(t))
            return BadRequest("table is required.");
        
        static string Clean(string s) => (s ?? "")
            .Trim().Trim('[', ']')
            .Replace("dbo.", "", StringComparison.OrdinalIgnoreCase)
            .ToLowerInvariant();

        var tname = Clean(t);
        var lname = string.IsNullOrWhiteSpace(lang) ? "TW" : lang.Trim();

        const string SQL = @"
    SELECT
        f.TableName,
        f.FieldName,
        COALESCE(l.DisplayLabel, f.DisplayLabel, f.FieldName) AS DisplayLabel,
        COALESCE(l.DisplaySize, f.DisplaySize) AS DisplaySize,
        f.DataType,
        f.FormatStr,
        f.SerialNum,
        Visible   = CASE WHEN ISNULL(f.Visible,1)=1 THEN 1 ELSE 0 END,
        ReadOnly  = CASE WHEN ISNULL(f.ReadOnly,0)=1 THEN 1 ELSE 0 END,
        f.ComboStyle,
        f.FieldNote,

        -- 標籤/欄位座標與尺寸
        f.iLabHeight,  f.iLabTop,   f.iLabLeft,   f.iLabWidth,
        f.iFieldHeight,f.iFieldTop, f.iFieldLeft, f.iFieldWidth,
        f.iShowWhere,

        -- 查詢/Lookup
        f.LookupTable, f.LookupKeyField, f.LookupResultField,

        -- 其他（若你的表裡有）
        f.IsNotesField,

        -- ★ 第二層 OCX Lookup（新增）
        f.OCXLKTableName,
        f.OCXLKResultName, 
        lk.KeyFieldName,
        lk.KeySelfName

    FROM CURdTableField f WITH (NOLOCK)
    LEFT JOIN CURdTableFieldLang l WITH (NOLOCK)
        ON l.TableName = f.TableName
        AND l.FieldName = f.FieldName
        AND l.LanguageId = @Lang

    LEFT JOIN CURdOCXTableFieldLK lk WITH (NOLOCK)
       ON lk.TableName = f.TableName
       AND lk.FieldName = f.FieldName

    WHERE (LOWER(f.TableName)=@TN OR LOWER(REPLACE(f.TableName,'dbo.',''))=@TN)
    ORDER BY CASE WHEN f.SerialNum IS NULL THEN 1 ELSE 0 END, f.SerialNum, f.FieldName;";

        var list = new List<object>();
        await using var cn = new SqlConnection(_connStr);
        await cn.OpenAsync();
        await using var cmd = new SqlCommand(SQL, cn);
        cmd.Parameters.Add(new SqlParameter("@TN", tname));
        cmd.Parameters.Add(new SqlParameter("@Lang", lname));

        using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            list.Add(new {
                TableName       = rd["TableName"]?.ToString() ?? "",
                FieldName       = rd["FieldName"]?.ToString() ?? "",
                DisplayLabel    = rd["DisplayLabel"]?.ToString() ?? "",
                DisplaySize     = rd["DisplaySize"] as int?,
                DataType        = rd["DataType"]?.ToString() ?? "",
                FormatStr       = rd["FormatStr"]?.ToString() ?? "",
                SerialNum       = rd["SerialNum"] as int?,
                Visible         = (rd["Visible"]?.ToString() ?? "1") == "1" ? 1 : 0,
                ReadOnly        = (rd["ReadOnly"]?.ToString() ?? "0") == "1" ? 1 : 0,
                FieldNote       = rd["FieldNote"]?.ToString() ?? "",
                ComboStyle      = rd["ComboStyle"] as int?,

                iLabHeight      = rd["iLabHeight"]  as int?,
                iLabTop         = rd["iLabTop"]     as int?,
                iLabLeft        = rd["iLabLeft"]    as int?,
                iLabWidth       = rd["iLabWidth"]   as int?,

                iFieldHeight    = rd["iFieldHeight"] as int?,
                iFieldTop       = rd["iFieldTop"]    as int?,
                iFieldLeft      = rd["iFieldLeft"]   as int?,
                iFieldWidth     = rd["iFieldWidth"]  as int?,
                iShowWhere      = rd["iShowWhere"]   as int?,

                LookupTable     = rd["LookupTable"]?.ToString() ?? "",
                LookupKeyField  = rd["LookupKeyField"]?.ToString() ?? "",
                LookupResultField = rd["LookupResultField"]?.ToString() ?? "",
                IsNotesField    = rd["IsNotesField"]?.ToString() ?? "",

                OCXLKTableName  = rd["OCXLKTableName"]?.ToString() ?? "",
                OCXLKResultName = rd["OCXLKResultName"]?.ToString() ?? "",
                KeyFieldName    = rd["KeyFieldName"]?.ToString() ?? "",
                KeySelfName     = rd["KeySelfName"]?.ToString() ?? ""
            });
        }

        return Ok(list);
    }

}
