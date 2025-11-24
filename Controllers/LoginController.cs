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
        public string HostName { get; set; }     // 前端傳
        public string ClientIp { get; set; }     // 前端傳
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] LoginRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.UserId))
            return BadRequest(new { error = "帳號不能為空" });

        // === 登入驗證 ===
        var user = await _context.CurdUsers
            .FirstOrDefaultAsync(u => u.UserId == req.UserId && 
                                      (req.UserId == "admin"
                                       ? (string.IsNullOrEmpty(req.Password) || u.UserPassword == req.Password)
                                       : u.UserPassword == req.Password));

        if (user == null)
            return Unauthorized(new { error = "帳號或密碼錯誤" });

        // ================
        //   ★ 新增線上紀錄
        // ================
        var jwtId = Guid.NewGuid();
        var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
        var shortUA = ShortenUserAgent(userAgent);
        var online = new CURdUserOnline
        {
            JwtId = jwtId,
            UserId = user.UserId,
            HostName = shortUA,
            ClientIp = req.ClientIp ?? HttpContext.Connection.RemoteIpAddress?.ToString(),
            LoginTime = DateTime.Now,
            LastActive = DateTime.Now
        };

        _context.CURdUserOnline.Add(online);
        await _context.SaveChangesAsync();

        // 前端目前不使用 JWT，因此只回傳 jwtId（之後 middleware 用）
        return Ok(new { success = true, jwtId });
    }
    private string ShortenUserAgent(string ua)
    {
        if (string.IsNullOrEmpty(ua)) return "";

        string browser = "Browser";
        string os = "OS";

        // 瀏覽器判斷
        if (ua.Contains("Edg/"))
            browser = "Edge " + GetVersion(ua, "Edg/");
        else if (ua.Contains("Chrome/"))
            browser = "Chrome " + GetVersion(ua, "Chrome/");
        else if (ua.Contains("Firefox/"))
            browser = "Firefox " + GetVersion(ua, "Firefox/");
        else if (ua.Contains("Safari/") && ua.Contains("Version/"))
            browser = "Safari " + GetVersion(ua, "Version/");

        // OS 判斷
        if (ua.Contains("Windows NT 10"))
            os = "Windows 10";
        else if (ua.Contains("Windows NT 11"))
            os = "Windows 11";
        else if (ua.Contains("Windows NT"))
            os = "Windows";
        else if (ua.Contains("Android"))
            os = "Android";
        else if (ua.Contains("iPhone"))
            os = "iOS";

        return $"{browser} ({os})";
    }

    private string GetVersion(string ua, string key)
    {
        try
        {
            int i = ua.IndexOf(key);
            if (i == -1) return "";
            i += key.Length;
            int j = ua.IndexOf(".", i);
            if (j == -1) j = ua.Length;
            return ua.Substring(i, j - i);
        }
        catch { return ""; }
    }

    [HttpPost("Ping")]
    public async Task<IActionResult> Ping([FromHeader(Name="X-JWTID")] string jwt)
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
