using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PcbErpApi.Pages.AJN
{
    public class AJNdClassMoneyModel : PageModel
    {
        // 之後要用到 DI 再加，例如：
        // private readonly IHttpClientFactory _http;
        // public CURdPaperInfoPageModel(IHttpClientFactory http) => _http = http;

        public void OnGet()
        {
            // 不需做事，畫面直接用 Partial 的設定產生
        }
    }
}