using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;
using System;

[ApiController]
[Route("api/[controller]/[action]")]
public class UserOnlineController : ControllerBase
{
    private readonly PcbErpContext _db;

    public UserOnlineController(PcbErpContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> KickUser([FromBody] Guid jwtId)
    {
        var item = await _db.CURdUserOnline.FindAsync(jwtId);
        if (item == null)
            return NotFound(new { error = "User not found" });

        _db.CURdUserOnline.Remove(item);
        await _db.SaveChangesAsync();
        return Ok(new { success = true });
    }

}
