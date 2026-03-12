using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace PcbErpApi.Controllers
{
    /// <summary>
    /// AJN00060 - 日記帳 自訂按鈕 API 控制器
    /// 對應 Delphi: JourItemSelectDLL.pas / JourItemSelectDLL.dfm
    /// 功能: btnC3 常用傳票選取
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AJN00060Controller : ControllerBase
    {
        private readonly string _cs;

        public AJN00060Controller(IConfiguration cfg)
        {
            _cs = cfg.GetConnectionString("DefaultConnection")
                  ?? throw new InvalidOperationException("找不到連線字串");
        }

        /// <summary>
        /// 取得公司別下拉選單資料
        /// GET /api/AJN00060/GetAccUse
        /// </summary>
        [HttpGet("GetAccUse")]
        public async Task<IActionResult> GetAccUse()
        {
            await using var conn = new SqlConnection(_cs);
            await conn.OpenAsync();

            const string sql = "SELECT UseId, UseName FROM AJNdAccUse(NOLOCK)";
            await using var cmd = new SqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            var rows = new List<object>();
            while (await reader.ReadAsync())
            {
                rows.Add(new
                {
                    useId = reader["UseId"]?.ToString()?.Trim() ?? "",
                    useName = reader["UseName"]?.ToString()?.Trim() ?? ""
                });
            }

            return Ok(new { ok = true, rows });
        }

        /// <summary>
        /// 查詢常用傳票清單 (對應 Delphi: AJNdJourItemChoice SP)
        /// GET /api/AJN00060/GetJourItems?useId=xxx
        /// </summary>
        [HttpGet("GetJourItems")]
        public async Task<IActionResult> GetJourItems([FromQuery] string? useId)
        {
            await using var conn = new SqlConnection(_cs);
            await conn.OpenAsync();

            var cond = "";
            if (!string.IsNullOrWhiteSpace(useId))
                cond = $" and t1.UseId = '{useId.Replace("'", "''").Trim()}'";

            await using var cmd = new SqlCommand("AJNdJourItemChoice", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@sCond", cond);
            cmd.Parameters.AddWithValue("@Type", 0);

            await using var reader = await cmd.ExecuteReaderAsync();

            var rows = new List<object>();
            while (await reader.ReadAsync())
            {
                rows.Add(new
                {
                    cjourName = reader["CJourName"]?.ToString()?.Trim() ?? "",
                    isRemark = reader.IsDBNull(reader.GetOrdinal("IsRemark")) ? 0 : Convert.ToInt32(reader["IsRemark"]),
                    userId = reader["UserId"]?.ToString()?.Trim() ?? "",
                    useId = reader["UseId"]?.ToString()?.Trim() ?? "",
                    paperNum = reader["PaperNum"]?.ToString()?.Trim() ?? "",
                    paperDate = reader.IsDBNull(reader.GetOrdinal("PaperDate"))
                        ? (DateTime?)null
                        : reader.GetDateTime(reader.GetOrdinal("PaperDate")),
                    buildDate = reader.IsDBNull(reader.GetOrdinal("BuildDate"))
                        ? (DateTime?)null
                        : reader.GetDateTime(reader.GetOrdinal("BuildDate")),
                    notes = reader["Notes"]?.ToString()?.Trim() ?? ""
                });
            }

            return Ok(new { ok = true, rows });
        }

        /// <summary>
        /// 查詢常用傳票明細 (對應 Delphi: tblCJourDtl / AJNdCJourSub)
        /// GET /api/AJN00060/GetCJourSub?paperNum=xxx
        /// </summary>
        [HttpGet("GetCJourSub")]
        public async Task<IActionResult> GetCJourSub([FromQuery] string paperNum)
        {
            if (string.IsNullOrWhiteSpace(paperNum))
                return Ok(new { ok = true, rows = new List<object>() });

            await using var conn = new SqlConnection(_cs);
            await conn.OpenAsync();

            const string sql = @"
                SELECT s.Item, s.IsD, s.AccId,
                       a.AccIdName,
                       s.SubAccId, s.Amount,
                       s.DepartId,
                       d.DepartName,
                       s.ProjectId, s.Notes
                FROM AJNdCJourSub s WITH (NOLOCK)
                LEFT JOIN AJNdAccId a WITH (NOLOCK) ON a.AccId = s.AccId
                LEFT JOIN AJNdDepart d WITH (NOLOCK) ON d.DepartId = s.DepartId
                WHERE s.PaperNum = @PaperNum
                ORDER BY s.DepartId, s.Item";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@PaperNum", paperNum.Trim());

            await using var reader = await cmd.ExecuteReaderAsync();

            var rows = new List<object>();
            while (await reader.ReadAsync())
            {
                rows.Add(new
                {
                    item = reader.IsDBNull(reader.GetOrdinal("Item")) ? 0 : Convert.ToInt32(reader["Item"]),
                    isD = reader.IsDBNull(reader.GetOrdinal("IsD")) ? 0 : Convert.ToInt32(reader["IsD"]),
                    accId = reader["AccId"]?.ToString()?.Trim() ?? "",
                    accIdName = reader["AccIdName"]?.ToString()?.Trim() ?? "",
                    subAccId = reader["SubAccId"]?.ToString()?.Trim() ?? "",
                    amount = reader.IsDBNull(reader.GetOrdinal("Amount")) ? 0m : Convert.ToDecimal(reader["Amount"]),
                    departId = reader["DepartId"]?.ToString()?.Trim() ?? "",
                    departName = reader["DepartName"]?.ToString()?.Trim() ?? "",
                    projectId = reader["ProjectId"]?.ToString()?.Trim() ?? "",
                    notes = reader["Notes"]?.ToString()?.Trim() ?? ""
                });
            }

            return Ok(new { ok = true, rows });
        }

        public class InsertJourSubRequest
        {
            public string PaperNum { get; set; } = "";
            public string SourNum { get; set; } = "";
        }

        /// <summary>
        /// 執行常用傳票套用 (對應 Delphi: qryOcxAdd → AJNdJourSubInsert SP)
        /// POST /api/AJN00060/InsertJourSub
        /// </summary>
        [HttpPost("InsertJourSub")]
        public async Task<IActionResult> InsertJourSub([FromBody] InsertJourSubRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.PaperNum))
                return BadRequest(new { ok = false, error = "PaperNum 為必填" });
            if (string.IsNullOrWhiteSpace(req.SourNum))
                return BadRequest(new { ok = false, error = "SourNum 為必填" });

            await using var conn = new SqlConnection(_cs);
            await conn.OpenAsync();

            try
            {
                await using var cmd = new SqlCommand("AJNdJourSubInsert", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@PaperNum", req.PaperNum.Trim());
                cmd.Parameters.AddWithValue("@SourNum", req.SourNum.Trim());

                await cmd.ExecuteNonQueryAsync();

                return Ok(new { ok = true, message = "套用成功" });
            }
            catch (Exception ex)
            {
                return Ok(new { ok = false, error = ex.Message });
            }
        }
    }
}