using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using System.Data;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class UpdateLogController : ControllerBase
{
    private readonly string _cs;

    public UpdateLogController(IConfiguration cfg, PcbErpContext db)
    {
        _cs = cfg.GetConnectionString("DefaultConnection")
              ?? db.Database.GetDbConnection().ConnectionString;
    }

    [HttpGet("Master")]
    public async Task<IActionResult> GetMaster(
        [FromQuery] string? paperId,
        [FromQuery] string? itemId,
        [FromQuery] string paperNum,
        [FromQuery] string? userId,
        [FromQuery] string? logType)
    {
        if (string.IsNullOrWhiteSpace(paperNum))
            return Ok(new { ok = false, error = "paperNum is required." });

        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync();

        // 1) 先嘗試呼叫 SP
        var spName = BuildSpName("CURdTableUpdateLogInq", logType);
        var sql = $"exec {spName} @PaperId, @PaperNum, @UserId";

        var result = await QueryWithFallbackAsync(conn, sql, paperId, itemId, paperNum, userId ?? string.Empty);

        // 2) SP 成功且有資料，直接回傳
        if (result.Ok && result.Rows.Count > 0)
            return Ok(new { ok = true, rows = result.Rows, usedPaperId = result.UsedPaperId, triedPaperIds = result.TriedPaperIds });

        // 3) SP 失敗或無資料 → 直接查 CURdTableUpdateLog 表
        var directRows = await DirectQueryLogTableAsync(conn, "CURdTableUpdateLog", paperId, itemId, paperNum);
        if (directRows.Count > 0)
            return Ok(new { ok = true, rows = directRows, source = "direct" });

        // 4) 都沒有資料
        if (result.Ok)
            return Ok(new { ok = true, rows = result.Rows, usedPaperId = result.UsedPaperId, triedPaperIds = result.TriedPaperIds });

        return Ok(new { ok = false, error = result.Error ?? "UpdateLog query failed.", triedPaperIds = result.TriedPaperIds });
    }

    [HttpGet("History")]
    public async Task<IActionResult> GetHistory(
        [FromQuery] string? paperId,
        [FromQuery] string? itemId,
        [FromQuery] string paperNum,
        [FromQuery] string? userId,
        [FromQuery] string? logType)
    {
        if (string.IsNullOrWhiteSpace(paperNum))
            return Ok(new { ok = false, error = "paperNum is required." });

        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync();

        // 1) 先嘗試呼叫 SP
        var spName = BuildSpName("CURdTableUpdateLogInqHis", logType);
        var sql = $"exec {spName} @PaperId, @PaperNum, @UserId";

        var result = await QueryWithFallbackAsync(conn, sql, paperId, itemId, paperNum, userId ?? string.Empty);

        // 2) SP 成功且有資料，直接回傳
        if (result.Ok && result.Rows.Count > 0)
            return Ok(new { ok = true, rows = result.Rows, usedPaperId = result.UsedPaperId, triedPaperIds = result.TriedPaperIds });

        // 3) SP 失敗或無資料 → 直接查 CURdTableUpdateLogHis 表
        var directRows = await DirectQueryLogTableAsync(conn, "CURdTableUpdateLogHis", paperId, itemId, paperNum);
        if (directRows.Count > 0)
            return Ok(new { ok = true, rows = directRows, source = "direct" });

        // 4) 都沒有資料
        if (result.Ok)
            return Ok(new { ok = true, rows = result.Rows, usedPaperId = result.UsedPaperId, triedPaperIds = result.TriedPaperIds });

        return Ok(new { ok = false, error = result.Error ?? "UpdateLog query failed.", triedPaperIds = result.TriedPaperIds });
    }

    /// <summary>
    /// 直接查詢 CURdTableUpdateLog / CURdTableUpdateLogHis 表，
    /// 作為 SP 不存在或查錯表時的 fallback。
    /// JOIN CURdTableName 取得中文表名 (DisplayLabel)，
    /// JOIN CURdUsers 取得作業者中文名 (UserName)。
    /// 對應 Delphi UpdateLog 表單的 qryMaster1 欄位結構。
    /// </summary>
    private static async Task<List<Dictionary<string, object?>>> DirectQueryLogTableAsync(
        SqlConnection conn, string tableName, string? paperId, string? itemId, string keyNum)
    {
        var candidates = BuildPaperIdCandidates(paperId, itemId);
        if (candidates.Count == 0) return new List<Dictionary<string, object?>>();

        foreach (var pid in candidates)
        {
            try
            {
                var sql = $@"
SELECT L.PaperId, L.TableName,
       ISNULL(N.DisplayLabel, L.TableName) AS DisplayLabel,
       L.SerialNum, L.KeyNum, L.UpdateTime,
       L.UserId,
       ISNULL(U.UserName, L.UserId) AS UserName,
       L.Notes, L.Difference, L.KeySalaryId
  FROM [{tableName}] L WITH (NOLOCK)
  LEFT JOIN CURdTableName N WITH (NOLOCK) ON N.TableName = L.TableName
  LEFT JOIN CURdUsers U WITH (NOLOCK) ON U.UserId = L.UserId
 WHERE L.PaperId = @PaperId AND L.KeyNum = @KeyNum
 ORDER BY L.SerialNum";
                var rows = await QueryRowsAsync(conn, sql,
                    new SqlParameter("@PaperId", pid),
                    new SqlParameter("@KeyNum", keyNum));
                if (rows.Count > 0) return rows;
            }
            catch { /* 表可能不存在，跳過 */ }
        }

        return new List<Dictionary<string, object?>>();
    }

    private static readonly System.Text.RegularExpressions.Regex SafeIdentifier =
        new(@"^[A-Za-z0-9_]+$", System.Text.RegularExpressions.RegexOptions.Compiled);

    private static string BuildSpName(string baseName, string? logType)
    {
        if (string.IsNullOrWhiteSpace(logType))
            return baseName;

        var suffix = logType.Trim();
        if (!SafeIdentifier.IsMatch(suffix))
            return baseName; // 不合法的後綴，回退到通用 SP

        return $"{baseName}_{suffix}";
    }

    private static async Task<(bool Ok, List<Dictionary<string, object?>> Rows, string? UsedPaperId, string[] TriedPaperIds, string? Error)>
        QueryWithFallbackAsync(SqlConnection conn, string sql, string? paperId, string? itemId, string paperNum, string userId)
    {
        var candidates = BuildPaperIdCandidates(paperId, itemId);
        if (candidates.Count == 0)
            return (false, new List<Dictionary<string, object?>>(), null, Array.Empty<string>(), "paperId/itemId is required.");

        Exception? lastEx = null;
        List<Dictionary<string, object?>>? firstRows = null;
        string? firstUsed = null;

        foreach (var pid in candidates)
        {
            try
            {
                var rows = await QueryRowsAsync(conn, sql,
                    new SqlParameter("@PaperId", pid),
                    new SqlParameter("@PaperNum", paperNum),
                    new SqlParameter("@UserId", userId));

                if (firstRows == null)
                {
                    firstRows = rows;
                    firstUsed = pid;
                }

                if (rows.Count > 0)
                    return (true, rows, pid, candidates.ToArray(), null);
            }
            catch (Exception ex)
            {
                lastEx = ex;
            }
        }

        if (firstRows != null)
            return (true, firstRows, firstUsed, candidates.ToArray(), null);

        return (false, new List<Dictionary<string, object?>>(), null, candidates.ToArray(), lastEx?.Message);
    }

    private static List<string> BuildPaperIdCandidates(string? paperId, string? itemId)
    {
        var result = new List<string>();

        void Add(string? value)
        {
            var v = (value ?? string.Empty).Trim();
            if (v.Length == 0) return;
            if (!result.Any(x => string.Equals(x, v, StringComparison.OrdinalIgnoreCase)))
                result.Add(v);
        }

        // Keep legacy behavior first (table id), then try item id.
        Add(paperId);
        Add(itemId);

        return result;
    }

    // POST /api/UpdateLog/RecordChanges
    // 對應 Delphi BeforePost 中呼叫 CURdTableUpdateLogUpdate_AT000002 的邏輯
    [HttpPost("RecordChanges")]
    public async Task<IActionResult> RecordChanges([FromBody] RecordChangesRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.TableName))
            return BadRequest(new { ok = false, error = "TableName is required." });
        if (string.IsNullOrWhiteSpace(req.KeyNum))
            return BadRequest(new { ok = false, error = "KeyNum is required." });
        if (req.Changes == null || req.Changes.Count == 0)
            return Ok(new { ok = true, logged = 0 });

        var spName = "CURdTableUpdateLogUpdate";
        if (!string.IsNullOrWhiteSpace(req.SpSuffix) && SafeIdentifier.IsMatch(req.SpSuffix.Trim()))
            spName = $"{spName}_{req.SpSuffix.Trim()}";

        var userId = req.UserId ?? string.Empty;
        int logged = 0;

        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync();

        foreach (var change in req.Changes)
        {
            if (string.IsNullOrWhiteSpace(change.FieldName)) continue;

            var oldVal = string.IsNullOrWhiteSpace(change.OldValue) ? "Null" : change.OldValue;
            var newVal = string.IsNullOrWhiteSpace(change.NewValue) ? "Null" : change.NewValue;

            if (oldVal == newVal) continue;

            var difference = $"【修改欄位：{change.FieldName}，修改內容：{oldVal} → {newVal}】";

            try
            {
                await using var cmd = new SqlCommand($"exec {spName} @PaperId, @FieldName, @UserId, @Difference, @KeyNum", conn);
                cmd.Parameters.AddWithValue("@PaperId", req.TableName);
                cmd.Parameters.AddWithValue("@FieldName", change.FieldName);
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@Difference", difference);
                cmd.Parameters.AddWithValue("@KeyNum", req.KeyNum);
                await cmd.ExecuteNonQueryAsync();
                logged++;
            }
            catch (Exception ex)
            {
                // SP 可能不存在，記錄但不中斷
                return Ok(new { ok = false, error = ex.Message, logged });
            }
        }

        return Ok(new { ok = true, logged });
    }

    public class RecordChangesRequest
    {
        public string TableName { get; set; } = string.Empty;
        public string KeyNum { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public string? SpSuffix { get; set; }
        public List<FieldChange> Changes { get; set; } = new();
    }

    public class FieldChange
    {
        public string FieldName { get; set; } = string.Empty;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
    }

    private static async Task<List<Dictionary<string, object?>>> QueryRowsAsync(
        SqlConnection conn,
        string sql,
        params SqlParameter[] parameters)
    {
        var list = new List<Dictionary<string, object?>>();
        await using var cmd = new SqlCommand(sql, conn);
        if (parameters?.Length > 0) cmd.Parameters.AddRange(parameters);
        await using var rd = await cmd.ExecuteReaderAsync();
        var cols = Enumerable.Range(0, rd.FieldCount).Select(rd.GetName).ToList();
        while (await rd.ReadAsync())
        {
            var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            foreach (var c in cols)
                row[c] = rd[c] == DBNull.Value ? null : rd[c];
            list.Add(row);
        }
        return list;
    }
}
