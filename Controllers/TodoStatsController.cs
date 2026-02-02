using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace PcbErpApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoStatsController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public TodoStatsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> GetTodoStats()
        {
            // 從 HttpContext.Items 或 localStorage 取得 UserId
            var userId = HttpContext.Items["UserId"]?.ToString()
                         ?? User.Identity?.Name
                         ?? Request.Query["userId"].ToString()
                         ?? "admin";

            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    var pendingCount = await GetPendingSignCount(connection, userId);

                    var stats = new
                    {
                        pendingSign = pendingCount,
                        messages = await GetUnreadMessagesCount(connection, userId),
                        notifications = await GetNotificationsCount(connection, userId),
                        sent = await GetSentMessagesCount(connection, userId),
                        // 除錯資訊
                        debug = new
                        {
                            userId = userId,
                            pendingSignQuery = $"SELECT COUNT(*) FROM XFLdeFlowInfo WHERE USERID = '{userId}'"
                        }
                    };

                    return Ok(stats);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        private async Task<int> GetPendingSignCount(SqlConnection connection, string userId)
        {
            try
            {
                // 取得簽核待審數量（與 PendingApprovals 頁面使用相同的查詢邏輯）
                var query = @"
                    SELECT COUNT(*)
                    FROM XFLdeFlowInfo f WITH (NOLOCK)
                    WHERE f.USERID = @UserId";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    var result = await command.ExecuteScalarAsync();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
            catch (Exception ex)
            {
                // 如果查詢失敗，記錄錯誤並返回 0
                Console.WriteLine($"GetPendingSignCount Error: {ex.Message}");
                throw new Exception($"查詢簽核待審失敗: {ex.Message}", ex);
            }
        }

        private async Task<int> GetUnreadMessagesCount(SqlConnection connection, string userId)
        {
            try
            {
                // 取得未讀訊息數量 - 根據您的實際資料表結構調整
                var query = @"
                    SELECT COUNT(*)
                    FROM CURdMailMsg (NOLOCK)
                    WHERE RecvId = @UserId
                    AND ReadFlag = 0";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    var result = await command.ExecuteScalarAsync();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
            catch (Exception)
            {
                // 如果資料表不存在或查詢失敗，返回 0
                return 0;
            }
        }

        private async Task<int> GetNotificationsCount(SqlConnection connection, string userId)
        {
            try
            {
                // 取得簽核通知數量 - 根據您的實際資料表結構調整
                var query = @"
                    SELECT COUNT(*)
                    FROM XFLdMailNotify (NOLOCK)
                    WHERE RecvId = @UserId
                    AND ReadFlag = 0";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    var result = await command.ExecuteScalarAsync();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
            catch (Exception)
            {
                // 如果資料表不存在或查詢失敗，返回 0
                return 0;
            }
        }

        private async Task<int> GetSentMessagesCount(SqlConnection connection, string userId)
        {
            try
            {
                // 取得已發送訊息數量 - 根據您的實際資料表結構調整
                var query = @"
                    SELECT COUNT(*)
                    FROM CURdMailMsg (NOLOCK)
                    WHERE SenderId = @UserId
                    AND SendTime >= DATEADD(day, -7, GETDATE())"; // 最近7天

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    var result = await command.ExecuteScalarAsync();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
            catch (Exception)
            {
                // 如果資料表不存在或查詢失敗，返回 0
                return 0;
            }
        }
    }
}
