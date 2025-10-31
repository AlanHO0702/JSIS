using Microsoft.AspNetCore.Mvc;
using PcbErpApi.Data;
using PcbErpApi.Models;
using System.IO;
using System.Threading.Tasks;

namespace PcbErpApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SystemGraphController : ControllerBase
    {
        private readonly PcbErpContext _context;
        private readonly string _graphPath = @"\\js2023\mis_STD\Client\SystemGraph";
        private readonly string _manualPath = @"\\js2023\mis_STD\Client\SystemManual";

        public SystemGraphController(PcbErpContext context)
        {
            _context = context;
        }

        public class UploadDto
        {
            public string SystemCode { get; set; } = "";
            public IFormFile File { get; set; } = null!;
        }

        // =========================
        // ✅ 上傳圖檔
        // =========================
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload([FromForm] UploadDto dto)
        {
            if (dto.File == null || dto.File.Length == 0)
                return BadRequest("未選擇檔案");
            if (string.IsNullOrWhiteSpace(dto.SystemCode))
                return BadRequest("系統代碼不可為空");

            var ext = Path.GetExtension(dto.File.FileName).ToLower();
            if (ext != ".bmp") return BadRequest("僅允許上傳 BMP 檔案");

            Directory.CreateDirectory(_graphPath);
            var saveName = dto.File.FileName.Trim();
            var savePath = Path.Combine(_graphPath, saveName);

            using (var fs = new FileStream(savePath, FileMode.Create))
            {
                await dto.File.CopyToAsync(fs);
            }

            var entity = await _context.CurdSystemSelects.FindAsync(dto.SystemCode);
            if (entity != null)
            {
                entity.GraphName = saveName;
                _context.Update(entity);
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "圖檔上傳成功", fileName = saveName });
        }

        // =========================
        // ✅ 上傳手冊
        // =========================
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadManual([FromForm] UploadDto dto)
        {
            if (dto.File == null || dto.File.Length == 0)
                return BadRequest("未選擇檔案");
            if (string.IsNullOrWhiteSpace(dto.SystemCode))
                return BadRequest("系統代碼不可為空");

            var ext = Path.GetExtension(dto.File.FileName).ToLower();
            if (ext != ".doc" && ext != ".docx")
                return BadRequest("僅允許上傳 Word 檔案");

            Directory.CreateDirectory(_manualPath);
            var saveName = dto.File.FileName.Trim();
            var savePath = Path.Combine(_manualPath, saveName);

            using (var fs = new FileStream(savePath, FileMode.Create))
            {
                await dto.File.CopyToAsync(fs);
            }

            var entity = await _context.CurdSystemSelects.FindAsync(dto.SystemCode);
            if (entity != null)
            {
                entity.ManualName = saveName;
                _context.Update(entity);
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "手冊上傳成功", fileName = saveName });
        }

        // =========================
        // ✅ 顯示圖檔
        // =========================
        [HttpGet]
        [Route("/api/SystemGraph/GetImage/{fileName}")]
        public IActionResult GetImage(string fileName)
        {
            var path = Path.Combine(_graphPath, fileName);
            if (!System.IO.File.Exists(path))
            {
                return NotFound(new
                {
                    message = "找不到圖檔",
                    path,
                    exists = System.IO.Directory.Exists(_graphPath),
                    fileName
                });
            }

            var bytes = System.IO.File.ReadAllBytes(path);
            return File(bytes, "image/bmp");
        }

        // ✅ 顯示手冊
        [HttpGet]
        [Route("/api/SystemGraph/GetManual/{fileName}")]
        public IActionResult GetManual(string fileName)
        {
            var path = Path.Combine(_manualPath, fileName);
            if (!System.IO.File.Exists(path))
                return NotFound(new { message = "找不到手冊", path, fileName });

            var bytes = System.IO.File.ReadAllBytes(path);
            return File(bytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", fileName);
        }

        // =========================
        // ✅ 刪除圖檔
        // =========================
        [HttpDelete]
        public async Task<IActionResult> Delete(string systemCode)
        {
            var entity = await _context.CurdSystemSelects.FindAsync(systemCode);
            if (entity == null) return NotFound();

            var fileName = entity.GraphName;
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                var path = Path.Combine(_graphPath, fileName);
                if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
                entity.GraphName = null;
                _context.Update(entity);
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "圖檔刪除成功" });
        }

        // ✅ 刪除手冊
        [HttpDelete]
        public async Task<IActionResult> DeleteManual(string systemCode)
        {
            var entity = await _context.CurdSystemSelects.FindAsync(systemCode);
            if (entity == null) return NotFound();

            var fileName = entity.ManualName;
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                var path = Path.Combine(_manualPath, fileName);
                if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
                entity.ManualName = null;
                _context.Update(entity);
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "手冊刪除成功" });
        }
    }
}
