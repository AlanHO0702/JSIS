using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using System.Data;

namespace PcbErpApi.Controllers;

/// <summary>
/// 批量過帳審核 API Controller
/// 對應 Delphi PassBatchExam.pas
/// </summary>
[Route("api/[controller]")]
[ApiController]
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public class FMEdPassBatchExamController : ControllerBase
{
    private readonly PcbErpContext _context;
    private readonly ILogger<FMEdPassBatchExamController> _logger;

    public FMEdPassBatchExamController(PcbErpContext context, ILogger<FMEdPassBatchExamController> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region 查詢待審核過帳單

    /// <summary>
    /// 查詢待審核的過帳單列表
    /// 對應 Delphi btFindClick
    /// </summary>
    [HttpPost("query")]
    public async Task<IActionResult> QueryPassList([FromBody] QueryPassRequest request)
    {
        try
        {
            var cs = _context.Database.GetConnectionString();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            var list = new List<Dictionary<string, object?>>();

            await using (var cmd = new SqlCommand("exec FMEdPassBatchExamGet @UserId, @ProcCode, @LineId, @LotNum", conn))
            {
                cmd.Parameters.AddWithValue("@UserId", request.UserId ?? "");
                cmd.Parameters.AddWithValue("@ProcCode", request.ProcCode ?? "");
                cmd.Parameters.AddWithValue("@LineId", request.LineId ?? "");
                cmd.Parameters.AddWithValue("@LotNum", request.LotNum ?? "");

                await using var rd = await cmd.ExecuteReaderAsync();
                while (await rd.ReadAsync())
                {
                    list.Add(ReadRowToDictionary(rd));
                }
            }

            return Ok(new { success = true, data = list });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "查詢待審核過帳單失敗");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    #endregion

    #region 查詢過帳明細

    /// <summary>
    /// 查詢單號明細
    /// 對應 Delphi msProcSelectSourceClick / msProcSelectTargetClick
    /// </summary>
    [HttpPost("detail")]
    public async Task<IActionResult> QueryPassDetail([FromBody] QueryDetailRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.PaperNum))
                return BadRequest(new { success = false, message = "請提供單號" });

            var cs = _context.Database.GetConnectionString();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            var list = new List<Dictionary<string, object?>>();

            await using (var cmd = new SqlCommand("exec FMEdPassSubView @PaperNum", conn))
            {
                cmd.Parameters.AddWithValue("@PaperNum", request.PaperNum);

                await using var rd = await cmd.ExecuteReaderAsync();
                while (await rd.ReadAsync())
                {
                    list.Add(ReadRowToDictionary(rd));
                }
            }

            // 加載原始狀態 lookup (LotStatus -> LotStatusName)
            await EnrichLotStatusAsync(conn, list);

            return Ok(new { success = true, data = list });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "查詢過帳明細失敗: {PaperNum}", request.PaperNum);
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    #endregion

    #region 審核/退審

    /// <summary>
    /// 批量審核過帳單
    /// 對應 Delphi btnExecuteClick
    /// </summary>
    [HttpPost("approve")]
    public async Task<IActionResult> ApprovePass([FromBody] ApproveRequest request)
    {
        try
        {
            if (request.PaperNums == null || request.PaperNums.Count == 0)
                return BadRequest(new { success = false, message = "請先選擇要審核的過帳單" });

            var cs = _context.Database.GetConnectionString();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            // 檢查權限 (對應 CURdItemsCheck paramId='0')
            var hasPermission = await CheckPermission(conn, request.UserId ?? "", "0");
            if (!hasPermission)
                return BadRequest(new { success = false, message = "沒有【已過帳單確認】權限" });

            var successCount = 0;
            var failCount = 0;
            var errors = new List<string>();

            foreach (var paperNum in request.PaperNums)
            {
                try
                {
                    await using var cmd = new SqlCommand("exec CURdPaperAction @TableName, @PaperNum, @UserId, @Action, @Type", conn);
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@TableName", "FMEdPassMain");
                    cmd.Parameters.AddWithValue("@PaperNum", paperNum);
                    cmd.Parameters.AddWithValue("@UserId", request.UserId ?? "");
                    cmd.Parameters.AddWithValue("@Action", 1);  // 1=確認
                    cmd.Parameters.AddWithValue("@Type", 1);    // 1=審核

                    await cmd.ExecuteNonQueryAsync();
                    successCount++;
                }
                catch (SqlException sqlEx)
                {
                    failCount++;
                    errors.Add($"{paperNum}: {sqlEx.Message}");
                    _logger.LogWarning(sqlEx, "審核失敗: {PaperNum}", paperNum);
                }
            }

            var message = $"已完成審核。成功: {successCount} 筆";
            if (failCount > 0)
                message += $"，失敗: {failCount} 筆";

            return Ok(new
            {
                success = true,
                message,
                successCount,
                failCount,
                errors
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量審核失敗");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// 退審過帳單
    /// 對應 Delphi btnRejectExamClick
    /// </summary>
    [HttpPost("reject")]
    public async Task<IActionResult> RejectPass([FromBody] RejectRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.PaperNum))
                return BadRequest(new { success = false, message = "請選擇要退審的過帳單" });

            var cs = _context.Database.GetConnectionString();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            // 檢查權限 (對應 CURdItemsCheck paramId='1')
            var hasPermission = await CheckPermission(conn, request.UserId ?? "", "1");
            if (!hasPermission)
                return BadRequest(new { success = false, message = "沒有【已過帳單退審】權限" });

            await using var cmd = new SqlCommand("exec CURdPaperAction @TableName, @PaperNum, @UserId, @Action, @Type", conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@TableName", "FMEdPassMain");
            cmd.Parameters.AddWithValue("@PaperNum", request.PaperNum);
            cmd.Parameters.AddWithValue("@UserId", request.UserId ?? "");
            cmd.Parameters.AddWithValue("@Action", 0);  // 0=取消
            cmd.Parameters.AddWithValue("@Type", 2);    // 2=退審

            await cmd.ExecuteNonQueryAsync();

            return Ok(new { success = true, message = $"單號 {request.PaperNum} 已退審" });
        }
        catch (SqlException sqlEx)
        {
            _logger.LogWarning(sqlEx, "退審失敗: {PaperNum}", request.PaperNum);
            return BadRequest(new { success = false, message = sqlEx.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "退審失敗");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    #endregion

    #region 權限檢查

    /// <summary>
    /// 檢查使用者權限
    /// 對應 Delphi CURdItemsCheck SP
    /// </summary>
    [HttpPost("check-permission")]
    public async Task<IActionResult> CheckUserPermission([FromBody] PermissionRequest request)
    {
        try
        {
            var cs = _context.Database.GetConnectionString();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            var hasApprove = await CheckPermission(conn, request.UserId ?? "", "0");
            var hasReject = await CheckPermission(conn, request.UserId ?? "", "1");

            return Ok(new
            {
                success = true,
                hasApprovePermission = hasApprove,
                hasRejectPermission = hasReject
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "檢查權限失敗");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// 內部權限檢查方法
    /// </summary>
    private async Task<bool> CheckPermission(SqlConnection conn, string userId, string paramId)
    {
        await using var cmd = new SqlCommand("exec CURdItemsCheck @UserId, @ParamId", conn);
        cmd.Parameters.AddWithValue("@UserId", userId);
        cmd.Parameters.AddWithValue("@ParamId", paramId);

        await using var rd = await cmd.ExecuteReaderAsync();
        // 如果有返回記錄，表示有權限
        return await rd.ReadAsync();
    }

    /// <summary>
    /// 加載原始狀態名稱 (LotStatus -> LotStatusName)
    /// </summary>
    private async Task EnrichLotStatusAsync(SqlConnection conn, List<Dictionary<string, object?>> list)
    {
        if (list.Count == 0) return;

        try
        {
            var statusDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            await using (var cmd = new SqlCommand("SELECT LotStatus, LotStatusName FROM EMOdStatus WITH(NOLOCK)", conn))
            await using (var rd = await cmd.ExecuteReaderAsync())
            {
                while (await rd.ReadAsync())
                {
                    var status = rd["LotStatus"]?.ToString();
                    var statusName = rd["LotStatusName"]?.ToString();
                    if (!string.IsNullOrEmpty(status) && !string.IsNullOrEmpty(statusName))
                    {
                        statusDict[status] = statusName;
                    }
                }
            }

            // 為每筆資料加上 LotStatusName
            foreach (var row in list)
            {
                if (row.ContainsKey("LotStatus"))
                {
                    var lotStatus = row["LotStatus"]?.ToString();
                    if (!string.IsNullOrEmpty(lotStatus) && statusDict.ContainsKey(lotStatus))
                    {
                        row["LotStatusName"] = statusDict[lotStatus];
                    }
                    else
                    {
                        row["LotStatusName"] = lotStatus; // 如果找不到對應的名稱，就顯示代碼
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "加載原始狀態名稱失敗，將略過此步驟");
            // 如果查詢失敗，不影響主要資料的返回
        }
    }

    #endregion

    #region Helper Methods

    private static Dictionary<string, object?> ReadRowToDictionary(SqlDataReader reader)
    {
        var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < reader.FieldCount; i++)
        {
            var name = reader.GetName(i);
            dict[name] = reader.IsDBNull(i) ? null : reader.GetValue(i);
        }
        return dict;
    }

    #endregion

    #region Request Models

    public class QueryPassRequest
    {
        public string? UserId { get; set; }
        public string? ProcCode { get; set; }
        public string? LineId { get; set; }
        public string? LotNum { get; set; }
    }

    public class QueryDetailRequest
    {
        public string PaperNum { get; set; } = "";
    }

    public class ApproveRequest
    {
        public string? UserId { get; set; }
        public List<string> PaperNums { get; set; } = new();
    }

    public class RejectRequest
    {
        public string? UserId { get; set; }
        public string PaperNum { get; set; } = "";
    }

    public class PermissionRequest
    {
        public string? UserId { get; set; }
    }

    #endregion
}
