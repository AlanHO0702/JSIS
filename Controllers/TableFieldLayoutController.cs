using System.Data;
using Microsoft.AspNetCore.Identity;
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
        _connStr = config.GetConnectionString("DefaultConnection");
    }

   [HttpPost("SaveHeaderLayout")]
    public async Task<IActionResult> SaveHeaderLayout([FromBody] SaveHeaderLayoutRequest request)
    {
        if (request == null)
            return BadRequest("Invalid request format");

        var tableName = request.TableName;
        var layoutDict = request.LayoutUpdates
            .Select(x => new { x.FieldName, x.Width, x.Height, x.Top, x.Left, x.iShowWhere })
            .ToDictionary(x => x.FieldName.ToLower(), x => x);

        // 只更新傳入的欄位
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

                // 更新資料庫（此處如果 EF tracking 不需要，也可整合為一筆）
                await _context.Database.ExecuteSqlRawAsync(@"
                    UPDATE CURdTableField
                    SET  iFieldWidth = @Width, iFieldHeight = @Height,
                        iFieldTop = @Top, iFieldLeft = @Left ,
                        iShowWhere = @iShowWhere
                    WHERE LOWER(FieldName) = @FieldName AND TableName = @TableName",
                    new[] {
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

        return Ok(new { success = true }); // ✅ 回傳 JSON 給前端
    }


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
                    // 多個 result 欄位
                    for (int i = 0; i < resultFields.Length; i++)
                    {
                        row[$"result{i}"] = reader[$"result{i}"];
                    }
                    list.Add(row);
                }
            }
        }
        return Ok(list);
    }

    public class DetailColWidthDto
    {
        public string tableName { get; set; } = "";
        public List<ColItem> cols { get; set; } = new();
        public class ColItem { public string fieldName { get; set; } = ""; public int width { get; set; } }
    }

[HttpPost("SaveDetailLayout")]
public async Task<IActionResult> SaveDetailLayout([FromBody] DetailColWidthDto dto)
{
    if (string.IsNullOrWhiteSpace(dto.tableName))
        return BadRequest("tableName is required");
    if (dto.cols == null || dto.cols.Count == 0)
        return Ok(new { ok = true, updated = 0 });

    // 正規化表名與欄名（去 []/dbo.、小寫）
    static string Clean(string s) => (s ?? "")
        .Trim()
        .Trim('[', ']')
        .Replace("dbo.", "", StringComparison.OrdinalIgnoreCase);

    var tname = Clean(dto.tableName).ToLowerInvariant();

    // 去重、保留最終寬度
    var items = dto.cols
        .Where(c => !string.IsNullOrWhiteSpace(c.fieldName))
        .Select(c => new { field = Clean(c.fieldName).ToLowerInvariant(), width = Math.Max(50, c.width) })
        .GroupBy(x => x.field)
        .Select(g => g.Last())       // 同欄位最後一次為主
        .ToList();

    int updated = 0;

    // 逐欄位用參數化 SQL 更新（容忍 TableName 有/沒有 dbo.）
    const string sql = @"
UPDATE CURdTableField
SET iFieldWidth = @W
WHERE LOWER(FieldName) = @F
  AND (
        LOWER(TableName) = @TN               -- 'spodordersub'
     OR LOWER(REPLACE(TableName,'dbo.','')) = @TN
     )";

    foreach (var it in items)
    {
        var n = await _context.Database.ExecuteSqlRawAsync(
            sql,
            new SqlParameter("@W", it.width),
            new SqlParameter("@F", it.field),
            new SqlParameter("@TN", tname)
        );
        // 不管影響列數是否為 0，都視為一次更新嘗試；你也可改成 if (n > 0) updated++;
        updated += n;
    }

    return Ok(new { ok = true, updated, table = tname, fields = items.Select(i => new { i.field, i.width }) });
}


    // 專用 DTO 類別
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
        public int Height { get; set; } // 👈 加入欄位高度
        public int Top { get; set; }
        public int Left { get; set; }
        public int iShowWhere { get; set; }  // 👈➕ 加上這行

    }


    public class HeaderLayoutDto
    {
        public string FieldName { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int SerialNum { get; set; }
        public int Top { get; set; }
        public int Left { get; set; }
        public int iShowWhere { get; set; }  // 👈➕ 加上這行

    }

}
