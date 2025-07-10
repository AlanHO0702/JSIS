using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PcbErpApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SPOdOrderMainsController : ControllerBase
    {
        private readonly PcbErpContext _context;
        private readonly PagedQueryService _pagedService;

        public SPOdOrderMainsController(PcbErpContext context, PagedQueryService pagedService)
        {
            _context = context;
            _pagedService = pagedService;
        }

        // 分頁查詢 GET: api/SPOdOrderMains/paged?page=1&pageSize=50
          [HttpGet("paged")]
        public async Task<IActionResult> GetPaged(int page = 1, int pageSize = 50)
        {
            var query = _context.SpodOrderMain.OrderByDescending(o => o.PaperDate);
            var result = await _pagedService.GetPagedAsync(query, page, pageSize);
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
            var order = await _context.SpodOrderMain
                .FirstOrDefaultAsync(s => s.PaperNum == paperNum);

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
        public async Task<ActionResult<SpodOrderMain>> PostSPOdOrderMain(SpodOrderMain order)
        {
            _context.SpodOrderMain.Add(order);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSPOdOrderMain), new { paperNum = order.PaperNum }, order);
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
            var order = await _context.SpodOrderMain
                .FirstOrDefaultAsync(s => s.PaperNum == paperNum);
            if (order == null)
            {
                return NotFound();
            }

            _context.SpodOrderMain.Remove(order);
            await _context.SaveChangesAsync();

            return NoContent();
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
