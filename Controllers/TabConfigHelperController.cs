using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using System.Text.RegularExpressions;

namespace PcbErpApi.Controllers
{
    /// <summary>
    /// 頁籤配置輔助工具 - 提供自動推導功能
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TabConfigHelperController : ControllerBase
    {
        private readonly PcbErpContext _context;
        private readonly ITableDictionaryService _dictService;

        public TabConfigHelperController(PcbErpContext context, ITableDictionaryService dictService)
        {
            _context = context;
            _dictService = dictService;
        }

        /// <summary>
        /// 取得資料庫中所有的明細表清單 (排除主檔)
        /// </summary>
        /// <returns>表名清單,附帶中文說明</returns>
        [HttpGet("available-tables")]
        public async Task<IActionResult> GetAvailableTables()
        {
            try
            {
                // 從資料庫 schema 中取得所有表格
                var tables = await _context.Database
                    .SqlQueryRaw<string>(@"
                        SELECT TABLE_NAME
                        FROM INFORMATION_SCHEMA.TABLES
                        WHERE TABLE_TYPE = 'BASE TABLE'
                        AND TABLE_NAME NOT LIKE '%Main'
                        AND TABLE_NAME LIKE '%Sub'
                           OR TABLE_NAME LIKE '%PO'
                           OR TABLE_NAME LIKE '%Mat'
                           OR TABLE_NAME LIKE '%Layer'
                        ORDER BY TABLE_NAME
                    ")
                    .ToListAsync();

                // 為每個表格加上中文說明
                var result = tables.Select(tableName =>
                {
                    // 嘗試從資料辭典取得表格說明
                    var description = GetTableDescription(tableName);

                    return new
                    {
                        TableName = tableName,
                        Description = description,
                        DisplayText = string.IsNullOrEmpty(description)
                            ? tableName
                            : $"{description} ({tableName})"
                    };
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "取得表格清單失敗", detail = ex.Message });
            }
        }

        /// <summary>
        /// 根據明細表名稱,自動推導完整的頁籤配置
        /// </summary>
        /// <param name="detailTable">明細表名稱,例如: FMEdIssuePO</param>
        /// <returns>推導出的頁籤配置</returns>
        [HttpPost("suggest-config")]
        public IActionResult SuggestConfig([FromBody] SuggestConfigRequest request)
        {
            try
            {
                var detailTable = request.DetailTable?.Trim();
                if (string.IsNullOrEmpty(detailTable))
                {
                    return BadRequest(new { error = "明細表名稱不可為空" });
                }

                // 1. 推導頁籤ID (從表名最後一段)
                var tabId = GenerateTabId(detailTable);

                // 2. 推導頁籤標題 (從資料辭典)
                var tabTitle = GetTableDescription(detailTable) ?? detailTable;

                // 3. 推導 API 路徑 (約定: /api/{TableName})
                var apiUrl = GenerateApiUrl(detailTable);

                // 4. 辭典表預設使用明細表
                var dictTable = detailTable;

                // 5. 取得目前該主檔已有的頁籤數量,決定排序
                var nextOrder = GetNextTabOrder(request.MasterTable);

                return Ok(new
                {
                    TabId = tabId,
                    TabTitle = tabTitle,
                    DetailTable = detailTable,
                    ApiUrl = apiUrl,
                    DictTable = dictTable,
                    TabOrder = nextOrder,
                    Confidence = new
                    {
                        TabId = "high",      // 規則明確
                        TabTitle = string.IsNullOrEmpty(GetTableDescription(detailTable)) ? "low" : "high",
                        ApiUrl = "medium",   // 約定慣例,但可能有例外
                        DictTable = "high"   // 通常相同
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "推導配置失敗", detail = ex.Message });
            }
        }

        /// <summary>
        /// 驗證頁籤配置是否正確 (檢查表格存在、API可用等)
        /// </summary>
        [HttpPost("validate")]
        public async Task<IActionResult> ValidateConfig([FromBody] ValidateConfigRequest request)
        {
            var errors = new List<string>();
            var warnings = new List<string>();

            try
            {
                // 1. 檢查表格是否存在
                var tableExists = await CheckTableExists(request.DetailTable);
                if (!tableExists)
                {
                    errors.Add($"表格 {request.DetailTable} 不存在");
                }

                // 2. 檢查 TabId 是否重複
                var tabIdExists = await _context.TabConfigs.AnyAsync(t =>
                    t.MasterTable == request.MasterTable &&
                    t.TabId == request.TabId &&
                    t.IsActive);

                if (tabIdExists)
                {
                    errors.Add($"頁籤ID '{request.TabId}' 已存在");
                }

                // 3. 檢查 API 路徑格式
                if (!request.ApiUrl.StartsWith("/api/"))
                {
                    warnings.Add("API路徑建議以 /api/ 開頭");
                }

                // 4. 檢查辭典表是否存在
                if (!string.IsNullOrEmpty(request.DictTable))
                {
                    var dictExists = await CheckTableExists(request.DictTable);
                    if (!dictExists)
                    {
                        warnings.Add($"辭典表 {request.DictTable} 不存在,將使用預設欄位");
                    }
                }

                // 5. 檢查表格是否有資料 (可選警告)
                var hasData = await CheckTableHasData(request.DetailTable);
                if (!hasData)
                {
                    warnings.Add($"表格 {request.DetailTable} 目前沒有資料");
                }

                return Ok(new
                {
                    IsValid = errors.Count == 0,
                    Errors = errors,
                    Warnings = warnings
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "驗證失敗", detail = ex.Message });
            }
        }

        #region 私有方法 - 推導邏輯

        /// <summary>
        /// 從表名推導頁籤ID
        /// 例如: FMEdIssuePO -> po, FMEdIssueMat -> mat
        /// </summary>
        private string GenerateTabId(string tableName)
        {
            // 方法1: 取最後一段大寫字母組合
            var match = Regex.Match(tableName, @"[A-Z][a-z]*$");
            if (match.Success)
            {
                return match.Value.ToLower();
            }

            // 方法2: 取最後幾個字元
            if (tableName.Length > 3)
            {
                return tableName.Substring(tableName.Length - 3).ToLower();
            }

            // 方法3: 使用完整表名的簡寫
            return new string(tableName.Where(char.IsUpper).ToArray()).ToLower();
        }

        /// <summary>
        /// 從表名推導 API 路徑
        /// 約定: /api/{TableName} (保留大小寫)
        /// </summary>
        private string GenerateApiUrl(string tableName)
        {
            return $"/api/{tableName}";
        }

        /// <summary>
        /// 從資料辭典取得表格說明
        /// </summary>
        private string? GetTableDescription(string tableName)
        {
            try
            {
                // 嘗試從 CURdTableField 取得第一個欄位的 TableNote
                var firstField = _context.Set<CURdTableField>()
                    .Where(f => f.TableName.ToLower() == tableName.ToLower())
                    .Select(f => new { f.TableNote })
                    .FirstOrDefault();

                return firstField?.TableNote;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 取得下一個頁籤的排序號碼
        /// </summary>
        private int GetNextTabOrder(string masterTable)
        {
            var maxOrder = _context.TabConfigs
                .Where(t => t.MasterTable == masterTable && t.IsActive)
                .Select(t => t.TabOrder)
                .DefaultIfEmpty(0)
                .Max();

            return maxOrder + 1;
        }

        /// <summary>
        /// 檢查表格是否存在
        /// </summary>
        private async Task<bool> CheckTableExists(string tableName)
        {
            try
            {
                var result = await _context.Database
                    .SqlQueryRaw<int>($@"
                        SELECT COUNT(*)
                        FROM INFORMATION_SCHEMA.TABLES
                        WHERE TABLE_NAME = '{tableName}'
                    ")
                    .FirstOrDefaultAsync();

                return result > 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 檢查表格是否有資料
        /// </summary>
        private async Task<bool> CheckTableHasData(string tableName)
        {
            try
            {
                var result = await _context.Database
                    .SqlQueryRaw<int>($"SELECT COUNT(*) FROM [{tableName}]")
                    .FirstOrDefaultAsync();

                return result > 0;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }

    #region DTO

    public class SuggestConfigRequest
    {
        public string MasterTable { get; set; } = string.Empty;
        public string DetailTable { get; set; } = string.Empty;
    }

    public class ValidateConfigRequest
    {
        public string MasterTable { get; set; } = string.Empty;
        public string TabId { get; set; } = string.Empty;
        public string DetailTable { get; set; } = string.Empty;
        public string ApiUrl { get; set; } = string.Empty;
        public string? DictTable { get; set; }
    }

    #endregion
}
