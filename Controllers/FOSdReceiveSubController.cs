using Microsoft.AspNetCore.Mvc;
using PcbErpApi.Data;
using PcbErpApi.Models;

[Route("api/[controller]")]
[ApiController]
public class FOSdReceiveSubController : ControllerBase
{
    private readonly PcbErpContext _context;
    public FOSdReceiveSubController(PcbErpContext context)
    {
        _context = context;
    }

    // GET api/FOSdReceiveSub?PaperNum=FR25120001
    [HttpGet]
    public ActionResult<List<FosdReceiveSub>> Get([FromQuery] string PaperNum)
    {
        var data = _context.FosdReceiveSub
                           .Where(x => x.PaperNum == PaperNum)
                           .ToList();
        return Ok(data);
    }
}
