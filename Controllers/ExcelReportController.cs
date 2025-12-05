using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ClosedXML.Excel;
using System.Data;

[Route("api/excel")]
[ApiController]
public class ExcelReportController : ControllerBase
{
    private readonly IConfiguration _config;
    public ExcelReportController(IConfiguration config) => _config = config;

    [HttpGet("download")]
    public IActionResult Download()
    {
        string connStr = _config.GetConnectionString("DefaultConnection"); // 你 appsettings.json 裡的

        string sql = @"
SELECT '傑偲資訊有限公司','','','','','','','','','',''
UNION ALL SELECT '明細分類帳','','','','','','','','','',''
UNION ALL SELECT '日期區間: 2025/08/01 到 2025/09/30','','','','','','','','','',''
UNION ALL SELECT '製表日期: 2025/11/26','','','','','','','','','',''
UNION ALL SELECT '傳票日期','傳票號碼','摘要','廠商編號','廠商名稱',
                 '幣別代號','借方金額','貸方金額','借方金額(本)','貸方金額(本)','結餘金額(本)'
UNION ALL SELECT '','','','','','','','','','',''
UNION ALL SELECT '科目代號:6128001  推-薪資支出','','','','','','','','','',''
UNION ALL SELECT '期初金額','','','','','','','','','',''
UNION ALL SELECT '2025/08/01','202508010002','','','','','','','','','';
";

        // 🔹 讀 DB
        DataTable dt = new();
        using (var conn = new SqlConnection(connStr))
        using (var da = new SqlDataAdapter(sql, conn))
            da.Fill(dt);

        // 🔹 產 Excel
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("明細分類帳");
        ws.Cell(1, 1).InsertData(dt);

        int col = dt.Columns.Count;
        for (int r = 1; r <= 4; r++)
        {
            ws.Range(r, 1, r, col).Merge();
            ws.Range(r, 1, r, col).Style.Font.Bold = true;
            ws.Range(r, 1, r, col).Style.Font.FontSize = 14;
            ws.Range(r, 1, r, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        ws.Row(5).Style.Font.Bold = true;

        //=== 欄寬優化設定 ===//

        // 先自動展開
        ws.Columns().AdjustToContents();

        // 再強制放大標題列，避免被字擠壓
        for (int i = 1; i <= col; i++)
        {
            double width = ws.Column(i).Width;

            // 如果自動寬度太小 → 更放大 (保證標題完整可見)
            if (width < 15)
                ws.Column(i).Width = 15;

            if (width > 35)  // 防止金額欄濫爆
                ws.Column(i).Width = 35;
        }

        //=== 第3欄 摘要欄位專屬設定（關鍵）===//

        var colRemark = ws.Column(3);     // C 欄
        colRemark.Width = 40;             // 📌 你可改 50~80 視資料長度
        colRemark.Style.Alignment.WrapText = true;  //允許換行

        // 讓 Excel 自動調整摘要列高度（跑完後執行）
        ws.Rows().AdjustToContents();


        // 讓標題列更醒目
        ws.Row(5).Style.Alignment.WrapText = true; //允許換行顯示
        ws.Row(5).Height = 23; //調整高度讓字不卡


        // 🔹 轉成下載檔案輸出
        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        stream.Position = 0;

        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "明細分類帳.xlsx");
    }
}
