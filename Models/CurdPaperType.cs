using System;
using System.Collections.Generic;

namespace PcbErpApi.Models;

public partial class CurdPaperType
{
    public string PaperId { get; set; } = null!;

    public int PaperType { get; set; }

    public string? PaperTypeName { get; set; }

    public string? HeadFirst { get; set; }
}
