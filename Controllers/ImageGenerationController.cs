using Microsoft.AspNetCore.Mvc;
using PcbErpApi.Services;

namespace PcbErpApi.Controllers
{
    [ApiController]
    [Route("api/imagegeneration")]
    public class ImageGenerationController : ControllerBase
    {
        private readonly AuditImageService _auditImageService;
        private readonly ILogger<ImageGenerationController> _logger;

        public ImageGenerationController(
            AuditImageService auditImageService,
            ILogger<ImageGenerationController> logger)
        {
            _auditImageService = auditImageService;
            _logger = logger;
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
                        message = "料號和版次不可為空"
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
