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
        [FromQuery] string? paperId,
        [FromQuery] string? itemId,
        [FromQuery] string paperNum,
        [FromQuery] string? userId)
    {
        if (string.IsNullOrWhiteSpace(paperNum))
            return Ok(new { ok = false, error = "paperNum is required." });

        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync();

        const string sql = @"
exec CURdTableUpdateLogInq
  @PaperId,
  @PaperNum,
  @UserId";

        var result = await QueryWithFallbackAsync(conn, sql, paperId, itemId, paperNum, userId ?? string.Empty);
        if (!result.Ok)
            return Ok(new { ok = false, error = result.Error ?? "UpdateLog query failed.", triedPaperIds = result.TriedPaperIds });

        return Ok(new
        {
            ok = true,
            rows = result.Rows,
            usedPaperId = result.UsedPaperId,
            triedPaperIds = result.TriedPaperIds
        });
    }

    [HttpGet("History")]
    public async Task<IActionResult> GetHistory(
        [FromQuery] string? paperId,
        [FromQuery] string? itemId,
        [FromQuery] string paperNum,
        [FromQuery] string? userId)
    {
        if (string.IsNullOrWhiteSpace(paperNum))
            return Ok(new { ok = false, error = "paperNum is required." });

        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync();

        const string sql = @"
exec CURdTableUpdateLogInqHis
  @PaperId,
  @PaperNum,
  @UserId";

        var result = await QueryWithFallbackAsync(conn, sql, paperId, itemId, paperNum, userId ?? string.Empty);
        if (!result.Ok)
            return Ok(new { ok = false, error = result.Error ?? "UpdateLog query failed.", triedPaperIds = result.TriedPaperIds });

        return Ok(new
        {
            ok = true,
            rows = result.Rows,
            usedPaperId = result.UsedPaperId,
            triedPaperIds = result.TriedPaperIds
        });
    }

    private static async Task<(bool Ok, List<Dictionary<string, object?>> Rows, string? UsedPaperId, string[] TriedPaperIds, string? Error)>
        QueryWithFallbackAsync(SqlConnection conn, string sql, string? paperId, string? itemId, string paperNum, string userId)
    {
        var candidates = BuildPaperIdCandidates(paperId, itemId);
        if (candidates.Count == 0)
            return (false, new List<Dictionary<string, object?>>(), null, Array.Empty<string>(), "paperId/itemId is required.");

        Exception? lastEx = null;
        List<Dictionary<string, object?>>? firstRows = null;
        string? firstUsed = null;

        foreach (var pid in candidates)
        {
            try
            {
                var rows = await QueryRowsAsync(conn, sql,
                    new SqlParameter("@PaperId", pid),
                    new SqlParameter("@PaperNum", paperNum),
                    new SqlParameter("@UserId", userId));

                if (firstRows == null)
                {
                    firstRows = rows;
                    firstUsed = pid;
                }

                if (rows.Count > 0)
                    return (true, rows, pid, candidates.ToArray(), null);
            }
            catch (Exception ex)
            {
                lastEx = ex;
            }
        }

        if (firstRows != null)
            return (true, firstRows, firstUsed, candidates.ToArray(), null);

        return (false, new List<Dictionary<string, object?>>(), null, candidates.ToArray(), lastEx?.Message);
    }

    private static List<string> BuildPaperIdCandidates(string? paperId, string? itemId)
    {
        var result = new List<string>();

        void Add(string? value)
        {
            var v = (value ?? string.Empty).Trim();
            if (v.Length == 0) return;
            if (!result.Any(x => string.Equals(x, v, StringComparison.OrdinalIgnoreCase)))
                result.Add(v);
        }

        // Keep legacy behavior first (table id), then try item id.
        Add(paperId);
        Add(itemId);

        return result;
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
