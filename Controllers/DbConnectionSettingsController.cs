using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;

namespace PcbErpApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DbConnectionSettingsController : ControllerBase
{
    private readonly PcbErpContext _context;

    public DbConnectionSettingsController(PcbErpContext context)
    {
        _context = context;
    }

    public class BuDto
    {
        public string Buid { get; set; } = string.Empty;
        public string? Buname { get; set; }
        public string? Name { get; set; }
        public string? EnglishName { get; set; }
        public string? Address { get; set; }
        public string? EnglishAddr { get; set; }
        public string? Phone { get; set; }
        public string? Fax { get; set; }
        public string? Dbserver { get; set; }
        public string? Dbname { get; set; }
        public string? LoginName { get; set; }
        public string? LoginPwd { get; set; }
        public string? ReportServer { get; set; }
        public string? SocketServer { get; set; }
        public string? WebreportServer { get; set; }
        public string? WebreportShare { get; set; }
        public string? WebreportLocal { get; set; }
        public int? Butype { get; set; }
        public string? SuperId { get; set; }
        public int? LevelNo { get; set; }
        public int? Seq { get; set; }
        public int? IsCompany { get; set; }
    }

    public class BuSystemDto
    {
        public string Buid { get; set; } = string.Empty;
        public string SystemId { get; set; } = string.Empty;
        public string? Dbserver { get; set; }
        public string? Dbname { get; set; }
        public string? LoginName { get; set; }
        public string? LoginPwd { get; set; }
        public string? ReportServer { get; set; }
        public string? SocketServer { get; set; }
        public string? WebreportServer { get; set; }
        public string? WebreportShare { get; set; }
        public string? WebreportLocal { get; set; }
    }

    [HttpGet("bu")]
    public async Task<IActionResult> GetBuList()
    {
        var rows = await _context.CurdBus
            .AsNoTracking()
            .OrderBy(x => x.Seq ?? int.MaxValue)
            .ThenBy(x => x.Buid)
            .ToListAsync();

        return Ok(rows.Select(ToBuDto));
    }

    [HttpPost("bu")]
    public async Task<IActionResult> CreateBu([FromBody] BuDto dto)
    {
        var buid = NormalizeKey(dto.Buid);
        if (string.IsNullOrWhiteSpace(buid))
            return BadRequest(new { success = false, message = "BUId 不可空白" });

        var existed = await _context.CurdBus.AnyAsync(x => x.Buid == buid);
        if (existed)
            return Conflict(new { success = false, message = $"BUId [{buid}] 已存在" });

        var entity = new CurdBu
        {
            Buid = buid,
            Butype = dto.Butype ?? 0,
            LevelNo = dto.LevelNo ?? 0,
            BUseIdHead = 1,
            IsCompany = dto.IsCompany ?? 0
        };

        ApplyBu(entity, dto);
        _context.CurdBus.Add(entity);
        await _context.SaveChangesAsync();

        return Ok(new { success = true, buid });
    }

    [HttpPut("bu/{buid}")]
    public async Task<IActionResult> UpdateBu(string buid, [FromBody] BuDto dto)
    {
        var key = NormalizeKey(buid);
        var entity = await _context.CurdBus.FirstOrDefaultAsync(x => x.Buid == key);
        if (entity == null)
            return NotFound(new { success = false, message = $"找不到 BUId [{key}]" });

        ApplyBu(entity, dto);
        await _context.SaveChangesAsync();
        return Ok(new { success = true });
    }

    [HttpDelete("bu/{buid}")]
    public async Task<IActionResult> DeleteBu(string buid)
    {
        var key = NormalizeKey(buid);
        var entity = await _context.CurdBus.FirstOrDefaultAsync(x => x.Buid == key);
        if (entity == null)
            return Ok(new { success = true });

        var systems = await _context.CurdBuSystems.Where(x => x.Buid == key).ToListAsync();
        if (systems.Count > 0)
            _context.CurdBuSystems.RemoveRange(systems);

        _context.CurdBus.Remove(entity);
        await _context.SaveChangesAsync();

        return Ok(new { success = true });
    }

