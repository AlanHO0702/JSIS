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
    private static Dictionary<string, bool> _spodOrderMainFieldNullable; // ✅ 新增
    private readonly object _fieldLock = new object();

    private void EnsureTableFields()
    {
        if (_spodOrderMainFieldTypes == null || _spodOrderMainFieldNullable == null)
        {
            lock (_fieldLock)
            {
                if (_spodOrderMainFieldTypes == null || _spodOrderMainFieldNullable == null)
                {
                    _spodOrderMainFieldTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    _spodOrderMainFieldNullable = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

                    using (var conn = new SqlConnection(_connStr))
                    {
                        conn.Open();
                        var cmd = conn.CreateCommand();
                        cmd.CommandText = @"
                            SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
                            FROM INFORMATION_SCHEMA.COLUMNS
                            WHERE TABLE_NAME = 'SpodOrderMain'
                        ";
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var col = reader.GetString(0);
                                var typ = reader.GetString(1);
                                var isNullable = reader.GetString(2);

                                _spodOrderMainFieldTypes[col] = typ;
                                _spodOrderMainFieldNullable[col] = isNullable.Equals("YES", StringComparison.OrdinalIgnoreCase);
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
	    
	        object newValFinal = ConvertJsonToDbType(newVal, dbType); // <-- 你原本的轉型邏輯抽出去

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
                // ✅ 避免把 NOT NULL 欄位更新成 NULL
                if (newValFinal == null && !_spodOrderMainFieldNullable[key])
                {
                    continue; // 跳過，不更新
                }

                updateFields.Add(key);
                updateValues[key] = newValFinal ?? DBNull.Value;
            }
        }

        if (!updateFields.Any())
            return Ok(new { updated = false });

        // Step 3: 組 UPDATE SQL (保持原本程式)
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
        
        // Step 4: 更新完，再查一次最新資料 (因為 Trigger 可能改了其他欄位)
        Dictionary<string, object> latestRow = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
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
                        latestRow[reader.GetName(i)] = reader.GetValue(i);
                    }
                }
            }
        }

        return Ok(new { updated = true, fields = updateFields, data = latestRow });

    }

    // 你原本的轉型邏輯可以放這裡
    private object ConvertJsonToDbType(object newVal, string dbType)
    {
        if (newVal == null) return null;

        if (newVal is JsonElement je)
        {
            // ✅ 空字串要視為 null
            if (je.ValueKind == JsonValueKind.String && string.IsNullOrWhiteSpace(je.GetString()))
                return null;

            if (dbType == "int" && je.ValueKind == JsonValueKind.Number) return je.GetInt32();
            if (dbType == "bigint" && je.ValueKind == JsonValueKind.Number) return je.GetInt64();
            if ((dbType == "decimal" || dbType == "numeric" || dbType == "money") && je.ValueKind == JsonValueKind.Number) return je.GetDecimal();
            if (dbType == "bit" && (je.ValueKind == JsonValueKind.True || je.ValueKind == JsonValueKind.False)) return je.GetBoolean();
            if (dbType.Contains("date") && je.ValueKind == JsonValueKind.String && DateTime.TryParse(je.GetString(), out var dt)) return dt;
            if ((dbType == "nvarchar" || dbType == "varchar") && je.ValueKind == JsonValueKind.String) return je.GetString();
            return je.ToString();
        }

        // ✅ 如果是 string 且為空，直接回傳 null
        if (newVal is string s && string.IsNullOrWhiteSpace(s))
            return null;

        return newVal;
    }

}
