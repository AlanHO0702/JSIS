using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;

namespace PcbErpApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class FQCdProcInfoController : ControllerBase
    {
        private readonly PcbErpContext _context;
        private readonly ILogger<FQCdProcInfoController> _logger;

        public FQCdProcInfoController(PcbErpContext context, ILogger<FQCdProcInfoController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 取得所有製程資料（支援按 procGroup 篩選）
        /// GET: api/FQCdProcInfo?procGroup={procGroup}
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FqcdProcInfo>>> GetAll([FromQuery] string? bProcCode = null)
        {
            try
            {
                var query = _context.FqcdProcInfos.AsQueryable();

                // 如果有指定 bProcCode，則篩選該製程
                if (!string.IsNullOrWhiteSpace(bProcCode))
                {
                    query = query.Where(x => x.BProcCode == bProcCode);
                }

                var data = await query
                    .OrderBy(x => x.SerialNum ?? 0)
                    .ThenBy(x => x.BProcCode)
                    .ToListAsync();

                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得製程資料失敗");
                return StatusCode(500, new { error = "取得資料失敗", message = ex.Message });
            }
        }

        /// <summary>
        /// 根據 ProcCode 取得單筆製程資料
        /// GET: api/FQCdProcInfo/{procCode}
        /// </summary>
        [HttpGet("{bProcCode}")]
        public async Task<ActionResult<FqcdProcInfo>> GetByProcCode(string bProcCode)
        {
            try
            {
                var data = await _context.FqcdProcInfos
                    .FirstOrDefaultAsync(x => x.BProcCode == bProcCode);

                if (data == null)
                {
                    return NotFound(new { error = $"找不到製程代號: {bProcCode}" });
                }

                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"取得製程資料失敗: {bProcCode}");
                return StatusCode(500, new { error = "取得資料失敗", message = ex.Message });
            }
        }

        /// <summary>
        /// 新增製程
        /// POST: api/FQCdProcInfo
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<FqcdProcInfo>> Create([FromBody] FqcdProcInfo item)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(item.BProcCode))
                {
                    return BadRequest(new { error = "製程代號不可為空" });
                }

                // 檢查是否已存在
                var exists = await _context.FqcdProcInfos
                    .AnyAsync(x => x.BProcCode == item.BProcCode);

                if (exists)
                {
                    return Conflict(new { error = $"製程代號 {item.BProcCode} 已存在" });
                }

                _context.FqcdProcInfos.Add(item);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetByProcCode), new { bProcCode = item.BProcCode }, item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "新增製程失敗");
                return StatusCode(500, new { error = "新增失敗", message = ex.Message });
            }
        }

        /// <summary>
        /// 更新製程
        /// PUT: api/FQCdProcInfo/{procCode}
        /// </summary>
        [HttpPut("{bProcCode}")]
        public async Task<IActionResult> Update(string bProcCode, [FromBody] FqcdProcInfo item)
        {
            try
            {
                if (bProcCode != item.BProcCode)
                {
                    return BadRequest(new { error = "製程代號不符" });
                }

                _context.Entry(item).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.FqcdProcInfos.AnyAsync(e => e.BProcCode == bProcCode))
                    {
                        return NotFound(new { error = $"找不到製程代號: {bProcCode}" });
                    }
                    throw;
                }

                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"更新製程失敗: {bProcCode}");
                return StatusCode(500, new { error = "更新失敗", message = ex.Message });
            }
        }

        /// <summary>
        /// 刪除製程
        /// DELETE: api/FQCdProcInfo/{procCode}
        /// </summary>
        [HttpDelete("{bProcCode}")]
        public async Task<IActionResult> Delete(string bProcCode)
        {
            try
            {
                var item = await _context.FqcdProcInfos
                    .FirstOrDefaultAsync(x => x.BProcCode == bProcCode);

                if (item == null)
                {
                    return NotFound(new { error = $"找不到製程代號: {bProcCode}" });
                }

                _context.FqcdProcInfos.Remove(item);
                await _context.SaveChangesAsync();

                return Ok(new { message = "刪除成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"刪除製程失敗: {bProcCode}");
                return StatusCode(500, new { error = "刪除失敗", message = ex.Message });
            }
        }
    }
}
