using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;

namespace PcbErpApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CURdSystemSelectController : ControllerBase
{
    private readonly PcbErpContext _context;
    public CURdSystemSelectController(PcbErpContext context) => _context = context;

    public class SystemSelectRowDto
    {
        public string SystemId { get; set; } = string.Empty;
        public string SystemName { get; set; } = string.Empty;
        public int Selected { get; set; }
        public int IsDLL { get; set; }
        public int OrderNum { get; set; }
        public string? ModuleId { get; set; }
        public int SerialNum { get; set; }
        public string? GraphName { get; set; }
        public string? ManualName { get; set; }
        public string? SOPName { get; set; }
    }

    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 50;

        var q = _context.CurdSystemSelects.AsNoTracking();

        // 預設排序（你可以改成 OrderNum、SerialNum …）
        q = q.OrderBy(x => x.SystemId);

        var total = await q.CountAsync();
        var data = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return Ok(new { totalCount = total, data });
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        var viewNames = await _context.CurdVSystems
            .AsNoTracking()
            .ToDictionaryAsync(
                x => x.SystemId.Trim(),
                x => x.SystemName,
                StringComparer.OrdinalIgnoreCase);

        // 回補來源：有些系統代號不在 CURdVSystems，改由 Level0 功能名稱補齊
        var level0NameRows = await _context.CurdSysItems
            .AsNoTracking()
            .Where(x => x.LevelNo == 0 && x.SystemId != null && x.ItemName != null)
            .OrderBy(x => x.SerialNum)
            .Select(x => new { x.SystemId, x.ItemName })
            .ToListAsync();

        var level0Names = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var row in level0NameRows)
        {
            var key = row.SystemId?.Trim();
            var name = row.ItemName?.Trim();
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(name)) continue;
            if (!level0Names.ContainsKey(key))
                level0Names[key] = name;
        }

        var rows = await _context.CurdSystemSelects
            .AsNoTracking()
            .OrderBy(x => x.OrderNum)
            .ThenBy(x => x.SystemId)
            .ToListAsync();

        var result = rows.Select(x =>
        {
            var systemId = x.SystemId?.Trim() ?? string.Empty;
            return new SystemSelectRowDto
            {
                SystemId = systemId,
                SystemName = ResolveSystemName(systemId, viewNames, level0Names),
                Selected = x.Selected,
                IsDLL = x.IsDLL,
                OrderNum = x.OrderNum,
                ModuleId = x.ModuleId?.Trim(),
                SerialNum = x.SerialNum,
                GraphName = x.GraphName?.Trim(),
                ManualName = x.ManualName?.Trim(),
                SOPName = x.SOPName?.Trim()
            };
        });

        return Ok(result);
    }

    [HttpPut("{systemId}")]
    public async Task<IActionResult> Update(string systemId, [FromBody] SystemSelectRowDto dto)
    {
        var key = (systemId ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(key))
            return BadRequest(new { success = false, message = "SystemId 不可空白" });

        var entity = await _context.CurdSystemSelects.FirstOrDefaultAsync(x => x.SystemId == key);
        if (entity == null)
            return NotFound(new { success = false, message = $"找不到 SystemId [{key}]" });

        entity.Selected = dto.Selected;
        entity.IsDLL = dto.IsDLL;
        entity.OrderNum = dto.OrderNum;
        entity.ModuleId = NormalizeNullable(dto.ModuleId);
        entity.SerialNum = dto.SerialNum;
        entity.GraphName = NormalizeNullable(dto.GraphName);
        entity.ManualName = NormalizeNullable(dto.ManualName);
        entity.SOPName = NormalizeNullable(dto.SOPName);

        await _context.SaveChangesAsync();
        return Ok(new { success = true });
    }

    private static string? NormalizeNullable(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string ResolveSystemName(
        string systemId,
        IReadOnlyDictionary<string, string> viewNames,
        IReadOnlyDictionary<string, string> level0Names)
    {
        if (!string.IsNullOrWhiteSpace(systemId))
        {
            if (viewNames.TryGetValue(systemId, out var viewName) && !string.IsNullOrWhiteSpace(viewName))
                return viewName.Trim();

            if (level0Names.TryGetValue(systemId, out var level0Name) && !string.IsNullOrWhiteSpace(level0Name))
                return level0Name.Trim();
        }

        return string.Empty;
    }
}
