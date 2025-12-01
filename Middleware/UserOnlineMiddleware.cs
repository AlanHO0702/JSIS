using Microsoft.AspNetCore.Http;
using PcbErpApi.Data;
using System;
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
        if (context.Request.Headers.TryGetValue("X-JWTID", out var jwtIdStr))
        {
            if (Guid.TryParse(jwtIdStr, out Guid jwtId))
            {
                var rec = await db.CURdUserOnline.FindAsync(jwtId);

                if (rec == null)
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("SessionExpired");
                    return;
                }

                rec.LastActive = DateTime.Now;
                await db.SaveChangesAsync();
            }
        }

        await _next(context);
    }



}
