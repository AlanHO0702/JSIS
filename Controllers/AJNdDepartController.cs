using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;

namespace PcbErpApi.Controllers
{
    /// <summary>
    /// 部門主檔 API 控制器
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class AJNdDepartController : ControllerBase
    {
        private readonly PcbErpContext _context;
        private readonly PaginationService _pagedService;
        private readonly ILogger<AJNdDepartController> _logger;

        public AJNdDepartController(PcbErpContext context, PaginationService pagedService, ILogger<AJNdDepartController> logger)
        {
            _context = context;
            _pagedService = pagedService;
            _logger = logger;
        }

        /// <summary>
        /// 分頁查詢部門資料
        /// GET: api/AJNdDepart/paged?page=1&pageSize=50
        /// </summary>
        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] string? DepartId = null,
            [FromQuery] string? DepartName = null,
            [FromQuery] string? SuperId = null,
            [FromQuery] byte? IsStop = null,
            [FromQuery] int? Type = null
        )
        {
            var query = _context.AjndDepart.AsQueryable();

            // 套用篩選條件
            if (!string.IsNullOrWhiteSpace(DepartId))
                query = query.Where(x => x.DepartId.Contains(DepartId));

            if (!string.IsNullOrWhiteSpace(DepartName))
                query = query.Where(x => x.DepartName != null && x.DepartName.Contains(DepartName));

            if (!string.IsNullOrWhiteSpace(SuperId))
                query = query.Where(x => x.SuperId == SuperId);

            if (IsStop.HasValue)
                query = query.Where(x => x.IsStop == IsStop.Value);

            if (Type.HasValue)
                query = query.Where(x => x.Type == Type.Value);

            // 排序：依階層編號、部門代號
            var orderedQuery = query
                .OrderBy(x => x.LEVelNo ?? 0)
                .ThenBy(x => x.DepartId);

            var result = await _pagedService.GetPagedAsync(orderedQuery, page, pageSize);
            return Ok(new { totalCount = result.TotalCount, data = result.Data });
        }

        /// <summary>
        /// 取得所有部門資料（用於 TreeView）
        /// GET: api/AJNdDepart
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AjndDepart>>> GetAllDepartments()
        {
            return await _context.AjndDepart
                .OrderBy(x => x.LEVelNo ?? 0)
                .ThenBy(x => x.DepartId)
                .ToListAsync();
        }

        /// <summary>
        /// 取得樹狀結構的部門資料（用於 TreeView 顯示）
        /// GET: api/AJNdDepart/tree
        /// </summary>
        [HttpGet("tree")]
        public async Task<ActionResult<IEnumerable<object>>> GetDepartmentTree()
        {
            var allDepts = await _context.AjndDepart
                .OrderBy(x => x.LEVelNo ?? 0)
                .ThenBy(x => x.DepartId)
                .ToListAsync();

            // 建立樹狀結構
            var rootDepts = allDepts.Where(d => string.IsNullOrEmpty(d.SuperId)).ToList();
            var tree = BuildTreeRecursive(rootDepts, allDepts);

            return Ok(tree);
        }

        /// <summary>
        /// 遞迴建立樹狀結構
        /// </summary>
        private List<object> BuildTreeRecursive(List<AjndDepart> currentLevel, List<AjndDepart> allDepts)
        {
            var result = new List<object>();

            foreach (var dept in currentLevel)
            {
                var children = allDepts.Where(d => d.SuperId == dept.DepartId).ToList();
                result.Add(new
                {
                    dept.DepartId,
                    dept.DepartName,
                    dept.SuperId,
                    LEVelNo = dept.LEVelNo,
                    dept.IsStop,
                    dept.Type,
                    dept.ManagerId,
                    HasChildren = children.Any(),
                    Children = children.Any() ? BuildTreeRecursive(children, allDepts) : null
                });
            }

            return result;
        }

        /// <summary>
        /// 根據部門代號取得單筆資料
        /// GET: api/AJNdDepart/{departId}
        /// </summary>
        [HttpGet("{departId}")]
        public async Task<ActionResult<AjndDepart>> GetAjndDepart(string departId)
        {
            var depart = await _context.AjndDepart.FirstOrDefaultAsync(d => d.DepartId == departId);

            if (depart == null)
            {
                return NotFound();
            }

            return depart;
        }

        /// <summary>
        /// 新增部門資料
        /// POST: api/AJNdDepart
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<AjndDepart>> PostAjndDepart(AjndDepart depart)
        {
            // 檢查部門代號是否已存在
            if (await _context.AjndDepart.AnyAsync(d => d.DepartId == depart.DepartId))
            {
                return BadRequest(new { error = "部門代號已存在" });
            }

            // 設定修改時間
            depart.ModifiedTime = DateTime.Now;
            // depart.UseId 應從目前登入使用者取得

            // 自動計算階層編號
            if (!string.IsNullOrEmpty(depart.SuperId))
            {
                var parent = await _context.AjndDepart.FirstOrDefaultAsync(d => d.DepartId == depart.SuperId);
                if (parent != null)
                {
                    depart.LEVelNo = (byte?)((parent.LEVelNo ?? 0) + 1);
                }
                else
                {
                    depart.LEVelNo = 1;
                }
            }
            else
            {
                depart.LEVelNo = 0; // 最上層部門
            }

            _context.AjndDepart.Add(depart);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAjndDepart), new { departId = depart.DepartId }, depart);
        }

        /// <summary>
        /// 更新部門資料
        /// PUT: api/AJNdDepart/{departId}
        /// </summary>
        [HttpPut("{departId}")]
        public async Task<IActionResult> PutAjndDepart(string departId, AjndDepart depart)
        {
            if (departId != depart.DepartId)
            {
                return BadRequest();
            }

            // 設定修改時間
            depart.ModifiedTime = DateTime.Now;
            // depart.UseId 應從目前登入使用者取得

            _context.Entry(depart).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AjndDepartExists(departId))
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
        /// 刪除部門
        /// DELETE: api/AJNdDepart/{departId}
        /// </summary>
        [HttpDelete("{departId}")]
        public async Task<IActionResult> DeleteAjndDepart(string departId)
        {
            try
            {
                // 檢查是否有下層部門
                var hasChildren = await _context.AjndDepart.AnyAsync(d => d.SuperId == departId);
                if (hasChildren)
                {
                    return BadRequest(new { error = "此部門下有子部門，無法刪除" });
                }

                // 使用原生 SQL 指令刪除（避免 OUTPUT 子句問題）
                var rows = await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM AJNdDepart WHERE DepartId = {0}", departId);

                if (rows == 0)
                    return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                // 取第一行錯誤訊息（trigger 訊息通常在第一行）
                string fullMsg = ex.InnerException?.Message ?? ex.Message;
                string userMsg = fullMsg.Split('\n')[0];
                return BadRequest(new { error = userMsg });
            }
        }

        /// <summary>
        /// 檢查部門是否存在
        /// </summary>
        private bool AjndDepartExists(string departId)
        {
            return _context.AjndDepart.Any(e => e.DepartId == departId);
        }
    }
}
