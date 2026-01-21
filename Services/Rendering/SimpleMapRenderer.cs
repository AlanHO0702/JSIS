using SkiaSharp;

namespace PcbErpApi.Services.Rendering
{
    /// <summary>
    /// 簡單圖形渲染器 - 用於測試和展示
    /// </summary>
    public class SimpleMapRenderer
    {
        private readonly ILogger<SimpleMapRenderer> _logger;
        private readonly ILoggerFactory _loggerFactory;

        public SimpleMapRenderer(ILogger<SimpleMapRenderer> logger, ILoggerFactory loggerFactory)
        {
            _logger = logger;
            _loggerFactory = loggerFactory;
        }

        /// <summary>
        /// 產生測試用的裁板圖
        /// </summary>
        public byte[] RenderTestPanelMap(int width = 1200, int height = 800)
        {
            var data = new PanelMapData
            {
                TotalWidth = 1245,
                TotalHeight = 940,
                VerticalLines = new List<float> { 408, 836 },
                HorizontalLines = new List<float> { 428 }
            };

            var renderer = new PanelMapRenderer(
                _loggerFactory.CreateLogger<PanelMapRenderer>()
            );

            return renderer.RenderPanelMap(data, width, height);
        }

        /// <summary>
        /// 產生佔位圖（當沒有真實資料時）
        /// </summary>
        public byte[] RenderPlaceholder(string message, int width = 800, int height = 600)
        {
            using var surface = SKSurface.Create(new SKImageInfo(width, height));
            var canvas = surface.Canvas;

            // 淺灰色背景
            canvas.Clear(new SKColor(240, 240, 240));

            // 繪製邊框
            using var borderPaint = new SKPaint
            {
                Color = SKColors.LightGray,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2
            };
            canvas.DrawRect(10, 10, width - 20, height - 20, borderPaint);

            // 繪製訊息
            using var textPaint = new SKPaint
            {
                Color = SKColors.Gray,
                TextSize = 32,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center,
                Typeface = SKTypeface.FromFamilyName("Arial")
            };
            canvas.DrawText(message, width / 2, height / 2, textPaint);

            // 繪製輔助文字
            using var smallTextPaint = new SKPaint
            {
                Color = SKColors.LightGray,
                TextSize = 16,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center
            };
            canvas.DrawText($"圖片尺寸: {width} x {height}", width / 2, height / 2 + 40, smallTextPaint);

            using var image = surface.Snapshot();
            using var data = image.Encode(SKEncodedImageFormat.Jpeg, 90);
            return data.ToArray();
        }

        /// <summary>
        /// 從 MapData 字串渲染（目前為測試版本）
        /// </summary>
        public byte[] RenderFromMapData(string mapData, int width = 800, int height = 600)
        {
            if (string.IsNullOrEmpty(mapData))
            {
                return RenderPlaceholder("無 MapData 資料", width, height);
            }

            // TODO: 實作真正的 MapData 解析和渲染
            // 目前先返回佔位圖
            return RenderPlaceholder($"MapData 長度: {mapData.Length}", width, height);
        }
    }
}
