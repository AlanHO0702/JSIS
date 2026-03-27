using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

[ApiController]
[Route("api/[controller]")]
public class SQUdSetNumEMOFieldController : ControllerBase
{
    private readonly string _connStr;

    public SQUdSetNumEMOFieldController(IConfiguration config)
    {
        _connStr = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    [HttpGet("TreeData")]
    public async Task<IActionResult> TreeData()
    {
        var list = new List<object>();
        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand("exec SQUdTableDictionary @PowerType", conn);
        cmd.Parameters.AddWithValue("@PowerType", 0);
        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            list.Add(new
            {
                itemId = (rd["ItemId"] ?? "").ToString()?.Trim(),
                itemName = (rd["ItemName"] ?? "").ToString()?.Trim(),
                levelNo = rd["LevelNo"] is DBNull ? 0 : Convert.ToInt32(rd["LevelNo"]),
                superId = (rd["SuperId"] ?? "").ToString()?.Trim(),
                serialNum = rd["SerialNum"] is DBNull ? 0 : Convert.ToInt32(rd["SerialNum"])
            });
        }
        return Ok(list);
    }

    public class UpdateRequest
    {
        public string NumId { get; set; } = "";
        public string EMOdField { get; set; } = "";
    }

    [HttpPost("Update")]
    public async Task<IActionResult> Update([FromBody] UpdateRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.NumId))
            return BadRequest(new { ok = false, error = "NumId 為必填" });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(
            "UPDATE SQUdSetNumSub SET EMOdField = @EMOdField WHERE NumId = @NumId", conn);
        cmd.Parameters.AddWithValue("@EMOdField", req.EMOdField ?? "");
        cmd.Parameters.AddWithValue("@NumId", req.NumId.Trim());
        var affected = await cmd.ExecuteNonQueryAsync();
        return Ok(new { ok = true, affected });
    }
}
