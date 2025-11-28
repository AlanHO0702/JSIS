using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace JSIS.Controllers
{
    /// <summary>
    /// 動態 SP 執行控制器
    /// 根據 CURdOCXItemCustButton 和 CURdOCXSearchParams 的設定，動態執行 SP
    /// 不需要事先在程式碼中定義白名單
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class DynamicSpController : ControllerBase
    {
        private readonly string _connStr;

        public DynamicSpController(IConfiguration config)
        {
            _connStr = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        /// <summary>
        /// 執行動態按鈕的 SP
        /// </summary>
        /// <param name="request">包含 ItemId, ButtonName, 以及前端收集的參數</param>
        [HttpPost("exec")]
        public async Task<IActionResult> Execute([FromBody] DynamicSpRequest request)
        {
            Console.WriteLine("==================== [DynamicSp] 收到執行請求 ====================");
            Console.WriteLine($"[DynamicSp] ItemId: {request.ItemId}");
            Console.WriteLine($"[DynamicSp] ButtonName: {request.ButtonName}");
            Console.WriteLine($"[DynamicSp] 前端傳入的參數 (Args):");
            if (request.Args != null)
            {
                foreach (var kvp in request.Args)
                {
                    Console.WriteLine($"  - {kvp.Key} = {kvp.Value}");
                }
            }
            else
            {
                Console.WriteLine("  (無參數)");
            }

            if (string.IsNullOrWhiteSpace(request.ItemId) || string.IsNullOrWhiteSpace(request.ButtonName))
            {
                Console.WriteLine("[DynamicSp] ❌ 錯誤: ItemId 和 ButtonName 為必填");
                return BadRequest(new { ok = false, error = "ItemId 和 ButtonName 為必填" });
            }

            await using var conn = new SqlConnection(_connStr);
            await conn.OpenAsync();

            try
            {
                // 1. 從 CURdOCXItemCustButton 取得 SP 名稱
                Console.WriteLine("[DynamicSp] 步驟1: 查詢按鈕設定，取得 SP 名稱...");
                var spName = await GetSpName(conn, request.ItemId, request.ButtonName);
                Console.WriteLine($"[DynamicSp] 查詢到的 SP 名稱: {spName ?? "(未設定)"}");

                if (string.IsNullOrWhiteSpace(spName))
                {
                    Console.WriteLine($"[DynamicSp] ❌ 找不到按鈕設定或未設定 SP: {request.ItemId}/{request.ButtonName}");
                    return BadRequest(new { ok = false, error = $"找不到按鈕設定或未設定 SP：{request.ItemId}/{request.ButtonName}" });
                }

                // 2. 從 SQL Server 取得 SP 實際接受的參數
                Console.WriteLine("[DynamicSp] 步驟2: 取得 SP 實際接受的參數列表...");
                var spFullName = spName.StartsWith("dbo.") ? spName : $"dbo.{spName}";
                var spAcceptedParams = await GetSpParameters(conn, spFullName);
                Console.WriteLine($"[DynamicSp] SP 接受的參數: {string.Join(", ", spAcceptedParams)}");

                // 3. 從 CURdOCXSearchParams 取得參數定義，並根據 SP 實際參數調整
                Console.WriteLine("[DynamicSp] 步驟3: 從資料庫取得參數定義...");
                var paramDefs = await GetParamDefinitions(conn, request.ItemId, request.ButtonName, spAcceptedParams);
                Console.WriteLine($"[DynamicSp] 資料庫中定義的參數: {paramDefs.Count} 個");
                foreach (var p in paramDefs)
                {
                    Console.WriteLine($"  - {p.ParamName}: DefaultValue={p.DefaultValue}");
                }

                // 4. 組合 SQL 命令
                await using var cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = spFullName;

                // 加入返回值參數（用於接收 RETURN 值）
                var returnParam = cmd.Parameters.Add("@RETURN_VALUE", SqlDbType.Int);
                returnParam.Direction = ParameterDirection.ReturnValue;

                // 記錄已處理的參數名稱（避免重複）
                var addedParams = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                // 5a. 先根據資料庫定義加入參數（只加入 SP 實際接受的參數）
                foreach (var paramDef in paramDefs)
                {
                    var paramName = paramDef.ParamName;
                    var cleanName = paramName.TrimStart('@');

                    // 檢查 SP 是否接受此參數
                    if (!spAcceptedParams.Contains(cleanName))
                    {
                        continue; // SP 不接受此參數，跳過
                    }

                    var sqlParamName = paramName.StartsWith("@") ? paramName : $"@{paramName}";
                    var lookupKeys = new[] { paramName, sqlParamName, cleanName };

                    object? value = null;
                    foreach (var key in lookupKeys)
                    {
                        if (request.Args != null && request.Args.TryGetValue(key, out var v) && v != null)
                        {
                            value = ConvertJsonElement(v);
                            break;
                        }
                    }

                    // 如果前端沒傳，使用預設值
                    if (value == null && !string.IsNullOrEmpty(paramDef.DefaultValue))
                    {
                        value = paramDef.DefaultValue;
                    }

                    cmd.Parameters.AddWithValue(sqlParamName, value ?? DBNull.Value);
                    addedParams.Add(cleanName);
                }

                // 5b. 加入前端傳來但資料庫沒定義的參數（只加入 SP 實際接受的參數）
                if (request.Args != null)
                {
                    foreach (var kvp in request.Args)
                    {
                        var cleanName = kvp.Key.TrimStart('@');
                        if (addedParams.Contains(cleanName)) continue; // 已經加過了

                        // 檢查 SP 是否接受此參數
                        if (!spAcceptedParams.Contains(cleanName))
                        {
                            continue; // SP 不接受此參數，跳過
                        }

                        var sqlParamName = $"@{cleanName}";
                        var convertedValue = ConvertJsonElement(kvp.Value);
                        cmd.Parameters.AddWithValue(sqlParamName, convertedValue ?? DBNull.Value);
                        addedParams.Add(cleanName);
                    }
                }

                // Debug: 記錄實際執行的參數
                Console.WriteLine("[DynamicSp] 步驟4: 準備執行 SP...");
                Console.WriteLine($"[DynamicSp] SP 全名: {cmd.CommandText}");
                Console.WriteLine($"[DynamicSp] 實際傳入的參數:");
                var paramLog = string.Join(", ", cmd.Parameters.Cast<SqlParameter>()
                    .Select(p => $"{p.ParameterName}={p.Value}"));
                foreach (SqlParameter p in cmd.Parameters)
                {
                    Console.WriteLine($"  - {p.ParameterName} = {p.Value} (Type: {p.SqlDbType})");
                }

                // 5. 執行 SP（使用 ExecuteReader 來捕捉 SELECT 輸出的錯誤訊息）
                Console.WriteLine("[DynamicSp] 執行中...");

                string? errorMessage = null;
                var resultSets = new List<List<Dictionary<string, object?>>>();

                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    // 讀取所有結果集
                    do
                    {
                        var rows = new List<Dictionary<string, object?>>();
                        while (await reader.ReadAsync())
                        {
                            var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                var fieldName = reader.GetName(i);
                                var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                                row[fieldName] = value;

                                // 檢查是否有錯誤訊息欄位（常見命名：ErrorMsg, ErrMsg, Message, Msg）
                                if ((fieldName.Equals("ErrorMsg", StringComparison.OrdinalIgnoreCase) ||
                                     fieldName.Equals("ErrMsg", StringComparison.OrdinalIgnoreCase) ||
                                     fieldName.Equals("Message", StringComparison.OrdinalIgnoreCase) ||
                                     fieldName.Equals("Msg", StringComparison.OrdinalIgnoreCase)) &&
                                    value != null && !string.IsNullOrWhiteSpace(value.ToString()))
                                {
                                    errorMessage = value.ToString();
                                }
                            }
                            rows.Add(row);
                        }
                        if (rows.Count > 0)
                        {
                            resultSets.Add(rows);
                            Console.WriteLine($"[DynamicSp] 結果集 {resultSets.Count}: {rows.Count} 列");
                            // 記錄第一列資料（用於檢查錯誤訊息）
                            if (rows.Count > 0)
                            {
                                Console.WriteLine($"[DynamicSp] 第一列資料: {string.Join(", ", rows[0].Select(kvp => $"{kvp.Key}={kvp.Value}"))}");
                            }
                        }
                    } while (await reader.NextResultAsync());
                }

                // 檢查返回值
                var returnValue = returnParam.Value as int? ?? 0;
                Console.WriteLine($"[DynamicSp] SP 返回值 (RETURN): {returnValue}");

                // 判斷是否有錯誤
                if (returnValue != 0)
                {
                    var errMsg = errorMessage ?? $"SP 返回錯誤碼: {returnValue}";
                    Console.WriteLine($"[DynamicSp] ❌ SP 執行失敗: {errMsg}");
                    return Ok(new {
                        ok = false,
                        error = errMsg,
                        returnValue = returnValue,
                        resultSets = resultSets,
                        spName = cmd.CommandText
                    });
                }

                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    Console.WriteLine($"[DynamicSp] ❌ SP 回傳錯誤訊息: {errorMessage}");
                    return Ok(new {
                        ok = false,
                        error = errorMessage,
                        returnValue = returnValue,
                        resultSets = resultSets,
                        spName = cmd.CommandText
                    });
                }

                Console.WriteLine($"[DynamicSp] ✅ 執行完成，返回值: {returnValue}");
                Console.WriteLine("==================== [DynamicSp] 執行成功 ====================");

                return Ok(new {
                    ok = true,
                    returnValue = returnValue,
                    resultSets = resultSets,
                    spName = cmd.CommandText,
                    debug = new { parameters = paramLog }
                });
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"[DynamicSp] ❌ SQL 錯誤: {ex.Message}");
                Console.WriteLine($"[DynamicSp] SQL 錯誤詳情: {ex}");
                return StatusCode(500, new { ok = false, error = ex.Message, spError = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DynamicSp] ❌ 一般錯誤: {ex.Message}");
                Console.WriteLine($"[DynamicSp] 錯誤堆疊: {ex.StackTrace}");
                return StatusCode(500, new { ok = false, error = ex.GetBaseException().Message });
            }
        }

        /// <summary>
        /// 轉換 JsonElement 成實際的值
        /// </summary>
        private static object? ConvertJsonElement(object? value)
        {
            if (value == null) return null;

            // 如果是 JsonElement，需要轉換成實際的值
            if (value is System.Text.Json.JsonElement jsonElement)
            {
                return jsonElement.ValueKind switch
                {
                    System.Text.Json.JsonValueKind.String => jsonElement.GetString(),
                    System.Text.Json.JsonValueKind.Number => jsonElement.TryGetInt32(out var i) ? i : jsonElement.GetDouble(),
                    System.Text.Json.JsonValueKind.True => true,
                    System.Text.Json.JsonValueKind.False => false,
                    System.Text.Json.JsonValueKind.Null => null,
                    _ => jsonElement.ToString()
                };
            }

            return value;
        }

        /// <summary>
        /// 從 SQL Server 系統表取得 SP 接受的參數列表
        /// </summary>
        private async Task<HashSet<string>> GetSpParameters(SqlConnection conn, string spName)
        {
            const string sql = @"
                SELECT p.name
                FROM sys.parameters p
                INNER JOIN sys.objects o ON p.object_id = o.object_id
                WHERE o.type IN ('P', 'PC')  -- Stored Procedure
                  AND OBJECT_ID(@SpName) = o.object_id
                ORDER BY p.parameter_id";

            var paramNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@SpName", spName);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var name = reader.GetString(0).TrimStart('@');
                paramNames.Add(name);
            }

            return paramNames;
        }

        /// <summary>
        /// 從 CURdOCXItemCustButton 取得 SP 名稱
        /// </summary>
        private async Task<string?> GetSpName(SqlConnection conn, string itemId, string buttonName)
        {
            const string sql = @"
                SELECT SpName
                FROM CURdOCXItemCustButton WITH (NOLOCK)
                WHERE ItemId = @ItemId AND ButtonName = @ButtonName";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ItemId", itemId);
            cmd.Parameters.AddWithValue("@ButtonName", buttonName);

            var result = await cmd.ExecuteScalarAsync();
            return result?.ToString();
        }

        /// <summary>
        /// 從參數表取得參數定義（優先使用 CURdOCXItmCusBtnParam，若無則用 CURdOCXSearchParams）
        /// </summary>
        private async Task<List<ParamDefinition>> GetParamDefinitions(SqlConnection conn, string itemId, string buttonName, HashSet<string> spAcceptedParams)
        {
            var list = new List<ParamDefinition>();

            // 1. 先從 CURdOCXItmCusBtnParam 查詢（新表，較準確）
            const string sql1 = @"
                SELECT ParamFieldName, ParamType, SeqNum
                FROM CURdOCXItmCusBtnParam WITH (NOLOCK)
                WHERE ItemId = @ItemId AND ButtonName = @ButtonName
                ORDER BY SeqNum";

            await using (var cmd1 = new SqlCommand(sql1, conn))
            {
                cmd1.Parameters.AddWithValue("@ItemId", itemId);
                cmd1.Parameters.AddWithValue("@ButtonName", buttonName);

                await using var reader1 = await cmd1.ExecuteReaderAsync();
                while (await reader1.ReadAsync())
                {
                    var paramFieldName = reader1["ParamFieldName"]?.ToString();
                    var paramType = reader1["ParamType"] as int? ?? 0;

                    // ParamType = 5 時，ParamFieldName 可能是 NULL，表示使用目前單號
                    // 這時候參數名稱應該是 @PaperNum 或 @DLLPaperNum
                    string paramName;
                    if (paramType == 5)
                    {
                        // 目前單號：根據 SP 實際接受的參數決定使用哪個名稱
                        if (spAcceptedParams.Contains("DLLPaperNum"))
                        {
                            paramName = "@DLLPaperNum";
                        }
                        else if (spAcceptedParams.Contains("PaperNum"))
                        {
                            paramName = "@PaperNum";
                        }
                        else
                        {
                            // SP 不接受單號參數，跳過
                            continue;
                        }
                    }
                    else if (!string.IsNullOrEmpty(paramFieldName))
                    {
                        paramName = paramFieldName.StartsWith("@") ? paramFieldName : $"@{paramFieldName}";
                    }
                    else
                    {
                        continue; // 跳過無效的參數定義
                    }

                    list.Add(new ParamDefinition
                    {
                        ParamName = paramName,
                        ParamType = paramType,
                        ParamFieldName = paramFieldName,
                        DefaultValue = null,
                        ParamValue = null
                    });
                }
            }

            // 2. 如果 CURdOCXItmCusBtnParam 沒有資料，從 CURdOCXSearchParams 查詢（舊表）
            if (list.Count == 0)
            {
                const string sql2 = @"
                    SELECT ParamName, ParamType, DefaultValue, ParamValue
                    FROM CURdOCXSearchParams WITH (NOLOCK)
                    WHERE ItemId = @ItemId AND ButtonName = @ButtonName
                    ORDER BY ParamSN";

                await using var cmd2 = new SqlCommand(sql2, conn);
                cmd2.Parameters.AddWithValue("@ItemId", itemId);
                cmd2.Parameters.AddWithValue("@ButtonName", buttonName);

                await using var reader2 = await cmd2.ExecuteReaderAsync();
                while (await reader2.ReadAsync())
                {
                    list.Add(new ParamDefinition
                    {
                        ParamName = reader2["ParamName"]?.ToString() ?? "",
                        ParamType = reader2["ParamType"] as int? ?? 0,
                        ParamFieldName = null,
                        DefaultValue = reader2["DefaultValue"]?.ToString(),
                        ParamValue = reader2["ParamValue"]?.ToString()
                    });
                }
            }

            return list;
        }

        // ===== DTOs =====
        public class DynamicSpRequest
        {
            public string ItemId { get; set; } = "";
            public string ButtonName { get; set; } = "";
            public Dictionary<string, object?>? Args { get; set; }
        }

        private class ParamDefinition
        {
            public string ParamName { get; set; } = "";
            public int ParamType { get; set; }
            public string? ParamFieldName { get; set; }
            public string? DefaultValue { get; set; }
            public string? ParamValue { get; set; }
        }
    }
}
