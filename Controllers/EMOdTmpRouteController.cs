using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;

namespace PcbErpApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class EMOdTmpRouteController : ControllerBase
{
    private readonly string _connStr;
    private readonly ILogger<EMOdTmpRouteController> _logger;

    public EMOdTmpRouteController(PcbErpContext context, IConfiguration config, ILogger<EMOdTmpRouteController> logger)
    {
        _connStr = config.GetConnectionString("DefaultConnection")
            ?? context?.Database.GetDbConnection().ConnectionString
            ?? throw new InvalidOperationException("Missing connection string.");
        _logger = logger;
    }

    // ==================== Helper ====================

    private static void AddParam(SqlCommand cmd, string name, object? value)
    {
        cmd.Parameters.AddWithValue(name, value ?? DBNull.Value);
    }

    private async Task<string> LoadUserIdAsync(SqlConnection conn)
    {
        var userId = string.Empty;
        var jwtHeader = Request?.Headers["X-JWTID"].ToString();
        if (!string.IsNullOrWhiteSpace(jwtHeader) && Guid.TryParse(jwtHeader, out var jwtId))
        {
            await using var cmd = new SqlCommand(
                "SELECT UserId FROM CURdUserOnline WITH (NOLOCK) WHERE JwtId = @jwtId", conn);
            cmd.Parameters.AddWithValue("@jwtId", jwtId);
            userId = (await cmd.ExecuteScalarAsync())?.ToString() ?? string.Empty;
        }
        if (string.IsNullOrWhiteSpace(userId))
            userId = User?.Identity?.Name ?? string.Empty;
        return userId.Trim();
    }

