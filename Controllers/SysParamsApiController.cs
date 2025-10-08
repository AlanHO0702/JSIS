// Controllers/SysParamsApiController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;

[ApiController]
[Route("api/[controller]")]
public class SysParamsApiController : ControllerBase
{
    private readonly PcbErpContext _ctx;
    public SysParamsApiController(PcbErpContext ctx) { _ctx = ctx; }

    public class RowDto
    {
        public string SystemId { get; set; } = "";
        public string ParamId  { get; set; } = "";
        public string? Notes   { get; set; }
        public string? Value   { get; set; }
        public int ParamType   { get; set; }
        public int ComboStyle  { get; set; }
        public int IsLock      { get; set; }
        public int AllowUserUpdate { get; set; }
        public string? Note2   { get; set; }
    }

    [HttpPost("update-rows")]
    public async Task<IActionResult> UpdateRows([FromBody] List<RowDto> rows)
    {
        if (rows == null || rows.Count == 0)
            return Ok(new { success = true, count = 0 });

        foreach (var r in rows)
        {
            var sys = (r.SystemId ?? "").Trim();
            var id  = (r.ParamId  ?? "").Trim();
            var db  = await _ctx.CURdSysParams.FindAsync(sys, id);
            if (db == null)
                return NotFound(new { success = false, message = $"找不到 {sys}-{id}" });

            db.Notes            = r.Notes;
            db.Value            = r.Value;
            db.ParamType        = r.ParamType;
            db.ComboStyle       = r.ComboStyle;
            db.IsLock           = r.IsLock;
            db.AllowUserUpdate  = r.AllowUserUpdate;
            db.Note2            = r.Note2;
            db.LastDate         = DateTime.Now;
        }

        var n = await _ctx.SaveChangesAsync();
        return Ok(new { success = true, count = n });
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] RowDto dto)
    {
        var sys = (dto.SystemId ?? "").Trim();
        var id  = (dto.ParamId  ?? "").Trim();
        if (string.IsNullOrEmpty(sys) || string.IsNullOrEmpty(id))
            return BadRequest(new { success = false, message = "SystemId/ParamId 必填" });

        var exists = await _ctx.CURdSysParams.FindAsync(sys, id);
        if (exists != null)
            return Conflict(new { success = false, message = "同鍵值已存在" });

        var e = new CURdSysParams
        {
            SystemId        = sys,
            ParamId         = id,
            Notes           = dto.Notes,
            Value           = dto.Value,
            ParamType       = dto.ParamType,
            ComboStyle      = dto.ComboStyle,
            IsLock          = dto.IsLock,
            AllowUserUpdate = dto.AllowUserUpdate,
            Note2           = dto.Note2,
            LastDate        = DateTime.Now
        };
        _ctx.Add(e);
        await _ctx.SaveChangesAsync();
        return Ok(new { success = true, row = e });
    }
}
