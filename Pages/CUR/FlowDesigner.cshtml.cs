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

                // 使用原生 SQL 插入流程主檔（避免 Trigger 干擾 EF Core OUTPUT 子句）
                await _context.Database.ExecuteSqlRawAsync(
                    @"INSERT INTO XFLdPRC(PRCID, PRCNAME, CREATOR, CDATE, DESCRIP, Finished)
                      VALUES({0}, {1}, {2}, GETDATE(), {3}, 0)",
                    prcId, prcName, CurrentUserId, descrip ?? "");

                // 使用原生 SQL 插入預設事件（完全模仿 DELPHI 行為）
                await _context.Database.ExecuteSqlRawAsync(
                    @"INSERT INTO XFLdEVT(PRCID, RELATEID, EVTNAME, EVTTYPE, ONEXEC)
                      VALUES({0}, {1}, {2}, 0, {3})",
                    prcId, "", "流程初始行為", "");

                await _context.Database.ExecuteSqlRawAsync(
                    @"INSERT INTO XFLdEVT(PRCID, RELATEID, EVTNAME, EVTTYPE, ONEXEC)
                      VALUES({0}, {1}, {2}, 0, {3})",
                    prcId, "", "流程結束行為", "");

                await _context.Database.ExecuteSqlRawAsync(
                    @"INSERT INTO XFLdEVT(PRCID, RELATEID, EVTNAME, EVTTYPE, ONEXEC)
                      VALUES({0}, {1}, {2}, 0, {3})",
                    prcId, "", "流程取消行為",
                    "EXEC CURdOCXFlowBackToPaper @單據類型,@單據編號,33,@申請人,@申請人,@流程編號,@代理人");

                // 自動建立流程開始和流程結束節點（模仿 DELPHI 的 FileNew 行為）
                // FlowStart: 位置 (200, 250)
                await _context.Database.ExecuteSqlRawAsync(
                    @"INSERT INTO XFLdAct(ACTID, PRCID, ACTNAME, X, Y, ACTTYPE)
                      VALUES({0}, {1}, {2}, {3}, {4}, {5})",
                    "flowstart", prcId, "流程開始", 200, 250, 0);

                // 為 FlowStart 建立 3 個活動事件
                await _context.Database.ExecuteSqlRawAsync(
                    @"INSERT INTO XFLdEVT(PRCID, RELATEID, EVTNAME, EVTTYPE, ONEXEC)
                      VALUES({0}, {1}, {2}, 1, {3})",
                    prcId, "flowstart", "收件前行為", "SET @收件人 = @申請人");

                await _context.Database.ExecuteSqlRawAsync(
                    @"INSERT INTO XFLdEVT(PRCID, RELATEID, EVTNAME, EVTTYPE, ONEXEC)
                      VALUES({0}, {1}, {2}, 1, {3})",
                    prcId, "flowstart", "收件後行為", "");

                await _context.Database.ExecuteSqlRawAsync(
                    @"INSERT INTO XFLdEVT(PRCID, RELATEID, EVTNAME, EVTTYPE, ONEXEC)
                      VALUES({0}, {1}, {2}, 1, {3})",
                    prcId, "flowstart", "寄件行為", "");

                // FlowEnd: 位置 (500, 250)
                await _context.Database.ExecuteSqlRawAsync(
                    @"INSERT INTO XFLdAct(ACTID, PRCID, ACTNAME, X, Y, ACTTYPE)
                      VALUES({0}, {1}, {2}, {3}, {4}, {5})",
                    "flowend", prcId, "流程結束", 500, 250, 0);

                // 為 FlowEnd 建立 3 個活動事件
                await _context.Database.ExecuteSqlRawAsync(
                    @"INSERT INTO XFLdEVT(PRCID, RELATEID, EVTNAME, EVTTYPE, ONEXEC)
                      VALUES({0}, {1}, {2}, 1, {3})",
                    prcId, "flowend", "收件前行為", "SET @收件人 = @申請人");

                await _context.Database.ExecuteSqlRawAsync(
                    @"INSERT INTO XFLdEVT(PRCID, RELATEID, EVTNAME, EVTTYPE, ONEXEC)
                      VALUES({0}, {1}, {2}, 1, {3})",
                    prcId, "flowend", "收件後行為", "");

                await _context.Database.ExecuteSqlRawAsync(
                    @"INSERT INTO XFLdEVT(PRCID, RELATEID, EVTNAME, EVTTYPE, ONEXEC)
                      VALUES({0}, {1}, {2}, 1, {3})",
                    prcId, "flowend", "寄件行為", "");

                TempData["Success"] = "新增流程成功";
                return RedirectToPage(new { prcId = prcId });
            }
            catch (Exception ex)
            {
                // 取得完整的錯誤訊息，包含內部異常
                var errorMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += $" | Inner: {ex.InnerException.Message}";
                    if (ex.InnerException.InnerException != null)
                    {
                        errorMessage += $" | Inner2: {ex.InnerException.InnerException.Message}";
                    }
                }
                TempData["Error"] = $"新增流程失敗：{errorMessage}";
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

                // 使用原生 SQL 刪除（避免 Trigger 干擾 EF Core OUTPUT 子句）
                // 按照順序刪除：先刪除轉換、事件、活動，最後刪除流程
                await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM XFLdTRA WHERE PRCID = {0}", prcId);

                await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM XFLdEVT WHERE PRCID = {0}", prcId);

                await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM XFLdAct WHERE PRCID = {0}", prcId);

                await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM XFLdPRC WHERE PRCID = {0}", prcId);

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
