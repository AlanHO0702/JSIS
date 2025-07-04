using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace PcbErpApi.Controllers
{
    public class PivotController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.JsonData = "[]"; // 空陣列先載入
            return View();
        }

        [HttpPost]
        public IActionResult Index(string jsonDataRaw, string tableName)
        {
            JArray jsonData = new JArray();

            if (!string.IsNullOrWhiteSpace(jsonDataRaw))
            {
                try
                {
                    jsonData = JArray.Parse(jsonDataRaw);
                }
                catch
                {
                    jsonData = new JArray();
                }
            }

            ViewBag.JsonData = jsonData.ToString(Newtonsoft.Json.Formatting.None);
            ViewBag.TableName = tableName; // ✅ 傳給 View 顯示
            return View();
        }

    }
}
