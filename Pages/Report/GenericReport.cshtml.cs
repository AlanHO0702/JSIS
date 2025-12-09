// /Pages/Report/GenericReport.cshtml.cs
using System;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore; // 確認頂部有引入
using PcbErpApi.Data;
using Microsoft.Data.SqlClient;
using System.Data.Common;
using System.IO;
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
    

    public ReportDirectConfig BuildDirectConfigFromDb(string itemId, int? expectedItemType = null, int? expectedOutputType = null)
    {
        var item = _ctx.CurdSysItems
            .AsNoTracking()
            .Where(x => x.ItemId == itemId)
            .Select(x => new { x.ItemId, x.ItemName, x.ClassName, x.ObjectName, x.ItemType, x.OutputType })
            .SingleOrDefault() ?? throw new InvalidOperationException($"Item {itemId} not found.");

        if (expectedItemType.HasValue && item.ItemType != expectedItemType.Value)
            throw new InvalidOperationException($"Item {itemId} not allowed (ItemType mismatch).");
        if (expectedOutputType.HasValue && item.OutputType != expectedOutputType.Value)
            throw new InvalidOperationException($"Item {itemId} not allowed (OutputType mismatch).");

        var cfg = new ReportDirectConfig {
            Title      = $"{item.ItemId}{item.ItemName}",
            SpName     = item.ObjectName ?? "",
            ReportName = Path.GetFileNameWithoutExtension(item.ClassName) ?? "", // 去掉 .rpt,
        };

        var addonParams = _ctx.CurdAddonParams
            .AsNoTracking()
            .Where(p => p.ItemId == itemId)
            .OrderBy(p => p.ParamSn)
            .ToList();

        var paramSpecs = new List<ParamSpec>();
        var extras = new Dictionary<string,string>();

        foreach (var p in addonParams)
        {
            var name = (p.ParamName ?? "").TrimStart('@');
            var label = p.DisplayName?.Trim() ?? "";
            var defaultVal = ResolveDefaultValue(p);

            if (string.IsNullOrWhiteSpace(label))
            {
                // DisplayName 為空 → 放 ExtraParams
                extras[name] = defaultVal;
                continue;
            }

            paramSpecs.Add(new ParamSpec {
                Name = name,
                Label = label,
                DefaultValue = defaultVal,
                SuperId = p.SuperId,
                Ui = string.IsNullOrWhiteSpace(p.CommandText)
                    ? (LooksLikeDate(name, label, defaultVal) ? "date" : "text")
                    : "select",
                // 若有 CommandText，給 lookup key，下面的 API 負責執行 CommandText 取 value/text
                LookupKey = string.IsNullOrWhiteSpace(p.CommandText) ? null : $"db:{itemId}:{name}"
            });
        }

        cfg.Params = paramSpecs;
        cfg.ExtraParams = extras;
        return cfg;
    }


    public ReportConfig BuildReportConfigFromDb(string itemId, int? expectedItemType = null, int? expectedOutputType = null)
    {
        var item = _ctx.CurdSysItems
            .AsNoTracking()
            .Where(x => x.ItemId == itemId)
            .Select(x => new { x.ItemId, x.ItemName, x.ClassName, x.ObjectName, x.ItemType, x.OutputType })
            .SingleOrDefault() ?? throw new InvalidOperationException($"Item {itemId} not found.");

        if (expectedItemType.HasValue && item.ItemType != expectedItemType.Value)
            throw new InvalidOperationException($"Item {itemId} not allowed (ItemType mismatch).");
        if (expectedOutputType.HasValue && item.OutputType != expectedOutputType.Value)
            throw new InvalidOperationException($"Item {itemId} not allowed (OutputType mismatch).");

        var cfg = new ReportConfig {
            Title         = $"{item.ItemId}{item.ItemName}",
            SpName        = item.ObjectName ?? "",
            ReportName    = Path.GetFileNameWithoutExtension(item.ClassName) ?? "",
            DictTableName = item.ItemId
        };

        var addonParams = _ctx.CurdAddonParams
            .AsNoTracking()
            .Where(p => p.ItemId == itemId)
            .OrderBy(p => p.ParamSn)
            .ToList();

        var paramDefs = new List<ParamDef>();
        var extras = new Dictionary<string,string>();

        foreach (var p in addonParams)
        {
            var name = (p.ParamName ?? "").TrimStart('@');
            var label = p.DisplayName?.Trim() ?? "";
            var defaultVal = ResolveDefaultValue(p);

            if (string.IsNullOrWhiteSpace(label))
            {
                extras[name] = defaultVal;
                continue;
            }

            paramDefs.Add(new ParamDef {
                Name = name,
                Label = label,
                DefaultValue = defaultVal,
                Ui = string.IsNullOrWhiteSpace(p.CommandText)
                    ? (LooksLikeDate(name, label, defaultVal) ? ParamUiType.Date : ParamUiType.Text)
                    : ParamUiType.Select,
                LookupKey = string.IsNullOrWhiteSpace(p.CommandText) ? null : $"db:{itemId}:{name}"
            });
        }

        cfg.ParamDefs = paramDefs;
        cfg.ExtraParams = extras;
        return cfg;
    }

    private static bool LooksLikeDate(string name, string label, string defaultVal)
    {
        bool hasDateKeyword = name.Contains("date", StringComparison.OrdinalIgnoreCase)
                              || label.Contains("日")
                              || label.Contains("日期")
                              || label.Contains("Date", StringComparison.OrdinalIgnoreCase);
        if (hasDateKeyword) return true;
        return DateTime.TryParse(defaultVal, out _);
    }
    private string ResolveDefaultValue(CurdAddonParam p)
    {
        if (p.DefaultType == 1 && !string.IsNullOrWhiteSpace(p.DefaultValue))
        {
            try
            {
                var cs = _ctx.Database.GetConnectionString();
                if (string.IsNullOrWhiteSpace(cs))
                    return p.DefaultValue ?? "";

                using var conn = new SqlConnection(cs);
                using var cmd = conn.CreateCommand();
                cmd.CommandText = p.DefaultValue;
                conn.Open();
                var result = cmd.ExecuteScalar();
                return result?.ToString() ?? "";
            }
            catch
            {
                return p.DefaultValue ?? "";
            }
        }

        return p.DefaultValue ?? "";
    }


}
    

