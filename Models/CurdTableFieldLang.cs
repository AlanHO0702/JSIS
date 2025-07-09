using System;
using System.Collections.Generic;

namespace PcbErpApi.Models;

public partial class CurdTableFieldLang
{
    public string LanguageId { get; set; } = null!;

    public string TableName { get; set; } = null!;

    public string FieldName { get; set; } = null!;

    public int? SerialNum { get; set; }

    public string? DisplayLabel { get; set; }

    public int? DisplaySize { get; set; }

    public string? FontName { get; set; }

    public int? FontSize { get; set; }

    public string? FontColor { get; set; }

    public string? FontStyle { get; set; }

    public int IShowWhere { get; set; }

    public int ILabTop { get; set; }

    public int ILabLeft { get; set; }

    public int ILabHeight { get; set; }

    public int ILabWidth { get; set; }

    public int IFieldTop { get; set; }

    public int IFieldLeft { get; set; }

    public int IFieldHeight { get; set; }

    public int IFieldWidth { get; set; }

    public int ILayRow { get; set; }

    public int ILayColumn { get; set; }

    public string? EditColor { get; set; }

    public int? IsBatch { get; set; }

    public string? HintComment { get; set; }

    public int? SubTotal { get; set; }

    public int? ReportGroup { get; set; }

    public int? OrderBy { get; set; }

    public int? Autofit { get; set; }
}
