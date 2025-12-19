using SkiaSharp;
using PcbErpApi.Services.MapData;

namespace PcbErpApi.Services.Rendering
{
    /// <summary>
    /// 通用圖形渲染器 - 根據解析的 MapData 渲染圖形
    /// </summary>
    public class UniversalMapRenderer
    {
        private readonly ILogger<UniversalMapRenderer> _logger;
        private readonly MapDataParser _parser;

        public UniversalMapRenderer(
            ILogger<UniversalMapRenderer> logger,
            MapDataParser parser)
        {
            _logger = logger;
            _parser = parser;
        }

        /// <summary>
        /// 從 MapData 字串渲染圖片
        /// </summary>
        public byte[] RenderFromMapData(string mapData, int imageWidth = 1200, int imageHeight = 800)
        {
            try
            {
                // 解析 MapData
                var parsed = _parser.Parse(mapData);

                if (parsed.Elements.Count == 0)
                {
                    _logger.LogWarning("No elements found in MapData");
                    return RenderPlaceholder("無法解析圖形資料", imageWidth, imageHeight);
                }

                // 計算邊界
                var bounds = CalculateBounds(parsed.Elements);
                _logger.LogInformation("Map bounds: ({MinX}, {MinY}) to ({MaxX}, {MaxY})",
                    bounds.MinX, bounds.MinY, bounds.MaxX, bounds.MaxY);

                // 建立畫布
                var info = new SKImageInfo(imageWidth, imageHeight);
                using var surface = SKSurface.Create(info);
                var canvas = surface.Canvas;

                // 背景
                canvas.Clear(SKColors.White);

                // 計算縮放比例
                var scaleX = (imageWidth - 160) / (bounds.MaxX - bounds.MinX);
                var scaleY = (imageHeight - 160) / (bounds.MaxY - bounds.MinY);
                var scale = Math.Min(scaleX, scaleY);

                // 偏移量 (置中)
                var offsetX = 80 + (imageWidth - 160 - (bounds.MaxX - bounds.MinX) * scale) / 2;
                var offsetY = 80 + (imageHeight - 160 - (bounds.MaxY - bounds.MinY) * scale) / 2;

                _logger.LogInformation("Scale: {Scale}, Offset: ({OffsetX}, {OffsetY})",
                    scale, offsetX, offsetY);

                // 渲染所有元素
                foreach (var element in parsed.Elements)
                {
                    RenderElement(canvas, element, scale, offsetX, offsetY, bounds);
                }

                // 輸出為 JPEG
                using var image = surface.Snapshot();
                using var data = image.Encode(SKEncodedImageFormat.Jpeg, 90);
                return data.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rendering MapData");
                return RenderPlaceholder($"渲染錯誤: {ex.Message}", imageWidth, imageHeight);
            }
        }

        private MapBounds CalculateBounds(List<MapElement> elements)
        {
            var bounds = new MapBounds
            {
                MinX = float.MaxValue,
                MinY = float.MaxValue,
                MaxX = float.MinValue,
                MaxY = float.MinValue
            };

            foreach (var element in elements)
            {
                bounds.MinX = Math.Min(bounds.MinX, element.X);
                bounds.MinY = Math.Min(bounds.MinY, element.Y);
                bounds.MaxX = Math.Max(bounds.MaxX, element.X + element.Width);
                bounds.MaxY = Math.Max(bounds.MaxY, element.Y + element.Height);
            }

            // 確保有效的邊界
            if (bounds.MinX == float.MaxValue || bounds.MaxX == float.MinValue)
            {
                bounds.MinX = 0;
                bounds.MinY = 0;
                bounds.MaxX = 1000;
                bounds.MaxY = 800;
            }

            // 加一點邊距
            var marginX = (bounds.MaxX - bounds.MinX) * 0.05f;
            var marginY = (bounds.MaxY - bounds.MinY) * 0.05f;

            bounds.MinX -= marginX;
            bounds.MinY -= marginY;
            bounds.MaxX += marginX;
            bounds.MaxY += marginY;

            return bounds;
        }

