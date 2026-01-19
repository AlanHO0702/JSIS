using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;

namespace PcbErpApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MatDetailApiController : ControllerBase
{
    private readonly PcbErpContext _context;
    private readonly string _connStr;
    private readonly ILogger<MatDetailApiController> _logger;

    public MatDetailApiController(
        PcbErpContext context,
        IConfiguration config,
        ILogger<MatDetailApiController> logger)
    {
        _context = context;
        _connStr = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        _logger = logger;
    }

    // =========================================
    // 1. 取得物料基本資料 (呼叫 MINdLookMatInfoDtl)
    // =========================================
    [HttpGet("GetMatInfo")]
    public async Task<IActionResult> GetMatInfo(
        [FromQuery] string partNum,
        [FromQuery] string? useId = "A001")
    {
        if (string.IsNullOrWhiteSpace(partNum))
            return BadRequest("PartNum is required");

        try
        {
            await using var conn = new SqlConnection(_connStr);
            await conn.OpenAsync();

            // 使用直接 SQL 呼叫，因為 SP 參數名稱不明確
            var sql = "EXEC MINdLookMatInfoDtl @p0, @p1, @p2, @p3, @p4, @p5";
            await using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@p0", partNum);
            cmd.Parameters.AddWithValue("@p1", 0);
            cmd.Parameters.AddWithValue("@p2", DBNull.Value);
            cmd.Parameters.AddWithValue("@p3", string.Empty);
            cmd.Parameters.AddWithValue("@p4", 255);
            cmd.Parameters.AddWithValue("@p5", useId ?? "A001");

            var result = new Dictionary<string, object>();

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var name = reader.GetName(i);
                    var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    result[name] = value ?? DBNull.Value;
                }
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting material info for PartNum: {PartNum}", partNum);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // =========================================
    // 2. 取得庫存彙總 (呼叫 MINdLookMatStockQnty)
    // =========================================
    [HttpGet("GetStockSummary")]
    public async Task<IActionResult> GetStockSummary(
        [FromQuery] string partNum,
        [FromQuery] string? useId = "A001")
    {
        if (string.IsNullOrWhiteSpace(partNum))
            return BadRequest("PartNum is required");

        try
        {
            await using var conn = new SqlConnection(_connStr);
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("MINdLookMatStockQnty", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@PartNum", partNum);
            cmd.Parameters.AddWithValue("@UseId", useId ?? "A001");

            var list = new List<Dictionary<string, object>>();

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var name = reader.GetName(i);
                    var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    row[name] = value ?? DBNull.Value;
                }
                list.Add(row);
            }

            return Ok(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stock summary for PartNum: {PartNum}", partNum);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // =========================================
    // 3. 取得庫存明細 (呼叫 MINdLookMatStockQntyDtl)
    // =========================================
    [HttpGet("GetStockDetail")]
    public async Task<IActionResult> GetStockDetail(
        [FromQuery] string partNum,
        [FromQuery] string stockId)
    {
        if (string.IsNullOrWhiteSpace(partNum) || string.IsNullOrWhiteSpace(stockId))
            return BadRequest("PartNum and StockId are required");

        try
        {
            await using var conn = new SqlConnection(_connStr);
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("MINdLookMatStockQntyDtl", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@PartNum", partNum);
            cmd.Parameters.AddWithValue("@StockId", stockId);

            var list = new List<Dictionary<string, object>>();

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var name = reader.GetName(i);
                    var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    row[name] = value ?? DBNull.Value;
                }
                list.Add(row);
            }

            return Ok(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stock detail for PartNum: {PartNum}, StockId: {StockId}", partNum, stockId);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // =========================================
    // 4. 取得供應明細 (呼叫 MINdLookMatIn)
    // =========================================
    [HttpGet("GetIncomingSupply")]
    public async Task<IActionResult> GetIncomingSupply([FromQuery] int spId)
    {
        if (spId <= 0)
            return BadRequest("SPId is required");

        try
        {
            await using var conn = new SqlConnection(_connStr);
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("MINdLookMatIn", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@SPId", spId);

            var list = new List<Dictionary<string, object>>();

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var name = reader.GetName(i);
                    var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    row[name] = value ?? DBNull.Value;
                }
                list.Add(row);
            }

            return Ok(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting incoming supply for SPId: {SPId}", spId);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // =========================================
    // 5. 取得需求明細 (呼叫 MINdLookMatOut)
    // =========================================
    [HttpGet("GetOutgoingDemand")]
    public async Task<IActionResult> GetOutgoingDemand([FromQuery] int spId)
    {
        if (spId <= 0)
            return BadRequest("SPId is required");

        try
        {
            await using var conn = new SqlConnection(_connStr);
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("MINdLookMatOut", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@SPId", spId);

            var list = new List<Dictionary<string, object>>();

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var name = reader.GetName(i);
                    var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    row[name] = value ?? DBNull.Value;
                }
                list.Add(row);
            }

            return Ok(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting outgoing demand for SPId: {SPId}", spId);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // =========================================
    // 6. 取得往來單據明細 (呼叫 MINdLookMatDtl)
    // =========================================
    [HttpGet("GetPaperDetails")]
    public async Task<IActionResult> GetPaperDetails(
        [FromQuery] int spId,
        [FromQuery] int? filterNum = null)
    {
        if (spId <= 0)
            return BadRequest("SPId is required");

        try
        {
            await using var conn = new SqlConnection(_connStr);
            await conn.OpenAsync();

            var sql = filterNum.HasValue
                ? "SELECT * FROM MINdLookMatDtl (NOLOCK) WHERE SPId = @SPId AND FilterNum = @FilterNum"
                : "SELECT * FROM MINdLookMatDtl (NOLOCK) WHERE SPId = @SPId";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@SPId", spId);
            if (filterNum.HasValue)
                cmd.Parameters.AddWithValue("@FilterNum", filterNum.Value);

            var list = new List<Dictionary<string, object>>();

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var name = reader.GetName(i);
                    var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    row[name] = value ?? DBNull.Value;
                }
                list.Add(row);
            }

            return Ok(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paper details for SPId: {SPId}", spId);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // =========================================
    // 7. 取得預計庫存 (呼叫 MINdLookMatPreCount)
    // =========================================
    [HttpGet("GetPreCount")]
    public async Task<IActionResult> GetPreCount([FromQuery] int spId)
    {
        if (spId <= 0)
            return BadRequest("SPId is required");

        try
        {
            await using var conn = new SqlConnection(_connStr);
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("MINdLookMatPreCount", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@SPId", spId);

            var list = new List<Dictionary<string, object>>();

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var name = reader.GetName(i);
                    var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    row[name] = value ?? DBNull.Value;
                }
                list.Add(row);
            }

            return Ok(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pre-count for SPId: {SPId}", spId);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // =========================================
    // 8. 取得製程現帳 (呼叫 MINdWIPDetail)
    // =========================================
    [HttpGet("GetWipDetail")]
    public async Task<IActionResult> GetWipDetail([FromQuery] string partNum)
    {
        if (string.IsNullOrWhiteSpace(partNum))
            return BadRequest("PartNum is required");

        try
        {
            await using var conn = new SqlConnection(_connStr);
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("MINdWIPDetail", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@PartNum", partNum);

            var list = new List<Dictionary<string, object>>();

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var name = reader.GetName(i);
                    var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    row[name] = value ?? DBNull.Value;
                }
                list.Add(row);
            }

            return Ok(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting WIP detail for PartNum: {PartNum}", partNum);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // =========================================
    // 8. 取得倉庫進出歷程 (呼叫 MINdStockHIOGet SP)
    // =========================================
    [HttpGet("GetStockHistory")]
    public async Task<IActionResult> GetStockHistory(
        [FromQuery] string partNum,
        [FromQuery] string? stockId = "")
    {
        if (string.IsNullOrWhiteSpace(partNum))
            return BadRequest("PartNum is required");

        try
        {
            await using var conn = new SqlConnection(_connStr);
            await conn.OpenAsync();

            // 呼叫 SP: exec MINdStockHIOGet @PartNum, @StockId, 1
            await using var cmd = new SqlCommand("MINdStockHIOGet", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@PartNum", partNum);
            cmd.Parameters.AddWithValue("@StockId", string.IsNullOrWhiteSpace(stockId) ? "" : stockId);

            var list = new List<Dictionary<string, object>>();

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var name = reader.GetName(i);
                    var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    row[name] = value ?? DBNull.Value;
                }
                list.Add(row);
            }

            return Ok(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stock history for PartNum: {PartNum}, StockId: {StockId}", partNum, stockId);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // =========================================
    // 9. 整合查詢：一次取得所有資料
    // =========================================
    [HttpGet("GetFullDetail")]
    public async Task<IActionResult> GetFullDetail(
        [FromQuery] string partNum,
        [FromQuery] string? useId = "A001")
    {
        if (string.IsNullOrWhiteSpace(partNum))
            return BadRequest("PartNum is required");

        try
        {
            var result = new Dictionary<string, object>();

            // 1. 取得 SPId
            int? spId = null;
            await using (var conn = new SqlConnection(_connStr))
            {
                await conn.OpenAsync();

                var sql = "EXEC MINdLookMatInfoDtl @p0, @p1, @p2, @p3, @p4, @p5";
                await using var cmd = new SqlCommand(sql, conn);

                cmd.Parameters.AddWithValue("@p0", partNum);
                cmd.Parameters.AddWithValue("@p1", 0);
                cmd.Parameters.AddWithValue("@p2", DBNull.Value);
                cmd.Parameters.AddWithValue("@p3", string.Empty);
                cmd.Parameters.AddWithValue("@p4", 255);
                cmd.Parameters.AddWithValue("@p5", useId ?? "A001");

                await using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var baseData = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        var name = reader.GetName(i);
                        var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                        baseData[name] = value ?? DBNull.Value;

                        if (name.Equals("SPId", StringComparison.OrdinalIgnoreCase) && value != null)
                        {
                            spId = Convert.ToInt32(value);
                        }
                    }
                    result["baseData"] = baseData;
                }
            }

            if (!spId.HasValue)
            {
                return NotFound(new { error = "Material not found or SPId not generated" });
            }

            // 2. 從 MINdLookMatDtl 取得 Unit、CanUseQnty、NeedQnty
            await using (var conn = new SqlConnection(_connStr))
            {
                await conn.OpenAsync();
                var sql = "SELECT TOP 1 Unit, CanUseQnty, NeedQnty FROM MINdLookMatDtl WITH(NOLOCK) WHERE SPId = @SPId";
                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@SPId", spId.Value);

                await using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var baseData = result["baseData"] as Dictionary<string, object>;
                    if (baseData != null)
                    {
                        baseData["Unit"] = reader.IsDBNull(0) ? "" : reader.GetString(0);
                        baseData["CanUseQnty"] = reader.IsDBNull(1) ? 0m : reader.GetDecimal(1);
                        baseData["NeedQnty"] = reader.IsDBNull(2) ? 0m : reader.GetDecimal(2);
                    }
                }
            }

            // 3. 並行取得其他資料
            var stockTask = GetStockSummaryInternal(partNum, useId);
            var incomingTask = GetIncomingSupplyInternal(spId.Value);
            var outgoingTask = GetOutgoingDemandInternal(spId.Value);
            var preCountTask = GetPreCountInternal(spId.Value);

            await Task.WhenAll(stockTask, incomingTask, outgoingTask, preCountTask);

            result["stockSummary"] = stockTask.Result;
            result["incomingSupply"] = incomingTask.Result;
            result["outgoingDemand"] = outgoingTask.Result;
            result["preCount"] = preCountTask.Result;

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting full detail for PartNum: {PartNum}", partNum);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // 內部方法，避免重複程式碼
    private async Task<List<Dictionary<string, object>>> GetStockSummaryInternal(string partNum, string? useId)
    {
        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand("MINdLookMatStockQnty", conn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.AddWithValue("@PartNum", partNum);
        cmd.Parameters.AddWithValue("@UseId", useId ?? "A001");

        var list = new List<Dictionary<string, object>>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var row = new Dictionary<string, object>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                row[reader.GetName(i)] = reader.IsDBNull(i) ? DBNull.Value : reader.GetValue(i);
            }
            list.Add(row);
        }
        return list;
    }

    private async Task<List<Dictionary<string, object>>> GetIncomingSupplyInternal(int spId)
    {
        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand("MINdLookMatIn", conn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.AddWithValue("@SPId", spId);

        var list = new List<Dictionary<string, object>>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var row = new Dictionary<string, object>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                row[reader.GetName(i)] = reader.IsDBNull(i) ? DBNull.Value : reader.GetValue(i);
            }
            list.Add(row);
        }
        return list;
    }

    private async Task<List<Dictionary<string, object>>> GetOutgoingDemandInternal(int spId)
    {
        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand("MINdLookMatOut", conn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.AddWithValue("@SPId", spId);

        var list = new List<Dictionary<string, object>>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var row = new Dictionary<string, object>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                row[reader.GetName(i)] = reader.IsDBNull(i) ? DBNull.Value : reader.GetValue(i);
            }
            list.Add(row);
        }
        return list;
    }

    private async Task<List<Dictionary<string, object>>> GetPreCountInternal(int spId)
    {
        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand("MINdLookMatPreCount", conn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.AddWithValue("@SPId", spId);

        var list = new List<Dictionary<string, object>>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var row = new Dictionary<string, object>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                row[reader.GetName(i)] = reader.IsDBNull(i) ? DBNull.Value : reader.GetValue(i);
            }
            list.Add(row);
        }
        return list;
    }

    // =========================================
    // 10. 取得辭典欄位定義
    // =========================================
    [HttpGet("GetTableFields/{tableName}")]
    public async Task<IActionResult> GetTableFields(string tableName)
    {
        try
        {
            var fields = await _context.CURdTableFields
                .Where(x => x.TableName != null &&
                           (x.TableName.ToLower() == tableName.ToLower() ||
                            x.TableName.ToLower().Replace("dbo.", "") == tableName.ToLower()))
                .OrderBy(x => x.SerialNum)
                .Select(x => new
                {
                    x.FieldName,
                    x.DisplayLabel,
                    x.DisplaySize,
                    x.Visible,
                    x.SerialNum,
                    x.DataType
                })
                .ToListAsync();

            // 檢查語系表
            var langFields = await _context.CurdTableFieldLangs
                .Where(x => x.LanguageId == "TW" &&
                           x.TableName != null &&
                           (x.TableName.ToLower() == tableName.ToLower() ||
                            x.TableName.ToLower().Replace("dbo.", "") == tableName.ToLower()))
                .ToListAsync();

            var result = fields.Select(f =>
            {
                var lang = langFields.FirstOrDefault(l =>
                    l.FieldName?.Equals(f.FieldName, StringComparison.OrdinalIgnoreCase) == true);

                return new
                {
                    f.FieldName,
                    DisplayLabel = !string.IsNullOrWhiteSpace(lang?.DisplayLabel) ? lang.DisplayLabel : f.DisplayLabel,
                    f.DisplaySize,
                    f.Visible,
                    f.SerialNum,
                    f.DataType
                };
            }).ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting table fields for: {TableName}", tableName);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // =========================================
    // 11. 取得辭典標題名稱
    // =========================================
    [HttpGet("GetDictTitles")]
    public async Task<IActionResult> GetDictTitles()
    {
        try
        {
            var spNames = new[]
            {
                "MINdLookMatStockQnty",      // 現有庫存
                "MINdLookMatIn",             // 預計供給
                "MINdLookMatOut",            // 需求
                "MINdLookMatStockQntyDtl",   // 庫存一覽
                "MINdLookMatDtl",            // 單據明細
                "MINdLookMatPreCount",       // 預估存量
                "MINdStockHIOGet",           // 倉庫進出歷史
                "MINdWIPDetail"              // 製程現帳
            };

            var titles = new Dictionary<string, string>();

            foreach (var spName in spNames)
            {
                var field = await _context.CURdTableFields
                    .Where(x => x.TableName != null &&
                               (x.TableName.ToLower() == spName.ToLower() ||
                                x.TableName.ToLower().Replace("dbo.", "") == spName.ToLower()))
                    .Select(x => new { x.TableName, x.DisplayLabel })
                    .FirstOrDefaultAsync();

                if (field != null && !string.IsNullOrWhiteSpace(field.DisplayLabel))
                {
                    titles[spName] = field.DisplayLabel;
                }
                else
                {
                    // 如果找不到，使用預設值
                    titles[spName] = spName switch
                    {
                        "MINdLookMatStockQnty" => "現有庫存",
                        "MINdLookMatIn" => "預計供給",
                        "MINdLookMatOut" => "需求",
                        "MINdLookMatStockQntyDtl" => "庫存一覽",
                        "MINdLookMatDtl" => "單據明細",
                        "MINdLookMatPreCount" => "預估存量",
                        "MINdStockHIOGet" => "倉庫進出歷史",
                        "MINdWIPDetail" => "製程現帳",
                        _ => spName
                    };
                }
            }

            // 檢查語系表
            var langFields = await _context.CurdTableFieldLangs
                .Where(x => x.LanguageId == "TW" &&
                           x.TableName != null &&
                           spNames.Contains(x.TableName))
                .ToListAsync();

            foreach (var lang in langFields)
            {
                if (!string.IsNullOrWhiteSpace(lang.DisplayLabel) && lang.TableName != null)
                {
                    titles[lang.TableName] = lang.DisplayLabel;
                }
            }

            return Ok(titles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dictionary titles");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}