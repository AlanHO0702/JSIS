namespace PcbErpApi.Services
{
    /// <summary>
    /// 送審圖檔服務 - 處理送審時自動產生圖檔的功能
    /// </summary>
    public class AuditImageService
    {
        private readonly MapService _mapService;
        private readonly ILogger<AuditImageService> _logger;

        public AuditImageService(MapService mapService, ILogger<AuditImageService> logger)
        {
            _mapService = mapService;
            _logger = logger;
        }

        /// <summary>
        /// 為送審自動產生所有圖檔
        /// </summary>
        public async Task<GenerateImagesResult> GenerateImagesForAuditAsync(
            string partNum,
            string revision)
        {
            var result = new GenerateImagesResult
            {
                PartNum = partNum,
                Revision = revision
            };

            try
            {
                _logger.LogInformation("開始為 {PartNum}/{Revision} 產生圖檔", partNum, revision);

                // 產生裁板圖 (Panel)
                await GenerateSingleImageAsync(result, partNum, revision, MapType.Panel, "裁板圖");

                // 產生排板圖 (Cut)
                await GenerateSingleImageAsync(result, partNum, revision, MapType.Cut, "排板圖");

                // 產生 PF 裁剪圖
                await GenerateSingleImageAsync(result, partNum, revision, MapType.PF, "PF裁剪圖");

                // 產生疊構圖 (Stackup)
                await GenerateSingleImageAsync(result, partNum, revision, MapType.Stackup, "疊構圖");

                result.Success = result.GeneratedFiles.Count > 0;
                result.Message = result.Success
                    ? $"成功產生 {result.GeneratedFiles.Count} 張圖片"
                    : "沒有產生任何圖片";

                _logger.LogInformation("圖檔產生完成: {Message}", result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "產生圖檔時發生錯誤");
                result.Success = false;
                result.Message = $"產生圖檔失敗: {ex.Message}";
                result.Errors.Add(ex.Message);
            }

            return result;
        }

        private async Task GenerateSingleImageAsync(
            GenerateImagesResult result,
            string partNum,
            string revision,
            MapType mapType,
            string typeName)
        {
            try
            {
                var path = await _mapService.GenerateAndSaveImageAsync(partNum, revision, mapType);

                result.GeneratedFiles.Add(new GeneratedImageInfo
                {
                    Type = typeName,
                    MapType = mapType,
                    FilePath = path,
                    FileName = Path.GetFileName(path),
                    Success = true
                });

                _logger.LogInformation("{TypeName} 產生成功: {Path}", typeName, path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "產生 {TypeName} 失敗", typeName);
                result.Errors.Add($"{typeName}: {ex.Message}");

                result.GeneratedFiles.Add(new GeneratedImageInfo
                {
                    Type = typeName,
                    MapType = mapType,
                    Success = false,
                    ErrorMessage = ex.Message
                });
            }
        }

        /// <summary>
        /// 批次產生多個產品的圖檔
        /// </summary>
        public async Task<Dictionary<string, GenerateImagesResult>> GenerateImagesForBatchAsync(
            List<(string PartNum, string Revision)> products)
        {
            var results = new Dictionary<string, GenerateImagesResult>();

            foreach (var (partNum, revision) in products)
            {
                var key = $"{partNum}/{revision}";
                try
                {
                    results[key] = await GenerateImagesForAuditAsync(partNum, revision);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "批次產生圖檔失敗: {Key}", key);
                    results[key] = new GenerateImagesResult
                    {
                        PartNum = partNum,
                        Revision = revision,
                        Success = false,
                        Message = ex.Message
                    };
                }
            }

            return results;
        }
    }

    /// <summary>
    /// 產生圖檔結果
    /// </summary>
    public class GenerateImagesResult
    {
        public string PartNum { get; set; } = string.Empty;
        public string Revision { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<GeneratedImageInfo> GeneratedFiles { get; set; } = new();
        public List<string> Errors { get; set; } = new();
    }

    /// <summary>
    /// 產生的圖檔資訊
    /// </summary>
    public class GeneratedImageInfo
    {
        public string Type { get; set; } = string.Empty;
        public MapType MapType { get; set; }
        public string? FilePath { get; set; }
        public string? FileName { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
