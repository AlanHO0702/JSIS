namespace PcbErpApi.Models
{
    public class QueryFieldViewModel
    {
        public string ColumnName { get; set; }
        public string ColumnCaption { get; set; }
        public int DataType { get; set; }
        public int ControlType { get; set; }
        public string? EditMask { get; set; }
        public string? DefaultValue { get; set; }
        public string? DefaultEqual { get; set; }
        public int? SortOrder { get; set; }
    }
}
