using Microsoft.AspNetCore.Mvc;
using PcbErpApi.Data;
using PcbErpApi.Models;

[Route("api/[controller]")]
[ApiController]
public class SpodOrderSubController : ControllerBase
{
    private readonly PcbErpContext _context;
    public SpodOrderSubController(PcbErpContext context)
    {
        _context = context;
    }

    // GET api/SpodOrderSub?PaperNum=SA25050075
    [HttpGet]
    public ActionResult<List<SpodOrderSub>> Get([FromQuery] string PaperNum)
    {
        var data = _context.SpodOrderSub
                           .Where(x => x.PaperNum == PaperNum)
                           .ToList();
        return Ok(data);
    }
}
