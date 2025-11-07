using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;

[Route("api/[controller]")]
[ApiController]
public class FMEdIssueMatController : ControllerBase
{
    private readonly PcbErpContext _context;
    private readonly ITableDictionaryService _dictService;

    public FMEdIssueMatController(PcbErpContext context, ITableDictionaryService dictService)
    {
        _context = context;
        _dictService = dictService;
    }

    // GET api/FMEdIssueMat?PaperNum=P25110001
    [HttpGet]
    public ActionResult<List<FmedIssueMat>> Get([FromQuery] string PaperNum)
    {
        var data = _context.FmedIssueMat
                           .Where(x => x.PaperNum == PaperNum)
                           .OrderBy(x => x.Item)
                           .ToList();
        return Ok(data);
    }

    // GET api/FMEdIssueMat/{paperNum}/{item}
    [HttpGet("{paperNum}/{item}")]
    public async Task<ActionResult<FmedIssueMat>> GetById(string paperNum, int item)
    {
        var data = await _context.FmedIssueMat
            .FirstOrDefaultAsync(x => x.PaperNum == paperNum && x.Item == item);

        if (data == null)
            return NotFound();

        return Ok(data);
    }

    // POST api/FMEdIssueMat
    [HttpPost]
    public async Task<ActionResult<FmedIssueMat>> Post([FromBody] FmedIssueMat mat)
    {
        if (mat == null || string.IsNullOrWhiteSpace(mat.PaperNum))
            return BadRequest("資料不完整");

        _context.FmedIssueMat.Add(mat);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { paperNum = mat.PaperNum, item = mat.Item }, mat);
    }

    // PUT api/FMEdIssueMat/{paperNum}/{item}
    [HttpPut("{paperNum}/{item}")]
    public async Task<IActionResult> Put(string paperNum, int item, [FromBody] FmedIssueMat mat)
    {
        if (paperNum != mat.PaperNum || item != mat.Item)
            return BadRequest("PaperNum 或 Item 不符");

        _context.Entry(mat).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            bool exists = _context.FmedIssueMat.Any(x => x.PaperNum == paperNum && x.Item == item);
            if (!exists)
                return NotFound();
            else
                throw;
        }

        return NoContent();
    }

    // DELETE api/FMEdIssueMat/{paperNum}/{item}
    [HttpDelete("{paperNum}/{item}")]
    public async Task<IActionResult> Delete(string paperNum, int item)
    {
        var target = await _context.FmedIssueMat
                                   .FirstOrDefaultAsync(x => x.PaperNum == paperNum && x.Item == item);
        if (target == null)
            return NotFound();

        _context.FmedIssueMat.Remove(target);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
