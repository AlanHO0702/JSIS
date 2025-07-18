using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;


[Route("api/[controller]")]
[ApiController]
public class TableFieldLayoutController : ControllerBase
{
    private readonly PcbErpContext _context;

    public TableFieldLayoutController(PcbErpContext context)
    {
        _context = context;
    }
    
[HttpPost("SaveHeaderLayout")]
public async Task<IActionResult> SaveHeaderLayout([FromBody] SaveHeaderLayoutRequest request)
{
    var tableName = request.TableName;
    var layoutDict = request.LayoutUpdates
        .Select((x, i) => new { x.FieldName, x.Width, SerialNum = i })
        .ToDictionary(x => x.FieldName.ToLower(), x => x);

    // 1. 查出所有欄位
    var allFieldNames = await _context.CURdTableFields
        .Where(x => x.TableName == tableName)
        .Select(x => x.FieldName)
        .ToListAsync();

    // 2. 全部清成 NULL，避免留有歷史殘留排序
    var clearSql = @"UPDATE CURdTableField SET SerialNum = NULL WHERE TableName = @TableName";
    await _context.Database.ExecuteSqlRawAsync(clearSql, new[] {
        new SqlParameter("@TableName", tableName)
    });

    // 3. 建立新的排序（先 layout 的、後其餘）
    int serial = 0;
    var sortedFieldNames = layoutDict.Keys.ToList();

    // 把 layout 外的欄位也加入最後
    sortedFieldNames.AddRange(allFieldNames
        .Where(fn => !layoutDict.ContainsKey(fn.ToLower()))
        .Select(fn => fn.ToLower()));

    foreach (var fieldName in sortedFieldNames.Distinct())
    {
        var width = layoutDict.ContainsKey(fieldName) ? layoutDict[fieldName].Width : 160;

        var updateSql = @"
            UPDATE CURdTableField
            SET SerialNum = @SerialNum, iFieldWidth = @Width
            WHERE LOWER(FieldName) = @FieldName AND TableName = @TableName";

        await _context.Database.ExecuteSqlRawAsync(updateSql, new[] {
            new SqlParameter("@SerialNum", serial++),
            new SqlParameter("@Width", width),
            new SqlParameter("@FieldName", fieldName),
            new SqlParameter("@TableName", tableName)
        });
    }

    return Ok();
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
}

}