    private static async Task<List<Dictionary<string, object?>>> ReadRowsAsync(SqlCommand cmd)
    {
        var rows = new List<Dictionary<string, object?>>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var row = new Dictionary<string, object?>();
            for (int i = 0; i < reader.FieldCount; i++)
                row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            rows.Add(row);
        }
        return rows;
    }

    // ==================== 主檔分頁查詢 ====================

    /// <summary>
    /// 分頁查詢途程主檔
    /// 對應 Delphi 的 btnOKClick (tblTmpMas 查詢)
    /// GET: api/EMOdTmpRouteMas/paged
    /// </summary>
    [HttpGet("/api/EMOdTmpRouteMas/paged")]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? TmpId = null,
        [FromQuery] string? Notes = null,
        [FromQuery] int? Status = null)
    {
        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        var where = new List<string>();
        var parameters = new List<SqlParameter>();

        if (!string.IsNullOrWhiteSpace(TmpId))
        {
            where.Add("t1.TmpId LIKE @TmpId");
            parameters.Add(new SqlParameter("@TmpId", $"%{TmpId.Trim()}%"));
        }
        if (!string.IsNullOrWhiteSpace(Notes))
        {
            where.Add("t1.Notes LIKE @Notes");
            parameters.Add(new SqlParameter("@Notes", $"%{Notes.Trim()}%"));
        }
        if (Status.HasValue && Status.Value >= 0 && Status.Value <= 1)
        {
            where.Add("t1.Status = @Status");
            parameters.Add(new SqlParameter("@Status", Status.Value));
        }

        var whereClause = where.Count > 0 ? "WHERE " + string.Join(" AND ", where) : "";

        // 取得總筆數
        var countSql = $@"SELECT COUNT(*) FROM EMOdTmpRouteMas t1 WITH (NOLOCK)
                          LEFT JOIN EMOdTmpRouteStatus t2 WITH (NOLOCK) ON t1.Status = t2.Status
                          {whereClause}";
        await using var countCmd = new SqlCommand(countSql, conn);
        foreach (var p in parameters) countCmd.Parameters.Add(new SqlParameter(p.ParameterName, p.Value));
        var totalCount = (int)(await countCmd.ExecuteScalarAsync() ?? 0);

        // 取得分頁資料
        var offset = (page - 1) * pageSize;
        var sql = $@"SELECT t1.*, t2.StatusName
                     FROM EMOdTmpRouteMas t1 WITH (NOLOCK)
                     LEFT JOIN EMOdTmpRouteStatus t2 WITH (NOLOCK) ON t1.Status = t2.Status
                     {whereClause}
                     ORDER BY t1.TmpId
                     OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        await using var dataCmd = new SqlCommand(sql, conn);
        foreach (var p in parameters) dataCmd.Parameters.Add(new SqlParameter(p.ParameterName, p.Value));
        dataCmd.Parameters.AddWithValue("@Offset", offset);
        dataCmd.Parameters.AddWithValue("@PageSize", pageSize);

        var rows = await ReadRowsAsync(dataCmd);
        return Ok(new { totalCount, data = rows });
    }

    // ==================== 途程明細 ====================

    /// <summary>
    /// 取得途程明細 (依 TmpId)
    /// 對應 Delphi 的 tblTmpDtl (EMOdTmpRouteDtl)
    /// </summary>
    [HttpGet("GetRouteDtl")]
    public async Task<IActionResult> GetRouteDtl([FromQuery] string tmpId)
    {
        if (string.IsNullOrWhiteSpace(tmpId))
            return BadRequest(new { ok = false, error = "TmpId required." });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand(
            @"SELECT t1.TmpId, t1.SerialNum, t1.ProcCode, t1.Notes, t1.FinishRate,
                     t2.ProcName
              FROM EMOdTmpRouteDtl t1 WITH (NOLOCK)
              LEFT JOIN EmodProcInfo t2 WITH (NOLOCK) ON t1.ProcCode = t2.ProcCode
              WHERE t1.TmpId = @TmpId
              ORDER BY t1.SerialNum", conn);
        AddParam(cmd, "@TmpId", tmpId.Trim());

        var rows = await ReadRowsAsync(cmd);
        return Ok(new { ok = true, data = rows });
    }

    // ==================== 製程資料查詢 (供選擇使用) ====================

    /// <summary>
    /// 查詢製程基本資料 (供 TdlgTmpRouteSet 選擇製程)
    /// 對應 Delphi 的 qryProcBasic
    /// </summary>
    [HttpGet("GetProcList")]
    public async Task<IActionResult> GetProcList(
        [FromQuery] string? bProc = null,
        [FromQuery] string? eProc = null,
        [FromQuery] string? procLike = null,
        [FromQuery] string? procNameLike = null)
    {
        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        var where = new List<string>();
        var parameters = new List<SqlParameter>();

        if (!string.IsNullOrWhiteSpace(bProc))
        {
            where.Add("ProcCode >= @BProc");
            parameters.Add(new SqlParameter("@BProc", bProc.Trim()));
        }
        if (!string.IsNullOrWhiteSpace(eProc))
        {
            where.Add("ProcCode <= @EProc");
            parameters.Add(new SqlParameter("@EProc", eProc.Trim()));
        }
        if (!string.IsNullOrWhiteSpace(procLike))
        {
            where.Add("ProcCode LIKE @ProcLike");
            parameters.Add(new SqlParameter("@ProcLike", $"%{procLike.Trim()}%"));
        }
        if (!string.IsNullOrWhiteSpace(procNameLike))
        {
            where.Add("ProcName LIKE @ProcNameLike");
            parameters.Add(new SqlParameter("@ProcNameLike", $"%{procNameLike.Trim()}%"));
        }

        var whereClause = where.Count > 0 ? "WHERE " + string.Join(" AND ", where) : "";
        var sql = $"SELECT ProcCode, ProcName FROM EmodProcInfo WITH (NOLOCK) {whereClause} ORDER BY ProcCode";

        await using var cmd = new SqlCommand(sql, conn);
        foreach (var p in parameters) cmd.Parameters.Add(new SqlParameter(p.ParameterName, p.Value));

        var rows = await ReadRowsAsync(cmd);
        return Ok(new { ok = true, data = rows });
    }

    // ==================== 異動途程 ====================

    public class ChangeRouteRequest
    {
        public string TmpId { get; set; } = "";
        public List<string> ProcCodes { get; set; } = new();
    }

    /// <summary>
    /// 異動途程明細 (對應 Delphi btChangeClick)
    /// 呼叫 EMOdTmpRouteHoldNote 鎖定 → 刪舊明細 → 逐筆 INSERT → 解鎖
    /// </summary>
    [HttpPost("ChangeRoute")]
    public async Task<IActionResult> ChangeRoute([FromBody] ChangeRouteRequest req)
    {
        if (req == null || string.IsNullOrWhiteSpace(req.TmpId))
            return BadRequest(new { ok = false, error = "TmpId required." });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        // 確認主檔狀態
        await using (var chk = new SqlCommand(
            "SELECT Status FROM EMOdTmpRouteMas WITH (NOLOCK) WHERE TmpId = @TmpId", conn))
        {
            AddParam(chk, "@TmpId", req.TmpId.Trim());
            var status = await chk.ExecuteScalarAsync();
            if (status != null && Convert.ToInt32(status) == 1)
                return Ok(new { ok = false, error = "此途程範本已使用中，不可修改!" });
        }

        try
        {
            // 鎖定 (HoldNote = 0)
            await using (var holdCmd = new SqlCommand(
                $"exec EMOdTmpRouteHoldNote '{req.TmpId.Trim().Replace("'", "''")}', 0", conn))
            {
                await holdCmd.ExecuteNonQueryAsync();
            }

            // 刪除舊明細 (status=0 才刪)
            await using (var delCmd = new SqlCommand(
                @"DELETE dbo.EMOdTmpRouteDtl FROM EMOdTmpRouteMas t1 WITH (NOLOCK), EMOdTmpRouteDtl t2
                  WHERE t1.TmpId = @TmpId AND t1.TmpId = t2.TmpId AND t1.Status = 0", conn))
            {
                AddParam(delCmd, "@TmpId", req.TmpId.Trim());
                await delCmd.ExecuteNonQueryAsync();
            }

            // 逐筆插入新明細
            for (int i = 0; i < req.ProcCodes.Count; i++)
            {
                await using var insCmd = new SqlCommand(
                    @"IF (SELECT Status FROM dbo.EMOdTmpRouteMas WITH (NOLOCK) WHERE TmpId = @TmpId) = 0
                      BEGIN
                        INSERT INTO dbo.EMOdTmpRouteDtl (TmpId, SerialNum, ProcCode, FinishRate)
                        VALUES (@TmpId, @SerialNum, @ProcCode, 1)
                      END", conn);
                AddParam(insCmd, "@TmpId", req.TmpId.Trim());
                AddParam(insCmd, "@SerialNum", i + 1);
                AddParam(insCmd, "@ProcCode", req.ProcCodes[i]);
                await insCmd.ExecuteNonQueryAsync();
            }

            // 解鎖 (HoldNote = 1)
            await using (var releaseCmd = new SqlCommand(
                $"exec EMOdTmpRouteHoldNote '{req.TmpId.Trim().Replace("'", "''")}', 1", conn))
            {
                await releaseCmd.ExecuteNonQueryAsync();
            }

            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ChangeRoute failed for TmpId={TmpId}", req.TmpId);
            return StatusCode(500, new { ok = false, error = ex.Message });
        }
    }

    // ==================== 另存新檔 ====================

    public class SaveAsRequest
    {
        public string SourceTmpId { get; set; } = "";
        public string NewTmpId { get; set; } = "";
        public List<string> ProcCodes { get; set; } = new();
    }

    /// <summary>
    /// 另存途程範本 (對應 Delphi btSaveAsClick)
    /// </summary>
    [HttpPost("SaveAs")]
    public async Task<IActionResult> SaveAs([FromBody] SaveAsRequest req)
    {
        if (req == null || string.IsNullOrWhiteSpace(req.NewTmpId))
            return BadRequest(new { ok = false, error = "NewTmpId required." });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        // 檢查新代碼是否已存在
        await using (var chk = new SqlCommand(
            "SELECT COUNT(*) FROM EMOdTmpRouteMas WITH (NOLOCK) WHERE TmpId = @TmpId", conn))
        {
            AddParam(chk, "@TmpId", req.NewTmpId.Trim());
            var cnt = (int)(await chk.ExecuteScalarAsync() ?? 0);
            if (cnt > 0)
                return Ok(new { ok = false, error = "已存在該代碼!" });
        }

        await using var tran = conn.BeginTransaction();
        try
        {
            // 新增主檔
            await using (var insCmd = new SqlCommand(
                "INSERT INTO EMOdTmpRouteMas (TmpId, Notes, Status, IsStop) SELECT @NewTmpId, '', 0, 0", conn, tran))
            {
                AddParam(insCmd, "@NewTmpId", req.NewTmpId.Trim());
                await insCmd.ExecuteNonQueryAsync();
            }

            // 逐筆插入明細
            for (int i = 0; i < req.ProcCodes.Count; i++)
            {
                await using var dtlCmd = new SqlCommand(
                    @"IF (SELECT Status FROM dbo.EMOdTmpRouteMas WITH (NOLOCK) WHERE TmpId = @TmpId) = 0
                      BEGIN
                        INSERT INTO dbo.EMOdTmpRouteDtl (TmpId, SerialNum, ProcCode, FinishRate)
                        VALUES (@TmpId, @SerialNum, @ProcCode, 1)
                      END", conn, tran);
                AddParam(dtlCmd, "@TmpId", req.NewTmpId.Trim());
                AddParam(dtlCmd, "@SerialNum", i + 1);
                AddParam(dtlCmd, "@ProcCode", req.ProcCodes[i]);
                await dtlCmd.ExecuteNonQueryAsync();
            }

            await tran.CommitAsync();

            // 更新 UserId
            var userId = await LoadUserIdAsync(conn);
            await using var uidCmd = new SqlCommand("EMOdTmpUpdateUserId", conn);
            uidCmd.CommandType = CommandType.StoredProcedure;
            uidCmd.CommandTimeout = 30;
            AddParam(uidCmd, "@TmpId", req.NewTmpId.Trim());
            AddParam(uidCmd, "@UserId", userId);
            AddParam(uidCmd, "@TableName", "EMOdTmpRouteMas");
            await uidCmd.ExecuteNonQueryAsync();

            return Ok(new { ok = true });
        }
        catch (SqlException ex)
        {
            await tran.RollbackAsync();
            _logger.LogError(ex, "SaveAs failed: {Src} -> {New}", req.SourceTmpId, req.NewTmpId);
            var msg = ex.Number == 2627 || ex.Number == 2601
                ? $"代碼 [{req.NewTmpId.Trim()}] 已存在，無法複製"
                : ex.Message;
            return Ok(new { ok = false, error = msg });
        }
        catch (Exception ex)
        {
            await tran.RollbackAsync();
            _logger.LogError(ex, "SaveAs failed for NewTmpId={NewTmpId}", req.NewTmpId);
            return StatusCode(500, new { ok = false, error = ex.Message });
        }
    }

    // ==================== 新增 / 刪除主檔 ====================

    public class InsertMasterRequest
    {
        public string TmpId { get; set; } = "";
        public string Notes { get; set; } = "";
    }

    /// <summary>
    /// 新增途程主檔
    /// </summary>
    [HttpPost("InsertMaster")]
    public async Task<IActionResult> InsertMaster([FromBody] InsertMasterRequest req)
    {
        if (req == null || string.IsNullOrWhiteSpace(req.TmpId))
            return BadRequest(new { ok = false, error = "TmpId required." });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        // 檢查是否已存在
        await using (var chk = new SqlCommand(
            "SELECT COUNT(*) FROM EMOdTmpRouteMas WITH (NOLOCK) WHERE TmpId = @TmpId", conn))
        {
            AddParam(chk, "@TmpId", req.TmpId.Trim());
            var cnt = (int)(await chk.ExecuteScalarAsync() ?? 0);
            if (cnt > 0)
                return Ok(new { ok = false, error = "此代碼已存在" });
        }

        var userId = await LoadUserIdAsync(conn);

        try
        {
            await using var cmd = new SqlCommand(
                "INSERT INTO EMOdTmpRouteMas (TmpId, Notes, Status, IsStop) VALUES (@TmpId, @Notes, 0, 0)", conn);
            AddParam(cmd, "@TmpId", req.TmpId.Trim());
            AddParam(cmd, "@Notes", req.Notes?.Trim() ?? "");
            await cmd.ExecuteNonQueryAsync();

            // 更新 UserId
            await using var uidCmd = new SqlCommand("EMOdTmpUpdateUserId", conn);
            uidCmd.CommandType = CommandType.StoredProcedure;
            uidCmd.CommandTimeout = 30;
            AddParam(uidCmd, "@TmpId", req.TmpId.Trim());
            AddParam(uidCmd, "@UserId", userId);
            AddParam(uidCmd, "@TableName", "EMOdTmpRouteMas");
            await uidCmd.ExecuteNonQueryAsync();
        }
        catch (SqlException ex)
        {
            var msg = ex.Number == 2627 || ex.Number == 2601
                ? $"代碼 [{req.TmpId.Trim()}] 已存在"
                : ex.Message;
            return Ok(new { ok = false, error = msg });
        }

        return Ok(new { ok = true });
    }

    public class DeleteMasterRequest
    {
        public string TmpId { get; set; } = "";
    }

    /// <summary>
    /// 刪除途程主檔 (對應 Delphi tblTmpMasBeforeDelete)
    /// </summary>
    [HttpPost("DeleteMaster")]
    public async Task<IActionResult> DeleteMaster([FromBody] DeleteMasterRequest req)
    {
        if (req == null || string.IsNullOrWhiteSpace(req.TmpId))
            return BadRequest(new { ok = false, error = "TmpId required." });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        // 檢查狀態 (Status=1 已使用，不可刪除)
        await using (var chk = new SqlCommand(
            "SELECT Status FROM EMOdTmpRouteMas WITH (NOLOCK) WHERE TmpId = @TmpId", conn))
        {
            AddParam(chk, "@TmpId", req.TmpId.Trim());
            var status = await chk.ExecuteScalarAsync();
            if (status != null && Convert.ToInt32(status) == 1)
                return Ok(new { ok = false, error = "此途程範本已使用中，不可刪除!" });
        }

        try
        {
            await using var cmd = new SqlCommand(
                "DELETE EMOdTmpRouteMas WHERE TmpId = @TmpId", conn);
            AddParam(cmd, "@TmpId", req.TmpId.Trim());
            await cmd.ExecuteNonQueryAsync();
            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteMaster failed for TmpId={TmpId}", req.TmpId);
            return StatusCode(500, new { ok = false, error = ex.Message });
        }
    }

    // ==================== 更新主檔欄位 ====================

    public class UpdateMasterFieldRequest
    {
        public string TmpId { get; set; } = "";
        public string FieldName { get; set; } = "";
        public string? Value { get; set; }
    }

    /// <summary>
    /// 更新主檔單一欄位 (行內編輯 Notes)
    /// </summary>
    [HttpPost("UpdateMasterField")]
    public async Task<IActionResult> UpdateMasterField([FromBody] UpdateMasterFieldRequest req)
    {
        if (req == null || string.IsNullOrWhiteSpace(req.TmpId) || string.IsNullOrWhiteSpace(req.FieldName))
            return BadRequest(new { ok = false, error = "TmpId and FieldName required." });

        var allowedFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Notes" };
        if (!allowedFields.Contains(req.FieldName))
            return BadRequest(new { ok = false, error = $"Field '{req.FieldName}' is not editable." });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        // 檢查狀態
        await using (var chk = new SqlCommand(
            "SELECT Status FROM EMOdTmpRouteMas WITH (NOLOCK) WHERE TmpId = @TmpId", conn))
        {
            AddParam(chk, "@TmpId", req.TmpId.Trim());
            var status = await chk.ExecuteScalarAsync();
            if (status != null && Convert.ToInt32(status) == 1)
                return Ok(new { ok = false, error = "此途程範本已使用中，不可修改!" });
        }

        var sql = $"UPDATE EMOdTmpRouteMas SET [{req.FieldName}] = @Value WHERE TmpId = @TmpId";
        await using var cmd = new SqlCommand(sql, conn);
        AddParam(cmd, "@TmpId", req.TmpId.Trim());
        AddParam(cmd, "@Value", req.Value?.Trim() ?? "");
        await cmd.ExecuteNonQueryAsync();

        return Ok(new { ok = true });
    }

    // ==================== 更新明細 Notes ====================

    public class UpdateDtlNoteRequest
    {
        public string TmpId { get; set; } = "";
        public int SerialNum { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// 更新途程明細的 Notes (DBMemo)
    /// 對應 Delphi 的 DBMemo1 直接編輯
    /// </summary>
    [HttpPost("UpdateDtlNote")]
    public async Task<IActionResult> UpdateDtlNote([FromBody] UpdateDtlNoteRequest req)
    {
        if (req == null || string.IsNullOrWhiteSpace(req.TmpId))
            return BadRequest(new { ok = false, error = "TmpId required." });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        // 檢查主檔狀態
        await using (var chk = new SqlCommand(
            "SELECT Status FROM EMOdTmpRouteMas WITH (NOLOCK) WHERE TmpId = @TmpId", conn))
        {
            AddParam(chk, "@TmpId", req.TmpId.Trim());
            var status = await chk.ExecuteScalarAsync();
            if (status != null && Convert.ToInt32(status) == 1)
                return Ok(new { ok = false, error = "此途程範本已使用中，不可修改!" });
        }

        await using var cmd = new SqlCommand(
            "UPDATE EMOdTmpRouteDtl SET Notes = @Notes WHERE TmpId = @TmpId AND SerialNum = @SerialNum", conn);
        AddParam(cmd, "@TmpId", req.TmpId.Trim());
        AddParam(cmd, "@SerialNum", req.SerialNum);
        AddParam(cmd, "@Notes", req.Notes ?? "");
        await cmd.ExecuteNonQueryAsync();

        return Ok(new { ok = true });
    }

    // ==================== 送審 / 退審 ====================

    public class ApprovalPostRequest
    {
        public string TmpId { get; set; } = "";
        public int IsPost { get; set; } // 1=送審, 0=退審
    }

    /// <summary>
    /// 送審/退審 (對應 Delphi btnC1Click)
    /// </summary>
    [HttpPost("ApprovalPost")]
    public async Task<IActionResult> ApprovalPost([FromBody] ApprovalPostRequest req)
    {
        if (req == null || string.IsNullOrWhiteSpace(req.TmpId))
            return BadRequest(new { ok = false, error = "TmpId required." });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        var userId = await LoadUserIdAsync(conn);

        try
        {
            await using var cmd = new SqlCommand("EMOdTmpRoutePost", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 30;
            AddParam(cmd, "@TmpId", req.TmpId.Trim());
            AddParam(cmd, "@IsPost", req.IsPost);
            AddParam(cmd, "@Source", "EMOdTmpRouteMas");
            AddParam(cmd, "@UserId", userId);

            var rows = await ReadRowsAsync(cmd);

            if (rows.Count > 0 && rows[0].ContainsKey("ResultStr"))
            {
                var resultStr = rows[0]["ResultStr"]?.ToString() ?? "";
                if (!string.IsNullOrEmpty(resultStr))
                    return Ok(new { ok = false, error = resultStr });
            }

            return Ok(new { ok = true });
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "ApprovalPost failed for TmpId={TmpId}, IsPost={IsPost}", req.TmpId, req.IsPost);
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    // ==================== 欄位辭典 ====================

    /// <summary>
    /// 取得欄位辭典
    /// </summary>
    [HttpGet("DictFields")]
    public async Task<IActionResult> GetDictFields([FromQuery] string table)
    {
        if (string.IsNullOrWhiteSpace(table))
            return BadRequest(new { ok = false, error = "table required." });

        var tname = table.Trim().TrimStart('[').TrimEnd(']')
            .Replace("dbo.", "", StringComparison.OrdinalIgnoreCase)
            .ToLowerInvariant();

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        const string sql = @"
SELECT f.FieldName,
       COALESCE(l.DisplayLabel, f.DisplayLabel, f.FieldName) AS DisplayLabel,
       COALESCE(l.DisplaySize, f.DisplaySize) AS DisplaySize,
       COALESCE(l.IFieldWidth, f.iFieldWidth) AS FieldWidth,
       f.ComboStyle, f.DataType, f.FormatStr, f.SerialNum,
       Visible = CASE WHEN f.Visible=1 THEN 1 ELSE 0 END
FROM CURdTableField f WITH (NOLOCK)
LEFT JOIN CURdTableFieldLang l WITH (NOLOCK)
       ON l.TableName = f.TableName AND l.FieldName = f.FieldName AND l.LanguageId = 'TW'
WHERE (LOWER(f.TableName)=@TN OR LOWER(REPLACE(f.TableName,'dbo.',''))=@TN)
ORDER BY CASE WHEN f.SerialNum IS NULL THEN 1 ELSE 0 END, f.SerialNum, f.FieldName";

        await using var cmd = new SqlCommand(sql, conn);
        AddParam(cmd, "@TN", tname);

        var list = new List<object>();
        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            list.Add(new
            {
                FieldName    = rd["FieldName"]?.ToString() ?? "",
                DisplayLabel = rd["DisplayLabel"]?.ToString() ?? "",
                DisplaySize  = rd.IsDBNull(rd.GetOrdinal("DisplaySize")) ? (int?)null : Convert.ToInt32(rd["DisplaySize"]),
                FieldWidth   = rd.IsDBNull(rd.GetOrdinal("FieldWidth")) ? (int?)null : Convert.ToInt32(rd["FieldWidth"]),
                ComboStyle   = rd.IsDBNull(rd.GetOrdinal("ComboStyle")) ? (int?)null : Convert.ToInt32(rd["ComboStyle"]),
                DataType     = rd["DataType"]?.ToString() ?? "",
                FormatStr    = rd["FormatStr"]?.ToString() ?? "",
                SerialNum    = rd.IsDBNull(rd.GetOrdinal("SerialNum")) ? (int?)null : Convert.ToInt32(rd["SerialNum"]),
                Visible      = (rd["Visible"]?.ToString() ?? "0") == "1"
            });
        }

        return Ok(list);
    }

    // ==================== 系統參數 ====================

    /// <summary>
    /// 取得系統參數
    /// </summary>
    [HttpGet("SysParam")]
    public async Task<IActionResult> GetSysParam([FromQuery] string systemId, [FromQuery] string paramId)
    {
        if (string.IsNullOrWhiteSpace(systemId) || string.IsNullOrWhiteSpace(paramId))
            return BadRequest(new { ok = false, error = "SystemId and ParamId required." });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand(
            "SELECT Value FROM CURdSysParams WITH (NOLOCK) WHERE SystemId = @SystemId AND ParamId = @ParamId", conn);
        AddParam(cmd, "@SystemId", systemId.Trim());
        AddParam(cmd, "@ParamId", paramId.Trim());

        var value = (await cmd.ExecuteScalarAsync())?.ToString() ?? "";
        return Ok(new { ok = true, value });
    }
}