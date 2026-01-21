using Microsoft.AspNetCore.Mvc;
using PcbErpApi.Data;
using PcbErpApi.Models;

[Route("api/[controller]")]
[ApiController]
public class FOSdOrderSubController : ControllerBase
{
    private readonly PcbErpContext _context;
    public FOSdOrderSubController(PcbErpContext context)
    {
        _context = context;
    }

    // GET api/FOSdOrderSub?PaperNum=FO25120001
    [HttpGet]
    public ActionResult<List<FosdOrderSub>> Get([FromQuery] string PaperNum)
    {
        var data = _context.FosdOrderSub
                           .Where(x => x.PaperNum == PaperNum)
                           .ToList();
        return Ok(data);
    }
}
