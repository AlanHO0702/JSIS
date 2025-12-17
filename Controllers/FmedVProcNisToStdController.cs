using Microsoft.AspNetCore.Mvc;
using PcbErpApi.Models;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using System.Reflection;

namespace PcbErpApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FmedVProcNisToStdController : ControllerBase
    {
        private readonly PcbErpContext _context;

        public FmedVProcNisToStdController(PcbErpContext context)
        {
            _context = context;
        }

            // GET: api/FmedVProcNisToStd
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FmedVProcNisToStd>>> GetFmedVProcNisToStd()
        {
            return await _context.FmedVProcNisToStd.ToListAsync();
        }

        // GET: api/FmedVProcNisToStd/paged
        [HttpGet("paged")]
        public async Task<IActionResult> GetPagedFmedVProcNisToStd(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            var query = _context.FmedVProcNisToStd.AsQueryable();

            var totalCount = await query.CountAsync();
            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new { totalCount, data });
        }


        // POST: api/FmedVProcNisToStd/FixFieldVisibility
        [HttpPost("FixFieldVisibility")]
        public async Task<IActionResult> FixFieldVisibility()
        {
            var tableName = "FMEdV_ProcNIS_ToStd";

            // 步驟 0: 先刪除大小寫錯誤的舊記錄
            await _context.Database.ExecuteSqlRawAsync(@"
                DELETE FROM CURdTableField
                WHERE TableName = @TableName
                AND FieldName IN ('strL_LLPiece', 'Popname')",
                new Microsoft.Data.SqlClient.SqlParameter("@TableName", tableName)
            );

            // 步驟 1: 先將所有欄位設為不可見
            await _context.Database.ExecuteSqlRawAsync(
                "UPDATE CURdTableField SET Visible = 0 WHERE TableName = @TableName",
                new Microsoft.Data.SqlClient.SqlParameter("@TableName", tableName)
            );

            // 步驟 2: 確保 TableName 存在於 CURdTableName
            await _context.Database.ExecuteSqlRawAsync(@"
                IF NOT EXISTS (SELECT 1 FROM CURdTableName WHERE TableName = @TableName)
                BEGIN
                    INSERT INTO CURdTableName (TableName, TableNote)
                    VALUES (@TableName, N'製程現帳')
                END",
                new Microsoft.Data.SqlClient.SqlParameter("@TableName", tableName)
            );

            // 步驟 3: 只將模型中存在的欄位設為可見，並設定完整資訊（使用 MERGE）
            // 欄位名稱必須與 Model 屬性名稱完全一致
            var fieldsToShow = new[] {
                new { FieldName = "PaperNum", DisplayLabel = "製令單號", SerialNum = 1, DataType = "text" },
                new { FieldName = "ExpStkTime", DisplayLabel = "期望繳庫日", SerialNum = 2, DataType = "date" },
                new { FieldName = "PartNum", DisplayLabel = "品號", SerialNum = 3, DataType = "text" },
                new { FieldName = "Revision", DisplayLabel = "版序", SerialNum = 4, DataType = "text" },
                new { FieldName = "LotNum", DisplayLabel = "批號", SerialNum = 5, DataType = "text" },
                new { FieldName = "StrL_LLpiece", DisplayLabel = "排版數", SerialNum = 6, DataType = "text" },
                new { FieldName = "Qnty", DisplayLabel = "數量", SerialNum = 7, DataType = "number" },
                new { FieldName = "PopName", DisplayLabel = "形狀", SerialNum = 8, DataType = "text" },
                new { FieldName = "ProcCode", DisplayLabel = "製程", SerialNum = 9, DataType = "text" },
                new { FieldName = "ProcName", DisplayLabel = "製程名稱", SerialNum = 10, DataType = "text" },
                new { FieldName = "AftProcNameString", DisplayLabel = "後續製程名稱", SerialNum = 11, DataType = "text" }
            };

            foreach (var field in fieldsToShow)
            {
                // 使用 MERGE 語法：如果存在就更新，不存在就插入
                await _context.Database.ExecuteSqlRawAsync(@"
                    MERGE CURdTableField AS target
                    USING (SELECT @TableName AS TableName, @FieldName AS FieldName) AS source
                    ON target.TableName = source.TableName AND target.FieldName = source.FieldName
                    WHEN MATCHED THEN
                        UPDATE SET
                            Visible = 1,
                            SerialNum = @SerialNum,
                            DisplayLabel = @DisplayLabel,
                            DataType = @DataType
                    WHEN NOT MATCHED THEN
                        INSERT (TableName, FieldName, DisplayLabel, SerialNum, Visible, DataType,
                                iFieldWidth, iShowWhere, ComboTextSize, IsMoneyField, bShow4Money,
                                ReadOnly, PK, FK, IsNeed)
                        VALUES (@TableName, @FieldName, @DisplayLabel, @SerialNum, 1, @DataType,
                                120, 3, 0, 0, 0, 0, 0, 0, 0);",
                    new Microsoft.Data.SqlClient.SqlParameter("@TableName", tableName),
                    new Microsoft.Data.SqlClient.SqlParameter("@FieldName", field.FieldName),
                    new Microsoft.Data.SqlClient.SqlParameter("@SerialNum", field.SerialNum),
                    new Microsoft.Data.SqlClient.SqlParameter("@DisplayLabel", field.DisplayLabel),
                    new Microsoft.Data.SqlClient.SqlParameter("@DataType", field.DataType)
                );
            }

            return Ok(new {
                message = "已修復欄位可見性設定（含 DisplayLabel 和 DataType）",
                totalVisible = fieldsToShow.Length,
                fields = fieldsToShow.Select(f => new { f.FieldName, f.DisplayLabel, f.DataType }).ToArray()
            });
        }

        // POST: api/FmedVProcNisToStd/InitializeFields
        [HttpPost("InitializeFields")]
        public async Task<IActionResult> InitializeFields()
        {
            var tableName = "FMEdV_ProcNIS_ToStd";

            // 檢查是否已有欄位定義
            var existingCount = await _context.CURdTableFields
                .CountAsync(x => x.TableName == tableName);

            if (existingCount > 0)
            {
                return Ok(new { message = $"欄位定義已存在，共 {existingCount} 筆", count = existingCount });
            }

            // 先確保 TableName 存在於 CURdTableName（避免外鍵衝突）
            await _context.Database.ExecuteSqlRawAsync(@"
                    IF NOT EXISTS (SELECT 1 FROM CURdTableName WHERE TableName = @TableName)
                    BEGIN
                        INSERT INTO CURdTableName (TableName, TableNote)
                        VALUES (@TableName, N'製程現帳')
                    END",
                    new Microsoft.Data.SqlClient.SqlParameter("@TableName", tableName)
                );

            // 定義要顯示的欄位（根據原本的表格）
            var fieldDefinitions = new[]
            {
                new { FieldName = "PaperNum", DisplayLabel = "單號", SerialNum = 1, DataType = "text" },
                new { FieldName = "ExpStkTime", DisplayLabel = "期望繳庫日", SerialNum = 2, DataType = "date" },
                new { FieldName = "PartNum", DisplayLabel = "料號", SerialNum = 3, DataType = "text" },
                new { FieldName = "Revision", DisplayLabel = "版序", SerialNum = 4, DataType = "text" },
                new { FieldName = "LotNum", DisplayLabel = "批號", SerialNum = 5, DataType = "text" },
                new { FieldName = "StrL_LLpiece", DisplayLabel = "排版數", SerialNum = 6, DataType = "text" },
                new { FieldName = "Qnty", DisplayLabel = "數量", SerialNum = 7, DataType = "number" },
                new { FieldName = "PopName", DisplayLabel = "形狀", SerialNum = 8, DataType = "text" },
                new { FieldName = "ProcCode", DisplayLabel = "製程", SerialNum = 9, DataType = "text" },
                new { FieldName = "ProcName", DisplayLabel = "製程名稱", SerialNum = 10, DataType = "text" },
                new { FieldName = "AftProcNameString", DisplayLabel = "後續製程名稱", SerialNum = 11, DataType = "text" }
            };

            // 使用 SQL 直接插入，避開 EF Core 的驗證問題
            foreach (var fieldDef in fieldDefinitions)
            {
                var sql = @"
                    INSERT INTO CURdTableField
                    (TableName, FieldName, DisplayLabel, SerialNum, Visible, DataType,
                     iFieldWidth, iShowWhere, ComboTextSize, IsMoneyField, bShow4Money,
                     ReadOnly, PK, FK, IsNeed)
                    VALUES
                    (@TableName, @FieldName, @DisplayLabel, @SerialNum, 1, @DataType,
                     120, 3, 0, 0, 0,
                     0, 0, 0, 0)";

                await _context.Database.ExecuteSqlRawAsync(sql,
                    new Microsoft.Data.SqlClient.SqlParameter("@TableName", tableName),
                    new Microsoft.Data.SqlClient.SqlParameter("@FieldName", fieldDef.FieldName),
                    new Microsoft.Data.SqlClient.SqlParameter("@DisplayLabel", fieldDef.DisplayLabel),
                    new Microsoft.Data.SqlClient.SqlParameter("@SerialNum", fieldDef.SerialNum),
                    new Microsoft.Data.SqlClient.SqlParameter("@DataType", fieldDef.DataType)
                );
            }

            return Ok(new { message = "欄位定義已成功初始化", count = fieldDefinitions.Length });
        }

        private string GetDataType(Type type)
        {
            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            if (underlyingType == typeof(string)) return "text";
            if (underlyingType == typeof(int)) return "number";
            if (underlyingType == typeof(decimal)) return "number";
            if (underlyingType == typeof(float)) return "number";
            if (underlyingType == typeof(double)) return "number";
            if (underlyingType == typeof(DateTime)) return "date";
            if (underlyingType == typeof(bool)) return "checkbox";

            return "text";
        }

        // GET: api/FmedVProcNisToStd/debug-fields
        [HttpGet("debug-fields")]
        public async Task<IActionResult> DebugFields()
        {
            var tableName = "FMEdV_ProcNIS_ToStd";

            var fields = await _context.CURdTableFields
                .Where(x => x.TableName == tableName && x.Visible == 1)
                .OrderBy(x => x.SerialNum)
                .Select(x => new {
                    x.FieldName,
                    x.DisplayLabel,
                    x.SerialNum,
                    x.Visible,
                    x.DataType,
                    x.FormatStr,
                    x.iShowWhere
                })
                .ToListAsync();

            return Ok(new {
                tableName,
                totalFields = fields.Count,
                fields
            });
        }

        // POST: api/FmedVProcNisToStd/InitializeQueryFields
        [HttpPost("InitializeQueryFields")]
        public async Task<IActionResult> InitializeQueryFields()
        {
            var tableName = "FMEdV_ProcNIS_ToStd";

            // 檢查是否已有查詢欄位設定
            var existingCount = await _context.CURdPaperSelected
                .CountAsync(x => x.TableName == tableName);

            if (existingCount > 0)
            {
                return Ok(new { message = $"查詢欄位定義已存在，共 {existingCount} 筆", count = existingCount });
            }

            // 定義查詢欄位
            var queryFields = new[]
            {
                new { ColumnName = "PaperNum", ColumnCaption = "製令單號", DataType = "text", ControlType = 1, SortOrder = 1, DefaultEqual = "Contains" },
                new { ColumnName = "PartNum", ColumnCaption = "品號", DataType = "text", ControlType = 1, SortOrder = 2, DefaultEqual = "Contains" },
                new { ColumnName = "ProcCode", ColumnCaption = "製程", DataType = "text", ControlType = 1, SortOrder = 3, DefaultEqual = "Contains" }
            };

            int insertedCount = 0;
            foreach (var field in queryFields)
            {
                await _context.Database.ExecuteSqlRawAsync(@"
                    INSERT INTO CURdPaperSelected
                    (TableName, ColumnName, ColumnCaption, DataType, ControlType, SortOrder, IVisible, DefaultEqual)
                    VALUES
                    (@TableName, @ColumnName, @ColumnCaption, @DataType, @ControlType, @SortOrder, 1, @DefaultEqual)",
                    new Microsoft.Data.SqlClient.SqlParameter("@TableName", tableName),
                    new Microsoft.Data.SqlClient.SqlParameter("@ColumnName", field.ColumnName),
                    new Microsoft.Data.SqlClient.SqlParameter("@ColumnCaption", field.ColumnCaption),
                    new Microsoft.Data.SqlClient.SqlParameter("@DataType", field.DataType),
                    new Microsoft.Data.SqlClient.SqlParameter("@ControlType", field.ControlType),
                    new Microsoft.Data.SqlClient.SqlParameter("@SortOrder", field.SortOrder),
                    new Microsoft.Data.SqlClient.SqlParameter("@DefaultEqual", field.DefaultEqual)
                );
                insertedCount++;
            }

            return Ok(new { message = "查詢欄位定義已成功初始化", count = insertedCount });
        }

        [HttpGet("pivot-data")]
        public async Task<IActionResult> GetPivotData()
        {
            var data = await _context.MindStockCostPn
                .Where(x => x.HisId == "2023.10" && x.MB == 1)
                .Select(x => new {
                    x.PartNum,
                    x.SouQnty,
                    x.SouCost,
                    x.InQnty,
                    x.InCost,
                    x.ElseInCost,
                    x.ElseInQnty,
                    x.RejQnty,
                    x.RejCost,
                    x.BalQnty,
                    x.BalCost,
                    x.SaleQnty,
                    x.SaleCost,
                    x.OtherOutQnty,
                    x.OtherOutCost,
                    x.BackQnty,
                    x.BackCost,
                    x.ScrapQnty,
                    x.ScrapCost,
                    x.ElseOutQnty,
                    x.ElseOutCost,
                    x.EndQnty,
                    x.EndCost,
                    x.Back4NetQnty,
                    x.Back4NetCost,
                    x.FGInQnty,
                    x.FGInCost,
                    x.SalesReturnQnty,
                    x.SalesReturnCost,
                    x.FarmInQnty,
                    x.FarmInCost,
                    x.FarmOutQnty,
                    x.FarmOutCost,
                    x.UnitCost
                })
                .ToListAsync();

            return Ok(data);
        }
    }
}
