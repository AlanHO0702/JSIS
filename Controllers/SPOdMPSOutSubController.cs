using Microsoft.AspNetCore.Mvc;
using PcbErpApi.Data;
using PcbErpApi.Models;

[Route("api/[controller]")]
[ApiController]
public class SPOdMPSOutSubController : ControllerBase
{
    private readonly PcbErpContext _context;
    public SPOdMPSOutSubController(PcbErpContext context)
    {
        _context = context;
    }

    // GET api/SPOdMPSOutSub
    [HttpGet]
    public ActionResult<List<SPOdMPSOutSub>> Get([FromQuery] string PaperNum)
    {
        var data = _context.SPOdMPSOutSub
                           .Where(x => x.PaperNum == PaperNum)
                           .ToList();
        return Ok(data);
    }
}