    [HttpGet("bu/{buid}/system")]
    public async Task<IActionResult> GetBuSystemList(string buid)
    {
        var key = NormalizeKey(buid);
        if (string.IsNullOrWhiteSpace(key))
            return Ok(Array.Empty<BuSystemDto>());

        var rows = await _context.CurdBuSystems
            .AsNoTracking()
            .Where(x => x.Buid == key)
            .OrderBy(x => x.SystemId)
            .ToListAsync();

        return Ok(rows.Select(ToBuSystemDto));
    }

    [HttpPost("system")]
    public async Task<IActionResult> CreateSystem([FromBody] BuSystemDto dto)
    {
        var buid = NormalizeKey(dto.Buid);
        var systemId = NormalizeKey(dto.SystemId);
        if (string.IsNullOrWhiteSpace(buid) || string.IsNullOrWhiteSpace(systemId))
            return BadRequest(new { success = false, message = "BUId / SystemId 不可空白" });

        var buExists = await _context.CurdBus.AnyAsync(x => x.Buid == buid);
        if (!buExists)
            return NotFound(new { success = false, message = $"找不到 BUId [{buid}]" });

        var existed = await _context.CurdBuSystems.AnyAsync(x => x.Buid == buid && x.SystemId == systemId);
        if (existed)
            return Conflict(new { success = false, message = $"系統別 [{systemId}] 已存在" });

        var entity = new CurdBuSystem
        {
            Buid = buid,
            SystemId = systemId
        };

        ApplySystem(entity, dto);
        _context.CurdBuSystems.Add(entity);
        await _context.SaveChangesAsync();

        return Ok(new { success = true, buid, systemId });
    }

    [HttpPut("system/{buid}/{systemId}")]
    public async Task<IActionResult> UpdateSystem(string buid, string systemId, [FromBody] BuSystemDto dto)
    {
        var buKey = NormalizeKey(buid);
        var sysKey = NormalizeKey(systemId);
        var entity = await _context.CurdBuSystems.FirstOrDefaultAsync(x => x.Buid == buKey && x.SystemId == sysKey);
        if (entity == null)
            return NotFound(new { success = false, message = $"找不到明細 [{buKey}/{sysKey}]" });

        ApplySystem(entity, dto);
        await _context.SaveChangesAsync();
        return Ok(new { success = true });
    }

    [HttpDelete("system/{buid}/{systemId}")]
    public async Task<IActionResult> DeleteSystem(string buid, string systemId)
    {
        var buKey = NormalizeKey(buid);
        var sysKey = NormalizeKey(systemId);
        var entity = await _context.CurdBuSystems.FirstOrDefaultAsync(x => x.Buid == buKey && x.SystemId == sysKey);
        if (entity == null)
            return Ok(new { success = true });

        _context.CurdBuSystems.Remove(entity);
        await _context.SaveChangesAsync();
        return Ok(new { success = true });
    }

    private static BuDto ToBuDto(CurdBu x)
    {
        return new BuDto
        {
            Buid = x.Buid?.Trim() ?? string.Empty,
            Buname = x.Buname?.Trim(),
            Name = x.Name?.Trim(),
            EnglishName = x.EnglishName?.Trim(),
            Address = x.Address?.Trim(),
            EnglishAddr = x.EnglishAddr?.Trim(),
            Phone = x.Phone?.Trim(),
            Fax = x.Fax?.Trim(),
            Dbserver = x.Dbserver?.Trim(),
            Dbname = x.Dbname?.Trim(),
            LoginName = x.LoginName?.Trim(),
            LoginPwd = x.LoginPwd?.Trim(),
            ReportServer = x.ReportServer?.Trim(),
            SocketServer = x.SocketServer?.Trim(),
            WebreportServer = x.WebreportServer?.Trim(),
            WebreportShare = x.WebreportShare?.Trim(),
            WebreportLocal = x.WebreportLocal?.Trim(),
            Butype = x.Butype,
            SuperId = x.SuperId?.Trim(),
            LevelNo = x.LevelNo,
            Seq = x.Seq,
            IsCompany = x.IsCompany
        };
    }

