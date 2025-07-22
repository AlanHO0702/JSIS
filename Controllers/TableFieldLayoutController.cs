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
            .Select(x => new { x.FieldName, x.Width, x.Height, x.Top, x.Left })
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
                        iFieldTop = @Top, iFieldLeft = @Left 
                    WHERE LOWER(FieldName) = @FieldName AND TableName = @TableName",
                    new[] {
                        new SqlParameter("@Width", layout.Width),
                        new SqlParameter("@Height", layout.Height),
                        new SqlParameter("@FieldName", layout.FieldName),
                        new SqlParameter("@TableName", tableName),
                        new SqlParameter("@Top", layout.Top),
                        new SqlParameter("@Left", layout.Left)
                    });
            }
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
