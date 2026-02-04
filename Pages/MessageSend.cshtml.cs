using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PcbErpApi.Pages
{
    public class MessageSendModel : PageModel
    {
        public string CurrentUserId { get; set; } = "";

        public void OnGet(string? userId = null)
        {
            userId = HttpContext.Items["UserId"]?.ToString()
                     ?? userId
                     ?? User.Identity?.Name
                     ?? "admin";
            CurrentUserId = userId;
        }
    }
}
