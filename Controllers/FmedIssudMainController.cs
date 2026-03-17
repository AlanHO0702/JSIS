using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;

namespace PcbErpApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class FMEdIssueMainController : ControllerBase
    {
        private readonly PcbErpContext _context;
        private readonly PaginationService _pagedService;
        private readonly ILogger<FMEdIssueMainController> _logger;

        public FMEdIssueMainController(PcbErpContext context, PaginationService pagedService, ILogger<FMEdIssueMainController> logger)
        {
            _context = context;
            _pagedService = pagedService;
            _logger = logger;
        }

        // 分頁查詢 GET: api/FMEdIssueMain/paged?page=1&pageSize=50
        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] string? PaperNum = null,
            [FromQuery] string? PartNum = null
        )
        {
            var query = _context.FmedIssueMain.AsQueryable();

            // 建立快取 key（根據過濾條件）
            var cacheKey = "FMEdIssueMain";
            if (!string.IsNullOrWhiteSpace(PaperNum))
            {
                query = query.Where(x => x.PaperNum.Contains(PaperNum));
                cacheKey += $"_P_{PaperNum}";
            }

            if (!string.IsNullOrWhiteSpace(PartNum))
            {
                query = query.Where(x => x.PartNum.Contains(PartNum));
                cacheKey += $"_N_{PartNum}";
            }

            var orderedQuery = query.OrderByDescending(o => o.PaperDate);
            var result = await _pagedService.GetPagedAsync(orderedQuery, page, pageSize, cacheKey);
            return Ok(new { totalCount = result.TotalCount, data = result.Data });
        }


        /// <summary>
        /// 取得所有製令單資料。
        /// </summary>
        /// <returns>製令單清單</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FmedIssueMain>>> GetFMEdIssueMains()
        {
            return await _context.FmedIssueMain.ToListAsync();
        }


        /// <summary>
        /// 根據 PaperNum 取得單筆製令單資料。
        /// </summary>
        /// <param name="paperNum">製令單號</param>
        /// <returns>單一製令單資料</returns>
        [HttpGet("{paperNum}")]
        public async Task<ActionResult<FmedIssueMain>> GetFMEdIssueMain(string paperNum)
        {
            var issue = await _context.FmedIssueMain.FirstOrDefaultAsync(s => s.PaperNum == paperNum);

            if (issue == null)
                return NotFound();

            return issue;
        }


        /// <summary>
        /// 新增一筆製令單資料。
        /// </summary>
        /// <returns>建立成功的製令單</returns>
        [HttpPost]
        public async Task<ActionResult<FmedIssueMain>> PostFMEdIssueMain()
        {
            var prefix = "P" + DateTime.Now.ToString("yyMM");
            var lastToday = await _context.FmedIssueMain
                .Where(x => x.PaperNum.StartsWith(prefix))
                .OrderByDescending(x => x.PaperNum)
                .FirstOrDefaultAsync();

            string nextNum = GenerateNextPaperNum(lastToday?.PaperNum);

            var issue = new FmedIssueMain
            {
                PaperNum = nextNum,
                PaperDate = DateTime.Now,
                CompanyId = "",
                PartNum = "",
                Revision = "",
                UseId = "",
                MotherIssueNum = nextNum,  // 母製令單號設為自己的單號
                // 可根據實際欄位預設值再補
            };

            _context.FmedIssueMain.Add(issue);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetFMEdIssueMain), new { paperNum = issue.PaperNum }, issue);
        }


        // 實作一個產生新單號的方法（例：P2511xxxx）
        private string GenerateNextPaperNum(string? lastPaperNum)
        {
            var prefix = "P" + DateTime.Now.ToString("yyMM");
            int nextSeq = 1;

            if (!string.IsNullOrEmpty(lastPaperNum) && lastPaperNum.StartsWith(prefix))
            {
                var lastSeq = int.Parse(lastPaperNum.Substring(5, 4));
                nextSeq = lastSeq + 1;
            }

            return prefix + nextSeq.ToString("D4");
        }


        /// <summary>
        /// 根據 PaperNum 更新製令單資料。
        /// </summary>
        /// <param name="paperNum">製令單號</param>
        /// <param name="issue">更新後的資料</param>
        /// <returns>NoContent 或錯誤訊息</returns>
        [HttpPut("{paperNum}")]
        public async Task<IActionResult> PutFMEdIssueMain(string paperNum, FmedIssueMain issue)
        {
            if (paperNum != issue.PaperNum)
                return BadRequest();

            _context.Entry(issue).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FMEdIssueMainExists(paperNum))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }


        /// <summary>
        /// 根據 PaperNum 刪除製令單。
        /// </summary>
        /// <param name="paperNum">製令單號</param>
        /// <returns>NoContent 或錯誤訊息</returns>
        [HttpDelete("{paperNum}")]
        public async Task<IActionResult> DeleteFMEdIssueMain(string paperNum)
        {
            try
            {
                var rows = await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM FMEdIssueMain WHERE PaperNum = {0}", paperNum);

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


        /// <summary>
        /// 判斷製令單是否存在。
        /// </summary>
        /// <param name="paperNum">製令單號</param>
        /// <returns>布林值</returns>
        private bool FMEdIssueMainExists(string paperNum)
        {
            return _context.FmedIssueMain.Any(e => e.PaperNum == paperNum);
        }
    }
}
