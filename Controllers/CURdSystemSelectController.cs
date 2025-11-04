using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;

namespace PcbErpApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CURdSystemSelectController : ControllerBase
{
    private readonly PcbErpContext _context;
    public CURdSystemSelectController(PcbErpContext context) => _context = context;

    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 50;

        var q = _context.CurdSystemSelects.AsNoTracking();

        // 預設排序（你可以改成 OrderNum、SerialNum …）
        q = q.OrderBy(x => x.SystemId);

        var total = await q.CountAsync();
        var data = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return Ok(new { totalCount = total, data });
    }
}
