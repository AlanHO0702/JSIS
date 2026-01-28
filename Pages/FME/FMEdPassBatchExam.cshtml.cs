using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PcbErpApi.Pages.FME;

/// <summary>
/// 批量過帳審核 Page Model
/// 對應 Delphi PassBatchExam.pas
/// </summary>
public class FMEdPassBatchExamModel : PageModel
{
    public string UserId { get; set; } = "admin"; // TODO: 从 session 获取

    public void OnGet()
    {
        // 初始化页面
    }
}
