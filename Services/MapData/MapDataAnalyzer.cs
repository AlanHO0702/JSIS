using System.Text;
using System.Text.RegularExpressions;

namespace PcbErpApi.Services.MapData
{
    /// <summary>
    /// MapData 分析工具 - 用於研究和除錯 MapData 格式
    /// </summary>
    public class MapDataAnalyzer
    {
        private readonly ILogger<MapDataAnalyzer> _logger;

        public MapDataAnalyzer(ILogger<MapDataAnalyzer> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 分析 MapData 並產生報告
        /// </summary>
        public MapDataAnalysisReport Analyze(string mapData)
        {
            var report = new MapDataAnalysisReport
            {
                Length = mapData?.Length ?? 0,
                Preview = mapData?.Substring(0, Math.Min(500, mapData?.Length ?? 0)) ?? string.Empty
            };

            if (string.IsNullOrEmpty(mapData))
            {
                report.Notes.Add("MapData is null or empty");
                return report;
            }

            // 檢測格式
            report.DetectedFormat = DetectFormat(mapData);

            // 分析內容
            AnalyzeContent(mapData, report);

            // 尋找模式
            FindPatterns(mapData, report);

            return report;
        }

        private string DetectFormat(string data)
        {
            if (data.StartsWith("object"))
                return "Delphi DFM";
            if (data.Contains("TXFlowDrawBox"))
                return "Delphi DFM (XFlowDrawBox)";
            if (data.StartsWith("<"))
                return "XML";
            if (data.StartsWith("{") || data.StartsWith("["))
                return "JSON";
            return "Unknown/Custom";
        }

        private void AnalyzeContent(string data, MapDataAnalysisReport report)
        {
            // 行數
            report.LineCount = data.Split('\n').Length;

            // 數字
            var numbers = Regex.Matches(data, @"-?\d+\.?\d*");
            report.NumberCount = numbers.Count;
            report.SampleNumbers = numbers.Take(20).Select(m => m.Value).ToList();

            // 分隔符號
            var separators = new Dictionary<string, int>
            {
                ["|"] = data.Split('|').Length - 1,
                [","] = data.Split(',').Length - 1,
                [";"] = data.Split(';').Length - 1,
                [":"] = data.Split(':').Length - 1,
                ["\\t"] = data.Split('\t').Length - 1,
                ["\\n"] = data.Split('\n').Length - 1
            };

            foreach (var sep in separators.Where(s => s.Value > 0))
            {
                report.Separators[sep.Key] = sep.Value;
            }

            // 關鍵字
            var keywords = new[] { "object", "end", "Width", "Height", "Left", "Top",
                                   "Line", "Rectangle", "Text", "Color" };
            foreach (var keyword in keywords)
            {
                int count = Regex.Matches(data, keyword, RegexOptions.IgnoreCase).Count;
                if (count > 0)
                {
                    report.Keywords[keyword] = count;
                }
            }
        }

        private void FindPatterns(string data, MapDataAnalysisReport report)
        {
            // 尋找座標模式 (x,y)
            var coordPattern = @"\(?\s*(-?\d+\.?\d*)\s*,\s*(-?\d+\.?\d*)\s*\)?";
            var coords = Regex.Matches(data, coordPattern);
            if (coords.Count > 0)
            {
                report.Notes.Add($"Found {coords.Count} coordinate pairs");
                report.SamplePatterns.Add($"Coordinates: {coords[0].Value}");
            }

            // 尋找尺寸模式 (width x height)
            var sizePattern = @"(\d+\.?\d*)\s*[xX×]\s*(\d+\.?\d*)";
            var sizes = Regex.Matches(data, sizePattern);
            if (sizes.Count > 0)
            {
                report.Notes.Add($"Found {sizes.Count} size patterns");
                report.SamplePatterns.Add($"Size: {sizes[0].Value}");
            }

            // 尋找屬性模式 (key = value)
            var propPattern = @"(\w+)\s*=\s*([^;\n]+)";
            var props = Regex.Matches(data, propPattern);
            if (props.Count > 0)
            {
                report.Notes.Add($"Found {props.Count} properties");
                foreach (Match prop in props.Take(5))
                {
                    report.SamplePatterns.Add($"Property: {prop.Value}");
                }
            }
        }

        /// <summary>
        /// 產生文字報告
        /// </summary>
        public string GenerateTextReport(MapDataAnalysisReport report)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== MapData 分析報告 ===");
            sb.AppendLine($"長度: {report.Length}");
            sb.AppendLine($"行數: {report.LineCount}");
            sb.AppendLine($"偵測格式: {report.DetectedFormat}");
            sb.AppendLine();

            if (report.Separators.Any())
            {
                sb.AppendLine("分隔符號:");
                foreach (var sep in report.Separators)
                {
                    sb.AppendLine($"  '{sep.Key}': {sep.Value} 次");
                }
                sb.AppendLine();
            }

            if (report.Keywords.Any())
            {
                sb.AppendLine("關鍵字:");
                foreach (var kw in report.Keywords.OrderByDescending(k => k.Value))
                {
                    sb.AppendLine($"  '{kw.Key}': {kw.Value} 次");
                }
                sb.AppendLine();
            }

            if (report.SampleNumbers.Any())
            {
                sb.AppendLine("數字樣本 (前 20 個):");
                sb.AppendLine($"  {string.Join(", ", report.SampleNumbers)}");
                sb.AppendLine();
            }

            if (report.SamplePatterns.Any())
            {
                sb.AppendLine("發現的模式:");
                foreach (var pattern in report.SamplePatterns)
                {
                    sb.AppendLine($"  {pattern}");
                }
                sb.AppendLine();
            }

            if (report.Notes.Any())
            {
                sb.AppendLine("備註:");
                foreach (var note in report.Notes)
                {
                    sb.AppendLine($"  - {note}");
                }
                sb.AppendLine();
            }

            sb.AppendLine("前 500 字元預覽:");
            sb.AppendLine(report.Preview);

            return sb.ToString();
        }
    }

    /// <summary>
    /// MapData 分析報告
    /// </summary>
    public class MapDataAnalysisReport
    {
        public int Length { get; set; }
        public int LineCount { get; set; }
        public int NumberCount { get; set; }
        public string DetectedFormat { get; set; } = string.Empty;
        public string Preview { get; set; } = string.Empty;
        public Dictionary<string, int> Separators { get; set; } = new();
        public Dictionary<string, int> Keywords { get; set; } = new();
        public List<string> SampleNumbers { get; set; } = new();
        public List<string> SamplePatterns { get; set; } = new();
        public List<string> Notes { get; set; } = new();
    }
}
