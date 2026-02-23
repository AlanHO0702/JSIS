using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PcbErpApi.Data;

namespace PcbErpApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CCSCompanyAddController : ControllerBase
{
    private readonly PcbErpContext _ctx;
    private readonly ILogger<CCSCompanyAddController> _logger;

    public CCSCompanyAddController(PcbErpContext ctx, ILogger<CCSCompanyAddController> logger)
    {
        _ctx = ctx;
        _logger = logger;
    }

    [HttpGet("init")]
    public async Task<IActionResult> Init([FromQuery] string itemId, [FromQuery] int? systemId = null)
    {
        itemId = (itemId ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(itemId))
            return BadRequest(new { success = false, message = "itemId is required." });

        var context = await LoadContextAsync(itemId, systemId);
        if (context == null)
            return NotFound(new { success = false, message = $"Item {itemId} not found." });

        try
        {
            await using var conn = new SqlConnection(GetConnStr());
            await conn.OpenAsync();

            var emptyShortName = "0";
            await using (var cmd = new SqlCommand(@"
select Value
from CURdSysParams with(nolock)
where SystemId='CCS' and ParamId='EmptyShortName';", conn))
            {
                var val = await cmd.ExecuteScalarAsync();
                if (val != null && val != DBNull.Value)
                    emptyShortName = (val?.ToString() ?? "0").Trim();
            }

            var list = new List<AddListRow>();
            await using (var cmd = new SqlCommand("exec AJNdCompanySystemSearch @SystemId", conn))
            {
                cmd.Parameters.Add(new SqlParameter("@SystemId", SqlDbType.Int) { Value = context.SystemId });
                await using var rd = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);
                while (await rd.ReadAsync())
                {
                    list.Add(new AddListRow
                    {
                        CompanyId = rd["CompanyId"]?.ToString() ?? string.Empty,
                        ShortName = rd["ShortName"]?.ToString() ?? string.Empty
                    });
                }
            }

            return Ok(new
            {
                success = true,
                systemId = context.SystemId,
                paperType = context.PaperType,
                subSystemId = context.SubSystemId,
                emptyShortName,
                list
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CCSCompanyAdd init failed for {ItemId}", itemId);
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpGet("company")]
    public async Task<IActionResult> GetCompany([FromQuery] string companyId)
    {
        companyId = (companyId ?? string.Empty).Trim();
        if (companyId.Length == 0)
            return Ok(new { success = true, shortName = string.Empty });

        try
        {
            await using var conn = new SqlConnection(GetConnStr());
            await conn.OpenAsync();

            await using var cmd = new SqlCommand(@"
select ShortName
from AJNdCompany with(nolock)
where CompanyId = @CompanyId;", conn);
            cmd.Parameters.Add(new SqlParameter("@CompanyId", SqlDbType.VarChar, 16) { Value = companyId });

            var shortName = string.Empty;
            var val = await cmd.ExecuteScalarAsync();
            if (val != null && val != DBNull.Value)
                shortName = val.ToString() ?? string.Empty;

            return Ok(new { success = true, shortName });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CCSCompanyAdd company failed for {CompanyId}", companyId);
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpGet("check-shortname")]
    public async Task<IActionResult> CheckShortName([FromQuery] string shortName)
    {
        shortName = (shortName ?? string.Empty).Trim();
        if (shortName.Length == 0)
            return Ok(new { success = true, exists = false });

        try
        {
            await using var conn = new SqlConnection(GetConnStr());
            await conn.OpenAsync();

            await using var cmd = new SqlCommand(@"
select top 1 1
from AJNdCompany with(nolock)
where ShortName = @ShortName;", conn);
            cmd.Parameters.Add(new SqlParameter("@ShortName", SqlDbType.NVarChar, 50) { Value = shortName });
            var exists = (await cmd.ExecuteScalarAsync()) != null;

            return Ok(new { success = true, exists });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CCSCompanyAdd check-shortname failed for {ShortName}", shortName);
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpGet("query-init")]
    public async Task<IActionResult> QueryInit([FromQuery] string itemId, [FromQuery] int? systemId = null)
    {
        itemId = (itemId ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(itemId))
            return BadRequest(new { success = false, message = "itemId is required." });

        var context = await LoadContextAsync(itemId, systemId);
        if (context == null)
            return NotFound(new { success = false, message = $"Item {itemId} not found." });

        try
        {
            await using var conn = new SqlConnection(GetConnStr());
            await conn.OpenAsync();

            var querySystemId = (context.SystemId == 103 || context.SystemId == 104) ? 1 : context.SystemId;
            var showSales = context.SystemId == 1 || context.SystemId == 9 || context.SystemId == 103;

            var companyList = new List<AddListRow>();
            await using (var cmd = new SqlCommand("exec AJNdCompanySystemSearch @SystemId", conn))
            {
                cmd.Parameters.Add(new SqlParameter("@SystemId", SqlDbType.Int) { Value = querySystemId });
                await using var rd = await cmd.ExecuteReaderAsync();
                while (await rd.ReadAsync())
                {
                    companyList.Add(new AddListRow
                    {
                        CompanyId = rd["CompanyId"]?.ToString() ?? string.Empty,
                        ShortName = rd["ShortName"]?.ToString() ?? string.Empty
                    });
                }
            }

            var subClassList = new List<SubClassRow>();
            await using (var cmd = new SqlCommand(@"
select SubSystemId, SubSystemName
from AJNdCompanySubSystemTable with(nolock)
where SystemId = @SystemId
order by SubSystemId;", conn))
            {
                cmd.Parameters.Add(new SqlParameter("@SystemId", SqlDbType.Int) { Value = querySystemId });
                await using var rd = await cmd.ExecuteReaderAsync();
                while (await rd.ReadAsync())
                {
                    subClassList.Add(new SubClassRow
                    {
                        SubSystemId = Convert.ToInt32(rd["SubSystemId"]),
                        SubSystemName = rd["SubSystemName"]?.ToString() ?? string.Empty
                    });
                }
            }

            var users = new List<UserRow>();
            await using (var cmd = new SqlCommand("select UserId, UserName from CURdUsers with(nolock)", conn))
            await using (var rd = await cmd.ExecuteReaderAsync())
            {
                while (await rd.ReadAsync())
                {
                    users.Add(new UserRow
                    {
                        UserId = rd["UserId"]?.ToString() ?? string.Empty,
                        UserName = rd["UserName"]?.ToString() ?? string.Empty
                    });
                }
            }

            return Ok(new
            {
                success = true,
                systemId = context.SystemId,
                querySystemId,
                showSales,
                companyList,
                subClassList,
                users
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CCSCompanyAdd query-init failed for {ItemId}", itemId);
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpGet("show-system")]
    public async Task<IActionResult> ShowSystem([FromQuery] string companyId)
    {
        companyId = (companyId ?? string.Empty).Trim();
        if (companyId.Length == 0)
            return BadRequest(new { success = false, message = "companyId is required." });

        try
        {
            await using var conn = new SqlConnection(GetConnStr());
            await conn.OpenAsync();

            var result = new ShowSystemDto { CompanyId = companyId };

            await using (var cmd = new SqlCommand(@"
select top 1 CompanyId, CompanyName, ShortName
from AJNdCompany with(nolock)
where CompanyId = @CompanyId;", conn))
            {
                cmd.Parameters.Add(new SqlParameter("@CompanyId", SqlDbType.VarChar, 16) { Value = companyId });
                await using var rd = await cmd.ExecuteReaderAsync();
                if (await rd.ReadAsync())
                {
                    result.CompanyId = rd["CompanyId"]?.ToString() ?? companyId;
                    result.CompanyName = rd["CompanyName"]?.ToString() ?? string.Empty;
                    result.ShortName = rd["ShortName"]?.ToString() ?? string.Empty;
                }
            }

            await using (var cmd = new SqlCommand("exec CCSdShowSystem @CompanyId", conn))
            {
                cmd.Parameters.Add(new SqlParameter("@CompanyId", SqlDbType.VarChar, 16) { Value = companyId });
                await using var rd = await cmd.ExecuteReaderAsync();
                if (await rd.ReadAsync())
                {
                    result.Sys_1 = GetInt(rd, "Sys_1");
                    result.Sys_2 = GetInt(rd, "Sys_2");
                    result.Sys_3 = GetInt(rd, "Sys_3");
                    result.Sys_4 = GetInt(rd, "Sys_4");
                    result.Sys_5 = GetInt(rd, "Sys_5");
                    result.Sys_6 = GetInt(rd, "Sys_6");
                    result.Sys_7 = GetInt(rd, "Sys_7");
                    result.Sys_8 = GetInt(rd, "Sys_8");
                    result.Sys_9 = GetInt(rd, "Sys_9");
                }
            }

            await using (var cmd = new SqlCommand(@"
select top 1 1
from AJNdCompanySystem with(nolock)
where SystemId = 9;", conn))
            {
                result.HasSystem9 = (await cmd.ExecuteScalarAsync()) != null;
            }

            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CCSCompanyAdd show-system failed for {CompanyId}", companyId);
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpPost("add")]
    public async Task<IActionResult> Add([FromBody] AddCompanyRequest req)
    {
        var itemId = (req.ItemId ?? string.Empty).Trim();
        var companyId = (req.CompanyId ?? string.Empty).Trim();
        var companyName = (req.CompanyName ?? string.Empty).Trim();
        if (itemId.Length == 0 || companyId.Length == 0 || companyName.Length == 0)
            return BadRequest(new { success = false, message = "ItemId/CompanyId/CompanyName are required." });

        var context = await LoadContextAsync(itemId, req.SystemId);
        if (context == null)
            return NotFound(new { success = false, message = $"Item {itemId} not found." });

        try
        {
            await using var conn = new SqlConnection(GetConnStr());
            await conn.OpenAsync();

            var companyExists = await GetCompanyExistsAsync(conn, companyId);
            if (companyExists && !req.Overwrite)
            {
                return Ok(new
                {
                    success = false,
                    needOverwriteConfirm = true,
                    code = "OVERWRITE_CONFIRM",
                    message = "Overwrite confirm required."
                });
            }

            if (await ExistsInSystemAsync(conn, companyId, context.SystemId, context.SubSystemId))
            {
                return Ok(new
                {
                    success = false,
                    code = "DUPLICATE_CODE",
                    message = "Company code already exists in same system/subsystem."
                });
            }

            if (!req.AllowOtherSubSystem
                && await ExistsInOtherSubSystemAsync(conn, companyId, context.SystemId, context.SubSystemId))
            {
                return Ok(new
                {
                    success = false,
                    needOtherSubSystemConfirm = true,
                    code = "OTHER_SUBSYSTEM_CONFIRM",
                    message = "Code exists in another subsystem."
                });
            }

            var userId = ResolveUserId();
            await using var cmd = new SqlCommand(@"
exec AJNdCompanyAdd
  @CompanyId,
  @CompanyName,
  @SystemId,
  @PaperType,
  @OverWrite,
  @UserId;", conn);

            cmd.Parameters.Add(new SqlParameter("@CompanyId", SqlDbType.VarChar, 16) { Value = companyId });
            cmd.Parameters.Add(new SqlParameter("@CompanyName", SqlDbType.NVarChar, 120) { Value = companyName });
            cmd.Parameters.Add(new SqlParameter("@SystemId", SqlDbType.Int) { Value = context.SystemId });
            cmd.Parameters.Add(new SqlParameter("@PaperType", SqlDbType.Int) { Value = context.PaperType });
            cmd.Parameters.Add(new SqlParameter("@OverWrite", SqlDbType.Int) { Value = req.Overwrite ? 1 : 0 });
            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.VarChar, 16) { Value = userId });

            await cmd.ExecuteNonQueryAsync();

            return Ok(new
            {
                success = true,
                companyId,
                systemId = context.SystemId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CCSCompanyAdd add failed for {ItemId}/{CompanyId}", itemId, companyId);
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    private async Task<ItemContext?> LoadContextAsync(string itemId, int? requestedSystemId)
    {
        var item = await _ctx.CurdSysItems.AsNoTracking()
            .Where(x => x.ItemId == itemId)
            .Select(x => new { x.PowerType, x.PaperType })
            .FirstOrDefaultAsync();
        if (item == null) return null;

        var requested = NormalizeSystemId(requestedSystemId);
        var systemId = ResolveSystemId(item.PowerType, requested);
        var paperType = item.PaperType;
        var subSystemId = paperType == 255 ? 0 : Math.Max(0, paperType);

        return new ItemContext
        {
            ItemId = itemId,
            PowerType = item.PowerType,
            PaperType = paperType,
            SystemId = systemId,
            SubSystemId = subSystemId
        };
    }

    private static int NormalizeSystemId(int? id)
    {
        if (id.HasValue && id.Value > 0) return id.Value;
        return 0;
    }

    private static int ResolveSystemId(int? powerType, int requestedSystem)
    {
        if (requestedSystem > 0) return requestedSystem;
        if (!powerType.HasValue) return 1;

        var pt = powerType.Value;
        if (pt <= 100)
        {
            if (pt <= 50) return Math.Max(1, pt);
            return Math.Max(1, pt - 50);
        }
        if (pt == 101 || pt == 103 || pt == 104) return 1;
        return 1;
    }

    private async Task<bool> GetCompanyExistsAsync(SqlConnection conn, string companyId)
    {
        await using var cmd = new SqlCommand("exec CCSdCompanyExists @CompanyId", conn);
        cmd.Parameters.Add(new SqlParameter("@CompanyId", SqlDbType.VarChar, 16) { Value = companyId });
        await using var rd = await cmd.ExecuteReaderAsync();
        if (!await rd.ReadAsync()) return false;

        var val = rd["CompanyExists"]?.ToString()?.Trim();
        return string.Equals(val, "1", StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<bool> ExistsInSystemAsync(SqlConnection conn, string companyId, int systemId, int subSystemId)
    {
        await using var cmd = new SqlCommand(@"
select top 1 1
from AJNdCompanySystem with(nolock)
where CompanyId = @CompanyId
  and SystemId = @SystemId
  and SubSystemId = @SubSystemId;", conn);
        cmd.Parameters.Add(new SqlParameter("@CompanyId", SqlDbType.VarChar, 16) { Value = companyId });
        cmd.Parameters.Add(new SqlParameter("@SystemId", SqlDbType.Int) { Value = systemId });
        cmd.Parameters.Add(new SqlParameter("@SubSystemId", SqlDbType.Int) { Value = subSystemId });
        return (await cmd.ExecuteScalarAsync()) != null;
    }

    private static async Task<bool> ExistsInOtherSubSystemAsync(SqlConnection conn, string companyId, int systemId, int subSystemId)
    {
        await using var cmd = new SqlCommand(@"
select top 1 1
from AJNdCompanySystem with(nolock)
where CompanyId = @CompanyId
  and SystemId = @SystemId
  and SubSystemId <> @SubSystemId;", conn);
        cmd.Parameters.Add(new SqlParameter("@CompanyId", SqlDbType.VarChar, 16) { Value = companyId });
        cmd.Parameters.Add(new SqlParameter("@SystemId", SqlDbType.Int) { Value = systemId });
        cmd.Parameters.Add(new SqlParameter("@SubSystemId", SqlDbType.Int) { Value = subSystemId });
        return (await cmd.ExecuteScalarAsync()) != null;
    }

    private string ResolveUserId()
    {
        var claim = User?.Claims?.FirstOrDefault(c =>
            string.Equals(c.Type, "UserId", StringComparison.OrdinalIgnoreCase))?.Value;
        var item = HttpContext.Items["UserId"]?.ToString();
        var header = Request.Headers["X-UserId"].FirstOrDefault();
        return (claim ?? item ?? header ?? "Admin").Trim();
    }

    private string GetConnStr()
    {
        var cs = _ctx.Database.GetConnectionString();
        if (string.IsNullOrWhiteSpace(cs))
            throw new InvalidOperationException("Connection string is not configured.");
        return cs;
    }

    private static int GetInt(SqlDataReader rd, string fieldName)
    {
        try
        {
            var v = rd[fieldName];
            if (v == null || v == DBNull.Value) return 0;
            return Convert.ToInt32(v);
        }
        catch
        {
            return 0;
        }
    }

    private sealed class ItemContext
    {
        public string ItemId { get; set; } = string.Empty;
        public int? PowerType { get; set; }
        public int PaperType { get; set; }
        public int SystemId { get; set; }
        public int SubSystemId { get; set; }
    }

    private sealed class AddListRow
    {
        public string CompanyId { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
    }

    private sealed class SubClassRow
    {
        public int SubSystemId { get; set; }
        public string SubSystemName { get; set; } = string.Empty;
    }

    private sealed class UserRow
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
    }

    private sealed class ShowSystemDto
    {
        public string CompanyId { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public int Sys_1 { get; set; }
        public int Sys_2 { get; set; }
        public int Sys_3 { get; set; }
        public int Sys_4 { get; set; }
        public int Sys_5 { get; set; }
        public int Sys_6 { get; set; }
        public int Sys_7 { get; set; }
        public int Sys_8 { get; set; }
        public int Sys_9 { get; set; }
        public bool HasSystem9 { get; set; }
    }

    public sealed class AddCompanyRequest
    {
        public string? ItemId { get; set; }
        public int? SystemId { get; set; }
        public string? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public bool Overwrite { get; set; }
        public bool AllowOtherSubSystem { get; set; }
    }
}
