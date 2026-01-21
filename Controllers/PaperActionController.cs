#nullable enable
using Microsoft.AspNetCore.Mvc;
using PcbErpApi.Data;
using PcbErpApi.Models;
using PcbErpApi.Helpers;
using static PcbErpApi.Helpers.DynamicQueryHelper;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Globalization;
using Microsoft.Data.SqlClient;
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
            public string? ItemId { get; set; }
            public string? UseId { get; set; }
            public string? CurrTypeHead { get; set; }
            public string? VoidReason { get; set; }
            public string? GlobalId { get; set; }
            public bool? AcceptPaperMsg { get; set; }
            public bool? AcceptExamMsg { get; set; }
            public bool? AcceptBeforeSql { get; set; }
            public bool? AutoExam { get; set; }
            public bool? AcceptRejMsg { get; set; }
            public string? RejectNotes { get; set; }
        }

        [HttpPost("DoAction")]
        public async Task<IActionResult> DoAction([FromBody] DoActionRequest req)
        {
            if (string.IsNullOrEmpty(req.PaperId) || string.IsNullOrEmpty(req.PaperNum))
                return BadRequest("缺少必要參數");

            var connStr = _config.GetConnectionString("DefaultConnection");
            using var conn = new SqlConnection(connStr);

            try
            {
                await conn.OpenAsync();
                var dictPaperId = req.PaperId.Trim();

                // 從 CURdTableName 取得實際的資料表名稱
                var safePaperId = await ResolveRealTableNameAsync(conn, dictPaperId);
                if (!IsSafeIdentifier(safePaperId))
                    return BadRequest("PaperId 不合法");

                if (!await TableExistsAsync(conn, safePaperId))
                    return BadRequest(new { ok = false, error = $"找不到 Table {safePaperId} (嘗試對應到 DbSet: {dictPaperId.ToLowerInvariant().Replace("_", "")})" });

                var paperNum = req.PaperNum.Trim();
                var userId = string.IsNullOrWhiteSpace(req.UserId) ? "admin" : req.UserId.Trim();
                var useId = string.IsNullOrWhiteSpace(req.UseId) ? "A001" : req.UseId.Trim();

                var columns = await GetTableColumnsAsync(conn, safePaperId);
                if (!columns.Contains("PaperNum"))
                    return BadRequest("單據資料表缺少 PaperNum 欄位");

                var row = await LoadPaperRowAsync(conn, safePaperId, paperNum, columns);
                if (row == null)
                    return BadRequest("找不到指定單據");

                var finished = TryParseInt(row, "Finished");
                var rowPaperId = TryGetString(row, "PaperId");
                var rowUserId = TryGetString(row, "UserId");
                var rowPaperDate = TryGetDateString(row, "PaperDate");
                var rowHeadFirst = TryGetString(row, "dllHeadFirst");

                var (canbRunFlow, canbSelectType, canbLockPaperDate, canbLockUserEdit, canbMustNotes, headFirst) =
                    await LoadPaperInfoAsync(conn, dictPaperId, safePaperId);
                var (canbScrap, canbUpdate, canbAudit, canbAuditBack, canbUpdateNotes, canbUpdateMoney, canbViewMoney, canbPrint) =
                    await LoadUserItemPowerAsync(conn, req.ItemId, userId, useId);

                if (req.AftFinished == 1)
                {
                    return await HandleConfirmAsync(
                        conn,
                        dictPaperId,
                        safePaperId,
                        paperNum,
                        userId,
                        useId,
                        req,
                        row,
                        columns,
                        finished,
                        rowUserId,
                        canbRunFlow,
                        canbLockUserEdit,
                        canbAudit,
                        rowPaperDate,
                        TryParseInt(row, "FlowStatus") ?? 0
                    );
                }

                if (req.AftFinished == 3)
                {
                    return await HandleRejectExamAsync(
                        conn,
                        dictPaperId,
                        safePaperId,
                        paperNum,
                        userId,
                        useId,
                        req,
                        row,
                        columns,
                        finished,
                        rowPaperId,
                        canbRunFlow,
                        canbAuditBack,
                        canbMustNotes
                    );
                }

                if (req.AftFinished == 2)
                {
                    if (finished == 2)
                        return BadRequest(new { message = "此單據「已作廢」,不須作廢" });
                    if (finished == 4)
                        return BadRequest(new { message = "此單據「已結案」,不可作廢" });

                    if (canbRunFlow == 1 && finished == 1)
                    {
                        var canVoid = await CheckCanVoidAsync(conn, safePaperId);  // SP 需要真實表名稱
                        if (canVoid == 0)
                            return BadRequest(new { message = "此單據「已完成」,不可作廢" });
                    }

                    if (canbRunFlow == 1 && canbScrap == 0)
                        return BadRequest(new { message = "您沒有「作廢」的權限" });

                    if (!string.IsNullOrWhiteSpace(rowPaperId))
                        return BadRequest(new { message = "拋轉單據，請由原單據作廢" });

                    if (canbLockUserEdit == 1 && !string.IsNullOrWhiteSpace(rowUserId))
                    {
                        if (!string.Equals(userId, rowUserId, StringComparison.OrdinalIgnoreCase)
                            && !string.Equals(userId, "admin", StringComparison.OrdinalIgnoreCase))
                        {
                            return BadRequest(new { message = "此單據已被設定「只有建檔者可編輯及作廢」" });
                        }
                    }

                    var currTypeHead = (rowHeadFirst ?? req.CurrTypeHead ?? headFirst ?? string.Empty).Trim();
                    var canDeleteMaxNum = false;
                    if (finished == 0)
                    {
                        var maxNum = await GetMaxPaperNumAsync(conn, safePaperId, useId, currTypeHead, rowPaperDate);
                        if (!string.IsNullOrWhiteSpace(maxNum)
                            && string.Equals(maxNum, paperNum, StringComparison.OrdinalIgnoreCase))
                        {
                            canDeleteMaxNum = true;
                        }
                    }

                    if (canDeleteMaxNum)
                    {
                        await ExecPaperDeleteMaxNumAsync(conn, safePaperId, paperNum);
                        await DeletePaperRowAsync(conn, safePaperId, paperNum);
                        return Ok(new { message = "作廢成功！", deleted = true });
                    }

                    if (canbMustNotes == 1)
                    {
                        var reason = (req.VoidReason ?? string.Empty).Trim();
                        if (string.IsNullOrWhiteSpace(reason))
                            return BadRequest(new { code = "NEED_VOID_REASON", message = "請輸入作廢原因" });

                        if (columns.Contains("Notes"))
                            await AppendNotesAsync(conn, safePaperId, paperNum, reason);
                    }
                }

                using var cmd = new SqlCommand("CURdPaperAction", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@PaperId", safePaperId);  // SP 需要真實表名稱
                cmd.Parameters.AddWithValue("@PaperNum", paperNum);
                cmd.Parameters.AddWithValue("@UserId", userId); // 預設值
                cmd.Parameters.AddWithValue("@EOC", req.EOC);
                cmd.Parameters.AddWithValue("@AftFinished", req.AftFinished);

                await cmd.ExecuteNonQueryAsync();
                return Ok(new { message = "已成功執行 CURdPaperAction" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { ok = false, message = ex.Message, error = ex.Message });
            }
        }

        private static bool IsSafeIdentifier(string name)
            => Regex.IsMatch(name ?? "", @"^[A-Za-z0-9_]+$");

        private static async Task<bool> TableExistsAsync(SqlConnection conn, string table)
        {
            const string sql = "SELECT 1 FROM sys.objects WHERE name = @t AND type = 'U'";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@t", table);
            var obj = await cmd.ExecuteScalarAsync();
            return obj != null && obj != DBNull.Value;
        }

        private static async Task<HashSet<string>> GetTableColumnsAsync(SqlConnection conn, string table)
        {
            const string sql = @"
SELECT c.name
  FROM sys.columns c
  JOIN sys.objects o ON o.object_id = c.object_id
 WHERE o.type = 'U' AND o.name = @t;";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@t", table);
            using var rd = await cmd.ExecuteReaderAsync();
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            while (await rd.ReadAsync())
                set.Add(rd.GetString(0));
            return set;
        }

        private static async Task<Dictionary<string, object?>?> LoadPaperRowAsync(
            SqlConnection conn,
            string table,
            string paperNum,
            HashSet<string> columns)
        {
            var selectCols = new List<string>();
            void Add(string col)
            {
                if (columns.Contains(col)) selectCols.Add($"[{col}]");
            }

            Add("Finished");
            Add("PaperId");
            Add("UserId");
            Add("PaperDate");
            Add("dllHeadFirst");
            Add("Notes");
            Add("FlowStatus");

            if (selectCols.Count == 0) return null;

            var sql = $"SELECT TOP 1 {string.Join(", ", selectCols)} FROM [{table}] WITH (NOLOCK) WHERE [PaperNum] = @PaperNum";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@PaperNum", paperNum);
            using var rd = await cmd.ExecuteReaderAsync();
            if (!await rd.ReadAsync()) return null;

            var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < rd.FieldCount; i++)
            {
                var name = rd.GetName(i);
                row[name] = rd.IsDBNull(i) ? null : rd.GetValue(i);
            }
            return row;
        }

        private static async Task<(int canbRunFlow, int canbSelectType, int canbLockPaperDate, int canbLockUserEdit, int canbMustNotes, string? headFirst)>
            LoadPaperInfoAsync(SqlConnection conn, string dictPaperId, string realPaperId)
        {
            try
            {
                var infoCols = await GetTableColumnsAsync(conn, "CURdPaperInfo");
                if (infoCols.Count == 0)
                    return (0, 0, 0, 0, 0, null);

                var selectCols = new List<string>();
                void Add(string col)
                {
                    if (infoCols.Contains(col)) selectCols.Add($"[{col}]");
                }

                Add("RunFlow");
                Add("SelectType");
                Add("LockPaperDate");
                Add("LockUserEdit");
                Add("MustNotes");
                Add("HeadFirst");

                if (selectCols.Count == 0)
                    return (0, 0, 0, 0, 0, null);

                // CURdPaperInfo 中的 PaperId 可能是字典表名稱或真實表名稱，兩者都嘗試
                var sql = $@"
SELECT TOP 1 {string.Join(", ", selectCols)}
  FROM CURdPaperInfo WITH (NOLOCK)
 WHERE LOWER(PaperId) = LOWER(@dictPaperId) OR LOWER(PaperId) = LOWER(@realPaperId);";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@dictPaperId", dictPaperId);
                cmd.Parameters.AddWithValue("@realPaperId", realPaperId);
                using var rd = await cmd.ExecuteReaderAsync();
                if (!await rd.ReadAsync())
                    return (0, 0, 0, 0, 0, null);

                var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                for (var i = 0; i < rd.FieldCount; i++)
                {
                    var name = rd.GetName(i);
                    row[name] = rd.IsDBNull(i) ? null : rd.GetValue(i);
                }

                return (
                    TryParseInt(row, "RunFlow") ?? 0,
                    TryParseInt(row, "SelectType") ?? 0,
                    TryParseInt(row, "LockPaperDate") ?? 0,
                    TryParseInt(row, "LockUserEdit") ?? 0,
                    TryParseInt(row, "MustNotes") ?? 0,
                    TryGetString(row, "HeadFirst")
                );
            }
            catch (SqlException)
            {
                return (0, 0, 0, 0, 0, null);
            }
        }

        private static async Task<(int canbScrap, int canbUpdate, int canbAudit, int canbAuditBack, int canbUpdateNotes, int canbUpdateMoney, int canbViewMoney, int canbPrint)>
            LoadUserItemPowerAsync(SqlConnection conn, string? itemId, string userId, string useId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
                return (1, 1, 1, 1, 1, 1, 1, 1);

            const string sql = "exec CURdGetUserSysItems @Blank1, @UserId, @Blank2, @UseId, @Zero, @ItemId";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Blank1", "");
            cmd.Parameters.AddWithValue("@UserId", userId ?? "");
            cmd.Parameters.AddWithValue("@Blank2", "");
            cmd.Parameters.AddWithValue("@UseId", useId ?? "");
            cmd.Parameters.AddWithValue("@Zero", 0);
            cmd.Parameters.AddWithValue("@ItemId", itemId ?? "");

            using var rd = await cmd.ExecuteReaderAsync();
            if (!await rd.ReadAsync())
                return (1, 1, 1, 1, 1, 1, 1, 1);

            var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < rd.FieldCount; i++)
            {
                var name = rd.GetName(i);
                row[name] = rd.IsDBNull(i) ? null : rd.GetValue(i);
            }

            return (
                TryParseInt(row, "bScrap") ?? 0,
                TryParseInt(row, "bUpdate") ?? 0,
                TryParseInt(row, "bAudit") ?? 0,
                TryParseInt(row, "bAuditBack") ?? 0,
                TryParseInt(row, "bUpdateNotes") ?? 0,
                TryParseInt(row, "bUpdateMoney") ?? 0,
                TryParseInt(row, "bViewMoney") ?? 0,
                TryParseInt(row, "bPrint") ?? 0
            );
        }

        private static async Task<int> CheckCanVoidAsync(SqlConnection conn, string paperId)
        {
            const string sql = "exec CURdCanVoidPaper @PaperId";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@PaperId", paperId);
            using var rd = await cmd.ExecuteReaderAsync();
            if (await rd.ReadAsync()) return 1;
            return 0;
        }

        private static async Task<string?> GetMaxPaperNumAsync(
            SqlConnection conn,
            string paperId,
            string useId,
            string currTypeHead,
            string? paperDate)
        {
            var currUseHead = string.IsNullOrWhiteSpace(useId) ? "" : useId.Substring(0, 1);
            const string sql = @"
exec CURdGetMaxPaperNum @PaperId, @Blank, @CurrUseHead, @PaperDate, @CurrTypeHead, @UseId";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@PaperId", paperId);
            cmd.Parameters.AddWithValue("@Blank", "");
            cmd.Parameters.AddWithValue("@CurrUseHead", currUseHead);
            cmd.Parameters.AddWithValue("@PaperDate", paperDate ?? "");
            cmd.Parameters.AddWithValue("@CurrTypeHead", currTypeHead ?? "");
            cmd.Parameters.AddWithValue("@UseId", useId ?? "");
            var obj = await cmd.ExecuteScalarAsync();
            return obj == null || obj == DBNull.Value ? null : obj.ToString();
        }

        private static async Task ExecPaperDeleteMaxNumAsync(SqlConnection conn, string paperId, string paperNum)
        {
            const string sql = "exec CURdPaperDeleteMaxNum @PaperId, @PaperNum";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@PaperId", paperId);
            cmd.Parameters.AddWithValue("@PaperNum", paperNum);
            await cmd.ExecuteNonQueryAsync();
        }

        private static async Task DeletePaperRowAsync(SqlConnection conn, string table, string paperNum)
        {
            var sql = $"DELETE FROM [{table}] WHERE [PaperNum] = @PaperNum";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@PaperNum", paperNum);
            await cmd.ExecuteNonQueryAsync();
        }

        private static async Task AppendNotesAsync(SqlConnection conn, string table, string paperNum, string reason)
        {
            var sql = $@"
UPDATE [{table}]
   SET Notes = RTRIM(ISNULL(Notes, '')) + @Reason
 WHERE [PaperNum] = @PaperNum";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Reason", reason);
            cmd.Parameters.AddWithValue("@PaperNum", paperNum);
            await cmd.ExecuteNonQueryAsync();
        }

        private static int? TryParseInt(Dictionary<string, object?> row, string key)
        {
            if (!row.TryGetValue(key, out var obj) || obj == null) return null;
            if (obj is int i) return i;
            return int.TryParse(obj.ToString(), out var v) ? v : null;
        }

        private static string? TryGetString(Dictionary<string, object?> row, string key)
        {
            if (!row.TryGetValue(key, out var obj) || obj == null) return null;
            var s = obj.ToString();
            return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
        }

        private static string? TryGetDateString(Dictionary<string, object?> row, string key)
        {
            if (!row.TryGetValue(key, out var obj) || obj == null) return null;
            if (obj is DateTime dt) return dt.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
            if (DateTime.TryParse(obj.ToString(), out var parsed))
                return parsed.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
            return null;
        }

        private sealed class ConfirmResult
        {
            public bool Ok { get; set; }
            public bool NeedConfirm { get; set; }
            public string? ConfirmType { get; set; }
            public string? ConfirmMessage { get; set; }
            public string? Message { get; set; }
            public bool TransferPrompt { get; set; }
            public string? TransferMessage { get; set; }
        }

        private sealed class RejectExamResult
        {
            public bool Ok { get; set; }
            public bool NeedConfirm { get; set; }
            public string? ConfirmType { get; set; }
            public string? ConfirmMessage { get; set; }
            public bool RejectRequiresNotes { get; set; }
            public string? Message { get; set; }
        }

        private async Task<IActionResult> HandleConfirmAsync(
            SqlConnection conn,
            string dictPaperId,
            string safePaperId,
            string paperNum,
            string userId,
            string useId,
            DoActionRequest req,
            Dictionary<string, object?> row,
            HashSet<string> columns,
            int? finished,
            string? rowUserId,
            int canbRunFlow,
            int canbLockUserEdit,
            int canbAudit,
            string? rowPaperDate,
            int flowStatus)
        {
            var result = new ConfirmResult();

            var msgCompleted = await GetPaperMsgAsync(conn, req.ItemId, new[] { "Completed", "Confirm", "確認" }, 0);
            if (!string.IsNullOrWhiteSpace(msgCompleted) && req.AcceptPaperMsg != true)
            {
                result.NeedConfirm = true;
                result.ConfirmType = "paperMsg";
                result.ConfirmMessage = msgCompleted;
                return Ok(result);
            }

            if (finished == 1)
                return BadRequest(new { message = "此單據「已完成」,不須重複操作" });
            if (finished == 2)
                return BadRequest(new { message = "此單據「已作廢」,不可完成" });
            if (finished == 4)
                return BadRequest(new { message = "此單據「已結案」,不可完成" });

            if (canbLockUserEdit == 1 && !string.IsNullOrWhiteSpace(rowUserId))
            {
                if (!string.Equals(userId, rowUserId, StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(userId, "admin", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new { message = "此單據已被設定「只有建檔者可編輯及完成」" });
                }
            }

            var required = await GetRequiredFieldsAsync(conn, req.PaperId);
            foreach (var f in required)
            {
                if (!columns.Contains(f.FieldName)) continue;
                if (!row.TryGetValue(f.FieldName, out var raw) || raw == null)
                {
                    return BadRequest(new { message = $"主檔必須輸入「{f.DisplayLabel}」" });
                }
                if (IsStringType(f.DataType) && string.IsNullOrWhiteSpace(raw.ToString()))
                {
                    return BadRequest(new { message = $"主檔必須輸入「{f.DisplayLabel}」" });
                }
            }

            var (paperExamBeforeSql, paperCallPaperAftExam) = await LoadPaperExamConfigAsync(conn, req.PaperId);
            if (!string.IsNullOrWhiteSpace(paperExamBeforeSql))
            {
                var openCheck = paperExamBeforeSql.Contains('^');
                var sql = paperExamBeforeSql.Replace("^", "");
                sql = ReplaceSqlTokens(sql, row);
                sql = ReplaceSqlToken(sql, "PaperNum", paperNum);

                if (openCheck)
                {
                    var msg = await RunQueryFirstStringAsync(conn, sql);
                    if (!string.IsNullOrWhiteSpace(msg) && req.AcceptBeforeSql != true)
                    {
                        result.NeedConfirm = true;
                        result.ConfirmType = "beforeSql";
                        result.ConfirmMessage = msg;
                        return Ok(result);
                    }
                }
                else
                {
                    await ExecSqlAsync(conn, sql);
                }
            }

            var mergeToConfirm = string.Equals(
                await GetSysParamAsync(conn, "CUR", "PaperExamMergeToConfirm"),
                "1",
                StringComparison.OrdinalIgnoreCase);

            var systemId = await GetSystemIdAsync(conn, req.ItemId);
            var flowPrcId = await GetFlowPrcIdAsync(conn, req.ItemId);

            var sFlag = "";
            if (!string.IsNullOrWhiteSpace(systemId))
            {
                var idx = systemId.IndexOf('^');
                if (idx >= 0 && idx + 1 < systemId.Length)
                {
                    sFlag = systemId.Substring(idx + 1, 1);
                    systemId = systemId.Substring(0, idx);
                }
            }

            if (canbRunFlow == 0 && string.IsNullOrWhiteSpace(sFlag))
            {
                return await RunPaperExamAsync(conn, dictPaperId, safePaperId, paperNum, userId, req.ItemId, canbRunFlow, canbAudit, paperCallPaperAftExam, req.GlobalId, useId);
            }

            var bUseFlow = canbRunFlow == 1 && !string.IsNullOrWhiteSpace(flowPrcId);
            var flowResult = "NOFLOW";
            if (bUseFlow)
            {
                flowResult = await TryRunFlowAsync(conn, req.ItemId, safePaperId, paperNum, userId, useId, systemId, flowStatus);
            }

            if (string.Equals(flowResult, "NOFLOW", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(sFlag))
                {
                    if (canbRunFlow == 1 && canbAudit == 1)
                    {
                        var msgExam = await GetPaperMsgAsync(conn, req.ItemId, new[] { "Exam", "審核" }, 0);
                        if (!string.IsNullOrWhiteSpace(msgExam))
                        {
                            if (mergeToConfirm)
                            {
                                return await RunPaperExamAsync(conn, dictPaperId, safePaperId, paperNum, userId, req.ItemId, canbRunFlow, canbAudit, paperCallPaperAftExam, req.GlobalId, useId);
                            }

                            if (req.AutoExam != true && req.AutoExam != false)
                            {
                                result.NeedConfirm = true;
                                result.ConfirmType = "autoExam";
                                result.ConfirmMessage = "您有審核權限，是否要直接核准此單據？";
                                return Ok(result);
                            }
                            if (req.AutoExam == true)
                            {
                                return await RunPaperExamAsync(conn, dictPaperId, safePaperId, paperNum, userId, req.ItemId, canbRunFlow, canbAudit, paperCallPaperAftExam, req.GlobalId, useId);
                            }
                        }
                        else
                        {
                            return await RunPaperExamAsync(conn, dictPaperId, safePaperId, paperNum, userId, req.ItemId, canbRunFlow, canbAudit, paperCallPaperAftExam, req.GlobalId, useId);
                        }
                    }
                }
                else if (sFlag == "1" && canbAudit == 1)
                {
                    return await RunPaperExamAsync(conn, dictPaperId, safePaperId, paperNum, userId, req.ItemId, canbRunFlow, canbAudit, paperCallPaperAftExam, req.GlobalId, useId);
                }

                return await RunSendForAuditAsync(conn, dictPaperId, safePaperId, paperNum, userId, paperCallPaperAftExam, req.GlobalId, useId);
            }

            if (string.Equals(flowResult, "INTOFLOW", StringComparison.OrdinalIgnoreCase))
            {
                result.Ok = true;
                result.Message = "已進入電子簽核流程";
                return Ok(result);
            }

            if (string.IsNullOrWhiteSpace(flowResult))
                return BadRequest(new { message = "發生錯誤" });

            if (flowResult.StartsWith("OK", StringComparison.OrdinalIgnoreCase))
            {
                result.Ok = true;
                result.Message = flowResult;
                return Ok(result);
            }

            return BadRequest(new { message = flowResult });
        }

        private async Task<IActionResult> HandleRejectExamAsync(
            SqlConnection conn,
            string dictPaperId,
            string safePaperId,
            string paperNum,
            string userId,
            string useId,
            DoActionRequest req,
            Dictionary<string, object?> row,
            HashSet<string> columns,
            int? finished,
            string? rowPaperId,
            int canbRunFlow,
            int canbAuditBack,
            int canbMustNotes)
        {
            var result = new RejectExamResult();

            var msgBefore = await GetPaperMsgAsync(conn, req.ItemId, new[] { "RejExam", "退審" }, 0);
            if (!string.IsNullOrWhiteSpace(msgBefore) && req.AcceptRejMsg != true)
            {
                result.NeedConfirm = true;
                result.ConfirmType = "rejExamMsg";
                result.ConfirmMessage = msgBefore;
                return Ok(result);
            }

            if (finished == 0)
                return BadRequest(new { message = "此單據「作業中」,不須退審" });
            if (finished == 2)
                return BadRequest(new { message = "此單據「已作廢」,不可退審" });
            if (finished == 4)
                return BadRequest(new { message = "此單據「已結案」,不可退審" });

            if (canbRunFlow == 1 && canbAuditBack == 0)
                return BadRequest(new { message = "您沒有「退審」的權限" });

            if (!string.IsNullOrWhiteSpace(rowPaperId))
            {
                if (!string.Equals(dictPaperId, "MPHdExtendMain", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(dictPaperId, "MPHdPettyMain", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new { message = "此單據是其它單據拋轉而來，請由原單據修改" });
                }
            }

            var reason = (req.RejectNotes ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(reason))
                reason = reason.Replace("'", "");

            if (finished == 3)
            {
                if (string.Equals(dictPaperId, "fmedpassmain", StringComparison.OrdinalIgnoreCase))
                    return BadRequest(new { message = "此單據「審核中」,不可退審" });

                if (canbMustNotes == 1 && string.IsNullOrWhiteSpace(reason))
                {
                    result.NeedConfirm = true;
                    result.ConfirmType = "rejExamNotes";
                    result.ConfirmMessage = "請輸入退審原因";
                    result.RejectRequiresNotes = true;
                    return Ok(result);
                }

                var sql = "exec CURdPaperDoNewStatus @PaperId,@PaperNum,@UserId,0,3,@Reason";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@PaperId", safePaperId);  // SP 需要真實表名稱
                    cmd.Parameters.AddWithValue("@PaperNum", paperNum);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@Reason", reason ?? "");
                    using var rd = await cmd.ExecuteReaderAsync();
                    var msg = "";
                    if (await rd.ReadAsync())
                        msg = rd.GetValue(0)?.ToString() ?? "";
                    if (string.IsNullOrWhiteSpace(msg) || !msg.StartsWith("OK", StringComparison.OrdinalIgnoreCase))
                        return BadRequest(new { message = string.IsNullOrWhiteSpace(msg) ? "退審失敗" : msg });
                    result.Ok = true;
                    result.Message = msg;
                    return Ok(result);
                }
            }

            if (canbMustNotes == 1 && string.IsNullOrWhiteSpace(reason))
            {
                result.NeedConfirm = true;
                result.ConfirmType = "rejExamNotes";
                result.ConfirmMessage = "請輸入退審原因";
                result.RejectRequiresNotes = true;
                return Ok(result);
            }

            if (!string.IsNullOrWhiteSpace(reason))
            {
                if (columns.Contains("Notes"))
                    await AppendNotesAsync(conn, safePaperId, paperNum, reason);  // 查詢用真實表名

                try
                {
                    await ExecRejNotesAsync(conn, safePaperId, reason, paperNum, req.ItemId ?? "", userId);  // SP 需要真實表名稱
                }
                catch
                {
                    // ignore missing SP
                }
            }

            var status = string.Equals(dictPaperId, "fmedpassmain", StringComparison.OrdinalIgnoreCase) ? 2 : 3;

            using (var cmd = new SqlCommand("CURdPaperAction", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@PaperId", safePaperId);  // SP 需要真實表名稱
                cmd.Parameters.AddWithValue("@PaperNum", paperNum);
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@EOC", 0);
                cmd.Parameters.AddWithValue("@AftFinished", status);
                await cmd.ExecuteNonQueryAsync();
            }

            var msgAfter = await GetPaperMsgAsync(conn, req.ItemId, new[] { "RejExam", "退審" }, 1);
            result.Ok = true;
            result.Message = string.IsNullOrWhiteSpace(msgAfter) ? "退審完成" : msgAfter;
            return Ok(result);
        }

        private static async Task<IActionResult> RunPaperExamAsync(
            SqlConnection conn,
            string dictPaperId,
            string safePaperId,
            string paperNum,
            string userId,
            string? itemId,
            int canbRunFlow,
            int canbAudit,
            string? paperCallPaperAftExam,
            string? globalId,
            string useId)
        {
            if (canbRunFlow == 1 && canbAudit == 0)
                return new BadRequestObjectResult(new { message = "您沒有「審核」的權限" });

            using (var cmd = new SqlCommand("CURdPaperAction", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@PaperId", safePaperId);  // SP 需要真實表名稱
                cmd.Parameters.AddWithValue("@PaperNum", paperNum);
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@EOC", 1);
                cmd.Parameters.AddWithValue("@AftFinished", 1);
                await cmd.ExecuteNonQueryAsync();
            }

            var msgAfter = await GetPaperMsgAsync(conn, itemId, new[] { "Exam", "審核" }, 1);
            var result = new ConfirmResult
            {
                Ok = true,
                Message = string.IsNullOrWhiteSpace(msgAfter) ? "已完成審核" : msgAfter
            };

            await AppendTransferPromptAsync(conn, dictPaperId, safePaperId, paperNum, userId, useId, paperCallPaperAftExam, globalId, result);
            return new OkObjectResult(result);
        }

        private static async Task<IActionResult> RunSendForAuditAsync(
            SqlConnection conn,
            string dictPaperId,
            string safePaperId,
            string paperNum,
            string userId,
            string? paperCallPaperAftExam,
            string? globalId,
            string useId)
        {
            var sql = "exec CURdPaperDoNewStatus @PaperId,@PaperNum,@UserId,1,3,@Blank";
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@PaperId", safePaperId);  // SP 需要真實表名稱
                cmd.Parameters.AddWithValue("@PaperNum", paperNum);
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@Blank", "");
                using var rd = await cmd.ExecuteReaderAsync();
                var msg = "";
                if (await rd.ReadAsync())
                    msg = rd.GetValue(0)?.ToString() ?? "";
                if (string.IsNullOrWhiteSpace(msg) || !msg.StartsWith("OK", StringComparison.OrdinalIgnoreCase))
                    return new BadRequestObjectResult(new { message = string.IsNullOrWhiteSpace(msg) ? "送審失敗" : msg });

                var result = new ConfirmResult { Ok = true, Message = msg };
                await AppendTransferPromptAsync(conn, dictPaperId, safePaperId, paperNum, userId, useId, paperCallPaperAftExam, globalId, result);
                return new OkObjectResult(result);
            }
        }

        private static async Task AppendTransferPromptAsync(
            SqlConnection conn,
            string dictPaperId,
            string safePaperId,
            string paperNum,
            string userId,
            string useId,
            string? paperCallPaperAftExam,
            string? globalId,
            ConfirmResult result)
        {
            if (string.IsNullOrWhiteSpace(paperCallPaperAftExam)) return;
            if (string.IsNullOrWhiteSpace(globalId)) return;

            var finished = await LoadFinishedAsync(conn, safePaperId, paperNum);  // 查詢用真實表名
            if (finished is not (1 or 4)) return;

            var sql = @"
exec CURdCallPaperAftTran @GlobalId,@PaperCallPaperAftExam,@PaperId,@PaperNum,@UserId,@UseId";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@GlobalId", globalId);
            cmd.Parameters.AddWithValue("@PaperCallPaperAftExam", paperCallPaperAftExam);
            cmd.Parameters.AddWithValue("@PaperId", safePaperId);  // SP 需要真實表名稱
            cmd.Parameters.AddWithValue("@PaperNum", paperNum);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@UseId", useId);
            using var rd = await cmd.ExecuteReaderAsync();
            if (!await rd.ReadAsync()) return;
            var msg = rd.GetValue(0)?.ToString() ?? "";
            if (string.Equals(msg, "OK", StringComparison.OrdinalIgnoreCase))
            {
                result.TransferPrompt = true;
                result.TransferMessage = "是否要檢視拋轉的單據？";
            }
        }

        private static async Task<int?> LoadFinishedAsync(SqlConnection conn, string table, string paperNum)
        {
            var sql = $"SELECT TOP 1 [Finished] FROM [{table}] WITH (NOLOCK) WHERE [PaperNum] = @PaperNum";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@PaperNum", paperNum);
            var obj = await cmd.ExecuteScalarAsync();
            return obj == null || obj == DBNull.Value ? null : int.TryParse(obj.ToString(), out var v) ? v : null;
        }

        private static async Task<(string? paperExamBeforeSql, string? paperCallPaperAftExam)> LoadPaperExamConfigAsync(SqlConnection conn, string paperId)
        {
            try
            {
                var cols = await GetTableColumnsAsync(conn, "CURdPaperInfo");
                if (cols.Count == 0) return (null, null);

                var selectCols = new List<string>();
                if (cols.Contains("PaperExamBeforeSQL")) selectCols.Add("[PaperExamBeforeSQL]");
                if (cols.Contains("PaperCallPaperAftExam")) selectCols.Add("[PaperCallPaperAftExam]");
                if (selectCols.Count == 0) return (null, null);

                var sql = $@"
SELECT TOP 1 {string.Join(", ", selectCols)}
  FROM CURdPaperInfo WITH (NOLOCK)
 WHERE LOWER(PaperId) = LOWER(@paperId);";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@paperId", paperId);
                using var rd = await cmd.ExecuteReaderAsync();
                if (!await rd.ReadAsync()) return (null, null);

                string? before = cols.Contains("PaperExamBeforeSQL") && !rd.IsDBNull(0) ? rd.GetValue(0)?.ToString() : null;
                string? aftExam = null;
                if (cols.Contains("PaperCallPaperAftExam"))
                {
                    var idx = cols.Contains("PaperExamBeforeSQL") ? 1 : 0;
                    if (rd.FieldCount > idx && !rd.IsDBNull(idx))
                        aftExam = rd.GetValue(idx)?.ToString();
                }
                return (before, aftExam);
            }
            catch
            {
                return (null, null);
            }
        }

        private static async Task<string?> GetSystemIdAsync(SqlConnection conn, string? itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId)) return null;
            const string sql = @"
