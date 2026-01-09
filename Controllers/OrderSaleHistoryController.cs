using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

[ApiController]
[Route("api/[controller]")]
public class OrderSaleHistoryController : ControllerBase
{
    private readonly string _cs;

    public OrderSaleHistoryController(IConfiguration cfg)
    {
        _cs = cfg.GetConnectionString("DefaultConnection")
              ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    [HttpGet("Order")]
    public async Task<IActionResult> GetOrderHistory(string paperNum, string partNum)
    {
        if (string.IsNullOrWhiteSpace(paperNum) || string.IsNullOrWhiteSpace(partNum))
            return Ok(new { ok = false, error = "PaperNum/PartNum is required." });

        const string sql = @"
SELECT t1.*
  FROM SPOdV_OrderSaleHis t1 WITH (NOLOCK)
  JOIN SPOdV_OrderCusSub t2 WITH (NOLOCK)
    ON t2.PaperNum = @paperNum
   AND t1.CustomerId = t2.CustomerId
 WHERE t1.PartNum = @partNum
 ORDER BY t1.OrderDate DESC;";

        var rows = await QueryRowsAsync(sql, new SqlParameter("@paperNum", paperNum), new SqlParameter("@partNum", partNum));
        return Ok(new { ok = true, rows });
    }

    [HttpGet("Out")]
    public async Task<IActionResult> GetOutHistory(string paperNum, string partNum)
    {
        if (string.IsNullOrWhiteSpace(paperNum) || string.IsNullOrWhiteSpace(partNum))
            return Ok(new { ok = false, error = "PaperNum/PartNum is required." });

        const string sql = @"
SELECT t1.*
  FROM SPOdV_OutSaleHis t1 WITH (NOLOCK)
  JOIN SPOdV_OrderCusSub t2 WITH (NOLOCK)
    ON t2.PaperNum = @paperNum
   AND t1.CustomerId = t2.CustomerId
 WHERE t1.PartNum = @partNum
 ORDER BY t1.PaperDate DESC;";

        var rows = await QueryRowsAsync(sql, new SqlParameter("@paperNum", paperNum), new SqlParameter("@partNum", partNum));
        return Ok(new { ok = true, rows });
    }

    [HttpGet("Price")]
    public async Task<IActionResult> GetPriceInfo(string paperNum, int? item)
    {
        if (string.IsNullOrWhiteSpace(paperNum) || item is null)
            return Ok(new { ok = false, error = "PaperNum/Item is required." });

        const string sql = @"
SELECT *
  FROM SPOdV_OrderPriceInfo WITH (NOLOCK)
 WHERE PaperNum = @paperNum
   AND Item = @item;";

        var rows = await QueryRowsAsync(sql, new SqlParameter("@paperNum", paperNum), new SqlParameter("@item", item.Value));
        return Ok(new { ok = true, rows });
    }

    private async Task<List<Dictionary<string, object?>>> QueryRowsAsync(string sql, params SqlParameter[] parameters)
    {
        var list = new List<Dictionary<string, object?>>();
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        if (parameters?.Length > 0) cmd.Parameters.AddRange(parameters);
        await using var rd = await cmd.ExecuteReaderAsync();
        var cols = Enumerable.Range(0, rd.FieldCount).Select(rd.GetName).ToList();
        while (await rd.ReadAsync())
        {
            var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            foreach (var c in cols)
                row[c] = rd[c] == DBNull.Value ? null : rd[c];
            list.Add(row);
        }
        return list;
    }
}
