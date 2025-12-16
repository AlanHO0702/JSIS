using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;

namespace PcbErpApi.Controllers
{
    /// <summary>
    /// 會計科目總類 API 控制器
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class AJNdAccClassController : ControllerBase
    {
        private readonly PcbErpContext _context;
        private readonly ILogger<AJNdAccClassController> _logger;

        public AJNdAccClassController(PcbErpContext context, ILogger<AJNdAccClassController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 取得所有總類資料
        /// GET: api/AJNdAccClass
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AjndAccClass>>> GetAllAccClass()
        {
            return await _context.AjndAccClass
                .OrderBy(x => x.ClassId)
                .ToListAsync();
        }

        /// <summary>
        /// 根據總類代號取得單筆資料
        /// GET: api/AJNdAccClass/{classId}
        /// </summary>
        [HttpGet("{classId}")]
        public async Task<ActionResult<AjndAccClass>> GetAccClass(string classId)
        {
            var accClass = await _context.AjndAccClass.FindAsync(classId);

            if (accClass == null)
            {
                return NotFound();
            }

            return accClass;
        }

        /// <summary>
        /// 新增總類資料
        /// POST: api/AJNdAccClass
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<AjndAccClass>> PostAccClass(AjndAccClass accClass)
        {
            // 檢查代號是否已存在
            if (await _context.AjndAccClass.AnyAsync(x => x.ClassId == accClass.ClassId))
            {
                return BadRequest(new { error = "總類代號已存在" });
            }

            _context.AjndAccClass.Add(accClass);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAccClass), new { classId = accClass.ClassId }, accClass);
        }

        /// <summary>
        /// 更新總類資料
        /// PUT: api/AJNdAccClass/{classId}
        /// </summary>
        [HttpPut("{classId}")]
        public async Task<IActionResult> PutAccClass(string classId, AjndAccClass accClass)
        {
            if (classId != accClass.ClassId)
            {
                return BadRequest();
            }

            _context.Entry(accClass).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AccClassExists(classId))
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
        /// 刪除總類
        /// DELETE: api/AJNdAccClass/{classId}
        /// </summary>
        [HttpDelete("{classId}")]
        public async Task<IActionResult> DeleteAccClass(string classId)
        {
            try
            {
                // 檢查是否有關聯的分類
                var hasClassDtl = await _context.AjndAccClassDtl.AnyAsync(x => x.ClassId == classId);
                if (hasClassDtl)
                {
                    return BadRequest(new { error = "此總類下有分類資料，無法刪除" });
                }

                // 使用原生 SQL 指令刪除（避免 OUTPUT 子句問題）
                var rows = await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM AJNdAccClass WHERE ClassId = {0}", classId);

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

        private bool AccClassExists(string classId)
        {
            return _context.AjndAccClass.Any(e => e.ClassId == classId);
        }
    }
}