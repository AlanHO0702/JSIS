using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class CurdUsersController : ControllerBase
{
        private readonly PcbErpContext _context;
        private readonly PaginationService _pagedService;

        public CurdUsersController(PcbErpContext context, PaginationService pagedService)
        {
            _context = context;
            _pagedService = pagedService;
        }


    // GET: api/CurdUsers
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CurdUser>>> GetCurdUsers()
    {
        return await _context.CurdUsers.ToListAsync();
    }

    // GET: api/CurdUsers/user01
    [HttpGet("{userId}")]
    public async Task<ActionResult<CurdUser>> GetCurdUser(string userId)
    {
        var user = await _context.CurdUsers.FirstOrDefaultAsync(u => u.UserId == userId);
        if (user == null) return NotFound();
        return user;
    }

         // 分頁查詢 GET: api/SPOdOrderMains/paged?page=1&pageSize=50
        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged(int page = 1, int pageSize = 50)
        {
            var query = _context.CurdUsers.OrderByDescending(o => o.UserId);
            var result = await _pagedService.GetPagedAsync(query, page, pageSize);
            return Ok(new { totalCount = result.TotalCount, data = result.Data });
        }   
    // POST: api/CurdUsers
    [HttpPost]
    public async Task<ActionResult<CurdUser>> PostCurdUser(CurdUser user)
    {
        _context.CurdUsers.Add(user);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCurdUser), new { userId = user.UserId }, user);
    }

    // PUT: api/CurdUsers/user01
    [HttpPut("{userId}")]
    public async Task<IActionResult> PutCurdUser(string userId, CurdUser user)
    {
        if (userId != user.UserId) return BadRequest();

        _context.Entry(user).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.CurdUsers.Any(u => u.UserId == userId))
                return NotFound();
            else
                throw;
        }

        return NoContent();
    }

    // DELETE: api/CurdUsers/user01
    [HttpDelete("{userId}")]
    public async Task<IActionResult> DeleteCurdUser(string userId)
    {
        var user = await _context.CurdUsers.FirstOrDefaultAsync(u => u.UserId == userId);
        if (user == null) return NotFound();

        _context.CurdUsers.Remove(user);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
