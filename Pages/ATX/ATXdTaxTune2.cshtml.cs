using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PcbErpApi.Data;

namespace PcbErpApi.Pages.ATX
{
    public class ATXdTaxTune2Model : PageModel
    {
        private readonly PcbErpContext _context;
        private readonly ILogger<ATXdTaxTune2Model> _logger;

        public ATXdTaxTune2Model(PcbErpContext context, ILogger<ATXdTaxTune2Model> logger)
        {
            _context = context;
            _logger = logger;
        }

        public List<(string Id, string Name)> UseIdOptions { get; set; } = new();
        public List<(string Id, string Name)> UserIdOptions { get; set; } = new();
        public string DefaultHisId { get; set; } = "";
        public string DefaultUseId { get; set; } = "";
        public string DefaultUserId { get; set; } = "";
        public string? LoadError { get; set; }

        public async Task OnGetAsync()
        {
            ViewData["Title"] = "零稅率申報檔";
            var conn = _context.Database.GetDbConnection();
            var opened = conn.State != ConnectionState.Open;
            if (opened) await _context.Database.OpenConnectionAsync();
            try
            {
                // 載入期別預設值 (LockCostMonth)
                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "select Value from CURdSysParams(nolock) Where SystemId='ATX' and ParamId='LockCostMonth'";
                    var val = await cmd.ExecuteScalarAsync();
                    if (val != null && val != DBNull.Value)
                    {
                        var lockCostMonth = val.ToString() ?? "";
                        if (lockCostMonth.Length >= 7)
                        {
                            var year = int.Parse(lockCostMonth.Substring(0, 4)) - 1911;
                            var month = lockCostMonth.Substring(5, 2);
                            DefaultHisId = year.ToString() + month;
                        }
                    }
                }

                // 載入公司別
                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "Select BUId, BUName from dbo.CURdBU(nolock)";
                    await using var rd = await cmd.ExecuteReaderAsync();
                    while (await rd.ReadAsync())
                        UseIdOptions.Add((rd["BUId"]?.ToString()?.Trim() ?? "", rd["BUName"]?.ToString()?.Trim() ?? ""));
                }

                // 載入作業人員
                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "select UserId, UserName from CURdUsers(nolock)";
                    await using var rd = await cmd.ExecuteReaderAsync();
                    while (await rd.ReadAsync())
                        UserIdOptions.Add((rd["UserId"]?.ToString()?.Trim() ?? "", rd["UserName"]?.ToString()?.Trim() ?? ""));
                }

                // 預設值
                DefaultUseId = ResolveUseId();
                DefaultUserId = ResolveUserId();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "載入零稅率申報檔初始資料失敗");
                LoadError = ex.Message;
            }
            finally
            {
                if (opened) await _context.Database.CloseConnectionAsync();
            }
        }

        public async Task<IActionResult> OnGetExportAsync(string hisId, string useId, string userId, string reportType)
        {
            var conn = _context.Database.GetDbConnection();
            var opened = conn.State != ConnectionState.Open;
            if (opened) await _context.Database.OpenConnectionAsync();
            try
            {
                // 取得統一編號作為檔名
                string uniformId = "TaxReport";
                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "select UniformId from AJNdCompany(nolock) where CompanyId='A001'";
                    var val = await cmd.ExecuteScalarAsync();
                    if (val != null && val != DBNull.Value)
                    {
                        var uid = val.ToString()?.Trim();
                        if (!string.IsNullOrEmpty(uid))
                            uniformId = uid;
                    }
                }

                // 依報表類型決定預存程序、副檔名、欄位寬度
                bool isZeroTax = reportType != "2";
                string spName = isZeroTax ? "ATXdZeroTaxReport" : "ATXdAssetTaxReport";
                string fileExt = isZeroTax ? "T02" : "T08";
                int padWidth = isZeroTax ? 83 : 0; // 零稅率補空白至83字元，固資退稅不補

                var lines = new List<string>();
                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"Exec {spName} @HisId, @UseId, @UserId";

                    var pHisId = cmd.CreateParameter();
                    pHisId.ParameterName = "@HisId";
                    pHisId.Value = (object?)hisId ?? DBNull.Value;
                    cmd.Parameters.Add(pHisId);

                    var pUseId = cmd.CreateParameter();
                    pUseId.ParameterName = "@UseId";
                    pUseId.Value = (object?)useId ?? DBNull.Value;
                    cmd.Parameters.Add(pUseId);

                    var pUserId = cmd.CreateParameter();
                    pUserId.ParameterName = "@UserId";
                    pUserId.Value = (object?)userId ?? DBNull.Value;
                    cmd.Parameters.Add(pUserId);

                    await using var rd = await cmd.ExecuteReaderAsync();
                    while (await rd.ReadAsync())
                    {
                        var taxFile = rd["TaxFile"]?.ToString() ?? "";
                        if (padWidth > 0)
                            lines.Add(taxFile.PadRight(padWidth));
                        else
                            lines.Add(taxFile);
                    }
                }

                // 產生檔案
                var sb = new StringBuilder();
                foreach (var line in lines)
                    sb.AppendLine(line);

                var bytes = Encoding.Default.GetBytes(sb.ToString());
                var fileName = $"{uniformId}.{fileExt}";

                return File(bytes, "application/octet-stream", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "產生零稅率申報檔失敗");
                return BadRequest(new { error = ex.Message });
            }
            finally
            {
                if (opened) await _context.Database.CloseConnectionAsync();
            }
        }

        private string ResolveUseId()
        {
            var claim = User?.Claims?.FirstOrDefault(c => string.Equals(c.Type, "UseId", StringComparison.OrdinalIgnoreCase))?.Value;
            var item = HttpContext.Items["UseId"]?.ToString();
            return string.IsNullOrWhiteSpace(claim) ? (string.IsNullOrWhiteSpace(item) ? "" : item) : claim;
        }

        private string ResolveUserId()
        {
            var claim = User?.Claims?.FirstOrDefault(c => string.Equals(c.Type, "UserId", StringComparison.OrdinalIgnoreCase))?.Value;
            var item = HttpContext.Items["UserId"]?.ToString();
            return string.IsNullOrWhiteSpace(claim) ? (string.IsNullOrWhiteSpace(item) ? "" : item) : claim;
        }
    }
}
