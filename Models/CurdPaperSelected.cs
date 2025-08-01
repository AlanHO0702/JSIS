using System;
using System.Collections.Generic;

namespace PcbErpApi.Models;

public partial class CurdPaperSelected
{
    public string PaperId { get; set; } = null!;

    public string TableName { get; set; } = null!;

    public string? AliasName { get; set; }

    public string ColumnName { get; set; } = null!;

    public string ColumnCaption { get; set; } = null!;

    public int DataType { get; set; }

    public int SortOrder { get; set; }

    public string? DefaultValue { get; set; }

    public string DefaultEqual { get; set; } = null!;

    public int? ControlType { get; set; }

    public string? CommandText { get; set; }

    public int DefaultType { get; set; }

    public string? EditMask { get; set; }

    public string? SuperId { get; set; }

    public string? ParamValue { get; set; }

    public int ParamType { get; set; }

    public int IReadOnly { get; set; }

    public int IVisible { get; set; }

    public string? TableKind { get; set; }
}
