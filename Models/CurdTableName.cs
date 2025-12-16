using System;
using System.Collections.Generic;

namespace PcbErpApi.Models;

public partial class CurdTableName
{
    public string TableName { get; set; } = null!;

    public string? DisplayLabel { get; set; }

    public string TableNote { get; set; } = null!;

    public int? SerialNum { get; set; }

    public int? TableType { get; set; }

    public int? LevelNo { get; set; }

    public string SystemId { get; set; } = null!;

    public string? SuperId { get; set; }

    public string? RealTableName { get; set; }

    public string? OrderByField { get; set; }

    public string? DisplayLabelCn { get; set; }

    public string? DisplayLabelEn { get; set; }

    public string? DisplayLabelJp { get; set; }

    public string? DisplayLabelTh { get; set; }

    public string? LogKeildFieldName { get; set; }
}