    private static BuSystemDto ToBuSystemDto(CurdBuSystem x)
    {
        return new BuSystemDto
        {
            Buid = x.Buid?.Trim() ?? string.Empty,
            SystemId = x.SystemId?.Trim() ?? string.Empty,
            Dbserver = x.Dbserver?.Trim(),
            Dbname = x.Dbname?.Trim(),
            LoginName = x.LoginName?.Trim(),
            LoginPwd = x.LoginPwd?.Trim(),
            ReportServer = x.ReportServer?.Trim(),
            SocketServer = x.SocketServer?.Trim(),
            WebreportServer = x.WebreportServer?.Trim(),
            WebreportShare = x.WebreportShare?.Trim(),
            WebreportLocal = x.WebreportLocal?.Trim()
        };
    }

    private static void ApplyBu(CurdBu entity, BuDto dto)
    {
        if (dto.Buname != null) entity.Buname = NormalizeNullable(dto.Buname);
        if (dto.Name != null) entity.Name = NormalizeNullable(dto.Name);
        if (dto.EnglishName != null) entity.EnglishName = NormalizeNullable(dto.EnglishName);
        if (dto.Address != null) entity.Address = NormalizeNullable(dto.Address);
        if (dto.EnglishAddr != null) entity.EnglishAddr = NormalizeNullable(dto.EnglishAddr);
        if (dto.Phone != null) entity.Phone = NormalizeNullable(dto.Phone);
        if (dto.Fax != null) entity.Fax = NormalizeNullable(dto.Fax);
        if (dto.Dbserver != null) entity.Dbserver = NormalizeNullable(dto.Dbserver);
        if (dto.Dbname != null) entity.Dbname = NormalizeNullable(dto.Dbname);
        if (dto.LoginName != null) entity.LoginName = NormalizeNullable(dto.LoginName);
        if (dto.LoginPwd != null) entity.LoginPwd = NormalizeNullable(dto.LoginPwd);
        if (dto.ReportServer != null) entity.ReportServer = NormalizeNullable(dto.ReportServer);
        if (dto.SocketServer != null) entity.SocketServer = NormalizeNullable(dto.SocketServer);
        if (dto.WebreportServer != null) entity.WebreportServer = NormalizeNullable(dto.WebreportServer);
        if (dto.WebreportShare != null) entity.WebreportShare = NormalizeNullable(dto.WebreportShare);
        if (dto.WebreportLocal != null) entity.WebreportLocal = NormalizeNullable(dto.WebreportLocal);
        if (dto.SuperId != null) entity.SuperId = NormalizeNullable(dto.SuperId);

        if (dto.Butype.HasValue) entity.Butype = dto.Butype.Value;
        if (dto.LevelNo.HasValue) entity.LevelNo = dto.LevelNo.Value;
        if (dto.Seq.HasValue) entity.Seq = dto.Seq.Value;
        if (dto.IsCompany.HasValue) entity.IsCompany = dto.IsCompany.Value;
    }

    private static void ApplySystem(CurdBuSystem entity, BuSystemDto dto)
    {
        if (dto.Dbserver != null) entity.Dbserver = NormalizeNullable(dto.Dbserver);
        if (dto.Dbname != null) entity.Dbname = NormalizeNullable(dto.Dbname);
        if (dto.LoginName != null) entity.LoginName = NormalizeNullable(dto.LoginName);
        if (dto.LoginPwd != null) entity.LoginPwd = NormalizeNullable(dto.LoginPwd);
        if (dto.ReportServer != null) entity.ReportServer = NormalizeNullable(dto.ReportServer);
        if (dto.SocketServer != null) entity.SocketServer = NormalizeNullable(dto.SocketServer);
        if (dto.WebreportServer != null) entity.WebreportServer = NormalizeNullable(dto.WebreportServer);
        if (dto.WebreportShare != null) entity.WebreportShare = NormalizeNullable(dto.WebreportShare);
        if (dto.WebreportLocal != null) entity.WebreportLocal = NormalizeNullable(dto.WebreportLocal);
    }

    private static string NormalizeKey(string? value) => (value ?? string.Empty).Trim();

    private static string? NormalizeNullable(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
