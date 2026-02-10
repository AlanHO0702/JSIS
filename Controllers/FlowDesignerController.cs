using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;
using System.Text.Json;

namespace PcbErpApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlowDesignerController : ControllerBase
    {
        private readonly PcbErpContext _context;

        public FlowDesignerController(PcbErpContext context)
        {
            _context = context;
        }

        #region 流程列表與查詢

        /// <summary>
        /// 取得所有流程列表
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<XFLdPRC>>> GetAllFlows()
        {
            try
            {
                var flows = await _context.XFLdPRCs
                    .OrderByDescending(p => p.CDATE)
                    .ToListAsync();

                return Ok(flows);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "取得流程列表失敗", message = ex.Message });
            }
        }

        /// <summary>
        /// 取得單一流程詳情（包含活動、轉換、事件）
        /// </summary>
        [HttpGet("{prcId}")]
        public async Task<ActionResult<object>> GetFlowDetail(string prcId)
        {
            try
            {
                var flow = await _context.XFLdPRCs.FindAsync(prcId);
                if (flow == null)
                {
                    return NotFound(new { error = "找不到指定的流程" });
                }

                var activities = await _context.XFLdActs
                    .Where(a => a.PRCID == prcId)
                    .ToListAsync();

                var transitions = await _context.XFLdTRAs
                    .Where(t => t.PRCID == prcId)
                    .ToListAsync();

                var events = await _context.XFLdEVTs
                    .Where(e => e.PRCID == prcId)
                    .ToListAsync();

                return Ok(new
                {
                    flow = flow,
                    activities = activities,
                    transitions = transitions,
                    events = events
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "取得流程詳情失敗", message = ex.Message });
            }
        }

        #endregion

        #region 新增、更新、刪除流程

        /// <summary>
        /// 新增流程
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<XFLdPRC>> CreateFlow([FromBody] CreateFlowRequest request)
        {
            try
            {
                // 檢查流程代碼是否已存在
                var existingFlow = await _context.XFLdPRCs.FindAsync(request.PRCID);
                if (existingFlow != null)
                {
                    return BadRequest(new { error = "流程代碼已存在" });
                }

                var newFlow = new XFLdPRC
                {
                    PRCID = request.PRCID ?? "",
                    PRCNAME = request.PRCNAME,
                    DESCRIP = request.DESCRIP,
                    CREATOR = request.UserId,
                    CDATE = DateTime.Now,
                    Finished = 0 // 預設為停用
                };

                _context.XFLdPRCs.Add(newFlow);

                // 新增預設事件
                var defaultEvents = new[]
                {
                    new XFLdEVT { PRCID = newFlow.PRCID, RELATEID = "", EVTNAME = "流程初始行為", EVTTYPE = 0, ONEXEC = "" },
                    new XFLdEVT { PRCID = newFlow.PRCID, RELATEID = "", EVTNAME = "流程結束行為", EVTTYPE = 0, ONEXEC = "" },
                    new XFLdEVT { PRCID = newFlow.PRCID, RELATEID = "", EVTNAME = "流程取消行為", EVTTYPE = 0,
                        ONEXEC = "EXEC CURdOCXFlowBackToPaper @單據類型,@單據編號,33,@申請人,@申請人,@流程編號,@代理人" }
                };

                _context.XFLdEVTs.AddRange(defaultEvents);

                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetFlowDetail), new { prcId = newFlow.PRCID }, newFlow);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "新增流程失敗", message = ex.Message });
            }
        }

        /// <summary>
        /// 更新流程資訊
        /// </summary>
        [HttpPut("{prcId}")]
        public async Task<IActionResult> UpdateFlow(string prcId, [FromBody] UpdateFlowRequest request)
        {
            try
            {
                var flow = await _context.XFLdPRCs.FindAsync(prcId);
                if (flow == null)
                {
                    return NotFound(new { error = "找不到指定的流程" });
                }

                // 只有停用狀態才能修改
                if (flow.Finished == 1)
                {
                    return BadRequest(new { error = "已啟用的流程無法修改，請先停用" });
                }

                flow.PRCNAME = request.PRCNAME ?? flow.PRCNAME;
                flow.DESCRIP = request.DESCRIP ?? flow.DESCRIP;
                flow.FLOWCHART = request.FLOWCHART ?? flow.FLOWCHART;
                flow.MODIFICATOR = request.UserId;

                await _context.SaveChangesAsync();

                return Ok(new { message = "更新成功", flow = flow });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "更新流程失敗", message = ex.Message });
            }
        }

        /// <summary>
        /// 刪除流程
        /// </summary>
        [HttpDelete("{prcId}")]
        public async Task<IActionResult> DeleteFlow(string prcId)
        {
            try
            {
                var flow = await _context.XFLdPRCs.FindAsync(prcId);
                if (flow == null)
                {
                    return NotFound(new { error = "找不到指定的流程" });
                }

                // 只有停用狀態才能刪除
                if (flow.Finished == 1)
                {
                    return BadRequest(new { error = "已啟用的流程無法刪除，請先停用" });
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

                return Ok(new { message = "刪除成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "刪除流程失敗", message = ex.Message });
            }
        }

        #endregion

        #region 啟用/停用流程

        /// <summary>
        /// 切換流程啟用狀態
        /// </summary>
        [HttpPost("{prcId}/toggle-status")]
        public async Task<IActionResult> ToggleFlowStatus(string prcId, [FromBody] UpdateStatusRequest request)
        {
            try
            {
                var flow = await _context.XFLdPRCs.FindAsync(prcId);
                if (flow == null)
                {
                    return NotFound(new { error = "找不到指定的流程" });
                }

                flow.Finished = flow.Finished == 1 ? 0 : 1;
                flow.MODIFICATOR = request.UserId;

                await _context.SaveChangesAsync();

                var statusText = flow.Finished == 1 ? "啟用" : "停用";
                return Ok(new { message = $"已{statusText}流程", finished = flow.Finished });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "切換狀態失敗", message = ex.Message });
            }
        }

        #endregion

        #region 複製流程

        /// <summary>
        /// 複製流程
        /// </summary>
        [HttpPost("{prcId}/copy")]
        public async Task<ActionResult<XFLdPRC>> CopyFlow(string prcId, [FromBody] CopyFlowRequest request)
        {
            try
            {
                // 檢查來源流程是否存在
                var sourceFlow = await _context.XFLdPRCs.FindAsync(prcId);
                if (sourceFlow == null)
                {
                    return NotFound(new { error = "找不到來源流程" });
                }

                // 檢查目標流程代碼是否已存在
                var existingFlow = await _context.XFLdPRCs.FindAsync(request.TargetPRCID);
                if (existingFlow != null)
                {
                    return BadRequest(new { error = "目標流程代碼已存在" });
                }

                // 複製流程主檔
                var newFlow = new XFLdPRC
                {
                    PRCID = request.TargetPRCID ?? "",
                    PRCNAME = request.TargetPRCNAME ?? sourceFlow.PRCNAME,
                    DESCRIP = request.TargetDESCRIP ?? sourceFlow.DESCRIP,
                    CREATOR = request.UserId,
                    CDATE = DateTime.Now,
                    FLOWCHART = sourceFlow.FLOWCHART,
                    Finished = 0 // 預設為停用
                };
                _context.XFLdPRCs.Add(newFlow);

                // 複製活動
                var sourceActivities = await _context.XFLdActs.Where(a => a.PRCID == prcId).ToListAsync();
                foreach (var act in sourceActivities)
                {
                    _context.XFLdActs.Add(new XFLdAct
                    {
                        ACTID = act.ACTID,
                        PRCID = newFlow.PRCID,
                        ACTNAME = act.ACTNAME,
                        X = act.X,
                        Y = act.Y,
                        ACTTYPE = act.ACTTYPE
                    });
                }

                // 複製轉換
                var sourceTransitions = await _context.XFLdTRAs.Where(t => t.PRCID == prcId).ToListAsync();
                foreach (var tra in sourceTransitions)
                {
                    _context.XFLdTRAs.Add(new XFLdTRA
                    {
                        TRAID = tra.TRAID,
                        PRCID = newFlow.PRCID,
                        SRCACT = tra.SRCACT,
                        DSTACT = tra.DSTACT,
                        CAPTION = tra.CAPTION,
                        TRATYPE = tra.TRATYPE
                    });
                }

                // 複製事件
                var sourceEvents = await _context.XFLdEVTs.Where(e => e.PRCID == prcId).ToListAsync();
                foreach (var evt in sourceEvents)
                {
                    _context.XFLdEVTs.Add(new XFLdEVT
                    {
                        PRCID = newFlow.PRCID,
                        RELATEID = evt.RELATEID,
                        EVTNAME = evt.EVTNAME,
                        EVTTYPE = evt.EVTTYPE,
                        ONEXEC = evt.ONEXEC
                        // EVTSEQ 會自動生成
                    });
                }

                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetFlowDetail), new { prcId = newFlow.PRCID }, newFlow);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "複製流程失敗", message = ex.Message });
            }
        }

        #endregion

        #region 活動管理

        /// <summary>
        /// 新增或更新活動
        /// </summary>
        [HttpPost("{prcId}/activities")]
        public async Task<IActionResult> SaveActivity(string prcId, [FromBody] XFLdAct activity)
        {
            try
            {
                var flow = await _context.XFLdPRCs.FindAsync(prcId);
                if (flow == null)
                {
                    return NotFound(new { error = "找不到指定的流程" });
                }

                if (flow.Finished == 1)
                {
                    return BadRequest(new { error = "已啟用的流程無法修改活動" });
                }

                activity.PRCID = prcId;

                var existingActivity = await _context.XFLdActs.FindAsync(activity.ACTID);
                if (existingActivity != null)
                {
                    existingActivity.ACTNAME = activity.ACTNAME;
                    existingActivity.X = activity.X;
                    existingActivity.Y = activity.Y;
                    existingActivity.ACTTYPE = activity.ACTTYPE;
                }
                else
                {
                    _context.XFLdActs.Add(activity);
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = "儲存成功", activity = activity });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "儲存活動失敗", message = ex.Message });
            }
        }

        /// <summary>
        /// 刪除活動
        /// </summary>
        [HttpDelete("{prcId}/activities/{actId}")]
        public async Task<IActionResult> DeleteActivity(string prcId, string actId)
        {
            try
            {
                var flow = await _context.XFLdPRCs.FindAsync(prcId);
                if (flow == null || flow.Finished == 1)
                {
                    return BadRequest(new { error = "無法刪除活動" });
                }

                var activity = await _context.XFLdActs.FindAsync(actId);
                if (activity != null)
                {
                    // 刪除相關的轉換
                    var relatedTransitions = await _context.XFLdTRAs
                        .Where(t => t.SRCACT == actId || t.DSTACT == actId)
                        .ToListAsync();
                    _context.XFLdTRAs.RemoveRange(relatedTransitions);

                    _context.XFLdActs.Remove(activity);
                    await _context.SaveChangesAsync();
                }

                return Ok(new { message = "刪除成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "刪除活動失敗", message = ex.Message });
            }
        }

        /// <summary>
        /// 批次儲存活動位置（佈局用，啟用的流程也能存）
        /// </summary>
        [HttpPut("{prcId}/positions")]
        public async Task<IActionResult> SavePositions(string prcId, [FromBody] List<ActivityPosition> positions)
        {
            try
            {
                var flow = await _context.XFLdPRCs.FindAsync(prcId);
                if (flow == null)
                {
                    return NotFound(new { error = "找不到指定的流程" });
                }

                // 使用原生 SQL 避免觸發器干擾 EF Core 的 OUTPUT 子句
                foreach (var pos in positions)
                {
                    if (string.IsNullOrWhiteSpace(pos.ActId)) continue;

                    await _context.Database.ExecuteSqlRawAsync(
                        "UPDATE XFLdAct SET X = {0}, Y = {1} WHERE ACTID = {2} AND PRCID = {3}",
                        pos.X, pos.Y, pos.ActId, prcId);
                }

                return Ok(new { message = "位置儲存成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "儲存位置失敗", message = ex.Message });
            }
        }

        #endregion

        #region 轉換管理

        /// <summary>
        /// 新增或更新轉換
        /// </summary>
        [HttpPost("{prcId}/transitions")]
        public async Task<IActionResult> SaveTransition(string prcId, [FromBody] XFLdTRA transition)
        {
            try
            {
                var flow = await _context.XFLdPRCs.FindAsync(prcId);
                if (flow == null || flow.Finished == 1)
                {
                    return BadRequest(new { error = "無法修改轉換" });
                }

                transition.PRCID = prcId;

                var existingTransition = await _context.XFLdTRAs.FindAsync(prcId, transition.TRAID);
                if (existingTransition != null)
                {
                    existingTransition.SRCACT = transition.SRCACT;
                    existingTransition.DSTACT = transition.DSTACT;
                    existingTransition.CAPTION = transition.CAPTION;
                    existingTransition.TRATYPE = transition.TRATYPE;
                }
                else
                {
                    _context.XFLdTRAs.Add(transition);
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = "儲存成功", transition = transition });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "儲存轉換失敗", message = ex.Message });
            }
        }

        /// <summary>
        /// 刪除轉換
        /// </summary>
        [HttpDelete("{prcId}/transitions/{traId}")]
        public async Task<IActionResult> DeleteTransition(string prcId, string traId)
        {
            try
            {
                var flow = await _context.XFLdPRCs.FindAsync(prcId);
                if (flow == null || flow.Finished == 1)
                {
                    return BadRequest(new { error = "無法刪除轉換" });
                }

                var transition = await _context.XFLdTRAs.FindAsync(traId);
                if (transition != null)
                {
                    _context.XFLdTRAs.Remove(transition);
                    await _context.SaveChangesAsync();
                }

                return Ok(new { message = "刪除成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "刪除轉換失敗", message = ex.Message });
            }
        }

        #endregion

        #region 事件管理

        /// <summary>
        /// 更新事件
        /// </summary>
        [HttpPut("{prcId}/events")]
        public async Task<IActionResult> UpdateEvents(string prcId, [FromBody] List<XFLdEVT> events)
        {
            try
            {
                var flow = await _context.XFLdPRCs.FindAsync(prcId);
                if (flow == null || flow.Finished == 1)
                {
                    return BadRequest(new { error = "無法修改事件" });
                }

                // 刪除舊的事件並新增新的
                var oldEvents = await _context.XFLdEVTs.Where(e => e.PRCID == prcId).ToListAsync();
                _context.XFLdEVTs.RemoveRange(oldEvents);

                foreach (var evt in events)
                {
                    evt.PRCID = prcId;
                    _context.XFLdEVTs.Add(evt);
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = "更新成功", events = events });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "更新事件失敗", message = ex.Message });
            }
        }

        #endregion
    }

    #region Request Models

    public class CreateFlowRequest
    {
        public string? PRCID { get; set; }
        public string? PRCNAME { get; set; }
        public string? DESCRIP { get; set; }
        public string? UserId { get; set; }
    }

    public class UpdateFlowRequest
    {
        public string? PRCNAME { get; set; }
        public string? DESCRIP { get; set; }
        public string? FLOWCHART { get; set; }
        public string? UserId { get; set; }
    }

    public class UpdateStatusRequest
    {
        public string? UserId { get; set; }
    }

    public class CopyFlowRequest
    {
        public string? TargetPRCID { get; set; }
        public string? TargetPRCNAME { get; set; }
        public string? TargetDESCRIP { get; set; }
        public string? UserId { get; set; }
    }

    public class ActivityPosition
    {
        public string ActId { get; set; } = string.Empty;
        public int X { get; set; }
        public int Y { get; set; }
    }

    #endregion
}
