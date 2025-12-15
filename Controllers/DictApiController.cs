using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient; 
using PcbErpApi.Data;
using PcbErpApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

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

    // ⭐ 語系更新語系表的語言代碼
    const string DefaultLang = "TW";

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
                if (input.ComboStyle != null)
                {
                    setList.Add("ComboStyle = @ComboStyle");
                    cmd.Parameters.AddWithValue("@ComboStyle", input.ComboStyle);
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
                if (input.ReadOnly != null)
                {
                    setList.Add("ReadOnly = @ReadOnly");
                    cmd.Parameters.AddWithValue("@ReadOnly", input.ReadOnly);
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

                // ===== Layout / Size 欄位 =====
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
                AddInt("iShowWhere", input.iShowWhere);

                AddStr("LookupTable", input.LookupTable);
                AddStr("LookupKeyField", input.LookupKeyField);
                AddStr("IsNotesField", input.IsNotesField);

                if (setList.Count == 0) continue;

                // === 寫入主表 CURdTableField ===
                sql += string.Join(", ", setList);
                sql += " WHERE TableName = @TableName AND FieldName = @FieldName";
                cmd.CommandText = sql;
                cmd.Connection = conn;
                await cmd.ExecuteNonQueryAsync();

                // === ⭐ 同步：寫入語系表 CURdTableFieldLang (只有 TW) ===
                var langCmd = new SqlCommand(@"
    IF EXISTS (
        SELECT 1 FROM CURdTableFieldLang 
        WHERE TableName=@TableName AND FieldName=@FieldName AND LanguageId=@Lang
    )
    BEGIN
        UPDATE CURdTableFieldLang
        SET 
            DisplayLabel = COALESCE(@DisplayLabel, DisplayLabel),
            DisplaySize  = COALESCE(@DisplaySize, DisplaySize),
            iLabHeight   = COALESCE(@iLabHeight, iLabHeight),
            iLabTop      = COALESCE(@iLabTop, iLabTop),
            iLabLeft     = COALESCE(@iLabLeft, iLabLeft),
            iLabWidth    = COALESCE(@iLabWidth, iLabWidth),
            iFieldHeight = COALESCE(@iFieldHeight, iFieldHeight),
            iFieldTop    = COALESCE(@iFieldTop, iFieldTop),
            iFieldLeft   = COALESCE(@iFieldLeft, iFieldLeft),
            iFieldWidth  = COALESCE(@iFieldWidth, iFieldWidth),
            iShowWhere   = COALESCE(@iShowWhere, iShowWhere)
        WHERE TableName=@TableName AND FieldName=@FieldName AND LanguageId=@Lang
    END
    ELSE
    BEGIN
        INSERT INTO CURdTableFieldLang
        (LanguageId, TableName, FieldName, DisplayLabel, DisplaySize,
        iLabHeight, iLabTop, iLabLeft, iLabWidth,
        iFieldHeight, iFieldTop, iFieldLeft, iFieldWidth, iShowWhere)
        VALUES (
            @Lang, @TableName, @FieldName,
            @DisplayLabel, @DisplaySize,
            @iLabHeight, @iLabTop, @iLabLeft, @iLabWidth,
            @iFieldHeight, @iFieldTop, @iFieldLeft, @iFieldWidth, @iShowWhere
        )
    END
    ", conn);

                langCmd.Parameters.AddWithValue("@Lang", DefaultLang);
                langCmd.Parameters.AddWithValue("@TableName", input.TableName ?? "");
                langCmd.Parameters.AddWithValue("@FieldName", input.FieldName ?? "");

                langCmd.Parameters.AddWithValue("@DisplayLabel", (object?)input.DisplayLabel ?? DBNull.Value);
                langCmd.Parameters.AddWithValue("@DisplaySize", (object?)input.DisplaySize ?? DBNull.Value);

                langCmd.Parameters.AddWithValue("@iLabHeight", (object?)input.iLabHeight ?? DBNull.Value);
                langCmd.Parameters.AddWithValue("@iLabTop", (object?)input.iLabTop ?? DBNull.Value);
                langCmd.Parameters.AddWithValue("@iLabLeft", (object?)input.iLabLeft ?? DBNull.Value);
                langCmd.Parameters.AddWithValue("@iLabWidth", (object?)input.iLabWidth ?? DBNull.Value);

                langCmd.Parameters.AddWithValue("@iFieldHeight", (object?)input.iFieldHeight ?? DBNull.Value);
                langCmd.Parameters.AddWithValue("@iFieldTop", (object?)input.iFieldTop ?? DBNull.Value);
                langCmd.Parameters.AddWithValue("@iFieldLeft", (object?)input.iFieldLeft ?? DBNull.Value);
                langCmd.Parameters.AddWithValue("@iFieldWidth", (object?)input.iFieldWidth ?? DBNull.Value);

                langCmd.Parameters.AddWithValue("@iShowWhere", (object?)input.iShowWhere ?? DBNull.Value);

                await langCmd.ExecuteNonQueryAsync();
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
