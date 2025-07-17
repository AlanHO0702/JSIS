using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;

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
        public string UserId { get; set; }
        public string Password { get; set; }
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] LoginRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.UserId))
            return BadRequest(new { error = "帳號不能為空" });

        // admin 密碼允許空白
        if (req.UserId == "admin")
        {
            var adminUser = await _context.CurdUsers
                .FirstOrDefaultAsync(u => u.UserId == "admin");
            if (adminUser != null && (string.IsNullOrEmpty(req.Password) || adminUser.UserPassword == req.Password))
            {
                return Ok(new { success = true });
            }
            return Unauthorized(new { error = "帳號或密碼錯誤" });
        }

        var user = await _context.CurdUsers
            .FirstOrDefaultAsync(u => u.UserId == req.UserId && u.UserPassword == req.Password);

        if (user == null)
            return Unauthorized(new { error = "帳號或密碼錯誤" });

        return Ok(new { success = true });
    }
}
