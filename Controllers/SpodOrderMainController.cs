using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using PcbErpApi.Helpers;

namespace PcbErpApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class SPOdOrderMainController : ControllerBase
    {
        private readonly PcbErpContext _context;
        private readonly PaginationService _pagedService;

        public SPOdOrderMainController(PcbErpContext context, PaginationService pagedService)
        {
            _context = context;
            _pagedService = pagedService;
        }

        // 分頁查詢 GET: api/SPOdOrderMains/paged?page=1&pageSize=50
        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 50, 
            [FromQuery] string? PaperNum = null
        )
        {
            var query = _context.SpodOrderMain.AsQueryable();

            if (!string.IsNullOrWhiteSpace(PaperNum))
            query = query.Where(x => x.PaperNum.Contains(PaperNum));

            var orderedQuery = query.OrderByDescending(o => o.PaperDate);
            var result = await _pagedService.GetPagedAsync(orderedQuery, page, pageSize);
            return Ok(new { totalCount = result.TotalCount, data = result.Data });
        }

        [HttpPost("pagedQuery")]
        public async Task<IActionResult> GetPagedAuto([FromBody] Dictionary<string, string> queryParams)
        {
            var query = _context.SpodOrderMain.AsQueryable();

            // 取得所有 CURdPaperSelected 設定為查詢條件的欄位
            var queryFieldDefs = _context.CURdPaperSelected
                .Where(x => x.TableName == "SPOdOrderMain" && x.IVisible == 1)
                .ToList();

            // 動態組 where 條件
            foreach (var field in queryFieldDefs)
            {
                var col = field.ColumnName;
                if (!queryParams.TryGetValue(col, out var value)) continue;
                if (string.IsNullOrWhiteSpace(value)) continue;

                var prop = typeof(SpodOrderMain).GetProperty(col);
                if (prop == null) continue;

                // 常見 DataType:
                // 0: 字串, 1: 日期, 2: 數字, 3: 金額...（根據你的 DataType 定義）
                switch (field.DataType)
                {
                    case 0: // 字串類 like
                        query = query.WhereDynamicStringContains(col, value);
                        break;
                    case 1: // 日期類，支援範圍查詢
                        if (col.ToLower().Contains("start") && DateTime.TryParse(value, out var d1))
                            query = query.WhereDateGreaterThan(col.Replace("Start", ""), d1);
                        else if (col.ToLower().Contains("end") && DateTime.TryParse(value, out var d2))
                            query = query.WhereDateLessThanOrEqual(col.Replace("End", ""), d2);
                        else if (DateTime.TryParse(value, out var dt))
                            query = query.WhereDynamicDateEquals(col, dt);
                        break;
                    case 2: // 數字完全相等
                        if (decimal.TryParse(value, out var dnum))
                            query = query.WhereDynamicDecimalEquals(col, dnum);
                        break;
                    default:
                        // 其他型別依需求擴充
                        break;
                }
            }

            // 分頁/排序略
            var page = queryParams.ContainsKey("page") ? int.Parse(queryParams["page"]) : 1;
            var pageSize = queryParams.ContainsKey("pageSize") ? int.Parse(queryParams["pageSize"]) : 50;

            var result = await _pagedService.GetPagedAsync(query.OrderByDescending(x => x.PaperDate), page, pageSize);
            return Ok(new { totalCount = result.TotalCount, data = result.Data });
        }

        /// <summary>
        /// 取得所有銷售訂單資料。
        /// </summary>
        /// <returns>銷售訂單清單</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SpodOrderMain>>> GetSPOdOrderMains()
        {
            return await _context.SpodOrderMain.ToListAsync();
        }

        /// <summary>
        /// 根據 PaperNum 取得單筆訂單資料。
        /// </summary>
        /// <param name="paperNum">訂單號碼</param>
        /// <returns>單一訂單資料</returns>
        [HttpGet("{paperNum}")]
        public async Task<ActionResult<SpodOrderMain>> GetSPOdOrderMain(string paperNum)
        {
            var order = await _context.SpodOrderMain.FirstOrDefaultAsync(s => s.PaperNum == paperNum);

            if (order == null)
            {
                return NotFound();
            }

            return order;
        }

        /// <summary>
        /// 新增一筆訂單資料。
        /// </summary>
        /// <param name="order">訂單資料</param>
        /// <returns>建立成功的訂單資料</returns>
        [HttpPost]
        public async Task<ActionResult<SpodOrderMain>> PostSPOdOrderMain()
        {
            var prefix = "SA" + DateTime.Now.ToString("yyMM");
            var lastToday = await _context.SpodOrderMain
                .Where(x => x.PaperNum.StartsWith(prefix))
                .OrderByDescending(x => x.PaperNum)
                .FirstOrDefaultAsync();

            string nextNum = GenerateNextPaperNum(lastToday?.PaperNum);

            var order = new SpodOrderMain
            {
                PaperNum = nextNum,
                PaperDate = DateTime.Now,
                CustomerId = "",
                SourCustomerId = "",
                FdrCode = "",
                // 其他預設
            };

            _context.SpodOrderMain.Add(order);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSPOdOrderMain), new { paperNum = order.PaperNum }, order);
        }


        // 實作一個產生新單號的方法
        private string GenerateNextPaperNum(string? lastPaperNum)
        {
            var prefix = "SA" + DateTime.Now.ToString("yyMM"); // SA2507

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
        public async Task<IActionResult> PutSPOdOrderMain(string paperNum, SpodOrderMain order)
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
                if (!SPOdOrderMainExists(paperNum))
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
    public async Task<IActionResult> DeleteSPOdOrderMain(string paperNum)
    {
        try
        {
            // 用原生 SQL 指令（這樣就不會有 OUTPUT 子句問題）
            var rows = await _context.Database.ExecuteSqlRawAsync(
                "DELETE FROM SpodOrderMain WHERE PaperNum = {0}", paperNum);

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
        private bool SPOdOrderMainExists(string paperNum)
        {
            return _context.SpodOrderMain.Any(e => e.PaperNum == paperNum);
        }
    }
}
