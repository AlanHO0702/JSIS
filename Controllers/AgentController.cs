using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using PcbErpApi.Models;

namespace PcbErpApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AgentController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AgentController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // 取得代理人列表
        [HttpGet]
        public async Task<IActionResult> GetAgents([FromQuery] string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest(new { error = "UserId is required" });
            }

            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            var agents = new List<XFLdAgent>();

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // 檢查資料表是否存在
                    var checkTableQuery = @"
                        SELECT COUNT(*)
                        FROM INFORMATION_SCHEMA.TABLES
                        WHERE TABLE_NAME = 'XFLdAgent'";

                    using (var checkCommand = new SqlCommand(checkTableQuery, connection))
                    {
                        var tableExists = Convert.ToInt32(await checkCommand.ExecuteScalarAsync()) > 0;

                        if (!tableExists)
                        {
                            // 資料表不存在，返回空列表
                            return Ok(new {
                                agents = new List<XFLdAgent>(),
                                warning = "資料表 XFLdAgent 不存在，請先建立資料表"
                            });
                        }
                    }

                    var query = @"
                        SELECT iSeq, USERID, AGENTID, SDATE, EDATE, AGENTPART
                        FROM XFLdAgent
                        WHERE USERID = @UserId
                        ORDER BY SDATE DESC";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", userId);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                agents.Add(new XFLdAgent
                                {
                                    iSeq = reader["iSeq"] as int?,
                                    USERID = reader["USERID"]?.ToString(),
                                    AGENTID = reader["AGENTID"]?.ToString(),
                                    SDATE = reader["SDATE"] as DateTime?,
                                    EDATE = reader["EDATE"] as DateTime?,
                                    AGENTPART = reader["AGENTPART"]?.ToString()
                                });
                            }
                        }
                    }
                }

                return Ok(agents);
            }
            catch (SqlException ex) when (ex.Number == 208) // 無效的物件名稱
            {
                return Ok(new {
                    agents = new List<XFLdAgent>(),
                    warning = "資料表 XFLdAgent 不存在"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        // 新增代理人
        [HttpPost]
        public async Task<IActionResult> CreateAgent([FromBody] XFLdAgent agent)
        {
            if (string.IsNullOrWhiteSpace(agent.USERID))
            {
                return BadRequest(new { error = "USERID is required" });
            }

            // AGENTID 可以為空，允許新增後再填寫

            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    var query = @"
                        INSERT INTO XFLdAgent (USERID, AGENTID, SDATE, EDATE, AGENTPART)
                        VALUES (@UserId, @AgentId, @SDate, @EDate, @AgentPart);
                        SELECT CAST(SCOPE_IDENTITY() as int)";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", agent.USERID);
                        command.Parameters.AddWithValue("@AgentId", agent.AGENTID);
                        command.Parameters.AddWithValue("@SDate", (object?)agent.SDATE ?? DBNull.Value);
                        command.Parameters.AddWithValue("@EDate", (object?)agent.EDATE ?? DBNull.Value);
                        command.Parameters.AddWithValue("@AgentPart", (object?)agent.AGENTPART ?? DBNull.Value);

                        var newId = await command.ExecuteScalarAsync();
                        agent.iSeq = Convert.ToInt32(newId);
                    }
                }

                return Ok(agent);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        // 更新代理人
        [HttpPut("{seq}")]
        public async Task<IActionResult> UpdateAgent(int seq, [FromBody] XFLdAgent agent)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    var query = @"
                        UPDATE XFLdAgent
                        SET AGENTID = @AgentId,
                            SDATE = @SDate,
                            EDATE = @EDate,
                            AGENTPART = @AgentPart
                        WHERE iSeq = @Seq";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Seq", seq);
                        command.Parameters.AddWithValue("@AgentId", agent.AGENTID ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@SDate", (object?)agent.SDATE ?? DBNull.Value);
                        command.Parameters.AddWithValue("@EDate", (object?)agent.EDATE ?? DBNull.Value);
                        command.Parameters.AddWithValue("@AgentPart", (object?)agent.AGENTPART ?? DBNull.Value);

                        await command.ExecuteNonQueryAsync();
                    }
                }

                return Ok(agent);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        // 刪除代理人
        [HttpDelete("{seq}")]
        public async Task<IActionResult> DeleteAgent(int seq)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    var query = "DELETE FROM XFLdAgent WHERE iSeq = @Seq";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Seq", seq);
                        await command.ExecuteNonQueryAsync();
                    }
                }

                return Ok(new { message = "Agent deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }
    }
}
