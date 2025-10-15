using System;
using System.Collections.Generic;

namespace PcbErpApi.Models;

public partial class CurdPaperPaper
{
    public string PaperId { get; set; } = null!;

    public int SerialNum { get; set; }

    public string? ItemName { get; set; }

    public int Enabled { get; set; }

    public string? Notes { get; set; }

    public string? ClassName { get; set; }

    public string? ObjectName { get; set; }

    public int LinkType { get; set; }

    public int DisplayType { get; set; }

    public int OutputType { get; set; }

    public int ShowTitle { get; set; }

    public int ShowTree { get; set; }

    public string? TableIndex { get; set; }

    public int ItemCount { get; set; }

    public string? PrintItemId { get; set; }
}
