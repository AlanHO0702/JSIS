public class TableFieldViewModel
{
    public string FieldName { get; set; }
    public string DisplayLabel { get; set; }
    public int? iShowWhere { get; set; }
    public int? iLayRow { get; set; }
    public int? iLayColumn { get; set; }
    public int? iFieldWidth { get; set; }
    public int? iFieldHeight { get; set; }
    public int? iFieldTop { get; set; }
    public int? iFieldLeft { get; set; }
    public int? iLabWidth { get; set; }
    public int? iLabHeight { get; set; }
    public int? iLabTop { get; set; }
    public int? iLabLeft { get; set; }
    public bool Visible { get; set; } = true;
    public int SerialNum { get; set; }
    public string? DataType { get; set; }
    public string? FormatStr { get; set; }
    public string? LookupTable { get; set; }
    public string? LookupKeyField { get; set; }
    public string? LookupResultField { get; set; }
    public int? ReadOnly { get; set; }
    public int? DisplaySize { get; set; }
    public int? ComboStyle { get; set; } // 辭典「勾選框」(CURdTableField.ComboStyle)
    public string? EditColor { get; set; }
}
