using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PcbErpApi.Data;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class MG000006Controller : ControllerBase
{
    private readonly string _connStr;
    private readonly ILogger<MG000006Controller> _logger;

    public MG000006Controller(IConfiguration cfg, PcbErpContext db, ILogger<MG000006Controller> logger)
    {
        _connStr = cfg.GetConnectionString("Default")
                   ?? db?.Database.GetDbConnection().ConnectionString
                   ?? throw new InvalidOperationException("缺少資料庫連線字串");
        _logger = logger;
    }

    public record KeyRequest(string? SetClass, string? NumId);
    public record TestRequest(string? SetClass);
    public record ImportRequest(string? SetClass, string? NumId);
    public record CopyRequest(string? SetClass, string? NumId, string? UserId, int? IsMust, int? IsHand);
    public record DictWidthDto(string TableName, string FieldName, int Width);

    [HttpGet("mat-class")]
    public async Task<ActionResult<IEnumerable<IDictionary<string, object?>>>> GetMatClassAsync()
    {
        const string sql = @"SELECT * FROM dbo.MINdMatClass WITH (NOLOCK) ORDER BY MatClass";
        var list = await QueryListAsync(sql);
        return Ok(list);
    }

    [HttpDelete("mat-class/{matClass}")]
    public async Task<IActionResult> DeleteMatClassAsync(string matClass)
    {
        if (string.IsNullOrWhiteSpace(matClass))
            return BadRequest(new { ok = false, error = "缺少 MatClass" });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand("DELETE FROM dbo.MINdMatClass WHERE MatClass = @matClass", conn);
        cmd.Parameters.Add(new SqlParameter("@matClass", SqlDbType.VarChar, 8) { Value = matClass.Trim() });
        var affected = await cmd.ExecuteNonQueryAsync();
        return Ok(new { ok = affected > 0, affected });
    }

    [HttpGet("setnum-main")]
    public async Task<ActionResult<IEnumerable<IDictionary<string, object?>>>> GetSetNumMainAsync()
    {
        const string sql = @"
SELECT m.*,
       c.ClassName
  FROM dbo.MGNdSetNumMain m WITH (NOLOCK)
  LEFT JOIN dbo.MINdMatClass c WITH (NOLOCK)
    ON c.MatClass = m.SetClass
 ORDER BY m.SetClass";
        var list = await QueryListAsync(sql);
        return Ok(list);
    }

    [HttpDelete("setnum-main/{setClass}")]
    public async Task<IActionResult> DeleteSetNumMainAsync(string setClass)
    {
        if (string.IsNullOrWhiteSpace(setClass))
            return BadRequest(new { ok = false, error = "缺少 SetClass" });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand("DELETE FROM dbo.MGNdSetNumMain WHERE SetClass = @setClass", conn);
        cmd.Parameters.Add(new SqlParameter("@setClass", SqlDbType.VarChar, 12) { Value = setClass.Trim() });
        var affected = await cmd.ExecuteNonQueryAsync();
        return Ok(new { ok = affected > 0, affected });
    }

    [HttpGet("setnum-sub")]
    public async Task<ActionResult<IEnumerable<IDictionary<string, object?>>>> GetSetNumSubAsync([FromQuery] string? setClass)
    {
        if (string.IsNullOrWhiteSpace(setClass))
            return BadRequest(new { ok = false, error = "缺少 SetClass" });

        const string sql = @"SELECT * FROM dbo.MGNdSetNumSub WITH (NOLOCK) WHERE SetClass = @setClass ORDER BY NumId";
        var list = await QueryListAsync(sql, new SqlParameter("@setClass", SqlDbType.VarChar, 12) { Value = setClass.Trim() });
        return Ok(list);
    }

    [HttpDelete("setnum-sub")]
    public async Task<IActionResult> DeleteSetNumSubAsync([FromQuery] string? setClass, [FromQuery] string? numId)
    {
        if (string.IsNullOrWhiteSpace(setClass) || string.IsNullOrWhiteSpace(numId))
            return BadRequest(new { ok = false, error = "缺少 SetClass 或 NumId" });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand("DELETE FROM dbo.MGNdSetNumSub WHERE SetClass = @setClass AND NumId = @numId", conn);
        cmd.Parameters.Add(new SqlParameter("@setClass", SqlDbType.VarChar, 12) { Value = setClass.Trim() });
        cmd.Parameters.Add(new SqlParameter("@numId", SqlDbType.Char, 1) { Value = numId.Trim() });
        var affected = await cmd.ExecuteNonQueryAsync();
        return Ok(new { ok = affected > 0, affected });
    }

    [HttpGet("setnum-subdtl")]
    public async Task<ActionResult<IEnumerable<IDictionary<string, object?>>>> GetSetNumSubDtlAsync([FromQuery] string? setClass, [FromQuery] string? numId)
    {
        if (string.IsNullOrWhiteSpace(setClass) || string.IsNullOrWhiteSpace(numId))
            return BadRequest(new { ok = false, error = "缺少 SetClass 或 NumId" });

        const string sql = @"SELECT * FROM dbo.MGNdSetNumSubDtl WITH (NOLOCK) WHERE SetClass = @setClass AND NumId = @numId ORDER BY EnCode";
        var list = await QueryListAsync(
            sql,
            new SqlParameter("@setClass", SqlDbType.VarChar, 12) { Value = setClass.Trim() },
            new SqlParameter("@numId", SqlDbType.Char, 1) { Value = numId.Trim() });
        return Ok(list);
    }

    [HttpDelete("setnum-subdtl")]
    public async Task<IActionResult> DeleteSetNumSubDtlAsync([FromQuery] string? setClass, [FromQuery] string? numId, [FromQuery] string? encode)
    {
        if (string.IsNullOrWhiteSpace(setClass) || string.IsNullOrWhiteSpace(numId) || string.IsNullOrWhiteSpace(encode))
            return BadRequest(new { ok = false, error = "缺少 SetClass / NumId / EnCode" });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand("DELETE FROM dbo.MGNdSetNumSubDtl WHERE SetClass = @setClass AND NumId = @numId AND EnCode = @encode", conn);
        cmd.Parameters.Add(new SqlParameter("@setClass", SqlDbType.VarChar, 12) { Value = setClass.Trim() });
        cmd.Parameters.Add(new SqlParameter("@numId", SqlDbType.Char, 1) { Value = numId.Trim() });
        cmd.Parameters.Add(new SqlParameter("@encode", SqlDbType.VarChar, 24) { Value = encode.Trim() });
        var affected = await cmd.ExecuteNonQueryAsync();
        return Ok(new { ok = affected > 0, affected });
    }

    [HttpPost("test-number")]
    public async Task<ActionResult<object>> TestNumberAsync([FromBody] TestRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.SetClass))
            return BadRequest(new { ok = false, error = "缺少 SetClass" });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand("MGNdGenSetTestNum", conn)
        {
            CommandType = CommandType.StoredProcedure
        };
        cmd.Parameters.Add(new SqlParameter("@SetClass", SqlDbType.VarChar, 12) { Value = req.SetClass.Trim() });

        try
        {
            var result = await cmd.ExecuteScalarAsync();
            return Ok(new { ok = true, result = result?.ToString() ?? string.Empty });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MG000006 test-number failed");
            return StatusCode(StatusCodes.Status500InternalServerError, new { ok = false, error = ex.Message });
        }
    }

    [HttpPost("import-mapping")]
    public async Task<IActionResult> ImportMappingAsync([FromBody] ImportRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.SetClass) || string.IsNullOrWhiteSpace(req?.NumId))
            return BadRequest(new { ok = false, error = "缺少 SetClass 或 NumId" });

        var sql = "exec MGNdSetNumSubDtlImp @SetClass, @NumId";

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add(new SqlParameter("@SetClass", SqlDbType.VarChar, 12) { Value = req.SetClass.Trim() });
        cmd.Parameters.Add(new SqlParameter("@NumId", SqlDbType.Char, 1) { Value = req.NumId.Trim() });

        try
        {
            await cmd.ExecuteNonQueryAsync();
            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MG000006 import-mapping failed, SetClass={SetClass}, NumId={NumId}", req.SetClass, req.NumId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { ok = false, error = ex.Message });
        }
    }

    [HttpPost("copy-to-mat")]
    public async Task<IActionResult> CopyToMatAsync([FromBody] CopyRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.SetClass) || string.IsNullOrWhiteSpace(req?.NumId))
            return BadRequest(new { ok = false, error = "缺少 SetClass 或 NumId" });

        var sql = "exec MGNdSetNumSubCopyToMat @SetClass, @NumId, @UserId, @IsMust, @IsHand";

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add(new SqlParameter("@SetClass", SqlDbType.VarChar, 12) { Value = req.SetClass.Trim() });
        cmd.Parameters.Add(new SqlParameter("@NumId", SqlDbType.Char, 1) { Value = req.NumId.Trim() });
        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.VarChar, 16) { Value = (req.UserId ?? string.Empty).Trim() });
        cmd.Parameters.Add(new SqlParameter("@IsMust", SqlDbType.Int) { Value = req.IsMust ?? 0 });
        cmd.Parameters.Add(new SqlParameter("@IsHand", SqlDbType.Int) { Value = req.IsHand ?? 0 });

        try
        {
            var count = await cmd.ExecuteNonQueryAsync();
            return Ok(new { ok = true, count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MG000006 copy-to-mat failed, SetClass={SetClass}, NumId={NumId}", req.SetClass, req.NumId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { ok = false, error = ex.Message });
        }
    }

    [HttpGet("dict-widths")]
    public async Task<ActionResult<IEnumerable<DictWidthDto>>> GetDictWidthsAsync()
    {
        var tables = new[] { "MGN_MINdMatClass", "MGNdSetNumMain", "MGNdSetNumSub", "MGNdSetNumSubDtl" };
        var tableList = string.Join(",", tables.Select(t => $"'{t}'"));
        var sql = $@"
SELECT TableName, FieldName, ISNULL(iFieldWidth, 0) AS W, ISNULL(DisplaySize, 0) AS DS
  FROM CURdTableField WITH (NOLOCK)
 WHERE TableName IN ({tableList})";

        var list = new List<DictWidthDto>();

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            var tableName = rd["TableName"]?.ToString() ?? string.Empty;
            var fieldName = rd["FieldName"]?.ToString() ?? string.Empty;
            var w = rd["W"] as int? ?? Convert.ToInt32(rd["W"]);
            var ds = rd["DS"] as int? ?? Convert.ToInt32(rd["DS"]);

            var width = w > 0 ? w : (ds > 0 ? ds * 10 : 0);
            if (width <= 0) continue;
            list.Add(new DictWidthDto(tableName, fieldName, width));
        }

        return Ok(list);
    }

    private async Task<List<IDictionary<string, object?>>> QueryListAsync(string sql, params SqlParameter[] parameters)
    {
        var list = new List<IDictionary<string, object?>>();

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        if (parameters != null && parameters.Length > 0)
            cmd.Parameters.AddRange(parameters);

        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < rd.FieldCount; i++)
            {
                row[rd.GetName(i)] = rd.IsDBNull(i) ? null : rd.GetValue(i);
            }
            list.Add(row);
        }

        return list;
    }
}
