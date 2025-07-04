using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;

[ApiController]
[Route("api/[controller]")]
public class AddItemApiController : ControllerBase
{
    private readonly PcbErpContext _context;

    public AddItemApiController(PcbErpContext context)
    {
        _context = context;
    }

    public class AddItemRequest
    {
        public string ItemId { get; set; }
        public string SuperId { get; set; }
        public string ItemName { get; set; }
        public int ItemType { get; set; }  // 新增欄位
    }

    [HttpPost]
    public async Task<IActionResult> AddItem([FromBody] AddItemRequest input)
    {
        if (input == null || string.IsNullOrWhiteSpace(input.ItemId) ||
            string.IsNullOrWhiteSpace(input.SuperId) || string.IsNullOrWhiteSpace(input.ItemName))
        {
            return BadRequest(new { success = false, message = "格式錯誤或缺欄位" });
        }

        if (await _context.CurdSysItems.AnyAsync(x => x.ItemId == input.ItemId))
        {
            return Conflict(new { success = false, message = "ItemId 已存在" });
        }

        var systemId = input.ItemId.Substring(0, 3);

        var maxSerial = await _context.CurdSysItems
            .Where(x => x.SuperId == input.SuperId)
            .MaxAsync(x => (int?)x.SerialNum) ?? 0;

        var newSerial = maxSerial + 1;

        var item = new CurdSysItem
        {
            ItemId = input.ItemId,
            SuperId = input.SuperId,
            ItemName = input.ItemName,
            SystemId = systemId,
            LevelNo = 2,
            SerialNum = newSerial,
            Enabled = 1,
            WindowState = 1,
            ItemType = input.ItemType  // 套用前端傳來的 ItemType
        };

        _context.CurdSysItems.Add(item);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            item = new
            {
                item.ItemId,
                item.ItemName,
                item.ItemType  // 回傳 ItemType 給前端使用
            }
        });
    }
}
