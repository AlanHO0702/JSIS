using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;
using System;

[ApiController]
[Route("api/[controller]")]
public class LoginController : ControllerBase
{
    private readonly PcbErpContext _context;

    public LoginController(PcbErpContext context)
    {
        _context = context;
    }

    public class LoginRequest
    {
        public string UserId { get; set; } = "";
        public string Password { get; set; } = "";
        public string? HostName { get; set; } = "";   // 🔥 裝置識別碼
        public string? ClientIp { get; set; } = "";
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] LoginRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.UserId))
            return BadRequest(new { error = "帳號不能為空" });

        var user = await _context.CurdUsers
            .FirstOrDefaultAsync(u => u.UserId == req.UserId &&
                                      (req.UserId == "admin"
                                          ? (string.IsNullOrEmpty(req.Password) || u.UserPassword == req.Password)
                                          : u.UserPassword == req.Password));

        if (user == null)
            return Unauthorized(new { error = "帳號或密碼錯誤" });

        // 🔥🔥 不允許同裝置重複登入（判斷 hostName）
        bool exists = await _context.CURdUserOnline
            .AnyAsync(x =>
                x.HostName == req.HostName &&
                x.LastActive >= DateTime.Now.AddMinutes(-30));

        //if (exists)
            //return BadRequest(new { error = "此電腦已登入，不可重複登入。" });
            // 目前有效在線人數
        var onlineCount = _context.CURdUserOnline
            .Count(x => x.LastActive >= DateTime.Now.AddMinutes(-5));

        if (onlineCount >= 30)
        {
            return BadRequest(new { error = "超過授權人數 30 人，請稍後再試。" });
        }


        var jwtId = Guid.NewGuid();

        var online = new CURdUserOnline
        {
            JwtId = jwtId,
            UserId = user.UserId,
            HostName = req.HostName,                  // 🔥 使用前端傳入固定 HostName
            ClientIp = req.ClientIp ?? HttpContext.Connection.RemoteIpAddress?.ToString(),
            LoginTime = DateTime.Now,
            LastActive = DateTime.Now
        };

        // 清除同使用者、同 HostName 的舊紀錄
        var oldRecords = _context.CURdUserOnline
            .Where(x => x.UserId == user.UserId && x.HostName == req.HostName);

        _context.CURdUserOnline.RemoveRange(oldRecords);
        await _context.SaveChangesAsync();

        _context.CURdUserOnline.Add(online);
        await _context.SaveChangesAsync();

        return Ok(new { success = true, jwtId });
    }

    // Ping 更新緩存時間
    [HttpPost("Ping")]
    public async Task<IActionResult> Ping([FromHeader(Name = "X-JWTID")] string jwt)
    {
        if (Guid.TryParse(jwt, out Guid jwtId))
        {
            var item = await _context.CURdUserOnline.FindAsync(jwtId);
            if (item != null)
            {
                item.LastActive = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }
        return Ok(new { success = true });
    }
}
