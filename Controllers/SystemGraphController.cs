using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace PcbErpApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class SystemGraphController : ControllerBase
{
    private static readonly (string Ext, string ContentType)[] Allowed =
    [
        (".png", "image/png"),
        (".jpg", "image/jpeg"),
        (".jpeg", "image/jpeg"),
        (".gif", "image/gif"),
        (".webp", "image/webp"),
        (".svg", "image/svg+xml"),
        (".bmp", "image/bmp"),
    ];

    private readonly IWebHostEnvironment _env;

    public SystemGraphController(IWebHostEnvironment env)
    {
        _env = env;
    }

    // GET /api/SystemGraph/CUR
    [HttpGet("{systemId}")]
    public IActionResult Get(string systemId)
    {
        var id = (systemId ?? string.Empty).Trim();
        if (id.Length == 0) return BadRequest("systemId is required.");
        if (id.Length > 64) return BadRequest("systemId too long.");
        if (id.IndexOfAny(['/', '\\']) >= 0) return BadRequest("invalid systemId.");
        if (!id.All(ch => char.IsLetterOrDigit(ch) || ch is '_' or '-')) return BadRequest("invalid systemId.");

        var baseDir = Path.Combine(_env.ContentRootPath, "SystemGraph");
        if (!Directory.Exists(baseDir)) return NotFound();

        // 1) Match: SystemGraph/{id}.* (case-insensitive)
        // If multiple, pick the shortest filename (prefer exact match).
        var files =
            Directory.EnumerateFiles(baseDir, "*", SearchOption.TopDirectoryOnly)
                .Select(p => new FileInfo(p))
                .ToList();

        var candidates = files
            .Where(fi => string.Equals(Path.GetFileNameWithoutExtension(fi.Name), id, StringComparison.OrdinalIgnoreCase))
            .OrderBy(fi => fi.Name.Length)
            .ToList();

        // 2) Fallback: if not found, allow matching by system name (e.g. Chinese filename contains "系統管理")
        // Client can pass: /api/SystemGraph/CUR?name=系統管理
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
        var allowed = Allowed.FirstOrDefault(x => x.Ext == ext);
        if (allowed.Ext is null) return NotFound();

        return PhysicalFile(file.FullName, allowed.ContentType, enableRangeProcessing: true);
    }
}
