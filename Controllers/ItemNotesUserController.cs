using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;

namespace PcbErpApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItemNotesUserController : ControllerBase
    {
        private readonly PcbErpContext _context;

        public ItemNotesUserController(PcbErpContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string itemId, [FromQuery] string userId)
        {
            if (string.IsNullOrWhiteSpace(itemId) || string.IsNullOrWhiteSpace(userId))
                return BadRequest(new { error = "itemId and userId are required" });

            await using var conn = _context.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open)
                await conn.OpenAsync();

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT CONVERT(varchar(4000), ISNULL(Notes,'')) FROM CURdSysItemsNotes WITH (NOLOCK) WHERE ItemId = @itemId AND UserId = @userId";

            var p1 = cmd.CreateParameter();
            p1.ParameterName = "@itemId";
            p1.Value = itemId.Trim();
            cmd.Parameters.Add(p1);

            var p2 = cmd.CreateParameter();
            p2.ParameterName = "@userId";
            p2.Value = userId.Trim();
            cmd.Parameters.Add(p2);

            var result = await cmd.ExecuteScalarAsync();
            var notes = result == null || result == DBNull.Value ? "" : result.ToString() ?? "";

            return Ok(new { itemId = itemId.Trim(), userId = userId.Trim(), notes });
        }

        public class ItemNotesUserUpdate
        {
            public string ItemId { get; set; } = "";
            public string UserId { get; set; } = "";
            public string Notes { get; set; } = "";
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ItemNotesUserUpdate req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.ItemId) || string.IsNullOrWhiteSpace(req.UserId))
                return BadRequest(new { error = "itemId and userId are required" });

            await using var conn = _context.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open)
                await conn.OpenAsync();

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
IF EXISTS (SELECT 1 FROM CURdSysItemsNotes WITH (NOLOCK) WHERE ItemId = @itemId AND UserId = @userId)
  UPDATE CURdSysItemsNotes SET Notes = @notes WHERE ItemId = @itemId AND UserId = @userId
ELSE
  INSERT INTO CURdSysItemsNotes (ItemId, UserId, Notes) VALUES (@itemId, @userId, @notes)";

            var p1 = cmd.CreateParameter();
            p1.ParameterName = "@itemId";
            p1.Value = req.ItemId.Trim();
            cmd.Parameters.Add(p1);

            var p2 = cmd.CreateParameter();
            p2.ParameterName = "@userId";
            p2.Value = req.UserId.Trim();
            cmd.Parameters.Add(p2);

            var p3 = cmd.CreateParameter();
            p3.ParameterName = "@notes";
            p3.Value = (req.Notes ?? "").Trim();
            cmd.Parameters.Add(p3);

            await cmd.ExecuteNonQueryAsync();

            return Ok(new { ok = true });
        }
    }
}
