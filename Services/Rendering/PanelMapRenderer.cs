using SkiaSharp;

namespace PcbErpApi.Services.Rendering
{
    /// <summary>
    /// Panel 裁板圖渲染器
    /// </summary>
    public class PanelMapRenderer
    {
        private readonly ILogger<PanelMapRenderer> _logger;
        private readonly SKPaint _linePaint;
        private readonly SKPaint _textPaint;
        private readonly SKPaint _dimPaint;

        public PanelMapRenderer(ILogger<PanelMapRenderer> logger)
        {
            _logger = logger;

            // 線條畫筆
            _linePaint = new SKPaint
            {
                Color = SKColors.Black,
                StrokeWidth = 2,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true
            };

            // 文字畫筆
            _textPaint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = 24,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Arial")
            };

            // 尺寸標註畫筆
            _dimPaint = new SKPaint
            {
                Color = SKColors.DarkGray,
                StrokeWidth = 1,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true
            };
        }

        public byte[] RenderPanelMap(PanelMapData data, int imageWidth, int imageHeight)
        {
            try
            {
                using var surface = SKSurface.Create(new SKImageInfo(imageWidth, imageHeight));
                var canvas = surface.Canvas;

                // 白色背景
                canvas.Clear(SKColors.White);

                if (data.TotalWidth <= 0 || data.TotalHeight <= 0)
                {
                    DrawPlaceholder(canvas, imageWidth, imageHeight, "無圖形資料");
                    return EncodeToJpeg(surface);
                }

                // 計算縮放比例（留少量邊距並放大）
                float margin = 10; // 固定邊距
                float scaleX = (imageWidth - margin * 2) / data.TotalWidth;
                float scaleY = (imageHeight - margin * 2) / data.TotalHeight;
                float scale = Math.Min(scaleX, scaleY);

                // 額外放大倍數（讓圖案更大）
                scale = scale * 3.0f;

                _logger.LogInformation("Panel縮放: TotalSize=({W},{H}), ImageSize=({IW},{IH}), Scale={S}",
                    data.TotalWidth, data.TotalHeight, imageWidth, imageHeight, scale);

                // 計算置中偏移
                float scaledWidth = data.TotalWidth * scale;
                float scaledHeight = data.TotalHeight * scale;
                float offsetX = (imageWidth - scaledWidth) / 2;
                float offsetY = (imageHeight - scaledHeight) / 2;

                canvas.Translate(offsetX, offsetY);
                canvas.Scale(scale);

                // 繪製外框
                DrawOuterFrame(canvas, data);

                // 繪製內部分割線
                DrawDivisionLines(canvas, data);

                // 繪製尺寸標註
                DrawDimensions(canvas, data, scale);

                return EncodeToJpeg(surface);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rendering panel map");
                throw;
            }
        }

        private void DrawOuterFrame(SKCanvas canvas, PanelMapData data)
        {
            var rect = new SKRect(0, 0, data.TotalWidth, data.TotalHeight);
            canvas.DrawRect(rect, _linePaint);
        }

        private void DrawDivisionLines(SKCanvas canvas, PanelMapData data)
        {
            // 垂直分割線
            foreach (var x in data.VerticalLines)
            {
                canvas.DrawLine(x, 0, x, data.TotalHeight, _linePaint);
            }

            // 水平分割線
            foreach (var y in data.HorizontalLines)
            {
                canvas.DrawLine(0, y, data.TotalWidth, y, _linePaint);
            }
        }

        private void DrawDimensions(SKCanvas canvas, PanelMapData data, float scale)
        {
            // 頂部總尺寸標註
            DrawHorizontalDimension(
                canvas,
                0, -30 / scale,
                data.TotalWidth, -30 / scale,
                data.TotalWidth.ToString("F0"),
                scale
            );

            // 左側總尺寸標註
            DrawVerticalDimension(
                canvas,
                -30 / scale, 0,
                -30 / scale, data.TotalHeight,
                data.TotalHeight.ToString("F0"),
                scale
            );
        }

        private void DrawHorizontalDimension(
            SKCanvas canvas,
            float x1, float y1,
            float x2, float y2,
            string text,
            float scale)
        {
            // 繪製水平尺寸線
            canvas.DrawLine(x1, y1, x2, y2, _dimPaint);

            // 繪製箭頭
            DrawArrow(canvas, x1, y1, -1, 0, scale);
            DrawArrow(canvas, x2, y2, 1, 0, scale);

            // 繪製文字
            canvas.Save();
            canvas.Scale(1 / scale);
            float textX = (x1 + x2) / 2 * scale;
            float textY = (y1 - 5) * scale;

            var textBounds = new SKRect();
            _textPaint.MeasureText(text, ref textBounds);
            textX -= textBounds.Width / 2;

            canvas.DrawText(text, textX, textY, _textPaint);
            canvas.Restore();
        }

        private void DrawVerticalDimension(
            SKCanvas canvas,
            float x1, float y1,
            float x2, float y2,
            string text,
            float scale)
        {
            canvas.DrawLine(x1, y1, x2, y2, _dimPaint);
            DrawArrow(canvas, x1, y1, 0, -1, scale);
            DrawArrow(canvas, x2, y2, 0, 1, scale);

            canvas.Save();
            canvas.Scale(1 / scale);
            float textX = (x1 - 5) * scale;
            float textY = (y1 + y2) / 2 * scale;
            canvas.RotateDegrees(-90, textX, textY);

            var textBounds = new SKRect();
            _textPaint.MeasureText(text, ref textBounds);
            textY += textBounds.Width / 2;

            canvas.DrawText(text, textX, textY, _textPaint);
            canvas.Restore();
        }

        private void DrawArrow(SKCanvas canvas, float x, float y, int dirX, int dirY, float scale)
        {
            float arrowSize = 10 / scale;
            using var path = new SKPath();

            if (dirX != 0) // 水平箭頭
            {
                path.MoveTo(x, y);
                path.LineTo(x - dirX * arrowSize, y - arrowSize / 2);
                path.LineTo(x - dirX * arrowSize, y + arrowSize / 2);
                path.Close();
            }
            else // 垂直箭頭
            {
                path.MoveTo(x, y);
                path.LineTo(x - arrowSize / 2, y - dirY * arrowSize);
                path.LineTo(x + arrowSize / 2, y - dirY * arrowSize);
                path.Close();
            }

            using var arrowPaint = new SKPaint
            {
                Color = SKColors.DarkGray,
                Style = SKPaintStyle.Fill
            };
            canvas.DrawPath(path, arrowPaint);
        }

        private void DrawPlaceholder(SKCanvas canvas, int width, int height, string message)
        {
            using var textPaint = new SKPaint
            {
                Color = SKColors.Gray,
                TextSize = 32,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center
            };

            canvas.DrawText(message, width / 2, height / 2, textPaint);
        }

        private byte[] EncodeToJpeg(SKSurface surface)
        {
            using var image = surface.Snapshot();
            using var data = image.Encode(SKEncodedImageFormat.Jpeg, 90);
            return data.ToArray();
        }
    }

    /// <summary>
    /// Panel 圖資料模型
    /// </summary>
    public class PanelMapData
    {
        public float TotalWidth { get; set; }
        public float TotalHeight { get; set; }
        public List<float> VerticalLines { get; set; } = new();
        public List<float> HorizontalLines { get; set; } = new();
    }
}
