using Microsoft.AspNetCore.Mvc;
using PcbErpApi.Models;

[Route("api/[controller]")]
[ApiController]
public class SpodOrderSubsController : ControllerBase
{
    private readonly CirContext _context;
    public SpodOrderSubsController(CirContext context)
    {
        _context = context;
    }

    // GET api/SpodOrderSubs?PaperNum=SA25050075
    [HttpGet]
    public ActionResult<List<SpodOrderSub>> Get([FromQuery] string PaperNum)
    {
        var data = _context.SpodOrderSub
                           .Where(x => x.PaperNum == PaperNum)
                           .ToList();
        return Ok(data);
    }
}
