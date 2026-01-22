using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;
using PcbErpApi.Services.Rendering;

namespace PcbErpApi.Services
{
    /// <summary>
    /// 圖形服務 - 處理圖形資料的取得和渲染
    /// </summary>
    public class MapService
    {
        private readonly PcbErpContext _context;
        private readonly SimpleMapRenderer _simpleRenderer;
        private readonly PanelMapRenderer _panelRenderer;
        private readonly UniversalMapRenderer _universalRenderer;
        private readonly ILogger<MapService> _logger;

        public MapService(
            PcbErpContext context,
            SimpleMapRenderer simpleRenderer,
            PanelMapRenderer panelRenderer,
            UniversalMapRenderer universalRenderer,
            ILogger<MapService> logger)
        {
            _context = context;
            _simpleRenderer = simpleRenderer;
            _panelRenderer = panelRenderer;
            _universalRenderer = universalRenderer;
            _logger = logger;
        }

        /// <summary>
        /// 取得圖形資料
        /// </summary>
        public async Task<string?> GetMapDataAsync(string partNum, string revision, byte mapKind)
        {
            try
            {
                _logger.LogInformation("Querying MapData: PartNum={PartNum}, Revision={Revision}, MapKind={MapKind}",
                    partNum, revision, mapKind);

                // 先檢查資料是否存在
                var exists = await _context.EmodProdMaps
                    .AnyAsync(m => m.PartNum == partNum && m.Revision == revision);

                _logger.LogInformation("Record exists for PartNum/Revision: {Exists}", exists);

                if (!exists)
                {
                    // 列出所有可能的料號和版序組合（前10筆）
                    var available = await _context.EmodProdMaps
                        .Select(m => new { m.PartNum, m.Revision, m.MapKindNo })
                        .Take(10)
                        .ToListAsync();

                    _logger.LogWarning("No data found. Available records: {Available}",
                        string.Join(", ", available.Select(a => $"{a.PartNum}/{a.Revision}/{a.MapKindNo}")));
                }

                var map = await _context.EmodProdMaps
                    .Where(m => m.PartNum == partNum
                             && m.Revision == revision
                             && m.MapKindNo == mapKind)
                    .Select(m => m.MapData)
                    .FirstOrDefaultAsync();

                _logger.LogInformation("MapData retrieved: {HasData}, Length={Length}",
                    map != null, map?.Length ?? 0);

                return map;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting MapData for {PartNum}/{Revision}/{MapKind}",
                    partNum, revision, mapKind);
                return null;
            }
        }

