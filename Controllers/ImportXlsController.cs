using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.RegularExpressions;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class ImportXlsController : ControllerBase
{
    private readonly string _connStr;

    public ImportXlsController(IConfiguration config)
    {
        _connStr = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }
    [HttpPost("PriceTable")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ImportPriceTableAsync([FromForm] ImportPriceTableRequest request)
    {
        var file = request.File;
        var itemId = request.ItemId;

        if (file == null || file.Length == 0)
            return BadRequest(new { ok = false, error = "Please select an Excel file." });

        var safeItemId = string.IsNullOrWhiteSpace(itemId) ? "SPO00040" : itemId.Trim();

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        var dictTableName = await LoadDictTableNameAsync(conn, safeItemId);
        if (string.IsNullOrWhiteSpace(dictTableName))
            return BadRequest(new { ok = false, error = "Missing dictionary setup." });

        var importDictTable = $"{dictTableName}_4IMP";
        var realTableName = await LoadRealTableNameAsync(conn, importDictTable);
        if (string.IsNullOrWhiteSpace(realTableName))
            return BadRequest(new { ok = false, error = "Missing import dictionary (_4IMP)." });

        var fields = await LoadImportFieldsAsync(conn, importDictTable);
        if (fields.Count == 0)
            return BadRequest(new { ok = false, error = "Import field setup is empty." });

        using var stream = file.OpenReadStream();
        using var workbook = new XLWorkbook(stream);
        var sheet = workbook.Worksheets.FirstOrDefault();
        if (sheet == null)
            return BadRequest(new { ok = false, error = "Excel has no worksheet." });

        var headerRow = sheet.FirstRowUsed();
        if (headerRow == null)
            return BadRequest(new { ok = false, error = "Excel has no header row." });

        var headerCells = headerRow.CellsUsed();
        var columnMap = BuildColumnMap(headerCells, fields);
        if (columnMap.Count == 0)
            return BadRequest(new { ok = false, error = "No matching import columns." });

        var lastRowNumber = sheet.LastRowUsed()?.RowNumber() ?? headerRow.RowNumber();
        if (lastRowNumber <= headerRow.RowNumber())
            return BadRequest(new { ok = false, error = "No data rows to import." });

        var insertColumns = columnMap.Select(m => m.FieldName).ToList();
        var colList = string.Join(", ", insertColumns.Select(QuoteIdentifier));
        var paramList = string.Join(", ", insertColumns.Select((_, i) => $"@p{i}"));
        var insertSql = $"INSERT INTO {QuoteIdentifier(realTableName)} ({colList}) VALUES ({paramList})";

        var inserted = 0;
        await using var tx = await conn.BeginTransactionAsync();
        try
        {
            for (var r = headerRow.RowNumber() + 1; r <= lastRowNumber; r++)
            {
                var row = sheet.Row(r);
                if (row == null) continue;

                var values = new object?[columnMap.Count];
                var hasValue = false;

                for (var i = 0; i < columnMap.Count; i++)
                {
                    var cell = row.Cell(columnMap[i].ColumnNumber);
                    var value = ConvertCellValue(cell);
                    values[i] = value ?? DBNull.Value;
                    if (value != null && value != DBNull.Value && value.ToString() != string.Empty)
                        hasValue = true;
                }

                if (!hasValue) continue;

                await using var cmd = new SqlCommand(insertSql, conn, (SqlTransaction)tx);
                for (var i = 0; i < values.Length; i++)
                    cmd.Parameters.AddWithValue($"@p{i}", values[i] ?? DBNull.Value);

                await cmd.ExecuteNonQueryAsync();
                inserted++;
            }

            await tx.CommitAsync();
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return BadRequest(new { ok = false, error = ex.Message });
        }

        return Ok(new { ok = true, inserted });
    }

    [HttpGet("PriceTable/Template")]
    public async Task<IActionResult> DownloadTemplateAsync([FromQuery] string? itemId)
    {
        var safeItemId = string.IsNullOrWhiteSpace(itemId) ? "SPO00040" : itemId.Trim();

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        var dictTableName = await LoadDictTableNameAsync(conn, safeItemId);
        if (string.IsNullOrWhiteSpace(dictTableName))
            return BadRequest("Missing dictionary setup.");

        var importDictTable = $"{dictTableName}_4IMP";
        var fields = await LoadImportFieldsAsync(conn, importDictTable);
        if (fields.Count == 0)
            return BadRequest("Import field setup is empty.");

        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("ImportTemplate");
        for (var i = 0; i < fields.Count; i++)
        {
            var header = string.IsNullOrWhiteSpace(fields[i].DisplayLabel) ? fields[i].FieldName : fields[i].DisplayLabel;
            sheet.Cell(1, i + 1).Value = header;
        }
        sheet.Row(1).Style.Font.Bold = true;

        await using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        ms.Position = 0;

        var fileName = $"{safeItemId}_ImportTemplate.xlsx";
        return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
    public sealed class ImportPriceTableRequest
    {
        public IFormFile? File { get; set; }
        public string? ItemId { get; set; }
    }

    private sealed record ImportField(string FieldName, string DisplayLabel);
    private sealed record ColumnMap(int ColumnNumber, string FieldName);

    private static List<ColumnMap> BuildColumnMap(IEnumerable<IXLCell> headerCells, List<ImportField> fields)
    {
        var fieldByName = fields.ToDictionary(f => f.FieldName.Trim(), f => f.FieldName, StringComparer.OrdinalIgnoreCase);
        var fieldByLabel = fields
            .Where(f => !string.IsNullOrWhiteSpace(f.DisplayLabel))
            .ToDictionary(f => f.DisplayLabel.Trim(), f => f.FieldName, StringComparer.OrdinalIgnoreCase);

        var map = new List<ColumnMap>();
        foreach (var cell in headerCells)
        {
            var header = cell.GetString().Trim();
            if (string.IsNullOrWhiteSpace(header)) continue;

            if (!fieldByName.TryGetValue(header, out var fieldName) &&
                !fieldByLabel.TryGetValue(header, out fieldName))
            {
                continue;
            }

            map.Add(new ColumnMap(cell.Address.ColumnNumber, fieldName));
        }

        return map;
    }

    private static object? ConvertCellValue(IXLCell cell)
    {
        if (cell == null || cell.IsEmpty()) return null;

        return cell.DataType switch
        {
            XLDataType.Number => cell.GetValue<decimal>(),
            XLDataType.DateTime => cell.GetDateTime(),
            XLDataType.Boolean => cell.GetBoolean(),
            _ => cell.GetString()
        };
    }

    private static string QuoteIdentifier(string name)
    {
        var n = (name ?? string.Empty).Trim();
        var parts = n.Split('.', StringSplitOptions.RemoveEmptyEntries);
        foreach (var p in parts)
        {
            if (!Regex.IsMatch(p, "^[A-Za-z_][A-Za-z0-9_]*$"))
                throw new InvalidOperationException("Invalid identifier.");
        }
        return string.Join(".", parts.Select(p => $"[{p}]"));
    }

    private static async Task<string?> LoadDictTableNameAsync(SqlConnection conn, string itemId)
    {
        const string sql = @"
SELECT TOP 1 TableName
  FROM CURdOCXTableSetUp WITH (NOLOCK)
 WHERE ItemId = @itemId
 ORDER BY TableKind;";
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@itemId", itemId ?? string.Empty);
        var obj = await cmd.ExecuteScalarAsync();
        return obj == null || obj == DBNull.Value ? null : obj.ToString();
    }

    private static async Task<string?> LoadRealTableNameAsync(SqlConnection conn, string dictTableName)
    {
        const string sql = @"
SELECT TOP 1 ISNULL(NULLIF(RealTableName,''), TableName) AS ActualName
  FROM CURdTableName WITH (NOLOCK)
 WHERE TableName = @tbl;";
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@tbl", dictTableName ?? string.Empty);
        var obj = await cmd.ExecuteScalarAsync();
        return obj == null || obj == DBNull.Value ? null : obj.ToString();
    }

    private static async Task<List<ImportField>> LoadImportFieldsAsync(SqlConnection conn, string dictTableName)
    {
        var list = new List<ImportField>();
        const string sql = @"
SELECT FieldName, DisplayLabel
  FROM CURdTableField WITH (NOLOCK)
 WHERE TableName = @tbl
 ORDER BY SerialNum, FieldName;";
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@tbl", dictTableName ?? string.Empty);
        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            var fieldName = rd["FieldName"]?.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(fieldName)) continue;
            list.Add(new ImportField(fieldName, rd["DisplayLabel"]?.ToString() ?? string.Empty));
        }
        return list;
    }
}
