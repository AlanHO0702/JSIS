using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;

namespace PcbErpApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class FMEdBigProcParamController : ControllerBase
    {
        private readonly PcbErpContext _context;
        private readonly ILogger<FMEdBigProcParamController> _logger;

        public FMEdBigProcParamController(PcbErpContext context, ILogger<FMEdBigProcParamController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 取得所有參數明細（支援按 procCode 或 procGroup 篩選）
        /// GET: api/FMEdBigProcParam?procCode={procCode}&procGroup={procGroup}
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FmedBigProcParam>>> GetAll(
            [FromQuery] string? procCode = null,
            [FromQuery] string? procGroup = null)
        {
            try
            {
                var query = _context.FmedBigProcParams.AsQueryable();

                // 篩選條件
                if (!string.IsNullOrWhiteSpace(procCode))
                {
                    query = query.Where(x => x.ProcCode == procCode);
                }
                else if (!string.IsNullOrWhiteSpace(procGroup))
                {
                    query = query.Where(x => x.ProcGroup == procGroup);
                }

                var data = await query
                    .OrderBy(x => x.ProcCode)
                    .ThenBy(x => x.SerialNum ?? 0)
                    .ThenBy(x => x.ParamId)
                    .ToListAsync();

                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得參數明細失敗");
                return StatusCode(500, new { error = "取得資料失敗", message = ex.Message });
            }
        }

        /// <summary>
        /// 根據 ProcCode 和 ParamId 取得單筆參數
        /// GET: api/FMEdBigProcParam/{procCode}/{paramId}
        /// </summary>
        [HttpGet("{procCode}/{paramId}")]
        public async Task<ActionResult<FmedBigProcParam>> GetByKey(string procCode, string paramId)
        {
            try
            {
                var data = await _context.FmedBigProcParams
                    .FirstOrDefaultAsync(x => x.ProcCode == procCode && x.ParamId == paramId);

                if (data == null)
                {
                    return NotFound(new { error = $"找不到參數: {procCode} / {paramId}" });
                }

                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"取得參數失敗: {procCode} / {paramId}");
                return StatusCode(500, new { error = "取得資料失敗", message = ex.Message });
            }
        }

        /// <summary>
        /// 新增參數
        /// POST: api/FMEdBigProcParam
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<FmedBigProcParam>> Create([FromBody] FmedBigProcParam item)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(item.ProcCode) || string.IsNullOrWhiteSpace(item.ParamId))
                {
                    return BadRequest(new { error = "製程代號和參數代號不可為空" });
                }

                // 檢查是否已存在
                var exists = await _context.FmedBigProcParams
                    .AnyAsync(x => x.ProcCode == item.ProcCode && x.ParamId == item.ParamId);

                if (exists)
                {
                    return Conflict(new { error = $"參數 {item.ProcCode} / {item.ParamId} 已存在" });
                }

                item.CreateDate = DateTime.Now;
                item.ModifyDate = DateTime.Now;

                _context.FmedBigProcParams.Add(item);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetByKey),
                    new { procCode = item.ProcCode, paramId = item.ParamId }, item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "新增參數失敗");
                return StatusCode(500, new { error = "新增失敗", message = ex.Message });
            }
        }

        /// <summary>
        /// 更新參數
        /// PUT: api/FMEdBigProcParam/{procCode}/{paramId}
        /// </summary>
        [HttpPut("{procCode}/{paramId}")]
        public async Task<IActionResult> Update(string procCode, string paramId, [FromBody] FmedBigProcParam item)
        {
            try
            {
                if (procCode != item.ProcCode || paramId != item.ParamId)
                {
                    return BadRequest(new { error = "製程代號或參數代號不符" });
                }

                var existing = await _context.FmedBigProcParams
                    .FirstOrDefaultAsync(x => x.ProcCode == procCode && x.ParamId == paramId);

                if (existing == null)
                {
                    return NotFound(new { error = $"找不到參數: {procCode} / {paramId}" });
                }

                // 更新欄位
                existing.ProcGroup = item.ProcGroup;
                existing.ParamName = item.ParamName;
                existing.ParamValue = item.ParamValue;
                existing.StdValue = item.StdValue;
                existing.UpperLimit = item.UpperLimit;
                existing.LowerLimit = item.LowerLimit;
                existing.Unit = item.Unit;
                existing.ParamType = item.ParamType;
                existing.IsRequired = item.IsRequired;
                existing.IsCheck = item.IsCheck;
                existing.SerialNum = item.SerialNum;
                existing.Memo = item.Memo;
                existing.Other1 = item.Other1;
                existing.Other2 = item.Other2;
                existing.Other3 = item.Other3;
                existing.ModifyDate = DateTime.Now;

                await _context.SaveChangesAsync();

                return Ok(existing);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"更新參數失敗: {procCode} / {paramId}");
                return StatusCode(500, new { error = "更新失敗", message = ex.Message });
            }
        }

        /// <summary>
        /// 刪除參數
        /// DELETE: api/FMEdBigProcParam/{procCode}/{paramId}
        /// </summary>
        [HttpDelete("{procCode}/{paramId}")]
        public async Task<IActionResult> Delete(string procCode, string paramId)
        {
            try
            {
                var item = await _context.FmedBigProcParams
                    .FirstOrDefaultAsync(x => x.ProcCode == procCode && x.ParamId == paramId);

                if (item == null)
                {
                    return NotFound(new { error = $"找不到參數: {procCode} / {paramId}" });
                }

                _context.FmedBigProcParams.Remove(item);
                await _context.SaveChangesAsync();

                return Ok(new { message = "刪除成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"刪除參數失敗: {procCode} / {paramId}");
                return StatusCode(500, new { error = "刪除失敗", message = ex.Message });
            }
        }
    }
}
