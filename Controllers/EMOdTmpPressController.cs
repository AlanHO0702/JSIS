using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;

namespace PcbErpApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class EMOdTmpPressController : ControllerBase
{
    private readonly string _connStr;
    private readonly ILogger<EMOdTmpPressController> _logger;

    public EMOdTmpPressController(PcbErpContext context, IConfiguration config, ILogger<EMOdTmpPressController> logger)
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
    /// 分頁查詢壓合疊構主檔
    /// GET: api/EMOdTmpPressMas/paged
    /// </summary>
    [HttpGet("/api/EMOdTmpPressMas/paged")]
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
            where.Add("TmpId LIKE @TmpId");
            parameters.Add(new SqlParameter("@TmpId", $"%{TmpId.Trim()}%"));
        }
        if (!string.IsNullOrWhiteSpace(Notes))
        {
            where.Add("Notes LIKE @Notes");
            parameters.Add(new SqlParameter("@Notes", $"%{Notes.Trim()}%"));
        }
        if (Status.HasValue && Status.Value >= 0 && Status.Value <= 1)
        {
            where.Add("Status = @Status");
            parameters.Add(new SqlParameter("@Status", Status.Value));
        }

        var whereClause = where.Count > 0 ? "WHERE " + string.Join(" AND ", where) : "";

        // 取得總筆數
        await using var countCmd = new SqlCommand(
            $"SELECT COUNT(*) FROM EMOdTmpPressMas WITH (NOLOCK) {whereClause}", conn);
        foreach (var p in parameters) countCmd.Parameters.Add(new SqlParameter(p.ParameterName, p.Value));
        var totalCount = (int)(await countCmd.ExecuteScalarAsync() ?? 0);

        // 取得分頁資料
        var offset = (page - 1) * pageSize;
        var sql = $@"SELECT * FROM EMOdTmpPressMas WITH (NOLOCK) {whereClause}
                     ORDER BY TmpId
                     OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        await using var dataCmd = new SqlCommand(sql, conn);
        foreach (var p in parameters) dataCmd.Parameters.Add(new SqlParameter(p.ParameterName, p.Value));
        dataCmd.Parameters.AddWithValue("@Offset", offset);
        dataCmd.Parameters.AddWithValue("@PageSize", pageSize);

        var rows = await ReadRowsAsync(dataCmd);
        return Ok(new { totalCount, data = rows });
    }

    // ==================== BOM 疊構 TreeView ====================

    /// <summary>
    /// 取得壓合疊構 BOM 明細 (供 TreeView 使用)
    /// 對應 Delphi 的 tblTmpBomDtl (EMOdTmpBOMDtl)
    /// </summary>
    [HttpGet("GetBOMDtl")]
    public async Task<IActionResult> GetBOMDtl([FromQuery] string tmpBOMId)
    {
        if (string.IsNullOrWhiteSpace(tmpBOMId))
            return BadRequest(new { ok = false, error = "TmpBOMId required." });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand(
            @"SELECT TmpId, LayerId, LayerName, AftLayerId, IssLayer, Degree, FL, EL,
                     Sort = Degree * 100 + FL
              FROM dbo.EMOdTmpBOMDtl WITH (NOLOCK)
              WHERE TmpId = @TmpId
              ORDER BY Degree, FL", conn);
        AddParam(cmd, "@TmpId", tmpBOMId.Trim());

        var rows = await ReadRowsAsync(cmd);
        return Ok(new { ok = true, data = rows });
    }

    // ==================== 壓合明細 ====================

    /// <summary>
    /// 取得壓合明細 (依 TmpId + LayerId)
    /// 對應 Delphi 的 tblTmpDtl (EMOdTmpPressDtl)
    /// </summary>
    [HttpGet("GetPressDtl")]
    public async Task<IActionResult> GetPressDtl([FromQuery] string tmpId, [FromQuery] string layerId)
    {
        if (string.IsNullOrWhiteSpace(tmpId))
            return BadRequest(new { ok = false, error = "TmpId required." });
        if (string.IsNullOrWhiteSpace(layerId))
            return BadRequest(new { ok = false, error = "LayerId required." });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand(
            @"SELECT t1.*
              FROM EMOdTmpPressDtl t1 WITH (NOLOCK)
              WHERE t1.TmpId = @TmpId AND t1.LayerId = @LayerId
              ORDER BY t1.SerialNum", conn);
        AddParam(cmd, "@TmpId", tmpId.Trim());
        AddParam(cmd, "@LayerId", layerId.Trim());

        var rows = await ReadRowsAsync(cmd);
        return Ok(new { ok = true, data = rows });
    }

    // ==================== 壓合材料設定 (異動) ====================

    /// <summary>
    /// 取得壓合預設值 (呼叫 EMOdPressDefault)
    /// 對應 Delphi 的 qryTmpPressDtl
    /// </summary>
    [HttpGet("GetPressDefault")]
    public async Task<IActionResult> GetPressDefault([FromQuery] string tmpId, [FromQuery] string layerId)
    {
        if (string.IsNullOrWhiteSpace(tmpId) || string.IsNullOrWhiteSpace(layerId))
            return BadRequest(new { ok = false, error = "TmpId and LayerId required." });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand("EMOdPressDefault", conn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandTimeout = 30;
        AddParam(cmd, "@TmpId", tmpId.Trim());
        AddParam(cmd, "@LayerId", layerId.Trim());

        var rows = await ReadRowsAsync(cmd);
        return Ok(new { ok = true, data = rows });
    }

    /// <summary>
    /// 取得物料類別清單 (呼叫 EMOdMatClassSelectEx)
    /// 對應 Delphi 的 qryMatClass
    /// </summary>
    [HttpGet("GetMatClass")]
    public async Task<IActionResult> GetMatClass()
    {
        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand("EMOdMatClassSelectEx", conn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandTimeout = 30;
        AddParam(cmd, "@Flag", 1);

        var rows = await ReadRowsAsync(cmd);
        return Ok(new { ok = true, data = rows });
    }

    /// <summary>
    /// 取得產品層別清單
    /// 對應 Delphi 的 qryProdLayer
    /// </summary>
    [HttpGet("GetProdLayer")]
    public async Task<IActionResult> GetProdLayer()
    {
        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand(
            "SELECT DISTINCT LayerId, LayerName FROM dbo.EMOdProdLayer WITH (NOLOCK)", conn);

        var rows = await ReadRowsAsync(cmd);
        return Ok(new { ok = true, data = rows });
    }

    /// <summary>
    /// 取得物料分類對照 (MINdMatClass)
    /// 對應 Delphi 的 qryMatClass Lookup
    /// </summary>
    [HttpGet("GetMatClassName")]
    public async Task<IActionResult> GetMatClassName()
    {
        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand(
            "SELECT MatClass, ClassName FROM MINdMatClass WITH (NOLOCK)", conn);

        var rows = await ReadRowsAsync(cmd);
        return Ok(new { ok = true, data = rows });
    }

    /// <summary>
    /// 取得 BOM 壓合名稱 (EMOdVBOMPressName)
    /// 對應 Delphi 的 qryDtlBOMName Lookup
    /// </summary>
    [HttpGet("GetBOMPressName")]
    public async Task<IActionResult> GetBOMPressName()
    {
        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand(
            "SELECT LayerId, LayerName FROM EMOdVBOMPressName WITH (NOLOCK)", conn);

        var rows = await ReadRowsAsync(cmd);
        return Ok(new { ok = true, data = rows });
    }

    // ==================== 儲存壓合材料 ====================

    public class SavePressDtlRequest
    {
        public string TmpId { get; set; } = "";
        public string LayerId { get; set; } = "";
        public List<PressDtlItem> Items { get; set; } = new();
    }

    public class PressDtlItem
    {
        public string MatClass { get; set; } = "";
        public string MatName { get; set; } = "";
    }

    /// <summary>
    /// 儲存壓合明細 (刪除指定 TmpId+LayerId 後重新插入)
    /// 對應 Delphi 的 btChangeClick
    /// </summary>
    [HttpPost("SavePressDtl")]
    public async Task<IActionResult> SavePressDtl([FromBody] SavePressDtlRequest req)
    {
        if (req == null || string.IsNullOrWhiteSpace(req.TmpId) || string.IsNullOrWhiteSpace(req.LayerId))
            return BadRequest(new { ok = false, error = "TmpId and LayerId required." });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();
        await using var tran = conn.BeginTransaction();

        try
        {
            // 刪除該層的舊明細
            await using (var delCmd = new SqlCommand(
                "DELETE dbo.EMOdTmpPressDtl WHERE TmpId = @TmpId AND LayerId = @LayerId", conn, tran))
            {
                AddParam(delCmd, "@TmpId", req.TmpId.Trim());
                AddParam(delCmd, "@LayerId", req.LayerId.Trim());
                await delCmd.ExecuteNonQueryAsync();
            }

            // 逐筆插入新明細
            for (int i = 0; i < req.Items.Count; i++)
            {
                await using var insCmd = new SqlCommand("EMOdTmpPressDtlIns", conn, tran);
                insCmd.CommandType = CommandType.StoredProcedure;
                insCmd.CommandTimeout = 30;
                AddParam(insCmd, "@TmpId", req.TmpId.Trim());
                AddParam(insCmd, "@LayerId", req.LayerId.Trim());
                AddParam(insCmd, "@SerialNum", i + 1);
                AddParam(insCmd, "@MatClass", req.Items[i].MatClass);
                AddParam(insCmd, "@MatName", req.Items[i].MatName);
                await insCmd.ExecuteNonQueryAsync();
            }

            await tran.CommitAsync();

            // 驗證壓合設定
            await using var chkCmd = new SqlCommand("EMOdCheckTmpPressSet", conn);
            chkCmd.CommandType = CommandType.StoredProcedure;
            chkCmd.CommandTimeout = 30;
            AddParam(chkCmd, "@TmpId", req.TmpId.Trim());
            await chkCmd.ExecuteNonQueryAsync();

            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            await tran.RollbackAsync();
            _logger.LogError(ex, "SavePressDtl failed for TmpId={TmpId}, LayerId={LayerId}", req.TmpId, req.LayerId);
            return StatusCode(500, new { ok = false, error = ex.Message });
        }
    }

    // ==================== 變更組合模型 ====================

    public class ChangeRouteRequest
    {
        public string TmpId { get; set; } = "";
        public string TmpBOMId { get; set; } = "";
    }

    /// <summary>
    /// 變更組合模型 (更新 TmpBOMId 並呼叫 EMOdTmpPressByBOM)
    /// 對應 Delphi 的 btnChangeRouteClick
    /// </summary>
    [HttpPost("ChangeRoute")]
    public async Task<IActionResult> ChangeRoute([FromBody] ChangeRouteRequest req)
    {
        if (req == null || string.IsNullOrWhiteSpace(req.TmpId) || string.IsNullOrWhiteSpace(req.TmpBOMId))
            return BadRequest(new { ok = false, error = "TmpId and TmpBOMId required." });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();
        await using var tran = conn.BeginTransaction();

        try
        {
            // 更新主檔 TmpBOMId
            await using (var updCmd = new SqlCommand(
                "UPDATE EMOdTmpPressMas SET TmpBOMId = @TmpBOMId WHERE TmpId = @TmpId", conn, tran))
            {
                AddParam(updCmd, "@TmpId", req.TmpId.Trim());
                AddParam(updCmd, "@TmpBOMId", req.TmpBOMId.Trim());
                await updCmd.ExecuteNonQueryAsync();
            }

            // 呼叫 EMOdTmpPressByBOM 更新壓合明細
            await using (var spCmd = new SqlCommand("EMOdTmpPressByBOM", conn, tran))
            {
                spCmd.CommandType = CommandType.StoredProcedure;
                spCmd.CommandTimeout = 30;
                AddParam(spCmd, "@TmpId", req.TmpId.Trim());
                await spCmd.ExecuteNonQueryAsync();
            }

            await tran.CommitAsync();
            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            await tran.RollbackAsync();
            _logger.LogError(ex, "ChangeRoute failed for TmpId={TmpId}", req.TmpId);
            return StatusCode(500, new { ok = false, error = ex.Message });
        }
    }

    // ==================== 另存 ====================

    public class SaveAsRequest
    {
        public string SourceTmpId { get; set; } = "";
        public string NewTmpId { get; set; } = "";
        public string LayerId { get; set; } = "";
        public List<PressDtlItem> Items { get; set; } = new();
    }

    /// <summary>
    /// 另存壓合模板 (複製主檔 + 插入新明細)
    /// 對應 Delphi 的 btSaveAsClick
    /// </summary>
    [HttpPost("SaveAs")]
    public async Task<IActionResult> SaveAs([FromBody] SaveAsRequest req)
    {
        if (req == null || string.IsNullOrWhiteSpace(req.SourceTmpId) || string.IsNullOrWhiteSpace(req.NewTmpId))
            return BadRequest(new { ok = false, error = "SourceTmpId and NewTmpId required." });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        // 檢查是否已存在
        await using (var chk = new SqlCommand(
            "SELECT COUNT(*) FROM EMOdTmpPressMas WITH (NOLOCK) WHERE TmpId = @TmpId", conn))
        {
            AddParam(chk, "@TmpId", req.NewTmpId.Trim());
            var cnt = (int)(await chk.ExecuteScalarAsync() ?? 0);
            if (cnt > 0)
                return Ok(new { ok = false, error = "已存在該代碼!" });
        }

        await using var tran = conn.BeginTransaction();

        try
        {
            // 從原始主檔複製
            await using (var insCmd = new SqlCommand(
                @"INSERT INTO EMOdTmpPressMas (TmpId, TmpBOMId, Notes, Status)
                  SELECT @NewTmpId, TmpBOMId, Notes, 0
                  FROM EMOdTmpPressMas WITH (NOLOCK)
                  WHERE TmpId = @SourceTmpId", conn, tran))
            {
                AddParam(insCmd, "@NewTmpId", req.NewTmpId.Trim());
                AddParam(insCmd, "@SourceTmpId", req.SourceTmpId.Trim());
                await insCmd.ExecuteNonQueryAsync();
            }

            // 插入壓合明細
            if (!string.IsNullOrWhiteSpace(req.LayerId) && req.Items.Count > 0)
            {
                for (int i = 0; i < req.Items.Count; i++)
                {
                    await using var dtlCmd = new SqlCommand("EMOdTmpPressDtlIns", conn, tran);
                    dtlCmd.CommandType = CommandType.StoredProcedure;
                    dtlCmd.CommandTimeout = 30;
                    AddParam(dtlCmd, "@TmpId", req.NewTmpId.Trim());
                    AddParam(dtlCmd, "@LayerId", req.LayerId.Trim());
                    AddParam(dtlCmd, "@SerialNum", i + 1);
                    AddParam(dtlCmd, "@MatClass", req.Items[i].MatClass);
                    AddParam(dtlCmd, "@MatName", req.Items[i].MatName);
                    await dtlCmd.ExecuteNonQueryAsync();
                }
            }

            await tran.CommitAsync();

            // 驗證壓合設定
            await using var chkCmd = new SqlCommand("EMOdCheckTmpPressSet", conn);
            chkCmd.CommandType = CommandType.StoredProcedure;
            chkCmd.CommandTimeout = 30;
            AddParam(chkCmd, "@TmpId", req.NewTmpId.Trim());
            await chkCmd.ExecuteNonQueryAsync();

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
    }

    // ==================== 新增 / 刪除主檔 ====================

    public class InsertMasterRequest
    {
        public string TmpId { get; set; } = "";
        public string Notes { get; set; } = "";
    }

    /// <summary>
    /// 新增壓合主檔
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
            "SELECT COUNT(*) FROM EMOdTmpPressMas WITH (NOLOCK) WHERE TmpId = @TmpId", conn))
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
                @"INSERT INTO EMOdTmpPressMas (TmpId, TmpBOMId, Notes, Status)
                  VALUES (@TmpId, '----', @Notes, 0)", conn);
            AddParam(cmd, "@TmpId", req.TmpId.Trim());
            AddParam(cmd, "@Notes", req.Notes?.Trim() ?? "");
            await cmd.ExecuteNonQueryAsync();

            // 更新 UserId
            await using var uidCmd = new SqlCommand("EMOdTmpUpdateUserId", conn);
            uidCmd.CommandType = CommandType.StoredProcedure;
            uidCmd.CommandTimeout = 30;
            AddParam(uidCmd, "@TmpId", req.TmpId.Trim());
            AddParam(uidCmd, "@UserId", userId);
            AddParam(uidCmd, "@TableName", "EMOdTmpPressMas");
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
    /// 刪除壓合主檔及其明細
    /// </summary>
    [HttpPost("DeleteMaster")]
    public async Task<IActionResult> DeleteMaster([FromBody] DeleteMasterRequest req)
    {
        if (req == null || string.IsNullOrWhiteSpace(req.TmpId))
            return BadRequest(new { ok = false, error = "TmpId required." });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        // 檢查狀態
        await using (var chk = new SqlCommand(
            "SELECT Status FROM EMOdTmpPressMas WITH (NOLOCK) WHERE TmpId = @TmpId", conn))
        {
            AddParam(chk, "@TmpId", req.TmpId.Trim());
            var status = await chk.ExecuteScalarAsync();
            if (status != null && Convert.ToInt32(status) == 1)
                return Ok(new { ok = false, error = "此範本已使用中，不可刪除!" });
        }

        await using var tran = conn.BeginTransaction();

        try
        {
            // 先刪明細
            await using (var delDtl = new SqlCommand(
                "DELETE FROM EMOdTmpPressDtl WHERE TmpId = @TmpId", conn, tran))
            {
                AddParam(delDtl, "@TmpId", req.TmpId.Trim());
                await delDtl.ExecuteNonQueryAsync();
            }

            // 再刪主檔
            await using (var delMas = new SqlCommand(
                "DELETE FROM EMOdTmpPressMas WHERE TmpId = @TmpId", conn, tran))
            {
                AddParam(delMas, "@TmpId", req.TmpId.Trim());
                await delMas.ExecuteNonQueryAsync();
            }

            await tran.CommitAsync();
            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            await tran.RollbackAsync();
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
    /// 更新壓合主檔單一欄位 (行內編輯)
    /// </summary>
    [HttpPost("UpdateMasterField")]
    public async Task<IActionResult> UpdateMasterField([FromBody] UpdateMasterFieldRequest req)
    {
        if (req == null || string.IsNullOrWhiteSpace(req.TmpId) || string.IsNullOrWhiteSpace(req.FieldName))
            return BadRequest(new { ok = false, error = "TmpId and FieldName required." });

        // 只允許更新安全的欄位
        var allowedFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Notes", "TmpBOMId"
        };
        if (!allowedFields.Contains(req.FieldName))
            return BadRequest(new { ok = false, error = $"Field '{req.FieldName}' is not editable." });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        // 檢查狀態
        await using (var chk = new SqlCommand(
            "SELECT Status FROM EMOdTmpPressMas WITH (NOLOCK) WHERE TmpId = @TmpId", conn))
        {
            AddParam(chk, "@TmpId", req.TmpId.Trim());
            var status = await chk.ExecuteScalarAsync();
            if (status != null && Convert.ToInt32(status) == 1)
                return Ok(new { ok = false, error = "此範本已使用中，不可修改!" });
        }

        var sql = $"UPDATE EMOdTmpPressMas SET [{req.FieldName}] = @Value WHERE TmpId = @TmpId";
        await using var cmd = new SqlCommand(sql, conn);
        AddParam(cmd, "@TmpId", req.TmpId.Trim());
        AddParam(cmd, "@Value", req.Value?.Trim() ?? "");
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
    /// 送審/退審 (透過 EMOdTmpRoutePost)
    /// 對應 Delphi 的 btnC1Click (確認)
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
            AddParam(cmd, "@Source", "EMOdTmpPressMas");
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
    /// 取得欄位辭典 (Delphi 相容: Visible=NULL 視為不可見)
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

    // ==================== BOM 模型查詢 (供選擇組合模型 Dialog) ====================

    /// <summary>
    /// 查詢 BOM 模型主檔 (供選擇組合模型 Dialog)
    /// 對應 Delphi 的 TdlgTmpBOMSelect
    /// </summary>
    [HttpGet("GetBOMMasList")]
    public async Task<IActionResult> GetBOMMasList(
        [FromQuery] string? tmpId = null,
        [FromQuery] string? notes = null,
        [FromQuery] int? status = null,
        [FromQuery] int? activeOnly = null)
    {
        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        var where = new List<string>();
        var parameters = new List<SqlParameter>();

        if (!string.IsNullOrWhiteSpace(tmpId))
        {
            where.Add("TmpId LIKE @TmpId");
            parameters.Add(new SqlParameter("@TmpId", $"%{tmpId.Trim()}%"));
        }
        if (!string.IsNullOrWhiteSpace(notes))
        {
            where.Add("Notes LIKE @Notes");
            parameters.Add(new SqlParameter("@Notes", $"%{notes.Trim()}%"));
        }
        if (activeOnly == 1)
        {
            where.Add("Status = 1");
        }
        else if (status.HasValue && status.Value >= 0 && status.Value <= 1)
        {
            where.Add("Status = @Status");
            parameters.Add(new SqlParameter("@Status", status.Value));
        }

        var whereClause = where.Count > 0 ? "WHERE " + string.Join(" AND ", where) : "";
        var sql = $"SELECT * FROM dbo.EMOdTmpBOMMas WITH (NOLOCK) {whereClause} ORDER BY TmpId";

        await using var cmd = new SqlCommand(sql, conn);
        foreach (var p in parameters) cmd.Parameters.Add(new SqlParameter(p.ParameterName, p.Value));

        var rows = await ReadRowsAsync(cmd);
        return Ok(new { ok = true, data = rows });
    }

    /// <summary>
    /// 取得 BOM 模型明細 (供選擇組合模型 Dialog 的 TreeView)
    /// </summary>
    [HttpGet("GetBOMMasDtl")]
    public async Task<IActionResult> GetBOMMasDtl([FromQuery] string tmpId)
    {
        if (string.IsNullOrWhiteSpace(tmpId))
            return BadRequest(new { ok = false, error = "TmpId required." });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand(
            @"SELECT *, Sort = Degree * 100 + FL
              FROM dbo.EMOdTmpBOMDtl WITH (NOLOCK)
              WHERE TmpId = @TmpId
              ORDER BY Degree, FL", conn);
        AddParam(cmd, "@TmpId", tmpId.Trim());

        var rows = await ReadRowsAsync(cmd);
        return Ok(new { ok = true, data = rows });
    }

    // ==================== 更新 UserId ====================

    public class UpdateUserIdRequest
    {
        public string TmpId { get; set; } = "";
    }

    /// <summary>
    /// 更新範本的 UserId
    /// </summary>
    [HttpPost("UpdateUserId")]
    public async Task<IActionResult> UpdateUserId([FromBody] UpdateUserIdRequest req)
    {
        if (req == null || string.IsNullOrWhiteSpace(req.TmpId))
            return BadRequest(new { ok = false, error = "TmpId required." });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        var userId = await LoadUserIdAsync(conn);

        await using var cmd = new SqlCommand("EMOdTmpUpdateUserId", conn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandTimeout = 30;
        AddParam(cmd, "@TmpId", req.TmpId.Trim());
        AddParam(cmd, "@UserId", userId);
        AddParam(cmd, "@TableName", "EMOdTmpPressMas");
        await cmd.ExecuteNonQueryAsync();

        return Ok(new { ok = true });
    }
}
