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

        // 儲存排序設定
        [HttpPost("SaveSortSettings")]
        public async Task<IActionResult> SaveSortSettings([FromBody] SaveSortSettingsRequest request)
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

                    // 執行 XFLdSaveSort 預存程序
                    using (var command = new SqlCommand("XFLdSaveSort", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@USERID", request.UserId);
                        command.Parameters.AddWithValue("@SORT1", request.Sort1 ?? "");
                        command.Parameters.AddWithValue("@SORT2", request.Sort2 ?? "");
                        command.Parameters.AddWithValue("@SORT3", request.Sort3 ?? "");

                        await command.ExecuteNonQueryAsync();

                        return Ok(new { success = true, message = "儲存排序設定成功" });
                    }
                }
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

        // 刪除通知
        [HttpPost("DeleteNotify")]
        public async Task<IActionResult> DeleteNotify([FromBody] DeleteNotifyRequest request)
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

                    // 查詢當前 Notify 狀態
                    var checkSql = "SELECT ISNULL(Notify, 0) AS Notify FROM XFLdMAIL WITH (NOLOCK) WHERE SEQ = @MailSeq";
                    int notify = 0;

                    using (var checkCommand = new SqlCommand(checkSql, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@MailSeq", request.MailSeq);
                        var result = await checkCommand.ExecuteScalarAsync();
                        if (result != null && result != DBNull.Value)
                        {
                            notify = Convert.ToInt32(result);
                        }
                    }

                    // 如果 Notify 是 1 或 2，則可以刪除
                    if (notify == 1 || notify == 2)
                    {
                        var updateSql = "UPDATE XFLdMAIL SET Notify=0, IsDelNotify=1 WHERE SEQ=@MailSeq";
                        using (var updateCommand = new SqlCommand(updateSql, connection))
                        {
                            updateCommand.Parameters.AddWithValue("@MailSeq", request.MailSeq);
                            await updateCommand.ExecuteNonQueryAsync();
                        }

                        return Ok(new { success = true, message = "刪除通知成功" });
                    }
                    else
                    {
                        return BadRequest(new { success = false, message = "副本通知，請由詳細資料讀取後清除!" });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // 刪除已讀通知
        [HttpPost("DeleteReadNotifications")]
        public async Task<IActionResult> DeleteReadNotifications([FromBody] DeleteReadRequest request)
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

                    // 執行 XFLdUserMailChkRead 檢查是否有已讀通知
                    int chkRead = 0;
                    using (var checkCommand = new SqlCommand("XFLdUserMailChkRead", connection))
                    {
                        checkCommand.CommandType = CommandType.StoredProcedure;
                        checkCommand.Parameters.AddWithValue("@Receiver", request.UserId);

                        using (var reader = await checkCommand.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                chkRead = reader["ChkRead"] != DBNull.Value ? Convert.ToInt32(reader["ChkRead"]) : 0;
                            }
                        }
                    }

                    // 如果有已讀通知，則執行刪除
                    if (chkRead == 1)
                    {
                        using (var deleteCommand = new SqlCommand("XFLdUserMailDelRead", connection))
                        {
                            deleteCommand.CommandType = CommandType.StoredProcedure;
                            deleteCommand.Parameters.AddWithValue("@Receiver", request.UserId);
                            await deleteCommand.ExecuteNonQueryAsync();
                        }

                        return Ok(new { success = true, message = "刪除已讀通知成功" });
                    }
                    else
                    {
                        return BadRequest(new { success = false, message = "副本通知，目前沒有已讀通知!" });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // 標記訊息為已讀
        [HttpPost("MarkMessageRead")]
        public async Task<IActionResult> MarkMessageRead([FromBody] MarkMessageReadRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ToUserId) || request.SerialNum == 0)
            {
                return BadRequest(new { error = "toUserId and serialNum are required" });
            }

            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    var updateSql = @"UPDATE CURdMsg
                                     SET iStatus = 1
                                     WHERE ToUserId = @ToUserId
                                     AND SerialNum = @SerialNum
                                     AND Kind <> 4
                                     AND iStatus = 0";

                    using (var command = new SqlCommand(updateSql, connection))
                    {
                        command.Parameters.AddWithValue("@ToUserId", request.ToUserId);
                        command.Parameters.AddWithValue("@SerialNum", request.SerialNum);
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

        // 刪除已讀訊息
        [HttpPost("DeleteReadMessages")]
        public async Task<IActionResult> DeleteReadMessages([FromBody] DeleteMessagesRequest request)
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

                    using (var command = new SqlCommand("CURdMsgTextedDelete", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@UserId", request.UserId);
                        await command.ExecuteNonQueryAsync();
                    }

                    return Ok(new { success = true, message = "刪除已讀訊息成功" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // 刪除全部訊息（批次刪除，依日期範圍）
        [HttpPost("DeleteAllMessages")]
        public async Task<IActionResult> DeleteAllMessages([FromBody] DeleteMessagesRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                return BadRequest(new { error = "userId is required" });
            }

            if (!request.BDate.HasValue || !request.EDate.HasValue)
            {
                return BadRequest(new { error = "起始日期和結束日期為必填" });
            }

            if (request.BDate.Value > request.EDate.Value)
            {
                return BadRequest(new { error = "起始日期不可大於結束日期" });
            }

            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("CURdMsgAllDeleteDate", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@UserId", request.UserId);
                        command.Parameters.AddWithValue("@BDate", request.BDate.Value);
                        command.Parameters.AddWithValue("@EDate", request.EDate.Value);
                        await command.ExecuteNonQueryAsync();
                    }

                    return Ok(new { success = true, message = "批次刪除訊息成功" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // 初始化訊息發送（建立草稿）
        [HttpPost("MessageSendInit")]
        public async Task<IActionResult> MessageSendInit([FromBody] MessageSendInitRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                return BadRequest(new { error = "userId is required" });
            }

            var userId = request.UserId.Trim();
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    string? useId = null;
                    using (var cmdUse = new SqlCommand("SELECT UseId FROM CURdUsers WITH (NOLOCK) WHERE UserId = @UserId", connection))
                    {
                        cmdUse.Parameters.AddWithValue("@UserId", userId);
                        var raw = await cmdUse.ExecuteScalarAsync();
                        useId = raw?.ToString();
                    }

                    int serialNum = 0;
                    using (var cmdSeq = new SqlCommand("SELECT ISNULL(MAX(ISNULL(SerialNum,0)),0)+1 FROM CURdMsg WITH (NOLOCK) WHERE ToUserId = @UserId", connection))
                    {
                        cmdSeq.Parameters.AddWithValue("@UserId", userId);
                        var raw = await cmdSeq.ExecuteScalarAsync();
                        serialNum = Convert.ToInt32(raw ?? 1);
                    }

                    DateTime buildDate;
                    using (var cmdDate = new SqlCommand("SELECT GETDATE()", connection))
                    {
                        var raw = await cmdDate.ExecuteScalarAsync();
                        buildDate = raw != null ? Convert.ToDateTime(raw) : DateTime.Now;
                    }

                    using (var cmdIns = new SqlCommand(@"
                        INSERT INTO CURdMsg(ToUserId, SerialNum, FromUserId, Kind, BuildDate, UseId)
                        VALUES (@ToUserId, @SerialNum, @FromUserId, 4, @BuildDate, @UseId)", connection))
                    {
                        cmdIns.Parameters.AddWithValue("@ToUserId", userId);
                        cmdIns.Parameters.AddWithValue("@SerialNum", serialNum);
                        cmdIns.Parameters.AddWithValue("@FromUserId", userId);
                        cmdIns.Parameters.AddWithValue("@BuildDate", buildDate);
                        cmdIns.Parameters.AddWithValue("@UseId", (object?)useId ?? DBNull.Value);
                        await cmdIns.ExecuteNonQueryAsync();
                    }

                    return Ok(new { userId, serialNum });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // 取消訊息發送（清除草稿）
        [HttpPost("MessageSendCancel")]
        public async Task<IActionResult> MessageSendCancel([FromBody] MessageSendCancelRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.UserId) || request.SerialNum <= 0)
            {
                return BadRequest(new { error = "userId and serialNum are required" });
            }

            var userId = request.UserId.Trim();
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (var cmdDelTo = new SqlCommand(@"
                        DELETE FROM CURdMsgHandSendToUser
                        WHERE ToUserId = @UserId AND SerialNum = @SerialNum", connection))
                    {
                        cmdDelTo.Parameters.AddWithValue("@UserId", userId);
                        cmdDelTo.Parameters.AddWithValue("@SerialNum", request.SerialNum);
                        await cmdDelTo.ExecuteNonQueryAsync();
                    }

                    using (var cmdDelMsg = new SqlCommand(@"
                        DELETE FROM CURdMsg
                        WHERE ToUserId = @UserId
                          AND SerialNum = @SerialNum
                          AND ISNULL(iStatus,-1) = -1", connection))
                    {
                        cmdDelMsg.Parameters.AddWithValue("@UserId", userId);
                        cmdDelMsg.Parameters.AddWithValue("@SerialNum", request.SerialNum);
                        await cmdDelMsg.ExecuteNonQueryAsync();
                    }

                    return Ok(new { success = true });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // 傳送訊息
        [HttpPost("MessageSend")]
        public async Task<IActionResult> MessageSend([FromBody] MessageSendRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.UserId) || request.SerialNum <= 0)
            {
                return BadRequest(new { error = "userId and serialNum are required" });
            }

            var userId = request.UserId.Trim();
            var recipients = (request.Recipients ?? new List<string>())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (recipients.Count == 0)
            {
                return BadRequest(new { error = "recipients required" });
            }

            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using (var tx = connection.BeginTransaction())
                    {
                        try
                        {
                            using (var cmdUpd = new SqlCommand(@"
                                UPDATE CURdMsg
                                SET Subjects = @Subjects,
                                    MsgText = @MsgText
                                WHERE ToUserId = @UserId AND SerialNum = @SerialNum", connection, tx))
                            {
                                cmdUpd.Parameters.AddWithValue("@Subjects", request.Subject ?? "");
                                cmdUpd.Parameters.AddWithValue("@MsgText", request.Message ?? "");
                                cmdUpd.Parameters.AddWithValue("@UserId", userId);
                                cmdUpd.Parameters.AddWithValue("@SerialNum", request.SerialNum);
                                await cmdUpd.ExecuteNonQueryAsync();
                            }

                            using (var cmdDel = new SqlCommand(@"
                                DELETE FROM CURdMsgHandSendToUser
                                WHERE ToUserId = @UserId AND SerialNum = @SerialNum", connection, tx))
                            {
                                cmdDel.Parameters.AddWithValue("@UserId", userId);
                                cmdDel.Parameters.AddWithValue("@SerialNum", request.SerialNum);
                                await cmdDel.ExecuteNonQueryAsync();
                            }

                            foreach (var toUserId in recipients)
                            {
                                using (var cmdIns = new SqlCommand(@"
                                    INSERT INTO CURdMsgHandSendToUser(ToUserId, SerialNum, SendToUserId)
                                    VALUES (@UserId, @SerialNum, @SendToUserId)", connection, tx))
                                {
                                    cmdIns.Parameters.AddWithValue("@UserId", userId);
                                    cmdIns.Parameters.AddWithValue("@SerialNum", request.SerialNum);
                                    cmdIns.Parameters.AddWithValue("@SendToUserId", toUserId);
                                    await cmdIns.ExecuteNonQueryAsync();
                                }
                            }

                            using (var cmdProc = new SqlCommand("CURdMsgGeneralInsert", connection, tx))
                            {
                                cmdProc.CommandType = CommandType.StoredProcedure;
                                cmdProc.Parameters.AddWithValue("@ToUserId", userId);
                                cmdProc.Parameters.AddWithValue("@SerialNum", request.SerialNum);
                                await cmdProc.ExecuteNonQueryAsync();
                            }

                            tx.Commit();
                            return Ok(new { success = true });
                        }
                        catch
                        {
                            tx.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // 依 PaperId/PaperNum/UserId/UseId 解析對應作業代碼
        [HttpPost("ResolveItemIdByFromType")]
        public async Task<IActionResult> ResolveItemIdByFromType([FromBody] ResolveItemIdByFromTypeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.PaperId) || string.IsNullOrWhiteSpace(request.PaperNum))
            {
                return BadRequest(new { error = "paperId and paperNum are required" });
            }
            if (string.IsNullOrWhiteSpace(request.UserId) || string.IsNullOrWhiteSpace(request.UseId))
            {
                return BadRequest(new { error = "userId and useId are required" });
            }

            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("CURdOCXItemIdByFromType", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@PaperId", request.PaperId.Trim());
                        command.Parameters.AddWithValue("@PaperNum", request.PaperNum.Trim());
                        command.Parameters.AddWithValue("@UserId", request.UserId.Trim());
                        command.Parameters.AddWithValue("@UseId", request.UseId.Trim());

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var itemId = reader["ItemId"]?.ToString() ?? "";
                                return Ok(new { itemId });
                            }
                        }
                    }
                }

                return NotFound(new { error = "itemId not found" });
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

    public class DeleteNotifyRequest
    {
        public string MailSeq { get; set; } = "";
    }

    public class DeleteReadRequest
    {
        public string UserId { get; set; } = "";
    }

    public class MarkMessageReadRequest
    {
        public string ToUserId { get; set; } = "";
        public int SerialNum { get; set; }
    }

    public class DeleteMessagesRequest
    {
        public string UserId { get; set; } = "";
        public DateTime? BDate { get; set; }
        public DateTime? EDate { get; set; }
    }

    public class MessageSendInitRequest
    {
        public string UserId { get; set; } = "";
    }

    public class MessageSendCancelRequest
    {
        public string UserId { get; set; } = "";
        public int SerialNum { get; set; }
    }

    public class MessageSendRequest
    {
        public string UserId { get; set; } = "";
        public int SerialNum { get; set; }
        public string? Subject { get; set; }
        public string? Message { get; set; }
        public List<string>? Recipients { get; set; }
    }

    public class ResolveItemIdByFromTypeRequest
    {
        public string PaperId { get; set; } = "";
        public string PaperNum { get; set; } = "";
        public string UserId { get; set; } = "";
        public string UseId { get; set; } = "";
    }

    public class SaveSortSettingsRequest
    {
        public string UserId { get; set; } = "";
        public string? Sort1 { get; set; }
        public string? Sort2 { get; set; }
        public string? Sort3 { get; set; }
    }
}