        /// <summary>
        /// 取得疊構圖資料（透過 SP EMOdLayerPressMap）
        /// </summary>
        public async Task<string?> GetStackupMapDataAsync(string partNum, string revision)
        {
            try
            {
                // 使用 ADO.NET 直接呼叫 SP
                using var connection = _context.Database.GetDbConnection();
                await connection.OpenAsync();

                using var command = connection.CreateCommand();
                command.CommandText = "EMOdLayerPressMap";
                command.CommandType = System.Data.CommandType.StoredProcedure;

                // 加入參數
                var paramPartNum = command.CreateParameter();
                paramPartNum.ParameterName = "@PartNum";
                paramPartNum.Value = partNum;
                command.Parameters.Add(paramPartNum);

                var paramRevision = command.CreateParameter();
                paramRevision.ParameterName = "@Revision";
                paramRevision.Value = revision;
                command.Parameters.Add(paramRevision);

                var paramAftLayerId = command.CreateParameter();
                paramAftLayerId.ParameterName = "@AftLayerId";
                paramAftLayerId.Value = "L0~0";
                command.Parameters.Add(paramAftLayerId);

                _logger.LogInformation("Calling SP EMOdLayerPressMap with PartNum={PartNum}, Revision={Revision}",
                    partNum, revision);

                string? mapData1 = null;
                string? mapData2 = null;
                string? mapData3 = null;

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    // 讀取三個 MapData 欄位
                    mapData1 = reader.IsDBNull(0) ? null : reader.GetString(0);
                    mapData2 = reader.IsDBNull(1) ? null : reader.GetString(1);
                    mapData3 = reader.IsDBNull(2) ? null : reader.GetString(2);

                    _logger.LogInformation("Stackup MapData retrieved: MapData_1={Len1}, MapData_2={Len2}, MapData_3={Len3}",
                        mapData1?.Length ?? 0, mapData2?.Length ?? 0, mapData3?.Length ?? 0);

                    // Debug: 輸出 MapData 內容的前面部分
                    if (!string.IsNullOrEmpty(mapData1))
                    {
                        _logger.LogInformation("MapData_1 preview (first 500 chars): {Preview}",
                            mapData1.Substring(0, Math.Min(500, mapData1.Length)));
                    }
                    if (!string.IsNullOrEmpty(mapData2))
                    {
                        _logger.LogInformation("MapData_2 preview (first 500 chars): {Preview}",
                            mapData2.Substring(0, Math.Min(500, mapData2.Length)));
                    }
                }
                else
                {
                    _logger.LogWarning("No stackup data found for {PartNum}/{Revision}", partNum, revision);
                    return null;
                }

                return (mapData1 ?? "") + (mapData2 ?? "") + (mapData3 ?? "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling EMOdLayerPressMap for {PartNum}/{Revision}",
                    partNum, revision);
                return null;
            }
        }

        /// <summary>
        /// 渲染圖形為圖片
        /// </summary>
        public async Task<byte[]> RenderMapAsync(
            string partNum,
            string revision,
            MapType mapType,
            int width = 800,
            int height = 600)
        {
            try
            {
                string? mapData;

                if (mapType == MapType.Stackup)
                {
                    mapData = await GetStackupMapDataAsync(partNum, revision);
                }
                else
                {
                    mapData = await GetMapDataAsync(partNum, revision, (byte)mapType);
                }

                if (string.IsNullOrEmpty(mapData))
                {
                    _logger.LogWarning("No MapData found for {PartNum}/{Revision}/{Type}",
                        partNum, revision, mapType);
                    return _simpleRenderer.RenderPlaceholder(
                        $"無圖形資料\n{partNum}/{revision}\n{GetMapTypeName(mapType)}",
                        width,
                        height
                    );
                }

                // 使用通用渲染器渲染 MapData
                _logger.LogInformation("Rendering {Type} map for {PartNum}/{Revision} using UniversalMapRenderer",
                    mapType, partNum, revision);
                return _universalRenderer.RenderFromMapData(mapData, width, height, mapType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rendering map");
                return _simpleRenderer.RenderPlaceholder($"渲染錯誤: {ex.Message}", width, height);
            }
        }

        /// <summary>
        /// 產生並儲存圖檔
        /// </summary>
        public async Task<string> GenerateAndSaveImageAsync(
            string partNum,
            string revision,
            MapType mapType)
        {
            // 取得儲存路徑
            var basePath = await GetImagePathAsync();
            var fileName = $"{partNum}{revision}_{GetMapTypeFileName(mapType)}.jpg";
            var fullPath = Path.Combine(basePath, fileName);

            // 如果檔案已存在，直接返回
            if (File.Exists(fullPath))
            {
                _logger.LogInformation("Image already exists: {Path}", fullPath);
                return fullPath;
            }

            // 渲染圖片（報表用適中尺寸）
            int width = mapType == MapType.Stackup ? 500 : 600;
            int height = mapType == MapType.Stackup ? 500 : 600;

            var imageBytes = await RenderMapAsync(partNum, revision, mapType, width, height);

            // 儲存檔案
            await File.WriteAllBytesAsync(fullPath, imageBytes);
            _logger.LogInformation("Image saved: {Path}", fullPath);

            return fullPath;
        }

        private async Task<string> GetImagePathAsync()
        {
            var path = await _context.CURdSysParams
                .Where(p => p.SystemId == "EMO" && p.ParamId == "MapDataPath")
                .Select(p => p.Value)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(path))
            {
                // 如果沒有設定，使用預設路徑
                path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "maps");
                _logger.LogWarning("MapDataPath not configured, using default: {Path}", path);
            }

            // 確保目錄存在
            Directory.CreateDirectory(path);

            return path;
        }

        private string GetMapTypeName(MapType mapType) => mapType switch
        {
            MapType.Panel => "裁板圖",
            MapType.Cut => "排板圖",
            MapType.PF => "PF裁剪圖",
            MapType.Stackup => "疊構圖",
            _ => "未知"
        };

        private string GetMapTypeFileName(MapType mapType) => mapType switch
        {
            MapType.Panel => "panel",
            MapType.Cut => "cut",
            MapType.PF => "pf",
            MapType.Stackup => "stackup",
            _ => "unknown"
        };
    }

    /// <summary>
    /// 圖形類型
    /// </summary>
    public enum MapType
    {
        Panel = 1,      // 裁板圖
        Cut = 3,        // 排板圖
        PF = 9,         // PF裁剪圖
        Stackup = 0     // 疊構圖
    }
}
