using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;

/// <summary>
/// HP000032 - 請假單 自訂按鈕 API 控制器
/// 對應 Delphi:
///   - btnC3: HPSdTimeOffSpeCalc.dll — 特休假查詢
///   - btnC4: MPHdUploadNote.dll     — 上傳附件 (HPSdTimeOffSub.AttchFile)
///   - btnC5: MPHdViewNote.dll       — 檢視附件
///   - btnC6: MPHdDelNote.dll        — 刪除附件
/// 附件路徑: CURdSysParams WHERE SystemId='HPS' AND ParamId='AttchFilePath'
/// 附件檔名格式: {PaperNum}-{Item}{ext}  (e.g. T2604080001-1.pdf)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class HP000032Controller : ControllerBase
{
    private readonly string _cs;
    private readonly ILogger<HP000032Controller> _logger;

    public HP000032Controller(IConfiguration cfg, PcbErpContext db, ILogger<HP000032Controller> logger)
    {
        _cs = cfg.GetConnectionString("DefaultConnection")
              ?? db?.Database.GetDbConnection().ConnectionString
              ?? throw new InvalidOperationException("找不到連線字串");
        _logger = logger;
    }

    // ─────────────────────────────────────────────────────────────
    // 共用：從 CURdSysParams 取得 HPS/AttchFilePath
    // 對應 Delphi: select Value from CURdSysParams where SystemId='HPS' and ParamId='AttchFilePath'
    // ─────────────────────────────────────────────────────────────
    private async Task<string?> GetAttachPathAsync(SqlConnection conn)
    {
        await using var cmd = new SqlCommand(
            "SELECT Value FROM CURdSysParams WITH (NOLOCK) WHERE SystemId='HPS' AND ParamId='AttchFilePath'", conn);
        var val = await cmd.ExecuteScalarAsync();
        var path = val?.ToString()?.Trim();
        if (string.IsNullOrWhiteSpace(path)) return null;
        if (!path.EndsWith(Path.DirectorySeparatorChar.ToString()) &&
            !path.EndsWith("/"))
            path += Path.DirectorySeparatorChar;
        return path;
    }

    // ─────────────────────────────────────────────────────────────
    // 共用：取得該明細列目前的 AttchFile（可為空）
    // ─────────────────────────────────────────────────────────────
    private async Task<string> GetAttchFileAsync(SqlConnection conn, string paperNum, int item)
    {
        await using var cmd = new SqlCommand(
            "SELECT AttchFile = ISNULL(AttchFile,'') FROM HPSdTimeOffSub WITH (NOLOCK) WHERE PaperNum=@PaperNum AND Item=@Item", conn);
        cmd.Parameters.AddWithValue("@PaperNum", paperNum);
        cmd.Parameters.AddWithValue("@Item", item);
        var val = await cmd.ExecuteScalarAsync();
        return val?.ToString() ?? "";
    }

    // Content-Type 對應表（和 CompanyAttachFileController 一致）
    private static string GetContentType(string ext) => ext.ToLowerInvariant() switch
    {
        ".pdf"  => "application/pdf",
        ".png"  => "image/png",
        ".jpg" or ".jpeg" => "image/jpeg",
        ".gif"  => "image/gif",
        ".bmp"  => "image/bmp",
        ".txt"  => "text/plain",
        ".csv"  => "text/csv",
        ".xls"  => "application/vnd.ms-excel",
        ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        ".doc"  => "application/msword",
        ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        _       => "application/octet-stream"
    };

    /// <summary>
    /// 依 PaperNum + Item 查詢員工特休假資料
    /// GET /api/HP000032/GetSpeHisCalc?paperNum=HP2024001&amp;item=1
    /// 對應 Delphi: btnGetParamsClick
    /// </summary>
    [HttpGet("GetSpeHisCalc")]
    public async Task<IActionResult> GetSpeHisCalc([FromQuery] string paperNum, [FromQuery] int item)
    {
        if (string.IsNullOrWhiteSpace(paperNum))
            return Ok(new { ok = false, error = "缺少 PaperNum" });

        try
        {
            await using var conn = new SqlConnection(_cs);
            await conn.OpenAsync();

            // Step 1: 呼叫 SP 取得 EmpId 和 BookYear
            // 對應 Delphi: exec HPSdFindEmpSpeHis 'PaperNum', Item
            string empId = "";
            string bookYear = "";

            var safePaperNum = paperNum.Replace("'", "''");
            var spSql = $"EXEC HPSdFindEmpSpeHis N'{safePaperNum}', {item}";
            await using (var cmdSp = new SqlCommand(spSql, conn))
            await using (var rdrSp = await cmdSp.ExecuteReaderAsync())
            {
                if (await rdrSp.ReadAsync())
                {
                    try { empId    = rdrSp.IsDBNull(rdrSp.GetOrdinal("EmpId"))    ? "" : rdrSp.GetString(rdrSp.GetOrdinal("EmpId")); }    catch { }
                    try { bookYear = rdrSp.IsDBNull(rdrSp.GetOrdinal("BookYear")) ? "" : rdrSp.GetValue(rdrSp.GetOrdinal("BookYear"))?.ToString() ?? ""; } catch { }
                }
            }

            if (string.IsNullOrWhiteSpace(empId))
                return Ok(new { ok = false, error = "找不到員工假別資料（SP 無回傳 EmpId）" });

            // Step 2: BookYear 可能是逗號分隔的年份字串，只允許數字與逗號防止 SQL injection
            var safeYear = new string(bookYear.Where(c => char.IsDigit(c) || c == ',').ToArray());
            if (string.IsNullOrWhiteSpace(safeYear))
                return Ok(new { ok = false, error = $"BookYear 格式不正確：{bookYear}" });

            // Step 3: 查詢特休假資料
            // 對應 Delphi: select * from HPSdEmpSpecial(nolock) where EmpId='...' and BookYear in (...) and SpecialId=1
            var rows    = new List<Dictionary<string, object?>>();
            var columns = new List<string>();

            var sql = $"SELECT * FROM HPSdEmpSpecial WITH (NOLOCK) WHERE EmpId = @EmpId AND BookYear IN ({safeYear}) AND SpecialId = 1";
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@EmpId", empId);

            await using var rdr = await cmd.ExecuteReaderAsync();
            for (int i = 0; i < rdr.FieldCount; i++)
                columns.Add(rdr.GetName(i));

            while (await rdr.ReadAsync())
            {
                var row = new Dictionary<string, object?>();
                for (int i = 0; i < rdr.FieldCount; i++)
                    row[rdr.GetName(i)] = rdr.IsDBNull(i) ? null : rdr.GetValue(i);
                rows.Add(row);
            }

            return Ok(new { ok = true, empId, bookYear, columns, rows });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    // ─────────────────────────────────────────────────────────────
    // btnC4: 上傳附件
    // 對應 Delphi: MPHdUploadNote.dll (sPaperId='HPSdTimeOffSub')
    //   1. 確認 AttchFile 為空（否則提示先刪除）
    //   2. 取得 AttchFilePath
    //   3. 存檔: {AttchFilePath}{PaperNum}-{Item}{ext}
    //   4. UPDATE HPSdTimeOffSub SET AttchFile=filename WHERE PaperNum=? AND Item=?
    // ─────────────────────────────────────────────────────────────
    [HttpPost("UploadAttachment")]
    [RequestSizeLimit(50 * 1024 * 1024)]
    public async Task<IActionResult> UploadAttachment([FromForm] UploadAttachRequest req)
    {
        var file      = req.File;
        var paperNum  = req.PaperNum;
        var item      = req.Item;

        if (file is null || file.Length == 0)
            return Ok(new { ok = false, error = "請選擇檔案" });
        if (string.IsNullOrWhiteSpace(paperNum))
            return Ok(new { ok = false, error = "缺少 PaperNum" });

        try
        {
            await using var conn = new SqlConnection(_cs);
            await conn.OpenAsync();

            // 確認尚未有附件（對應 Delphi: 已存在附件，請先刪除再上傳）
            var existing = await GetAttchFileAsync(conn, paperNum, item);
            if (!string.IsNullOrWhiteSpace(existing))
                return Ok(new { ok = false, error = "已存在附件，請先刪除再上傳！" });

            // 取得附件儲存路徑
            var attachPath = await GetAttachPathAsync(conn);
            if (string.IsNullOrWhiteSpace(attachPath))
                return Ok(new { ok = false, error = "尚未設定附件路徑（HPS/AttchFilePath）" });

            // 檔名格式: {PaperNum}-{Item}{ext}
            var ext      = Path.GetExtension(file.FileName ?? "");
            var safePaper = paperNum.Replace("'", "").Replace("/", "").Replace("\\", "");
            var fileOnly = $"{safePaper}-{item}{ext}";
            var fullPath = Path.GetFullPath(Path.Combine(attachPath, fileOnly));

            // 路徑穿越防護
            var safeBase = Path.GetFullPath(attachPath);
            if (!fullPath.StartsWith(safeBase, StringComparison.OrdinalIgnoreCase))
                return Ok(new { ok = false, error = "路徑不合法" });

            if (System.IO.File.Exists(fullPath))
                return Ok(new { ok = false, error = "檔名已存在，無法覆蓋" });

            // 儲存檔案
            Directory.CreateDirectory(attachPath);
            await using (var fs = System.IO.File.Create(fullPath))
                await file.CopyToAsync(fs);

            // UPDATE HPSdTimeOffSub SET AttchFile=? WHERE PaperNum=? AND Item=?
            await using var cmdUpd = new SqlCommand(
                "UPDATE HPSdTimeOffSub SET AttchFile=@FileOnly WHERE PaperNum=@PaperNum AND Item=@Item", conn);
            cmdUpd.Parameters.AddWithValue("@FileOnly",  fileOnly);
            cmdUpd.Parameters.AddWithValue("@PaperNum",  paperNum);
            cmdUpd.Parameters.AddWithValue("@Item",      item);
            await cmdUpd.ExecuteNonQueryAsync();

            return Ok(new { ok = true, fileName = fileOnly, message = "上傳成功" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HP000032 UploadAttachment failed");
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    // ─────────────────────────────────────────────────────────────
    // btnC5: 檢視附件
    // 對應 Delphi: MPHdViewNote.dll (sPaperId='HPSdTimeOffSub')
    //   1. 取得 AttchFilePath + AttchFile
    //   2. 組合完整路徑回傳檔案串流
    // ─────────────────────────────────────────────────────────────
    [HttpGet("ViewAttachment")]
    public async Task<IActionResult> ViewAttachment([FromQuery] string? paperNum, [FromQuery] int item)
    {
        if (string.IsNullOrWhiteSpace(paperNum))
            return Ok(new { ok = false, error = "缺少 PaperNum" });

        try
        {
            await using var conn = new SqlConnection(_cs);
            await conn.OpenAsync();

            var attachPath = await GetAttachPathAsync(conn);
            if (string.IsNullOrWhiteSpace(attachPath))
                return Ok(new { ok = false, error = "尚未設定附件路徑（HPS/AttchFilePath）" });

            var fileOnly = await GetAttchFileAsync(conn, paperNum, item);
            if (string.IsNullOrWhiteSpace(fileOnly))
                return Ok(new { ok = false, error = "此明細列尚無附件" });

            // 防止路徑穿越
            var safeFileName = Path.GetFileName(fileOnly);
            var fullPath     = Path.GetFullPath(Path.Combine(attachPath, safeFileName));
            var safeBase     = Path.GetFullPath(attachPath);
            if (!fullPath.StartsWith(safeBase, StringComparison.OrdinalIgnoreCase))
                return Ok(new { ok = false, error = "路徑不合法" });

            if (!System.IO.File.Exists(fullPath))
                return Ok(new { ok = false, error = $"檔案不存在：{safeFileName}" });

            var ext         = Path.GetExtension(safeFileName).ToLowerInvariant();
            var contentType = GetContentType(ext);
            var inline      = ext is ".pdf" or ".png" or ".jpg" or ".jpeg" or ".gif" or ".bmp";
            var disposition = inline ? "inline" : "attachment";
            Response.Headers.ContentDisposition =
                $"{disposition}; filename=\"{Uri.EscapeDataString(safeFileName)}\"";

            var stream = System.IO.File.OpenRead(fullPath);
            return File(stream, contentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HP000032 ViewAttachment failed");
            return StatusCode(500, ex.Message);
        }
    }

    // ─────────────────────────────────────────────────────────────
    // btnC6: 刪除附件
    // 對應 Delphi: MPHdDelNote.dll (sPaperId='HPSdTimeOffSub')
    //   1. 取得 AttchFile
    //   2. 刪除實體檔案
    //   3. UPDATE HPSdTimeOffSub SET AttchFile='' WHERE PaperNum=? AND Item=?
    // ─────────────────────────────────────────────────────────────
    [HttpPost("DeleteAttachment")]
    public async Task<IActionResult> DeleteAttachment([FromBody] DeleteAttachRequest? req)
    {
        if (req is null || string.IsNullOrWhiteSpace(req.PaperNum))
            return Ok(new { ok = false, error = "缺少參數" });

        try
        {
            await using var conn = new SqlConnection(_cs);
            await conn.OpenAsync();

            var attachPath = await GetAttachPathAsync(conn);
            if (string.IsNullOrWhiteSpace(attachPath))
                return Ok(new { ok = false, error = "尚未設定附件路徑（HPS/AttchFilePath）" });

            var fileOnly = await GetAttchFileAsync(conn, req.PaperNum, req.Item);
            if (!string.IsNullOrWhiteSpace(fileOnly))
            {
                // 刪除實體檔案
                var safeFileName = Path.GetFileName(fileOnly);
                var fullPath     = Path.GetFullPath(Path.Combine(attachPath, safeFileName));
                var safeBase     = Path.GetFullPath(attachPath);
                if (fullPath.StartsWith(safeBase, StringComparison.OrdinalIgnoreCase) &&
                    System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
            }

            // UPDATE HPSdTimeOffSub SET AttchFile='' WHERE PaperNum=? AND Item=?
            await using var cmdUpd = new SqlCommand(
                "UPDATE HPSdTimeOffSub SET AttchFile='' WHERE PaperNum=@PaperNum AND Item=@Item", conn);
            cmdUpd.Parameters.AddWithValue("@PaperNum", req.PaperNum);
            cmdUpd.Parameters.AddWithValue("@Item",     req.Item);
            await cmdUpd.ExecuteNonQueryAsync();

            return Ok(new { ok = true, message = "刪除成功" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HP000032 DeleteAttachment failed");
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    public sealed class DeleteAttachRequest
    {
        public string? PaperNum { get; set; }
        public int     Item     { get; set; }
    }

    public sealed class UploadAttachRequest
    {
        public IFormFile? File     { get; set; }
        public string?    PaperNum { get; set; }
        public int        Item     { get; set; }
    }
}
