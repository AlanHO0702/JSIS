using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;

namespace PcbErpApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TableDictionarySettingsController : ControllerBase
{
    private readonly PcbErpContext _context;
    private readonly string _connStr;

    public TableDictionarySettingsController(PcbErpContext context, IConfiguration config)
    {
        _context = context;
        _connStr = config.GetConnectionString("DefaultConnection")
            ?? _context.Database.GetConnectionString()
            ?? throw new InvalidOperationException("Missing connection string.");
    }

    public class SystemOption
    {
        public string SystemId { get; set; } = string.Empty;
        public string SystemName { get; set; } = string.Empty;
        public int OrderNum { get; set; }
    }

    public class TableDictionaryRow
    {
        public string TableName { get; set; } = string.Empty;
        public string? DisplayLabel { get; set; }
        public string? TableNote { get; set; }
        public int? SerialNum { get; set; }
        public int? TableType { get; set; }
        public int? LevelNo { get; set; }
        public string? SystemId { get; set; }
        public string? SuperId { get; set; }
        public string? RealTableName { get; set; }
        public string? DisplayLabelCn { get; set; }
        public string? DisplayLabelEn { get; set; }
        public string? DisplayLabelJp { get; set; }
        public string? DisplayLabelTh { get; set; }
    }

    public class SystemTreeNode
    {
        public string ItemId { get; set; } = string.Empty;
        public int LevelNo { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string SuperId { get; set; } = string.Empty;
    }

    [HttpGet("systems")]
    public async Task<IActionResult> GetSystems()
    {
        var viewNameRows = await _context.CurdVSystems
            .AsNoTracking()
            .Where(x => x.SystemId != null)
            .Select(x => new { x.SystemId, x.SystemName })
            .ToListAsync();

        var viewNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var row in viewNameRows)
        {
            var key = row.SystemId?.Trim();
            if (string.IsNullOrWhiteSpace(key)) continue;
            if (!viewNames.ContainsKey(key))
                viewNames[key] = row.SystemName ?? string.Empty;
        }

        var level0NameRows = await _context.CurdSysItems
            .AsNoTracking()
            .Where(x => x.LevelNo == 0 && x.SystemId != null && x.ItemName != null)
            .OrderBy(x => x.SerialNum)
            .Select(x => new { x.SystemId, x.ItemName })
            .ToListAsync();

