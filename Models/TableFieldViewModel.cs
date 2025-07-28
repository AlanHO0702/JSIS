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
    public bool Visible { get; set; } = true;
    public int SerialNum { get; set; }
    public string? DataType { get; set; }
    public string? FormatStr { get; set; }
}
