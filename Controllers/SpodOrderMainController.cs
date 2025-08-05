using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using PcbErpApi.Helpers;
using static PcbErpApi.Helpers.DynamicQueryHelper;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization; // 記得加這行 using

namespace PcbErpApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class SPOdOrderMainController : ControllerBase
    {

        private readonly PcbErpContext _context;
        private readonly PaginationService _pagedService;
        private readonly ILogger<SPOdOrderMainController> _logger;

        public SPOdOrderMainController(PcbErpContext context, PaginationService pagedService, ILogger<SPOdOrderMainController> logger)
        {
            _context = context;
            _pagedService = pagedService;
            _logger = logger;
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
        // 前端傳入的物件，裡面有 filters 清單
        public class QueryFilterRequest
        {
            public List<QueryParamDto> filters { get; set; }
        }

        public class QueryParamDto
        {
            [JsonPropertyName("Field")]
            public string Field { get; set; }
            [JsonPropertyName("Op")]
            public string Op { get; set; }
            [JsonPropertyName("Value")]
            public string Value { get; set; }
        }

      [HttpPost("pagedQuery")]
        public async Task<IActionResult> GetPagedAuto([FromBody] QueryFilterRequest request)
        {
            var filters = request?.filters ?? new List<QueryParamDto>();
            // 如果 filters 是空，新增一個永遠成立的條件
            if (!filters.Any())
            {
                filters.Add(new QueryParamDto { Field = "PaperNum", Op = "Contains", Value = "" });
            }

            // 過濾掉分頁參數，避免變成 Where 條件
            var filterConditions = filters
            .Where(x => !string.IsNullOrEmpty(x.Value) && x.Field.ToLower() != "page" && x.Field.ToLower() != "pagesize")
            .Select(x => new QueryParam
            {
                Field = x.Field,
                Op = ParseOp(x.Op),
                Value = x.Value
            })
            .ToList();

            IQueryable<SpodOrderMain> query = _context.SpodOrderMain.AsQueryable();
            try
            {
                query = query.ApplyDynamicWhere(filterConditions);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "套用動態條件時發生錯誤");
                return BadRequest("查詢條件格式錯誤");
            }

            var page = filters.FirstOrDefault(x => x.Field.ToLower() == "page")?.Value ?? "1";
            var pageSize = filters.FirstOrDefault(x => x.Field.ToLower() == "pagesize")?.Value ?? "50";

            if (!int.TryParse(page, out int pageNumber))
                pageNumber = 1;
            if (!int.TryParse(pageSize, out int pageSizeNumber))
                pageSizeNumber = 50;

            var result = await _pagedService.GetPagedAsync(query.OrderByDescending(x => x.PaperDate), pageNumber, pageSizeNumber);
            
            // ★ 這裡是重點：查出所有 lookup 設定（不要自己寫死） 
            // TableDictionaryService 應該有一個 GetOCXLookups 方法
            var tableDictService = new TableDictionaryService(_context); // 建議用依賴注入
                var lookupMaps = tableDictService.GetOCXLookups("SPOdOrderMain");

                // ★ 動態組 lookupMapData，直接全包所有 lookup 欄位
                var lookupMapData = LookupDisplayHelper.BuildLookupDisplayMap(
                    result.Data,
                    lookupMaps,
                    (SpodOrderMain item) => item.PaperNum?.Trim() ?? ""
                );


            return Ok(new { totalCount = result.TotalCount, data = result.Data, lookupMapData });
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
