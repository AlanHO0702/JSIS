using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;

namespace PcbErpApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // => /api/EditItemApi
    public class EditItemApiController : ControllerBase
    {
        private readonly PcbErpContext _context;
        public EditItemApiController(PcbErpContext context) => _context = context;

        public class EditItemDto
        {
            public string ItemId { get; set; } = "";
            public string ItemName { get; set; } = "";
            public string SuperId { get; set; } = "";
            public int SerialNum { get; set; }
            public int Enabled { get; set; }   // 0=停用, 1=啟用
            public string Notes { get; set; } = "";
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] EditItemDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.ItemId))
                return BadRequest(new { success = false, message = "ItemId 不可空白" });

            var item = await _context.CurdSysItems
                .FirstOrDefaultAsync(x => x.ItemId == dto.ItemId);

            if (item == null)
                return NotFound(new { success = false, message = "找不到項目" });

            item.ItemName  = dto.ItemName?.Trim();
            item.SuperId   = dto.SuperId?.Trim();
            item.SerialNum = dto.SerialNum;
            item.Enabled   = dto.Enabled;
            item.Notes   = dto.Notes?.Trim();

            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }
    }
}
