using Microsoft.AspNetCore.Mvc;
using PcbErpApi.Data;
using PcbErpApi.Models;

[Route("api/[controller]")]
[ApiController]
public class AjndJourSubController : ControllerBase
{
    private readonly PcbErpContext _context;
    public AjndJourSubController(PcbErpContext context)
    {
        _context = context;
    }

    // GET api/SpodOrderSub?PaperNum=SA25050075
    [HttpGet]
    public ActionResult<List<AjndJourSub>> Get([FromQuery] string PaperNum)
    {
        var data = _context.AjndJourSub
                           .Where(x => x.PaperNum == PaperNum)
                           .ToList();
        return Ok(data);
    }
}
