using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Services;
using PcbErpApi.Services.MapData;
using System.Data;
using System.IO;

namespace PcbErpApi.Controllers
{
    /// <summary>
    /// 圖形預覽 API
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MapPreviewController : ControllerBase
    {
        private readonly MapService _mapService;
        private readonly MapDataAnalyzer _analyzer;
        private readonly ILogger<MapPreviewController> _logger;
        private readonly PcbErpContext _context;

        public MapPreviewController(
            MapService mapService,
            MapDataAnalyzer analyzer,
            ILogger<MapPreviewController> logger,
            PcbErpContext context)
        {
            _mapService = mapService;
            _analyzer = analyzer;
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// 預覽圖形
        /// GET /api/mappreview/{partNum}/{revision}/{type}
        /// </summary>
        [HttpGet("{partNum}/{revision}/{type}")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> GetPreview(
            string partNum,
            string revision,
            string type,
            [FromQuery] int width = 600,
            [FromQuery] int height = 500)
        {
            try
            {
                var mapType = ParseMapType(type);
                if (mapType == null)
                {
                    return BadRequest(new { error = $"Invalid type: {type}" });
                }

                var imageBytes = await _mapService.RenderMapAsync(
                    partNum,
                    revision,
                    mapType.Value,
                    width,
                    height
                );

                return File(imageBytes, "image/jpeg");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting preview");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// 取得圖檔
        /// GET /api/mappreview/file?path=...
        /// </summary>
        [HttpGet("file")]
        public IActionResult GetFile([FromQuery] string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path))
                {
                    return NotFound(new { error = "File not found" });
                }

                var bytes = System.IO.File.ReadAllBytes(path);
                var ext = Path.GetExtension(path).ToLowerInvariant();
                var contentType = ext switch
                {
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".bmp" => "image/bmp",
                    ".wmf" => "image/wmf",
                    ".pdf" => "application/pdf",
                    _ => "application/octet-stream"
                };
                return File(bytes, contentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting file");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// 分析 MapData
        /// GET /api/mappreview/analyze/{partNum}/{revision}/{type}
        /// </summary>
        [HttpGet("analyze/{partNum}/{revision}/{type}")]
        public async Task<IActionResult> AnalyzeMapData(
            string partNum,
            string revision,
            string type)
        {
            try
            {
                var mapType = ParseMapType(type);
                if (mapType == null)
                {
                    return BadRequest(new { error = $"Invalid type: {type}" });
                }

                string? mapData;
                if (mapType == MapType.Stackup)
                {
                    mapData = await _mapService.GetStackupMapDataAsync(partNum, revision);
                }
                else
                {
                    mapData = await _mapService.GetMapDataAsync(partNum, revision, (byte)mapType.Value);
                }

                if (string.IsNullOrEmpty(mapData))
                {
                    return NotFound(new { error = "MapData not found" });
                }

                var report = _analyzer.Analyze(mapData);
                var textReport = _analyzer.GenerateTextReport(report);

                return Ok(new
                {
                    report,
                    textReport
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing MapData");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// 診斷端點 - 列出資料庫中的資料
        /// GET /api/mappreview/debug/list
        /// </summary>
        [HttpGet("debug/list")]
        public async Task<IActionResult> DebugListData([FromQuery] int limit = 20)
        {
            try
            {
                var maps = await _context.EmodProdMaps
                    .Select(m => new
                    {
                        m.PartNum,
                        m.Revision,
                        m.MapKindNo,
                        m.SerialNum,
                        m.MapData
                    })
                    .Take(limit)
                    .ToListAsync();

                var result = maps.Select(m => new
                {
                    m.PartNum,
                    m.Revision,
                    m.MapKindNo,
                    m.SerialNum,
                    HasMapData = m.MapData != null,
                    MapDataLength = m.MapData?.Length ?? 0
                }).ToList();

                return Ok(new
                {
                    count = result.Count,
                    records = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing MapData");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// 診斷端點 - 檢查特定料號
        /// GET /api/mappreview/debug/check/{partNum}/{revision}
        /// </summary>
        [HttpGet("debug/check/{partNum}/{revision}")]
        public async Task<IActionResult> DebugCheckPartNum(string partNum, string revision)
        {
            try
            {
                var maps = await _context.EmodProdMaps
                    .Where(m => m.PartNum == partNum && m.Revision == revision)
                    .Select(m => new
                    {
                        m.PartNum,
                        m.Revision,
                        m.MapKindNo,
                        m.SerialNum,
                        m.MapData
                    })
                    .ToListAsync();

                var result = maps.Select(m => new
                {
                    m.PartNum,
                    m.Revision,
                    m.MapKindNo,
                    m.SerialNum,
                    HasMapData = m.MapData != null,
                    MapDataLength = m.MapData?.Length ?? 0,
                    MapDataPreview = m.MapData != null && m.MapData.Length > 0
                        ? m.MapData.Substring(0, Math.Min(200, m.MapData.Length))
                        : null,
                    FullMapData = m.MapData
                }).ToList();

                return Ok(new
                {
                    partNum,
                    revision,
                    found = result.Count > 0,
                    count = result.Count,
                    records = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking PartNum");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// 診斷端點 - 查詢有 Stackup 資料的料號
        /// GET /api/mappreview/debug/find-stackup
        /// </summary>
        [HttpGet("debug/find-stackup")]
        public async Task<IActionResult> DebugFindStackup([FromQuery] int limit = 20)
        {
            try
            {
                // 使用跟 MapService 一樣的方式查詢
                var records = new List<object>();

                var allStackups = await _context.EmodLayerPresses
                    .Where(m => m.LayerId == "L0~0")
                    .Take(100)
                    .ToListAsync();

                _logger.LogInformation($"Found {allStackups.Count} stackup records");

                foreach (var stackup in allStackups)
                {
                    // 使用 MapService 的邏輯
                    var mapData = (stackup.MapData_1 ?? "") +
                                 (stackup.MapData_2 ?? "") +
                                 (stackup.MapData_3 ?? "");

                    if (!string.IsNullOrEmpty(mapData))
                    {
                        records.Add(new
                        {
                            stackup.PartNum,
                            stackup.Revision,
                            MapDataLength = mapData.Length,
                            MapDataPreview = mapData.Substring(0, Math.Min(150, mapData.Length))
                        });

                        if (records.Count >= limit)
                            break;
                    }
                }

                return Ok(new
                {
                    found = records.Count,
                    totalScanned = allStackups.Count,
                    records
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding Stackup data: {Message}", ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// 產生裁版圖 MapData（呼叫 EMOdGenMapXFlow 寫入 EMOdProdMap）
        /// POST /api/mappreview/generateMapData
        /// </summary>
        [HttpPost("generateMapData")]
        public async Task<IActionResult> GenerateMapData([FromBody] GenerateMapDataRequest req)
        {
            if (string.IsNullOrWhiteSpace(req?.PartNum))
                return BadRequest(new { ok = false, error = "PartNum 為必填" });

            var connStr = _context.Database.GetConnectionString();
            await using var conn = new SqlConnection(connStr);
            await conn.OpenAsync();

            // 先清空舊資料
            await using (var delCmd = new SqlCommand(
                "DELETE FROM EMOdProdMap WHERE PartNum=@PartNum AND Revision=@Revision", conn))
            {
                delCmd.Parameters.AddWithValue("@PartNum", req.PartNum.Trim());
                delCmd.Parameters.AddWithValue("@Revision", (req.Revision ?? "").Trim());
                await delCmd.ExecuteNonQueryAsync();
            }

            // 產生 MapKind = 1(裁板圖), 3(排板圖), 9(PP裁切圖)
            foreach (var mapKind in new[] { 1, 3, 9 })
            {
                try
                {
                    string mapDataStr = "";
                    using (var mapCmd = new SqlCommand("EMOdGenMapXFlow", conn))
                    {
                        mapCmd.CommandType = CommandType.StoredProcedure;
                        mapCmd.CommandTimeout = 60;
                        mapCmd.Parameters.AddWithValue("@PartNum", req.PartNum.Trim());
                        mapCmd.Parameters.AddWithValue("@Revision", (req.Revision ?? "").Trim());
                        mapCmd.Parameters.AddWithValue("@MapKind", mapKind);
                        using var reader = await mapCmd.ExecuteReaderAsync(CommandBehavior.SingleResult);
                        if (await reader.ReadAsync())
                        {
                            mapDataStr =
                                (reader["MapData"]  as string ?? "") +
                                (reader["StrMap"]   as string ?? "") +
                                (reader["StrMap2"]  as string ?? "") +
                                (reader["MapData2"] as string ?? "");
                        }
                    }

                    if (!string.IsNullOrEmpty(mapDataStr))
                    {
                        using var updCmd = new SqlCommand(
                            "UPDATE EMOdProdMap SET MapData=@MapData WHERE PartNum=@PartNum AND Revision=@Revision AND MapKindNo=@MapKindNo", conn);
                        updCmd.Parameters.AddWithValue("@MapData",   mapDataStr);
                        updCmd.Parameters.AddWithValue("@PartNum",   req.PartNum.Trim());
                        updCmd.Parameters.AddWithValue("@Revision",  (req.Revision ?? "").Trim());
                        updCmd.Parameters.AddWithValue("@MapKindNo", mapKind);
                        var affected = await updCmd.ExecuteNonQueryAsync();

                        // 若 UPDATE 影響 0 列，表示 SP 沒有先 INSERT，手動 INSERT
                        if (affected == 0)
                        {
                            using var insCmd = new SqlCommand(
                                "INSERT INTO EMOdProdMap (PartNum, Revision, MapKindNo, SerialNum, MapData) VALUES (@PartNum, @Revision, @MapKindNo, 1, @MapData)", conn);
                            insCmd.Parameters.AddWithValue("@PartNum",   req.PartNum.Trim());
                            insCmd.Parameters.AddWithValue("@Revision",  (req.Revision ?? "").Trim());
                            insCmd.Parameters.AddWithValue("@MapKindNo", mapKind);
                            insCmd.Parameters.AddWithValue("@MapData",   mapDataStr);
                            await insCmd.ExecuteNonQueryAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "GenerateMapData MapKind={MapKind} failed for {PartNum}", mapKind, req.PartNum);
                }
            }

            return Ok(new { ok = true });
        }

        public class GenerateMapDataRequest
        {
            public string PartNum  { get; set; } = "";
            public string? Revision { get; set; }
        }

        private MapType? ParseMapType(string type)
        {
            return type.ToLower() switch
            {
                "panel" or "1" => MapType.Panel,
                "cut" or "3" => MapType.Cut,
                "pf" or "9" => MapType.PF,
                "stackup" or "0" => MapType.Stackup,
                _ => null
            };
        }
    }
}
