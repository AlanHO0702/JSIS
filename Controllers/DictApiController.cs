using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient; 
using PcbErpApi.Data;
using PcbErpApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using static PcbErpApi.Pages.EMOdProdInfo.IndexModel;

[Route("api/[controller]/[action]")]
[ApiController]
public class DictApiController : ControllerBase
{
    private readonly string _connStr;

    public DictApiController(PcbErpContext context, IConfiguration config)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config));
        _connStr = config.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    // ✅ 更新辭典欄位（含所有版面欄位）
    [HttpPost]
    public async Task<IActionResult> UpdateDictFields([FromBody] List<UpdateDictFieldInput> list)
    {
        using (var conn = new SqlConnection(_connStr))
        {
            await conn.OpenAsync();

            foreach (var input in list)
            {
                var cmd = new SqlCommand();
                var sql = "UPDATE CURdTableField SET ";
                var setList = new List<string>();

                cmd.Parameters.AddWithValue("@TableName", input.TableName ?? "");
                cmd.Parameters.AddWithValue("@FieldName", input.FieldName ?? "");

                // ===== 原本欄位 =====
                if (input.DisplayLabel != null)
                {
                    setList.Add("DisplayLabel = @DisplayLabel");
                    cmd.Parameters.AddWithValue("@DisplayLabel", input.DisplayLabel);
                }
                if (input.DataType != null)
                {
                    setList.Add("DataType = @DataType");
                    cmd.Parameters.AddWithValue("@DataType", input.DataType);
                }
                if (input.FieldNote != null)
                {
                    setList.Add("FieldNote = @FieldNote");
                    cmd.Parameters.AddWithValue("@FieldNote", input.FieldNote);
                }
                if (input.SerialNum != null)
                {
                    setList.Add("SerialNum = @SerialNum");
                    cmd.Parameters.AddWithValue("@SerialNum", input.SerialNum);
                }
                if (input.Visible != null)
                {
                    setList.Add("Visible = @Visible");
                    cmd.Parameters.AddWithValue("@Visible", input.Visible);
                }
                if (input.FormatStr != null)
                {
                    setList.Add("FormatStr = @FormatStr");
                    cmd.Parameters.AddWithValue("@FormatStr", input.FormatStr);
                }
                if (input.LookupResultField != null)
                {
                    setList.Add("LookupResultField = @LookupResultField");
                    cmd.Parameters.AddWithValue("@LookupResultField", input.LookupResultField);
                }

                // ===== 新增欄位：版面設定 =====
                void AddInt(string name, int? value)
                {
                    if (value.HasValue)
                    {
                        setList.Add($"{name} = @{name}");
                        cmd.Parameters.AddWithValue("@" + name, value.Value);
                    }
                }
                void AddStr(string name, string? value)
                {
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        setList.Add($"{name} = @{name}");
                        cmd.Parameters.AddWithValue("@" + name, value);
                    }
                }

                AddInt("DisplaySize", input.DisplaySize);
                AddInt("iLabHeight", input.iLabHeight);
                AddInt("iLabTop", input.iLabTop);
                AddInt("iLabLeft", input.iLabLeft);
                AddInt("iLabWidth", input.iLabWidth);
                AddInt("iFieldHeight", input.iFieldHeight);
                AddInt("iFieldTop", input.iFieldTop);
                AddInt("iFieldLeft", input.iFieldLeft);
                AddInt("iFieldWidth", input.iFieldWidth);

                AddStr("LookupTable", input.LookupTable);
                AddStr("LookupKeyField", input.LookupKeyField);
                AddStr("IsNotesField", input.IsNotesField);

                if (setList.Count == 0) continue; // 沒有要更新的欄位就跳過

                sql += string.Join(", ", setList);
                sql += " WHERE TableName = @TableName AND FieldName = @FieldName";
                cmd.CommandText = sql;
                cmd.Connection = conn;

                await cmd.ExecuteNonQueryAsync();
            }
        }

        return Ok(new { success = true });
    }

    public class DictLayoutInput
    {
        public string FieldName { get; set; }
        public int? iShowWhere { get; set; }
        public int? iLayRow { get; set; }
        public int? iLayColumn { get; set; }
        public int? iFieldWidth { get; set; }
        public int? iFieldHeight { get; set; }
        public int? iFieldTop { get; set; }
        public int? iFieldLeft { get; set; }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateDictFieldsLayout([FromBody] List<DictLayoutInput> list)
    {
        using (var conn = new SqlConnection(_connStr))
        {
            await conn.OpenAsync();
            foreach (var input in list)
            {
                var cmd = new SqlCommand(@"
                    UPDATE CURdTableField 
                    SET iShowWhere = @iShowWhere, iLayRow = @iLayRow, iLayColumn = @iLayColumn, 
                        iFieldWidth = @iFieldWidth, iFieldHeight = @iFieldHeight,
                        iFieldTop = @iFieldTop, iFieldLeft = @iFieldLeft
                    WHERE FieldName = @FieldName", conn);

                cmd.Parameters.AddWithValue("@iShowWhere", input.iShowWhere.HasValue ? input.iShowWhere.Value : (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@iLayRow", input.iLayRow.HasValue ? input.iLayRow.Value : (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@iLayColumn", input.iLayColumn.HasValue ? input.iLayColumn.Value : (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@iFieldWidth", input.iFieldWidth.HasValue ? input.iFieldWidth.Value : (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@iFieldHeight", input.iFieldHeight.HasValue ? input.iFieldHeight.Value : (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@iFieldTop", input.iFieldTop.HasValue ? input.iFieldTop.Value : (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@iFieldLeft", input.iFieldLeft.HasValue ? input.iFieldLeft.Value : (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@FieldName", input.FieldName ?? "");

                await cmd.ExecuteNonQueryAsync();
            }
        }
        return Ok();
    }
}
