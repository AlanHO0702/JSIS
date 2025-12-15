using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PcbErpApi.Pages.CPN
{
    public class MGNdSpecDemandModel : PageModel
    {
        public string ItemId => "MGNdSpecDemand";
        public string PageTitle => "產品料號規格種類設定";

        public void OnGet()
        {
            ViewData["Title"] = $"{ItemId} {PageTitle}";
        }
    }
}