        var level0Names = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var row in level0NameRows)
        {
            var key = row.SystemId?.Trim();
            var name = row.ItemName?.Trim();
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(name)) continue;
            if (!level0Names.ContainsKey(key))
                level0Names[key] = name;
        }

        var options = await _context.CurdSystemSelects
            .AsNoTracking()
            .OrderBy(x => x.OrderNum)
            .ThenBy(x => x.SystemId)
            .Select(x => new SystemOption
            {
                SystemId = x.SystemId,
                OrderNum = x.OrderNum
            })
            .ToListAsync();

        foreach (var opt in options)
        {
            var key = (opt.SystemId ?? string.Empty).Trim();
            opt.SystemId = key;
            opt.SystemName = ResolveSystemName(key, viewNames, level0Names);
        }

        return Ok(options);
    }

    [HttpGet("rows")]
    public async Task<IActionResult> GetRows(
        [FromQuery] string? systemId = null,
        [FromQuery] bool useSystemFilter = true,
        [FromQuery] string? tableName = null,
        [FromQuery] string? displayLabel = null,
        [FromQuery] string? fieldName = null,
        [FromQuery] string? fieldDisplayLabel = null)
    {
        var list = new List<TableDictionaryRow>();

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        var sql = @"
SELECT DISTINCT
       t.TableName,
       t.DisplayLabel,
       t.TableNote,
       t.SerialNum,
       t.TableType,
       t.LevelNo,
       t.SystemId,
       t.SuperId,
       t.RealTableName,
       t.DisplayLabelCN,
       t.DisplayLabelEN,
       t.DisplayLabelJP,
       t.DisplayLabelTH
  FROM CURdTableName t WITH (NOLOCK)
  LEFT JOIN CURdTableField f WITH (NOLOCK) ON t.TableName = f.TableName
 WHERE 1 = 1";

        await using var cmd = new SqlCommand { Connection = conn };

        if (useSystemFilter && !string.IsNullOrWhiteSpace(systemId))
        {
            sql += " AND LTRIM(RTRIM(ISNULL(t.SystemId,''))) = @systemId";
            cmd.Parameters.AddWithValue("@systemId", systemId.Trim());
        }

        if (!string.IsNullOrWhiteSpace(tableName))
        {
            sql += " AND t.TableName LIKE @tableName";
            cmd.Parameters.AddWithValue("@tableName", tableName.Trim() + "%");
        }

        if (!string.IsNullOrWhiteSpace(displayLabel))
        {
            sql += " AND ISNULL(t.DisplayLabel,'') LIKE @displayLabel";
            cmd.Parameters.AddWithValue("@displayLabel", displayLabel.Trim() + "%");
        }

        if (!string.IsNullOrWhiteSpace(fieldName))
        {
            sql += " AND ISNULL(f.FieldName,'') LIKE @fieldName";
            cmd.Parameters.AddWithValue("@fieldName", fieldName.Trim() + "%");
        }

        if (!string.IsNullOrWhiteSpace(fieldDisplayLabel))
        {
            sql += " AND ISNULL(f.DisplayLabel,'') LIKE @fieldDisplayLabel";
            cmd.Parameters.AddWithValue("@fieldDisplayLabel", fieldDisplayLabel.Trim() + "%");
        }

        sql += " ORDER BY t.SystemId, t.TableName";
        cmd.CommandText = sql;

        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            list.Add(new TableDictionaryRow
            {
                TableName = rd["TableName"]?.ToString()?.Trim() ?? string.Empty,
                DisplayLabel = ReadNullableString(rd, "DisplayLabel"),
                TableNote = ReadNullableString(rd, "TableNote"),
                SerialNum = ReadNullableInt(rd, "SerialNum"),
                TableType = ReadNullableInt(rd, "TableType"),
                LevelNo = ReadNullableInt(rd, "LevelNo"),
                SystemId = ReadNullableString(rd, "SystemId"),
                SuperId = ReadNullableString(rd, "SuperId"),
                RealTableName = ReadNullableString(rd, "RealTableName"),
                DisplayLabelCn = ReadNullableString(rd, "DisplayLabelCN"),
                DisplayLabelEn = ReadNullableString(rd, "DisplayLabelEN"),
                DisplayLabelJp = ReadNullableString(rd, "DisplayLabelJP"),
                DisplayLabelTh = ReadNullableString(rd, "DisplayLabelTH")
            });
        }

        return Ok(list);
    }

    [HttpGet("tree")]
    public async Task<IActionResult> GetSystemTree([FromQuery] string? systemId = null)
    {
        var list = new List<SystemTreeNode>();

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand("exec CURdSystemsTree @SystemId", conn);
        cmd.Parameters.AddWithValue("@SystemId", string.IsNullOrWhiteSpace(systemId) ? string.Empty : systemId.Trim());

        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            list.Add(new SystemTreeNode
            {
                ItemId = ReadNullableString(rd, "ItemId") ?? string.Empty,
                LevelNo = ReadNullableInt(rd, "LevelNo") ?? 0,
                ItemName = ReadNullableString(rd, "ItemName") ?? string.Empty,
                SuperId = ReadNullableString(rd, "SuperId") ?? string.Empty
            });
        }

        return Ok(list);
    }

    [HttpPut("row/{tableName}")]
    public async Task<IActionResult> UpdateRow(string tableName, [FromBody] TableDictionaryRow input)
    {
        var key = (tableName ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(key))
            return BadRequest(new { success = false, message = "TableName 不可空白" });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand(@"
UPDATE CURdTableName
   SET DisplayLabel   = @DisplayLabel,
       TableNote      = @TableNote,
       SerialNum      = @SerialNum,
       TableType      = @TableType,
       LevelNo        = @LevelNo,
       SystemId       = @SystemId,
       SuperId        = @SuperId,
       RealTableName  = @RealTableName,
       DisplayLabelCN = @DisplayLabelCN,
       DisplayLabelEN = @DisplayLabelEN,
       DisplayLabelJP = @DisplayLabelJP,
       DisplayLabelTH = @DisplayLabelTH
 WHERE TableName = @TableName;", conn);

        cmd.Parameters.AddWithValue("@TableName", key);
        cmd.Parameters.AddWithValue("@DisplayLabel", ToDbValue(input.DisplayLabel));
        cmd.Parameters.AddWithValue("@TableNote", ToDbValue(input.TableNote));
        cmd.Parameters.AddWithValue("@SerialNum", (object?)input.SerialNum ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@TableType", (object?)input.TableType ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@LevelNo", (object?)input.LevelNo ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@SystemId", ToDbValue(input.SystemId));
        cmd.Parameters.AddWithValue("@SuperId", ToDbValue(input.SuperId));
        cmd.Parameters.AddWithValue("@RealTableName", ToDbValue(input.RealTableName));
        cmd.Parameters.AddWithValue("@DisplayLabelCN", ToDbValue(input.DisplayLabelCn));
        cmd.Parameters.AddWithValue("@DisplayLabelEN", ToDbValue(input.DisplayLabelEn));
        cmd.Parameters.AddWithValue("@DisplayLabelJP", ToDbValue(input.DisplayLabelJp));
        cmd.Parameters.AddWithValue("@DisplayLabelTH", ToDbValue(input.DisplayLabelTh));

        var affected = await cmd.ExecuteNonQueryAsync();
        if (affected <= 0)
            return NotFound(new { success = false, message = $"找不到 TableName [{key}]" });

        return Ok(new { success = true, affected });
    }

    private static string ResolveSystemName(
        string systemId,
        IReadOnlyDictionary<string, string> viewNames,
        IReadOnlyDictionary<string, string> level0Names)
    {
        if (viewNames.TryGetValue(systemId, out var viewName) && !string.IsNullOrWhiteSpace(viewName))
            return viewName.Trim();
        if (level0Names.TryGetValue(systemId, out var level0Name) && !string.IsNullOrWhiteSpace(level0Name))
            return level0Name.Trim();
        return string.Empty;
    }

    private static string? ReadNullableString(SqlDataReader rd, string col)
    {
        var v = rd[col];
        if (v == DBNull.Value) return null;
        return v?.ToString()?.Trim();
    }

    private static int? ReadNullableInt(SqlDataReader rd, string col)
    {
        var v = rd[col];
        if (v == DBNull.Value) return null;
        return int.TryParse(v?.ToString(), out var n) ? n : null;
    }

    private static object ToDbValue(string? value)
        => string.IsNullOrWhiteSpace(value) ? DBNull.Value : value.Trim();
}
