using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;

namespace PcbErpApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SpecialUIController : ControllerBase
    {
        private readonly PcbErpContext _context;

        public SpecialUIController(PcbErpContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 計算收付傳票的借貸總額
        /// </summary>
        /// <param name="paperNum">單號</param>
        /// <param name="tableName">單身表名稱</param>
        /// <returns>借方金額和貸方金額</returns>
        [HttpPost("GetPayRecvAmount")]
        public async Task<IActionResult> GetPayRecvAmount([FromForm] string paperNum, [FromForm] string tableName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(paperNum))
                {
                    return BadRequest(new { error = "單號不可為空" });
                }

                if (string.IsNullOrWhiteSpace(tableName))
                {
                    return BadRequest(new { error = "表格名稱不可為空" });
                }

                // 取得實際表名稱（從字典表名稱轉換）
                var realTableName = await GetRealTableNameAsync(tableName);

                // 建立 SQL 查詢
                var sql = $@"
                    SELECT
                        AmountD = SUM(ISNULL(IsD, 0) * ISNULL(Amount, 0)),
                        AmountC = SUM((1 - ISNULL(IsD, 0)) * ISNULL(Amount, 0))
                    FROM {realTableName} WITH (NOLOCK)
                    WHERE PaperNum = @PaperNum";

                var connectionString = _context.Database.GetConnectionString();

                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@PaperNum", paperNum);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var amountD = reader["AmountD"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["AmountD"]);
                                var amountC = reader["AmountC"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["AmountC"]);

                                return Ok(new { amountD, amountC });
                            }
                        }
                    }
                }

                // 如果沒有資料，返回 0
                return Ok(new { amountD = 0m, amountC = 0m });
            }
            catch (SqlException sqlEx)
            {
                return StatusCode(500, new { error = "資料庫查詢錯誤", detail = sqlEx.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "計算借貸總額時發生錯誤", detail = ex.Message });
            }
        }

        /// <summary>
        /// 從字典表名稱取得實際表名稱
        /// </summary>
        private async Task<string> GetRealTableNameAsync(string dictTableName)
        {
            var sql = @"
                SELECT TOP 1 ISNULL(NULLIF(RealTableName, ''), TableName) AS ActualName
                FROM CURdTableName WITH (NOLOCK)
                WHERE TableName = @TableName";

            var connectionString = _context.Database.GetConnectionString();

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@TableName", dictTableName);

                    var result = await command.ExecuteScalarAsync();

                    if (result != null && result != DBNull.Value)
                    {
                        return result.ToString() ?? dictTableName;
                    }
                }
            }

            return dictTableName;
        }
    }
}