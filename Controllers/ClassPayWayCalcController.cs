using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;

[ApiController]
[Route("api/[controller]")]
public class ClassPayWayCalcController : ControllerBase
{
    private readonly string _cs;

    public ClassPayWayCalcController(IConfiguration cfg, PcbErpContext db)
    {
        _cs = cfg.GetConnectionString("DefaultConnection")
              ?? db.Database.GetDbConnection().ConnectionString;
    }

    [HttpGet("Init")]
    public async Task<IActionResult> Init(int payWayCode = 0)
    {
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync();

        string payWayName = string.Empty;
        if (payWayCode > 0)
        {
            const string sql = @"select PayWayName from AJNdClassPayWay(nolock) where PayWayCode=@PayWayCode";
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@PayWayCode", payWayCode);
            var obj = await cmd.ExecuteScalarAsync();
            payWayName = obj?.ToString() ?? string.Empty;
        }

        var customers = await LoadCustomers(conn, false);
        var allCustomers = await LoadCustomers(conn, true);
        return Ok(new
        {
            ok = true,
            payWayCode,
            payWayName,
            customers,
            allCustomers
        });
    }

    [HttpGet("PRDay")]
    public async Task<IActionResult> PRDay(string paperDate, string useId, string companyId, int isIn = 1)
    {
        if (string.IsNullOrWhiteSpace(paperDate) || string.IsNullOrWhiteSpace(useId) || string.IsNullOrWhiteSpace(companyId))
            return Ok(new { ok = false, error = "參數不足" });

        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync();

        const string sql = "exec SPOdCopyExpectPayDate @PaperDate, @UseId, @CompanyId, @IsIn";
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@PaperDate", paperDate);
        cmd.Parameters.AddWithValue("@UseId", useId);
        cmd.Parameters.AddWithValue("@CompanyId", companyId);
        cmd.Parameters.AddWithValue("@IsIn", isIn);
        await using var rd = await cmd.ExecuteReaderAsync();
        if (await rd.ReadAsync())
            return Ok(new { ok = true, prDay = rd["PRDay"]?.ToString() ?? "" });

        return Ok(new { ok = true, prDay = "" });
    }

    [HttpGet("ExpectDate")]
    public async Task<IActionResult> ExpectDate(string paperDate, string useId, string companyId, int payWayCode, int isIn = 1)
    {
        if (string.IsNullOrWhiteSpace(paperDate) || string.IsNullOrWhiteSpace(useId) || string.IsNullOrWhiteSpace(companyId))
            return Ok(new { ok = false, error = "參數不足" });
        if (payWayCode <= 0)
            return Ok(new { ok = false, error = "付款方式代碼不可空白" });

        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync();

        var sql = isIn == 0
            ? "exec MPHdInvoiceExpectDate @UseId, @PaperDate, @PayWayCode, @CompanyId"
            : "exec SPOdInvoiceExpectDate @UseId, @PaperDate, @PayWayCode, @CompanyId";
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@UseId", useId);
        cmd.Parameters.AddWithValue("@PaperDate", paperDate);
        cmd.Parameters.AddWithValue("@PayWayCode", payWayCode);
        cmd.Parameters.AddWithValue("@CompanyId", companyId);
        await using var rd = await cmd.ExecuteReaderAsync();
        if (await rd.ReadAsync())
            return Ok(new { ok = true, expectDate = rd["ExpectDate"]?.ToString() ?? "" });

        return Ok(new { ok = false, error = "取得付款日期失敗" });
    }

    private static async Task<List<object>> LoadCustomers(SqlConnection conn, bool all)
    {
        var list = new List<object>();
        var sql = all
            ? "select CompanyId, ShortName from AJNdV_AllCustom(nolock) order by CompanyId"
            : "select CompanyId, ShortName from AJNdCustomer(nolock) order by CompanyId";
        await using var cmd = new SqlCommand(sql, conn);
        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            list.Add(new
            {
                companyId = rd["CompanyId"]?.ToString() ?? "",
                shortName = rd["ShortName"]?.ToString() ?? ""
            });
        }
        return list;
    }
}

