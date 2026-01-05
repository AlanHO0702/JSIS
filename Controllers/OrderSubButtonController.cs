using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

[ApiController]
[Route("api/[controller]")]
public class OrderSubButtonController : ControllerBase
{
    private readonly string _cs;
    public OrderSubButtonController(IConfiguration cfg, PcbErpContext db)
    {
        _cs = cfg.GetConnectionString("Default")
              ?? db.Database.GetDbConnection().ConnectionString;
    }

    // ===============================
    // 🧩 DTO: 出貨地址更新請求物件
    // ===============================
    public class UpdateOutAddrRequest
    {
        public string paperId { get; set; } = "";
        public string paperNum { get; set; } = "";
        public string addr { get; set; } = "";
        public string title { get; set; } = "";
    }

    public class OrderTailCancelRequest
    {
        public string paperId { get; set; } = "";
        public string paperNum { get; set; } = "";
        public string mode { get; set; } = "";
        public int? item { get; set; }
        public int reCompute { get; set; }
        public string notes { get; set; } = "";
        public bool plusCancel { get; set; }
    }

    // ===============================
    // 🧩 取得出貨地址清單
    // ===============================
    [HttpGet("GetOutAddrOptions")]
    public async Task<IActionResult> GetOutAddrOptions(string paperId, string paperNum)
    {
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync();

        // Step 1️⃣ 取客戶代號
        string sqlCus = $"exec SPOdGetOutAddrCus '{paperId}','{paperNum}'";
        string customerId = "";
        await using (var cmdCus = new SqlCommand(sqlCus, conn))
        await using (var reader = await cmdCus.ExecuteReaderAsync())
        {
            if (await reader.ReadAsync())
                customerId = reader["CustomerId"]?.ToString() ?? "";
        }

        if (string.IsNullOrEmpty(customerId))
            return Ok(new { ok = false, error = "找不到客戶編號" });

        // Step 2️⃣ 判斷顯示欄位名稱
        string fieldA = "OutAddr", fieldB = "ShipTo";
        if (paperId == "SPOdOrderMain" || paperId == "SPOdOrderChangeMain")
        {
            fieldA = "TransPlace";
            fieldB = "PkgTitle";
        }

        string labelSql = @"
            select Col1=t1.DisplayLabel, Col2=t2.DisplayLabel
            from CURdTableField t1, CURdTableField t2
            where t1.TableName=t2.TableName
              and t1.TableName=@paperId
              and t1.FieldName=@fieldA
              and t2.FieldName=@fieldB";
        string label = "";
        await using (var cmdLabel = new SqlCommand(labelSql, conn))
        {
            cmdLabel.Parameters.AddWithValue("@paperId", paperId);
            cmdLabel.Parameters.AddWithValue("@fieldA", fieldA);
            cmdLabel.Parameters.AddWithValue("@fieldB", fieldB);
            await using (var reader = await cmdLabel.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                    label = $"{reader["Col1"]}/{reader["Col2"]}";
            }
        }

        // Step 3️⃣ 查出貨地址清單（依 Delphi 原版）
        string addrSql = @"
            select OutAddress = Ltrim(OutAddress), OutTitle
            from AJNdCompanyOutAddr (nolock)
            where CompanyId = @CompanyId
            order by OutAddress";

        var list = new List<object>();
        await using (var cmdAddr = new SqlCommand(addrSql, conn))
        {
            cmdAddr.Parameters.AddWithValue("@CompanyId", customerId);
            await using (var reader = await cmdAddr.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    list.Add(new
                    {
                        Addr = reader["OutAddress"]?.ToString(),
                        Title = reader["OutTitle"]?.ToString()
                    });
                }
            }
        }

