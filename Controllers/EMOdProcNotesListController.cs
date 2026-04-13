using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;

namespace PcbErpApi.Controllers;

/// <summary>
/// 製程常用備註表 (EMO00042) API Controller
/// 對應 Delphi ProcNotesListDLL.pas
/// Master : CURdOCXTableSetUp MASTER1 → CURdTableName.RealTableName → ProcCode, ProcName
/// Detail : CURdOCXTableSetUp DETAIL1 → CURdTableName.RealTableName → ProcCode, ItemId, Notes
/// </summary>
[Route("api/[controller]")]
[ApiController]
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public class EMOdProcNotesListController : ControllerBase
{
    private const string SysItemId = "EMO00042";

    private readonly PcbErpContext _context;
    private readonly ILogger<EMOdProcNotesListController> _logger;

    public EMOdProcNotesListController(PcbErpContext context, ILogger<EMOdProcNotesListController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // ─── 私有輔助 ────────────────────────────────────────────────────────────

    /// <summary>
    /// 透過 CURdOCXTableSetUp + CURdTableName 取得指定 TableKind 的實際資料表名稱。
    /// 若設定不存在則回傳 fallback。
    /// </summary>
    private async Task<string> GetRealTableAsync(SqlConnection conn, string tableKind, string fallback)
    {
        const string sql = @"
            SELECT ISNULL(NULLIF(n.RealTableName,''), n.TableName) AS RealTableName
            FROM CURdOCXTableSetUp s WITH (NOLOCK)
            JOIN CURdTableName     n WITH (NOLOCK) ON n.TableName = s.TableName
            WHERE s.ItemId    = @itemId
              AND s.TableKind = @tableKind";

        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@itemId",    SysItemId);
        cmd.Parameters.AddWithValue("@tableKind", tableKind);
        var result = await cmd.ExecuteScalarAsync();
        var name   = result?.ToString();
        return string.IsNullOrWhiteSpace(name) ? fallback : name;
    }

    private async Task<List<Dictionary<string, object?>>> QueryListAsync(
        SqlConnection conn, string sql, params SqlParameter[] parms)
    {
        var list = new List<Dictionary<string, object?>>();
        await using var cmd = new SqlCommand(sql, conn);
        if (parms?.Length > 0) cmd.Parameters.AddRange(parms);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var row = new Dictionary<string, object?>();
            for (int i = 0; i < reader.FieldCount; i++)
                row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            list.Add(row);
        }
        return list;
    }

    // ─── Endpoints ──────────────────────────────────────────────────────────

    /// <summary>
    /// 取得主表 (MASTER1)：製程代碼 / 製程名稱
    /// 資料表名稱動態讀取自 CURdOCXTableSetUp + CURdTableName
    /// </summary>
    [HttpGet("master")]
    public async Task<IActionResult> GetMaster()
    {
        try
        {
            var cs = _context.Database.GetConnectionString();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            var table = await GetRealTableAsync(conn, "MASTER1", "EMOdProcInfo");

            var sql = $@"
                SELECT ProcCode, ProcName
                FROM dbo.[{table}] WITH (NOLOCK)
                ORDER BY ProcCode";

            var data = await QueryListAsync(conn, sql);
            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetMaster failed");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// 取得明細清單 (DETAIL1) by ProcCode
    /// 資料表名稱動態讀取自 CURdOCXTableSetUp + CURdTableName
    /// </summary>
    [HttpGet("detail/{procCode}")]
    public async Task<IActionResult> GetDetail(string procCode)
    {
        try
        {
            var cs = _context.Database.GetConnectionString();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            var table = await GetRealTableAsync(conn, "DETAIL1", "EMOdProcNotesList");

            var sql = $@"
                SELECT ProcCode,
                       ItemId,
                       ISNULL(Notes, '') AS Notes
                FROM dbo.[{table}] WITH (NOLOCK)
                WHERE ProcCode = @ProcCode
                ORDER BY ItemId";

            var data = await QueryListAsync(conn, sql,
                new SqlParameter("@ProcCode", procCode));

            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetDetail failed for ProcCode={ProcCode}", procCode);
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// 新增備注列
    /// INSERT INTO [DETAIL1] (ProcCode, ItemId, Notes)；若 ProcCode+ItemId 已存在則回傳錯誤
    /// </summary>
    [HttpPost("detail")]
    public async Task<IActionResult> AddDetail([FromBody] ProcNotesListAddRequest req)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(req.ProcCode) || string.IsNullOrWhiteSpace(req.ItemId))
                return BadRequest(new { success = false, message = "缺少 ProcCode 或 ItemId" });

            var cs = _context.Database.GetConnectionString();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            var table = await GetRealTableAsync(conn, "DETAIL1", "EMOdProcNotesList");

            // 檢查是否已存在
            var checkSql = $@"
                SELECT COUNT(1) FROM dbo.[{table}] WITH (NOLOCK)
                WHERE ProcCode = @ProcCode AND ItemId = @ItemId";
            await using (var chk = new SqlCommand(checkSql, conn))
            {
                chk.Parameters.AddWithValue("@ProcCode", req.ProcCode.Trim());
                chk.Parameters.AddWithValue("@ItemId",   req.ItemId.Trim());
                var cnt = (int)(await chk.ExecuteScalarAsync() ?? 0);
                if (cnt > 0)
                    return BadRequest(new { success = false, message = $"代號 {req.ItemId} 已存在" });
            }

            var insertSql = $@"
                INSERT INTO dbo.[{table}] (ProcCode, ItemId, Notes)
                VALUES (@ProcCode, @ItemId, @Notes)";
            await using var cmd = new SqlCommand(insertSql, conn);
            cmd.Parameters.AddWithValue("@ProcCode", req.ProcCode.Trim());
            cmd.Parameters.AddWithValue("@ItemId",   req.ItemId.Trim());
            cmd.Parameters.AddWithValue("@Notes",    (object?)(req.Notes ?? "") ?? DBNull.Value);
            await cmd.ExecuteNonQueryAsync();

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AddDetail failed");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// 刪除備注列
    /// DELETE FROM [DETAIL1] WHERE ProcCode=... AND ItemId=...
    /// </summary>
    [HttpDelete("detail/{procCode}/{itemId}")]
    public async Task<IActionResult> DeleteDetail(string procCode, string itemId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(procCode) || string.IsNullOrWhiteSpace(itemId))
                return BadRequest(new { success = false, message = "缺少 ProcCode 或 ItemId" });

            var cs = _context.Database.GetConnectionString();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            var table = await GetRealTableAsync(conn, "DETAIL1", "EMOdProcNotesList");

            var sql = $@"
                DELETE FROM dbo.[{table}]
                WHERE ProcCode = @ProcCode AND ItemId = @ItemId";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ProcCode", procCode.Trim());
            cmd.Parameters.AddWithValue("@ItemId",   itemId.Trim());
            var affected = await cmd.ExecuteNonQueryAsync();

            if (affected == 0)
                return NotFound(new { success = false, message = "找不到該筆資料" });

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteDetail failed");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// 更新備註 (Notes)
    /// Key: ProcCode + ItemId → UPDATE [DETAIL1 table] SET Notes=...
    /// </summary>
    [HttpPut("notes")]
    public async Task<IActionResult> UpdateNotes([FromBody] ProcNotesListUpdateRequest req)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(req.ProcCode) || string.IsNullOrWhiteSpace(req.ItemId))
                return BadRequest(new { success = false, message = "缺少 ProcCode 或 ItemId" });

            var cs = _context.Database.GetConnectionString();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            var table = await GetRealTableAsync(conn, "DETAIL1", "EMOdProcNotesList");

            var sql = $@"
                UPDATE dbo.[{table}]
                SET Notes = @Notes
                WHERE ProcCode = @ProcCode AND ItemId = @ItemId";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Notes",    (object?)(req.Notes ?? "") ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ProcCode", req.ProcCode.Trim());
            cmd.Parameters.AddWithValue("@ItemId",   req.ItemId.Trim());
            await cmd.ExecuteNonQueryAsync();

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateNotes failed");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
}

public record ProcNotesListUpdateRequest(string ProcCode, string ItemId, string? Notes);
public record ProcNotesListAddRequest(string ProcCode, string ItemId, string? Notes);
