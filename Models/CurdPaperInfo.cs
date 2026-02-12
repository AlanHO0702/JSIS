using System;
using System.Collections.Generic;

namespace PcbErpApi.Models;

public partial class CurdPaperInfo
{
    public string PaperId { get; set; } = null!;

    public string PaperName { get; set; } = null!;

    public string SystemId { get; set; } = null!;

    public int EncodeWay { get; set; }

    public string? HeadFirst { get; set; }

    public string? HeadDateFormat { get; set; }

    public int NumLength { get; set; }

    public string? TableName { get; set; }

    public string? PKName { get; set; }

    public int? YearDiff { get; set; }

    public int? RunFlow { get; set; }

    public int? SelectType { get; set; }

    public int? LockPaperDate { get; set; }

    public int? LockUserEdit { get; set; }

    public int? MustNotes { get; set; }
}
