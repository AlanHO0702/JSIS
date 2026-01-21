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

        private MapType? _currentMapType;

        /// <summary>
        /// 從 MapData 字串渲染圖片
        /// </summary>
        public byte[] RenderFromMapData(string mapData, int imageWidth = 1200, int imageHeight = 800, MapType? mapType = null)
        {
            try
            {
                // 儲存當前圖形類型，供渲染時判斷
                _currentMapType = mapType;

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

                // 根據圖形類型限制最小縮放比例
                if (_currentMapType == MapType.Stackup)
                {
                    scale = Math.Max(scale, 2.0f);  // 疊構圖：最小縮放比例為 2.0
                }
                else if (_currentMapType == MapType.Panel || _currentMapType == MapType.Cut || _currentMapType == MapType.PF)
                {
                    scale = Math.Max(scale, 0.75f);  // 裁板/排板/PF圖：最小縮放比例為 4.0（讓圖更大）
                }

                // 偏移量 (置中)
                var offsetX = 0;
                var offsetY = 40;

                _logger.LogInformation("{MapType} Scale: {Scale}, Offset: ({OffsetX}, {OffsetY})",
                    _currentMapType, scale, offsetX, offsetY);

                // 渲染所有元素（分兩階段：先渲染元件和矩形，再渲染箭頭線，確保箭頭在最上層）
                // 第一階段：渲染 Component, Rectangle, Line, Text
                foreach (var element in parsed.Elements.Where(e => e.Type != MapElementType.DrawLine))
                {
                    RenderElement(canvas, element, scale, offsetX, offsetY, bounds);
                }

                // 第二階段：渲染 DrawLine（箭頭），確保在最上層
                foreach (var element in parsed.Elements.Where(e => e.Type == MapElementType.DrawLine))
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
            // 元件類型: 1=普通元件(有框), 2=裁切孔位標記(圓圈), 3=尺寸標註(無框)
            var isTextOnly = element.ComponentType == 3;
            var isCircleMarker = element.ComponentType == 2;

            // 解析顏色：Colors[0]=邊框, Colors[2]=填充
            var strokeColor = element.Colors.Count > 0 ? element.Colors[0] : "clBlue";

            var rect = new SKRect(x, y, x + w, y + h);

            // 只有非文字元件才繪製框
            if (!isTextOnly)
            {
                // 疊構圖的 ComponentType=2 是矩形層，不是圓圈
                var isStackupLayer = isCircleMarker && _currentMapType == MapType.Stackup;

                if (isCircleMarker && !isStackupLayer)
                {
                    // ComponentType=2 (非疊構圖): 繪製圓圈標記（用於 PF 裁切孔位）
                    var centerX = x + w / 2;
                    var centerY = y + h / 2;
                    var radius = Math.Min(w, h) / 2;  // 圓圈半徑為矩形較小邊的一半

                    using var strokePaint = new SKPaint
                    {
                        Color = SKColors.Black,
                        Style = SKPaintStyle.Stroke,
                        StrokeWidth = 1,
                        IsAntialias = true
                    };
                    canvas.DrawCircle(centerX, centerY, radius, strokePaint);

                    // 檢查是否需要畫斜線（根據填充色判斷）
                    // clTeal 或其他特殊顏色表示需要斜線標記
                    var fillColor = element.Colors.Count > 2 ? element.Colors[2] : "";
                    if (fillColor.ToLower() == "clteal" || fillColor.ToLower() == "clgray")
                    {
                        // 繪製多條斜線填充（從右上到左下）
                        var spacing = 8f;  // 斜線間距
                        var lineCount = (int)(radius * 2 / spacing);  // 計算需要幾條線

                        for (int i = -lineCount; i <= lineCount; i++)
                        {
                            var offsetX = i * spacing;

                            // 計算斜線的起點和終點（從右上到左下）
                            var startX = centerX + offsetX + radius;
                            var startY = centerY - radius;
                            var endX = centerX + offsetX - radius;
                            var endY = centerY + radius;

                            // 裁剪線條使其只在圓圈內顯示
                            canvas.Save();
                            var clipPath = new SKPath();
                            clipPath.AddCircle(centerX, centerY, radius);
                            canvas.ClipPath(clipPath);

                            canvas.DrawLine(startX, startY, endX, endY, strokePaint);

                            canvas.Restore();
                            clipPath.Dispose();
                        }
                    }
                }
                else
                {
                    // ComponentType=1 或疊構圖的 ComponentType=2: 繪製矩形

                    // 只有非疊構圖才繪製白色填充（疊構圖只要斜線，不要實心背景）
                    if (_currentMapType != MapType.Stackup)
                    {
                        using var fillPaint = new SKPaint
                        {
                            Color = SKColors.White,
                            Style = SKPaintStyle.Fill,
                            IsAntialias = true
                        };
                        canvas.DrawRect(rect, fillPaint);
                    }

                    // 根據填充顏色繪製斜線或實心填充（疊構圖特有）
                    var fillColor = element.Colors.Count > 2 ? element.Colors[2] : "";
                    if (_currentMapType == MapType.Stackup && !string.IsNullOrEmpty(fillColor) && fillColor.ToLower() != "clwhite")
                    {
                        var fillColorLower = fillColor.ToLower();

                        // clAqua (青色) 或 clTeal (青綠色) = 半成品，使用青色 #008080 實心填滿
                        if (fillColorLower == "clteal")
                        {
                            using var solidFillPaint = new SKPaint
                            {
                                Color = SKColor.Parse("#008080"),  // 青色 #008080
                                Style = SKPaintStyle.Fill,
                                IsAntialias = true
                            };
                            canvas.DrawRect(rect, solidFillPaint);
                        }
                        else if (fillColorLower == "claqua" )
                        {
                            using var solidFillPaint = new SKPaint
                            {
                                Color = SKColor.Parse("#000000"),
                                Style = SKPaintStyle.Fill,
                                IsAntialias = true
                            };
                            canvas.DrawRect(rect, solidFillPaint);
                        }
                        // clRed (紅色) = 基板，使用交叉斜線（XX樣式）
                        else if (fillColorLower == "clred")
                        {
                            var hatchColor = ParseColor(fillColor);

                            using var hatchPaint = new SKPaint
                            {
                                Color = hatchColor,
                                Style = SKPaintStyle.Stroke,
                                StrokeWidth = 1,
                                IsAntialias = true
                            };

                            var spacing = 8f;

                            canvas.Save();
                            canvas.ClipRect(rect);

                            // 第一組斜線：左上到右下 (\\\)
                            for (float offsetX = 0; offsetX <= w; offsetX += spacing)
                            {
                                var startX = x + offsetX;
                                var startY = y;
                                canvas.DrawLine(startX, startY, startX + h, startY + h, hatchPaint);
                            }

                            // 第二組斜線：右上到左下 (///)，形成交叉 (XXX)
                            for (float offsetX = 0; offsetX <= w; offsetX += spacing)
                            {
                                var startX = x + offsetX;
                                var startY = y;
                                canvas.DrawLine(startX, startY, startX - h, startY + h, hatchPaint);
                            }

                            canvas.Restore();
                        }
                        else
                        {
                            // 其他顏色（clGreen, clBlue等）：單向斜線填充
                            var hatchColor = ParseColor(fillColor);

                            using var hatchPaint = new SKPaint
                            {
                                Color = hatchColor,
                                Style = SKPaintStyle.Stroke,
                                StrokeWidth = 1,
                                IsAntialias = true
                            };

                            // 繪製斜線填充（從左上到右下）
                            var spacing = 10f;

                            // 從左到右繪製斜線
                            for (float offsetX = 0; offsetX <= w; offsetX += spacing)
                            {
                                var startX = x + offsetX;
                                var startY = y;

                                canvas.Save();
                                canvas.ClipRect(rect);
                                canvas.DrawLine(startX, startY, startX + h, startY + h, hatchPaint);
                                canvas.Restore();
                            }
                        }
                    }

                    // 繪製邊框 - 全部用黑色，統一粗度
                    using var strokePaint = new SKPaint
                    {
                        Color = SKColors.Black,
                        Style = SKPaintStyle.Stroke,
                        StrokeWidth = 1,  // 統一粗度為 1
                        IsAntialias = true
                    };
                    canvas.DrawRect(rect, strokePaint);
                }
            }

            // 繪製文字
            // 疊構圖特殊處理：ComponentType=1 的矩形不顯示文字（文字由 ComponentType=3 單獨顯示）
            var isStackupRect = (_currentMapType == MapType.Stackup && element.ComponentType == 1);

            if (!string.IsNullOrEmpty(element.Text) && !isStackupRect)
            {
                // 疊構圖使用較小的字體，其他圖形使用較大的字體
                var fontSize = _currentMapType == MapType.Stackup
                    ? Math.Max(16, Math.Min(element.FontSize * 2, 24))  // 疊構圖: 16-24 (放大)
                    : Math.Max(20, Math.Min(element.FontSize, 40)); // 其他圖形: 20-40
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

                    // 為尺寸標註文字繪製白色背景（避免被線條遮住）
                    var textHeight = textPaint.TextSize;
                    var bgRect = new SKRect(
                        textX - 2,
                        textY - textHeight + 2,
                        textX + textWidth + 2,
                        textY + 4
                    );

                    using var bgPaint = new SKPaint
                    {
                        Color = SKColors.White,
                        Style = SKPaintStyle.Fill,
                        IsAntialias = true
                    };
                    canvas.DrawRect(bgRect, bgPaint);
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
            using var paint = new SKPaint
            {
                Color = ParseColor(element.Colors.FirstOrDefault() ?? "clBlack"),
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1,
                IsAntialias = true
            };

            // 判斷是圓圈標記還是箭頭線
            // 如果寬度和高度都很小（< 15），表示是裁切孔位標記（畫圓圈）
            const float circleThreshold = 15;
            var isCircleMarker = w < circleThreshold && h < circleThreshold;

            if (isCircleMarker)
            {
                // 繪製圓圈標記（用於 PF 裁切孔位）
                var centerX = x + w / 2;
                var centerY = y + h / 2;
                var radius = 8f;  // 圓圈半徑

                // 空心圓圈
                canvas.DrawCircle(centerX, centerY, radius, paint);
            }
            else
            {
                // 繪製線條（用於尺寸標註）
                // PF (PP裁切) 圖只畫直線，其他圖畫箭頭
                var isPfMap = _currentMapType == MapType.PF;

                if (h < w)
                {
                    // 水平線
                    var centerY = y + h / 2;

                    // PF 圖的長水平線需要往下調整，對齊圓圈邊緣
                    if (isPfMap && w > 200)  // 長水平線
                    {
                        // 往下調整 Y 座標，讓線條貼合圓圈頂部邊緣
                        var adjustedY = centerY + 3;  // 往下調整約 10 像素
                        canvas.DrawLine(x, adjustedY, x + w, adjustedY, paint);
                    }
                    else
                    {
                        canvas.DrawLine(x, centerY, x + w, centerY, paint);

                        if (!isPfMap)
                        {
                            // 繪製左右箭頭
                            DrawArrowHead(canvas, paint, x, centerY, -1, 0);
                            DrawArrowHead(canvas, paint, x + w, centerY, 1, 0);
                        }
                    }
                }
                else
                {
                    // 垂直線
                    var centerX = x + w / 2;
                    canvas.DrawLine(centerX, y, centerX, y + h, paint);

                    if (!isPfMap)
                    {
                        // 繪製上下箭頭
                        DrawArrowHead(canvas, paint, centerX, y, 0, -1);
                        DrawArrowHead(canvas, paint, centerX, y + h, 0, 1);
                    }
                }
            }
        }

        private void DrawArrowHead(SKCanvas canvas, SKPaint paint, float x, float y, int dirX, int dirY)
        {
            // 箭頭大小
            const float arrowSize = 6;

            using var fillPaint = new SKPaint
            {
                Color = paint.Color,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };

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