        private void RenderElement(SKCanvas canvas, MapElement element, float scale,
            float offsetX, float offsetY, MapBounds bounds)
        {
            // 轉換座標
            var x = (element.X - bounds.MinX) * scale + offsetX;
            var y = (element.Y - bounds.MinY) * scale + offsetY;
            var w = element.Width * scale;
            var h = element.Height * scale;

            switch (element.Type)
            {
                case MapElementType.Component:
                    RenderComponent(canvas, element, x, y, w, h);
                    break;

                case MapElementType.DrawLine:
                    RenderDrawLine(canvas, element, x, y, w, h);
                    break;

                case MapElementType.Line:
                    RenderLine(canvas, element, x, y, w, h);
                    break;

                case MapElementType.Rectangle:
                    RenderRectangle(canvas, element, x, y, w, h);
                    break;

                case MapElementType.Text:
                    RenderText(canvas, element, x, y);
                    break;
            }
        }

        private void RenderComponent(SKCanvas canvas, MapElement element, float x, float y, float w, float h)
        {
            // 元件類型: 1=普通元件(有框), 3=尺寸標註(無框)
            var isTextOnly = element.ComponentType == 3;

            // 解析顏色：Colors[0]=邊框, Colors[2]=填充
            var strokeColor = element.Colors.Count > 0 ? element.Colors[0] : "clBlue";

            var rect = new SKRect(x, y, x + w, y + h);

            // 只有非文字元件才繪製框
            if (!isTextOnly)
            {
                // 繪製填充（白色）
                using var fillPaint = new SKPaint
                {
                    Color = SKColors.White,
                    Style = SKPaintStyle.Fill,
                    IsAntialias = true
                };
                canvas.DrawRect(rect, fillPaint);

                // 繪製邊框 - 全部用黑色
                // 如果填充色是黑色(clBlack)，表示是最外圍大框，用粗框
                var isOuterFrame = element.Colors.Count > 2 &&
                                  element.Colors[2].ToLower() == "clblack";

                using var strokePaint = new SKPaint
                {
                    Color = SKColors.Black,  // 全部改成黑色
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = isOuterFrame ? 3 : 1,  // 外框粗一點，內框細一點
                    IsAntialias = true
                };
                canvas.DrawRect(rect, strokePaint);
            }

            // 繪製文字
            if (!string.IsNullOrEmpty(element.Text))
            {
                var fontSize = Math.Max(8, Math.Min(element.FontSize, 20));
                var textColor = element.Colors.Count > 3 ? element.Colors[3] : "clBlack";

                // 使用支援中文的字型
                var fontFamily = "Microsoft JhengHei";  // 微軟正黑體（支援中文）

                using var textPaint = new SKPaint
                {
                    Color = ParseColor(textColor),
                    TextSize = fontSize,
                    IsAntialias = true,
                    Typeface = SKTypeface.FromFamilyName(fontFamily)
                };

                float textX, textY;
                var textWidth = textPaint.MeasureText(element.Text);

                // 尺寸標註(type=3): 文字靠左，不置中
                if (isTextOnly)
                {
                    textX = x;  // 直接從 x 位置開始
                    textY = y + h / 2 + textPaint.TextSize / 3;
                }
                else
                {
                    // 一般元件: 置中
                    textX = x + w / 2 - textWidth / 2;
                    textY = y + h / 2 + textPaint.TextSize / 3;
                }

                canvas.DrawText(element.Text, textX, textY, textPaint);
            }
        }

        private void RenderDrawLine(SKCanvas canvas, MapElement element, float x, float y, float w, float h)
        {
            // DrawLine 是尺寸標註用的箭頭線
            using var paint = new SKPaint
            {
                Color = ParseColor(element.Colors.FirstOrDefault() ?? "clBlack"),
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1,
                IsAntialias = true
            };

            // 判斷是水平還是垂直線
            if (h < w)
            {
                // 水平箭頭線
                var centerY = y + h / 2;
                canvas.DrawLine(x, centerY, x + w, centerY, paint);

                // 繪製左箭頭
                DrawArrowHead(canvas, paint, x, centerY, -1, 0);
                // 繪製右箭頭
                DrawArrowHead(canvas, paint, x + w, centerY, 1, 0);
            }
            else
            {
                // 垂直箭頭線
                var centerX = x + w / 2;
                canvas.DrawLine(centerX, y, centerX, y + h, paint);

                // 繪製上箭頭
                DrawArrowHead(canvas, paint, centerX, y, 0, -1);
                // 繪製下箭頭
                DrawArrowHead(canvas, paint, centerX, y + h, 0, 1);
            }
        }

