using Microsoft.AspNetCore.Mvc;
using PcbErpApi.Data;
using PcbErpApi.Models;

[Route("api/[controller]")]
[ApiController]
public class AJNdClassMoneyController : ControllerBase
{
    private readonly PcbErpContext _context;
    public AJNdClassMoneyController(PcbErpContext context)
    {
        _context = context;
    }

    // GET api/SpodOrderSub?PaperNum=SA25050075
    [HttpGet]
    public ActionResult<List<AJNdClassMoney>> Get()
    {
        try
        {
            var data = _context.AJNdClassMoney.ToList();
            return Ok(data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
