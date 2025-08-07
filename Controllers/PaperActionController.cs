using Microsoft.AspNetCore.Mvc;
using PcbErpApi.Data;
using PcbErpApi.Models;
using PcbErpApi.Helpers;
using static PcbErpApi.Helpers.DynamicQueryHelper;
using System.Text.Json.Serialization;
using System.Data.SqlClient;
using System.Data;

namespace PcbErpApi.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class PaperActionController : ControllerBase
    {
        private readonly IConfiguration _config;

        public PaperActionController(IConfiguration config)
        {
            _config = config;
        }

        public class DoActionRequest
        {
            public string PaperId { get; set; }
            public string PaperNum { get; set; }
            public string UserId { get; set; }
            public int EOC { get; set; }
            public int AftFinished { get; set; }
        }

        [HttpPost("DoAction")]
        public async Task<IActionResult> DoAction([FromBody] DoActionRequest req)
        {
            if (string.IsNullOrEmpty(req.PaperId) || string.IsNullOrEmpty(req.PaperNum))
                return BadRequest("缺少必要參數");

            var connStr = _config.GetConnectionString("DefaultConnection");
            using var conn = new SqlConnection(connStr);
            using var cmd = new SqlCommand("CURdPaperAction", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@PaperId", req.PaperId);
            cmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);
            cmd.Parameters.AddWithValue("@UserId", req.UserId ?? "admin"); // 預設值
            cmd.Parameters.AddWithValue("@EOC", req.EOC);
            cmd.Parameters.AddWithValue("@AftFinished", req.AftFinished);

            try
            {
                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
                return Ok(new { message = "已成功執行 CURdPaperAction" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
