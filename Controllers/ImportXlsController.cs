using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Linq;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

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

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        ms.Position = 0;

        var fileName = file.FileName ?? string.Empty;
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        if (!string.IsNullOrWhiteSpace(ext) && ext != ".xls" && ext != ".xlsx")
            return BadRequest(new { ok = false, error = "Only .xls or .xlsx is supported." });

        IWorkbook? workbook = null;
        try
        {
            if (ext == ".xls")
            {
                workbook = new HSSFWorkbook(ms);
            }
            else if (ext == ".xlsx")
            {
                workbook = new XSSFWorkbook(ms);
            }
            else
            {
                try
                {
                    workbook = new XSSFWorkbook(ms);
                }
                catch
                {
                    ms.Position = 0;
                    workbook = new HSSFWorkbook(ms);
                }
            }
        }
        catch (Exception ex)
        {
            return BadRequest(new { ok = false, error = $"Invalid Excel file. {ex.Message}" });
        }

        var sheet = workbook.NumberOfSheets > 0 ? workbook.GetSheetAt(0) : null;
        if (sheet == null)
            return BadRequest(new { ok = false, error = "Excel has no worksheet." });

        var headerRow = GetFirstNonEmptyRow(sheet);
        if (headerRow == null)
            return BadRequest(new { ok = false, error = "Excel has no header row." });

        var headerCells = GetHeaderCells(headerRow);
        var columnMap = BuildColumnMap(headerCells, fields);
        if (columnMap.Count == 0)
            return BadRequest(new { ok = false, error = "No matching import columns." });

        var headerRowIndex = headerRow.RowNum;
        var lastRowIndex = sheet.LastRowNum;
        if (lastRowIndex <= headerRowIndex)
            return BadRequest(new { ok = false, error = "No data rows to import." });

        var insertColumns = columnMap.Select(m => m.FieldName).ToList();
        var colList = string.Join(", ", insertColumns.Select(QuoteIdentifier));
        var paramList = string.Join(", ", insertColumns.Select((_, i) => $"@p{i}"));
        var insertSql = $"INSERT INTO {QuoteIdentifier(realTableName)} ({colList}) VALUES ({paramList})";

        var inserted = 0;
        await using var tx = await conn.BeginTransactionAsync();
        try
        {
            for (var r = headerRowIndex + 1; r <= lastRowIndex; r++)
            {
                var row = sheet.GetRow(r);
                if (row == null) continue;

                var values = new object?[columnMap.Count];
                var hasValue = false;

                for (var i = 0; i < columnMap.Count; i++)
                {
                    var cell = row.GetCell(columnMap[i].ColumnIndex);
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
    public async Task<IActionResult> DownloadTemplateAsync([FromQuery] string? itemId, [FromQuery] string? format = null)
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

        var fmt = (format ?? string.Empty).Trim().ToLowerInvariant();
        if (!string.IsNullOrEmpty(fmt) && fmt != "xls" && fmt != "xlsx")
            return BadRequest("Only xls or xlsx is supported.");

        var useXls = fmt == "xls";
        IWorkbook workbook = useXls ? new HSSFWorkbook() : new XSSFWorkbook();
        var sheet = workbook.CreateSheet("ImportTemplate");
        var headerRow = sheet.CreateRow(0);
        var headerStyle = CreateHeaderStyle(workbook);
        for (var i = 0; i < fields.Count; i++)
        {
            var header = string.IsNullOrWhiteSpace(fields[i].DisplayLabel) ? fields[i].FieldName : fields[i].DisplayLabel;
            var cell = headerRow.CreateCell(i);
            cell.SetCellValue(header);
            if (headerStyle != null) cell.CellStyle = headerStyle;
        }

        await using var ms = new MemoryStream();
        workbook.Write(ms);
        ms.Position = 0;

        var fileExt = useXls ? "xls" : "xlsx";
        var contentType = useXls
            ? "application/vnd.ms-excel"
            : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        var fileName = $"{safeItemId}_ImportTemplate.{fileExt}";
        return File(ms.ToArray(), contentType, fileName);
    }
    public sealed class ImportPriceTableRequest
    {
        public IFormFile? File { get; set; }
        public string? ItemId { get; set; }
    }

    private sealed record ImportField(string FieldName, string DisplayLabel);
    private sealed record ColumnHeader(int ColumnIndex, string Header);
    private sealed record ColumnMap(int ColumnIndex, string FieldName);

    private static List<ColumnMap> BuildColumnMap(IEnumerable<ColumnHeader> headerCells, List<ImportField> fields)
    {
        var fieldByName = fields.ToDictionary(f => f.FieldName.Trim(), f => f.FieldName, StringComparer.OrdinalIgnoreCase);
        var fieldByLabel = fields
            .Where(f => !string.IsNullOrWhiteSpace(f.DisplayLabel))
            .ToDictionary(f => f.DisplayLabel.Trim(), f => f.FieldName, StringComparer.OrdinalIgnoreCase);

        var map = new List<ColumnMap>();
        foreach (var cell in headerCells)
        {
            var header = cell.Header.Trim();
            if (string.IsNullOrWhiteSpace(header)) continue;

            if (!fieldByName.TryGetValue(header, out var fieldName) &&
                !fieldByLabel.TryGetValue(header, out fieldName))
            {
                continue;
            }

            map.Add(new ColumnMap(cell.ColumnIndex, fieldName));
        }

        return map;
    }

    private static object? ConvertCellValue(ICell? cell)
    {
        if (cell == null) return null;
        var type = cell.CellType == CellType.Formula ? cell.CachedFormulaResultType : cell.CellType;

        return type switch
        {
            CellType.Numeric => DateUtil.IsCellDateFormatted(cell)
                ? cell.DateCellValue
                : Convert.ToDecimal(cell.NumericCellValue),
            CellType.Boolean => cell.BooleanCellValue,
            CellType.String => cell.StringCellValue,
            CellType.Blank => null,
            _ => cell.ToString()
        };
    }

    private static IRow? GetFirstNonEmptyRow(ISheet sheet)
    {
        for (var i = sheet.FirstRowNum; i <= sheet.LastRowNum; i++)
        {
            var row = sheet.GetRow(i);
            if (row == null) continue;
            if (RowHasValue(row)) return row;
        }
        return null;
    }

    private static bool RowHasValue(IRow row)
    {
        var start = row.FirstCellNum;
        var end = row.LastCellNum;
        if (start < 0 || end <= 0) return false;
        for (var c = start; c < end; c++)
        {
            var cell = row.GetCell(c);
            if (cell == null) continue;
            var type = cell.CellType == CellType.Formula ? cell.CachedFormulaResultType : cell.CellType;
            if (type == CellType.Blank) continue;
            if (type == CellType.String && string.IsNullOrWhiteSpace(cell.StringCellValue)) continue;
            return true;
        }
        return false;
    }

    private static List<ColumnHeader> GetHeaderCells(IRow row)
    {
        var list = new List<ColumnHeader>();
        var formatter = new DataFormatter(CultureInfo.InvariantCulture);
        var start = row.FirstCellNum;
        var end = row.LastCellNum;
        if (start < 0 || end <= 0) return list;
        for (var c = start; c < end; c++)
        {
            var cell = row.GetCell(c);
            if (cell == null) continue;
            var text = formatter.FormatCellValue(cell) ?? string.Empty;
            list.Add(new ColumnHeader(c, text));
        }
        return list;
    }

    private static ICellStyle? CreateHeaderStyle(IWorkbook workbook)
    {
        var font = workbook.CreateFont();
        font.IsBold = true;
        var style = workbook.CreateCellStyle();
        style.SetFont(font);
        return style;
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
