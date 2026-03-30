using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;

namespace PcbErpApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CompanyAttachFileController : ControllerBase
{
    public sealed class UploadRequest
    {
        public IFormFile? File { get; set; }
    }

    public sealed class MasAttachUploadRequest
    {
        public IFormFile? File { get; set; }
        public string? TableName { get; set; }
        public string? PaperNum { get; set; }
        public string? ItemId { get; set; }
        public string? UserId { get; set; }
    }

    private readonly IWebHostEnvironment _env;
    private readonly ILogger<CompanyAttachFileController> _logger;
    private readonly PcbErpContext _ctx;

    public CompanyAttachFileController(IWebHostEnvironment env, ILogger<CompanyAttachFileController> logger, PcbErpContext ctx)
    {
        _env = env;
        _logger = logger;
        _ctx = ctx;
    }

    private string BaseDir
    {
        get
        {
            // Keep uploaded files outside the project root so dotnet-watch
            // does not treat uploads as source changes and force browser refresh.
            var parent = Directory.GetParent(_env.ContentRootPath)?.FullName;
            var root = string.IsNullOrWhiteSpace(parent) ? _env.ContentRootPath : parent;
            return Path.Combine(root, "JSIS_Uploads", "CompanyAttach");
        }
    }

    private bool IsSafePath(string filePath)
    {
        var dir = BaseDir;
        return filePath.StartsWith(dir + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 從 CURdSysParams 讀取夾檔路徑（AttachPath），跟 Delphi sAttachPath 一致
    /// </summary>
    private async Task<string?> GetAttachPathAsync()
    {
        var cs = _ctx.Database.GetConnectionString();
        await using var conn = new SqlConnection(cs);
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Value FROM CURdSysParams(NOLOCK) WHERE SystemId='CUR' AND ParamId='AttachPath'";
        var result = await cmd.ExecuteScalarAsync();
        var path = result?.ToString()?.Trim();
        if (string.IsNullOrWhiteSpace(path)) return null;
        if (!path.EndsWith(Path.DirectorySeparatorChar.ToString()))
            path += Path.DirectorySeparatorChar;
        return path;
    }

    [HttpPost("Upload")]
    [RequestSizeLimit(50 * 1024 * 1024)]
    public async Task<IActionResult> Upload([FromForm] UploadRequest request)
    {
        var file = request?.File;
        if (file is null || file.Length == 0)
            return BadRequest("file is required.");

        var originalName = Path.GetFileName(file.FileName ?? string.Empty);
        if (string.IsNullOrWhiteSpace(originalName))
            return BadRequest("invalid file name.");

        Directory.CreateDirectory(BaseDir);

        var ext = Path.GetExtension(originalName);
        var saveName = $"{DateTime.Now:yyyyMMddHHmmssfff}_{Guid.NewGuid():N}{ext}";
        var dest = Path.Combine(BaseDir, saveName);
        if (!IsSafePath(dest))
            return BadRequest("invalid path.");

        try
        {
            await using var fs = System.IO.File.Create(dest);
            await file.CopyToAsync(fs);
            return Ok(new
            {
                fileName = saveName,
                originalName,
                fullPath = dest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Upload company attachment failed: {File}", originalName);
            return StatusCode(500, "上傳失敗");
        }
    }

    /// <summary>
    /// 單頭夾檔上傳：檔案存放到 {BaseDir}/{tableName}/{paperNum}/，並呼叫 CURdMasAttachAct 寫入資料庫
    /// </summary>
    [HttpPost("MasAttachUpload")]
    [RequestSizeLimit(50 * 1024 * 1024)]
    public async Task<IActionResult> MasAttachUpload(
        [FromForm] MasAttachUploadRequest request)
    {
        var file = request?.File;
        var tableName = request?.TableName;
        var paperNum = request?.PaperNum;
        var itemId = request?.ItemId;
        var userId = request?.UserId;

        if (file is null || file.Length == 0)
            return BadRequest("請選擇檔案");
        if (string.IsNullOrWhiteSpace(tableName) || string.IsNullOrWhiteSpace(paperNum))
            return BadRequest("tableName 與 paperNum 為必填");

        var originalName = Path.GetFileName(file.FileName ?? string.Empty);
        if (string.IsNullOrWhiteSpace(originalName))
            return BadRequest("無效的檔案名稱");

        // 安全性檢查：tableName / paperNum 只允許合法字元
        if (!System.Text.RegularExpressions.Regex.IsMatch(tableName, @"^[A-Za-z0-9_]+$"))
            return BadRequest("tableName 不合法");
        if (!System.Text.RegularExpressions.Regex.IsMatch(paperNum, @"^[A-Za-z0-9_\-]+$"))
            return BadRequest("paperNum 不合法");

        // 從 DB 讀取夾檔根路徑（跟 Delphi sAttachPath 一致）
        var attachPath = await GetAttachPathAsync();
        if (string.IsNullOrWhiteSpace(attachPath))
            return BadRequest("尚未設定夾檔路徑!!");

        // 存放路徑：{sAttachPath}/{tableName}/{paperNum}/
        var folder = Path.Combine(attachPath, tableName, paperNum);
        var dest = Path.Combine(folder, originalName);

        // 防止路徑穿越
        var fullFolder = Path.GetFullPath(folder);
        var fullDest = Path.GetFullPath(dest);
        if (!fullDest.StartsWith(fullFolder + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(fullDest, fullFolder, StringComparison.OrdinalIgnoreCase))
            return BadRequest("無效的路徑");

        // 檢查檔案是否已存在（跟 Delphi 邏輯一致）
        if (System.IO.File.Exists(dest))
            return Conflict("檔案已存在!!");

        try
        {
            Directory.CreateDirectory(folder);
            await using var fs = System.IO.File.Create(dest);
            await file.CopyToAsync(fs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MasAttach upload failed: {File}", originalName);
            return StatusCode(500, "檔案傳送失敗!!");
        }

        // 呼叫 stored procedure 寫入 CURdMasAttachment
        try
        {
            var cs = _ctx.Database.GetConnectionString();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "exec CURdMasAttachAct @TableName, @PaperNum, @FileName, 0, 0, @ItemId, @UserId";
            cmd.Parameters.AddWithValue("@TableName", tableName);
            cmd.Parameters.AddWithValue("@PaperNum", paperNum);
            cmd.Parameters.AddWithValue("@FileName", originalName);
            cmd.Parameters.AddWithValue("@ItemId", itemId ?? "");
            cmd.Parameters.AddWithValue("@UserId", userId ?? "");
            await cmd.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MasAttach DB insert failed: {File}", originalName);
            return StatusCode(500, "寫入路徑失敗!!");
        }

        return Ok(new { fileName = originalName, message = "上傳成功" });
    }

    /// <summary>
    /// 單頭夾檔下載/檢視：回傳檔案串流供瀏覽器開啟
    /// </summary>
    [HttpGet("MasAttachView")]
    public async Task<IActionResult> MasAttachView([FromQuery] string? tableName, [FromQuery] string? paperNum, [FromQuery] string? fileName)
    {
        if (string.IsNullOrWhiteSpace(tableName) || string.IsNullOrWhiteSpace(paperNum) || string.IsNullOrWhiteSpace(fileName))
            return BadRequest("參數不完整");

        if (!System.Text.RegularExpressions.Regex.IsMatch(tableName, @"^[A-Za-z0-9_]+$"))
            return BadRequest("tableName 不合法");
        if (!System.Text.RegularExpressions.Regex.IsMatch(paperNum, @"^[A-Za-z0-9_\-]+$"))
            return BadRequest("paperNum 不合法");

        var safeFileName = Path.GetFileName(fileName);
        if (string.IsNullOrWhiteSpace(safeFileName))
            return BadRequest("fileName 不合法");

        var attachPath = await GetAttachPathAsync();
        if (string.IsNullOrWhiteSpace(attachPath))
            return BadRequest("尚未設定夾檔路徑!!");

        var filePath = Path.Combine(attachPath, tableName, paperNum, safeFileName);
        var fullPath = Path.GetFullPath(filePath);
        var fullFolder = Path.GetFullPath(Path.Combine(attachPath, tableName, paperNum));
        if (!fullPath.StartsWith(fullFolder + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(fullPath, fullFolder, StringComparison.OrdinalIgnoreCase))
            return BadRequest("無效的路徑");

        if (!System.IO.File.Exists(filePath))
            return NotFound("檔案不存在");

        var contentType = "application/octet-stream";
        var ext = Path.GetExtension(safeFileName).ToLowerInvariant();
        contentType = ext switch
        {
            ".pdf" => "application/pdf",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".txt" => "text/plain",
            ".csv" => "text/csv",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            _ => "application/octet-stream"
        };

        // PDF 和圖片在瀏覽器內預覽，其他格式下載讓使用者用本機程式開啟
        var inline = ext is ".pdf" or ".png" or ".jpg" or ".jpeg" or ".gif" or ".bmp";
        var disposition = inline ? "inline" : "attachment";
        Response.Headers.ContentDisposition = $"{disposition}; filename=\"{Uri.EscapeDataString(safeFileName)}\"";
        var stream = System.IO.File.OpenRead(filePath);
        return File(stream, contentType);
    }

    /// <summary>
    /// 單頭夾檔刪除：刪除檔案 + 呼叫 CURdMasAttachAct (mode=1)
    /// </summary>
    [HttpPost("MasAttachDelete")]
    public async Task<IActionResult> MasAttachDelete(
        [FromBody] MasAttachDeleteRequest? request)
    {
        if (request is null)
            return BadRequest("參數不完整");

        var tableName = request.TableName ?? "";
        var paperNum = request.PaperNum ?? "";
        var fileName = request.FileName ?? "";
        var item = request.Item;
        var itemId = request.ItemId ?? "";
        var userId = request.UserId ?? "";

        if (string.IsNullOrWhiteSpace(tableName) || string.IsNullOrWhiteSpace(paperNum))
            return BadRequest("tableName 與 paperNum 為必填");

        if (!System.Text.RegularExpressions.Regex.IsMatch(tableName, @"^[A-Za-z0-9_]+$"))
            return BadRequest("tableName 不合法");
        if (!System.Text.RegularExpressions.Regex.IsMatch(paperNum, @"^[A-Za-z0-9_\-]+$"))
            return BadRequest("paperNum 不合法");

        // 從 DB 讀取夾檔根路徑
        var attachPath = await GetAttachPathAsync();
        if (string.IsNullOrWhiteSpace(attachPath))
            return BadRequest("尚未設定夾檔路徑!!");

        // 刪除實體檔案
        if (!string.IsNullOrWhiteSpace(fileName))
        {
            var safeFileName = Path.GetFileName(fileName);
            if (!string.IsNullOrWhiteSpace(safeFileName))
            {
                var filePath = Path.Combine(attachPath, tableName, paperNum, safeFileName);
                var fullPath = Path.GetFullPath(filePath);
                var fullFolder = Path.GetFullPath(Path.Combine(attachPath, tableName, paperNum));
                if (fullPath.StartsWith(fullFolder + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        if (System.IO.File.Exists(filePath))
                            System.IO.File.Delete(filePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "MasAttach file delete failed: {File}", safeFileName);
                        return StatusCode(500, "檔案移除失敗!!");
                    }
                }
            }
        }

        // 呼叫 stored procedure 刪除記錄 (mode=1)
        try
        {
            var cs = _ctx.Database.GetConnectionString();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "exec CURdMasAttachAct @TableName, @PaperNum, '', @Item, 1, @ItemId, @UserId";
            cmd.Parameters.AddWithValue("@TableName", tableName);
            cmd.Parameters.AddWithValue("@PaperNum", paperNum);
            cmd.Parameters.AddWithValue("@Item", item);
            cmd.Parameters.AddWithValue("@ItemId", itemId);
            cmd.Parameters.AddWithValue("@UserId", userId);
            await cmd.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MasAttach DB delete failed");
            return StatusCode(500, "刪除夾檔失敗!!");
        }

        return Ok(new { message = "刪除成功" });
    }

    public sealed class MasAttachDeleteRequest
    {
        public string? TableName { get; set; }
        public string? PaperNum { get; set; }
        public string? FileName { get; set; }
        public int Item { get; set; }
        public string? ItemId { get; set; }
        public string? UserId { get; set; }
    }

    /// <summary>
    /// 明細表檔案載入：上傳檔案後，在指定的 detail table 新增一筆（FileName + FilePath）
    /// 對應 Delphi: MeetingPaper.pas btnProdMapSetClick
    /// </summary>
    [HttpPost("DetailFileUpload")]
    [RequestSizeLimit(50 * 1024 * 1024)]
    public async Task<IActionResult> DetailFileUpload([FromForm] DetailFileUploadRequest request)
    {
        var file = request?.File;
        var detailTable = request?.DetailTable;
        var paperNum = request?.PaperNum;

        if (file is null || file.Length == 0)
            return BadRequest("請選擇檔案");
        if (string.IsNullOrWhiteSpace(detailTable) || string.IsNullOrWhiteSpace(paperNum))
            return BadRequest("DetailTable 與 PaperNum 為必填");

        if (!System.Text.RegularExpressions.Regex.IsMatch(detailTable, @"^[A-Za-z0-9_]+$"))
            return BadRequest("DetailTable 不合法");
        if (!System.Text.RegularExpressions.Regex.IsMatch(paperNum, @"^[A-Za-z0-9_\-]+$"))
            return BadRequest("PaperNum 不合法");

        var originalName = Path.GetFileName(file.FileName ?? string.Empty);
        if (string.IsNullOrWhiteSpace(originalName))
            return BadRequest("無效的檔案名稱");

        // 儲存檔案到 BaseDir/{detailTable}/{paperNum}/
        var folder = Path.Combine(BaseDir, detailTable, paperNum);
        var dest = Path.Combine(folder, originalName);
        var fullFolder = Path.GetFullPath(folder);
        var fullDest = Path.GetFullPath(dest);
        if (!fullDest.StartsWith(fullFolder + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(fullDest, fullFolder, StringComparison.OrdinalIgnoreCase))
            return BadRequest("無效的路徑");

        try
        {
            Directory.CreateDirectory(folder);
            await using var fs = System.IO.File.Create(dest);
            await file.CopyToAsync(fs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DetailFileUpload file save failed: {File}", originalName);
            return StatusCode(500, "檔案傳送失敗");
        }

        // 在 detail table 新增一列（FileName + FilePath）
        try
        {
            var cs = _ctx.Database.GetConnectionString();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            // 辭典表名 → 實際表名
            var realTable = detailTable;
            await using (var cmdResolve = conn.CreateCommand())
            {
                cmdResolve.CommandText = @"
                    SELECT TOP 1 ISNULL(NULLIF(RealTableName,''), TableName)
                    FROM CURdTableName WITH (NOLOCK)
                    WHERE TableName = @tbl";
                cmdResolve.Parameters.AddWithValue("@tbl", detailTable);
                var resolved = await cmdResolve.ExecuteScalarAsync();
                if (resolved != null && resolved != DBNull.Value)
                    realTable = resolved.ToString()!;
            }

            // 動態查詢表的欄位名稱，找出項次欄位（Item 或 NumId 等）
            var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            await using (var cmdCols = conn.CreateCommand())
            {
                cmdCols.CommandText = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME=@t";
                cmdCols.Parameters.AddWithValue("@t", realTable);
                await using var rd = await cmdCols.ExecuteReaderAsync();
                while (await rd.ReadAsync())
                    columns.Add(rd.GetString(0));
            }

            // 找項次欄位：嘗試多種常見命名
            string? itemCol = null;
            foreach (var candidate in new[] { "Item", "NumId", "Seq", "SeqNo", "ItemNo", "SortOrder", "Idx" })
            {
                if (columns.Contains(candidate)) { itemCol = candidate; break; }
            }

            if (itemCol == null)
            {
                _logger.LogWarning("DetailFileUpload: 表 {Table}(實際:{Real}) 找不到項次欄位，所有欄位: {Cols}",
                    detailTable, realTable, string.Join(", ", columns));
                return StatusCode(500, $"明細表 {realTable} 找不到項次欄位，欄位: {string.Join(", ", columns)}");
            }

            // 取下一個項次
            await using var cmdMax = conn.CreateCommand();
            cmdMax.CommandText = $"SELECT ISNULL(MAX([{itemCol}]),0)+1 FROM [{realTable}] WHERE [PaperNum]=@PaperNum";
            cmdMax.Parameters.AddWithValue("@PaperNum", paperNum);
            var nextItem = Convert.ToInt32(await cmdMax.ExecuteScalarAsync());

            // 動態組 INSERT（只插入存在的欄位）
            var cols = new List<string> { "PaperNum", $"{itemCol}" };
            var pars = new List<string> { "@PaperNum", "@ItemVal" };
            await using var cmdIns = conn.CreateCommand();
            cmdIns.Parameters.AddWithValue("@PaperNum", paperNum);
            cmdIns.Parameters.AddWithValue("@ItemVal", nextItem);

            if (columns.Contains("FileName"))
            {
                cols.Add("FileName");
                pars.Add("@FileName");
                cmdIns.Parameters.AddWithValue("@FileName", originalName);
            }
            if (columns.Contains("FilePath"))
            {
                cols.Add("FilePath");
                pars.Add("@FilePath");
                cmdIns.Parameters.AddWithValue("@FilePath", fullFolder + Path.DirectorySeparatorChar);
            }

            cmdIns.CommandText = $"INSERT INTO [{realTable}] ({string.Join(",", cols.Select(c => $"[{c}]"))}) VALUES ({string.Join(",", pars)})";
            await cmdIns.ExecuteNonQueryAsync();

            return Ok(new { fileName = originalName, item = nextItem, message = "上傳成功" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DetailFileUpload DB insert failed: {File}", originalName);
            return StatusCode(500, "寫入明細失敗：" + ex.Message);
        }
    }

    public sealed class DetailFileUploadRequest
    {
        public IFormFile? File { get; set; }
        public string? DetailTable { get; set; }
        public string? PaperNum { get; set; }
    }

    /// <summary>
    /// 明細表檔案檢視：回傳 DetailFileUpload 存放的檔案
    /// </summary>
    [HttpGet("DetailFileView")]
    public IActionResult DetailFileView([FromQuery] string? detailTable, [FromQuery] string? paperNum, [FromQuery] string? fileName)
    {
        if (string.IsNullOrWhiteSpace(detailTable) || string.IsNullOrWhiteSpace(paperNum) || string.IsNullOrWhiteSpace(fileName))
            return BadRequest("參數不完整");

        if (!System.Text.RegularExpressions.Regex.IsMatch(detailTable, @"^[A-Za-z0-9_]+$"))
            return BadRequest("detailTable 不合法");
        if (!System.Text.RegularExpressions.Regex.IsMatch(paperNum, @"^[A-Za-z0-9_\-]+$"))
            return BadRequest("paperNum 不合法");

        var safeFileName = Path.GetFileName(fileName);
        if (string.IsNullOrWhiteSpace(safeFileName))
            return BadRequest("fileName 不合法");

        var filePath = Path.Combine(BaseDir, detailTable, paperNum, safeFileName);
        var fullPath = Path.GetFullPath(filePath);
        var fullFolder = Path.GetFullPath(Path.Combine(BaseDir, detailTable, paperNum));
        if (!fullPath.StartsWith(fullFolder + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(fullPath, fullFolder, StringComparison.OrdinalIgnoreCase))
            return BadRequest("無效的路徑");

        if (!System.IO.File.Exists(filePath))
            return NotFound("檔案不存在");

        var ext = Path.GetExtension(safeFileName).ToLowerInvariant();
        var contentType = ext switch
        {
            ".pdf" => "application/pdf",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".txt" => "text/plain",
            ".csv" => "text/csv",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            _ => "application/octet-stream"
        };

        var inline = ext is ".pdf" or ".png" or ".jpg" or ".jpeg" or ".gif" or ".bmp";
        var disposition = inline ? "inline" : "attachment";
        Response.Headers.ContentDisposition = $"{disposition}; filename=\"{Uri.EscapeDataString(safeFileName)}\"";
        var stream = System.IO.File.OpenRead(filePath);
        return File(stream, contentType);
    }
}
