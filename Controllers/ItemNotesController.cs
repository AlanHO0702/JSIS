using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using Microsoft.Data.SqlClient;

namespace PcbErpApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItemNotesController : ControllerBase
    {
        private readonly PcbErpContext _context;

        public ItemNotesController(PcbErpContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
                return BadRequest(new { error = "itemId is required" });

            await using var conn = _context.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open)
                await conn.OpenAsync();

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT CONVERT(varchar(4000), ISNULL(Notes,'')) FROM CURdSysItems WITH (NOLOCK) WHERE ItemId = @itemId";
            var p = cmd.CreateParameter();
            p.ParameterName = "@itemId";
            p.Value = itemId.Trim();
            cmd.Parameters.Add(p);

            var result = await cmd.ExecuteScalarAsync();
            var notes = result == null || result == DBNull.Value ? "" : result.ToString() ?? "";

            return Ok(new { itemId = itemId.Trim(), notes });
        }

        public class ItemNotesUpdate
        {
            public string ItemId { get; set; } = "";
            public string Notes { get; set; } = "";
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ItemNotesUpdate req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.ItemId))
                return BadRequest(new { error = "itemId is required" });

            var item = await _context.CurdSysItems
                .FirstOrDefaultAsync(x => x.ItemId == req.ItemId.Trim());
            if (item == null)
                return NotFound(new { error = "item not found" });

            item.Notes = (req.Notes ?? "").Trim();
            await _context.SaveChangesAsync();

            return Ok(new { ok = true });
        }
    }
}
