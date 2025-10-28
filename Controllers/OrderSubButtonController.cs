using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using System.Data;

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
}
