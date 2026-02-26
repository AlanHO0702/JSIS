using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;

namespace PcbErpApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class EMOdTmpBOM_SCController : ControllerBase
{
    private readonly string _connStr;
    private readonly ILogger<EMOdTmpBOM_SCController> _logger;

    public EMOdTmpBOM_SCController(PcbErpContext context, IConfiguration config, ILogger<EMOdTmpBOM_SCController> logger)
    {
        _connStr = config.GetConnectionString("DefaultConnection")
            ?? context?.Database.GetDbConnection().ConnectionString
            ?? throw new InvalidOperationException("Missing connection string.");
        _logger = logger;
    }

    /// <summary>
    /// 分頁查詢主檔資料
    /// GET: api/EMOdTmpBOMMas/paged
    /// </summary>
    [HttpGet("/api/EMOdTmpBOMMas/paged")]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? TmpId = null,
        [FromQuery] string? Notes = null)
    {
        try
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

            var whereClause = where.Count > 0 ? "WHERE " + string.Join(" AND ", where) : "";

            // 取得總筆數
            await using var countCmd = new SqlCommand($"SELECT COUNT(*) FROM EMOdTmpBOMMas WITH (NOLOCK) {whereClause}", conn);
            foreach (var p in parameters) countCmd.Parameters.Add(new SqlParameter(p.ParameterName, p.Value));
            var totalCount = (int)(await countCmd.ExecuteScalarAsync() ?? 0);

            // 取得分頁資料
            var offset = (page - 1) * pageSize;
            var sql = $@"SELECT * FROM EMOdTmpBOMMas WITH (NOLOCK) {whereClause}
                         ORDER BY TmpId
                         OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            await using var dataCmd = new SqlCommand(sql, conn);
            foreach (var p in parameters) dataCmd.Parameters.Add(new SqlParameter(p.ParameterName, p.Value));
            dataCmd.Parameters.AddWithValue("@Offset", offset);
            dataCmd.Parameters.AddWithValue("@PageSize", pageSize);

            var rows = new List<Dictionary<string, object?>>();
            await using var reader = await dataCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object?>();
                for (int i = 0; i < reader.FieldCount; i++)
                    row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                rows.Add(row);
            }

            return Ok(new { totalCount, data = rows });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetPaged failed: page={Page}, TmpId={TmpId}, Notes={Notes}", page, TmpId, Notes);
            return StatusCode(500, new { ok = false, error = ex.Message });
        }
    }

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

    /// <summary>
    /// 取得 BOM 明細資料 (透過 EMOdTmpBomGetData)
    /// </summary>
    [HttpGet("GetBOMData")]
    public async Task<IActionResult> GetBOMData([FromQuery] string tmpId)
    {
        if (string.IsNullOrWhiteSpace(tmpId))
            return BadRequest(new { ok = false, error = "TmpId required." });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand("EMOdTmpBomGetData", conn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandTimeout = 30;
        AddParam(cmd, "@TmpID", tmpId.Trim());

        var rows = new List<Dictionary<string, object?>>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var row = new Dictionary<string, object?>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            }
            rows.Add(row);
        }

        return Ok(new { ok = true, data = rows });
    }

    /// <summary>
    /// 取得 BOM 明細資料 (直接查詢 EMOdTmpBOMDtl 表)
    /// </summary>
    [HttpGet("GetBOMDtl")]
    public async Task<IActionResult> GetBOMDtl([FromQuery] string tmpId)
    {
        if (string.IsNullOrWhiteSpace(tmpId))
            return BadRequest(new { ok = false, error = "TmpId required." });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand(
            "SELECT * FROM EMOdTmpBOMDtl WITH (NOLOCK) WHERE TmpId = @TmpId ORDER BY FL", conn);
        AddParam(cmd, "@TmpId", tmpId.Trim());

        var rows = new List<Dictionary<string, object?>>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var row = new Dictionary<string, object?>();
            for (int i = 0; i < reader.FieldCount; i++)
                row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            rows.Add(row);
        }

        return Ok(new { ok = true, data = rows });
    }

    /// <summary>
    /// 檢查 BOM 範本是否可編輯 (透過 EMOdTmpBOMUpdateChk)
    /// </summary>
    [HttpGet("CheckUpdate")]
    public async Task<IActionResult> CheckUpdate([FromQuery] string tmpId)
    {
        if (string.IsNullOrWhiteSpace(tmpId))
            return BadRequest(new { ok = false, error = "TmpId required." });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand("EMOdTmpBOMUpdateChk", conn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandTimeout = 30;
        AddParam(cmd, "@TmpId", tmpId.Trim());

        var rows = new List<Dictionary<string, object?>>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var row = new Dictionary<string, object?>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            }
            rows.Add(row);
        }

        return Ok(new { ok = true, canEdit = rows.Count == 0, usedBy = rows });
    }

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

    /// <summary>
    /// 取得最大層數和度數
    /// </summary>
    [HttpGet("GetMaxLayerDegree")]
    public async Task<IActionResult> GetMaxLayerDegree([FromQuery] string tmpId)
    {
        if (string.IsNullOrWhiteSpace(tmpId))
            return BadRequest(new { ok = false, error = "TmpId required." });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand(
            "SELECT L=MAX(EL), Degree=MAX(Degree)+1 FROM EMOdTmpBOMDtl WITH (NOLOCK) WHERE TmpId = @TmpId", conn);
        AddParam(cmd, "@TmpId", tmpId.Trim());

        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var maxLayer = reader.IsDBNull(0) ? 0 : Convert.ToInt32(reader.GetValue(0));
            var maxDegree = reader.IsDBNull(1) ? 0 : Convert.ToInt32(reader.GetValue(1));
            return Ok(new { ok = true, maxLayer, maxDegree });
        }

        return Ok(new { ok = true, maxLayer = 0, maxDegree = 0 });
    }

    public class SaveBOMRequest
    {
        public string TmpId { get; set; } = "";
        public List<BOMLayerItem> Layers { get; set; } = new();
    }

    public class BOMLayerItem
    {
        public int IssLayer { get; set; }
        public int Degree { get; set; }
        public int FL { get; set; }
        public int EL { get; set; }
        public int AftFL { get; set; }
        public int AftEL { get; set; }
        public string LayerName { get; set; } = "";
    }

    /// <summary>
    /// 儲存 BOM 組合模型 (刪除舊資料後重新插入)
    /// </summary>
    [HttpPost("SaveBOM")]
    public async Task<IActionResult> SaveBOM([FromBody] SaveBOMRequest req)
    {
        if (req == null || string.IsNullOrWhiteSpace(req.TmpId))
            return BadRequest(new { ok = false, error = "TmpId required." });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();
        await using var tran = conn.BeginTransaction();

        try
        {
            // 刪除舊資料
            await using (var delCmd = new SqlCommand(
                "DELETE FROM dbo.EMOdTmpBOMDtl WHERE TmpId = @TmpId", conn, tran))
            {
                AddParam(delCmd, "@TmpId", req.TmpId.Trim());
                await delCmd.ExecuteNonQueryAsync();
            }

            // 逐筆插入新資料
            foreach (var layer in req.Layers)
            {
                await using var insCmd = new SqlCommand("EMOdTmpBomIns", conn, tran);
                insCmd.CommandType = CommandType.StoredProcedure;
                insCmd.CommandTimeout = 30;
                AddParam(insCmd, "@TmpId", req.TmpId.Trim());
                AddParam(insCmd, "@IssLayer", layer.IssLayer);
                AddParam(insCmd, "@Degree", layer.Degree);
                AddParam(insCmd, "@FL", layer.FL);
                AddParam(insCmd, "@EL", layer.EL);
                AddParam(insCmd, "@AftFL", layer.AftFL);
                AddParam(insCmd, "@AftEL", layer.AftEL);
                AddParam(insCmd, "@LayerName", layer.LayerName);
                await insCmd.ExecuteNonQueryAsync();
            }

            // 驗證結果
            await using (var chkCmd = new SqlCommand("EMOdCheckTmpBOMSet", conn, tran))
            {
                chkCmd.CommandType = CommandType.StoredProcedure;
                chkCmd.CommandTimeout = 30;
                AddParam(chkCmd, "@TmpId", req.TmpId.Trim());

                var errRows = new List<Dictionary<string, object?>>();
                await using var reader = await chkCmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var row = new Dictionary<string, object?>();
                    for (int i = 0; i < reader.FieldCount; i++)
                        row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    errRows.Add(row);
                }

                if (errRows.Count > 0)
                {
                    await tran.RollbackAsync();
                    return Ok(new { ok = false, error = "BOM 設定驗證失敗", details = errRows });
                }
            }

            // 重新計算 degree
            await using (var degCmd = new SqlCommand("EMOdTmpBomDegree", conn, tran))
            {
                degCmd.CommandType = CommandType.StoredProcedure;
                degCmd.CommandTimeout = 30;
                AddParam(degCmd, "@TmpId", req.TmpId.Trim());
                await degCmd.ExecuteNonQueryAsync();
            }

            await tran.CommitAsync();
            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            await tran.RollbackAsync();
            _logger.LogError(ex, "SaveBOM failed for TmpId={TmpId}", req.TmpId);
            return StatusCode(500, new { ok = false, error = ex.Message });
        }
    }

    public class CopyBOMRequest
    {
        public string SourceTmpId { get; set; } = "";
        public string NewTmpId { get; set; } = "";
    }

    /// <summary>
    /// 複製 BOM 範本 (透過 EMOdTmpBOMCopy)
    /// </summary>
    [HttpPost("CopyBOM")]
    public async Task<IActionResult> CopyBOM([FromBody] CopyBOMRequest req)
    {
        if (req == null || string.IsNullOrWhiteSpace(req.SourceTmpId) || string.IsNullOrWhiteSpace(req.NewTmpId))
            return BadRequest(new { ok = false, error = "SourceTmpId and NewTmpId required." });

        try
        {
            await using var conn = new SqlConnection(_connStr);
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("EMOdTmpBOMCopy", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 60;
            AddParam(cmd, "@TmpId", req.SourceTmpId.Trim());
            AddParam(cmd, "@NewTmpId", req.NewTmpId.Trim());

            await using var reader = await cmd.ExecuteReaderAsync();
            var resultStr = "";
            if (await reader.ReadAsync())
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    if (reader.GetName(i) == "ResultStr")
                        resultStr = reader.IsDBNull(i) ? "" : reader.GetValue(i).ToString() ?? "";
                }
            }

            if (!string.IsNullOrEmpty(resultStr))
                return Ok(new { ok = false, error = resultStr });

            return Ok(new { ok = true });
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "CopyBOM failed: {Src} -> {New}", req.SourceTmpId, req.NewTmpId);
            var msg = ex.Number == 2627 || ex.Number == 2601
                ? $"代碼 [{req.NewTmpId.Trim()}] 已存在，無法複製"
                : ex.Message;
            return Ok(new { ok = false, error = msg });
        }
    }

    /// <summary>
    /// 更新範本的 UserId (透過 EMOdTmpUpdateUserId)
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
        AddParam(cmd, "@TableName", "EMOdTmpBOMMas");
        await cmd.ExecuteNonQueryAsync();

        return Ok(new { ok = true });
    }

    public class UpdateUserIdRequest
    {
        public string TmpId { get; set; } = "";
    }

    // ==================== Insert / Delete Master ====================

    public class InsertMasterRequest
    {
        public string TmpId { get; set; } = "";
        public string Notes { get; set; } = "";
    }

    /// <summary>
    /// 新增主檔
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
            "SELECT COUNT(*) FROM EMOdTmpBOMMas WITH (NOLOCK) WHERE TmpId = @TmpId", conn))
        {
            AddParam(chk, "@TmpId", req.TmpId.Trim());
            var cnt = (int)(await chk.ExecuteScalarAsync() ?? 0);
            if (cnt > 0)
                return Ok(new { ok = false, error = "此代碼已存在" });
        }

        var userId = await LoadUserIdAsync(conn);

        await using var cmd = new SqlCommand(
            @"INSERT INTO EMOdTmpBOMMas (TmpId, Notes, BuildDate, UserId)
              VALUES (@TmpId, @Notes, GETDATE(), @UserId)", conn);
        AddParam(cmd, "@TmpId", req.TmpId.Trim());
        AddParam(cmd, "@Notes", req.Notes?.Trim() ?? "");
        AddParam(cmd, "@UserId", userId);

        try
        {
            await cmd.ExecuteNonQueryAsync();
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
    /// 刪除主檔及其明細
    /// </summary>
    [HttpPost("DeleteMaster")]
    public async Task<IActionResult> DeleteMaster([FromBody] DeleteMasterRequest req)
    {
        if (req == null || string.IsNullOrWhiteSpace(req.TmpId))
            return BadRequest(new { ok = false, error = "TmpId required." });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();
        await using var tran = conn.BeginTransaction();

        try
        {
            // 先刪明細
            await using (var delDtl = new SqlCommand(
                "DELETE FROM EMOdTmpBOMDtl WHERE TmpId = @TmpId", conn, tran))
            {
                AddParam(delDtl, "@TmpId", req.TmpId.Trim());
                await delDtl.ExecuteNonQueryAsync();
            }

            // 再刪主檔
            await using (var delMas = new SqlCommand(
                "DELETE FROM EMOdTmpBOMMas WHERE TmpId = @TmpId", conn, tran))
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

    // ==================== 送審 / 退審 ====================

    public class ApprovalPostRequest
    {
        public string TmpId { get; set; } = "";
        public int IsPost { get; set; } // 1=送審, 0=退審
    }

    /// <summary>
    /// 送審/退審 (透過 EMOdTmpRoutePost)
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
            AddParam(cmd, "@Source", "EMOdTmpBOMMas");
            AddParam(cmd, "@UserId", userId);

            // SP 可能回傳結果集 (錯誤訊息等)
            var rows = new List<Dictionary<string, object?>>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object?>();
                for (int i = 0; i < reader.FieldCount; i++)
                    row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                rows.Add(row);
            }

            // 檢查是否有 ResultStr 欄位 (錯誤訊息)
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
}
