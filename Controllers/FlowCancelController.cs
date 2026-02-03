using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace PcbErpApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlowCancelController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public FlowCancelController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // 取消簽核
        [HttpPost]
        public async Task<IActionResult> CancelFlow([FromBody] CancelFlowRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.PrcSeq))
            {
                return BadRequest(new { error = "PrcSeq is required" });
            }

            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                return BadRequest(new { error = "UserId is required" });
            }

            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // 執行取消簽核的 stored procedure: XFLdPRCCANCEL
                    using (var command = new SqlCommand("XFLdPRCCANCEL", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@PRCSEQ", request.PrcSeq);
                        command.Parameters.AddWithValue("@USERID", request.UserId);

                        await command.ExecuteNonQueryAsync();
                    }
                }

                return Ok(new { message = "已取消" });
            }
            catch (SqlException ex)
            {
                // SQL Server 錯誤處理
                // 檢查是否為權限錯誤訊息
                if (ex.Message.Contains("非原申請人不可抽單") || ex.Message.Contains("非簽等待人"))
                {
                    return BadRequest(new { error = ex.Message });
                }
                return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }
    }

    public class CancelFlowRequest
    {
        public string? PrcSeq { get; set; }
        public string? UserId { get; set; }
    }
}
