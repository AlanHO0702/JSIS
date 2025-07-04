using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

public class IndexModel : PageModel
{
    private readonly IConfiguration _configuration;

    public IndexModel(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public List<string> Columns { get; set; } = new();
    public DataTable TableData { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string TableName { get; set; }

    public string SortColumn { get; set; }
    public string SortDirection { get; set; } = "ASC";

    // ★ 核心：儲存「實體欄位名稱」與「顯示名稱」
    public Dictionary<string, string> FieldLabels { get; set; } = new();

    public async Task OnGetAsync(string? tableName, string? sortColumn, string? sortDirection)
    {
        TableName = tableName;
        SortColumn = sortColumn ?? "";
        SortDirection = sortDirection?.ToUpper() == "DESC" ? "DESC" : "ASC";
        FieldLabels = new Dictionary<string, string>();

        if (!string.IsNullOrWhiteSpace(TableName))
        {
            using (var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await conn.OpenAsync();

                // 1. 取得 Table 的實體欄位清單
                var realColumns = new HashSet<string>();
                using (var columnCmd = new SqlCommand(
                    @"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME=@TableName", conn))
                {
                    columnCmd.Parameters.AddWithValue("@TableName", TableName);
                    using (var reader = await columnCmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            realColumns.Add(reader.GetString(0));
                        }
                    }
                }

                // 2. 取得 CURdTableField 中 Visible=1 欄位設定，**且真的在 Table 存在**
                using (var fieldLabelCmd = new SqlCommand(
                    @"SELECT FieldName, DisplayLabel
                      FROM CURdTableField
                      WHERE TableName = @TableName AND Visible = 1
                      ORDER BY SerialNum", conn))
                {
                    fieldLabelCmd.Parameters.AddWithValue("@TableName", TableName);
                    using (var reader = await fieldLabelCmd.ExecuteReaderAsync())
                    {
                        FieldLabels.Clear();
                        while (await reader.ReadAsync())
                        {
                            var field = reader.GetString(0);
                            var label = reader.IsDBNull(1) ? field : reader.GetString(1);
                            if (realColumns.Contains(field))  // ★ 只取 Table 真實有的欄位
                                FieldLabels[field] = label;
                        }
                    }
                }

                if (FieldLabels.Count > 0)
                {
                    // 3. 組查詢僅包含真實欄位
                    var columns = string.Join(",", FieldLabels.Keys.Select(c => $"[{c}]"));
                    var query = $"SELECT TOP 100 {columns} FROM [{TableName}]";
                    // 排序
                    if (!string.IsNullOrEmpty(SortColumn) && FieldLabels.ContainsKey(SortColumn))
                    {
                        query += $" ORDER BY [{SortColumn}] {SortDirection}";
                    }

                    using var adapter = new SqlDataAdapter(query, conn);
                    var dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    TableData = dataTable;
                    Columns = FieldLabels.Keys.ToList();
                }
            }
        }
    }
}
