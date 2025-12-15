using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;

namespace PcbErpApi.Controllers
{
    /// <summary>
    /// 會計總帳科目 API 控制器
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class AJNdAccIdController : ControllerBase
    {
        private readonly PcbErpContext _context;
        private readonly ILogger<AJNdAccIdController> _logger;

        public AJNdAccIdController(PcbErpContext context, ILogger<AJNdAccIdController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 取得會計科目資料（可依總類、分類篩選）
        /// GET: api/AJNdAccId?ClassId=1&ClassDtlId=11
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AjndAccId>>> GetAccId(
            [FromQuery] string? ClassId = null,
            [FromQuery] string? ClassDtlId = null)
        {
            var query = _context.AjndAccId.AsQueryable();

            if (!string.IsNullOrWhiteSpace(ClassId))
            {
                query = query.Where(x => x.ClassId == ClassId);
            }

            if (!string.IsNullOrWhiteSpace(ClassDtlId))
            {
                query = query.Where(x => x.ClassDtlId == ClassDtlId);
            }

            return await query
                .OrderBy(x => x.AccId)
                .ToListAsync();
        }

        /// <summary>
        /// 根據會計科目代號取得單筆資料
        /// GET: api/AJNdAccId/{accId}
        /// </summary>
        [HttpGet("{accId}")]
        public async Task<ActionResult<AjndAccId>> GetAccId(string accId)
        {
            var acc = await _context.AjndAccId.FindAsync(accId);

            if (acc == null)
            {
                return NotFound();
            }

            return acc;
        }

        /// <summary>
        /// 新增會計科目資料
        /// POST: api/AJNdAccId
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<AjndAccId>> PostAccId(AjndAccId accId)
        {
            // 檢查代號是否已存在
            if (await _context.AjndAccId.AnyAsync(x => x.AccId == accId.AccId))
            {
                return BadRequest(new { error = "會計科目代號已存在" });
            }

            _context.AjndAccId.Add(accId);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAccId), new { accId = accId.AccId }, accId);
        }

        /// <summary>
        /// 更新會計科目資料
        /// PUT: api/AJNdAccId/{accId}
        /// </summary>
        [HttpPut("{accId}")]
        public async Task<IActionResult> PutAccId(string accId, AjndAccId acc)
        {
            if (accId != acc.AccId)
            {
                return BadRequest();
            }

            _context.Entry(acc).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AccIdExists(accId))
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
        /// 刪除會計科目
        /// DELETE: api/AJNdAccId/{accId}
        /// </summary>
        [HttpDelete("{accId}")]
        public async Task<IActionResult> DeleteAccId(string accId)
        {
            try
            {
                // 檢查是否有關聯的明細科目
                var hasSubAcc = await _context.AjndSubAccId.AnyAsync(x => x.AccId == accId);
                if (hasSubAcc)
                {
                    return BadRequest(new { error = "此會計科目下有明細科目資料，無法刪除" });
                }

                // 使用原生 SQL 指令刪除
                var rows = await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM AJNdAccId WHERE AccId = {0}", accId);

                if (rows == 0)
                    return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                string fullMsg = ex.InnerException?.Message ?? ex.Message;
                string userMsg = fullMsg.Split('\n')[0];
                return BadRequest(new { error = userMsg });
            }
        }

        private bool AccIdExists(string accId)
        {
            return _context.AjndAccId.Any(e => e.AccId == accId);
        }
    }
}