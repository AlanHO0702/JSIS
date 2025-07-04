// ✅ Controller: ReportController.cs
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;

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
                var connStr = _config.GetConnectionString("DefaultConnection");
                using var conn = new SqlConnection(connStr);
                using var cmd = conn.CreateCommand();

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "AJNdJourPaper_CIR";
                cmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);

                conn.Open();
                cmd.ExecuteNonQuery();

                var reportUrl = $"{_config["ReportApi:CrystalUrl"]}?paperNum={Uri.EscapeDataString(req.PaperNum)}&format=pdf";
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
        }

        public class ReportResponse
        {
            public string ReportUrl { get; set; } = string.Empty;
        }
    }
}
