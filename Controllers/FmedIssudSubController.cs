using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;

[Route("api/[controller]")]
[ApiController]
public class FMEdIssueSubController : ControllerBase
{
    private readonly PcbErpContext _context;
    public FMEdIssueSubController(PcbErpContext context)
    {
        _context = context;
    }

    // GET api/SpodOrderSub?PaperNum=SA25050075
    [HttpGet]
    public ActionResult<List<FmedIssueSub>> Get([FromQuery] string PaperNum)
    {
        var data = _context.FmedIssueSub
                           .Where(x => x.PaperNum == PaperNum)
                           .ToList();
        return Ok(data);
    }
}


/*
public class FMEdIssueSubController : ControllerBase
{
    private readonly PcbErpContext _context;

    public FMEdIssueSubController(PcbErpContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 根據 PaperNum 取得製令單明細資料。
    /// </summary>
    /// <param name="PaperNum">製令單號</param>
    /// <returns>該單號的所有明細資料</returns>
    // GET api/FMEdIssueSub?PaperNum=FA25070001
    [HttpGet]
    public ActionResult<List<FmedIssueSub>> Get([FromQuery] string PaperNum)
    {
        if (string.IsNullOrWhiteSpace(PaperNum))
            return BadRequest("必須提供 PaperNum");

        var data = _context.FmedIssueSub
                           .Where(x => x.PaperNum == PaperNum)
                           .OrderBy(x => x.Item)   // 順序顯示
                           .ToList();
        return Ok(data);
    }

    /// <summary>
    /// 新增一筆製令單明細。
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<FmedIssueSub>> Post([FromBody] FmedIssueSub sub)
    {
        if (sub == null || string.IsNullOrWhiteSpace(sub.PaperNum))
            return BadRequest("資料不完整");

        _context.FmedIssueSub.Add(sub);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { PaperNum = sub.PaperNum }, sub);
    }

    /// <summary>
    /// 更新一筆製令單明細。
    /// </summary>
    [HttpPut("{paperNum}/{item}")]
    public async Task<IActionResult> Put(string paperNum, int item, [FromBody] FmedIssueSub sub)
    {
        if (paperNum != sub.PaperNum || item != sub.Item)
            return BadRequest("PaperNum 或 Item 不符");

        _context.Entry(sub).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            bool exists = _context.FmedIssueSub.Any(x => x.PaperNum == paperNum && x.Item == item);
            if (!exists)
                return NotFound();
            else
                throw;
        }

        return NoContent();
    }

    /// <summary>
    /// 刪除一筆製令單明細。
    /// </summary>
    [HttpDelete("{paperNum}/{item}")]
    public async Task<IActionResult> Delete(string paperNum, int item)
    {
        var target = await _context.FmedIssueSub
                                   .FirstOrDefaultAsync(x => x.PaperNum == paperNum && x.Item == item);
        if (target == null)
            return NotFound();

        _context.FmedIssueSub.Remove(target);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
*/