using Microsoft.AspNetCore.Mvc;
using PcbErpApi.Services;

namespace PcbErpApi.Controllers
{
    [ApiController]
    [Route("api/imagegeneration")]
    public class ImageGenerationController : ControllerBase
    {
        private readonly AuditImageService _auditImageService;
        private readonly MapService _mapService;
        private readonly ILogger<ImageGenerationController> _logger;

        public ImageGenerationController(
            AuditImageService auditImageService,
            MapService mapService,
            ILogger<ImageGenerationController> logger)
        {
            _auditImageService = auditImageService;
            _mapService = mapService;
            _logger = logger;
        }

        /// <summary>
        /// 取得工程圖預覽圖片
        /// </summary>
        [HttpGet("preview/{partNum}/{revision}/{mapType}")]
        public async Task<IActionResult> GetMapPreview(string partNum, string revision, string mapType)
        {
            try
            {
                _logger.LogInformation("取得工程圖預覽: {PartNum}/{Revision}/{MapType}", partNum, revision, mapType);

                // 將字串轉換為 MapType enum
                MapType mapTypeEnum;
                switch (mapType.ToLower())
                {
                    case "panel":
                        mapTypeEnum = MapType.Panel;
                        break;
                    case "cut":
                        mapTypeEnum = MapType.Cut;
                        break;
                    case "pf":
                        mapTypeEnum = MapType.PF;
                        break;
                    case "stackup":
                        mapTypeEnum = MapType.Stackup;
                        break;
                    default:
                        _logger.LogWarning("不支援的 MapType: {MapType}", mapType);
                        return BadRequest($"不支援的圖形類型: {mapType}");
                }

                // 渲染圖片（800x600 預覽尺寸）
                var imageBytes = await _mapService.RenderMapAsync(
                    partNum,
                    revision,
                    mapTypeEnum,
                    width: 800,
                    height: 600
                );

                // 返回圖片
                return File(imageBytes, "image/png");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得工程圖預覽失敗");

                // 返回錯誤圖片
                return File(new byte[0], "image/png");
            }
        }

        /// <summary>
        /// 產生工程圖檔
        /// </summary>
        [HttpPost("generate")]
        public async Task<IActionResult> GenerateImages([FromBody] GenerateImagesRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.PartNum) || string.IsNullOrEmpty(request.Revision))
                {
                    return Ok(new
                    {
                        success = false,
                        message = "料號和版序不可為空"
                    });
                }

                _logger.LogInformation("開始產生圖檔: {PartNum}/{Revision}", request.PartNum, request.Revision);

                var result = await _auditImageService.GenerateImagesForAuditAsync(
                    request.PartNum,
                    request.Revision
                );

                return Ok(new
                {
                    success = result.Success,
                    message = result.Message,
                    images = result.GeneratedFiles.Select(f => new
                    {
                        type = f.Type,
                        fileName = f.FileName,
                        success = f.Success,
                        errorMessage = f.ErrorMessage
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "產生圖檔時發生錯誤");
                return Ok(new
                {
                    success = false,
                    message = $"產生圖檔失敗: {ex.Message}"
                });
            }
        }

        public class GenerateImagesRequest
        {
            public string PartNum { get; set; } = string.Empty;
            public string Revision { get; set; } = string.Empty;
        }
    }
}
