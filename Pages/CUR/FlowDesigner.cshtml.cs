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

        /// <summary>
        /// 查詢會簽人員列表
        /// </summary>
        public async Task<IActionResult> OnGetMultiSignUsersAsync(string prcId, string actId)
        {
            try
            {
                var users = await _context.CURdFlowMultiSignUsers
                    .Where(u => u.PRCID == prcId && u.ACTID == actId)
                    .OrderBy(u => u.UserId)
                    .ToListAsync();

                // 如果沒有會簽人員，直接返回空列表
                if (users.Count == 0)
                {
                    return new JsonResult(new { success = true, data = users });
                }

                // 從 CURdUsers 表取得員工姓名
                var userIds = users.Select(u => u.UserId).ToList();

                // 使用原生 SQL 查詢避免 EF Core 參數化問題
                var userIdList = string.Join("','", userIds.Select(id => id.Replace("'", "''")));
                var sql = $"SELECT UserId, UserName FROM CURdUsers WHERE UserId IN ('{userIdList}')";

                var userNames = await _context.Database
                    .SqlQueryRaw<UserNameDto>(sql)
                    .ToListAsync();

                // 合併資料
                foreach (var user in users)
                {
                    var userName = userNames.FirstOrDefault(un => un.UserId == user.UserId);
                    user.UserName = userName?.UserName;
                }

                return new JsonResult(new { success = true, data = users });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 查詢所有啟用的員工（用於下拉選單）
        /// </summary>
        public async Task<IActionResult> OnGetActiveUsersAsync()
        {
            try
            {
                var users = await _context.CurdUsers
                    .Where(u => u.Permit == 1)
                    .OrderBy(u => u.UserId)
                    .Select(u => new { u.UserId, u.UserName })
                    .ToListAsync();

                return new JsonResult(new { success = true, data = users });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 新增會簽人員
        /// </summary>
        public async Task<IActionResult> OnPostAddMultiSignUserAsync([FromBody] MultiSignUserRequest request)
        {
            try
            {
                CurrentUserId = HttpContext.Items["UserId"]?.ToString() ?? "admin";

                // 檢查是否已存在
                var exists = await _context.CURdFlowMultiSignUsers
                    .AnyAsync(u => u.PRCID == request.PrcId && u.ACTID == request.ActId && u.UserId == request.UserId);

                if (exists)
                {
                    return new JsonResult(new { success = false, message = "該員工已在會簽人員清單中" });
                }

                var multiSignUser = new CURdFlowMultiSignUser
                {
                    PRCID = request.PrcId,
                    ACTID = request.ActId,
                    UserId = request.UserId,
                    BuildDate = DateTime.Now,
                    Update_UserId = CurrentUserId
                };

                _context.CURdFlowMultiSignUsers.Add(multiSignUser);
                await _context.SaveChangesAsync();

                // 檢查是否為第一個會簽人員，如果是則自動設定會簽欄位
                var multiSignUserCount = await _context.CURdFlowMultiSignUsers
                    .CountAsync(u => u.PRCID == request.PrcId && u.ACTID == request.ActId);

                if (multiSignUserCount == 1)
                {
                    // 第一次新增會簽人員，自動設定會簽相關欄位
                    await _context.Database.ExecuteSqlRawAsync(
                        @"UPDATE XFLdAct
                          SET iMultiSign = 1,
                              iMultiSignAllow = 1,
                              ALLOWRETURN = 0,
                              ALLOWADD = 0
                          WHERE ACTID = {0} AND PRCID = {1}",
                        request.ActId, request.PrcId);

                    // 清空 ONEXEC 欄位（收件前行為）
                    await _context.Database.ExecuteSqlRawAsync(
                        @"UPDATE XFLdEVT
                          SET ONEXEC = ''
                          WHERE PRCID = {0} AND RELATEID = {1} AND EVTNAME = {2}",
                        request.PrcId, request.ActId, "收件前行為");
                }

                return new JsonResult(new { success = true, message = "新增成功", isFirstUser = multiSignUserCount == 1 });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 刪除會簽人員
        /// </summary>
        public async Task<IActionResult> OnPostDeleteMultiSignUserAsync([FromBody] MultiSignUserRequest request)
        {
            try
            {
                var multiSignUser = await _context.CURdFlowMultiSignUsers
                    .FirstOrDefaultAsync(u => u.PRCID == request.PrcId && u.ACTID == request.ActId && u.UserId == request.UserId);

                if (multiSignUser == null)
                {
                    return new JsonResult(new { success = false, message = "找不到該會簽人員" });
                }

                _context.CURdFlowMultiSignUsers.Remove(multiSignUser);
                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true, message = "刪除成功" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 儲存會簽設定（更新活動的會簽欄位）
        /// </summary>
        public async Task<IActionResult> OnPostSaveMultiSignSettingAsync([FromBody] SaveMultiSignSettingRequest request)
        {
            try
            {
                // 驗證核准的同意人數至少為 1
                if (request.MultiSignAllowCount < 1)
                {
                    return new JsonResult(new { success = false, message = "核准的同意人數至少為 1" });
                }

                // 更新活動的會簽相關欄位
                await _context.Database.ExecuteSqlRawAsync(
                    @"UPDATE XFLdAct
                      SET iMultiSign = 1,
                          iMultiSignAllow = {2},
                          ALLOWRETURN = 0,
                          ALLOWADD = 0
                      WHERE ACTID = {0} AND PRCID = {1}",
                    request.ActId, request.PrcId, request.MultiSignAllowCount);

                // 清空 ONEXEC 欄位（收件前行為）
                await _context.Database.ExecuteSqlRawAsync(
                    @"UPDATE XFLdEVT
                      SET ONEXEC = ''
                      WHERE PRCID = {0} AND RELATEID = {1} AND EVTNAME = {2}",
                    request.PrcId, request.ActId, "收件前行為");

                return new JsonResult(new { success = true, message = "會簽設定已儲存" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 取消會簽設定
        /// </summary>
        public async Task<IActionResult> OnPostCancelMultiSignAsync([FromBody] CancelMultiSignRequest request)
        {
            try
            {
                // 刪除所有會簽人員
                await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM CURdFlowMultiSignUser WHERE PRCID = {0} AND ACTID = {1}",
                    request.PrcId, request.ActId);

                // 更新活動欄位：取消會簽設定，並恢復允許加簽和允許退回為預設值
                await _context.Database.ExecuteSqlRawAsync(
                    @"UPDATE XFLdAct
                      SET iMultiSign = 0,
                          iMultiSignAllow = NULL,
                          ALLOWADD = 1,
                          ALLOWRETURN = 1
                      WHERE ACTID = {0} AND PRCID = {1}",
                    request.ActId, request.PrcId);

                return new JsonResult(new { success = true, message = "會簽設定已取消" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 儲存活動內容
        /// </summary>
        public async Task<IActionResult> OnPostSaveActivityContentAsync([FromBody] SaveActivityContentRequest request)
        {
            try
            {
                // 更新活動主檔
                await _context.Database.ExecuteSqlRawAsync(
                    @"UPDATE XFLdAct
                      SET ACTNAME = {0},
                          DESCRIP = {1},
                          ALLOWADD = {2},
                          ALLOWRETURN = {3},
                          ALLOWMAIL = {4},
                          RECTYPE = {5},
                          RECPARAM = {6}
                      WHERE ACTID = {7} AND PRCID = {8}",
                    request.ActName,
                    request.Descrip ?? "",
                    request.AllowAdd ? 1 : 0,
                    request.AllowReturn ? 1 : 0,
                    request.AllowMail ? 1 : 0,
                    request.RecType ?? "",
                    request.RecParam ?? "",
                    request.ActId,
                    request.PrcId);

                // 更新活動事件
                if (request.Events != null && request.Events.Count > 0)
                {
                    foreach (var evt in request.Events)
                    {
                        await _context.Database.ExecuteSqlRawAsync(
                            @"UPDATE XFLdEVT
                              SET ONEXEC = {0}
                              WHERE PRCID = {1} AND RELATEID = {2} AND EVTNAME = {3}",
                            evt.OnExec ?? "",
                            request.PrcId,
                            request.ActId,
                            evt.EvtName);
                    }
                }

                return new JsonResult(new { success = true, message = "儲存成功" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 查詢收件類型列表
        /// </summary>
        public async Task<IActionResult> OnGetRecTypesAsync()
        {
            try
            {
                var recTypes = await _context.Database
                    .SqlQueryRaw<RecTypeDto>("SELECT SEQ, ACTRECTYPE, DEFSTAT, LOOKUPSQL FROM XFLdRECTYPE (NOLOCK) ORDER BY SEQ")
                    .ToListAsync();

                return new JsonResult(new { success = true, data = recTypes });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 根據收件類型查詢參數列表
        /// </summary>
        public async Task<IActionResult> OnGetRecTypeParamsAsync(string recType)
        {
            try
            {
                // 查詢收件類型的 LOOKUPSQL
                var recTypeData = await _context.Database
                    .SqlQueryRaw<RecTypeDto>(
                        "SELECT SEQ, ACTRECTYPE, DEFSTAT, LOOKUPSQL FROM XFLdRECTYPE (NOLOCK) WHERE ACTRECTYPE = {0}",
                        recType)
                    .FirstOrDefaultAsync();

                if (recTypeData == null || string.IsNullOrWhiteSpace(recTypeData.LOOKUPSQL))
                {
                    return new JsonResult(new { success = true, data = new List<object>(), hasParams = false });
                }

                // 執行 LOOKUPSQL 來查詢參數列表
                var connection = _context.Database.GetDbConnection();
                var command = connection.CreateCommand();
                command.CommandText = recTypeData.LOOKUPSQL;

                await connection.OpenAsync();
                var reader = await command.ExecuteReaderAsync();

                var parameters = new List<Dictionary<string, object>>();
                while (await reader.ReadAsync())
                {
                    var param = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        param[reader.GetName(i)] = reader.GetValue(i) ?? "";
                    }
                    parameters.Add(param);
                }
                await reader.CloseAsync();
                await connection.CloseAsync();

                return new JsonResult(new { success = true, data = parameters, hasParams = true, defstat = recTypeData.DEFSTAT });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 查詢所有流程列表（用於複製流程下拉選單）
        /// </summary>
        public async Task<IActionResult> OnGetFlowListAsync()
        {
            try
            {
                var flows = await _context.XFLdPRCs
                    .OrderBy(p => p.PRCID)
                    .Select(p => new { p.PRCID, p.PRCNAME, p.DESCRIP })
                    .ToListAsync();

                return new JsonResult(new { success = true, data = flows });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 複製流程
        /// </summary>
        public async Task<IActionResult> OnPostCopyFlowAsync([FromBody] CopyFlowRequest request)
        {
            try
            {
                CurrentUserId = HttpContext.Items["UserId"]?.ToString() ?? "admin";

                // 檢查目的流程代碼是否已存在
                var existingFlow = await _context.XFLdPRCs.FindAsync(request.TargetPrcId);
                if (existingFlow != null)
                {
                    return new JsonResult(new { success = false, message = "目的流程代碼已存在" });
                }

                // 執行複製流程的 stored procedure
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC CURdFlowPrcCopy {0}, {1}, {2}, {3}, {4}",
                    request.SourcePrcId,
                    request.TargetPrcId,
                    request.TargetPrcName,
                    request.TargetDescrip ?? "",
                    CurrentUserId);

                return new JsonResult(new { success = true, message = "複製成功" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }
    }

    public class FinishedOption
    {
        public int Value { get; set; }
        public string Name { get; set; } = "";
    }

    public class UserNameDto
    {
        public string UserId { get; set; } = "";
        public string? UserName { get; set; }
    }

    public class MultiSignUserRequest
    {
        public string PrcId { get; set; } = "";
        public string ActId { get; set; } = "";
        public string UserId { get; set; } = "";
    }

    public class SaveMultiSignSettingRequest
    {
        public string PrcId { get; set; } = "";
        public string ActId { get; set; } = "";
        public int MultiSignAllowCount { get; set; }
    }

    public class CancelMultiSignRequest
    {
        public string PrcId { get; set; } = "";
        public string ActId { get; set; } = "";
    }

    public class SaveActivityContentRequest
    {
        public string PrcId { get; set; } = "";
        public string ActId { get; set; } = "";
        public string ActName { get; set; } = "";
        public string? Descrip { get; set; }
        public bool AllowAdd { get; set; }
        public bool AllowReturn { get; set; }
        public bool AllowMail { get; set; }
        public string? RecType { get; set; }
        public string? RecParam { get; set; }
        public List<ActivityEventDto>? Events { get; set; }
    }

    public class ActivityEventDto
    {
        public string EvtName { get; set; } = "";
        public string? OnExec { get; set; }
    }

    public class RecTypeDto
    {
        public byte SEQ { get; set; }
        public string? ACTRECTYPE { get; set; }
        public string? DEFSTAT { get; set; }
        public string? LOOKUPSQL { get; set; }
    }

    public class CopyFlowRequest
    {
        public string SourcePrcId { get; set; } = "";
        public string TargetPrcId { get; set; } = "";
        public string TargetPrcName { get; set; } = "";
        public string? TargetDescrip { get; set; }
    }
}