        return Ok(new { ok = true, label, list });
    }

    // ===============================
    // 🧩 更新出貨地址（主要按鈕邏輯）
    // ===============================
    [HttpPost("UpdateOutAddr")]
    public async Task<IActionResult> UpdateOutAddr([FromBody] UpdateOutAddrRequest body)
    {
        try
        {
            string paperId = body.paperId;
            string paperNum = body.paperNum;
            string addr = body.addr;
            string title = body.title;

            // ✅ 自動將 Sub 結尾轉成 Main（例如 SPOdOrderSub → SPOdOrderMain）
            if (paperId.EndsWith("Sub", StringComparison.OrdinalIgnoreCase))
                paperId = paperId.Replace("Sub", "Main");

            // ✅ 對應不同主檔的更新語法
            string sql = paperId switch
            {
                "SPOdOrderMain" =>
                    $"update SPOdOrderMain set TransPlace=N'{addr}', PkgTitle=N'{title}' where PaperNum='{paperNum}'",

                "SPOdOutMain" =>
                    $"update SPOdOutMain set OutAddr=N'{addr}', ShipTo=N'{title}' where PaperNum='{paperNum}'",

                "SPOdMPSOutMain" =>
                    $"update SPOdMPSOutMain set OutAddr=N'{addr}', ShipTo=N'{title}' where PaperNum='{paperNum}'",

                "SPOdOrderChangeMain" =>
                    $"update SPOdOrderChangeMain set TransPlace=N'{addr}', PkgTitle=N'{title}' where PaperNum='{paperNum}'",

                _ => ""
            };

            if (string.IsNullOrEmpty(sql))
                return Ok(new { ok = false, error = $"未支援的表單類型：{paperId}" });

            await using var conn = new SqlConnection(_cs);
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(sql, conn);
            var rows = await cmd.ExecuteNonQueryAsync();

            return Ok(new { ok = true, rows });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.GetBaseException().Message });
        }
    }

    [HttpPost("OrderTailCancel")]
    public async Task<IActionResult> OrderTailCancel([FromBody] OrderTailCancelRequest body)
    {
        try
        {
            var paperNum = (body.paperNum ?? string.Empty).Trim();
            var paperId = (body.paperId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(paperNum))
                return Ok(new { ok = false, error = "沒有單號" });
            if (string.IsNullOrWhiteSpace(paperId))
                return Ok(new { ok = false, error = "沒有單別" });

            if (paperId.EndsWith("Sub", StringComparison.OrdinalIgnoreCase))
                paperId = paperId[..^3] + "Main";

            var mode = (body.mode ?? string.Empty).Trim().ToLowerInvariant();
            var isAll = mode != "single";
            var item = isAll ? 1 : (body.item ?? 0);
            if (!isAll && item <= 0)
                return Ok(new { ok = false, error = "項次必須輸入" });

            var reType = isAll ? 1 : 2;
            var reCompute = body.reCompute != 0 ? 1 : 0;
            var notes = body.notes ?? string.Empty;

            await using var conn = new SqlConnection(_cs);
            await conn.OpenAsync();
            await using var tx = await conn.BeginTransactionAsync();

            var userId = await LoadUserIdAsync(conn, (SqlTransaction)tx);
            if (string.IsNullOrWhiteSpace(userId)) userId = "admin";

            if (!isAll)
            {
                var subTable = ReplaceMainToSub(paperId);
                var safeTable = QuoteIdentifier(subTable);
                if (string.IsNullOrWhiteSpace(safeTable))
                    return Ok(new { ok = false, error = "無效的單別" });

                var existsSql = $"select count(1) from {safeTable} with (nolock) where PaperNum=@paperNum and Item=@item";
                await using (var cmdExists = new SqlCommand(existsSql, conn, (SqlTransaction)tx))
                {
                    cmdExists.Parameters.AddWithValue("@paperNum", paperNum);
                    cmdExists.Parameters.AddWithValue("@item", item);
                    var countObj = await cmdExists.ExecuteScalarAsync();
                    var count = countObj == null || countObj == DBNull.Value ? 0 : Convert.ToInt32(countObj);
                    if (count < 1)
                        return Ok(new { ok = false, error = "項次不存在" });
                }
            }

            await using (var cmd = new SqlCommand("SPOdOCXOrderHandClose", conn, (SqlTransaction)tx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@PaperId", paperId);
                cmd.Parameters.AddWithValue("@PaperNum", paperNum);
                cmd.Parameters.AddWithValue("@Item", item);
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@ReCompute", reCompute);
                cmd.Parameters.AddWithValue("@ReType", reType);
                cmd.Parameters.AddWithValue("@Notes", notes);
                await cmd.ExecuteNonQueryAsync();
            }

            var plusDone = false;
            if (body.plusCancel && paperId.Equals("MPHdOrderMain", StringComparison.OrdinalIgnoreCase))
            {
                await using var cmdPlus = new SqlCommand("MPHdOrderHandClosePlus", conn, (SqlTransaction)tx)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmdPlus.Parameters.AddWithValue("@PaperNum", paperNum);
                cmdPlus.Parameters.AddWithValue("@Item", item);
                cmdPlus.Parameters.AddWithValue("@UserID", userId);
                cmdPlus.Parameters.AddWithValue("@ReCompute", reCompute);
                cmdPlus.Parameters.AddWithValue("@ReType", reType);
                cmdPlus.Parameters.AddWithValue("@Notes", notes);
                await cmdPlus.ExecuteNonQueryAsync();
                plusDone = true;
            }

            await tx.CommitAsync();
            return Ok(new { ok = true, plusDone });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.GetBaseException().Message });
        }
    }

    private static string ReplaceMainToSub(string paperId)
    {
        if (paperId.EndsWith("Main", StringComparison.OrdinalIgnoreCase))
            return paperId[..^4] + "Sub";
        return paperId.Replace("Main", "Sub", StringComparison.OrdinalIgnoreCase);
    }

    private static string QuoteIdentifier(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return string.Empty;
        var parts = name.Split('.', StringSplitOptions.RemoveEmptyEntries);
        foreach (var p in parts)
        {
            if (!Regex.IsMatch(p, "^[A-Za-z_][A-Za-z0-9_]*$"))
                return string.Empty;
        }
        return string.Join(".", parts.Select(p => $"[{p}]"));
    }

    private async Task<string> LoadUserIdAsync(SqlConnection conn, SqlTransaction? tx = null)
    {
        var userId = string.Empty;
        var jwtHeader = Request?.Headers["X-JWTID"].ToString();
        if (!string.IsNullOrWhiteSpace(jwtHeader) && Guid.TryParse(jwtHeader, out var jwtId))
        {
            await using var cmd = new SqlCommand(
                "SELECT UserId FROM CURdUserOnline WITH (NOLOCK) WHERE JwtId = @jwtId", conn, tx);
            cmd.Parameters.AddWithValue("@jwtId", jwtId);
            userId = (await cmd.ExecuteScalarAsync())?.ToString() ?? string.Empty;
        }

        if (string.IsNullOrWhiteSpace(userId))
            userId = User?.Identity?.Name ?? string.Empty;

        return userId.Trim();
    }
}
