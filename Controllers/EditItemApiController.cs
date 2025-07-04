using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Models;

[ApiController]
[Route("api/[controller]")]
public class EditItemApiController : ControllerBase
{
    private readonly CirContext _context;
    public EditItemApiController(CirContext context) => _context = context;

    public class EditItemRequest
    {
        public string ItemId { get; set; }
        public string ItemName { get; set; }
        public string SuperId { get; set; } // 加入 SuperId
        public int ItemType { get; set; }
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] EditItemRequest req)
    {
        var item = await _context.CurdSysItems.FirstOrDefaultAsync(x => x.ItemId == req.ItemId);
        if (item == null)
            return NotFound(new { success = false, message = "找不到該項目" });

            item.ItemName = req.ItemName.Trim();
            item.SuperId = req.SuperId?.Trim();
            item.ItemType = req.ItemType;

            // 手動標記要更新的欄位
            _context.Entry(item).Property(x => x.ItemName).IsModified = true;
            _context.Entry(item).Property(x => x.SuperId).IsModified = true;
            _context.Entry(item).Property(x => x.ItemType).IsModified = true;

            await _context.SaveChangesAsync();


        return Ok(new
        {
            success = true,
            item = new
            {
                item.ItemId,
                item.ItemName,
                item.SuperId,
                item.ItemType
            }
        });
    }
}
