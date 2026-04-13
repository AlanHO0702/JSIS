using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;

namespace PcbErpApi.Controllers;

/// <summary>
/// 群組製程備註資料 (EMOdTmpProcNotes) API Controller
/// 對應 Delphi TmpProcNotesDLL.pas
/// 資料表: EMOdProdStyleTree (群組樹) / EMOdProdStyleTreeSub (製程明細)
/// </summary>
[Route("api/[controller]")]
[ApiController]
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public class EMOdTmpProcNotesController : ControllerBase
{
    private readonly PcbErpContext _context;
    private readonly ILogger<EMOdTmpProcNotesController> _logger;

    public EMOdTmpProcNotesController(PcbErpContext context, ILogger<EMOdTmpProcNotesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // ──────────────────────────────────────────
    // 輔助方法
    // ──────────────────────────────────────────
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

    private async Task<List<Dictionary<string, object?>>> QueryListAsync(
        SqlConnection conn, SqlTransaction tran, string sql, params SqlParameter[] parms)
    {
        var list = new List<Dictionary<string, object?>>();
        await using var cmd = new SqlCommand(sql, conn, tran);
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

    // ──────────────────────────────────────────
    // 讀取類 API
    // ──────────────────────────────────────────

    /// <summary>
    /// 取得樹狀結構資料 (EMOdProdStyleTree)
    /// 對應 tblStyleTree → trvMas
    /// </summary>
    [HttpGet("tree")]
    public async Task<IActionResult> GetTree()
    {
        try
        {
            var cs = _context.Database.GetConnectionString();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            var sql = @"
                SELECT ItemId, ItemName, LevelNo, SuperId
                FROM dbo.EMOdProdStyleTree (nolock)
                ORDER BY ItemId";

            var data = await QueryListAsync(conn, sql);
            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetTree failed");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// 取得群組下的製程明細 (EMOdProdStyleTreeSub JOIN EMOdProcInfo)
    /// 對應 tblProdStyleTreeSub → grdProdStyleTreeSub
    /// </summary>
    [HttpGet("sub/{itemId}")]
    public async Task<IActionResult> GetSub(string itemId)
    {
        try
        {
            var cs = _context.Database.GetConnectionString();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            var sql = @"
                SELECT t1.ItemId,
                       t1.ProcCode,
                       ISNULL(t1.Notes, '') AS Notes,
                       ISNULL(t2.ProcName, '') AS ProcName
                FROM dbo.EMOdProdStyleTreeSub t1 (nolock)
                LEFT JOIN EMOdProcInfo t2 (nolock) ON t1.ProcCode = t2.ProcCode
                WHERE t1.ItemId = @ItemId
                ORDER BY t1.ProcCode";

            var data = await QueryListAsync(conn, sql,
                new SqlParameter("@ItemId", itemId));

            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetSub failed for ItemId={ItemId}", itemId);
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// 取得可新增的製程清單 (不在群組內的製程)
    /// 對應 qrySource (ProcNoteSearch.dfm 左側來源清單)
    /// </summary>
    [HttpGet("procs-available/{itemId}")]
    public async Task<IActionResult> GetProcsAvailable(string itemId)
    {
        try
        {
            var cs = _context.Database.GetConnectionString();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            var sql = @"
                SELECT ProcCode, ProcName
                FROM dbo.EMOdProcInfo (nolock)
                WHERE ProcCode NOT IN (
                    SELECT ProcCode
                    FROM EMOdProdStyleTreeSub (nolock)
                    WHERE ItemId = @ItemId
                )
                ORDER BY ProcCode";

            var data = await QueryListAsync(conn, sql,
                new SqlParameter("@ItemId", itemId));

            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetProcsAvailable failed");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// 取得群組內已選的製程清單
    /// 對應 qryNotClose (ProcNoteSearch.dfm 右側目的清單)
    /// </summary>
    [HttpGet("procs-selected/{itemId}")]
    public async Task<IActionResult> GetProcsSelected(string itemId)
    {
        try
        {
            var cs = _context.Database.GetConnectionString();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            var sql = @"
                SELECT t1.ProcCode, ISNULL(t2.ProcName, '') AS ProcName
                FROM EMOdProdStyleTreeSub t1 (nolock)
                LEFT JOIN EMOdProcInfo t2 (nolock) ON t1.ProcCode = t2.ProcCode
                WHERE t1.ItemId = @ItemId
                ORDER BY t1.ProcCode";

            var data = await QueryListAsync(conn, sql,
                new SqlParameter("@ItemId", itemId));

            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetProcsSelected failed");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// 取得所有群組 (用於新增/刪除 Modal 的下拉選單)
    /// 對應 cobSuperId (ProcNotesAdd.dfm) / cobItemId (ProcNotesDel.dfm)
    /// </summary>
    [HttpGet("tree-for-combo")]
    public async Task<IActionResult> GetTreeForCombo()
    {
        try
        {
            var cs = _context.Database.GetConnectionString();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            var sql = @"
                SELECT ItemId, ItemName, LevelNo
                FROM dbo.EMOdProdStyleTree (nolock)
                ORDER BY ItemId";

            var data = await QueryListAsync(conn, sql);
            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetTreeForCombo failed");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    // ──────────────────────────────────────────
    // 寫入類 API
    // ──────────────────────────────────────────

    /// <summary>
    /// 新增群組節點 (對應 btAppendClick)
    /// 呼叫 EMOdTmpProcNotesGetNum 取得新 ItemId / LevelNo，再 INSERT EMOdProdStyleTree
    /// </summary>
    [HttpPost("append")]
    public async Task<IActionResult> Append([FromBody] TmpProcNotesAppendRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.SuperId))
            return BadRequest(new { success = false, message = "請輸入上層代碼" });
        if (string.IsNullOrWhiteSpace(req.ItemName))
            return BadRequest(new { success = false, message = "請輸入名稱" });

        try
        {
            var cs = _context.Database.GetConnectionString();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            // 1. 呼叫 SP 取得新 ItemId 與 LevelNo
            string newItemId = "";
            int newLevelNo = 1;

            await using (var cmd = new SqlCommand("exec EMOdTmpProcNotesGetNum @SuperId", conn))
            {
                cmd.Parameters.AddWithValue("@SuperId", req.SuperId.Trim());
                await using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    newItemId = reader["ItemId"]?.ToString()?.Trim() ?? "";
                    var lnoRaw = reader["LevelNo"];
                    newLevelNo = lnoRaw == DBNull.Value ? 1 : Convert.ToInt32(lnoRaw);
                }
            }

            if (string.IsNullOrWhiteSpace(newItemId))
                return BadRequest(new { success = false, message = "無法取得新代碼，請確認上層代碼是否正確" });

            // 2. INSERT EMOdProdStyleTree
            var insertSql = @"
                INSERT INTO EMOdProdStyleTree (ItemId, ItemName, LevelNo, SuperId)
                VALUES (@ItemId, @ItemName, @LevelNo, @SuperId)";

            await using (var cmd = new SqlCommand(insertSql, conn))
            {
                cmd.Parameters.AddWithValue("@ItemId", newItemId);
                cmd.Parameters.AddWithValue("@ItemName", req.ItemName.Trim());
                cmd.Parameters.AddWithValue("@LevelNo", newLevelNo);
                cmd.Parameters.AddWithValue("@SuperId", req.SuperId.Trim());
                await cmd.ExecuteNonQueryAsync();
            }

            return Ok(new { success = true, newItemId, newLevelNo });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Append failed");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// 改動群組製程清單 (對應 btChangeClick / dlgProcNoteSearch)
    /// 先呼叫 EMOdProdStyleTreeTmpDel 刪除舊明細，
    /// 再 INSERT 新選製程 並嘗試從 EMOdProdStyleTreeTmpSub 帶回舊備註
    /// </summary>
    [HttpPost("change")]
    public async Task<IActionResult> Change([FromBody] TmpProcNotesChangeRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.ItemId))
            return BadRequest(new { success = false, message = "請選擇群組節點" });

        try
        {
            var cs = _context.Database.GetConnectionString();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();
            await using var tran = conn.BeginTransaction();

            try
            {
                // 1. 刪除舊明細 (SP 可能先備份至 TmpSub)
                await using (var cmd = new SqlCommand("exec EMOdProdStyleTreeTmpDel @ItemId", conn, tran))
                {
                    cmd.Parameters.AddWithValue("@ItemId", req.ItemId.Trim());
                    await cmd.ExecuteNonQueryAsync();
                }

                // 2. 逐筆 INSERT 新選製程，並嘗試帶回舊備註
                var insertSql = @"
                    INSERT INTO dbo.EMOdProdStyleTreeSub (ItemId, ProcCode, Notes)
                    VALUES (@ItemId1, @ProcCode, '');

                    UPDATE t1
                    SET t1.Notes = t2.Notes
                    FROM EMOdProdStyleTreeSub t1 (nolock),
                         EMOdProdStyleTreeTmpSub t2 (nolock)
                    WHERE t1.ProcCode = t2.ProcCode
                      AND t1.ItemId   = t2.ItemId
                      AND t1.ItemId   = @ItemId2;";

                foreach (var procCode in req.ProcCodes ?? new List<string>())
                {
                    await using var cmd = new SqlCommand(insertSql, conn, tran);
                    cmd.Parameters.AddWithValue("@ItemId1", req.ItemId.Trim());
                    cmd.Parameters.AddWithValue("@ProcCode", procCode.Trim());
                    cmd.Parameters.AddWithValue("@ItemId2", req.ItemId.Trim());
                    await cmd.ExecuteNonQueryAsync();
                }

                await tran.CommitAsync();
                return Ok(new { success = true });
            }
            catch
            {
                await tran.RollbackAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Change failed for ItemId={ItemId}", req.ItemId);
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// 刪除群組節點 (對應 btDeleteClick / dlgProcNotesDel)
    /// DELETE EMOdProdStyleTree WHERE ItemId = :ItemId
    /// </summary>
    [HttpPost("delete")]
    public async Task<IActionResult> Delete([FromBody] TmpProcNotesDeleteRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.ItemId))
            return BadRequest(new { success = false, message = "請選擇要刪除的節點" });

        try
        {
            var cs = _context.Database.GetConnectionString();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            // 刪除節點本身
            var sql = "DELETE EMOdProdStyleTree WHERE ItemId = @ItemId";
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ItemId", req.ItemId.Trim());
            await cmd.ExecuteNonQueryAsync();

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Delete failed for ItemId={ItemId}", req.ItemId);
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// 更名節點 (對應 btnNameClick)
    /// UPDATE EMOdProdStyleTree SET ItemName=... WHERE ItemId=...
    /// </summary>
    [HttpPut("rename")]
    public async Task<IActionResult> Rename([FromBody] TmpProcNotesRenameRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.ItemId))
            return BadRequest(new { success = false, message = "缺少 ItemId" });
        if (string.IsNullOrWhiteSpace(req.ItemName))
            return BadRequest(new { success = false, message = "請輸入名稱" });

        try
        {
            var cs = _context.Database.GetConnectionString();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            var sql = "UPDATE EMOdProdStyleTree SET ItemName = @ItemName WHERE ItemId = @ItemId";
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ItemName", req.ItemName.Trim());
            cmd.Parameters.AddWithValue("@ItemId", req.ItemId.Trim());
            await cmd.ExecuteNonQueryAsync();

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rename failed for ItemId={ItemId}", req.ItemId);
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// 更新製程備註 (對應 DBMemo1 → Notes 欄位存檔)
    /// UPDATE EMOdProdStyleTreeSub SET Notes=... WHERE ItemId=... AND ProcCode=...
    /// </summary>
    [HttpPut("notes")]
    public async Task<IActionResult> UpdateNotes([FromBody] TmpProcNotesUpdateNotesRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.ItemId) || string.IsNullOrWhiteSpace(req.ProcCode))
            return BadRequest(new { success = false, message = "缺少 ItemId 或 ProcCode" });

        try
        {
            var cs = _context.Database.GetConnectionString();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            var sql = @"
                UPDATE dbo.EMOdProdStyleTreeSub
                SET Notes = @Notes
                WHERE ItemId = @ItemId AND ProcCode = @ProcCode";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Notes", (object?)(req.Notes ?? "") ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ItemId", req.ItemId.Trim());
            cmd.Parameters.AddWithValue("@ProcCode", req.ProcCode.Trim());
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

// ──────────────────────────────────────────
// Request Models
// ──────────────────────────────────────────
public record TmpProcNotesAppendRequest(string SuperId, string ItemName);
public record TmpProcNotesChangeRequest(string ItemId, List<string>? ProcCodes);
public record TmpProcNotesDeleteRequest(string ItemId);
public record TmpProcNotesRenameRequest(string ItemId, string ItemName);
public record TmpProcNotesUpdateNotesRequest(string ItemId, string ProcCode, string? Notes);
