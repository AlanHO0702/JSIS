using System.Text.RegularExpressions;

namespace PcbErpApi.Services.MapData
{
    /// <summary>
    /// MapData 格式解析器 - 解析 Delphi XFlowDrawBox 格式
    /// </summary>
    public class MapDataParser
    {
        private readonly ILogger<MapDataParser> _logger;

        public MapDataParser(ILogger<MapDataParser> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 解析 MapData 字串
        /// </summary>
        public ParsedMapData Parse(string mapData)
        {
            if (string.IsNullOrEmpty(mapData))
            {
                return new ParsedMapData();
            }

            var result = new ParsedMapData
            {
                RawData = mapData
            };

            try
            {
                // 按行分割
                var lines = mapData.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in lines)
                {
                    ParseLine(line.Trim(), result);
                }

                _logger.LogInformation("Parsed {ElementCount} elements from MapData", result.Elements.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing MapData");
                result.ParseErrors.Add(ex.Message);
            }

            return result;
        }

        private void ParseLine(string line, ParsedMapData result)
        {
            if (string.IsNullOrWhiteSpace(line))
                return;

            // 根據開頭判斷元素類型
            if (line.StartsWith("C^&"))
            {
                // Component (矩形元件)
                ParseComponent(line, result);
            }
            else if (line.StartsWith("DL^&"))
            {
                // DrawLine (線條)
                ParseDrawLine(line, result);
            }
            else if (line.StartsWith("L^&"))
            {
                // Line (簡單線條)
                ParseSimpleLine(line, result);
            }
            else if (line.StartsWith("R^&"))
            {
                // Rectangle (矩形)
                ParseRectangle(line, result);
            }
            else if (line.StartsWith("T^&"))
            {
                // Text (文字)
                ParseText(line, result);
            }
            else
            {
                _logger.LogDebug("Unknown element type: {Line}", line.Substring(0, Math.Min(50, line.Length)));
            }
        }

        private void ParseComponent(string line, ParsedMapData result)
        {
            // 格式: C^&x^&y^&width^&height^&...^&color^&...^&font^&fontSize^&...^&text
            var parts = line.Split(new[] { "^&" }, StringSplitOptions.None);

            if (parts.Length < 5)
            {
                _logger.LogWarning("Invalid Component format: {Line}", line);
                return;
            }

            try
            {
                var element = new MapElement
                {
                    Type = MapElementType.Component,
                    X = ParseFloat(parts[1]),
                    Y = ParseFloat(parts[2]),
                    Width = ParseFloat(parts[3]),
                    Height = ParseFloat(parts[4])
                };

                // 解析顏色 (索引 5-10 之間可能有顏色)
                for (int i = 5; i < Math.Min(parts.Length, 12); i++)
                {
                    if (parts[i].StartsWith("cl"))
                    {
                        element.Colors.Add(parts[i]);
                    }
                }

                // 解析字型 (通常在 "MS Sans Serif" 或 "Tahoma" 等)
                for (int i = 5; i < parts.Length; i++)
                {
                    if (parts[i].Contains("Sans") || parts[i].Contains("Tahoma") || parts[i].Contains("Arial"))
                    {
                        element.FontName = parts[i];
                        // 字型大小通常在字型名稱後面
                        if (i + 1 < parts.Length && int.TryParse(parts[i + 1], out int fontSize))
                        {
                            element.FontSize = fontSize;
                        }
                        break;
                    }
                }

                // 最後一個通常是文字標籤 (C1001, C1002 等)
                if (parts.Length > 0)
                {
                    var lastPart = parts[parts.Length - 1].Trim();
                    if (!string.IsNullOrEmpty(lastPart) && !lastPart.StartsWith("-"))
                    {
                        element.Text = lastPart;
                    }
                }

                result.Elements.Add(element);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error parsing component: {Line}", line);
            }
        }

        private void ParseDrawLine(string line, ParsedMapData result)
        {
            // 格式: DL^&x^&y^&width^&height^&...^&color^&text
            var parts = line.Split(new[] { "^&" }, StringSplitOptions.None);

            if (parts.Length < 5)
            {
                _logger.LogWarning("Invalid DrawLine format: {Line}", line);
                return;
            }

            try
            {
                var element = new MapElement
                {
                    Type = MapElementType.DrawLine,
                    X = ParseFloat(parts[1]),
                    Y = ParseFloat(parts[2]),
                    Width = ParseFloat(parts[3]),
                    Height = ParseFloat(parts[4])
                };

                // 解析顏色
                for (int i = 5; i < parts.Length; i++)
                {
                    if (parts[i].StartsWith("cl"))
                    {
                        element.Colors.Add(parts[i]);
                    }
                }

                // 最後一個可能是文字標籤
                if (parts.Length > 0)
                {
                    var lastPart = parts[parts.Length - 1].Trim();
                    if (!string.IsNullOrEmpty(lastPart) && lastPart.StartsWith("L"))
                    {
                        element.Text = lastPart;
                    }
                }

                result.Elements.Add(element);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error parsing draw line: {Line}", line);
            }
        }

        private void ParseSimpleLine(string line, ParsedMapData result)
        {
            var parts = line.Split(new[] { "^&" }, StringSplitOptions.None);
            if (parts.Length < 5)
                return;

            try
            {
                result.Elements.Add(new MapElement
                {
                    Type = MapElementType.Line,
                    X = ParseFloat(parts[1]),
                    Y = ParseFloat(parts[2]),
                    Width = ParseFloat(parts[3]),
                    Height = ParseFloat(parts[4])
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error parsing line: {Line}", line);
            }
        }

        private void ParseRectangle(string line, ParsedMapData result)
        {
            var parts = line.Split(new[] { "^&" }, StringSplitOptions.None);
            if (parts.Length < 5)
                return;

            try
            {
                result.Elements.Add(new MapElement
                {
                    Type = MapElementType.Rectangle,
                    X = ParseFloat(parts[1]),
                    Y = ParseFloat(parts[2]),
                    Width = ParseFloat(parts[3]),
                    Height = ParseFloat(parts[4])
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error parsing rectangle: {Line}", line);
            }
        }

        private void ParseText(string line, ParsedMapData result)
        {
            var parts = line.Split(new[] { "^&" }, StringSplitOptions.None);
            if (parts.Length < 3)
                return;

            try
            {
                var element = new MapElement
                {
                    Type = MapElementType.Text,
                    X = ParseFloat(parts[1]),
                    Y = ParseFloat(parts[2])
                };

                if (parts.Length > 3)
                {
                    element.Text = parts[3];
                }

                result.Elements.Add(element);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error parsing text: {Line}", line);
            }
        }

        private float ParseFloat(string value)
        {
            if (float.TryParse(value, out float result))
                return result;
            return 0f;
        }
    }

    /// <summary>
    /// 解析後的 MapData
    /// </summary>
    public class ParsedMapData
    {
        public string RawData { get; set; } = string.Empty;
        public List<MapElement> Elements { get; set; } = new();
        public List<string> ParseErrors { get; set; } = new();
    }

    /// <summary>
    /// 圖形元素
    /// </summary>
    public class MapElement
    {
        public MapElementType Type { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public List<string> Colors { get; set; } = new();
        public string? FontName { get; set; }
        public int FontSize { get; set; } = 10;
        public string? Text { get; set; }
    }

    /// <summary>
    /// 圖形元素類型
    /// </summary>
    public enum MapElementType
    {
        Component,      // C^& - 矩形元件
        DrawLine,       // DL^& - 線條
        Line,           // L^& - 簡單線條
        Rectangle,      // R^& - 矩形
        Text            // T^& - 文字
    }
}
