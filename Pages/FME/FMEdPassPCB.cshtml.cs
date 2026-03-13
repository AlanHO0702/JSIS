using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PcbErpApi.Pages.FME;

/// <summary>
/// 單批過帳 (FME00021) PageModel
/// 不使用強型別 Model，資料由前端透過 API 取得
/// </summary>
public class FMEdPassPCBModel : PageModel
{
    private readonly ILogger<FMEdPassPCBModel> _logger;

    public FMEdPassPCBModel(ILogger<FMEdPassPCBModel> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 當前單號 (從 URL 參數取得)
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public string? PaperNum { get; set; }

    /// <summary>
    /// 當前使用者ID
    /// </summary>
    public string UserId { get; set; } = "admin";

    public void OnGet(string? paperNum)
    {
        // 從 Session 取得使用者ID
        UserId = HttpContext.Session.GetString("UserId") ?? "admin";
        PaperNum = paperNum;
    }
}
