using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;

namespace PcbErpApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CURdBUController : ControllerBase
    {
        private readonly PcbErpContext _context;
        public CURdBUController(PcbErpContext context)
        {
            _context = context;
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged(int page = 1, int pageSize = 50)
        {
            // ✅ 這裡的 DbSet 名稱要用 _context.CurdBu
            var query = _context.CurdBus.OrderBy(x => x.Buid);

            var total = await query.CountAsync();
            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new { totalCount = total, data });
        }
    }
}
