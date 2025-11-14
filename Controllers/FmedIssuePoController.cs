using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;
using PcbErpApi.Helpers;

[Route("api/[controller]")]
[ApiController]
public class FMEdIssuePOController : ControllerBase
{
    private readonly PcbErpContext _context;

    public FMEdIssuePOController(PcbErpContext context)
    {
        _context = context;
    }

    // GET api/FMEdIssuePO?PaperNum=P25110001
    [HttpGet]
    public ActionResult<List<FmedIssuePo>> Get([FromQuery] string PaperNum)
    {
        var data = _context.FmedIssuePo
                           .Where(x => x.PaperNum == PaperNum)
                           .OrderBy(x => x.Item)
                           .ToList();
        return Ok(data);
    }

    // GET api/FMEdIssuePO/{paperNum}/{item}
    [HttpGet("{paperNum}/{item}")]
    public async Task<ActionResult<FmedIssuePo>> GetById(string paperNum, int item)
    {
        var data = await _context.FmedIssuePo
            .FirstOrDefaultAsync(x => x.PaperNum == paperNum && x.Item == item);

        if (data == null)
            return NotFound();

        return Ok(data);
    }

    // POST api/FMEdIssuePO
    [HttpPost]
    public async Task<ActionResult<FmedIssuePo>> Post([FromBody] FmedIssuePo po)
    {
        if (po == null || string.IsNullOrWhiteSpace(po.PaperNum))
            return BadRequest("資料不完整");

        _context.FmedIssuePo.Add(po);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { paperNum = po.PaperNum, item = po.Item }, po);
    }

    // PUT api/FMEdIssuePO/{paperNum}/{item}
    [HttpPut("{paperNum}/{item}")]
    public async Task<IActionResult> Put(string paperNum, int item, [FromBody] FmedIssuePo po)
    {
        if (paperNum != po.PaperNum || item != po.Item)
            return BadRequest("PaperNum 或 Item 不符");

        _context.Entry(po).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            bool exists = _context.FmedIssuePo.Any(x => x.PaperNum == paperNum && x.Item == item);
            if (!exists)
                return NotFound();
            else
                throw;
        }

        return NoContent();
    }

    // DELETE api/FMEdIssuePO/{paperNum}/{item}
    [HttpDelete("{paperNum}/{item}")]
    public async Task<IActionResult> Delete(string paperNum, int item)
    {
        // 使用 Helper 統一處理刪除邏輯和錯誤訊息
        return await DetailApiHelper.ExecuteDeleteWithErrorHandling(
            _context,
            paperNum,
            item,
            _context.FmedIssuePo
        );
    }
}
