using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;

[ApiController]
[Route("api/[controller]")]
public class ItemHelperApiController : ControllerBase
{
    private readonly PcbErpContext _context;

    public ItemHelperApiController(PcbErpContext context)
    {
        _context = context;
    }

    [HttpGet("next-id")]
    public async Task<IActionResult> GetNextItemId([FromQuery] string systemId)
    {
        if (string.IsNullOrWhiteSpace(systemId) || systemId.Length != 3)
            return BadRequest(new { success = false, message = "SystemId 必須為三碼" });

        var maxId = await _context.CurdSysItems
            .Where(x => x.ItemId.StartsWith(systemId) && x.LevelNo == 2) // 只看功能項目
            .OrderByDescending(x => x.ItemId)
            .Select(x => x.ItemId)
            .FirstOrDefaultAsync();

        int nextNum = 1;

        if (!string.IsNullOrEmpty(maxId) && maxId.Length >= 6 && int.TryParse(maxId.Substring(3, 3), out int lastNum))
        {
            nextNum = lastNum + 1;
        }

        var nextItemId = systemId + nextNum.ToString("D3");

        return Ok(new { nextItemId });
    }
}
