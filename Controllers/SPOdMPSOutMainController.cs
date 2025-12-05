//這四段讓下面這支 Controller 可以用「簡寫」的方式來使用
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;

//這個檔案裡的類別都屬於PcbErpApi.Controllers 這個命名空間
//如果別的檔案要用這個 Controller，就可以 using PcbErpApi.Controllers;
namespace PcbErpApi.Controllers
{
    [Route("api/[controller]")]//決定網址開頭是API+class名字再去掉controller字尾
    [ApiController]//這是 ASP.NET Core 告訴框架：「這是一支 Web API Controller。」
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]//這支 API的回應不要被瀏覽器快取起來
    //這個 class 就是 一支 Web API 控制器。
    //:ControllerBase = 繼承 MVC 的 base 類別，可以用 Ok()、BadRequest() 等 helper。
    public class SPOdMPSOutMainController : ControllerBase
    {

        private readonly PcbErpContext _context;
        private readonly PaginationService _pagedService;//你們自己寫的分頁工具，幫你算總筆數、頁數、Skip / Take。
        private readonly ILogger<SPOdMPSOutMainController> _logger;//記 log 用的，例如記錯誤、誰查了什麼。

        public SPOdMPSOutMainController(PcbErpContext context, PaginationService pagedService, ILogger<SPOdMPSOutMainController> logger)
        {
            _context = context;
            _pagedService = pagedService;
            _logger = logger;
        }

        // 分頁查詢 GET: api/SPOdOrderMains/paged?page=1&pageSize=50
        [HttpGet("paged")]
        //這三個參數都從 Query String= 放在網址 ? 後面，用來傳「參數」的那一段字串。
        public async Task<IActionResult> GetPaged(
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 50, 
            [FromQuery] string? PaperNum = null
        )
        {   //第一步：先選「哪一張表」當基礎
            //_context.SPOdMPSOutMain= EF Core 的 DbSet，指向資料庫的 SPOdMPSOutMain 表
            //AsQueryable()= 告訴 EF：「我現在要開始組查詢條件，但還不要真的打 DB。」
            var query = _context.SPOdMPSOutMain.AsQueryable();

            //第二步：如果PaperNum 有值，就在原本的query上面再加一條篩選條件，PaperNum LIKE '%@PaperNum%'
            if (!string.IsNullOrWhiteSpace(PaperNum))
            query = query.Where(x => x.PaperNum.Contains(PaperNum));

            //第三步：把組好的 query 丟給分頁服務，並指定排序方式 (這裡是用 PaperDate 降冪排序)
            var orderedQuery = query.OrderByDescending(o => o.PaperDate);

            //第四步：真的出手打DB拿資料，此時 EF Core會把你前面組好的整串條件，轉成一支完整 SQL：
            var result = await _pagedService.GetPagedAsync(orderedQuery, page, pageSize);

            //第五步：把分頁結果包成一個物件回傳給前端
            return Ok(new { totalCount = result.TotalCount, data = result.Data });
        }
   

        /// 這個 Action 就是「把所有銷貨單主檔整包抓出來」的 API。
        [HttpGet]
        //public = 這個方法可以被外面呼叫
        //async = 這個方法是「非同步版」，裡面可以 await 等資料庫、IO 等動作，不會卡住整個網站。
            public async Task<ActionResult<IEnumerable<SPOdMPSOutMain>>> GetSPOdMPSOutMains()//回傳型別
        {
            return await _context.SPOdMPSOutMain.ToListAsync();
        }



        /// 這支 API 是「用單號去查單頭，如果有就回資料，沒有就回 404」。
        /// <param name="paperNum">訂單號碼</param>
        /// <returns>單一訂單資料</returns>
        [HttpGet("{paperNum}")]
        public async Task<ActionResult<SPOdMPSOutMain>> GetSPOdMPSOutMain(string paperNum)
        {
            var order = await _context.SPOdMPSOutMain.FirstOrDefaultAsync(s => s.PaperNum == paperNum);

            if (order == null)
            {
                return NotFound();
            }

            return order;
        }


        /// 新增一筆訂單資料。
        /// <param name="order">訂單資料</param>
        /// <returns>建立成功的訂單資料</returns>
        [HttpPost]
        public async Task<ActionResult<SPOdMPSOutMain>> PostSPOdMPSOutMain()
        {   //決定新單號的「前綴」
            var prefix = "DD" + DateTime.Now.ToString("yyMM");
            //找出目前這個年月的「最新一張單」
            var lastToday = await _context.SPOdMPSOutMain
                .Where(x => x.PaperNum.StartsWith(prefix))
                .OrderByDescending(x => x.PaperNum)
                .FirstOrDefaultAsync();
            //產生下一張單號
            string nextNum = GenerateNextPaperNum(lastToday?.PaperNum);
            //建立新訂單物件，指定新單號、給預設值
            var order = new SPOdMPSOutMain
            {
                PaperNum = nextNum,
                PaperDate = DateTime.Now,
                CustomerId = "",
                //SourCustomerId = "",
                FdrCode = "",
                // 其他預設
            };
            //存到資料庫
            _context.SPOdMPSOutMain.Add(order);//告訴 EF Core 我要新增這筆資料
            await _context.SaveChangesAsync();//實際執行 SQL 的 INSERT，把資料寫進資料庫。

            return CreatedAtAction(nameof(GetSPOdMPSOutMain), new { paperNum = order.PaperNum }, order);
        }


        // 實作一個產生新單號的方法
        private string GenerateNextPaperNum(string? lastPaperNum)
        {
            var prefix = "DD" + DateTime.Now.ToString("yyMM"); // DD2511

            int nextSeq = 1;

            // 如果有今天單號，流水號遞增
            if (!string.IsNullOrEmpty(lastPaperNum) && lastPaperNum.StartsWith(prefix))
            {
                // 取流水號部分 (第6~9碼，4碼)
                var lastSeq = int.Parse(lastPaperNum.Substring(6, 4));
                nextSeq = lastSeq + 1;
            }

            // 補足4碼流水號，總長10碼
            return prefix + nextSeq.ToString("D4");
        }



        /// <summary>
        /// 根據 PaperNum 更新訂單資料。
        /// </summary>
        /// <param name="paperNum">訂單號碼</param>
        /// <param name="order">更新後的訂單資料</param>
        /// <returns>NoContent 或錯誤訊息</returns>
        [HttpPut("{paperNum}")]
        public async Task<IActionResult> PutSPOdMPSOutMain(string paperNum, SPOdMPSOutMain order)
        {
            if (paperNum != order.PaperNum)
            {
                return BadRequest();
            }
            //告訴 EF Core「這個物件是已存在的，要修改」
            _context.Entry(order).State = EntityState.Modified;
            //try 裡放「可能出錯」的程式碼
            // 出錯就進 catch，決定怎麼處理 / 回傳
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SPOdMPSOutMainExists(paperNum))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        /// <summary>
        /// 根據 PaperNum 刪除訂單。
        /// </summary>
        /// <param name="paperNum">訂單號碼</param>
        /// <returns>NoContent 或錯誤訊息</returns>
   [HttpDelete("{paperNum}")]
    public async Task<IActionResult> DeleteSPOdMPSOutMain(string paperNum)
    {
        try
        {
            // 用原生 SQL 指令（這樣就不會有 OUTPUT 子句問題）
            var rows = await _context.Database.ExecuteSqlRawAsync(
                "DELETE FROM SPOdMPSOutMain WHERE PaperNum = {0}", paperNum);

            if (rows == 0)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            // 只取第一行（trigger常見訊息會在第一行）
            string fullMsg = ex.InnerException?.Message ?? ex.Message;
            string userMsg = fullMsg.Split('\n')[0]; // 取第一行
            return BadRequest(new { error = userMsg });
        }

    }


        /// <summary>
        /// 判斷訂單是否存在。
        /// </summary>
        /// <param name="paperNum">訂單號碼</param>
        /// <returns>布林值</returns>
        private bool SPOdMPSOutMainExists(string paperNum)
        {
            return _context.SPOdMPSOutMain.Any(e => e.PaperNum == paperNum);
        }
    }
}
