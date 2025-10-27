public class UpdateDictFieldInput
{
    public string TableName { get; set; }
    public string FieldName { get; set; }
    public int? SerialNum { get; set; }
    public string DisplayLabel { get; set; }
    public int? Visible { get; set; }
    public string DataType { get; set; }
    public string FormatStr { get; set; }
    public string FieldNote { get; set; }

    public int? DisplaySize { get; set; }
    public int? iLabHeight { get; set; }
    public int? iLabTop { get; set; }
    public int? iLabLeft { get; set; }
    public int? iLabWidth { get; set; }
    public int? iFieldHeight { get; set; }
    public int? iFieldTop { get; set; }
    public int? iFieldLeft { get; set; }
    public int? iFieldWidth { get; set; }

    public string LookupTable { get; set; }
    public string LookupKeyField { get; set; }
    public string LookupResultField { get; set; }
    public string IsNotesField { get; set; }
}
