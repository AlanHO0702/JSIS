using Microsoft.AspNetCore.Mvc;
using PcbErpApi.Models;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;

namespace PcbErpApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FmedVProcNisToStdController : ControllerBase
    {
        private readonly PcbErpContext _context;

        public FmedVProcNisToStdController(PcbErpContext context)
        {
            _context = context;
        }

            // GET: api/MindMatInfo
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FmedVProcNisToStd>>> GetFmedVProcNisToStd()
        {
            return await _context.FmedVProcNisToStd.ToListAsync();
        }


        [HttpGet("pivot-data")]
        public async Task<IActionResult> GetPivotData()
        {
            var data = await _context.MindStockCostPn
                .Where(x => x.HisId == "2023.10" && x.MB == 1)
                .Select(x => new {
                    x.PartNum,
                    x.SouQnty,
                    x.SouCost,
                    x.InQnty,
                    x.InCost,
                    x.ElseInCost,
                    x.ElseInQnty,
                    x.RejQnty,
                    x.RejCost,
                    x.BalQnty,
                    x.BalCost,
                    x.SaleQnty,
                    x.SaleCost,
                    x.OtherOutQnty,
                    x.OtherOutCost,
                    x.BackQnty,
                    x.BackCost,
                    x.ScrapQnty,
                    x.ScrapCost,
                    x.ElseOutQnty,
                    x.ElseOutCost,
                    x.EndQnty,
                    x.EndCost,
                    x.Back4NetQnty,
                    x.Back4NetCost,
                    x.FGInQnty,
                    x.FGInCost,
                    x.SalesReturnQnty,
                    x.SalesReturnCost,
                    x.FarmInQnty,
                    x.FarmInCost,
                    x.FarmOutQnty,
                    x.FarmOutCost,
                    x.UnitCost
                })
                .ToListAsync();

            return Ok(data);
        }
    }
}
