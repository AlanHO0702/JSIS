using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PcbErpApi.Pages.CPN
{
    public class MGNdSetNumModel : PageModel
    {
        public string ItemId => "MG000006";
        public string PageTitle => "料號主分類及編碼設定";

        public void OnGet()
        {
            ViewData["Title"] = $"{ItemId} {PageTitle}";
        }
    }
}
