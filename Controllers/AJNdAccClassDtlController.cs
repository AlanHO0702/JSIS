using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;

namespace PcbErpApi.Controllers
{
    /// <summary>
    /// 會計科目分類 API 控制器
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class AJNdAccClassDtlController : ControllerBase
    {
        private readonly PcbErpContext _context;
        private readonly ILogger<AJNdAccClassDtlController> _logger;

        public AJNdAccClassDtlController(PcbErpContext context, ILogger<AJNdAccClassDtlController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 取得分類資料（可依總類代號篩選）
        /// GET: api/AJNdAccClassDtl?ClassId=1
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AjndAccClassDtl>>> GetAccClassDtl(
            [FromQuery] string? ClassId = null)
        {
            var query = _context.AjndAccClassDtl.AsQueryable();

            if (!string.IsNullOrWhiteSpace(ClassId))
            {
                query = query.Where(x => x.ClassId == ClassId);
            }

            return await query
                .OrderBy(x => x.ClassDtlId)
                .ToListAsync();
        }

        /// <summary>
        /// 根據分類代號取得單筆資料
        /// GET: api/AJNdAccClassDtl/{classDtlId}
        /// </summary>
        [HttpGet("{classDtlId}")]
        public async Task<ActionResult<AjndAccClassDtl>> GetAccClassDtlById(string classDtlId)
        {
            var accClassDtl = await _context.AjndAccClassDtl.FindAsync(classDtlId);

            if (accClassDtl == null)
            {
                return NotFound();
            }

            return accClassDtl;
        }

        /// <summary>
        /// 新增分類資料
        /// POST: api/AJNdAccClassDtl
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<AjndAccClassDtl>> PostAccClassDtl(AjndAccClassDtl accClassDtl)
        {
            // 檢查代號是否已存在
            if (await _context.AjndAccClassDtl.AnyAsync(x => x.ClassDtlId == accClassDtl.ClassDtlId))
            {
                return BadRequest(new { error = "分類代號已存在" });
            }

            _context.AjndAccClassDtl.Add(accClassDtl);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAccClassDtl), new { classDtlId = accClassDtl.ClassDtlId }, accClassDtl);
        }

        /// <summary>
        /// 更新分類資料
        /// PUT: api/AJNdAccClassDtl/{classDtlId}
        /// </summary>
        [HttpPut("{classDtlId}")]
        public async Task<IActionResult> PutAccClassDtl(string classDtlId, AjndAccClassDtl accClassDtl)
        {
            if (classDtlId != accClassDtl.ClassDtlId)
            {
                return BadRequest();
            }

            _context.Entry(accClassDtl).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AccClassDtlExists(classDtlId))
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
        /// 刪除分類
        /// DELETE: api/AJNdAccClassDtl/{classDtlId}
        /// </summary>
        [HttpDelete("{classDtlId}")]
        public async Task<IActionResult> DeleteAccClassDtl(string classDtlId)
        {
            try
            {
                // 檢查是否有關聯的會計科目
                var hasAccId = await _context.AjndAccId.AnyAsync(x => x.ClassDtlId == classDtlId);
                if (hasAccId)
                {
                    return BadRequest(new { error = "此分類下有會計科目資料，無法刪除" });
                }

                // 使用原生 SQL 指令刪除
                var rows = await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM AJNdAccClassDtl WHERE ClassDtlId = {0}", classDtlId);

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

        private bool AccClassDtlExists(string classDtlId)
        {
            return _context.AjndAccClassDtl.Any(e => e.ClassDtlId == classDtlId);
        }
    }
}