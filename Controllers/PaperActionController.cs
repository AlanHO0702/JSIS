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
                var safePaperId = req.PaperId.Trim();
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
    }
}
