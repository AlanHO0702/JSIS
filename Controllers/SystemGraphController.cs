using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using System.Linq;

namespace PcbErpApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class SystemGraphController : ControllerBase
{
    private static readonly (string Ext, string ContentType)[] AllowedImages =
    [
        (".png",  "image/png"),
        (".jpg",  "image/jpeg"),
        (".jpeg", "image/jpeg"),
        (".gif",  "image/gif"),
        (".webp", "image/webp"),
        (".svg",  "image/svg+xml"),
        (".bmp",  "image/bmp"),
    ];

    private static readonly string[] AllowedManualExts = [".doc", ".docx", ".pdf"];

    private readonly IWebHostEnvironment _env;
    private readonly PcbErpContext _db;

    public SystemGraphController(IWebHostEnvironment env, PcbErpContext db)
    {
        _env = env;
        _db = db;
    }

    private string BaseDir => Path.Combine(_env.ContentRootPath, "SystemGraph");

    private bool IsSafePath(string filePath)
    {
        var dir = BaseDir;
        return filePath.StartsWith(dir + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }

    // ── GET /api/SystemGraph/GetImage/模組架構圖_04報價管理.bmp ──────────────
    [HttpGet("GetImage/{fileName}")]
    public IActionResult GetImage(string fileName)
    {
        var safe = Path.GetFileName(fileName ?? string.Empty);
        if (string.IsNullOrEmpty(safe)) return BadRequest("fileName is required.");

        var ext = Path.GetExtension(safe).ToLowerInvariant();
        var allowed = AllowedImages.FirstOrDefault(x => x.Ext == ext);
        if (allowed.Ext is null) return BadRequest("不支援的檔案類型。");

        var path = Path.Combine(BaseDir, safe);
        if (!IsSafePath(path)) return BadRequest("invalid path.");
        if (!System.IO.File.Exists(path)) return NotFound();

        return PhysicalFile(path, allowed.ContentType, enableRangeProcessing: true);
    }

    // ── GET /api/SystemGraph/GetManual/XXX使用手冊.docx ──────────────────────
    [HttpGet("GetManual/{fileName}")]
    public IActionResult GetManual(string fileName)
    {
        var safe = Path.GetFileName(fileName ?? string.Empty);
        if (string.IsNullOrEmpty(safe)) return BadRequest("fileName is required.");

        var ext = Path.GetExtension(safe).ToLowerInvariant();
        var contentType = ext switch
        {
            ".doc"  => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".pdf"  => "application/pdf",
            _       => null
        };
        if (contentType is null) return BadRequest("不支援的檔案類型。");

        var path = Path.Combine(BaseDir, safe);
        if (!IsSafePath(path)) return BadRequest("invalid path.");
        if (!System.IO.File.Exists(path)) return NotFound();

        return PhysicalFile(path, contentType, safe, enableRangeProcessing: true);
    }

    // ── POST /api/SystemGraph/Upload ─────────────────────────────────────────
    [HttpPost("Upload")]
    public async Task<IActionResult> Upload([FromForm] string systemCode, IFormFile file)
    {
        if (string.IsNullOrWhiteSpace(systemCode)) return BadRequest("systemCode is required.");
        if (file is null || file.Length == 0) return BadRequest("file is required.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowed = AllowedImages.FirstOrDefault(x => x.Ext == ext);
        if (allowed.Ext is null) return BadRequest("不支援的圖檔類型。");

        Directory.CreateDirectory(BaseDir);
        var safe = Path.GetFileName(file.FileName);
        var dest = Path.Combine(BaseDir, safe);
        if (!IsSafePath(dest)) return BadRequest("invalid path.");

        using (var fs = System.IO.File.Create(dest))
            await file.CopyToAsync(fs);

        var row = await _db.CurdSystemSelects.FirstOrDefaultAsync(r => r.SystemId == systemCode);
        if (row is null) return NotFound($"SystemId '{systemCode}' 不存在。");
        row.GraphName = safe;
        await _db.SaveChangesAsync();

        return Ok(new { fileName = safe });
    }

    // ── POST /api/SystemGraph/UploadManual ───────────────────────────────────
    [HttpPost("UploadManual")]
    public async Task<IActionResult> UploadManual([FromForm] string systemCode, IFormFile file)
    {
        if (string.IsNullOrWhiteSpace(systemCode)) return BadRequest("systemCode is required.");
        if (file is null || file.Length == 0) return BadRequest("file is required.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedManualExts.Contains(ext)) return BadRequest("不支援的手冊類型，請上傳 .doc / .docx / .pdf。");

        Directory.CreateDirectory(BaseDir);
        var safe = Path.GetFileName(file.FileName);
        var dest = Path.Combine(BaseDir, safe);
        if (!IsSafePath(dest)) return BadRequest("invalid path.");

        using (var fs = System.IO.File.Create(dest))
            await file.CopyToAsync(fs);

        var row = await _db.CurdSystemSelects.FirstOrDefaultAsync(r => r.SystemId == systemCode);
        if (row is null) return NotFound($"SystemId '{systemCode}' 不存在。");
        row.ManualName = safe;
        await _db.SaveChangesAsync();

        return Ok(new { fileName = safe });
    }

    // ── DELETE /api/SystemGraph/Delete?systemCode=CIR ────────────────────────
    [HttpDelete("Delete")]
    public async Task<IActionResult> Delete([FromQuery] string systemCode)
    {
        if (string.IsNullOrWhiteSpace(systemCode)) return BadRequest("systemCode is required.");

        var row = await _db.CurdSystemSelects.FirstOrDefaultAsync(r => r.SystemId == systemCode);
        if (row is null) return NotFound($"SystemId '{systemCode}' 不存在。");

        if (!string.IsNullOrEmpty(row.GraphName))
        {
            var path = Path.Combine(BaseDir, Path.GetFileName(row.GraphName));
            if (IsSafePath(path) && System.IO.File.Exists(path))
                System.IO.File.Delete(path);
            row.GraphName = null;
            await _db.SaveChangesAsync();
        }

        return Ok();
    }

    // ── DELETE /api/SystemGraph/DeleteManual?systemCode=CIR ──────────────────
    [HttpDelete("DeleteManual")]
    public async Task<IActionResult> DeleteManual([FromQuery] string systemCode)
    {
        if (string.IsNullOrWhiteSpace(systemCode)) return BadRequest("systemCode is required.");

        var row = await _db.CurdSystemSelects.FirstOrDefaultAsync(r => r.SystemId == systemCode);
        if (row is null) return NotFound($"SystemId '{systemCode}' 不存在。");

        if (!string.IsNullOrEmpty(row.ManualName))
        {
            var path = Path.Combine(BaseDir, Path.GetFileName(row.ManualName));
            if (IsSafePath(path) && System.IO.File.Exists(path))
                System.IO.File.Delete(path);
            row.ManualName = null;
            await _db.SaveChangesAsync();
        }

        return Ok();
    }

    // ── GET /api/SystemGraph/GetSop/XXX_SOP.pdf ──────────────────────────────
    [HttpGet("GetSop/{fileName}")]
    public IActionResult GetSop(string fileName)
    {
        var safe = Path.GetFileName(fileName ?? string.Empty);
        if (string.IsNullOrEmpty(safe)) return BadRequest("fileName is required.");

        var ext = Path.GetExtension(safe).ToLowerInvariant();
        var contentType = ext switch
        {
            ".doc"  => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".pdf"  => "application/pdf",
            _       => null
        };
        if (contentType is null) return BadRequest("不支援的檔案類型。");

        var path = Path.Combine(BaseDir, safe);
        if (!IsSafePath(path)) return BadRequest("invalid path.");
        if (!System.IO.File.Exists(path)) return NotFound();

        return PhysicalFile(path, contentType, safe, enableRangeProcessing: true);
    }

    // ── POST /api/SystemGraph/UploadSop ──────────────────────────────────────
    [HttpPost("UploadSop")]
    public async Task<IActionResult> UploadSop([FromForm] string systemCode, IFormFile file)
    {
        if (string.IsNullOrWhiteSpace(systemCode)) return BadRequest("systemCode is required.");
        if (file is null || file.Length == 0) return BadRequest("file is required.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedManualExts.Contains(ext)) return BadRequest("不支援的 SOP 類型，請上傳 .doc / .docx / .pdf。");

        Directory.CreateDirectory(BaseDir);
        var safe = Path.GetFileName(file.FileName);
        var dest = Path.Combine(BaseDir, safe);
        if (!IsSafePath(dest)) return BadRequest("invalid path.");

        using (var fs = System.IO.File.Create(dest))
            await file.CopyToAsync(fs);

        var row = await _db.CurdSystemSelects.FirstOrDefaultAsync(r => r.SystemId == systemCode);
        if (row is null) return NotFound($"SystemId '{systemCode}' 不存在。");
        row.SOPName = safe;
        await _db.SaveChangesAsync();

        return Ok(new { fileName = safe });
    }

    // ── DELETE /api/SystemGraph/DeleteSop?systemCode=CIR ─────────────────────
    [HttpDelete("DeleteSop")]
    public async Task<IActionResult> DeleteSop([FromQuery] string systemCode)
    {
        if (string.IsNullOrWhiteSpace(systemCode)) return BadRequest("systemCode is required.");

        var row = await _db.CurdSystemSelects.FirstOrDefaultAsync(r => r.SystemId == systemCode);
        if (row is null) return NotFound($"SystemId '{systemCode}' 不存在。");

        if (!string.IsNullOrEmpty(row.SOPName))
        {
            var path = Path.Combine(BaseDir, Path.GetFileName(row.SOPName));
            if (IsSafePath(path) && System.IO.File.Exists(path))
                System.IO.File.Delete(path);
            row.SOPName = null;
            await _db.SaveChangesAsync();
        }

        return Ok();
    }

    // ── GET /api/SystemGraph/CUR ──────────────────────────────────────────────
    [HttpGet("{systemId}")]
    public IActionResult Get(string systemId)
    {
        var id = (systemId ?? string.Empty).Trim();
        if (id.Length == 0) return BadRequest("systemId is required.");
        if (id.Length > 64) return BadRequest("systemId too long.");
        if (id.IndexOfAny(['/', '\\']) >= 0) return BadRequest("invalid systemId.");
        if (!id.All(ch => char.IsLetterOrDigit(ch) || ch is '_' or '-')) return BadRequest("invalid systemId.");

        if (!Directory.Exists(BaseDir)) return NotFound();

        var files = Directory.EnumerateFiles(BaseDir, "*", SearchOption.TopDirectoryOnly)
            .Select(p => new FileInfo(p))
            .ToList();

        var candidates = files
            .Where(fi => string.Equals(Path.GetFileNameWithoutExtension(fi.Name), id, StringComparison.OrdinalIgnoreCase))
            .OrderBy(fi => fi.Name.Length)
            .ToList();

        if (candidates.Count == 0)
        {
            var name = (Request.Query["name"].ToString() ?? string.Empty).Trim();
            if (name.Length > 0 && name.Length <= 64)
            {
                candidates = files
                    .Where(fi =>
                        fi.Name.Contains(name, StringComparison.OrdinalIgnoreCase) ||
                        Path.GetFileNameWithoutExtension(fi.Name).Contains(name, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(fi => fi.Name.Length)
                    .ToList();
            }
        }

        if (candidates.Count == 0) return NotFound();

        var file = candidates[0];
        var ext = file.Extension.ToLowerInvariant();
        var allowed = AllowedImages.FirstOrDefault(x => x.Ext == ext);
        if (allowed.Ext is null) return NotFound();

        return PhysicalFile(file.FullName, allowed.ContentType, enableRangeProcessing: true);
    }
}
