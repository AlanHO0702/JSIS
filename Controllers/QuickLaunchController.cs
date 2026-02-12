using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using PcbErpApi.Data;
using System.Data;

namespace PcbErpApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuickLaunchController : ControllerBase
    {
        private readonly PcbErpContext _context;
        private readonly string _connStr;

        public QuickLaunchController(PcbErpContext context)
        {
            _context = context;
            _connStr = context.Database.GetDbConnection().ConnectionString;
        }

        /// <summary>
        /// 取得可快速啟動的項目清單
        /// </summary>
        /// <param name="userId">使用者ID(未來可用於權限篩選)</param>
        /// <returns>項目清單</returns>
        [HttpGet("Items")]
        public async Task<IActionResult> GetQuickLaunchItems([FromQuery] string? userId = null)
        {
            try
            {
                // 先嘗試從 CURdMenuTemp 查詢(如果表存在)
                try
                {
                    await using var conn = new SqlConnection(_connStr);
                    await conn.OpenAsync();

                    // 檢查表是否存在
                    await using var checkCmd = new SqlCommand(@"
                        IF OBJECT_ID('CURdMenuTemp', 'U') IS NOT NULL
                            SELECT 1
                        ELSE
                            SELECT 0", conn);

                    var tableExists = Convert.ToInt32(await checkCmd.ExecuteScalarAsync()) == 1;

                    if (tableExists)
                    {
                        // 從 CURdMenuTemp 查詢
                        await using var cmd = new SqlCommand(@"
                            SELECT DISTINCT
                                ItemId,
                                RealItemName AS ProgramName,
                                ClassName,
                                OCXTemplete,
                                SystemId,
                                sServerName,
                                sDBName,
                                ItemType,
                                SuperId,
                                OutputType
                            FROM CURdMenuTemp WITH (NOLOCK)
                            WHERE ItemType IN (2, 6)
                            ORDER BY ItemId", conn);

                        var items = new List<object>();
                        await using var reader = await cmd.ExecuteReaderAsync();
                        while (await reader.ReadAsync())
                        {
                            items.Add(new
                            {
                                itemId = reader["ItemId"]?.ToString()?.Trim() ?? "",
                                programName = reader["ProgramName"]?.ToString()?.Trim() ?? "",
                                className = reader["ClassName"]?.ToString()?.Trim(),
                                ocxTemplete = reader["OCXTemplete"]?.ToString()?.Trim(),
                                systemId = reader["SystemId"]?.ToString()?.Trim(),
                                serverName = reader["sServerName"]?.ToString()?.Trim(),
                                dbName = reader["sDBName"]?.ToString()?.Trim(),
                                itemType = reader["ItemType"] == DBNull.Value ? 0 : Convert.ToInt32(reader["ItemType"]),
                                superId = reader["SuperId"]?.ToString()?.Trim(),
                                outputType = reader["OutputType"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["OutputType"])
                            });
                        }

                        return Ok(items);
                    }
                }
                catch
                {
                    // 如果查詢失敗,繼續使用 CURdSysItems
                }

                // 從 CURdSysItems 查詢作為後備方案
                var sysItems = await _context.CurdSysItems
                    .AsNoTracking()
                    .Where(i => i.ItemType == 2 || i.ItemType == 6)
                    .Where(i => i.Enabled == 1)
                    .OrderBy(i => i.ItemId)
                    .Select(i => new
                    {
                        itemId = i.ItemId,
                        programName = i.ItemName,
                        className = i.ClassName,
                        ocxTemplete = i.Ocxtemplete,
                        systemId = i.SystemId,
                        serverName = (string?)null,
                        dbName = (string?)null,
                        itemType = i.ItemType,
                        superId = i.SuperId,
                        outputType = i.OutputType
                    })
                    .ToListAsync();

                return Ok(sysItems);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"查詢失敗: {ex.Message}" });
            }
        }

        /// <summary>
        /// 取得特定項目的詳細資訊
        /// </summary>
        [HttpGet("Item/{itemId}")]
        public async Task<IActionResult> GetItemInfo(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
                return BadRequest(new { error = "ItemId is required" });

            try
            {
                // 先嘗試從 CURdMenuTemp 查詢
                try
                {
                    await using var conn = new SqlConnection(_connStr);
                    await conn.OpenAsync();

                    await using var checkCmd = new SqlCommand(@"
                        IF OBJECT_ID('CURdMenuTemp', 'U') IS NOT NULL
                            SELECT 1
                        ELSE
                            SELECT 0", conn);

                    var tableExists = Convert.ToInt32(await checkCmd.ExecuteScalarAsync()) == 1;

                    if (tableExists)
                    {
                        await using var cmd = new SqlCommand(@"
                            SELECT TOP 1
                                ItemId,
                                RealItemName AS ProgramName,
                                ClassName,
                                OCXTemplete,
                                SystemId,
                                sServerName,
                                sDBName,
                                ItemType,
                                SuperId,
                                OutputType
                            FROM CURdMenuTemp WITH (NOLOCK)
                            WHERE ItemId = @itemId
                            AND ItemType IN (2, 6)", conn);

                        cmd.Parameters.Add(new SqlParameter("@itemId", SqlDbType.VarChar, 15) { Value = itemId.Trim() });

                        await using var reader = await cmd.ExecuteReaderAsync();
                        if (await reader.ReadAsync())
                        {
                            var result = new
                            {
                                itemId = reader["ItemId"]?.ToString()?.Trim() ?? "",
                                programName = reader["ProgramName"]?.ToString()?.Trim() ?? "",
                                className = reader["ClassName"]?.ToString()?.Trim(),
                                ocxTemplete = reader["OCXTemplete"]?.ToString()?.Trim(),
                                systemId = reader["SystemId"]?.ToString()?.Trim(),
                                serverName = reader["sServerName"]?.ToString()?.Trim(),
                                dbName = reader["sDBName"]?.ToString()?.Trim(),
                                itemType = reader["ItemType"] == DBNull.Value ? 0 : Convert.ToInt32(reader["ItemType"]),
                                superId = reader["SuperId"]?.ToString()?.Trim(),
                                outputType = reader["OutputType"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["OutputType"])
                            };

                            return Ok(result);
                        }
                    }
                }
                catch
                {
                    // 繼續使用 CURdSysItems
                }

                // 從 CURdSysItems 查詢
                var item = await _context.CurdSysItems
                    .AsNoTracking()
                    .Where(i => i.ItemId == itemId.Trim())
                    .Where(i => i.ItemType == 2 || i.ItemType == 6)
                    .Select(i => new
                    {
                        itemId = i.ItemId,
                        programName = i.ItemName,
                        className = i.ClassName,
                        ocxTemplete = i.Ocxtemplete,
                        systemId = i.SystemId,
                        serverName = (string?)null,
                        dbName = (string?)null,
                        itemType = i.ItemType,
                        superId = i.SuperId,
                        outputType = i.OutputType
                    })
                    .FirstOrDefaultAsync();

                if (item == null)
                    return NotFound(new { error = "項目不存在或您沒有權限" });

                return Ok(item);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"查詢失敗: {ex.Message}" });
            }
        }
    }
}
