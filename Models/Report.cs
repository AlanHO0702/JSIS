namespace PcbErpApi.Models
{

    public enum ParamUiType { Text, Date, Select, Datetime, Number }

    public class ParamDef
    {
        public string Name { get; set; } = default!;
        public string Label { get; set; } = default!;
        public ParamUiType Ui { get; set; } = ParamUiType.Text;

        // A-1：保留原來的共用 Lookup（用你的 /api/Report/lookup/{key}）
        public string? LookupKey { get; set; }

        // A-2：新增：這份報表「就地」寫死的選項（不打 API）
        // 用 tuple 比較順手，也可以改成 List<ParamOption> 都行
        public List<(string value, string text)>? Options { get; set; }

        public string? DefaultValue { get; set; }
    }

    public class ReportConfig
    {
        public string Title { get; set; } = "";
        public string SpName { get; set; } = "";
        public string DictTableName { get; set; } = "";
        public List<ParamDef> ParamDefs { get; set; } = new();
    }

}
