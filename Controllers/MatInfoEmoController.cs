using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MatInfoEmoController : ControllerBase
{
    private readonly string _connStr;

    public MatInfoEmoController(PcbErpContext context, IConfiguration config)
    {
        _connStr = config.GetConnectionString("DefaultConnection")
            ?? context?.Database.GetDbConnection().ConnectionString
            ?? throw new InvalidOperationException("Missing connection string.");
    }

    public class ConvertRequest
    {
        public string PartNum { get; set; } = "";
        public string Revision { get; set; } = "";
    }

    private static void AddParam(SqlCommand cmd, string name, object? value)
    {
        cmd.Parameters.AddWithValue(name, value ?? DBNull.Value);
    }

    private async Task<(string UserId, string UseId)> LoadUserContextAsync(SqlConnection conn)
    {
        var userId = string.Empty;
        var useId = string.Empty;

        var jwtHeader = Request?.Headers["X-JWTID"].ToString();
        if (!string.IsNullOrWhiteSpace(jwtHeader) && Guid.TryParse(jwtHeader, out var jwtId))
        {
            await using var cmd = new SqlCommand(
                "SELECT UserId FROM CURdUserOnline WITH (NOLOCK) WHERE JwtId = @jwtId", conn);
            cmd.Parameters.AddWithValue("@jwtId", jwtId);
            userId = (await cmd.ExecuteScalarAsync())?.ToString() ?? string.Empty;
        }

        if (string.IsNullOrWhiteSpace(userId))
            userId = User?.Identity?.Name ?? string.Empty;

        userId = userId.Trim();
        if (!string.IsNullOrWhiteSpace(userId))
        {
            await using var cmd = new SqlCommand(
                "SELECT UseId FROM CURdUsers WITH (NOLOCK) WHERE UserId = @userId", conn);
            cmd.Parameters.AddWithValue("@userId", userId);
            useId = (await cmd.ExecuteScalarAsync())?.ToString() ?? string.Empty;
        }

        if (string.IsNullOrWhiteSpace(useId))
        {
            var claim =
                User?.Claims?.FirstOrDefault(c => string.Equals(c.Type, "UseId", StringComparison.OrdinalIgnoreCase))
                ?? User?.Claims?.FirstOrDefault(c => string.Equals(c.Type, "useid", StringComparison.OrdinalIgnoreCase));
            useId = claim?.Value?.Trim() ?? string.Empty;
        }
        if (string.IsNullOrWhiteSpace(useId)) useId = "A001";

        return (userId, useId);
    }

    [HttpGet("Check")]
    public async Task<IActionResult> Check([FromQuery] string partNum)
    {
        if (string.IsNullOrWhiteSpace(partNum))
            return BadRequest(new { ok = false, error = "PartNum required." });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(
            "select top 1 PartNum from EMOdProdInfo (nolock) where PartNum = @PartNum", conn);
        AddParam(cmd, "@PartNum", partNum.Trim());
        var exists = (await cmd.ExecuteScalarAsync()) != null;
        return Ok(new { ok = true, exists });
    }

    [HttpPost("Convert")]
    public async Task<IActionResult> Convert([FromBody] ConvertRequest req)
    {
        if (req == null || string.IsNullOrWhiteSpace(req.PartNum) || string.IsNullOrWhiteSpace(req.Revision))
            return BadRequest(new { ok = false, error = "PartNum and Revision required." });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        var (userId, useId) = await LoadUserContextAsync(conn);

        await using (var chk = new SqlCommand("MGNdCheckBefToEMO", conn))
        {
            chk.CommandType = CommandType.StoredProcedure;
            chk.CommandTimeout = 120;
            AddParam(chk, "@PartNum", req.PartNum.Trim());
            AddParam(chk, "@Revision", req.Revision.Trim());
            await chk.ExecuteNonQueryAsync();
        }

        await using (var exec = new SqlCommand("MGNdMat2ProdInfo", conn))
        {
            exec.CommandType = CommandType.StoredProcedure;
            exec.CommandTimeout = 600;
            AddParam(exec, "@PartNum", req.PartNum.Trim());
            AddParam(exec, "@Revision", req.Revision.Trim());
            AddParam(exec, "@UseId", useId);
            AddParam(exec, "@UserId", userId);
            await exec.ExecuteNonQueryAsync();
        }

        return Ok(new { ok = true });
    }
}
