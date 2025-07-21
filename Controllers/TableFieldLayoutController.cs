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

    public TableFieldLayoutController(PcbErpContext context)
    {
        _context = context;
    }

    [HttpPost("SaveHeaderLayout")]
    public async Task<IActionResult> SaveHeaderLayout([FromBody] SaveHeaderLayoutRequest request)
    {
        if (request == null)
            return BadRequest("Invalid request format");

        var tableName = request.TableName;
        var layoutDict = request.LayoutUpdates
            .Select((x, i) => new { x.FieldName, x.Width, x.Height, x.Top, x.Left  })
            .ToDictionary(x => x.FieldName.ToLower(), x => x);

        // 1. 查出所有欄位名稱
        var allFieldNames = await _context.CURdTableFields
            .Where(x => x.TableName == tableName)
            .Select(x => x.FieldName)
            .ToListAsync();

        // 3. 準備排序順序
        int serial = 0;
        var sortedFieldNames = layoutDict.Keys.ToList();
        sortedFieldNames.AddRange(allFieldNames
            .Where(fn => !layoutDict.ContainsKey(fn.ToLower()))
            .Select(fn => fn.ToLower()));

        // 4. 一筆一筆更新（不經過 EF tracking，可避免 Trigger OUTPUT 問題）
        foreach (var fieldName in sortedFieldNames.Distinct())
        {
            if (!layoutDict.TryGetValue(fieldName, out var layout))
            {
                layout = new { FieldName = fieldName, Width = 160, Height = 22, Top = 0, Left = 0 };
            }

            await _context.Database.ExecuteSqlRawAsync(@"
                UPDATE CURdTableField
                SET  iFieldWidth = @Width, iFieldHeight = @Height,
                iFieldTop = @Top, iFieldLeft = @Left 
                WHERE LOWER(FieldName) = @FieldName AND TableName = @TableName",
                new[] {
                    new SqlParameter("@Width", layout.Width),
                    new SqlParameter("@Height", layout.Height),
                    new SqlParameter("@FieldName", fieldName),
                    new SqlParameter("@TableName", tableName),
                    new SqlParameter("@Top", layout.Top),
                    new SqlParameter("@Left", layout.Left)
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
        public int Height { get; set; } // 👈 加入欄位高度
        public int Top { get; set; }
        public int Left { get; set; }

    }


    public class HeaderLayoutDto
    {
        public string FieldName { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int SerialNum { get; set; }
        public int Top { get; set; }
        public int Left { get; set; }

    }

}
