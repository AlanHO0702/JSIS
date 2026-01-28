using System.Data;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;

/// <summary>
/// 應收帳款立帳單自訂按鈕 API 控制器
/// 對應 Delphi: PRSelectDLL.pas / PRSelectDLL.dfm
/// 功能: 選擇可審核之憑證加入付款收款單
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PRSelectController : ControllerBase
{
    private readonly string _cs;

    public PRSelectController(IConfiguration cfg, PcbErpContext db)
    {
        _cs = cfg.GetConnectionString("DefaultConnection")
              ?? db?.Database.GetDbConnection().ConnectionString
              ?? throw new InvalidOperationException("找不到連線字串");
    }

    /// <summary>
    /// 取得主檔資料 (對應 btnGetParamsClick)
    /// GET /api/PRSelect/GetMainData?paperNum=xxx
    /// </summary>
    [HttpGet("GetMainData")]
    public async Task<IActionResult> GetMainData([FromQuery] string paperNum)
    {
        if (string.IsNullOrWhiteSpace(paperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            // 取得主檔資料
            var sql = @"
                SELECT AccId, CJourName, PaperType, CompanyId, IsIn, ExpectDate, PaperDate
                FROM APRdPayRecvMain WITH (NOLOCK)
                WHERE PaperNum = @paperNum";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@paperNum", paperNum);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return Ok(new { ok = false, error = "找不到單據資料" });

            var accId = reader["AccId"]?.ToString() ?? "";
            var cjourName = reader["CJourName"]?.ToString() ?? "";
            var paperType = reader["PaperType"] == DBNull.Value ? 0 : Convert.ToInt32(reader["PaperType"]);
            var companyId = reader["CompanyId"]?.ToString() ?? "";
            var isIn = reader["IsIn"] == DBNull.Value ? 0 : Convert.ToInt32(reader["IsIn"]);
            var expectDate = reader["ExpectDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["ExpectDate"]);
            var paperDate = reader["PaperDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["PaperDate"]);

            await reader.CloseAsync();

            // 取得 UseId (從 CURdUsers)
            var jwtHeader = Request?.Headers["X-JWTID"].ToString();
            string useId = "A001";
            if (!string.IsNullOrWhiteSpace(jwtHeader) && Guid.TryParse(jwtHeader, out var jwtId))
            {
                await using var useIdCmd = new SqlCommand(
                    @"SELECT u.UseId
                      FROM CURdUserOnline o WITH (NOLOCK)
                      INNER JOIN CURdUsers u WITH (NOLOCK) ON o.UserId = u.UserId
                      WHERE o.JwtId = @jwtId", conn);
                useIdCmd.Parameters.AddWithValue("@jwtId", jwtId);
                var result = await useIdCmd.ExecuteScalarAsync();
                if (result != null && result != DBNull.Value)
                    useId = result.ToString() ?? "A001";
            }

            // 檢查 PRFlowChange 參數
            var prFlowChange = 0;
            await using var flowCmd = new SqlCommand(
                @"SELECT Value FROM CURdSysParams WITH (NOLOCK)
                  WHERE SystemId = 'APR' AND ParamId = 'PRFlowChange' AND Value = '1'", conn);
            var flowResult = await flowCmd.ExecuteScalarAsync();
            if (flowResult != null && flowResult != DBNull.Value)
                prFlowChange = 1;

            // 檢查帳本科目是否必要
            if (string.IsNullOrWhiteSpace(cjourName) && string.IsNullOrWhiteSpace(accId) && paperType == 0)
            {
                if (prFlowChange == 0)
                {
                    return Ok(new { ok = false, error = "帳款科目是必要欄位，請輸入" });
                }
            }

            // 檢查 PRIsReplace 參數
            var showReplace = false;
            await using var replaceCmd = new SqlCommand(
                @"SELECT Value FROM CURdSysParams WITH (NOLOCK)
                  WHERE SystemId = 'APR' AND ParamId = 'PRIsReplace' AND ISNULL(Value, '') = '1'", conn);
            var replaceResult = await replaceCmd.ExecuteScalarAsync();
            if (replaceResult != null && replaceResult != DBNull.Value)
                showReplace = true;

            // 檢查 SSDepartMode 參數
            var showDepart = false;
            var shipToMode = false;
            await using var departCmd = new SqlCommand(
                @"SELECT Value FROM CURdSysParams WITH (NOLOCK)
                  WHERE SystemId = 'SPO' AND ParamId = 'SSDepartMode' AND ISNULL(Value, '') = '1'", conn);
            var departResult = await departCmd.ExecuteScalarAsync();
            if (departResult != null && departResult != DBNull.Value)
            {
                showDepart = true;
                if (isIn == 0)
                    shipToMode = true;
            }

            return Ok(new
            {
                ok = true,
                paperType,
                useId,
                companyId,
                isIn,
                expectDate,
                paperDate,
                showReplace,
                showDepart,
                shipToMode
            });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 帳款型態選項 (對應 qryIsIn)
    /// GET /api/PRSelect/IsInOptions
    /// </summary>
    [HttpGet("IsInOptions")]
    public async Task<IActionResult> IsInOptions()
    {
        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();
            var sql = "SELECT IsIn AS value, IsInName AS text FROM APRdIsIn WITH (NOLOCK)";
            await using var cmd = new SqlCommand(sql, conn);
            var list = new List<object>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new
                {
                    value = reader["value"]?.ToString() ?? "",
                    text = reader["text"]?.ToString() ?? ""
                });
            }
            return Ok(list);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// 公司別選項 (對應 qryUseId)
    /// GET /api/PRSelect/UseIdOptions
    /// </summary>
    [HttpGet("UseIdOptions")]
    public async Task<IActionResult> UseIdOptions()
    {
        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();
            var sql = "SELECT BUId AS value, BUName AS text FROM dbo.CURdBU WITH (NOLOCK)";
            await using var cmd = new SqlCommand(sql, conn);
            var list = new List<object>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new
                {
                    value = reader["value"]?.ToString()?.Trim() ?? "",
                    text = reader["text"]?.ToString()?.Trim() ?? ""
                });
            }
            return Ok(list);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// 客廠名稱選項 (對應 qryVCompany)
    /// GET /api/PRSelect/CompanyOptions
    /// </summary>
    [HttpGet("CompanyOptions")]
    public async Task<IActionResult> CompanyOptions()
    {
        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();
            var sql = "SELECT CompanyId AS value, ShortName AS text FROM dbo.AJNdCompany WITH (NOLOCK)";
            await using var cmd = new SqlCommand(sql, conn);
            var list = new List<object>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new
                {
                    value = reader["value"]?.ToString() ?? "",
                    text = reader["text"]?.ToString() ?? ""
                });
            }
            return Ok(list);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// 幣別選項 (對應 qryClassMoney)
    /// GET /api/PRSelect/MoneyOptions
    /// </summary>
    [HttpGet("MoneyOptions")]
    public async Task<IActionResult> MoneyOptions()
    {
        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();
            var sql = "SELECT MoneyCode AS value, MoneyName AS text FROM dbo.AJNdClassMoney WITH (NOLOCK)";
            await using var cmd = new SqlCommand(sql, conn);
            var list = new List<object>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new
                {
                    value = reader["value"]?.ToString() ?? "",
                    text = reader["text"]?.ToString() ?? ""
                });
            }
            return Ok(list);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// 請購人選項 (對應 qryPettyEmpId)
    /// GET /api/PRSelect/EmpOptions
    /// </summary>
    [HttpGet("EmpOptions")]
    public async Task<IActionResult> EmpOptions()
    {
        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();
            var sql = "SELECT EmpId AS value, EmpName AS text FROM dbo.HPSdEmpInfo WITH (NOLOCK)";
            await using var cmd = new SqlCommand(sql, conn);
            var list = new List<object>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new
                {
                    value = reader["value"]?.ToString() ?? "",
                    text = reader["text"]?.ToString() ?? ""
                });
            }
            return Ok(list);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// 作業者選項 (對應 qryPettyUserId)
    /// GET /api/PRSelect/UserOptions
    /// </summary>
    [HttpGet("UserOptions")]
    public async Task<IActionResult> UserOptions()
    {
        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();
            var sql = "SELECT UserId AS value, UserName AS text FROM dbo.CURdUsers WITH (NOLOCK)";
            await using var cmd = new SqlCommand(sql, conn);
            var list = new List<object>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new
                {
                    value = reader["value"]?.ToString() ?? "",
                    text = reader["text"]?.ToString() ?? ""
                });
            }
            return Ok(list);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// 部門選項 (對應 qryDepart)
    /// GET /api/PRSelect/DepartOptions
    /// </summary>
    [HttpGet("DepartOptions")]
    public async Task<IActionResult> DepartOptions()
    {
        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();
            var sql = "SELECT DepartId AS value, DepartName AS text FROM dbo.AJNdDepart WITH (NOLOCK)";
            await using var cmd = new SqlCommand(sql, conn);
            var list = new List<object>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new
                {
                    value = reader["value"]?.ToString() ?? "",
                    text = reader["text"]?.ToString() ?? ""
                });
            }
            return Ok(list);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// 出貨欄頭選項 (對應 qryShipTo)
    /// GET /api/PRSelect/ShipToOptions?companyId=xxx
    /// </summary>
    [HttpGet("ShipToOptions")]
    public async Task<IActionResult> ShipToOptions([FromQuery] string companyId)
    {
        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();
            var sql = @"SELECT OutTitle AS value, OutAddress AS text
                        FROM dbo.AJNdCompanyOutAddr WITH (NOLOCK)
                        WHERE CompanyId = @companyId";
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@companyId", companyId ?? "");
            var list = new List<object>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new
                {
                    value = reader["value"]?.ToString() ?? "",
                    text = reader["text"]?.ToString() ?? ""
                });
            }
            return Ok(list);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// 取得已選取的資料 (對應 qrySelected)
    /// GET /api/PRSelect/GetSelected?paperNum=xxx
    /// </summary>
    [HttpGet("GetSelected")]
    public async Task<IActionResult> GetSelected([FromQuery] string paperNum)
    {
        if (string.IsNullOrWhiteSpace(paperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("APRdPayRecvChoiceed", conn)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 120
            };
            cmd.Parameters.AddWithValue("@PaperNum", paperNum);

            var rows = new List<Dictionary<string, object?>>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object?>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var name = reader.GetName(i);
                    var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    if (value is DateTime dt)
                        row[name] = dt.ToString("yyyy-MM-dd");
                    else
                        row[name] = value;
                }
                rows.Add(row);
            }

            return Ok(new { ok = true, rows });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 查詢可選憑證 (對應 btFindClick)
    /// POST /api/PRSelect/Search
    /// </summary>
    [HttpPost("Search")]
    public async Task<IActionResult> Search([FromBody] PRSearchRequest req)
    {
        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            // 建構條件字串 (對應 Delphi 的 sCondition)
            var conditions = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(req.IsIn))
                conditions.Append($" and t1.IsIn = '{req.IsIn}' ");
            if (!string.IsNullOrWhiteSpace(req.UseId))
                conditions.Append($" and t1.UseId = '{req.UseId}' ");
            if (!string.IsNullOrWhiteSpace(req.CompanyId))
                conditions.Append($" and t1.CompanyId = '{req.CompanyId}' ");
            if (!string.IsNullOrWhiteSpace(req.PaperDateB))
                conditions.Append($" and t1.PaperDate >= '{req.PaperDateB}' ");
            if (!string.IsNullOrWhiteSpace(req.PaperDateE))
                conditions.Append($" and t1.PaperDate <= '{req.PaperDateE}' ");
            if (!string.IsNullOrWhiteSpace(req.ExpectDate))
                conditions.Append($" and t1.ExpectDate <= '{req.ExpectDate}' ");
            if (!string.IsNullOrWhiteSpace(req.PaperNum))
                conditions.Append($" and t1.PaperNum = '{req.PaperNum}' ");
            if (!string.IsNullOrWhiteSpace(req.SourNum))
                conditions.Append($" and t1.SourNum = '{req.SourNum}' ");
            if (!string.IsNullOrWhiteSpace(req.MoneyCode))
                conditions.Append($" and t1.MoneyCode = '{req.MoneyCode}' ");
            if (!string.IsNullOrWhiteSpace(req.PettyEmpId))
                conditions.Append($" and t1.PettyEmpId = '{req.PettyEmpId}' ");

            // 作業者/出貨欄頭
            if (!string.IsNullOrWhiteSpace(req.PettyUserId))
            {
                if (req.ShipToMode)
                    conditions.Append($" and t1.ShipTo like '%{req.PettyUserId}%' ");
                else
                    conditions.Append($" and t1.PettyUserId = '{req.PettyUserId}' ");
            }

            // 部門
            if (!string.IsNullOrWhiteSpace(req.DepartId))
                conditions.Append($" and t1.DepartId = '{req.DepartId}' ");

            // 取代已存在
            if (req.ShowReplace && !string.IsNullOrWhiteSpace(req.MainPaperNum))
                conditions.Append($" #&#" + req.MainPaperNum + "#&# ");

            await using var cmd = new SqlCommand("APRdPayRecvChoice", conn)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 120
            };
            cmd.Parameters.AddWithValue("@sCondition", conditions.ToString());
            cmd.Parameters.AddWithValue("@PaperType", req.PaperType);
            cmd.Parameters.AddWithValue("@UseId", req.UseId ?? "");

            var rows = new List<Dictionary<string, object?>>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object?>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var name = reader.GetName(i);
                    var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    if (value is DateTime dt)
                        row[name] = dt.ToString("yyyy-MM-dd");
                    else
                        row[name] = value;
                }
                rows.Add(row);
            }

            // 排除已存在明細的憑證
            if (req.ExcludePaperNums != null && req.ExcludePaperNums.Count > 0)
            {
                var excludeSet = new HashSet<string>(req.ExcludePaperNums, StringComparer.OrdinalIgnoreCase);
                rows = rows.Where(r =>
                {
                    var paperNum = r.TryGetValue("PaperNum", out var v) ? v?.ToString() : null;
                    return string.IsNullOrEmpty(paperNum) || !excludeSet.Contains(paperNum);
                }).ToList();
            }

            return Ok(new { ok = true, rows });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 確定儲存 (對應 btnOKClick)
    /// POST /api/PRSelect/Confirm
    /// </summary>
    [HttpPost("Confirm")]
    public async Task<IActionResult> Confirm([FromBody] PRConfirmRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.PaperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        if (req.Rows == null || req.Rows.Count == 0)
            return Ok(new { ok = false, error = "沒有選擇任何憑證" });

        await using var conn = new SqlConnection(_cs);
        SqlTransaction? tx = null;

        try
        {
            await conn.OpenAsync();
            tx = (SqlTransaction)await conn.BeginTransactionAsync();

            // 如果不是取代模式，先刪除現有資料
            if (!req.Replace)
            {
                await using var deleteCmd = new SqlCommand(
                    "DELETE FROM APRdPayRecvSub WHERE PaperNum = @PaperNum", conn, tx);
                deleteCmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);
                await deleteCmd.ExecuteNonQueryAsync();
            }

            // 逐筆插入選擇的憑證 (對應 qryOcxAdd)
            foreach (var sourNum in req.Rows)
            {
                await using var insertCmd = new SqlCommand("APRdPayRecvSubIns", conn, tx)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 60
                };
                insertCmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);
                insertCmd.Parameters.AddWithValue("@SourNum", sourNum);

                try
                {
                    await insertCmd.ExecuteNonQueryAsync();
                }
                catch (Exception ex)
                {
                    await tx.RollbackAsync();
                    return Ok(new { ok = false, error = ex.Message });
                }
            }

            // 執行彙總 (對應 qryOcxAddSum)
            await using var sumCmd = new SqlCommand("APRdPayRecvSubInsSum", conn, tx)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 60
            };
            sumCmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);
            sumCmd.Parameters.AddWithValue("@CJourNum", "");

            try
            {
                await sumCmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return Ok(new { ok = false, error = ex.Message });
            }

            // 預設子科目 (對應 qryOcxAddAccId)
            await using var accIdCmd = new SqlCommand("APRdDefaultSubAccID", conn, tx)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 60
            };
            accIdCmd.Parameters.AddWithValue("@PaperId", "APRdPayRecvSub");
            accIdCmd.Parameters.AddWithValue("@PaperNum", req.PaperNum);

            try
            {
                await accIdCmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return Ok(new { ok = false, error = ex.Message });
            }

            await tx.CommitAsync();
            return Ok(new { ok = true, count = req.Rows.Count });
        }
        catch (Exception ex)
        {
            if (tx != null)
            {
                try { await tx.RollbackAsync(); }
                catch { }
            }
            return Ok(new { ok = false, error = ex.Message });
        }
    }

    // Request Models
    public class PRSearchRequest
    {
        public string? IsIn { get; set; }
        public string? UseId { get; set; }
        public string? CompanyId { get; set; }
        public string? PaperDateB { get; set; }
        public string? PaperDateE { get; set; }
        public string? ExpectDate { get; set; }
        public string? PaperNum { get; set; }
        public string? SourNum { get; set; }
        public string? MoneyCode { get; set; }
        public string? PettyEmpId { get; set; }
        public string? PettyUserId { get; set; }
        public string? DepartId { get; set; }
        public int PaperType { get; set; }
        public string? MainPaperNum { get; set; }
        public bool ShowReplace { get; set; }
        public bool ShipToMode { get; set; }
        public List<string>? ExcludePaperNums { get; set; }  // 排除已存在明細的憑證號碼
    }

    public class PRConfirmRequest
    {
        public string? PaperNum { get; set; }
        public bool Replace { get; set; }
        public List<string>? Rows { get; set; }
    }

    /// <summary>
    /// 取得 PR000003.cshtml 的內容 (用於動態載入)
    /// GET /api/PRSelect/GetViewContent
    /// </summary>
    [HttpGet("GetViewContent")]
    public async Task<IActionResult> GetViewContent()
    {
        try
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Pages", "CustomButton", "PR000003.cshtml");

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound(new { ok = false, error = "找不到 PR000003.cshtml 文件" });
            }

            var content = await System.IO.File.ReadAllTextAsync(filePath);

            // 返回純文字內容
            return Content(content, "text/html");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { ok = false, error = ex.Message });
        }
    }
}
