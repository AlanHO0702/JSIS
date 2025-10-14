// ✅ Controller: ReportController.cs
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Microsoft.Data.SqlClient;

namespace PcbErpApi.Controllers
{
    [ApiController]
    [Route("api/report")]
    public class ReportController : ControllerBase
    {
        private readonly IConfiguration _config;
        public ReportController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost("generate-url")]
        public IActionResult GenerateUrl([FromBody] BuildRequest req)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(req.SpName))
                    return BadRequest(new { error = "缺少 SpName 參數" });

                var connStr = _config.GetConnectionString("DefaultConnection");
                using var conn = new SqlConnection(connStr);
                using var cmd = conn.CreateCommand();

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = req.SpName;  // ✅ 改成使用傳入的參數
                cmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);

                conn.Open();
                cmd.ExecuteNonQuery();

                var reportUrl = $"{_config["ReportApi:CrystalUrl"]}/api/report/render?reportName={Uri.EscapeDataString(req.ReportName)}&paperNum={Uri.EscapeDataString(req.PaperNum)}&format=pdf";

                return Ok(new { reportUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        public class BuildRequest
        {
            public string PaperNum { get; set; } = string.Empty;
            public string SessionId { get; set; } = string.Empty;
            public string SpName { get; set; } = string.Empty;
            public string ReportName { get; set; } = string.Empty;
        }

        public class ReportResponse
        {
            public string ReportUrl { get; set; } = string.Empty;
        }
    }
}
