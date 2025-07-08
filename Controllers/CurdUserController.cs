using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;

[ApiController]
[Route("api/[controller]")]
public class CurdUsersController : ControllerBase
{
    private readonly PcbErpContext _context;

    public CurdUsersController(PcbErpContext context)
    {
        _context = context;
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
