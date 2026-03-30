using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

/// <summary>
/// LA201 樣品報價單 (SQUdQuotaMain) 專用 Controller
/// 對應 Delphi: QuotaPaper.pas / PartNumCondData.pas
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SQUdQuotaMainController : ControllerBase
{
    private readonly string _connStr;

    public SQUdQuotaMainController(IConfiguration config)
    {
        _connStr = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    #region ===== ChkData (確認/審核前檢查) =====

    /// <summary>
    /// 確認/審核前呼叫 SQUdQuotaChkData 檢查資料
    /// Delphi: QuotaPaper.pas ChkData()
    /// </summary>
    [HttpPost("ChkData")]
    public async Task<IActionResult> ChkData([FromBody] ChkDataRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.PaperNum))
            return BadRequest(new { ok = false, error = "PaperNum 為必填" });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand("exec SQUdQuotaChkData @PaperNum", conn);
        cmd.Parameters.AddWithValue("@PaperNum", req.PaperNum.Trim());

        await using var rd = await cmd.ExecuteReaderAsync();
        if (await rd.ReadAsync())
        {
            var msg = rd["ReturnMsg"]?.ToString()?.Trim() ?? "";
            if (!string.IsNullOrEmpty(msg))
                return Ok(new { ok = true, needConfirm = true, message = msg });
        }

        return Ok(new { ok = true, needConfirm = false, message = "" });
    }

    public class ChkDataRequest
    {
        public string PaperNum { get; set; } = "";
    }

    #endregion

    #region ===== SetupAddData (設定規格明細) =====

    /// <summary>
    /// 設定規格明細資料 (ComputeId 變更時或搜尋料號後觸發)
    /// Delphi: QuotaPaper.pas SetupAddData()
    /// 呼叫 SQUdGenSetNumTable -> 重新載入 Detail1
    /// </summary>
    [HttpPost("SetupAddData")]
    public async Task<IActionResult> SetupAddData([FromBody] SetupAddDataRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.PaperNum))
            return BadRequest(new { ok = false, error = "PaperNum 為必填" });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand($"exec SQUdGenSetNumTable @PaperNum", conn);
        cmd.Parameters.AddWithValue("@PaperNum", req.PaperNum.Trim());
        await cmd.ExecuteNonQueryAsync();

        return Ok(new { ok = true });
    }

    public class SetupAddDataRequest
    {
        public string PaperNum { get; set; } = "";
    }

    #endregion

    #region ===== 搜尋料號 (btnLoadData) =====

    /// <summary>
    /// 初始化搜尋條件表 (搜尋料號對話框開啟前)
    /// Delphi: QuotaPaper.pas btnLoadDataClick -> SQUdGenSetNumCondTable
    /// </summary>
    [HttpPost("InitCondTable")]
    public async Task<IActionResult> InitCondTable([FromBody] InitCondTableRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.SpId))
            return BadRequest(new { ok = false, error = "SpId 為必填" });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand(
            "exec SQUdGenSetNumCondTable @SpId, @IsDelete, @SearchEx", conn);
        cmd.Parameters.AddWithValue("@SpId", req.SpId.Trim());
        cmd.Parameters.AddWithValue("@IsDelete", req.IsDelete);
        cmd.Parameters.AddWithValue("@SearchEx", req.SearchEx);
        await cmd.ExecuteNonQueryAsync();

        return Ok(new { ok = true });
    }

    public class InitCondTableRequest
    {
        public string SpId { get; set; } = "";
        public int IsDelete { get; set; } = 0;
        public int SearchEx { get; set; } = 0;
    }

    /// <summary>
    /// 載入搜尋條件資料 (SQUdSetNumCondData)
    /// Delphi: PartNumCondData.pas tblAddData
    /// </summary>
    [HttpGet("CondData")]
    public async Task<IActionResult> GetCondData([FromQuery] string spId)
    {
        if (string.IsNullOrWhiteSpace(spId))
            return BadRequest(new { ok = false, error = "spId 為必填" });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand(
            "SELECT * FROM SQUdSetNumCondData WHERE spid = @spid ORDER BY SearchCondSN", conn);
        cmd.Parameters.AddWithValue("@spid", spId.Trim());

        var rows = new List<Dictionary<string, object?>>();
        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            var row = new Dictionary<string, object?>();
            for (int i = 0; i < rd.FieldCount; i++)
                row[rd.GetName(i)] = rd.IsDBNull(i) ? null : rd.GetValue(i);
            rows.Add(row);
        }

        return Ok(new { ok = true, data = rows });
    }

    /// <summary>
    /// 更新搜尋條件明細值 (DtlNumId_B, DtlNumId_E)
    /// Delphi: PartNumCondData.pas tblAddData post
    /// </summary>
    [HttpPost("UpdateCondData")]
    public async Task<IActionResult> UpdateCondData([FromBody] UpdateCondDataRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.SpId) || string.IsNullOrWhiteSpace(req.NumId))
            return BadRequest(new { ok = false, error = "SpId 和 NumId 為必填" });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand(@"
            UPDATE SQUdSetNumCondData
            SET DtlNumId_B = @DtlNumId_B, DtlNumName_B = @DtlNumName_B,
                DtlNumId_E = @DtlNumId_E, DtlNumName_E = @DtlNumName_E
            WHERE SpId = @SpId AND NumId = @NumId", conn);
        cmd.Parameters.AddWithValue("@SpId", req.SpId.Trim());
        cmd.Parameters.AddWithValue("@NumId", req.NumId.Trim());
        cmd.Parameters.AddWithValue("@DtlNumId_B", (object?)req.DtlNumId_B?.Trim() ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@DtlNumName_B", (object?)req.DtlNumName_B?.Trim() ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@DtlNumId_E", (object?)req.DtlNumId_E?.Trim() ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@DtlNumName_E", (object?)req.DtlNumName_E?.Trim() ?? DBNull.Value);

        var affected = await cmd.ExecuteNonQueryAsync();
        return Ok(new { ok = true, affected });
    }

    public class UpdateCondDataRequest
    {
        public string SpId { get; set; } = "";
        public string NumId { get; set; } = "";
        public string? DtlNumId_B { get; set; }
        public string? DtlNumName_B { get; set; }
        public string? DtlNumId_E { get; set; }
        public string? DtlNumName_E { get; set; }
    }

    /// <summary>
    /// 取得搜尋條件的下拉選項 (SQUdGetCondItemList)
    /// Delphi: PartNumCondData.pas qryGetItemList
    /// </summary>
    [HttpGet("CondItemList")]
    public async Task<IActionResult> GetCondItemList(
        [FromQuery] string spId, [FromQuery] string? setClass, [FromQuery] string? numId)
    {
        if (string.IsNullOrWhiteSpace(spId))
            return BadRequest(new { ok = false, error = "spId 為必填" });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand("exec SQUdGetCondItemList @spid, @SetClass, @NumId", conn);
        cmd.Parameters.AddWithValue("@spid", spId.Trim());
        cmd.Parameters.AddWithValue("@SetClass", (object?)(setClass?.Trim()) ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@NumId", (object?)(numId?.Trim()) ?? DBNull.Value);

        var rows = new List<Dictionary<string, object?>>();
        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            var row = new Dictionary<string, object?>();
            for (int i = 0; i < rd.FieldCount; i++)
                row[rd.GetName(i)] = rd.IsDBNull(i) ? null : rd.GetValue(i);
            rows.Add(row);
        }

        return Ok(new { ok = true, data = rows });
    }

    /// <summary>
    /// 重置條件資料為預設值 (btnDelete in PartNumCondData)
    /// Delphi: PartNumCondData.pas btDeleteClick
    /// </summary>
    [HttpPost("ResetCondData")]
    public async Task<IActionResult> ResetCondData([FromBody] InitCondTableRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.SpId))
            return BadRequest(new { ok = false, error = "SpId 為必填" });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand(
            "exec SQUdGenSetNumCondTable @SpId, 1, @SearchEx", conn);
        cmd.Parameters.AddWithValue("@SpId", req.SpId.Trim());
        cmd.Parameters.AddWithValue("@SearchEx", req.SearchEx);
        await cmd.ExecuteNonQueryAsync();

        return Ok(new { ok = true });
    }

    /// <summary>
    /// 執行搜尋料號 (SQUdQuotaPartNumWhere) 回傳結果
    /// Delphi: PartNumCondData.pas OpenCondData -> SQUdQuotaPartNumWhere
    /// </summary>
    [HttpGet("PartNumSearch")]
    public async Task<IActionResult> PartNumSearch([FromQuery] string spId, [FromQuery] int searchEx = 0)
    {
        if (string.IsNullOrWhiteSpace(spId))
            return BadRequest(new { ok = false, error = "spId 為必填" });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand($"exec SQUdQuotaPartNumWhere @spId, @searchEx", conn);
        cmd.Parameters.AddWithValue("@spId", spId.Trim());
        cmd.Parameters.AddWithValue("@searchEx", searchEx);

        var rows = new List<Dictionary<string, object?>>();
        var columns = new List<string>();
        await using var rd = await cmd.ExecuteReaderAsync();
        for (int i = 0; i < rd.FieldCount; i++)
            columns.Add(rd.GetName(i));
        while (await rd.ReadAsync())
        {
            var row = new Dictionary<string, object?>();
            for (int i = 0; i < rd.FieldCount; i++)
                row[rd.GetName(i)] = rd.IsDBNull(i) ? null : rd.GetValue(i);
            rows.Add(row);
        }

        return Ok(new { ok = true, columns, data = rows });
    }

    #endregion

    #region ===== 搜尋後處理 (料號帶入後的 SP 呼叫) =====

    /// <summary>
    /// 搜尋料號後帶入材料群組資料
    /// Delphi: QuotaPaper.pas btnLoadDataClick -> SQUdOCXQuotaMGNMatGet
    /// </summary>
    [HttpPost("QuotaMGNMatGet")]
    public async Task<IActionResult> QuotaMGNMatGet([FromBody] PaperNumRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.PaperNum))
            return BadRequest(new { ok = false, error = "PaperNum 為必填" });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand("exec SQUdOCXQuotaMGNMatGet @PaperNum", conn);
        cmd.Parameters.AddWithValue("@PaperNum", req.PaperNum.Trim());
        await cmd.ExecuteNonQueryAsync();

        return Ok(new { ok = true });
    }

    /// <summary>
    /// 搜尋料號後帶入製程成本資料
    /// Delphi: QuotaPaper.pas btnLoadDataClick -> SQUdOCXQuotaSubCostIns
    /// </summary>
    [HttpPost("QuotaSubCostIns")]
    public async Task<IActionResult> QuotaSubCostIns([FromBody] PaperNumRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.PaperNum))
            return BadRequest(new { ok = false, error = "PaperNum 為必填" });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand("exec SQUdOCXQuotaSubCostIns @PaperNum", conn);
        cmd.Parameters.AddWithValue("@PaperNum", req.PaperNum.Trim());
        await cmd.ExecuteNonQueryAsync();

        return Ok(new { ok = true });
    }

    public class PaperNumRequest
    {
        public string PaperNum { get; set; } = "";
    }

    #endregion

    #region ===== 客戶→報價方式自動帶入 =====

    /// <summary>
    /// 客戶變更時自動取得 ComputeId
    /// Delphi: QuotaPaper.pas cboCompanyIdChange
    /// </summary>
    [HttpGet("GetComputeId")]
    public async Task<IActionResult> GetComputeId([FromQuery] string companyId)
    {
        if (string.IsNullOrWhiteSpace(companyId))
            return Ok(new { ok = true, computeId = "" });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand(@"
            SELECT t1.ComputeId
            FROM AJNdCompany t1(NOLOCK)
            INNER JOIN SQUdQuotaMethod t2(NOLOCK) ON t1.ComputeId = t2.FactorId
            WHERE t1.CompanyId = @CompanyId
              AND ISNULL(t1.ComputeId, '') <> ''", conn);
        cmd.Parameters.AddWithValue("@CompanyId", companyId.Trim());

        var result = await cmd.ExecuteScalarAsync();
        return Ok(new { ok = true, computeId = result?.ToString()?.Trim() ?? "" });
    }

    #endregion

    #region ===== 結構比對 (tabStruct) =====

    /// <summary>
    /// 取得報價料號的結構樹資料
    /// Delphi: QuotaPaper.pas treeOpen -> qryDetailTree
    /// </summary>
    [HttpGet("StructTree")]
    public async Task<IActionResult> GetStructTree([FromQuery] string paperNum)
    {
        if (string.IsNullOrWhiteSpace(paperNum))
            return BadRequest(new { ok = false, error = "paperNum 為必填" });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        // 報價料號結構 (Delphi: qryDetailTree)
        await using var cmd = new SqlCommand(@"
            SELECT PaperNum, Item, Value, Notes,
                   Caption = CONVERT(VARCHAR(255), RTRIM(Notes) + '=' + RTRIM(ISNULL(Value,'NULL'))),
                   SuperId, LevelNo
            FROM SQUdQuotaSub(NOLOCK)
            WHERE PaperNum = @PaperNum
            ORDER BY Notes", conn);
        cmd.Parameters.AddWithValue("@PaperNum", paperNum.Trim());

        var rows = new List<Dictionary<string, object?>>();
        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            var row = new Dictionary<string, object?>();
            for (int i = 0; i < rd.FieldCount; i++)
                row[rd.GetName(i)] = rd.IsDBNull(i) ? null : rd.GetValue(i);
            rows.Add(row);
        }

        return Ok(new { ok = true, data = rows });
    }

    /// <summary>
    /// 取得參考料號的結構樹資料
    /// Delphi: QuotaPaper.pas treeOpen -> qryDetailTreeOrg
    /// </summary>
    [HttpGet("StructTreeOrg")]
    public async Task<IActionResult> GetStructTreeOrg([FromQuery] string partNumRevision)
    {
        if (string.IsNullOrWhiteSpace(partNumRevision))
            return BadRequest(new { ok = false, error = "partNumRevision 為必填" });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        // 參考料號結構 (Delphi: qryDetailTreeOrg)
        await using var cmd = new SqlCommand(@"
            SELECT t1.PaperNum, t1.Item, t1.Value, t1.Notes,
                   Caption = CONVERT(VARCHAR(255), RTRIM(t1.Notes) + '=' + RTRIM(ISNULL(t1.Value,'NULL'))),
                   t1.SuperId, t1.LevelNo
            FROM SQUdQuotaSub t1(NOLOCK)
            INNER JOIN SQUdQuotaMain t3(NOLOCK) ON t1.PaperNum = t3.PaperNum
            WHERE t1.PaperNum = @PaperNum AND t3.PaperType = 255
            ORDER BY t1.Notes", conn);
        cmd.Parameters.AddWithValue("@PaperNum", partNumRevision.Trim());

        var rows = new List<Dictionary<string, object?>>();
        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            var row = new Dictionary<string, object?>();
            for (int i = 0; i < rd.FieldCount; i++)
                row[rd.GetName(i)] = rd.IsDBNull(i) ? null : rd.GetValue(i);
            rows.Add(row);
        }

        return Ok(new { ok = true, data = rows });
    }

    #endregion

    #region ===== 明細格搜尋 (gridDetail1 DblClick) =====

    /// <summary>
    /// 明細格雙擊搜尋子項目 (SQUdGetItemListSub)
    /// Delphi: QuotaPaper.pas gridDetail1DblClick
    /// </summary>
    [HttpGet("ItemListSub")]
    public async Task<IActionResult> GetItemListSub(
        [FromQuery] string paperNum, [FromQuery] string numId)
    {
        if (string.IsNullOrWhiteSpace(paperNum) || string.IsNullOrWhiteSpace(numId))
            return BadRequest(new { ok = false, error = "paperNum 和 numId 為必填" });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand(
            "exec SQUdGetItemListSub @PaperNum, '', @NumId", conn);
        cmd.Parameters.AddWithValue("@PaperNum", paperNum.Trim());
        cmd.Parameters.AddWithValue("@NumId", numId.Trim());

        var rows = new List<Dictionary<string, object?>>();
        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            var row = new Dictionary<string, object?>();
            for (int i = 0; i < rd.FieldCount; i++)
                row[rd.GetName(i)] = rd.IsDBNull(i) ? null : rd.GetValue(i);
            rows.Add(row);
        }

        return Ok(new { ok = true, data = rows });
    }

    /// <summary>
    /// 取得 Detail1 表中每一列的 Item → NumId 對應
    /// 用於前端雙擊 DtlNumId 時取得 NumId（因 Grid 字典不一定包含 NumId 欄位）
    /// </summary>
    [HttpGet("DetailNumIds")]
    public async Task<IActionResult> GetDetailNumIds([FromQuery] string paperNum)
    {
        if (string.IsNullOrWhiteSpace(paperNum))
            return BadRequest(new { ok = false, error = "paperNum 為必填" });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        // 1) 從 CurdOcxtableSetUp 取得 DETAIL1 的字典表名
        string? dictTable = null;
        await using (var cmd1 = new SqlCommand(@"
            SELECT TOP 1 TableName FROM CURdOCXTableSetUp WITH (NOLOCK)
            WHERE ItemId = 'SQ000006' AND TableKind = 'DETAIL1'", conn))
        {
            var r = await cmd1.ExecuteScalarAsync();
            dictTable = r?.ToString();
        }
        if (string.IsNullOrWhiteSpace(dictTable))
            return Ok(new { ok = true, data = Array.Empty<object>() });

        // 2) 解析實體表名
        string realTable = dictTable;
        await using (var cmd2 = new SqlCommand(@"
            SELECT TOP 1 ISNULL(NULLIF(RealTableName,''), TableName)
            FROM CURdTableName WITH (NOLOCK) WHERE TableName = @tbl", conn))
        {
            cmd2.Parameters.AddWithValue("@tbl", dictTable);
            var r = await cmd2.ExecuteScalarAsync();
            if (r != null && r != DBNull.Value)
                realTable = r.ToString()!;
        }

        // 3) 先查出該表有哪些欄位，找出 key 欄位和 NumId
        var colNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        await using (var cmdCols = new SqlCommand(@"
            SELECT c.name FROM sys.columns c
            JOIN sys.objects o ON c.object_id = o.object_id
            WHERE o.name = @tbl", conn))
        {
            cmdCols.Parameters.AddWithValue("@tbl", realTable);
            await using var rdCols = await cmdCols.ExecuteReaderAsync();
            while (await rdCols.ReadAsync())
                colNames.Add(rdCols.GetString(0));
        }

        if (!colNames.Contains("NumId") || !colNames.Contains("NumName"))
            return Ok(new { ok = true, data = Array.Empty<object>(),
                debug = new { realTable, columns = colNames.ToArray() } });

        // 4) 用 NumName 作為 key 回傳 NumName → NumId 對應
        var rows = new List<Dictionary<string, object?>>();
        await using (var cmd3 = new SqlCommand(
            $"SELECT [NumName], [NumId] FROM [{realTable}] WITH (NOLOCK) WHERE PaperNum = @pn", conn))
        {
            cmd3.Parameters.AddWithValue("@pn", paperNum.Trim());
            await using var rd = await cmd3.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                rows.Add(new Dictionary<string, object?>
                {
                    ["NumName"] = rd.IsDBNull(0) ? null : rd.GetValue(0),
                    ["NumId"] = rd.IsDBNull(1) ? null : rd.GetValue(1)
                });
            }
        }

        return Ok(new { ok = true, data = rows });
    }

    #endregion

    #region ===== VerNum 取得 =====

    /// <summary>
    /// 取得料號最小版本號
    /// Delphi: QuotaPaper.pas btnLoadDataClick -> MIN(VerNum)
    /// </summary>
    [HttpGet("GetVerNum")]
    public async Task<IActionResult> GetVerNum([FromQuery] string matGroup)
    {
        if (string.IsNullOrWhiteSpace(matGroup))
            return Ok(new { ok = true, verNum = "" });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand(
            "SELECT VerNum = MIN(VerNum) FROM MGNdMatGroupItemsVer(NOLOCK) WHERE MatGroup = @MatGroup", conn);
        cmd.Parameters.AddWithValue("@MatGroup", matGroup.Trim());

        var result = await cmd.ExecuteScalarAsync();
        return Ok(new { ok = true, verNum = result?.ToString()?.Trim() ?? "" });
    }

    #endregion

    #region ===== UOM 取得 =====

    /// <summary>
    /// 取得 UOM 下拉選項
    /// Delphi: QuotaPaper.pas JSdLookupCombo1Enter -> qryUOM
    /// </summary>
    [HttpGet("GetUOM")]
    public async Task<IActionResult> GetUOM([FromQuery] string partNum)
    {
        if (string.IsNullOrWhiteSpace(partNum))
            return Ok(new { ok = true, data = new List<object>() });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand(
            "exec SQUdGetUOM @PartNum", conn);
        cmd.Parameters.AddWithValue("@PartNum", partNum.Trim());

        var rows = new List<Dictionary<string, object?>>();
        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            var row = new Dictionary<string, object?>();
            for (int i = 0; i < rd.FieldCount; i++)
                row[rd.GetName(i)] = rd.IsDBNull(i) ? null : rd.GetValue(i);
            rows.Add(row);
        }

        return Ok(new { ok = true, data = rows });
    }

    #endregion
}
