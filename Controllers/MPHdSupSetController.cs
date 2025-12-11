using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PcbErpApi.Data;

[ApiController]
[Route("api/[controller]")]
public class MPHdSupSetController : ControllerBase
{
    private readonly string _connStr;
    private readonly ILogger<MPHdSupSetController> _logger;

    public MPHdSupSetController(IConfiguration cfg, PcbErpContext db, ILogger<MPHdSupSetController> logger)
    {
        _connStr = cfg.GetConnectionString("Default")
                   ?? db?.Database.GetDbConnection().ConnectionString
                   ?? throw new InvalidOperationException("缺少資料庫連線字串");
        _logger = logger;
    }

    public record IdNameDto(string Id, string Name);
    public record MappingResponse(List<string> Items);
    public record SaveSupplierRequest(string SupId, List<string>? MatClasses);
    public record SaveMatClassRequest(string MatClass, List<string>? SupIds);

    [HttpGet("suppliers")]
    public async Task<ActionResult<IEnumerable<IdNameDto>>> GetSuppliers()
    {
        const string sql = @"
SELECT CompanyId AS Id,
       ISNULL(NULLIF(ShortName, ''), CompanyName) AS Name
  FROM dbo.AJNdMTLSupplier WITH (NOLOCK)
 ORDER BY CompanyId";

        var list = await QueryListAsync(sql, r =>
        {
            var id = r["Id"]?.ToString() ?? string.Empty;
            var name = r["Name"]?.ToString() ?? string.Empty;
            return new IdNameDto(id, string.IsNullOrWhiteSpace(name) ? id : name);
        });

        return Ok(list);
    }

    [HttpGet("mat-classes")]
    public async Task<ActionResult<IEnumerable<IdNameDto>>> GetMatClasses()
    {
        const string sql = @"
SELECT MatClass AS Id,
       ClassName AS Name
  FROM dbo.MINdMatClass WITH (NOLOCK)
 ORDER BY MatClass";

        var list = await QueryListAsync(sql, r =>
        {
            var id = r["Id"]?.ToString() ?? string.Empty;
            var name = r["Name"]?.ToString() ?? string.Empty;
            return new IdNameDto(id, string.IsNullOrWhiteSpace(name) ? id : name);
        });

        return Ok(list);
    }

    [HttpGet("supplier/{supId}/mat-classes")]
    public async Task<ActionResult<MappingResponse>> GetMappingsBySupplier(string supId)
    {
        if (string.IsNullOrWhiteSpace(supId))
            return BadRequest(new { ok = false, error = "缺少廠商代碼" });

        var items = await QueryListByProcAsync("MPHdSCItemsAddSup",
            r => (r["MatClass"]?.ToString() ?? string.Empty).Trim(),
            new SqlParameter("@MatClass", SqlDbType.Char, 8) { Value = string.Empty },
            new SqlParameter("@CompanyId", SqlDbType.VarChar, 16) { Value = supId.Trim() });

        var normalized = items
            .Select(x => (x ?? string.Empty).Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return Ok(new MappingResponse(normalized));
    }

    [HttpGet("mat-class/{matClass}/suppliers")]
    public async Task<ActionResult<MappingResponse>> GetMappingsByMatClass(string matClass)
    {
        if (string.IsNullOrWhiteSpace(matClass))
            return BadRequest(new { ok = false, error = "缺少分類代碼" });

        var items = await QueryListByProcAsync("MPHdSCItemsAddMat",
            r => (r["SupId"]?.ToString() ?? string.Empty).Trim(),
            new SqlParameter("@MatClass", SqlDbType.Char, 8) { Value = matClass.Trim() },
            new SqlParameter("@CompanyId", SqlDbType.VarChar, 16) { Value = string.Empty });

        var normalized = items
            .Select(x => (x ?? string.Empty).Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return Ok(new MappingResponse(normalized));
    }

    [HttpPost("supplier/save")]
    public async Task<IActionResult> SaveSupplier([FromBody] SaveSupplierRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.SupId))
            return BadRequest(new { ok = false, error = "缺少廠商代碼" });

        var supId = req.SupId.Trim();
        var matClasses = NormalizeList(req.MatClasses, 8).ToList();

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();
        await using var tran = await conn.BeginTransactionAsync();

        try
        {
            await using (var del = new SqlCommand("DELETE FROM dbo.MINdSCItems WHERE SupId = @supId", conn, (SqlTransaction)tran))
            {
                del.Parameters.Add(new SqlParameter("@supId", SqlDbType.VarChar, 16) { Value = supId });
                await del.ExecuteNonQueryAsync();
            }

            foreach (var mat in matClasses)
            {
                await using var ins = new SqlCommand("INSERT INTO dbo.MINdSCItems (SupId, MatClass) VALUES (@supId, @matClass)", conn, (SqlTransaction)tran);
                ins.Parameters.Add(new SqlParameter("@supId", SqlDbType.VarChar, 16) { Value = supId });
                ins.Parameters.Add(new SqlParameter("@matClass", SqlDbType.VarChar, 8) { Value = mat });
                await ins.ExecuteNonQueryAsync();
            }

            await ((SqlTransaction)tran).CommitAsync();
            return Ok(new { ok = true, count = matClasses.Count });
        }
        catch (Exception ex)
        {
            await ((SqlTransaction)tran).RollbackAsync();
            _logger.LogError(ex, "Save supplier mapping failed for SupId={SupId}", supId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { ok = false, error = ex.Message });
        }
    }

