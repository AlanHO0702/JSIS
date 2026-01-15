using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using System.Data;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class UpdateLogController : ControllerBase
{
    private readonly string _cs;

    public UpdateLogController(IConfiguration cfg, PcbErpContext db)
    {
        _cs = cfg.GetConnectionString("DefaultConnection")
              ?? db.Database.GetDbConnection().ConnectionString;
    }

    [HttpGet("Master")]
    public async Task<IActionResult> GetMaster(
        [FromQuery] string paperId,
        [FromQuery] string paperNum,
        [FromQuery] string? userId)
    {
        if (string.IsNullOrWhiteSpace(paperId) || string.IsNullOrWhiteSpace(paperNum))
        {
            return Ok(new { ok = false, error = "參數不足" });
        }

        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync();

        const string sql = @"
exec CURdTableUpdateLogInq
  @PaperId,
  @PaperNum,
  @UserId";

        var rows = await QueryRowsAsync(conn, sql,
            new SqlParameter("@PaperId", paperId),
            new SqlParameter("@PaperNum", paperNum),
            new SqlParameter("@UserId", userId ?? string.Empty));

        return Ok(new { ok = true, rows });
    }

    [HttpGet("History")]
    public async Task<IActionResult> GetHistory(
        [FromQuery] string paperId,
        [FromQuery] string paperNum,
        [FromQuery] string? userId)
    {
        if (string.IsNullOrWhiteSpace(paperId) || string.IsNullOrWhiteSpace(paperNum))
        {
            return Ok(new { ok = false, error = "參數不足" });
        }

        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync();

        const string sql = @"
exec CURdTableUpdateLogInqHis
  @PaperId,
  @PaperNum,
  @UserId";

        var rows = await QueryRowsAsync(conn, sql,
            new SqlParameter("@PaperId", paperId),
            new SqlParameter("@PaperNum", paperNum),
            new SqlParameter("@UserId", userId ?? string.Empty));

        return Ok(new { ok = true, rows });
    }

    private static async Task<List<Dictionary<string, object?>>> QueryRowsAsync(
        SqlConnection conn,
        string sql,
        params SqlParameter[] parameters)
    {
        var list = new List<Dictionary<string, object?>>();
        await using var cmd = new SqlCommand(sql, conn);
        if (parameters?.Length > 0) cmd.Parameters.AddRange(parameters);
        await using var rd = await cmd.ExecuteReaderAsync();
        var cols = Enumerable.Range(0, rd.FieldCount).Select(rd.GetName).ToList();
        while (await rd.ReadAsync())
        {
            var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            foreach (var c in cols)
                row[c] = rd[c] == DBNull.Value ? null : rd[c];
            list.Add(row);
        }
        return list;
    }
}
