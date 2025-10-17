using Microsoft.AspNetCore.Mvc.RazorPages;

public class ReportPageModel : PageModel
{
    public string ReportTitle { get; set; } = "訂單未交貨明細表";

    public void OnGet()
    {
        // 頁面初始化時可載入預設值
    }
}