    [HttpPost("mat-class/save")]
    public async Task<IActionResult> SaveMatClass([FromBody] SaveMatClassRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.MatClass))
            return BadRequest(new { ok = false, error = "缺少分類代碼" });

        var matClass = req.MatClass.Trim();
        var supIds = NormalizeList(req.SupIds, 16).ToList();

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();
        await using var tran = await conn.BeginTransactionAsync();

        try
        {
            await using (var del = new SqlCommand("DELETE FROM dbo.MINdSCItems WHERE MatClass = @matClass", conn, (SqlTransaction)tran))
            {
                del.Parameters.Add(new SqlParameter("@matClass", SqlDbType.VarChar, 8) { Value = matClass });
                await del.ExecuteNonQueryAsync();
            }

            foreach (var sup in supIds)
            {
                await using var ins = new SqlCommand("INSERT INTO dbo.MINdSCItems (SupId, MatClass) VALUES (@supId, @matClass)", conn, (SqlTransaction)tran);
                ins.Parameters.Add(new SqlParameter("@supId", SqlDbType.VarChar, 16) { Value = sup });
                ins.Parameters.Add(new SqlParameter("@matClass", SqlDbType.VarChar, 8) { Value = matClass });
                await ins.ExecuteNonQueryAsync();
            }

            await ((SqlTransaction)tran).CommitAsync();
            return Ok(new { ok = true, count = supIds.Count });
        }
        catch (Exception ex)
        {
            await ((SqlTransaction)tran).RollbackAsync();
            _logger.LogError(ex, "Save mat-class mapping failed for MatClass={MatClass}", matClass);
            return StatusCode(StatusCodes.Status500InternalServerError, new { ok = false, error = ex.Message });
        }
    }

    private static IEnumerable<string> NormalizeList(IEnumerable<string>? values, int maxLength)
    {
        if (values == null) yield break;
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var v in values)
        {
            var val = (v ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(val)) continue;
            if (val.Length > maxLength) val = val[..maxLength];
            if (seen.Add(val))
                yield return val;
        }
    }

    private async Task<List<T>> QueryListAsync<T>(string sql, Func<IDataRecord, T> map, params SqlParameter[] parameters)
    {
        var result = new List<T>();
        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        if (parameters != null && parameters.Length > 0)
            cmd.Parameters.AddRange(parameters);

        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            result.Add(map(rd));
        }

        return result;
    }

    private async Task<List<T>> QueryListByProcAsync<T>(string procName, Func<IDataRecord, T> map, params SqlParameter[] parameters)
    {
        var result = new List<T>();
        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(procName, conn)
        {
            CommandType = CommandType.StoredProcedure
        };
        if (parameters != null && parameters.Length > 0)
            cmd.Parameters.AddRange(parameters);

        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            result.Add(map(rd));
        }
        return result;
    }
}
