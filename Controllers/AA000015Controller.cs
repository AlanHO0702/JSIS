using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace PcbErpApi.Controllers
{
    [ApiController]
    [Route("api/AA000015")]
    public class AA000015Controller : ControllerBase
    {
        private readonly IConfiguration _config;

        public AA000015Controller(IConfiguration config)
        {
            _config = config;
        }

        public class CloseRequest
        {
            public string PaperNum { get; set; } = "";
            public string ItemId { get; set; } = "";
            public string UserId { get; set; } = "";
            public string UseId { get; set; } = "";
            public string PaperId { get; set; } = "";
            public int OpKind { get; set; } = 1;
        }

        /// <summary>
        /// 關閉作業 (對應 Delphi HsaInsJourDLL.pas btnGetParamsClick → DLLSetPowerType)
        /// 檢查項目設定、使用者權限、單據設定
        /// </summary>
        [HttpPost("Close")]
        public async Task<IActionResult> Close([FromBody] CloseRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.ItemId))
                return BadRequest(new { ok = false, message = "ItemId 為必填" });

            await using var conn = await OpenConnectionAsync();

            try
            {
                // 1. 查詢項目設定 (CURdSysItems)
                int paperType = 0, powerType = 0, functionType = 0;

                var sqlItem = "SELECT PaperType, PowerType, FunctionType FROM CURdSysItems(NOLOCK) WHERE ItemId = @ItemId";
                await using (var cmd = new SqlCommand(sqlItem, conn))
                {
                    cmd.Parameters.AddWithValue("@ItemId", req.ItemId);
                    await using var reader = await cmd.ExecuteReaderAsync();
                    if (!await reader.ReadAsync())
                    {
                        if (req.OpKind != -1)
                            return Ok(new { ok = false, message = $"程式 {req.ItemId} 未設定" });
                    }
                    else
                    {
                        paperType = reader.GetInt32(reader.GetOrdinal("PaperType"));
                        powerType = reader.GetInt32(reader.GetOrdinal("PowerType"));
                        functionType = reader.GetInt32(reader.GetOrdinal("FunctionType"));
                    }
                }

                // 2. 查詢使用者權限 (CURdGetUserSysItems)
                var userPower = await LoadUserPowerAsync(conn, req.ItemId, req.UserId, req.UseId);
                if (userPower == null)
                {
                    if (req.OpKind != -1)
                        return Ok(new { ok = false, message = $"使用者 {req.UserId} 對程式 {req.ItemId} 無權限" });
                    userPower = new Dictionary<string, int>();
                }

                // 3. 查詢單據設定 (CURdPaperInfo) - 僅 OpKind=1 時
                var paperPower = new Dictionary<string, int>();

                if (req.OpKind == 1 && !string.IsNullOrWhiteSpace(req.PaperId))
                {
                    var sqlPaper = "SELECT RunFlow, SelectType, LockPaperDate, LockUserEdit, MustNotes FROM CURdPaperInfo(NOLOCK) WHERE PaperId = @PaperId";
                    await using var cmd = new SqlCommand(sqlPaper, conn);
                    cmd.Parameters.AddWithValue("@PaperId", req.PaperId);
                    await using var reader = await cmd.ExecuteReaderAsync();
                    if (!await reader.ReadAsync())
                    {
                        return Ok(new { ok = false, message = $"單據 {req.PaperId} 未設定 PaperInfo" });
                    }
                    else
                    {
                        paperPower["RunFlow"] = reader.GetInt32(reader.GetOrdinal("RunFlow"));
                        paperPower["SelectType"] = reader.GetInt32(reader.GetOrdinal("SelectType"));
                        paperPower["LockPaperDate"] = reader.GetInt32(reader.GetOrdinal("LockPaperDate"));
                        paperPower["LockUserEdit"] = reader.GetInt32(reader.GetOrdinal("LockUserEdit"));
                        paperPower["MustNotes"] = reader.GetInt32(reader.GetOrdinal("MustNotes"));
                    }
                }

                return Ok(new
                {
                    ok = true,
                    message = "關閉作業完成",
                    paperType,
                    powerType,
                    functionType,
                    userPower,
                    paperPower
                });
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new { ok = false, message = ex.Message });
            }
        }

        // ==================== 匯入功能 API ====================

        public class CheckPowerRequest
        {
            public string ItemId { get; set; } = "";
            public string UserId { get; set; } = "";
            public string UseId { get; set; } = "";
        }

        /// <summary>
        /// 檢查使用者是否有匯入權限 (bF12)
        /// </summary>
        [HttpPost("CheckImportPower")]
        public async Task<IActionResult> CheckImportPower([FromBody] CheckPowerRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.ItemId))
                return BadRequest(new { ok = false, message = "ItemId 為必填" });

            await using var conn = await OpenConnectionAsync();

            try
            {
                var userPower = await LoadUserPowerAsync(conn, req.ItemId, req.UserId, req.UseId);
                if (userPower == null)
                    return Ok(new { ok = false, message = $"使用者 {req.UserId} 對程式 {req.ItemId} 無權限" });

                if (userPower.GetValueOrDefault("bF12") != 1)
                    return Ok(new { ok = false, message = "您沒有匯入的使用權限 (bF12)" });

                return Ok(new { ok = true });
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new { ok = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 解析 Excel 檔案，依據匯入格式(_4IMP)比對欄位，回傳各 sheet 的比對結果與預覽資料。
        /// 若 itemId 有值則進行格式比對，欄位不一致時回傳錯誤。
        /// </summary>
        [HttpPost("ParseExcel")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ParseExcel(IFormFile file, [FromForm] string? itemId)
        {
            var (workbook, error) = await ReadWorkbookFromFileAsync(file);
            if (workbook == null)
                return BadRequest(new { ok = false, error });

            // 載入匯入格式欄位 (若有設定)
            List<ImportField>? importFields = null;
            if (!string.IsNullOrWhiteSpace(itemId))
            {
                await using var conn = await OpenConnectionAsync();
                var dictTableName = await LoadDictTableNameAsync(conn, itemId.Trim());
                if (!string.IsNullOrWhiteSpace(dictTableName))
                {
                    var importDictTable = $"{dictTableName}_4IMP";
                    importFields = await LoadImportFieldsAsync(conn, importDictTable);
                }
            }

            var formatter = new DataFormatter(CultureInfo.InvariantCulture);
            var sheets = new List<object>();

            for (var s = 0; s < workbook.NumberOfSheets; s++)
            {
                var sheet = workbook.GetSheetAt(s);

                // 找標題行
                var headerRow = GetFirstNonEmptyRow(sheet);
                if (headerRow == null)
                {
                    sheets.Add(new { name = sheet.SheetName, rows = new List<List<string>>(), matched = false, matchError = "無標題行" });
                    continue;
                }

                var headerCells = GetHeaderCells(headerRow);

                // 有匯入格式時進行欄位比對
                List<ColumnMap>? columnMap = null;
                List<string>? matchedLabels = null;
                string? matchError = null;

                if (importFields != null && importFields.Count > 0)
                {
                    columnMap = BuildColumnMap(headerCells, importFields);
                    if (columnMap.Count == 0)
                    {
                        var expected = string.Join(", ", importFields.Select(f =>
                            string.IsNullOrWhiteSpace(f.DisplayLabel) ? f.FieldName : f.DisplayLabel));
                        matchError = $"Excel 欄位與匯入格式不符。需要: {expected}";
                    }
                    else
                    {
                        // 用 DisplayLabel 當表頭 (有的話)
                        matchedLabels = columnMap.Select(cm =>
                        {
                            var field = importFields.FirstOrDefault(f =>
                                f.FieldName.Equals(cm.FieldName, StringComparison.OrdinalIgnoreCase));
                            return field != null && !string.IsNullOrWhiteSpace(field.DisplayLabel)
                                ? field.DisplayLabel : cm.FieldName;
                        }).ToList();
                    }
                }

                // 組預覽資料
                var rows = new List<List<string>>();
                var headerRowIndex = headerRow.RowNum;
                var maxRows = Math.Min(sheet.LastRowNum + 1, 200);

                if (columnMap != null && columnMap.Count > 0)
                {
                    // 有格式比對：只顯示匹配的欄位
                    rows.Add(matchedLabels!);
                    for (var r = headerRowIndex + 1; r < maxRows; r++)
                    {
                        var row = sheet.GetRow(r);
                        if (row == null) { rows.Add(columnMap.Select(_ => "").ToList()); continue; }
                        var cells = columnMap.Select(cm =>
                        {
                            var cell = row.GetCell(cm.ColumnIndex);
                            return cell == null ? "" : formatter.FormatCellValue(cell);
                        }).ToList();
                        rows.Add(cells);
                    }
                }
                else
                {
                    // 無格式比對：顯示全部欄位
                    for (var r = sheet.FirstRowNum; r < maxRows; r++)
                    {
                        var row = sheet.GetRow(r);
                        if (row == null) { rows.Add(new List<string>()); continue; }
                        var cells = new List<string>();
                        var lastCell = row.LastCellNum;
                        for (var c = 0; c < lastCell; c++)
                        {
                            var cell = row.GetCell(c);
                            cells.Add(cell == null ? "" : formatter.FormatCellValue(cell));
                        }
                        rows.Add(cells);
                    }
                }

                sheets.Add(new
                {
                    name = sheet.SheetName,
                    rows,
                    matched = columnMap != null && columnMap.Count > 0,
                    matchError = matchError ?? ""
                });
            }

            return Ok(new { ok = true, sheets });
        }

        /// <summary>
        /// 取得 DictTableName (供前端編修格式用)
        /// </summary>
        [HttpGet("GetDictTableName")]
        public async Task<IActionResult> GetDictTableName([FromQuery] string? itemId)
        {
            var safeItemId = string.IsNullOrWhiteSpace(itemId) ? "AA000015" : itemId.Trim();

            await using var conn = await OpenConnectionAsync();

            var dictTableName = await LoadDictTableNameAsync(conn, safeItemId);
            if (string.IsNullOrWhiteSpace(dictTableName))
                return Ok(new { ok = false, error = "尚未設定匯入格式 (CURdOCXTableSetUp)" });

            return Ok(new { ok = true, dictTableName });
        }

        public class ImportRequest
        {
            public IFormFile? File { get; set; }
            public int SheetIndex { get; set; } = 0;
            public string PaperDate { get; set; } = "";
            public string CompanyId { get; set; } = "";
            public int StdFormat { get; set; } = 1;
            public string ItemId { get; set; } = "";
            public string UserId { get; set; } = "";
            public string UseId { get; set; } = "";
        }

        /// <summary>
        /// 執行匯入
        /// </summary>
        [HttpPost("Import")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Import([FromForm] ImportRequest req)
        {
            if (req.File == null || req.File.Length == 0)
                return BadRequest(new { ok = false, error = "請選擇 Excel 檔案" });

            var safeItemId = string.IsNullOrWhiteSpace(req.ItemId) ? "AA000015" : req.ItemId.Trim();

            await using var conn = await OpenConnectionAsync();

            // 1. 權限檢查
            var userPower = await LoadUserPowerAsync(conn, safeItemId, req.UserId, req.UseId);
            if (userPower == null || userPower.GetValueOrDefault("bF12") != 1)
                return Ok(new { ok = false, error = "您沒有匯入權限 (bF12)" });

            // 2. 取得字典表名
            var dictTableName = await LoadDictTableNameAsync(conn, safeItemId);
            if (string.IsNullOrWhiteSpace(dictTableName))
                return BadRequest(new { ok = false, error = "缺少字典設定 (CURdOCXTableSetUp)" });

            var importDictTable = $"{dictTableName}_4IMP";
            var realTableName = await LoadRealTableNameAsync(conn, importDictTable);
            if (string.IsNullOrWhiteSpace(realTableName))
                return BadRequest(new { ok = false, error = $"缺少匯入字典 ({importDictTable})" });

            // 3. 取得匯入欄位
            var fields = await LoadImportFieldsAsync(conn, importDictTable);
            if (fields.Count == 0)
                return BadRequest(new { ok = false, error = "匯入欄位設定為空" });

            // 4. 讀取 Excel
            var (workbook, excelError) = await ReadWorkbookFromFileAsync(req.File);
            if (workbook == null)
                return BadRequest(new { ok = false, error = excelError });

            if (req.SheetIndex < 0 || req.SheetIndex >= workbook.NumberOfSheets)
                return BadRequest(new { ok = false, error = $"Sheet 索引 {req.SheetIndex} 超出範圍" });

            var sheet = workbook.GetSheetAt(req.SheetIndex);
            if (sheet == null)
                return BadRequest(new { ok = false, error = "Sheet 不存在" });

            // 5. 找標題行並建立欄位映射
            var headerRow = GetFirstNonEmptyRow(sheet);
            if (headerRow == null)
                return BadRequest(new { ok = false, error = "Excel 無標題行" });

            var headerCells = GetHeaderCells(headerRow);
            var columnMap = BuildColumnMap(headerCells, fields);
            if (columnMap.Count == 0)
                return BadRequest(new { ok = false, error = "無匹配的匯入欄位" });

            var headerRowIndex = headerRow.RowNum;
            var lastRowIndex = sheet.LastRowNum;
            if (lastRowIndex <= headerRowIndex)
                return BadRequest(new { ok = false, error = "無資料行可匯入" });

            // 6. 執行插入
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

        /// <summary>
        /// 匯出範例 Excel（僅含欄位標題，供使用者參考匯入格式）
        /// </summary>
        [HttpGet("ExportSample")]
        public async Task<IActionResult> ExportSample([FromQuery] string? itemId)
        {
            var safeItemId = string.IsNullOrWhiteSpace(itemId) ? "AA000015" : itemId.Trim();

            await using var conn = await OpenConnectionAsync();

            var dictTableName = await LoadDictTableNameAsync(conn, safeItemId);
            if (string.IsNullOrWhiteSpace(dictTableName))
                return BadRequest(new { ok = false, error = "尚未設定匯入格式 (CURdOCXTableSetUp)" });

            var importDictTable = $"{dictTableName}_4IMP";
            var fields = await LoadImportFieldsAsync(conn, importDictTable);
            if (fields.Count == 0)
                return BadRequest(new { ok = false, error = "匯入欄位設定為空" });

            // 建立 Excel，只寫標題列
            IWorkbook workbook = new XSSFWorkbook();
            var sheet = workbook.CreateSheet("匯入範例");
            var headerRow = sheet.CreateRow(0);

            // 設定標題樣式
            var headerStyle = workbook.CreateCellStyle();
            var font = workbook.CreateFont();
            font.IsBold = true;
            headerStyle.SetFont(font);
            headerStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.LightYellow.Index;
            headerStyle.FillPattern = FillPattern.SolidForeground;
            headerStyle.BorderBottom = BorderStyle.Thin;

            for (var i = 0; i < fields.Count; i++)
            {
                var cell = headerRow.CreateCell(i);
                // 優先使用 DisplayLabel，無則用 FieldName
                var label = string.IsNullOrWhiteSpace(fields[i].DisplayLabel)
                    ? fields[i].FieldName
                    : fields[i].DisplayLabel;
                cell.SetCellValue(label);
                cell.CellStyle = headerStyle;
                sheet.SetColumnWidth(i, 4000); // 預設欄寬
            }

            using var ms = new MemoryStream();
            workbook.Write(ms, leaveOpen: true);
            ms.Position = 0;

            var fileName = $"{safeItemId}_匯入範例.xlsx";
            return File(ms.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        // ==================== 共用方法 ====================

        /// <summary>
        /// 共用方法：建立並開啟 DB 連線
        /// </summary>
        private async Task<SqlConnection> OpenConnectionAsync()
        {
            var connStr = _config.GetConnectionString("DefaultConnection");
            var conn = new SqlConnection(connStr);
            await conn.OpenAsync();
            return conn;
        }

        /// <summary>
        /// 共用方法：驗證並讀取 IFormFile 為 IWorkbook。
        /// 回傳 (workbook, error)，若 error 不為 null 表示失敗。
        /// </summary>
        private static async Task<(IWorkbook? Workbook, string? Error)> ReadWorkbookFromFileAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return (null, "請選擇 Excel 檔案");

            var ext = Path.GetExtension(file.FileName ?? "").ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(ext) && ext != ".xls" && ext != ".xlsx")
                return (null, "僅支援 .xls 或 .xlsx 格式");

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            ms.Position = 0;

            try
            {
                return (OpenWorkbook(ms, ext), null);
            }
            catch (Exception ex)
            {
                return (null, $"無法讀取 Excel 檔案: {ex.Message}");
            }
        }

        private static IWorkbook OpenWorkbook(MemoryStream ms, string ext)
        {
            if (ext == ".xls")
                return new HSSFWorkbook(ms);
            if (ext == ".xlsx")
                return new XSSFWorkbook(ms);
            // 自動偵測
            try { return new XSSFWorkbook(ms); }
            catch { ms.Position = 0; return new HSSFWorkbook(ms); }
        }

        /// <summary>
        /// 共用方法：呼叫 CURdGetUserSysItems 取得使用者權限 Dictionary。
        /// 若查無資料回傳 null。
        /// </summary>
        private static async Task<Dictionary<string, int>?> LoadUserPowerAsync(
            SqlConnection conn, string? itemId, string? userId, string? useId)
        {
            await using var cmd = new SqlCommand("CURdGetUserSysItems", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@SystemId", "");
            cmd.Parameters.AddWithValue("@UserId", userId ?? "");
            cmd.Parameters.AddWithValue("@UserPassword", "");
            cmd.Parameters.AddWithValue("@UseId", useId ?? "");
            cmd.Parameters.AddWithValue("@OnlyItemId", 0);
            cmd.Parameters.AddWithValue("@SingleItemId", itemId ?? "");

            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync()) return null;

            var power = new Dictionary<string, int>();
            var fields = new[] { "bUpdate", "bUpdateMoney", "bAudit", "bAuditBack",
                                 "bScrap", "bViewMoney", "bPrint", "bUpdateNotes",
                                 "bExport", "bF9", "bF12" };
            foreach (var f in fields)
            {
                try { power[f] = reader.GetInt32(reader.GetOrdinal(f)); }
                catch { power[f] = 0; }
            }
            return power;
        }

        private sealed record ImportField(string FieldName, string DisplayLabel);
        private sealed record ColumnHeader(int ColumnIndex, string Header);
        private sealed record ColumnMap(int ColumnIndex, string FieldName);

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
                    continue;

                map.Add(new ColumnMap(cell.ColumnIndex, fieldName));
            }
            return map;
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
    }
}