SELECT TOP 1 SystemId
  FROM CURdSysItems WITH (NOLOCK)
 WHERE ItemId = @itemId";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@itemId", itemId);
            var obj = await cmd.ExecuteScalarAsync();
            return obj == null || obj == DBNull.Value ? null : obj.ToString()?.Trim();
        }

        private static async Task<string?> GetSysParamAsync(SqlConnection conn, string systemId, string paramName)
        {
            try
            {
                const string sql = "exec CURdOCXSysParamGet @SystemId, @ParamName";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@SystemId", systemId ?? "");
                cmd.Parameters.AddWithValue("@ParamName", paramName ?? "");
                using var rd = await cmd.ExecuteReaderAsync();
                if (await rd.ReadAsync())
                    return rd.GetValue(0)?.ToString();
            }
            catch
            {
                // ignore missing SP
            }
            return null;
        }

        private static async Task<string?> GetPaperMsgAsync(SqlConnection conn, string? itemId, IEnumerable<string> btnNames, int isAfter)
        {
            if (string.IsNullOrWhiteSpace(itemId)) return null;
            foreach (var btn in btnNames)
            {
                var msg = await TryGetPaperMsgAsync(conn, itemId, btn, isAfter);
                if (!string.IsNullOrWhiteSpace(msg)) return msg;
            }
            return null;
        }

        private static async Task<string?> TryGetPaperMsgAsync(SqlConnection conn, string itemId, string btnName, int isAfter)
        {
            try
            {
                const string sql = "exec CURdPaperMsgGet @ItemId, @BtnName, @IsAfter";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ItemId", itemId);
                cmd.Parameters.AddWithValue("@BtnName", btnName);
                cmd.Parameters.AddWithValue("@IsAfter", isAfter);
                using var rd = await cmd.ExecuteReaderAsync();
                if (await rd.ReadAsync())
                    return rd.GetValue(0)?.ToString();
            }
            catch
            {
                // ignore missing SP
            }
            return null;
        }

        private static async Task<string> TryRunFlowAsync(
            SqlConnection conn,
            string? itemId,
            string safePaperId,
            string paperNum,
            string userId,
            string useId,
            string? systemId,
            int nowFlowStatus)
        {
            if (string.IsNullOrWhiteSpace(itemId)) return "NOFLOW";
            try
            {
                var sql = "exec CURdOCXPaperToFlow @ItemId,@PaperId,@PaperNum,@UserId,@UseId,@SystemId,@FlowStatus";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ItemId", itemId);
                cmd.Parameters.AddWithValue("@PaperId", safePaperId);  // SP 需要真實表名稱
                cmd.Parameters.AddWithValue("@PaperNum", paperNum);
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@UseId", useId);
                cmd.Parameters.AddWithValue("@SystemId", systemId ?? "");
                cmd.Parameters.AddWithValue("@FlowStatus", nowFlowStatus);
                using var rd = await cmd.ExecuteReaderAsync();
                if (await rd.ReadAsync())
                    return rd.GetValue(0)?.ToString() ?? "NOFLOW";
            }
            catch
            {
                return "";
            }
            return "NOFLOW";
        }

        private static async Task<List<(string FieldName, string DisplayLabel, string DataType)>> GetRequiredFieldsAsync(SqlConnection conn, string? dictTable)
        {
            var list = new List<(string, string, string)>();
            if (string.IsNullOrWhiteSpace(dictTable)) return list;
            const string sql = @"
SELECT FieldName, ISNULL(NULLIF(DisplayLabel,''), FieldName) AS DisplayLabel, ISNULL(DataType,'') AS DataType
  FROM CURdTableField WITH (NOLOCK)
 WHERE TableName = @table AND ISNULL(IsNeed, 0) = 1;";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@table", dictTable);
            using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                var field = rd.GetValue(0)?.ToString() ?? "";
                if (string.IsNullOrWhiteSpace(field)) continue;
                var label = rd.GetValue(1)?.ToString() ?? field;
                var dataType = rd.GetValue(2)?.ToString() ?? "";
                list.Add((field, label, dataType));
            }
            return list;
        }

        private static bool IsStringType(string dataType)
        {
            var t = (dataType ?? "").Trim().ToLowerInvariant();
            return t.Contains("char") || t.Contains("string") || t.Contains("text");
        }

        private static string ReplaceSqlTokens(string sql, Dictionary<string, object?> row)
        {
            if (string.IsNullOrWhiteSpace(sql)) return sql;
            foreach (var kv in row)
            {
                if (string.IsNullOrWhiteSpace(kv.Key)) continue;
                sql = ReplaceSqlToken(sql, kv.Key, kv.Value);
            }
            return sql;
        }

        private static string ReplaceSqlToken(string sql, string key, object? value)
        {
            var literal = ToSqlLiteral(value);
            return Regex.Replace(
                sql,
                "@" + Regex.Escape(key),
                literal,
                RegexOptions.IgnoreCase);
        }

        private static string ToSqlLiteral(object? value)
        {
            if (value == null || value == DBNull.Value) return "NULL";
            if (value is bool b) return b ? "1" : "0";
            if (value is byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal)
                return Convert.ToString(value, CultureInfo.InvariantCulture) ?? "0";
            if (value is DateTime dt)
                return $"'{dt:yyyy/MM/dd HH:mm:ss}'";
            var s = value.ToString() ?? "";
            return $"'{s.Replace("'", "''")}'";
        }

        private static async Task<string?> RunQueryFirstStringAsync(SqlConnection conn, string sql)
        {
            using var cmd = new SqlCommand(sql, conn);
            using var rd = await cmd.ExecuteReaderAsync();
            if (await rd.ReadAsync())
                return rd.GetValue(0)?.ToString();
            return null;
        }

        private static async Task ExecSqlAsync(SqlConnection conn, string sql)
        {
            using var cmd = new SqlCommand(sql, conn);
            await cmd.ExecuteNonQueryAsync();
        }

        #region CheckCanEdit / RejectForEdit

        public class CheckCanEditRequest
        {
            public string PaperId { get; set; } = "";
            public string PaperNum { get; set; } = "";
            public string? ItemId { get; set; }
            public string? UserId { get; set; }
            public string? UseId { get; set; }
        }

        public class CheckCanEditResponse
        {
            public bool CanEdit { get; set; }
            public string? Message { get; set; }
            public string? BlockReason { get; set; }
            public bool RequiresRejection { get; set; }
            public bool CanAutoReject { get; set; }
            public bool RejectRequiresNotes { get; set; }
            public string? RejectMessage { get; set; }
            public Dictionary<string, object?>? LatestData { get; set; }
        }

        [HttpPost("CheckCanEdit")]
        public async Task<IActionResult> CheckCanEdit([FromBody] CheckCanEditRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.PaperId) || string.IsNullOrWhiteSpace(req.PaperNum))
                return BadRequest(new { message = "缺少必要參數" });

            var connStr = _config.GetConnectionString("DefaultConnection");
            using var conn = new SqlConnection(connStr);

            try
            {
                await conn.OpenAsync();
                var dictPaperId = req.PaperId.Trim();

                // 從 CURdTableName 取得實際的資料表名稱
                var safePaperId = await ResolveRealTableNameAsync(conn, dictPaperId);
                if (!IsSafeIdentifier(safePaperId))
                    return BadRequest(new { message = "PaperId 不合法" });

                if (!await TableExistsAsync(conn, safePaperId))
                    return BadRequest(new { message = "找不到單據資料表" });

                var paperNum = req.PaperNum.Trim();
                var userId = string.IsNullOrWhiteSpace(req.UserId) ? "admin" : req.UserId.Trim();
                var useId = string.IsNullOrWhiteSpace(req.UseId) ? "A001" : req.UseId.Trim();

                var columns = await GetTableColumnsAsync(conn, safePaperId);
                if (!columns.Contains("PaperNum"))
                    return BadRequest(new { message = "單據資料表缺少 PaperNum 欄位" });

                var row = await LoadPaperRowAsync(conn, safePaperId, paperNum, columns);
                if (row == null)
                    return BadRequest(new { message = "找不到指定單據" });

                var finished = TryParseInt(row, "Finished") ?? 0;
                var rowPaperId = TryGetString(row, "PaperId");
                var flowStatus = TryParseInt(row, "FlowStatus") ?? 0;

                var (canbRunFlow, canbSelectType, canbLockPaperDate, canbLockUserEdit, canbMustNotes, headFirst) =
                    await LoadPaperInfoAsync(conn, dictPaperId, safePaperId);
                var (canbScrap, canbUpdate, canbAudit, canbAuditBack, canbUpdateNotes, canbUpdateMoney, canbViewMoney, canbPrint) =
                    await LoadUserItemPowerAsync(conn, req.ItemId, userId, useId);

                var latestData = new Dictionary<string, object?>
                {
                    ["Finished"] = finished,
                    ["FlowStatus"] = flowStatus,
                    ["PaperId"] = rowPaperId
                };

                // 1. 檢查 PaperId 拋轉單據
                if (!string.IsNullOrWhiteSpace(rowPaperId))
                {
                    if (string.Equals(safePaperId, "MPHdExtendMain", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(safePaperId, "MPHdPettyMain", StringComparison.OrdinalIgnoreCase))
                    {
                        return Ok(new CheckCanEditResponse
                        {
                            CanEdit = false,
                            Message = "此單據是其它單據拋轉而來，不能修改!",
                            BlockReason = "TRANSFER_NO_EDIT",
                            LatestData = latestData
                        });
                    }
                    else
                    {
                        return Ok(new CheckCanEditResponse
                        {
                            CanEdit = false,
                            Message = "此單據是其它單據拋轉而來，請由原單據修改",
                            BlockReason = "TRANSFER_EDIT_ORIGINAL",
                            LatestData = latestData
                        });
                    }
                }

                // 2. 檢查已作廢/已結案
                if (finished == 2)
                {
                    return Ok(new CheckCanEditResponse
                    {
                        CanEdit = false,
                        Message = "此單據已作廢,不可修改",
                        BlockReason = "VOIDED",
                        LatestData = latestData
                    });
                }
                if (finished == 4)
                {
                    return Ok(new CheckCanEditResponse
                    {
                        CanEdit = false,
                        Message = "此單據已結案,不可修改",
                        BlockReason = "CLOSED",
                        LatestData = latestData
                    });
                }

                // 3. 檢查 FlowStatus (已送審/已駁回/已抽單) - 允許直接修改
                if (flowStatus == 31 || flowStatus == 32 || flowStatus == 33)
                {
                    return Ok(new CheckCanEditResponse
                    {
                        CanEdit = true,
                        LatestData = latestData
                    });
                }

                // 4. 檢查 Finished = 1 (已確認)
                if (finished == 1)
                {
                    if (canbRunFlow == 1 && canbAuditBack == 0)
                    {
                        return Ok(new CheckCanEditResponse
                        {
                            CanEdit = false,
                            Message = "此單據已確認,須先退審才可修改",
                            BlockReason = "FINISHED_NO_PERMISSION",
                            RequiresRejection = true,
                            CanAutoReject = false,
                            LatestData = latestData
                        });
                    }

                    if (canbRunFlow == 1 && canbAuditBack == 1)
                    {
                        return Ok(new CheckCanEditResponse
                        {
                            CanEdit = false,
                            Message = "此單據已確認,須先退審才可修改",
                            BlockReason = "FINISHED_NEED_REJECT",
                            RequiresRejection = true,
                            CanAutoReject = true,
                            RejectRequiresNotes = canbMustNotes == 1,
                            RejectMessage = "是否確定退審此單據？退審後可進行修改。",
                            LatestData = latestData
                        });
                    }
                }

                // 5. 檢查 Finished = 3 (審核中) - 允許直接修改
                if (finished == 3)
                {
                    return Ok(new CheckCanEditResponse
                    {
                        CanEdit = true,
                        LatestData = latestData
                    });
                }

                // 通過所有檢查，可以編輯
                return Ok(new CheckCanEditResponse
                {
                    CanEdit = true,
                    LatestData = latestData
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        public class RejectForEditRequest
        {
            public string PaperId { get; set; } = "";
            public string PaperNum { get; set; } = "";
            public string? ItemId { get; set; }
            public string? UserId { get; set; }
            public string? UseId { get; set; }
            public string? RejectNotes { get; set; }
        }

        [HttpPost("RejectForEdit")]
        public async Task<IActionResult> RejectForEdit([FromBody] RejectForEditRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.PaperId) || string.IsNullOrWhiteSpace(req.PaperNum))
                return BadRequest(new { message = "缺少必要參數" });

            var connStr = _config.GetConnectionString("DefaultConnection");
            using var conn = new SqlConnection(connStr);

            try
            {
                await conn.OpenAsync();
                var dictPaperId = req.PaperId.Trim();

                // 從 CURdTableName 取得實際的資料表名稱
                var safePaperId = await ResolveRealTableNameAsync(conn, dictPaperId);
                if (!IsSafeIdentifier(safePaperId))
                    return BadRequest(new { message = "PaperId 不合法" });

                if (!await TableExistsAsync(conn, safePaperId))
                    return BadRequest(new { message = "找不到單據資料表" });

                var paperNum = req.PaperNum.Trim();
                var userId = string.IsNullOrWhiteSpace(req.UserId) ? "admin" : req.UserId.Trim();
                var useId = string.IsNullOrWhiteSpace(req.UseId) ? "A001" : req.UseId.Trim();

                var columns = await GetTableColumnsAsync(conn, safePaperId);
                var row = await LoadPaperRowAsync(conn, safePaperId, paperNum, columns);
                if (row == null)
                    return BadRequest(new { message = "找不到指定單據" });

                var finished = TryParseInt(row, "Finished") ?? 0;

                var (canbRunFlow, _, _, _, canbMustNotes, _) = await LoadPaperInfoAsync(conn, dictPaperId, safePaperId);
                var (_, _, _, canbAuditBack, _, _, _, _) = await LoadUserItemPowerAsync(conn, req.ItemId, userId, useId);

                // 檢查權限
                if (canbRunFlow == 1 && canbAuditBack == 0)
                    return BadRequest(new { message = "您沒有「退審」的權限" });

                // 檢查是否需要備註
                if (canbMustNotes == 1 && string.IsNullOrWhiteSpace(req.RejectNotes))
                    return BadRequest(new { code = "NEED_REJECT_NOTES", message = "請輸入退審原因" });

                // 如果有備註，先更新 Notes 欄位
                if (!string.IsNullOrWhiteSpace(req.RejectNotes) && columns.Contains("Notes"))
                {
                    var notesText = $"\n[退審] {DateTime.Now:yyyy/MM/dd HH:mm} {userId}: {req.RejectNotes}";
                    await AppendNotesAsync(conn, safePaperId, paperNum, notesText);
                }

                // 呼叫 CURdRejNotes 記錄退審原因 (如果有備註)
                if (!string.IsNullOrWhiteSpace(req.RejectNotes))
                {
                    try
                    {
                        await ExecRejNotesAsync(conn, safePaperId, req.RejectNotes, paperNum, req.ItemId ?? "", userId);  // SP 需要真實表名稱
                    }
                    catch
                    {
                        // CURdRejNotes 可能不存在，忽略錯誤
                    }
                }

                // 呼叫 CURdPaperAction 執行退審 (AftFinished = 3 -> 審核中)
                using var cmd = new SqlCommand("CURdPaperAction", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@PaperId", safePaperId);  // SP 需要真實表名稱
                cmd.Parameters.AddWithValue("@PaperNum", paperNum);
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@EOC", 0);
                cmd.Parameters.AddWithValue("@AftFinished", 3); // 退回審核中狀態

                await cmd.ExecuteNonQueryAsync();

                return Ok(new { success = true, newFinished = 3 });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        private static async Task<string?> GetFlowPrcIdAsync(SqlConnection conn, string? itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId)) return null;

            const string sql = @"
SELECT TOP 1 FlowPrcId
  FROM CURdSysItems WITH (NOLOCK)
 WHERE ItemId = @itemId";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@itemId", itemId);
            var obj = await cmd.ExecuteScalarAsync();
            return obj == null || obj == DBNull.Value ? null : obj.ToString()?.Trim();
        }

        private static async Task<string> ResolveRealTableNameAsync(SqlConnection conn, string dictTableName)
        {
            if (string.IsNullOrWhiteSpace(dictTableName)) return dictTableName;

            const string sql = @"
SELECT TOP 1 ISNULL(NULLIF(RealTableName,''), TableName) AS ActualName
  FROM CURdTableName WITH (NOLOCK)
 WHERE TableName = @tbl";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@tbl", dictTableName.Trim());
            var obj = await cmd.ExecuteScalarAsync();
            return obj == null || obj == DBNull.Value ? dictTableName.Trim() : obj.ToString()?.Trim() ?? dictTableName.Trim();
        }

        private static async Task ExecRejNotesAsync(SqlConnection conn, string paperId, string reason, string paperNum, string itemId, string userId)
        {
            const string sql = "exec CURdRejNotes @PaperId, @Reason, @PaperNum, @ItemId, @UserId";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@PaperId", paperId);
            cmd.Parameters.AddWithValue("@Reason", reason ?? "");
            cmd.Parameters.AddWithValue("@PaperNum", paperNum);
            cmd.Parameters.AddWithValue("@ItemId", itemId ?? "");
            cmd.Parameters.AddWithValue("@UserId", userId ?? "");
            await cmd.ExecuteNonQueryAsync();
        }

        #endregion

        #region EMOdProdAudit - 工程資料審核

        public class EMOdProdAuditRequest
        {
            public string PartNum { get; set; } = "";
            public string Revision { get; set; } = "0";
            public int Tag { get; set; } = 0;
            public int IOType { get; set; } = 1;  // 1=審核, 6=退審
            public string UserId { get; set; } = "Admin";
            public string Meno { get; set; } = "";
        }

        /// <summary>
        /// 取得 EMOdProdInfo 的 Status 狀態
        /// </summary>
        [HttpGet("GetEMOdProdStatus")]
        public async Task<IActionResult> GetEMOdProdStatus([FromQuery] string partNum, [FromQuery] string revision = "0")
        {
            if (string.IsNullOrWhiteSpace(partNum))
                return BadRequest(new { success = false, message = "PartNum 不可為空" });

            var connStr = _config.GetConnectionString("DefaultConnection");
            using var conn = new SqlConnection(connStr);

            try
            {
                await conn.OpenAsync();

                const string sql = @"
                    SELECT Status
                    FROM EMOdProdInfo WITH (NOLOCK)
                    WHERE PartNum = @PartNum AND Revision = @Revision";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@PartNum", partNum.Trim());
                cmd.Parameters.AddWithValue("@Revision", revision?.Trim() ?? "0");

                var result = await cmd.ExecuteScalarAsync();
                if (result == null || result == DBNull.Value)
                    return NotFound(new { success = false, message = "找不到該料號" });

                var status = Convert.ToInt32(result);
                return Ok(new { success = true, status });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 執行 EMOdProdAudit 存儲過程（送審/審核/退審）
        /// </summary>
        [HttpPost("EMOdProdAudit")]
        public async Task<IActionResult> EMOdProdAudit([FromBody] EMOdProdAuditRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.PartNum))
                return BadRequest(new { success = false, message = "PartNum 不可為空" });

            var connStr = _config.GetConnectionString("DefaultConnection");
            using var conn = new SqlConnection(connStr);

            try
            {
                await conn.OpenAsync();

                using var cmd = new SqlCommand("EMOdProdAudit", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@PartNum", req.PartNum.Trim());
                cmd.Parameters.AddWithValue("@Revision", req.Revision?.Trim() ?? "0");
                cmd.Parameters.AddWithValue("@Tag", req.Tag);
                cmd.Parameters.AddWithValue("@IOType", req.IOType);
                cmd.Parameters.AddWithValue("@UserId", string.IsNullOrWhiteSpace(req.UserId) ? "Admin" : req.UserId.Trim());
                cmd.Parameters.AddWithValue("@Meno", req.Meno ?? "");

                await cmd.ExecuteNonQueryAsync();

                var actionName = req.IOType == 1 ? "審核" : "退審";
                return Ok(new { success = true, message = $"已成功執行{actionName}" });
            }
            catch (SqlException ex)
            {
                // 處理 SP 中的 RAISERROR
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        #endregion
    }
}
