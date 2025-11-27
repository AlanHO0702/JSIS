public class ReportDirectConfig
{
    public string Title { get; set; } = "";
    public string SpName { get; set; } = "";      // 要先跑的 SP
    public string ReportName { get; set; } = "";  // rpt 檔名（不含 .rpt）
    public List<ParamSpec> Params { get; set; } = new();
    public Dictionary<string, string>? ExtraParams { get; set; } = new(); // 不顯示在 UI、但送出 payload 要帶的預設參數
}

public class ParamSpec
{
    public string Name { get; set; } = "";   // 不要加 @，後端會自己補
    public string Label { get; set; } = "";
    public string Ui { get; set; } = "text"; // text | select | date | number
    public string? SuperId { get; set; } // parent label (if any)
    public string? DefaultValue { get; set; }
    public string? LookupKey { get; set; }   // 若用 lookup API，填 key 即可
    public List<(string value, string text)> Options { get; set; } = new(); // select 用（非 lookup）
}

