// /Pages/Report/GenericReport.cshtml.cs
using Microsoft.AspNetCore.Mvc.RazorPages;
using PcbErpApi.Data;
using PcbErpApi.Models;   // 這裡放上面的 ReportConfig/ParamDef 類別

public class GenericReportPageModel : PageModel
{
    private readonly PcbErpContext _ctx;
    public ReportConfig Config { get; set; } = new();
    public List<CURdTableField> FieldDictList { get; set; } = new();
    public List<TableFieldViewModel> TableFields { get; set; } = new();

    public GenericReportPageModel(PcbErpContext ctx) => _ctx = ctx;

    public void OnGet()
    {
        // 這個頁面本身不決定 Config，由「個別報表頁」在 OnGet 的 Razor 裡設定。
        // 這裡只負責：根據 Config.DictTableName 抓辭典，產出顯示欄位給前端。

        var baseFields = _ctx.CURdTableFields
            .Where(f => f.TableName == Config.DictTableName)
            .ToList();

        var langMap = _ctx.CurdTableFieldLangs
            .Where(l => l.TableName == Config.DictTableName && l.LanguageId == "TW")
            .ToDictionary(l => l.FieldName, l => l.DisplayLabel ?? "");

        foreach (var f in baseFields)
            if (langMap.TryGetValue(f.FieldName!, out var lab) && !string.IsNullOrWhiteSpace(lab))
                f.DisplayLabel = lab;

        FieldDictList = baseFields
            .OrderBy(x => x.SerialNum ?? int.MaxValue).ThenBy(x => x.FieldName)
            .ToList();

        TableFields = baseFields
            .Where(x => x.Visible == 1)
            .OrderBy(x => x.SerialNum ?? int.MaxValue).ThenBy(x => x.FieldName)
            .Select(x => new TableFieldViewModel
            {
                FieldName = x.FieldName,
                DisplayLabel = x.DisplayLabel,
                SerialNum = x.SerialNum ?? 0,
                Visible = x.Visible == 1,
                DataType = x.DataType,
                FormatStr = x.FormatStr
            }).ToList();
    }
    public void LoadDict(string dictTableName)
{
    var baseFields = _ctx.CURdTableFields
        .Where(f => f.TableName == dictTableName)
        .ToList();

    var langMap = _ctx.CurdTableFieldLangs
        .Where(l => l.TableName == dictTableName && l.LanguageId == "TW")
        .ToDictionary(l => l.FieldName, l => l.DisplayLabel ?? "");

    foreach (var f in baseFields)
        if (langMap.TryGetValue(f.FieldName!, out var lab) && !string.IsNullOrWhiteSpace(lab))
            f.DisplayLabel = lab;

    FieldDictList = baseFields
        .OrderBy(x => x.SerialNum ?? int.MaxValue).ThenBy(x => x.FieldName)
        .ToList();

    TableFields = baseFields
        .Where(x => x.Visible == 1)
        .OrderBy(x => x.SerialNum ?? int.MaxValue).ThenBy(x => x.FieldName)
        .Select(x => new TableFieldViewModel {
            FieldName    = x.FieldName,
            DisplayLabel = x.DisplayLabel,
            SerialNum    = x.SerialNum ?? 0,
            Visible      = x.Visible == 1,
            DataType     = x.DataType,
            FormatStr    = x.FormatStr
        }).ToList();
}
}
