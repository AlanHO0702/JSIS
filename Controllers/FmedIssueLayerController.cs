using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;

[Route("api/[controller]")]
[ApiController]
public class FMEdIssueLayerController : ControllerBase
{
    private readonly PcbErpContext _context;

    public FMEdIssueLayerController(PcbErpContext context)
    {
        _context = context;
    }

    // GET api/FMEdIssueLayer?PaperNum=P25110001
    [HttpGet]
    public ActionResult<List<FmedIssueLayer>> Get([FromQuery] string PaperNum)
    {
        var data = _context.FmedIssueLayer
                           .Where(x => x.PaperNum == PaperNum)
                           .OrderBy(x => x.Item)
                           .ToList();
        return Ok(data);
    }

    // GET api/FMEdIssueLayer/{paperNum}/{item}
    [HttpGet("{paperNum}/{item}")]
    public async Task<ActionResult<FmedIssueLayer>> GetById(string paperNum, int item)
    {
        var data = await _context.FmedIssueLayer
            .FirstOrDefaultAsync(x => x.PaperNum == paperNum && x.Item == item);

        if (data == null)
            return NotFound();

        return Ok(data);
    }

    // POST api/FMEdIssueLayer
    [HttpPost]
    public async Task<ActionResult<FmedIssueLayer>> Post([FromBody] FmedIssueLayer layer)
    {
        if (layer == null || string.IsNullOrWhiteSpace(layer.PaperNum))
            return BadRequest("資料不完整");

        _context.FmedIssueLayer.Add(layer);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { paperNum = layer.PaperNum, item = layer.Item }, layer);
    }

    // PUT api/FMEdIssueLayer/{paperNum}/{item}
    [HttpPut("{paperNum}/{item}")]
    public async Task<IActionResult> Put(string paperNum, int item, [FromBody] FmedIssueLayer layer)
    {
        if (paperNum != layer.PaperNum || item != layer.Item)
            return BadRequest("PaperNum 或 Item 不符");

        _context.Entry(layer).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            bool exists = _context.FmedIssueLayer.Any(x => x.PaperNum == paperNum && x.Item == item);
            if (!exists)
                return NotFound();
            else
                throw;
        }

        return NoContent();
    }

    // DELETE api/FMEdIssueLayer/{paperNum}/{item}
    [HttpDelete("{paperNum}/{item}")]
    public async Task<IActionResult> Delete(string paperNum, int item)
    {
        var target = await _context.FmedIssueLayer
                                   .FirstOrDefaultAsync(x => x.PaperNum == paperNum && x.Item == item);
        if (target == null)
            return NotFound();

        _context.FmedIssueLayer.Remove(target);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
