using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using System.Data;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class LinkPaperController : ControllerBase
{
    private readonly string _cs;

    public LinkPaperController(IConfiguration cfg, PcbErpContext db)
    {
        _cs = cfg.GetConnectionString("DefaultConnection")
              ?? db.Database.GetDbConnection().ConnectionString;
    }

    [HttpGet("From")]
    public async Task<IActionResult> GetFrom(
        [FromQuery] string nowPaperId,
        [FromQuery] string nowPaperNum,
        [FromQuery] int? nowItem,
        [FromQuery] string sourPaperId,
        [FromQuery] string sourNum)
    {
        if (string.IsNullOrWhiteSpace(nowPaperId) ||
            string.IsNullOrWhiteSpace(nowPaperNum) ||
            nowItem is null ||
            string.IsNullOrWhiteSpace(sourPaperId) ||
            string.IsNullOrWhiteSpace(sourNum))
        {
            return Ok(new { ok = false, error = "參數不足" });
        }

        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync();

        var realNowPaperId = await ResolveRealTableNameAsync(conn, nowPaperId) ?? nowPaperId;
        var realSourPaperId = await ResolveRealTableNameAsync(conn, sourPaperId) ?? sourPaperId;

        const string sql = @"
exec CURdOCXLinkPaperGetByDtl
  @NowPaperId,
  @NowPaperNum,
  @NowItem,
  @SourPaperId,
  @SourNum,
  1";

        var rows = await QueryRowsAsync(conn, sql,
            new SqlParameter("@NowPaperId", realNowPaperId),
            new SqlParameter("@NowPaperNum", nowPaperNum),
            new SqlParameter("@NowItem", nowItem.Value),
            new SqlParameter("@SourPaperId", realSourPaperId),
            new SqlParameter("@SourNum", sourNum));

        return Ok(new { ok = true, rows });
    }

    [HttpGet("To")]
    public async Task<IActionResult> GetTo(
        [FromQuery] string nowPaperId,
        [FromQuery] string nowPaperNum,
        [FromQuery] int? nowItem,
        [FromQuery] string sourPaperId,
        [FromQuery] string sourNum,
        [FromQuery] int? sourItem)
    {
        if (string.IsNullOrWhiteSpace(nowPaperId) ||
            string.IsNullOrWhiteSpace(nowPaperNum) ||
            nowItem is null ||
            string.IsNullOrWhiteSpace(sourPaperId) ||
            string.IsNullOrWhiteSpace(sourNum) ||
            sourItem is null)
        {
            return Ok(new { ok = false, error = "參數不足" });
        }

        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync();

        var realNowPaperId = await ResolveRealTableNameAsync(conn, nowPaperId) ?? nowPaperId;
        var realSourPaperId = await ResolveRealTableNameAsync(conn, sourPaperId) ?? sourPaperId;

        const string sql = @"
exec CURdOCXLinkPaperGetFromTo
  @NowPaperId,
  @NowPaperNum,
  @NowItem,
  @SourPaperId,
  @SourNum,
  @SourItem";

        var rows = await QueryRowsAsync(conn, sql,
            new SqlParameter("@NowPaperId", realNowPaperId),
            new SqlParameter("@NowPaperNum", nowPaperNum),
            new SqlParameter("@NowItem", nowItem.Value),
            new SqlParameter("@SourPaperId", realSourPaperId),
            new SqlParameter("@SourNum", sourNum),
            new SqlParameter("@SourItem", sourItem.Value));

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

    private static async Task<string?> ResolveRealTableNameAsync(SqlConnection conn, string dictTableName)
    {
        const string sql = @"
SELECT TOP 1 ISNULL(NULLIF(RealTableName,''), TableName) AS ActualName
  FROM CURdTableName WITH (NOLOCK)
 WHERE TableName = @tbl;";
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@tbl", dictTableName ?? string.Empty);
        var obj = await cmd.ExecuteScalarAsync();
        return obj == null || obj == DBNull.Value ? null : obj.ToString();
    }
}
