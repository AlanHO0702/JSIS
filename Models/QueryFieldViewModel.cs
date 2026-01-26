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
        public string? CommandText { get; set; }
        public int? DefaultType { get; set; }
        public string? SuperId { get; set; }
        public string? ParamValue { get; set; }
        public int? IReadOnly { get; set; }
        public string? TableKind { get; set; }
        public string? AliasName { get; set; }
        public string? TableName { get; set; }
    }
}
