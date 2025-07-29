using System.Data.SqlClient;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using PcbErpApi.Data;

[ApiController]
[Route("api/[controller]")]
public class OrderHeaderApiController : ControllerBase
{
    private readonly PcbErpContext _context;
    private readonly string _connStr;

    // [欄位名稱] => [型別字串，如 int、decimal、nvarchar、datetime]
    private static Dictionary<string, string> _spodOrderMainFieldTypes;
    private readonly object _fieldLock = new object();

    private void EnsureTableFields()
    {
        if (_spodOrderMainFieldTypes == null)
        {
            lock (_fieldLock)
            {
                if (_spodOrderMainFieldTypes == null)
                {
                    _spodOrderMainFieldTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    using (var conn = new SqlConnection(_connStr))
                    {
                        conn.Open();
                        var cmd = conn.CreateCommand();
                        cmd.CommandText = @"
                            SELECT COLUMN_NAME, DATA_TYPE
                            FROM INFORMATION_SCHEMA.COLUMNS
                            WHERE TABLE_NAME = 'SpodOrderMain'
                        ";
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var col = reader.GetString(0);
                                var typ = reader.GetString(1);
                                _spodOrderMainFieldTypes[col] = typ;
                            }
                        }
                    }
                }
            }
        }
    }

    public OrderHeaderApiController(PcbErpContext context, IConfiguration config)
    {
        _context = context;
        _connStr = config.GetConnectionString("DefaultConnection");
    }

    [HttpPost("SaveOrderHeader")]
    public async Task<IActionResult> SaveOrderHeader([FromBody] Dictionary<string, object> data)
    {
        EnsureTableFields();

        var paperNum = data["PaperNum"]?.ToString();
        if (string.IsNullOrEmpty(paperNum))
            return BadRequest("單號不可為空");

        // Step 1: 先查目前資料
        Dictionary<string, object> dbRow = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        using (var conn = new SqlConnection(_connStr))
        {
            await conn.OpenAsync();
            var selectCmd = conn.CreateCommand();
            selectCmd.CommandText = "SELECT * FROM SpodOrderMain WHERE PaperNum=@PaperNum";
            selectCmd.Parameters.AddWithValue("@PaperNum", paperNum);
            using (var reader = await selectCmd.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        dbRow[reader.GetName(i)] = reader.GetValue(i);
                    }
                }
                else
                {
                    return NotFound("找不到此單號");
                }
            }
        }

        // Step 2: 比對找出有異動的欄位（含型別轉換）
        var updateFields = new List<string>();
        var updateValues = new Dictionary<string, object>();
        foreach (var key in data.Keys)
        {
            if (key == "PaperNum" || !_spodOrderMainFieldTypes.ContainsKey(key))
                continue;

            object newVal = data[key];
            object dbVal = dbRow.ContainsKey(key) ? dbRow[key] : null;

            string dbType = _spodOrderMainFieldTypes[key].ToLower();

            // 轉型
            object newValFinal = null;
            if (newVal is JsonElement je)
            {
                if (dbType == "int" || dbType == "smallint" || dbType == "tinyint")
                {
                    if (je.ValueKind == JsonValueKind.Number) newValFinal = je.GetInt32();
                    else if (je.ValueKind == JsonValueKind.String && int.TryParse(je.GetString(), out var v)) newValFinal = v;
                }
                else if (dbType == "bigint")
                {
                    if (je.ValueKind == JsonValueKind.Number) newValFinal = je.GetInt64();
                    else if (je.ValueKind == JsonValueKind.String && long.TryParse(je.GetString(), out var v)) newValFinal = v;
                }
                else if (dbType == "decimal" || dbType == "numeric" || dbType == "money" || dbType == "smallmoney")
                {
                    if (je.ValueKind == JsonValueKind.Number) newValFinal = je.GetDecimal();
                    else if (je.ValueKind == JsonValueKind.String && decimal.TryParse(je.GetString(), out var v)) newValFinal = v;
                }
                else if (dbType == "bit")
                {
                    if (je.ValueKind == JsonValueKind.True || je.ValueKind == JsonValueKind.False)
                        newValFinal = je.GetBoolean();
                    else if (je.ValueKind == JsonValueKind.Number)
                        newValFinal = je.GetInt32() != 0;
                    else if (je.ValueKind == JsonValueKind.String && bool.TryParse(je.GetString(), out var bVal))
                        newValFinal = bVal;
                    else if (je.ValueKind == JsonValueKind.String && int.TryParse(je.GetString(), out var iVal))
                        newValFinal = iVal != 0;
                }
                else if (dbType == "datetime" || dbType == "smalldatetime" || dbType == "date" || dbType == "datetime2")
                {
                    if (je.ValueKind == JsonValueKind.String && DateTime.TryParse(je.GetString(), out var dtVal))
                        newValFinal = dtVal;
                }
                else if (dbType == "nvarchar" || dbType == "varchar")
                {
                    newValFinal = je.GetString();
                }
                else
                {
                    newValFinal = je.ToString();
                }
            }
            else if (dbType == "int" && newVal is string s1)
            {
                if (int.TryParse(s1, out var v)) newValFinal = v;
            }
            else if ((dbType == "decimal" || dbType == "numeric" || dbType == "money" || dbType == "smallmoney") && newVal is string s2)
            {
                if (decimal.TryParse(s2, out var v)) newValFinal = v;
            }
            else if (dbType == "nvarchar" || dbType == "varchar")
            {
                newValFinal = newVal?.ToString();
            }
            else
            {
                newValFinal = newVal;
            }

            // 與 DB 的值做比對（簡單處理 DBNull、null、ToString 等等，可自行優化）
            object dbValCompare = dbVal is DBNull ? null : dbVal;
            bool isDifferent;
            if (newValFinal == null && dbValCompare == null)
                isDifferent = false;
            else if (newValFinal == null || dbValCompare == null)
                isDifferent = true;
            else if (newValFinal is DateTime dt1 && dbValCompare is DateTime dt2)
                isDifferent = dt1 != dt2;
            else
                isDifferent = !newValFinal.Equals(dbValCompare);

            if (isDifferent)
            {
                updateFields.Add(key);
                updateValues[key] = newValFinal ?? DBNull.Value;
            }
        }

        if (!updateFields.Any())
            return Ok(new { updated = false });

        // Step 3: 組出只更新有異動的欄位
        var setClause = string.Join(", ", updateFields.Select(k => $"{k}=@{k}"));
        var sql = $"UPDATE SpodOrderMain SET {setClause} WHERE PaperNum=@PaperNum";

        using (var conn = new SqlConnection(_connStr))
        {
            await conn.OpenAsync();
            var cmd = conn.CreateCommand();
            cmd.CommandText = sql;

            foreach (var field in updateFields)
            {
                var dbType = _spodOrderMainFieldTypes[field].ToLower();
                var value = updateValues[field];

                SqlParameter param;
                if (dbType == "int" || dbType == "smallint" || dbType == "tinyint")
                    param = new SqlParameter($"@{field}", System.Data.SqlDbType.Int) { Value = value ?? DBNull.Value };
                else if (dbType == "bigint")
                    param = new SqlParameter($"@{field}", System.Data.SqlDbType.BigInt) { Value = value ?? DBNull.Value };
                else if (dbType == "decimal" || dbType == "numeric" || dbType == "money" || dbType == "smallmoney")
                    param = new SqlParameter($"@{field}", System.Data.SqlDbType.Decimal) { Value = value ?? DBNull.Value };
                else if (dbType == "datetime" || dbType == "smalldatetime" || dbType == "date" || dbType == "datetime2")
                    param = new SqlParameter($"@{field}", System.Data.SqlDbType.DateTime) { Value = value ?? DBNull.Value };
                else if (dbType == "bit")
                    param = new SqlParameter($"@{field}", System.Data.SqlDbType.Bit) { Value = value ?? DBNull.Value };
                else
                    param = new SqlParameter($"@{field}", value ?? DBNull.Value);

                cmd.Parameters.Add(param);
            }

            cmd.Parameters.AddWithValue("@PaperNum", paperNum);

            await cmd.ExecuteNonQueryAsync();
        }

        return Ok(new { updated = true, fields = updateFields });
    }
}
