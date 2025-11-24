using Microsoft.AspNetCore.Http;
using PcbErpApi.Data;
using System;
using System.Net;
using System.Threading.Tasks;

public class UserOnlineMiddleware
{
    private readonly RequestDelegate _next;

    public UserOnlineMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, PcbErpContext db)
    {
        // 取得 JWTID（只有登入後的請求才會更新）
        if (context.Request.Headers.TryGetValue("X-JWTID", out var jwtIdStr))
        {
            if (Guid.TryParse(jwtIdStr, out Guid jwtId))
            {
                var item = await db.CURdUserOnline.FindAsync(jwtId);
                if (item != null)
                {
                    // 1️⃣ 取得 real client IP（含 proxy）
                    var ip = GetClientIp(context);
                    item.ClientIp = ip;

                    // 2️⃣ 反查 computer hostname
                    item.HostName = ResolveHostName(ip);

                    // 避免太長（例如有人 fake user agent）
                    if (item.HostName.Length > 120)
                        item.HostName = item.HostName[..120];

                    item.LastActive = DateTime.Now;

                    await db.SaveChangesAsync();
                }
            }
        }

        await _next(context);
    }

    // 取得用戶真實 IP
    private string GetClientIp(HttpContext context)
    {
        string ip = context.Request.Headers["X-Forwarded-For"];
        if (!string.IsNullOrEmpty(ip))
            return ip.Split(',')[0].Trim();

        return context.Connection.RemoteIpAddress?.ToString() ?? "";
    }

    // 反查電腦主機名（如果是 ::1 / 127.0.0.1 回傳 Localhost）
    private string ResolveHostName(string ip)
    {
        try
        {
            if (string.IsNullOrEmpty(ip))
                return "Unknown";

            if (ip == "::1" || ip == "127.0.0.1")
                return "Localhost";

            var entry = Dns.GetHostEntry(ip);
            return entry.HostName;
        }
        catch
        {
            return ip; // 查不到就回傳 IP 字串
        }
    }
}
