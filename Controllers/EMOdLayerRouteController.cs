using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;

namespace PcbErpApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class EMOdLayerRouteController : ControllerBase
{
    private readonly string _connStr;
    private readonly ILogger<EMOdLayerRouteController> _logger;

    public EMOdLayerRouteController(PcbErpContext context, IConfiguration config, ILogger<EMOdLayerRouteController> logger)
    {
        _connStr = config.GetConnectionString("DefaultConnection")
            ?? context?.Database.GetDbConnection().ConnectionString
            ?? throw new InvalidOperationException("Missing connection string.");
        _logger = logger;
    }

    // ==================== Helper ====================

    private static void AddParam(SqlCommand cmd, string name, object? value)
        => cmd.Parameters.AddWithValue(name, value ?? DBNull.Value);

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

    // ==================== C途程異動：途程範本清單 ====================

    /// <summary>
    /// 取得目前層別的 TmpRouteId（途程代號顯示用）
    /// 對應 Delphi qryTmpRouteId：EMOdProdLayer
    /// </summary>
    [HttpGet("GetTmpRouteId")]
    public async Task<IActionResult> GetTmpRouteId(
        [FromQuery] string partNum,
        [FromQuery] string revision,
        [FromQuery] string layerId)
    {
        if (string.IsNullOrWhiteSpace(partNum) || string.IsNullOrWhiteSpace(revision) || string.IsNullOrWhiteSpace(layerId))
            return BadRequest(new { ok = false, error = "partNum, revision, layerId required." });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand(
            @"SELECT TmpRouteId FROM dbo.EMOdProdLayer WITH (NOLOCK)
              WHERE PartNum = @PartNum AND Revision = @Revision AND LayerId = @LayerId", conn);
        AddParam(cmd, "@PartNum", partNum.Trim());
        AddParam(cmd, "@Revision", revision.Trim());
        AddParam(cmd, "@LayerId", layerId.Trim());

        var value = (await cmd.ExecuteScalarAsync())?.ToString() ?? "";
        return Ok(new { ok = true, tmpRouteId = value });
    }

    /// <summary>
    /// 取得途程範本清單（C途程異動對話框左側）
    /// 對應 Delphi qryTmpMas：EMOdTmpRouteMas
    /// </summary>
    [HttpGet("GetTmpList")]
    public async Task<IActionResult> GetTmpList(
        [FromQuery] string? tmpId = null,
        [FromQuery] string? notes = null)
    {
        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        var where = new List<string> { "t1.IsStop = 0" };
        var parms = new List<SqlParameter>();

        if (!string.IsNullOrWhiteSpace(tmpId))
        {
            where.Add("t1.TmpId LIKE @TmpId");
            parms.Add(new SqlParameter("@TmpId", $"%{tmpId.Trim()}%"));
        }
        if (!string.IsNullOrWhiteSpace(notes))
        {
            where.Add("t1.Notes LIKE @Notes");
            parms.Add(new SqlParameter("@Notes", $"%{notes.Trim()}%"));
        }

        var sql = $@"SELECT t1.TmpId, t1.Notes, t1.Status
                     FROM EMOdTmpRouteMas t1 WITH (NOLOCK)
                     WHERE {string.Join(" AND ", where)}
                     ORDER BY t1.TmpId";

        await using var cmd = new SqlCommand(sql, conn);
        foreach (var p in parms) cmd.Parameters.Add(p);

        var rows = await ReadRowsAsync(cmd);
        return Ok(new { ok = true, data = rows });
    }

    /// <summary>
    /// 取得單一途程範本的製程明細（C途程異動對話框右側）
    /// 對應 Delphi qryTmpDtl：EMOdTmpRouteDtl
    /// </summary>
    [HttpGet("GetTmpDetail")]
    public async Task<IActionResult> GetTmpDetail([FromQuery] string tmpId)
    {
        if (string.IsNullOrWhiteSpace(tmpId))
            return BadRequest(new { ok = false, error = "tmpId required." });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand(
            @"SELECT t1.SerialNum, t1.ProcCode, t2.ProcName, t1.FinishRate, t1.Notes
              FROM EMOdTmpRouteDtl t1 WITH (NOLOCK)
              LEFT JOIN EMOdProcInfo t2 WITH (NOLOCK) ON t1.ProcCode = t2.ProcCode
              WHERE t1.TmpId = @TmpId
              ORDER BY t1.SerialNum", conn);
        AddParam(cmd, "@TmpId", tmpId.Trim());

        var rows = await ReadRowsAsync(cmd);
        return Ok(new { ok = true, data = rows });
    }

    // ==================== 途程內容變更：左側製程清單 ====================

    /// <summary>
    /// 查詢可用製程清單（途程內容變更對話框左側）
    /// 對應 Delphi exec EMOdProcChoice
    /// </summary>
    [HttpGet("GetProcChoice")]
    public async Task<IActionResult> GetProcChoice(
        [FromQuery] string? bProc = null,
        [FromQuery] string? eProc = null,
        [FromQuery] string? procLike = null,
        [FromQuery] string? procNameLike = null)
    {
        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand("exec EMOdProcChoice @BProc, @EProc, @ProcLike, @ProcNameLike", conn);
        AddParam(cmd, "@BProc", string.IsNullOrWhiteSpace(bProc) ? "" : bProc.Trim());
        AddParam(cmd, "@EProc", string.IsNullOrWhiteSpace(eProc) ? "" : eProc.Trim());
        AddParam(cmd, "@ProcLike", string.IsNullOrWhiteSpace(procLike) ? "" : procLike.Trim());
        AddParam(cmd, "@ProcNameLike", string.IsNullOrWhiteSpace(procNameLike) ? "" : procNameLike.Trim());

        var rows = await ReadRowsAsync(cmd);
        return Ok(new { ok = true, data = rows });
    }

    // ==================== 途程內容變更：右側現有途程 ====================

    /// <summary>
    /// 取得目前層別的途程內容（途程內容變更對話框右側初始狀態）
    /// 對應 Delphi qryTmpRouteDtl：EMOdLayerRoute
    /// </summary>
    [HttpGet("GetCurrentRoute")]
    public async Task<IActionResult> GetCurrentRoute(
        [FromQuery] string partNum,
        [FromQuery] string revision,
        [FromQuery] string layerId)
    {
        if (string.IsNullOrWhiteSpace(partNum) || string.IsNullOrWhiteSpace(revision) || string.IsNullOrWhiteSpace(layerId))
            return BadRequest(new { ok = false, error = "partNum, revision, layerId required." });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand(
            @"SELECT t1.ProcCode, t2.ProcName
              FROM dbo.EMOdLayerRoute t1 WITH (NOLOCK)
              LEFT JOIN dbo.EMOdProcInfo t2 WITH (NOLOCK) ON t1.ProcCode = t2.ProcCode
              WHERE t1.PartNum = @PartNum
                AND t1.Revision = @Revision
                AND t1.LayerId = @LayerId
              ORDER BY t1.SerialNum", conn);
        AddParam(cmd, "@PartNum", partNum.Trim());
        AddParam(cmd, "@Revision", revision.Trim());
        AddParam(cmd, "@LayerId", layerId.Trim());

        var rows = await ReadRowsAsync(cmd);
        return Ok(new { ok = true, data = rows });
    }

    // ==================== C途程異動：寫入 ====================

    public class InsLayerRouteRequest
    {
        public string PartNum { get; set; } = "";
        public string Revision { get; set; } = "";
        public string LayerId { get; set; } = "";
        public string TmpId { get; set; } = "";
    }

    /// <summary>
    /// C途程異動確認：依範本寫入此層別途程
    /// 對應 Delphi exec EMOdInsLayerRoute
    /// </summary>
    [HttpPost("InsLayerRoute")]
    public async Task<IActionResult> InsLayerRoute([FromBody] InsLayerRouteRequest req)
    {
        if (req == null || string.IsNullOrWhiteSpace(req.PartNum)
            || string.IsNullOrWhiteSpace(req.Revision)
            || string.IsNullOrWhiteSpace(req.LayerId)
            || string.IsNullOrWhiteSpace(req.TmpId))
            return BadRequest(new { ok = false, error = "PartNum, Revision, LayerId, TmpId required." });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        try
        {
            await using var cmd = new SqlCommand(
                "exec EMOdInsLayerRoute @PartNum, @Revision, @LayerId, @TmpId", conn);
            AddParam(cmd, "@PartNum", req.PartNum.Trim());
            AddParam(cmd, "@Revision", req.Revision.Trim());
            AddParam(cmd, "@LayerId", req.LayerId.Trim());
            AddParam(cmd, "@TmpId", req.TmpId.Trim());
            await cmd.ExecuteNonQueryAsync();
            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "InsLayerRoute failed");
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    // ==================== C途程備註：寫入 ====================

    public class RouteNotesRequest
    {
        public string PartNum { get; set; } = "";
        public string Revision { get; set; } = "";
        public string LayerId { get; set; } = "";
    }

    /// <summary>
    /// C途程備註確認：依選定途程範本建立備註
    /// 對應 Delphi exec EMOdProcNotesInsert
    /// </summary>
    [HttpPost("InsertNotes")]
    public async Task<IActionResult> InsertNotes([FromBody] RouteNotesRequest req)
    {
        if (req == null || string.IsNullOrWhiteSpace(req.PartNum)
            || string.IsNullOrWhiteSpace(req.Revision)
            || string.IsNullOrWhiteSpace(req.LayerId))
            return BadRequest(new { ok = false, error = "PartNum, Revision, LayerId required." });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        try
        {
            await using var cmd = new SqlCommand(
                "exec EMOdProcNotesInsert @PartNum, @Revision, @LayerId", conn);
            AddParam(cmd, "@PartNum", req.PartNum.Trim());
            AddParam(cmd, "@Revision", req.Revision.Trim());
            AddParam(cmd, "@LayerId", req.LayerId.Trim());
            await cmd.ExecuteNonQueryAsync();
            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "InsertNotes failed");
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    // ==================== 複製備註 ====================

    /// <summary>
    /// 複製備註：備份目前層別的製程備註
    /// 對應 Delphi exec EMOdProcNotesIN
    /// </summary>
    [HttpPost("BackupNotes")]
    public async Task<IActionResult> BackupNotes([FromBody] RouteNotesRequest req)
    {
        if (req == null || string.IsNullOrWhiteSpace(req.PartNum)
            || string.IsNullOrWhiteSpace(req.Revision)
            || string.IsNullOrWhiteSpace(req.LayerId))
            return BadRequest(new { ok = false, error = "PartNum, Revision, LayerId required." });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        try
        {
            await using var cmd = new SqlCommand(
                "exec EMOdProcNotesIN @PartNum, @Revision, @LayerId", conn);
            AddParam(cmd, "@PartNum", req.PartNum.Trim());
            AddParam(cmd, "@Revision", req.Revision.Trim());
            AddParam(cmd, "@LayerId", req.LayerId.Trim());
            await cmd.ExecuteNonQueryAsync();
            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "BackupNotes failed");
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    // ==================== 貼上備註 ====================

    /// <summary>
    /// 貼上備註：還原目前層別的製程備註
    /// 對應 Delphi exec EMOdProcNotesOUT
    /// </summary>
    [HttpPost("PasteNotes")]
    public async Task<IActionResult> PasteNotes([FromBody] RouteNotesRequest req)
    {
        if (req == null || string.IsNullOrWhiteSpace(req.PartNum)
            || string.IsNullOrWhiteSpace(req.Revision)
            || string.IsNullOrWhiteSpace(req.LayerId))
            return BadRequest(new { ok = false, error = "PartNum, Revision, LayerId required." });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        try
        {
            await using var cmd = new SqlCommand(
                "exec EMOdProcNotesOUT @PartNum, @Revision, @LayerId", conn);
            AddParam(cmd, "@PartNum", req.PartNum.Trim());
            AddParam(cmd, "@Revision", req.Revision.Trim());
            AddParam(cmd, "@LayerId", req.LayerId.Trim());
            await cmd.ExecuteNonQueryAsync();
            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PasteNotes failed");
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    // ==================== 途程內容變更 ====================

    public class RouteChangeRequest
    {
        public string PartNum { get; set; } = "";
        public string Revision { get; set; } = "";
        public string LayerId { get; set; } = "";
        public List<string> ProcCodes { get; set; } = new();
    }

    /// <summary>
    /// 途程內容變更確認（新模式 RouteChangeNewForm=1）
    /// 對應 Delphi：EMOdProdRouteBefChg → EMOdLayerRouteUpdateNew × N → EMOdRouteChangeCheck
    /// </summary>
    [HttpPost("RouteChange")]
    public async Task<IActionResult> RouteChange([FromBody] RouteChangeRequest req)
    {
        if (req == null || string.IsNullOrWhiteSpace(req.PartNum)
            || string.IsNullOrWhiteSpace(req.Revision)
            || string.IsNullOrWhiteSpace(req.LayerId))
            return BadRequest(new { ok = false, error = "PartNum, Revision, LayerId required." });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        // Step 1：取得 SPId（對應 exec EMOdProdRouteBefChg）
        int spId;
        try
        {
            await using var befCmd = new SqlCommand(
                "exec EMOdProdRouteBefChg @PartNum, @Revision, @LayerId", conn);
            AddParam(befCmd, "@PartNum", req.PartNum.Trim());
            AddParam(befCmd, "@Revision", req.Revision.Trim());
            AddParam(befCmd, "@LayerId", req.LayerId.Trim());
            var rows = await ReadRowsAsync(befCmd);
            spId = rows.Count > 0 && rows[0].ContainsKey("SpId")
                ? Convert.ToInt32(rows[0]["SpId"])
                : 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RouteChange BefChg failed");
            return Ok(new { ok = false, error = ex.Message });
        }

        // Step 2：逐筆呼叫 EMOdLayerRouteUpdateNew
        int iMax = req.ProcCodes.Count - 1;
        try
        {
            for (int i = 0; i <= iMax; i++)
            {
                await using var updCmd = new SqlCommand(
                    "exec EMOdLayerRouteUpdateNew @PartNum, @Revision, @LayerId, @ProcCode, @Index, @Max, @SPId",
                    conn);
                AddParam(updCmd, "@PartNum", req.PartNum.Trim());
                AddParam(updCmd, "@Revision", req.Revision.Trim());
                AddParam(updCmd, "@LayerId", req.LayerId.Trim());
                AddParam(updCmd, "@ProcCode", req.ProcCodes[i]);
                AddParam(updCmd, "@Index", i);
                AddParam(updCmd, "@Max", iMax);
                AddParam(updCmd, "@SPId", spId);
                await updCmd.ExecuteNonQueryAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RouteChange UpdateNew failed");
            return Ok(new { ok = false, error = ex.Message });
        }

        // Step 3：呼叫 EMOdRouteChangeCheck
        try
        {
            await using var chkCmd = new SqlCommand(
                "exec EMOdRouteChangeCheck @PartNum, @Revision, @LayerId, 0, 2", conn);
            AddParam(chkCmd, "@PartNum", req.PartNum.Trim());
            AddParam(chkCmd, "@Revision", req.Revision.Trim());
            AddParam(chkCmd, "@LayerId", req.LayerId.Trim());
            await chkCmd.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RouteChange Check failed");
            // 不視為嚴重錯誤，仍回傳成功
        }

        return Ok(new { ok = true });
    }
}
