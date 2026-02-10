using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;

namespace PcbErpApi.Pages.CUR
{
    public class FlowDesignerModel : PageModel
    {
        private readonly PcbErpContext _context;
        private readonly IConfiguration _configuration;

        public FlowDesignerModel(PcbErpContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        /// <summary>
        /// 當前用戶ID
        /// </summary>
        public string CurrentUserId { get; set; } = "";

        /// <summary>
        /// 流程列表
        /// </summary>
        public List<XFLdPRC> FlowList { get; set; } = new List<XFLdPRC>();

        /// <summary>
        /// 當前選中的流程
        /// </summary>
        public XFLdPRC? CurrentFlow { get; set; }

        /// <summary>
        /// 啟用狀態選項
        /// </summary>
        public List<FinishedOption> FinishedOptions { get; set; } = new List<FinishedOption>
        {
            new FinishedOption { Value = 0, Name = "停用" },
            new FinishedOption { Value = 1, Name = "啟用" }
        };

        public async Task OnGetAsync(string? prcId = null)
        {
            // 從 HttpContext 或參數取得 UserId
            CurrentUserId = HttpContext.Items["UserId"]?.ToString()
                            ?? User.Identity?.Name
                            ?? "admin";

            // 載入流程列表
            FlowList = await _context.XFLdPRCs
                .OrderBy(p => p.PRCID)
                .ToListAsync();

            // 如果有指定流程代碼，載入該流程
            if (!string.IsNullOrWhiteSpace(prcId))
            {
                CurrentFlow = await _context.XFLdPRCs.FindAsync(prcId);
            }
        }

        /// <summary>
        /// 新增流程
        /// </summary>
        public async Task<IActionResult> OnPostCreateAsync(string prcId, string prcName, string descrip)
        {
            try
            {
                CurrentUserId = HttpContext.Items["UserId"]?.ToString() ?? "admin";

                // 檢查流程代碼是否已存在
                var existingFlow = await _context.XFLdPRCs.FindAsync(prcId);
                if (existingFlow != null)
                {
                    TempData["Error"] = "流程代碼已存在";
                    return RedirectToPage();
                }

                var newFlow = new XFLdPRC
                {
                    PRCID = prcId,
                    PRCNAME = prcName,
                    DESCRIP = descrip,
                    CREATOR = CurrentUserId,
                    CDATE = DateTime.Now,
                    Finished = 0
                };

                _context.XFLdPRCs.Add(newFlow);

                // 新增預設事件
                var defaultEvents = new[]
                {
                    new XFLdEVT { PRCID = prcId, RELATEID = "", EVTNAME = "流程初始行為", EVTTYPE = 0, ONEXEC = "" },
                    new XFLdEVT { PRCID = prcId, RELATEID = "", EVTNAME = "流程結束行為", EVTTYPE = 0, ONEXEC = "" },
                    new XFLdEVT { PRCID = prcId, RELATEID = "", EVTNAME = "流程取消行為", EVTTYPE = 0,
                        ONEXEC = "EXEC CURdOCXFlowBackToPaper @單據類型,@單據編號,33,@申請人,@申請人,@流程編號,@代理人" }
                };

                _context.XFLdEVTs.AddRange(defaultEvents);

                await _context.SaveChangesAsync();

                TempData["Success"] = "新增流程成功";
                return RedirectToPage(new { prcId = prcId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"新增流程失敗：{ex.Message}";
                return RedirectToPage();
            }
        }

        /// <summary>
        /// 儲存流程
        /// </summary>
        public async Task<IActionResult> OnPostSaveAsync(string prcId, string? flowChart)
        {
            try
            {
                CurrentUserId = HttpContext.Items["UserId"]?.ToString() ?? "admin";

                var flow = await _context.XFLdPRCs.FindAsync(prcId);
                if (flow == null)
                {
                    return NotFound();
                }

                if (flow.Finished == 1)
                {
                    TempData["Error"] = "已啟用的流程無法修改";
                    return RedirectToPage(new { prcId = prcId });
                }

                // XFLdPRC 可能存在 Trigger，改用原生 SQL 更新以避免 EF OUTPUT 子句衝突
                var affectedRows = await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE XFLdPRC SET FLOWCHART = {0}, MODIFICATOR = {1} WHERE PRCID = {2}",
                    flowChart ?? (object)DBNull.Value,
                    CurrentUserId,
                    prcId);
                if (affectedRows == 0)
                {
                    TempData["Error"] = "儲存失敗：找不到流程資料";
                    return RedirectToPage(new { prcId = prcId });
                }

                TempData["Success"] = "儲存成功";
                return RedirectToPage(new { prcId = prcId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"儲存失敗：{ex.Message}";
                return RedirectToPage(new { prcId = prcId });
            }
        }

        /// <summary>
        /// 切換啟用狀態
        /// </summary>
        public async Task<IActionResult> OnPostToggleStatusAsync(string prcId)
        {
            try
            {
                CurrentUserId = HttpContext.Items["UserId"]?.ToString() ?? "admin";

                if (string.IsNullOrWhiteSpace(prcId))
                {
                    TempData["Error"] = "請先開啟FLOW流程檔";
                    return RedirectToPage();
                }

                var flow = await _context.XFLdPRCs.FindAsync(prcId);
                if (flow == null)
                {
                    TempData["Error"] = "沒有FLOW流程檔，無法處理";
                    return RedirectToPage();
                }

                // 呼叫既有的資料庫程序來切換流程啟用狀態
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC CURdFlowPrcFinish {0}, {1}",
                    prcId,
                    CurrentUserId);

                // 重新讀取狀態以回饋最新結果
                flow = await _context.XFLdPRCs.FindAsync(prcId);
                if (flow == null)
                {
                    TempData["Error"] = "流程狀態更新後讀取失敗";
                    return RedirectToPage();
                }

                var statusText = flow.Finished == 1 ? "啟用" : "停用";
                TempData["Success"] = $"已{statusText}流程";
                return RedirectToPage(new { prcId = prcId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"切換狀態失敗：{ex.Message}";
                return RedirectToPage(new { prcId = prcId });
            }
        }

        /// <summary>
        /// 刪除流程
        /// </summary>
        public async Task<IActionResult> OnPostDeleteAsync(string prcId)
        {
            try
            {
                var flow = await _context.XFLdPRCs.FindAsync(prcId);
                if (flow == null)
                {
                    return NotFound();
                }

                if (flow.Finished == 1)
                {
                    TempData["Error"] = "已啟用的流程無法刪除";
                    return RedirectToPage();
                }

                // 刪除相關的活動、轉換、事件
                var activities = await _context.XFLdActs.Where(a => a.PRCID == prcId).ToListAsync();
                var transitions = await _context.XFLdTRAs.Where(t => t.PRCID == prcId).ToListAsync();
                var events = await _context.XFLdEVTs.Where(e => e.PRCID == prcId).ToListAsync();

                _context.XFLdActs.RemoveRange(activities);
                _context.XFLdTRAs.RemoveRange(transitions);
                _context.XFLdEVTs.RemoveRange(events);
                _context.XFLdPRCs.Remove(flow);

                await _context.SaveChangesAsync();

                TempData["Success"] = "刪除成功";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"刪除失敗：{ex.Message}";
                return RedirectToPage();
            }
        }
    }

    public class FinishedOption
    {
        public int Value { get; set; }
        public string Name { get; set; } = "";
    }
}
