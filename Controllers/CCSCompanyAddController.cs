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
using System.Text;

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
                        SubSystemId = rd["SubSystemId"]?.ToString()?.Trim() ?? string.Empty,
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

    [HttpGet("query-first")]
    public async Task<IActionResult> QueryFirst([FromQuery] string itemId, [FromQuery] int? systemId = null)
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
            var whereParts = new List<string>
            {
                @"EXISTS (
                    SELECT 1
                    FROM AJNdCompanySystem s WITH (NOLOCK)
                    WHERE s.CompanyId = c.CompanyId
                      AND s.SystemId = @sysId
                )"
            };
            var parameters = new List<SqlParameter>
            {
                new("@sysId", SqlDbType.Int) { Value = querySystemId }
            };

            AppendCompanySearchFilters(whereParts, parameters, Request.Query, querySystemId);

            var whereSql = whereParts.Count > 0
                ? " WHERE " + string.Join(" AND ", whereParts)
                : string.Empty;

            var sql = $@"
SELECT TOP 1 c.CompanyId
FROM AJNdCompany c WITH (NOLOCK)
{whereSql}
ORDER BY c.CompanyId;";

            await using var cmd = new SqlCommand(sql, conn);
            foreach (var p in parameters) cmd.Parameters.Add(p);

            var companyId = (await cmd.ExecuteScalarAsync())?.ToString()?.Trim() ?? string.Empty;
            return Ok(new
            {
                success = true,
                companyId,
                found = !string.IsNullOrWhiteSpace(companyId),
                systemId = context.SystemId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CCSCompanyAdd query-first failed for {ItemId}", itemId);
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpGet("next-company-id")]
    public async Task<IActionResult> NextCompanyId([FromQuery] string itemId, [FromQuery] int? systemId = null)
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

            var numberSystemId = (context.SystemId == 103 || context.SystemId == 104) ? 1 : context.SystemId;
            var enCode = await GetSystemEnCodeAsync(conn, numberSystemId);
            if (string.IsNullOrWhiteSpace(enCode))
            {
                return Ok(new
                {
                    success = true,
                    companyId = string.Empty,
                    systemId = context.SystemId
                });
            }

            var nextByProcedure = await TryGetNextCompanyIdByProcedureAsync(conn, numberSystemId);
            if (!string.IsNullOrWhiteSpace(nextByProcedure)
                && !await GetCompanyExistsAsync(conn, nextByProcedure))
            {
                return Ok(new
                {
                    success = true,
                    companyId = nextByProcedure,
                    systemId = context.SystemId
                });
            }

            var codePattern = ResolveCodePattern(enCode, numberSystemId);
            if (string.IsNullOrWhiteSpace(codePattern.Prefix) || codePattern.DigitWidth <= 0)
            {
                return Ok(new
                {
                    success = true,
                    companyId = string.Empty,
                    systemId = context.SystemId
                });
            }

            var prefix = codePattern.Prefix;
            var maxNum = 0;
            var digitWidth = codePattern.DigitWidth;

            await using (var listCmd = new SqlCommand("exec AJNdCompanySystemSearch @SystemId", conn))
            {
                listCmd.Parameters.Add(new SqlParameter("@SystemId", SqlDbType.Int) { Value = numberSystemId });
                await using var rd = await listCmd.ExecuteReaderAsync();
                while (await rd.ReadAsync())
                {
                    var cid = (rd["CompanyId"]?.ToString() ?? string.Empty).Trim();
                    if (!cid.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) || cid.Length <= prefix.Length)
                        continue;

                    var digits = cid.Substring(prefix.Length);
                    if (!int.TryParse(digits, out var n))
                        continue;

                    if (n > maxNum)
                        maxNum = n;

                    if (digits.Length > digitWidth)
                        digitWidth = digits.Length;
                }
            }

            if (digitWidth <= 0)
            {
                digitWidth = string.Equals(prefix, "C", StringComparison.OrdinalIgnoreCase) ? 3 : 4;
            }

            var nextNum = maxNum + 1;
            var nextCompanyId = prefix + nextNum.ToString("D" + digitWidth);

            while (await GetCompanyExistsAsync(conn, nextCompanyId))
            {
                nextNum++;
                nextCompanyId = prefix + nextNum.ToString("D" + digitWidth);
            }

            return Ok(new
            {
                success = true,
                companyId = nextCompanyId,
                systemId = context.SystemId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CCSCompanyAdd next-company-id failed for {ItemId}", itemId);
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    private async Task<string?> TryGetNextCompanyIdByProcedureAsync(SqlConnection conn, int systemId)
    {
        var procName = await ResolveExistingProcedureAsync(conn, "CCSdGenSetNum");
        if (string.IsNullOrWhiteSpace(procName))
            return null;

        try
        {
            await using var cmd = new SqlCommand(procName, conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add(new SqlParameter("@SystemId", SqlDbType.Int) { Value = systemId });

            await using var rd = await cmd.ExecuteReaderAsync();
            do
            {
                while (await rd.ReadAsync())
                {
                    for (var i = 0; i < rd.FieldCount; i++)
                    {
                        if (rd.IsDBNull(i))
                            continue;

                        var val = (rd.GetValue(i)?.ToString() ?? string.Empty).Trim();
                        if (!string.IsNullOrWhiteSpace(val))
                            return val;
                    }
                }
            }
            while (await rd.NextResultAsync());
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "CCSdGenSetNum failed for SystemId={SystemId}, fallback to local numbering.", systemId);
        }

        return null;
    }

    private static async Task<string?> GetSystemEnCodeAsync(SqlConnection conn, int systemId)
    {
        await using var cmd = new SqlCommand(@"
select top 1 EnCode
from AJNdCompanySystemTable with(nolock)
where SystemId = @SystemId
  and IsNull(EnCode,'') <> '';", conn);
        cmd.Parameters.Add(new SqlParameter("@SystemId", SqlDbType.Int) { Value = systemId });
        var value = await cmd.ExecuteScalarAsync();
        return value == null || value == DBNull.Value ? null : value.ToString()?.Trim();
    }

    private static CodePattern ResolveCodePattern(string? enCode, int systemId)
    {
        var pattern = (enCode ?? string.Empty).Trim();
        if (!string.IsNullOrWhiteSpace(pattern))
        {
            var firstTilde = pattern.IndexOf('~');
            var lastTilde = pattern.LastIndexOf('~');
            if (firstTilde >= 0 && lastTilde >= firstTilde)
            {
                var prefix = pattern.Substring(0, firstTilde);
                var width = (lastTilde - firstTilde) + 1;
                if (!string.IsNullOrWhiteSpace(prefix) && width > 0)
                    return new CodePattern(prefix, width);
            }
        }

        return new CodePattern(string.Empty, 0);
    }

    private sealed class CodePattern
    {
        public CodePattern(string prefix, int digitWidth)
        {
            Prefix = prefix;
            DigitWidth = digitWidth;
        }

        public string Prefix { get; }
        public int DigitWidth { get; }
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
            if (companyExists && !req.Overwrite && !req.OverwriteChecked)
            {
                return Ok(new
                {
                    success = false,
                    needOverwriteConfirm = true,
                    code = "OVERWRITE_CONFIRM",
                    message = "Overwrite confirm required."
                });
            }

            var systemId = context.SystemId.ToString();
            var subSystemId = context.SubSystemId.ToString();

            if (await ExistsInSystemAsync(conn, companyId, systemId, subSystemId))
            {
                return Ok(new
                {
                    success = false,
                    code = "DUPLICATE_CODE",
                    message = "Company code already exists in same system/subsystem."
                });
            }

            if (!req.AllowOtherSubSystem
                && await ExistsInOtherSubSystemAsync(conn, companyId, systemId, subSystemId))
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

    [HttpPost("reject-edit")]
    public async Task<IActionResult> RejectEdit([FromBody] CcsBackReviewRequest req)
    {
        // backward-compatible route; keep old callers working.
        return await BackReview(req);
    }

    [HttpPost("back-review")]
    public async Task<IActionResult> BackReview([FromBody] CcsBackReviewRequest req)
    {
        var companyId = (req.CompanyId ?? string.Empty).Trim();
        if (companyId.Length == 0)
            return BadRequest(new { success = false, message = "CompanyId is required." });

        var userId = string.IsNullOrWhiteSpace(req.UserId) ? ResolveUserId() : req.UserId.Trim();
        var context = await LoadContextAsync((req.ItemId ?? string.Empty).Trim(), req.SystemId);
        var systemId = req.SystemId ?? context?.SystemId ?? 0;

        try
        {
            await using var conn = new SqlConnection(GetConnStr());
            await conn.OpenAsync();

            if (!string.IsNullOrWhiteSpace(req.RejectNotes))
            {
                await using var noteCmd = new SqlCommand("exec CURdRejNotes @PaperId, @Reason, @PaperNum, @ItemId, @UserId", conn);
                noteCmd.Parameters.Add(new SqlParameter("@PaperId", SqlDbType.VarChar, 50) { Value = "AJNdCompany" });
                noteCmd.Parameters.Add(new SqlParameter("@Reason", SqlDbType.NVarChar, 1000) { Value = req.RejectNotes.Trim() });
                noteCmd.Parameters.Add(new SqlParameter("@PaperNum", SqlDbType.VarChar, 16) { Value = companyId });
                noteCmd.Parameters.Add(new SqlParameter("@ItemId", SqlDbType.VarChar, 16) { Value = (req.ItemId ?? string.Empty).Trim() });
                noteCmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.VarChar, 30) { Value = userId });
                await noteCmd.ExecuteNonQueryAsync();
            }

            var procName = await ResolveExistingProcedureAsync(conn, "CCSdBackReview", "AJNdCompanyBackReview", "CCSdCompanyBackReview");
            if (string.IsNullOrWhiteSpace(procName))
            {
                return Ok(new { success = true, message = "此主檔不需退審，可直接編修" });
            }

            var affected = await ExecuteProcedureWithKnownParamsAsync(conn, procName, new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
            {
                ["CompanyId"] = companyId,
                ["PowerType"] = systemId,
                ["SystemId"] = systemId,
                ["UserId"] = userId
            });

            return Ok(new { success = true, message = "退審完成", affected });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CCSCompanyAdd back-review failed for {CompanyId}", companyId);
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpPost("delete-system-link")]
    public async Task<IActionResult> DeleteSystemLink([FromBody] CcsDeleteSystemLinkRequest req)
    {
        var companyId = (req.CompanyId ?? string.Empty).Trim();
        if (companyId.Length == 0)
            return BadRequest(new { success = false, message = "CompanyId is required." });

        var context = await LoadContextAsync((req.ItemId ?? string.Empty).Trim(), req.SystemId);
        var systemId = req.SystemId ?? context?.SystemId ?? 0;
        var subSystemId = req.SubSystemId ?? context?.SubSystemId ?? 0;
        if (systemId <= 0)
            return BadRequest(new { success = false, message = "SystemId is required." });

        try
        {
            await using var conn = new SqlConnection(GetConnStr());
            await conn.OpenAsync();

            await using var cmd = new SqlCommand(@"
delete AJNdCompanySystem
 where CompanyId = @CompanyId
   and SystemId = @SystemId
   and SubSystemId = @SubSystemId;", conn);
            cmd.Parameters.Add(new SqlParameter("@CompanyId", SqlDbType.VarChar, 16) { Value = companyId });
            cmd.Parameters.Add(new SqlParameter("@SystemId", SqlDbType.Int) { Value = systemId });
            cmd.Parameters.Add(new SqlParameter("@SubSystemId", SqlDbType.Int) { Value = subSystemId });
            var affected = await cmd.ExecuteNonQueryAsync();

            return Ok(new
            {
                success = true,
                message = affected > 0 ? "刪除完成" : "查無可刪除資料",
                affected
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CCSCompanyAdd delete-system-link failed for {CompanyId}", companyId);
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

    private static async Task<bool> ExistsInSystemAsync(SqlConnection conn, string companyId, string systemId, string subSystemId)
    {
        await using var cmd = new SqlCommand(@"
select top 1 1
from AJNdCompanySystem with(nolock)
where CompanyId = @CompanyId
  and SystemId = @SystemId
  and SubSystemId = @SubSystemId;", conn);
        cmd.Parameters.Add(new SqlParameter("@CompanyId", SqlDbType.VarChar, 16) { Value = companyId });
        cmd.Parameters.Add(new SqlParameter("@SystemId", SqlDbType.VarChar, 16) { Value = systemId });
        cmd.Parameters.Add(new SqlParameter("@SubSystemId", SqlDbType.VarChar, 16) { Value = subSystemId });
        return (await cmd.ExecuteScalarAsync()) != null;
    }

    private static async Task<bool> ExistsInOtherSubSystemAsync(SqlConnection conn, string companyId, string systemId, string subSystemId)
    {
        await using var cmd = new SqlCommand(@"
select top 1 1
from AJNdCompanySystem with(nolock)
where CompanyId = @CompanyId
  and SystemId = @SystemId
  and SubSystemId <> @SubSystemId;", conn);
        cmd.Parameters.Add(new SqlParameter("@CompanyId", SqlDbType.VarChar, 16) { Value = companyId });
        cmd.Parameters.Add(new SqlParameter("@SystemId", SqlDbType.VarChar, 16) { Value = systemId });
        cmd.Parameters.Add(new SqlParameter("@SubSystemId", SqlDbType.VarChar, 16) { Value = subSystemId });
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

    private static void AppendCompanySearchFilters(List<string> whereParts, List<SqlParameter> parameters, Microsoft.AspNetCore.Http.IQueryCollection query, int systemId)
    {
        static string Get(Microsoft.AspNetCore.Http.IQueryCollection q, string key) => (q[key].ToString() ?? string.Empty).Trim();
        static void AddLike(List<string> parts, List<SqlParameter> ps, string field, string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            var pn = $"@p{ps.Count}";
            parts.Add($"{field} LIKE {pn}");
            ps.Add(new SqlParameter(pn, $"%{value}%"));
        }

        var companyLike = Get(query, "cs_companyLike");
        if (!string.IsNullOrWhiteSpace(companyLike))
        {
            var pn = $"@p{parameters.Count}";
            whereParts.Add($"c.CompanyId LIKE {pn}");
            parameters.Add(new SqlParameter(pn, $"%{companyLike}%"));
        }

        var companyStart = Get(query, "cs_companyStart");
        if (!string.IsNullOrWhiteSpace(companyStart))
        {
            var pn = $"@p{parameters.Count}";
            whereParts.Add($"c.CompanyId >= {pn}");
            parameters.Add(new SqlParameter(pn, companyStart));
        }

        var companyEnd = Get(query, "cs_companyEnd");
        if (!string.IsNullOrWhiteSpace(companyEnd))
        {
            var pn = $"@p{parameters.Count}";
            whereParts.Add($"c.CompanyId <= {pn}");
            parameters.Add(new SqlParameter(pn, companyEnd));
        }

        AddLike(whereParts, parameters, "c.ShortName", Get(query, "cs_shortName"));
        AddLike(whereParts, parameters, "c.UniFormId", Get(query, "cs_uniFormId"));
        AddLike(whereParts, parameters, "c.CompanyName", Get(query, "cs_companyName"));
        AddLike(whereParts, parameters, "c.CompanyAddr", Get(query, "cs_companyAddr"));
        AddLike(whereParts, parameters, "c.BnsItem", Get(query, "cs_bnsItem"));

        var phone = Get(query, "cs_phone");
        if (!string.IsNullOrWhiteSpace(phone))
        {
            var cond = Get(query, "cs_phoneCond");
            var pn = $"@p{parameters.Count}";
            var keyword = string.Equals(cond, "1", StringComparison.OrdinalIgnoreCase)
                ? $"%{phone}%"
                : $"{phone}%";
            whereParts.Add($"(c.Phone1 LIKE {pn} OR c.Phone2 LIKE {pn})");
            parameters.Add(new SqlParameter(pn, keyword));
        }

        var fax = Get(query, "cs_fax");
        if (!string.IsNullOrWhiteSpace(fax))
        {
            var cond = Get(query, "cs_faxCond");
            var pn = $"@p{parameters.Count}";
            var keyword = string.Equals(cond, "1", StringComparison.OrdinalIgnoreCase)
                ? $"%{fax}%"
                : $"{fax}%";
            whereParts.Add($"(c.Fax1 LIKE {pn} OR c.Fax2 LIKE {pn})");
            parameters.Add(new SqlParameter(pn, keyword));
        }

        var subClass = Get(query, "cs_subClass");
        if (!string.IsNullOrWhiteSpace(subClass) && systemId >= 1 && systemId <= 9)
        {
            var field = $"CustomerSubClass{systemId}";
            var pn = $"@p{parameters.Count}";
            whereParts.Add($"c.[{field}] = {pn}");
            parameters.Add(new SqlParameter(pn, subClass));
        }

        var salesId = Get(query, "cs_salesId");
        if (!string.IsNullOrWhiteSpace(salesId))
        {
            var pn = $"@p{parameters.Count}";
            whereParts.Add($"c.SalesId = {pn}");
            parameters.Add(new SqlParameter(pn, salesId));
        }
    }

    private string GetConnStr()
    {
        var cs = _ctx.Database.GetConnectionString();
        if (string.IsNullOrWhiteSpace(cs))
            throw new InvalidOperationException("Connection string is not configured.");
        return cs;
    }

    private static async Task<string?> ResolveExistingProcedureAsync(SqlConnection conn, params string[] candidates)
    {
        if (candidates == null || candidates.Length == 0) return null;
        await using var cmd = new SqlCommand(@"
select top 1 o.name
from sys.objects o
where o.type in ('P','PC')
  and o.name in ({0});", conn);
        var names = new List<string>();
        for (var i = 0; i < candidates.Length; i++)
        {
            var pn = $"@p{i}";
            names.Add(pn);
            cmd.Parameters.AddWithValue(pn, candidates[i]);
        }
        cmd.CommandText = string.Format(cmd.CommandText, string.Join(",", names));
        var obj = await cmd.ExecuteScalarAsync();
        return obj == null || obj == DBNull.Value ? null : obj.ToString()?.Trim();
    }

    private static async Task<int> ExecuteProcedureWithKnownParamsAsync(SqlConnection conn, string procedureName, Dictionary<string, object?> values)
    {
        var paramNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        await using (var pCmd = new SqlCommand(@"
select p.name
from sys.parameters p
join sys.objects o on o.object_id = p.object_id
where o.type in ('P','PC') and o.name = @procName;", conn))
        {
            pCmd.Parameters.AddWithValue("@procName", procedureName);
            await using var rd = await pCmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                var name = rd.GetString(0).TrimStart('@');
                if (!string.IsNullOrWhiteSpace(name))
                    paramNames.Add(name);
            }
        }

        if (paramNames.Count == 0)
        {
            await using var noParam = new SqlCommand($"exec {procedureName}", conn);
            return await noParam.ExecuteNonQueryAsync();
        }

        var sql = new StringBuilder();
        sql.Append("exec ").Append(procedureName).Append(' ');
        var list = new List<string>();
        await using var cmd = new SqlCommand();
        cmd.Connection = conn;
        foreach (var kv in values)
        {
            if (!paramNames.Contains(kv.Key)) continue;
            var name = $"@{kv.Key}";
            list.Add($"{name}={name}");
            cmd.Parameters.AddWithValue(name, kv.Value ?? DBNull.Value);
        }
        sql.Append(string.Join(",", list));
        cmd.CommandText = sql.ToString();
        return await cmd.ExecuteNonQueryAsync();
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
        public string SubSystemId { get; set; } = string.Empty;
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
        public bool OverwriteChecked { get; set; }
        public bool AllowOtherSubSystem { get; set; }
    }

    public sealed class CcsBackReviewRequest
    {
        public string? CompanyId { get; set; }
        public string? ItemId { get; set; }
        public int? SystemId { get; set; }
        public string? UserId { get; set; }
        public string? RejectNotes { get; set; }
    }

    public sealed class CcsDeleteSystemLinkRequest
    {
        public string? CompanyId { get; set; }
        public string? ItemId { get; set; }
        public int? SystemId { get; set; }
        public int? SubSystemId { get; set; }
    }
}
