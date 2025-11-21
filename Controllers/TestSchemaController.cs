using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

[Route("api/[controller]")]
[ApiController]
public class TestSchemaController : ControllerBase
{
    private readonly IConfiguration _config;

    public TestSchemaController(IConfiguration config)
    {
        _config = config;
    }

    [HttpGet("columns/{tableName}")]
    public async Task<ActionResult> GetColumns(string tableName)
    {
        var connStr = _config.GetConnectionString("DefaultConnection");
        var columns = new List<object>();

        using var conn = new SqlConnection(connStr);
        await conn.OpenAsync();

        var sql = @"
            SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_NAME = @TableName
            ORDER BY ORDINAL_POSITION";

        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@TableName", tableName);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            columns.Add(new
            {
                ColumnName = reader.GetString(0),
                DataType = reader.GetString(1),
                MaxLength = reader.IsDBNull(2) ? (int?)null : reader.GetInt32(2),
                IsNullable = reader.GetString(3)
            });
        }

        return Ok(columns);
    }

    [HttpGet("sample/{tableName}")]
    public async Task<ActionResult> GetSample(string tableName)
    {
        var connStr = _config.GetConnectionString("DefaultConnection");
        var result = new List<Dictionary<string, object>>();

        using var conn = new SqlConnection(connStr);
        await conn.OpenAsync();

        var sql = $"SELECT TOP 1 * FROM {tableName}";
        using var cmd = new SqlCommand(sql, conn);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var row = new Dictionary<string, object>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            }
            result.Add(row);
        }

        return Ok(result);
    }
}
