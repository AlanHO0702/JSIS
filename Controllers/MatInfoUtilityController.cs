using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using System.Data;

namespace PcbErpApi.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class MatInfoUtilityController : ControllerBase
    {
        private readonly PcbErpContext _context;
        private readonly string _connStr;

        public MatInfoUtilityController(PcbErpContext context, IConfiguration config)
        {
            _context = context;
            _connStr = config.GetConnectionString("DefaultConnection")
                ?? _context.Database.GetConnectionString()
                ?? throw new InvalidOperationException("Missing connection string.");
        }

        public sealed class SaveHeightRequest
        {
            public string ItemId { get; set; } = string.Empty;
            public int Height { get; set; }
        }

        public sealed class SpecActionRequest
        {
            public string PartNum { get; set; } = string.Empty;
            public string Revision { get; set; } = string.Empty;
            public int? SpecType { get; set; }
        }

        public sealed class UpdateCustPnRequest
        {
            public string PartNum { get; set; } = string.Empty;
            public string NewCustPn { get; set; } = string.Empty;
            public string NewCustEg { get; set; } = string.Empty;
            public string UserId { get; set; } = string.Empty;
        }

        public sealed class CopyPnRequest
        {
            public string PartNum { get; set; } = string.Empty;
            public string UserId { get; set; } = string.Empty;
        }

        [HttpGet]
        public async Task<IActionResult> BrowseHeight([FromQuery] string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
                return BadRequest("itemId is required.");

            const string sql = @"
SELECT TOP 1 DLLValue
  FROM CURdOCXItemOtherRule WITH (NOLOCK)
 WHERE ItemId = @ItemId
   AND RuleId = 'PaperMasterHeight';";

            await using var conn = new SqlConnection(_connStr);
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ItemId", itemId);
            await conn.OpenAsync();
            var val = await cmd.ExecuteScalarAsync();
            if (val == null || val == DBNull.Value)
                return Ok(new { height = (int?)null });

            if (int.TryParse(Convert.ToString(val), out var h))
                return Ok(new { height = h });

            return Ok(new { height = (int?)null });
        }

        [HttpPost]
        public async Task<IActionResult> SaveBrowseHeight([FromBody] SaveHeightRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.ItemId))
                return BadRequest("itemId is required.");

            var safeItem = EscapeSqlLiteral(req.ItemId);
            var sql = $"exec CURdLayerHeightSave '{safeItem}', {req.Height}, 0";

            await using var conn = new SqlConnection(_connStr);
            await using var cmd = new SqlCommand(sql, conn);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            return Ok(new { ok = true });
        }

        [HttpPost]
        public async Task<IActionResult> SpecDataSet([FromBody] SpecActionRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.PartNum))
                return BadRequest("PartNum is required.");
            if (req.SpecType == null)
                return BadRequest("SpecType is required.");

            var safePart = EscapeSqlLiteral(req.PartNum);
            var safeRev = EscapeSqlLiteral(req.Revision ?? string.Empty);
            var sql = $"exec MGNdSpecDataSet '{safePart}', '{safeRev}', {req.SpecType}";

            await using var conn = new SqlConnection(_connStr);
            await using var cmd = new SqlCommand(sql, conn);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            return Ok(new { ok = true });
        }

        [HttpPost]
        public async Task<IActionResult> SpecDataClear([FromBody] SpecActionRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.PartNum))
                return BadRequest("PartNum is required.");

            var safePart = EscapeSqlLiteral(req.PartNum);
            var safeRev = EscapeSqlLiteral(req.Revision ?? string.Empty);
            var sql = $"exec MGNdSpecDataClear '{safePart}', '{safeRev}'";

            await using var conn = new SqlConnection(_connStr);
            await using var cmd = new SqlCommand(sql, conn);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            return Ok(new { ok = true });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCustPn([FromBody] UpdateCustPnRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.PartNum))
                return BadRequest("PartNum is required.");

            var safePart = EscapeSqlLiteral(req.PartNum);
            var safePn = EscapeSqlLiteral(req.NewCustPn ?? string.Empty);
            var safeUser = EscapeSqlLiteral(req.UserId ?? string.Empty);
            var safeEg = EscapeSqlLiteral(req.NewCustEg ?? string.Empty);

            var sql = $"exec MGNdMatInfoUpdateCustPn '{safePart}', '{safePn}', '{safeUser}', '{safeEg}'";

            await using var conn = new SqlConnection(_connStr);
            await using var cmd = new SqlCommand(sql, conn);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            return Ok(new { ok = true });
        }

        [HttpPost]
        public async Task<IActionResult> CopyCustomerVersion([FromBody] CopyPnRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.PartNum))
                return BadRequest("PartNum is required.");

            var safePart = EscapeSqlLiteral(req.PartNum);
            var safeUser = EscapeSqlLiteral(req.UserId ?? string.Empty);
            var sql = $"exec MGNdCopyPN4YX '{safePart}', '{safeUser}'";

            await using var conn = new SqlConnection(_connStr);
            await using var cmd = new SqlCommand(sql, conn);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            return Ok(new { ok = true });
        }

        private static string EscapeSqlLiteral(string input)
        {
            return (input ?? string.Empty).Replace("'", "''");
        }
    }
}
