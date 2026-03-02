using Microsoft.AspNetCore.Mvc;

namespace PcbErpApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CompanyAttachFileController : ControllerBase
{
    public sealed class UploadRequest
    {
        public IFormFile? File { get; set; }
    }

    private readonly IWebHostEnvironment _env;
    private readonly ILogger<CompanyAttachFileController> _logger;

    public CompanyAttachFileController(IWebHostEnvironment env, ILogger<CompanyAttachFileController> logger)
    {
        _env = env;
        _logger = logger;
    }

    private string BaseDir => Path.Combine(_env.ContentRootPath, "CompanyAttach");

    private bool IsSafePath(string filePath)
    {
        var dir = BaseDir;
        return filePath.StartsWith(dir + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
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
}