        private void DrawArrowHead(SKCanvas canvas, SKPaint paint, float x, float y, int dirX, int dirY)
        {
            // 箭頭大小
            const float arrowSize = 5;

            using var fillPaint = new SKPaint
            {
                Color = paint.Color,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };

            // 根據方向繪製箭頭
            var path = new SKPath();
            if (dirX != 0)  // 水平箭頭
            {
                path.MoveTo(x, y);
                path.LineTo(x - dirX * arrowSize, y - arrowSize / 2);
                path.LineTo(x - dirX * arrowSize, y + arrowSize / 2);
                path.Close();
            }
            else  // 垂直箭頭
            {
                path.MoveTo(x, y);
                path.LineTo(x - arrowSize / 2, y - dirY * arrowSize);
                path.LineTo(x + arrowSize / 2, y - dirY * arrowSize);
                path.Close();
            }

            canvas.DrawPath(path, fillPaint);
            path.Dispose();
        }

        private void RenderLine(SKCanvas canvas, MapElement element, float x, float y, float w, float h)
        {
            using var paint = new SKPaint
            {
                Color = SKColors.Black,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1,
                IsAntialias = true
            };

            canvas.DrawLine(x, y, x + w, y + h, paint);
        }

        private void RenderRectangle(SKCanvas canvas, MapElement element, float x, float y, float w, float h)
        {
            using var paint = new SKPaint
            {
                Color = ParseColor(element.Colors.FirstOrDefault() ?? "clBlack"),
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2,
                IsAntialias = true
            };

            canvas.DrawRect(x, y, w, h, paint);
        }

        private void RenderText(SKCanvas canvas, MapElement element, float x, float y)
        {
            if (string.IsNullOrEmpty(element.Text))
                return;

            using var paint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = element.FontSize,
                IsAntialias = true
            };

            canvas.DrawText(element.Text, x, y, paint);
        }

        private SKColor ParseColor(string colorName)
        {
            return colorName.ToLower() switch
            {
                "clblue" => SKColors.Blue,
                "clred" => SKColors.Red,
                "clgreen" => SKColors.Green,
                "cllime" => SKColors.Lime,
                "clblack" => SKColors.Black,
                "clwhite" => SKColors.White,
                "clgray" or "clgrey" => SKColors.Gray,
                "clyellow" => SKColors.Yellow,
                "claqua" => SKColors.Aqua,
                "clteal" => SKColors.Teal,
                "clnavy" => SKColors.Navy,
                "clmaroon" => SKColors.Maroon,
                "clpurple" => SKColors.Purple,
                "clfuchsia" => SKColors.Fuchsia,
                "clolive" => SKColors.Olive,
                "clsilver" => SKColors.Silver,
                "clorange" => SKColors.Orange,
                "clwindowtext" => SKColors.Black,
                "clbtnface" => SKColors.LightGray,
                _ => SKColors.LightGray  // 預設用淺灰色而不是黑色
            };
        }

        private byte[] RenderPlaceholder(string message, int width, int height)
        {
            var info = new SKImageInfo(width, height);
            using var surface = SKSurface.Create(info);
            var canvas = surface.Canvas;

            canvas.Clear(SKColors.WhiteSmoke);

            using var paint = new SKPaint
            {
                Color = SKColors.Gray,
                TextSize = 24,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center
            };

            canvas.DrawText(message, width / 2, height / 2, paint);

            using var image = surface.Snapshot();
            using var data = image.Encode(SKEncodedImageFormat.Jpeg, 90);
            return data.ToArray();
        }

        private class MapBounds
        {
            public float MinX { get; set; }
            public float MinY { get; set; }
            public float MaxX { get; set; }
            public float MaxY { get; set; }
        }
    }
}
