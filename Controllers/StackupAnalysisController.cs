using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using System.Data;

namespace PcbErpApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StackupAnalysisController : ControllerBase
{
    private readonly PcbErpContext _context;
    private readonly ILogger<StackupAnalysisController> _logger;

    public StackupAnalysisController(PcbErpContext context, ILogger<StackupAnalysisController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 分析料號的疊構圖 MatClass 分布
    /// </summary>
    [HttpGet("{partNum}/{revision}")]
    public async Task<IActionResult> AnalyzeStackup(string partNum, string revision)
    {
        try
        {
            // 呼叫 SP 取得 MapData
            using var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "EMOdLayerPressMap";
            command.CommandType = CommandType.StoredProcedure;

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

            string? mapData1 = null;
            string? mapData2 = null;
            string? mapData3 = null;

            using (var reader = await command.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    mapData1 = reader.IsDBNull(0) ? null : reader.GetString(0);
                    mapData2 = reader.IsDBNull(1) ? null : reader.GetString(1);
                    mapData3 = reader.IsDBNull(2) ? null : reader.GetString(2);
                }
            } // Reader 關閉

            // 解析 MapData 找出所有顏色
            var colors = new Dictionary<string, int>();
            var mapDataFull = (mapData1 ?? "") + (mapData2 ?? "") + (mapData3 ?? "");

            // 分割成元件，並分析每個元件的詳細資訊
            var components = mapDataFull.Split(new[] { "C^&" }, StringSplitOptions.RemoveEmptyEntries);
            var componentDetails = new List<object>();

            foreach (var comp in components)
            {
                var parts = comp.Split(new[] { "^&" }, StringSplitOptions.None);

                // Colors 在 index 6-9 的位置
                if (parts.Length > 8)
                {
                    // Colors[2] 是填充顏色 (index 8)
                    var fillColor = parts[8];
                    if (!string.IsNullOrEmpty(fillColor))
                    {
                        if (colors.ContainsKey(fillColor))
                            colors[fillColor]++;
                        else
                            colors[fillColor] = 1;
                    }

                    // 提取元件資訊：ComponentType, 文字, 顏色
                    var componentType = parts.Length > 5 ? parts[5] : "";
                    var text = parts.Length > 16 ? parts[parts.Length - 4] : "";

                    componentDetails.Add(new
                    {
                        Type = componentType,
                        FillColor = fillColor,
                        Text = text,
                        Y = parts.Length > 2 ? parts[2] : ""
                    });
                }
            }

            // 查詢這個料號的 MatClass 分布
            var matClassQuery = @"
                SELECT
                    MatClass,
                    COUNT(*) as LayerCount
                FROM MINdMatInfo WITH (NOLOCK)
                WHERE PartNum = @PartNum
                  AND Revision = @Revision
                GROUP BY MatClass
                ORDER BY MatClass
            ";

            using var matClassCmd = new SqlCommand(matClassQuery, (SqlConnection)connection);
            matClassCmd.Parameters.AddWithValue("@PartNum", partNum);
            matClassCmd.Parameters.AddWithValue("@Revision", revision);

            var matClasses = new List<object>();
            using var matClassReader = await matClassCmd.ExecuteReaderAsync();
            while (await matClassReader.ReadAsync())
            {
                matClasses.Add(new
                {
                    MatClass = matClassReader.IsDBNull(0) ? null : matClassReader.GetString(0),
                    LayerCount = matClassReader.GetInt32(1)
                });
            }

            return Ok(new
            {
                PartNum = partNum,
                Revision = revision,
                MapDataColors = colors.Select(c => new { Color = c.Key, Count = c.Value }),
                ComponentDetails = componentDetails,
                MatClasses = matClasses,
                RawMapDataLength = new
                {
                    MapData1 = mapData1?.Length ?? 0,
                    MapData2 = mapData2?.Length ?? 0,
                    MapData3 = mapData3?.Length ?? 0
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing stackup for {PartNum}/{Revision}", partNum, revision);
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
