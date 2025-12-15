using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;

namespace PcbErpApi.Controllers
{
    /// <summary>
    /// 會計明細科目 API 控制器
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class AJNdSubAccIdController : ControllerBase
    {
        private readonly PcbErpContext _context;
        private readonly ILogger<AJNdSubAccIdController> _logger;

        public AJNdSubAccIdController(PcbErpContext context, ILogger<AJNdSubAccIdController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 取得明細科目資料（可依會計科目、上層明細科目篩選）
        /// GET: api/AJNdSubAccId?AccId=1101&Parent=null
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AjndSubAccId>>> GetSubAccId(
            [FromQuery] string? AccId = null,
            [FromQuery] string? Parent = null,
            [FromQuery] bool? OnlyFirstLevel = null)
        {
            var query = _context.AjndSubAccId.AsQueryable();

            if (!string.IsNullOrWhiteSpace(AccId))
            {
                query = query.Where(x => x.AccId == AccId);
            }

            // 如果 OnlyFirstLevel = true，只顯示第一層（Parent 為 null 或空字串）
            if (OnlyFirstLevel == true)
            {
                query = query.Where(x => string.IsNullOrEmpty(x.Parent));
            }
            else if (Parent != null)
            {
                // 如果明確指定 Parent，則篩選
                if (string.IsNullOrWhiteSpace(Parent))
                {
                    query = query.Where(x => string.IsNullOrEmpty(x.Parent));
                }
                else
                {
                    query = query.Where(x => x.Parent == Parent);
                }
            }

            return await query
                .OrderBy(x => x.AccId)
                .ThenBy(x => x.SubAccId)
                .ToListAsync();
        }

        /// <summary>
        /// 根據複合主鍵取得單筆資料
        /// GET: api/AJNdSubAccId/{accId}/{subAccId}
        /// </summary>
        [HttpGet("{accId}/{subAccId}")]
        public async Task<ActionResult<AjndSubAccId>> GetSubAccId(string accId, string subAccId)
        {
            var subAcc = await _context.AjndSubAccId
                .FirstOrDefaultAsync(x => x.AccId == accId && x.SubAccId == subAccId);

            if (subAcc == null)
            {
                return NotFound();
            }

            return subAcc;
        }

        /// <summary>
        /// 新增明細科目資料
        /// POST: api/AJNdSubAccId
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<AjndSubAccId>> PostSubAccId(AjndSubAccId subAccId)
        {
            // 檢查複合主鍵是否已存在
            if (await _context.AjndSubAccId.AnyAsync(x =>
                x.AccId == subAccId.AccId && x.SubAccId == subAccId.SubAccId))
            {
                return BadRequest(new { error = "明細科目代號已存在" });
            }

            _context.AjndSubAccId.Add(subAccId);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSubAccId),
                new { accId = subAccId.AccId, subAccId = subAccId.SubAccId }, subAccId);
        }

        /// <summary>
        /// 更新明細科目資料
        /// PUT: api/AJNdSubAccId/{accId}/{subAccId}
        /// </summary>
        [HttpPut("{accId}/{subAccId}")]
        public async Task<IActionResult> PutSubAccId(string accId, string subAccId, AjndSubAccId subAcc)
        {
            if (accId != subAcc.AccId || subAccId != subAcc.SubAccId)
            {
                return BadRequest();
            }

            _context.Entry(subAcc).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SubAccIdExists(accId, subAccId))
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
        /// 刪除明細科目
        /// DELETE: api/AJNdSubAccId/{accId}/{subAccId}
        /// </summary>
        [HttpDelete("{accId}/{subAccId}")]
        public async Task<IActionResult> DeleteSubAccId(string accId, string subAccId)
        {
            try
            {
                // 檢查是否有子層明細科目
                var hasChildren = await _context.AjndSubAccId.AnyAsync(x =>
                    x.AccId == accId && x.Parent == subAccId);
                if (hasChildren)
                {
                    return BadRequest(new { error = "此明細科目下有子明細科目，無法刪除" });
                }

                // 使用原生 SQL 指令刪除
                var rows = await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM AJNdSubAccId WHERE AccId = {0} AND SubAccId = {1}", accId, subAccId);

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

        private bool SubAccIdExists(string accId, string subAccId)
        {
            return _context.AjndSubAccId.Any(e => e.AccId == accId && e.SubAccId == subAccId);
        }
    }
}