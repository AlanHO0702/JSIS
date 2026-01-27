using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

[Route("api/[controller]")]
[ApiController]
public class TestSchemaController : ControllerBase
{
    private readonly IConfiguration _config;

    public TestSchemaController(IConfiguration config)
    {
        _config = config;
    }

    [HttpGet("conninfo")]
    public ActionResult GetConnectionInfo()
    {
        var connStr = _config.GetConnectionString("DefaultConnection") ?? "";
        var result = new Dictionary<string, object?>
        {
            ["hasConnectionString"] = !string.IsNullOrWhiteSpace(connStr),
            ["rawConnectionString"] = "",
            ["dataSource"] = "",
            ["initialCatalog"] = "",
            ["userId"] = "",
            ["integratedSecurity"] = false,
            ["encrypt"] = "",
            ["trustServerCertificate"] = "",
        };

        if (string.IsNullOrWhiteSpace(connStr)) return Ok(result);

        try
        {
            var csb = new SqlConnectionStringBuilder(connStr);
            csb.Password = "";
            csb.PersistSecurityInfo = false;
            result["rawConnectionString"] = csb.ConnectionString;
            result["dataSource"] = csb.DataSource;
            result["initialCatalog"] = csb.InitialCatalog;
            result["userId"] = csb.UserID;
            result["integratedSecurity"] = csb.IntegratedSecurity;
            result["encrypt"] = csb.Encrypt.ToString();
            result["trustServerCertificate"] = csb.TrustServerCertificate.ToString();
            return Ok(result);
        }
        catch (Exception ex)
        {
            result["error"] = ex.Message;
            return Ok(result);
        }
    }

    [HttpGet("connping")]
    public async Task<ActionResult> PingConnection()
    {
        var connStr = _config.GetConnectionString("DefaultConnection") ?? "";
        if (string.IsNullOrWhiteSpace(connStr))
        {
            return Ok(new { ok = false, error = "Connection string 'DefaultConnection' not found." });
        }

        try
        {
            await using var conn = new SqlConnection(connStr);
            await conn.OpenAsync();

            const string sql = "SELECT @@SERVERNAME AS ServerName, DB_NAME() AS DatabaseName, SYSTEM_USER AS LoginName;";
            await using var cmd = new SqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return Ok(new
                {
                    ok = true,
                    serverName = reader["ServerName"]?.ToString(),
                    databaseName = reader["DatabaseName"]?.ToString(),
                    loginName = reader["LoginName"]?.ToString()
                });
            }

            return Ok(new { ok = false, error = "No data returned." });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
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
        var result = new List<Dictionary<string, object?>>();

        using var conn = new SqlConnection(connStr);
        await conn.OpenAsync();

        var sql = $"SELECT TOP 1 * FROM {tableName}";
        using var cmd = new SqlCommand(sql, conn);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var row = new Dictionary<string, object?>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            }
            result.Add(row);
        }

        return Ok(result);
    }
}
