using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PcbErpApi.Pages.FME;

/// <summary>
/// 群組製程備註資料 PageModel
/// 對應 Delphi EMOdTmpProcNotes / TmpProcNotesDLL
/// 資料由前端透過 API 取得 (/api/EMOdTmpProcNotes)
/// </summary>
public class EMOdTmpProcNotesModel : PageModel
{
    private readonly ILogger<EMOdTmpProcNotesModel> _logger;

    public EMOdTmpProcNotesModel(ILogger<EMOdTmpProcNotesModel> logger)
    {
        _logger = logger;
    }

    public string UserId { get; set; } = "admin";

    public void OnGet()
    {
        UserId = HttpContext.Session.GetString("UserId") ?? "admin";
    }
}
