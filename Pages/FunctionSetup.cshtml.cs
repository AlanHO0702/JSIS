using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PcbErpApi.Pages;

public class FunctionSetupModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string ItemId { get; set; } = "";

    [BindProperty(SupportsGet = true)]
    public string Level1Id { get; set; } = "";

    public void OnGet()
    {
        ItemId = (ItemId ?? string.Empty).Trim();
        Level1Id = (Level1Id ?? string.Empty).Trim();
    }
}

