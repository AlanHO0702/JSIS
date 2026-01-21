using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;


namespace PcbErpApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class FOSdReceiveMainController : ControllerBase
    {

        private readonly PcbErpContext _context;
        private readonly PaginationService _pagedService;
        private readonly ILogger<FOSdReceiveMainController> _logger;

        public FOSdReceiveMainController(PcbErpContext context, PaginationService pagedService, ILogger<FOSdReceiveMainController> logger)
        {
            _context = context;
            _pagedService = pagedService;
            _logger = logger;
        }

        // 分頁查詢 GET: api/FOSdReceiveMain/paged?page=1&pageSize=50
        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] string? PaperNum = null
        )
        {
            var query = _context.FosdReceiveMain.AsQueryable();

            if (!string.IsNullOrWhiteSpace(PaperNum))
            query = query.Where(x => x.PaperNum.Contains(PaperNum));

            var orderedQuery = query.OrderByDescending(o => o.PaperDate);
            var result = await _pagedService.GetPagedAsync(orderedQuery, page, pageSize);
            return Ok(new { totalCount = result.TotalCount, data = result.Data });
        }


        /// <summary>
        /// 取得所有委外回廠單資料。
        /// </summary>
        /// <returns>委外回廠單清單</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FosdReceiveMain>>> GetFOSdReceiveMains()
        {
            return await _context.FosdReceiveMain.ToListAsync();
        }

        /// <summary>
        /// 根據 PaperNum 取得單筆委外回廠單資料。
        /// </summary>
        /// <param name="paperNum">單據號碼</param>
        /// <returns>單一委外回廠單資料</returns>
        [HttpGet("{paperNum}")]
        public async Task<ActionResult<FosdReceiveMain>> GetFOSdReceiveMain(string paperNum)
        {
            var order = await _context.FosdReceiveMain.FirstOrDefaultAsync(s => s.PaperNum == paperNum);

            if (order == null)
            {
                return NotFound();
            }

            return order;
        }

        /// <summary>
        /// 新增一筆委外回廠單資料。
        /// </summary>
        /// <param name="order">委外回廠單資料</param>
        /// <returns>建立成功的委外回廠單資料</returns>
        [HttpPost]
        public async Task<ActionResult<FosdReceiveMain>> PostFOSdReceiveMain()
        {
            var prefix = "FR" + DateTime.Now.ToString("yyMM");
            var lastToday = await _context.FosdReceiveMain
                .Where(x => x.PaperNum.StartsWith(prefix))
                .OrderByDescending(x => x.PaperNum)
                .FirstOrDefaultAsync();

            string nextNum = GenerateNextPaperNum(lastToday?.PaperNum);

            var order = new FosdReceiveMain
            {
                PaperNum = nextNum,
                PaperDate = DateTime.Now,
                BuildDate = DateTime.Now,
                CompanyId = "",
                Status = 0,
                Finished = 0,
                PayWayCode = 0,
                MoneyCode = 0,
                RateToNT = 1,
                SubTotal = 0,
                InvoiceType = 0,
                Tax = 0,
                Total = 0,
                PaperType = 0,
                TranTo_LPcs = 0,
                IsSample = 0,
                OSType = 0,
                STDSubTotal = 0,
                STDTax = 0,
                STDTotal = 0,
                FromType = 0
            };

            _context.FosdReceiveMain.Add(order);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetFOSdReceiveMain), new { paperNum = order.PaperNum }, order);
        }


        // 實作一個產生新單號的方法
        private string GenerateNextPaperNum(string? lastPaperNum)
        {
            var prefix = "FR" + DateTime.Now.ToString("yyMM"); // FR2512

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
        /// 根據 PaperNum 更新委外回廠單資料。
        /// </summary>
        /// <param name="paperNum">單據號碼</param>
        /// <param name="order">更新後的委外回廠單資料</param>
        /// <returns>NoContent 或錯誤訊息</returns>
        [HttpPut("{paperNum}")]
        public async Task<IActionResult> PutFOSdReceiveMain(string paperNum, FosdReceiveMain order)
        {
            if (paperNum != order.PaperNum)
            {
                return BadRequest();
            }

            _context.Entry(order).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FOSdReceiveMainExists(paperNum))
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
        /// 根據 PaperNum 刪除委外回廠單。
        /// </summary>
        /// <param name="paperNum">單據號碼</param>
        /// <returns>NoContent 或錯誤訊息</returns>
   [HttpDelete("{paperNum}")]
    public async Task<IActionResult> DeleteFOSdReceiveMain(string paperNum)
    {
        try
        {
            // 用原生 SQL 指令（這樣就不會有 OUTPUT 子句問題）
            var rows = await _context.Database.ExecuteSqlRawAsync(
                "DELETE FROM FOSdReceiveMain WHERE PaperNum = {0}", paperNum);

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
        /// 判斷委外回廠單是否存在。
        /// </summary>
        /// <param name="paperNum">單據號碼</param>
        /// <returns>布林值</returns>
        private bool FOSdReceiveMainExists(string paperNum)
        {
            return _context.FosdReceiveMain.Any(e => e.PaperNum == paperNum);
        }
    }
}
