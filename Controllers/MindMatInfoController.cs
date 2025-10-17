using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PcbErpApi.Models;

[ApiController]
[Route("api/[controller]")]
public class MindMatInfoController : ControllerBase
{
    private readonly PcbErpContext _context;
    private readonly PaginationService _pagedService;

    public MindMatInfoController(PcbErpContext context, PaginationService pagedService)
    {
        _context = context;
        _pagedService = pagedService;
    }

    // GET: api/MindMatInfo
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MindMatInfo>>> GetMindMatInfos()
    {
        return await _context.MindMatInfo.ToListAsync();
    }

    // GET: api/MindMatInfo/{partnum}/{revision}
    [HttpGet("{partnum}/{revision}")]
    public async Task<ActionResult<MindMatInfo>> GetMindMatInfo(string partnum, string revision)
    {
        var item = await _context.MindMatInfo
            .FirstOrDefaultAsync(x => x.Partnum == partnum && x.Revision == revision);

        if (item == null)
            return NotFound();

        return item;
    }

    // GET: api/MindMatInfo/paged?page=1&pageSize=50
    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged(int page = 1, int pageSize = 50)
    {
        var query = _context.MindMatInfo
            .OrderByDescending(x => x.Partnum)
            .ThenByDescending(x => x.Revision);

        var result = await _pagedService.GetPagedAsync(query, page, pageSize);
        return Ok(new { totalCount = result.TotalCount, data = result.Data });
    }

    // POST: api/MindMatInfo
    [HttpPost]
    public async Task<ActionResult<MindMatInfo>> PostMindMatInfo(MindMatInfo item)
    {
        _context.MindMatInfo.Add(item);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMindMatInfo), new { partnum = item.Partnum, revision = item.Revision }, item);
    }

    // PUT: api/MindMatInfo/{partnum}/{revision}
    [HttpPut("{partnum}/{revision}")]
    public async Task<IActionResult> PutMindMatInfo(string partnum, string revision, MindMatInfo item)
    {
        if (partnum != item.Partnum || revision != item.Revision)
            return BadRequest();

        _context.Entry(item).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.MindMatInfo.Any(e => e.Partnum == partnum && e.Revision == revision))
                return NotFound();
            else
                throw;
        }

        return NoContent();
    }

    // DELETE: api/MindMatInfo/{partnum}/{revision}
    [HttpDelete("{partnum}/{revision}")]
    public async Task<IActionResult> DeleteMindMatInfo(string partnum, string revision)
    {
        var item = await _context.MindMatInfo
            .FirstOrDefaultAsync(x => x.Partnum == partnum && x.Revision == revision);

        if (item == null)
            return NotFound();

        _context.MindMatInfo.Remove(item);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
