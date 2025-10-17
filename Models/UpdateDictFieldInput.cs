// Models/UpdateDictFieldInput.cs
namespace PcbErpApi.Models
{
    public class UpdateDictFieldInput
    {
        public string TableName { get; set; }
        public string FieldName { get; set; }
        public string DisplayLabel { get; set; }
        public string DataType { get; set; }
        public string FieldNote { get; set; }
        public int? SerialNum { get; set; }
        public int? Visible { get; set; }
        public int? iShowWhere { get; set; }
        public string? LookupResultField { get; set; }
    }
}
