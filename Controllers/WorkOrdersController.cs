using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;

namespace PcbErpApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkOrdersController : ControllerBase
    {
        private readonly PcbErpContext _context;

        public WorkOrdersController(PcbErpContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<WorkOrder>>> GetWorkOrders()
        {
            return await _context.WorkOrders.ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<WorkOrder>> PostWorkOrder(WorkOrder order)
        {
            _context.WorkOrders.Add(order);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetWorkOrders), new { id = order.Id }, order);
        }
    }
}
