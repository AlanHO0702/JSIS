using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;

public class LoginModel : PageModel
{
    private readonly PcbErpContext _context;

    public LoginModel(PcbErpContext context)
    {
        _context = context;
    }

    [BindProperty]
    public LoginInputModel Input { get; set; } = new();

    public bool LoginFailed { get; set; } = false;

    public async Task<IActionResult> OnPostAsync()
    {
        try
    {
        if (!ModelState.IsValid)
            return Page();

        var user = await _context.CurdUsers
            .FirstOrDefaultAsync(u =>
                u.UserId == Input.UserId &&
                u.UserPassword == Input.Password);

        if (user == null)
        {
            LoginFailed = true;
            return Page();
        }

        return RedirectToPage("/Index");
    }
    catch (Exception ex)
    {
        ModelState.AddModelError(string.Empty, $"發生錯誤：{ex.Message}");
        return Page();
    }
    }
public class LoginInputModel
{
    [Required(ErrorMessage = "請輸入帳號")]
    public string UserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "請輸入密碼")]
    public string Password { get; set; } = string.Empty;
}
}
