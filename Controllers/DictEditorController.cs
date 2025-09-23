using Microsoft.AspNetCore.Mvc;

namespace PcbErpApi.Controllers
{
    [Route("DictEditor")]
    public class DictEditorController : Controller
    {
        public record DictEditorVm
        {
            public string Table { get; init; } = "";
            public string Lang  { get; init; } = "TW";
            public string Field { get; init; } = "";
        }

            [HttpGet("EditList")]
            public IActionResult EditList(string table, string lang = "TW", string field = "")
            {
                // View 自己從 QueryString 讀 table/lang/field，不用 model
                return View(); // 對應 Views/DictEditor/EditList.cshtml
            }
    }
}
