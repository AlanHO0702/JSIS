using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace PcbErpApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlowController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public FlowController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // 輔助方法：安全地讀取欄位值（嘗試多個可能的欄位名稱）
        private string? GetFieldValue(SqlDataReader reader, params string[] fieldNames)
        {
            foreach (var fieldName in fieldNames)
            {
                try
                {
                    var ordinal = reader.GetOrdinal(fieldName);
                    return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
                }
                catch
                {
                    continue;
                }
            }
            return null;
        }

        // 輔助方法：安全地讀取日期欄位
        private DateTime? GetDateValue(SqlDataReader reader, params string[] fieldNames)
        {
            foreach (var fieldName in fieldNames)
            {
                try
                {
                    var ordinal = reader.GetOrdinal(fieldName);
                    return reader.IsDBNull(ordinal) ? null : reader.GetDateTime(ordinal);
                }
                catch
                {
                    continue;
                }
            }
            return null;
        }

        // 取得審核歷史
        [HttpGet("ReviewHistory")]
        public async Task<IActionResult> GetReviewHistory([FromQuery] string mailSeq, [FromQuery] string? prcSeq)
        {
            if (string.IsNullOrWhiteSpace(mailSeq))
            {
                return BadRequest(new { error = "mailSeq is required" });
            }

            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            var historyList = new List<ReviewHistoryItem>();

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // 執行 XFLGetHistory2 預存程序
                    using (var command = new SqlCommand("XFLGetHistory2", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@MAILSEQ", mailSeq);

                        if (!string.IsNullOrWhiteSpace(prcSeq))
                        {
                            command.Parameters.AddWithValue("@PRCSEQ", prcSeq);
                        }

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var item = new ReviewHistoryItem
                                {
                                    FlowSeq = reader["FLOWSEQ"]?.ToString() ?? "",
                                    UserName = reader["USERNAME"]?.ToString() ?? "",
                                    ReviewDate = reader["REVIEWDATE"] != DBNull.Value
                                        ? Convert.ToDateTime(reader["REVIEWDATE"])
                                        : (DateTime?)null,
                                    Result = reader["RESULT"]?.ToString() ?? "",
                                    Comments = reader["COMMENTS"]?.ToString() ?? ""
                                };
                                historyList.Add(item);
                            }
                        }
                    }
                }

                return Ok(historyList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // 取得審核意見
        [HttpGet("ReviewComments")]
        public async Task<IActionResult> GetReviewComments([FromQuery] string? paperId, [FromQuery] string? paperNum)
        {
            if (string.IsNullOrWhiteSpace(paperId) || string.IsNullOrWhiteSpace(paperNum))
            {
                return Ok(new List<ReviewCommentItem>());
            }

            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            var commentsList = new List<ReviewCommentItem>();

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // 執行 XFLGetCommit2 預存程序
                    using (var command = new SqlCommand("XFLGetCommit2", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@PAPERID", paperId);
                        command.Parameters.AddWithValue("@PAPERNUM", paperNum);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var item = new ReviewCommentItem
                                {
                                    CommentType = reader["COMMENTTYPE"]?.ToString() ?? "",
                                    CommentContent = reader["COMMENTCONTENT"]?.ToString() ?? "",
                                    CreatedBy = reader["CREATEDBY"]?.ToString() ?? "",
                                    CreatedDate = reader["CREATEDDATE"] != DBNull.Value
                                        ? Convert.ToDateTime(reader["CREATEDDATE"])
                                        : (DateTime?)null
                                };
                                commentsList.Add(item);
                            }
                        }
                    }
                }

                return Ok(commentsList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // 取得意見彙總
        [HttpGet("OpinionSummary")]
        public async Task<IActionResult> GetOpinionSummary([FromQuery] string mailSeq)
        {
            if (string.IsNullOrWhiteSpace(mailSeq))
            {
                return BadRequest(new { error = "mailSeq is required" });
            }

            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // 從 XFLdeFlowInfo 表取得意見彙總 (Delphi: MEMO3)
                    var query = "SELECT MEMO3 FROM XFLdeFlowInfo WHERE MAILSEQ = @MAILSEQ";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@MAILSEQ", mailSeq);

                        var result = await command.ExecuteScalarAsync();
                        var summary = result?.ToString() ?? "";

                        return Ok(new { summary = summary });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // 取得意見和備註
        [HttpGet("CommentsAndNotes")]
        public async Task<IActionResult> GetCommentsAndNotes([FromQuery] string mailSeq)
        {
            if (string.IsNullOrWhiteSpace(mailSeq))
            {
                return BadRequest(new { error = "mailSeq is required" });
            }

            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // 從 XFLdeFlowInfo 表取得意見和備註
                    var query = "SELECT COMMENTS, NOTES FROM XFLdeFlowInfo WHERE MAILSEQ = @MAILSEQ";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@MAILSEQ", mailSeq);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var comments = reader["COMMENTS"]?.ToString() ?? "";
                                var notes = reader["NOTES"]?.ToString() ?? "";

                                return Ok(new { comments = comments, notes = notes });
                            }
                        }
                    }
                }

                return Ok(new { comments = "", notes = "" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // 儲存意見和備註
        [HttpPost("SaveCommentsAndNotes")]
        public async Task<IActionResult> SaveCommentsAndNotes([FromBody] SaveCommentsRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.MailSeq))
            {
                return BadRequest(new { error = "mailSeq is required" });
            }

            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // 更新 XFLdeFlowInfo 表的意見和備註
                    var query = @"UPDATE XFLdeFlowInfo
                                SET COMMENTS = @COMMENTS,
                                    NOTES = @NOTES
                                WHERE MAILSEQ = @MAILSEQ";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@MAILSEQ", request.MailSeq);
                        command.Parameters.AddWithValue("@COMMENTS", request.Comments ?? "");
                        command.Parameters.AddWithValue("@NOTES", request.Notes ?? "");

                        var rowsAffected = await command.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            return Ok(new { success = true, message = "儲存成功" });
                        }
                        else
                        {
                            return NotFound(new { error = "找不到對應的記錄" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // 取得完整審核歷史（使用 CURdPaperGetFlowHis）
        [HttpGet("PaperFlowHistory")]
        public async Task<IActionResult> GetPaperFlowHistory([FromQuery] string paperId, [FromQuery] string paperNum)
        {
            if (string.IsNullOrWhiteSpace(paperId) || string.IsNullOrWhiteSpace(paperNum))
            {
                return BadRequest(new { error = "paperId and paperNum are required" });
            }

            paperId = paperId.Trim();
            paperNum = paperNum.Trim();

            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            var historyRows = new List<Dictionary<string, object?>>();

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // 執行 CURdPaperGetFlowHis 預存程序
                    using (var command = new SqlCommand("CURdPaperGetFlowHis", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@PAPERID", paperId);
                        command.Parameters.AddWithValue("@PAPERNUM", paperNum);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    var name = reader.GetName(i);
                                    var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                                    row[name] = value;
                                }
                                historyRows.Add(row);
                            }
                        }
                    }
                }

                return Ok(historyRows);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // 取得完整審核意見（使用 CURdPaperGetFlowComt）
        [HttpGet("PaperFlowComments")]
        public async Task<IActionResult> GetPaperFlowComments([FromQuery] string paperId, [FromQuery] string paperNum)
        {
            if (string.IsNullOrWhiteSpace(paperId) || string.IsNullOrWhiteSpace(paperNum))
            {
                return BadRequest(new { error = "paperId and paperNum are required" });
            }

            paperId = paperId.Trim();
            paperNum = paperNum.Trim();

            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            var commentsRows = new List<Dictionary<string, object?>>();

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // 執行 CURdPaperGetFlowComt 預存程序
                    using (var command = new SqlCommand("CURdPaperGetFlowComt", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@PAPERID", paperId);
                        command.Parameters.AddWithValue("@PAPERNUM", paperNum);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    var name = reader.GetName(i);
                                    var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                                    row[name] = value;
                                }
                                commentsRows.Add(row);
                            }
                        }
                    }
                }

                return Ok(commentsRows);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // 儲存面板尺寸
        [HttpPost("SavePanelSize")]
        public async Task<IActionResult> SavePanelSize([FromBody] SavePanelSizeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                return BadRequest(new { error = "userId is required" });
            }

            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // 更新 CURdUsers 表的尺寸設定
                    var query = @"UPDATE CURdUsers
                                SET FlowHisCommtWidth = @FlowHisCommtWidth,
                                    FlowCommtWidth = @FlowCommtWidth,
                                    FlowDtlHeight = @FlowDtlHeight
                                WHERE USERID = @USERID";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@USERID", request.UserId);
                        command.Parameters.AddWithValue("@FlowHisCommtWidth", request.FlowHisCommtWidth);
                        command.Parameters.AddWithValue("@FlowCommtWidth", request.FlowCommtWidth);
                        command.Parameters.AddWithValue("@FlowDtlHeight", request.FlowDtlHeight);

                        var rowsAffected = await command.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            return Ok(new { success = true, message = "儲存成功" });
                        }
                        else
                        {
                            return NotFound(new { error = "找不到使用者記錄" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // 取得面板尺寸
        [HttpGet("GetPanelSize")]
        public async Task<IActionResult> GetPanelSize([FromQuery] string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest(new { error = "userId is required" });
            }

            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    var query = @"SELECT FlowHisCommtWidth, FlowCommtWidth, FlowDtlHeight
                                FROM CURdUsers
                                WHERE USERID = @USERID";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@USERID", userId);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return Ok(new
                                {
                                    flowHisCommtWidth = reader["FlowHisCommtWidth"] != DBNull.Value
                                        ? Convert.ToInt32(reader["FlowHisCommtWidth"])
                                        : 0,
                                    flowCommtWidth = reader["FlowCommtWidth"] != DBNull.Value
                                        ? Convert.ToInt32(reader["FlowCommtWidth"])
                                        : 0,
                                    flowDtlHeight = reader["FlowDtlHeight"] != DBNull.Value
                                        ? Convert.ToInt32(reader["FlowDtlHeight"])
                                        : 0
                                });
                            }
                        }
                    }
                }

                return Ok(new { flowHisCommtWidth = 0, flowCommtWidth = 0, flowDtlHeight = 0 });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // 取得排序設定
        [HttpGet("GetSortSettings")]
        public async Task<IActionResult> GetSortSettings([FromQuery] string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest(new { error = "userId is required" });
            }

            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // 從 XFLdeSortSetting 表取得排序設定
                    var query = @"SELECT SORT1, SORT2, SORT3
                                FROM XFLdeSortSetting
                                WHERE USERID = @USERID";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@USERID", userId);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return Ok(new
                                {
                                    sort1 = reader["SORT1"]?.ToString() ?? "",
                                    sort2 = reader["SORT2"]?.ToString() ?? "",
                                    sort3 = reader["SORT3"]?.ToString() ?? ""
                                });
                            }
                        }
                    }
                }

                // 如果沒有找到設定，返回空值
                return Ok(new { sort1 = "", sort2 = "", sort3 = "" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // 通知點擊後標記為已通知 (Notify=2)
        [HttpPost("MarkNotifyRead")]
        public async Task<IActionResult> MarkNotifyRead([FromBody] MarkNotifyReadRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.MailSeq))
            {
                return BadRequest(new { error = "mailSeq is required" });
            }

            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    var query = @"UPDATE XFLdMAIL
                                  SET Notify = 2
                                  WHERE SEQ = @MAILSEQ";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@MAILSEQ", request.MailSeq);
                        var rows = await command.ExecuteNonQueryAsync();
                        return Ok(new { success = rows > 0 });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // 審核歷史項目
    public class ReviewHistoryItem
    {
        public string FlowSeq { get; set; } = "";
        public string UserName { get; set; } = "";
        public DateTime? ReviewDate { get; set; }
        public string Result { get; set; } = "";
        public string Comments { get; set; } = "";
    }

    // 審核意見項目
    public class ReviewCommentItem
    {
        public string CommentType { get; set; } = "";
        public string CommentContent { get; set; } = "";
        public string CreatedBy { get; set; } = "";
        public DateTime? CreatedDate { get; set; }
    }

    // 儲存意見和備註請求
    public class SaveCommentsRequest
    {
        public string MailSeq { get; set; } = "";
        public string? Comments { get; set; }
        public string? Notes { get; set; }
    }

    // 儲存面板尺寸請求
    public class SavePanelSizeRequest
    {
        public string UserId { get; set; } = "";
        public int FlowHisCommtWidth { get; set; }
        public int FlowCommtWidth { get; set; }
        public int FlowDtlHeight { get; set; }
    }

    public class MarkNotifyReadRequest
    {
        public string MailSeq { get; set; } = "";
    }
}
