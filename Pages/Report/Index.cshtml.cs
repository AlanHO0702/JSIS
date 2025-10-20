using Microsoft.AspNetCore.Mvc.RazorPages;
using PcbErpApi.Data;
using PcbErpApi.Models;
using System.Linq;

public class ReportPageModel : PageModel
{
    private readonly PcbErpContext _ctx;

    // 這頁報表對應的辭典表名
    public string DictTableName => "SA000048";

    // 給 F3 modal
    public List<CURdTableField> FieldDictList { get; set; } = new();

    // 給表格（只取可視欄位）
    public List<TableFieldViewModel> TableFields { get; set; } = new();

    public ReportPageModel(PcbErpContext ctx) => _ctx = ctx;

    public void OnGet()
    {
        // 1) 取基礎欄位
        var baseFields = _ctx.CURdTableFields
            .Where(f => f.TableName == DictTableName)
            .ToList();

        // 2) 取語系（TW）顯示名稱
        var langMap = _ctx.CurdTableFieldLangs
            .Where(l => l.TableName == DictTableName && l.LanguageId == "TW")
            .ToDictionary(l => l.FieldName, l => l.DisplayLabel ?? "");

        // 3) 合併中文顯示
        foreach (var f in baseFields)
        {
            if (langMap.TryGetValue(f.FieldName!, out var label) && !string.IsNullOrWhiteSpace(label))
                f.DisplayLabel = label;
        }

        // 提供給 F3 的完整清單
        // 給 F3 modal 用的完整清單，也照 SerialNum 排好（空的排最後）
        FieldDictList = baseFields
            .OrderBy(x => x.SerialNum ?? int.MaxValue)
            .ThenBy(x => x.FieldName)
            .ToList();

        // 提供給表格的「可視欄位」
        TableFields = baseFields
            .Where(x => x.Visible == 1)
            .OrderBy(x => x.SerialNum ?? int.MaxValue)
            .ThenBy(x => x.FieldName)
            .Select(x => new TableFieldViewModel
            {
                FieldName    = x.FieldName,
                DisplayLabel = x.DisplayLabel,  // 已覆蓋中文
                SerialNum    = x.SerialNum ?? 0,
                Visible      = x.Visible == 1,
                DataType     = x.DataType,
                FormatStr    = x.FormatStr
            })
            .ToList();

        ViewData["Fields"]        = TableFields;          // 供前端畫表
        ViewData["DictTableName"] = DictTableName;        // 供 F3 / JS
    }
}
