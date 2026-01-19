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
                    return BadRequest("找不到單據資料表");

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
                    await LoadPaperInfoAsync(conn, safePaperId);
                var (canbScrap, canbUpdate, canbAudit, canbAuditBack, canbUpdateNotes, canbUpdateMoney, canbViewMoney, canbPrint) =
                    await LoadUserItemPowerAsync(conn, req.ItemId, userId, useId);

                if (req.AftFinished == 2)
                {
                    if (finished == 2)
                        return BadRequest(new { message = "此單據「已作廢」,不須作廢" });
                    if (finished == 4)
                        return BadRequest(new { message = "此單據「已結案」,不可作廢" });

                    if (canbRunFlow == 1 && finished == 1)
                    {
                        var canVoid = await CheckCanVoidAsync(conn, safePaperId);
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
                cmd.Parameters.AddWithValue("@PaperId", safePaperId);
                cmd.Parameters.AddWithValue("@PaperNum", paperNum);
                cmd.Parameters.AddWithValue("@UserId", userId); // 預設值
                cmd.Parameters.AddWithValue("@EOC", req.EOC);
                cmd.Parameters.AddWithValue("@AftFinished", req.AftFinished);

                await cmd.ExecuteNonQueryAsync();
                return Ok(new { message = "已成功執行 CURdPaperAction" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
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
            LoadPaperInfoAsync(SqlConnection conn, string paperId)
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

                var sql = $@"
SELECT TOP 1 {string.Join(", ", selectCols)}
  FROM CURdPaperInfo WITH (NOLOCK)
 WHERE LOWER(PaperId) = LOWER(@paperId);";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@paperId", paperId);
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
                    await LoadPaperInfoAsync(conn, safePaperId);
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

                // 3. 檢查 FlowStatus = 31 (已進入簽核流程)
                if (flowStatus == 31)
                {
                    var flowPrcId = await GetFlowPrcIdAsync(conn, req.ItemId);
                    var bUseFlow = canbRunFlow == 1;

                    if (bUseFlow && !string.IsNullOrWhiteSpace(flowPrcId))
                    {
                        return Ok(new CheckCanEditResponse
                        {
                            CanEdit = false,
                            Message = "此單據已進入電子簽核流程,不可修改",
                            BlockReason = "ELECTRONIC_FLOW",
                            LatestData = latestData
                        });
                    }
                    else if (canbRunFlow == 1)
                    {
                        return Ok(new CheckCanEditResponse
                        {
                            CanEdit = false,
                            Message = "此單據已送審,須先退審才可修改",
                            BlockReason = "SENT_FOR_APPROVAL",
                            RequiresRejection = true,
                            CanAutoReject = canbAuditBack == 1,
                            RejectRequiresNotes = canbMustNotes == 1,
                            RejectMessage = "是否確定退審此單據？退審後可進行修改。",
                            LatestData = latestData
                        });
                    }
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

                // 5. 檢查 Finished = 3 (審核中) - 視情況
                if (finished == 3)
                {
                    if (canbRunFlow == 1 && canbAuditBack == 0)
                    {
                        return Ok(new CheckCanEditResponse
                        {
                            CanEdit = false,
                            Message = "此單據審核中,須先退審才可修改",
                            BlockReason = "REVIEWING_NO_PERMISSION",
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
                            Message = "此單據審核中,須先退審才可修改",
                            BlockReason = "REVIEWING_NEED_REJECT",
                            RequiresRejection = true,
                            CanAutoReject = true,
                            RejectRequiresNotes = canbMustNotes == 1,
                            RejectMessage = "是否確定退審此單據？退審後可進行修改。",
                            LatestData = latestData
                        });
                    }
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

                var (canbRunFlow, _, _, _, canbMustNotes, _) = await LoadPaperInfoAsync(conn, safePaperId);
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
                        await ExecRejNotesAsync(conn, safePaperId, req.RejectNotes, paperNum, req.ItemId ?? "", userId);
                    }
                    catch
                    {
                        // CURdRejNotes 可能不存在，忽略錯誤
                    }
                }

                // 呼叫 CURdPaperAction 執行退審 (AftFinished = 0)
                using var cmd = new SqlCommand("CURdPaperAction", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@PaperId", safePaperId);
                cmd.Parameters.AddWithValue("@PaperNum", paperNum);
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@EOC", 0);
                cmd.Parameters.AddWithValue("@AftFinished", 0); // 退回作業中狀態

                await cmd.ExecuteNonQueryAsync();

                return Ok(new { success = true, message = "退審成功，可進行修改", newFinished = 0 });
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
    }
}
