using System;
using System.Collections.Generic;

namespace PcbErpApi.Models;

public partial class CurdOcxtableSetUp
{
    public string ItemId { get; set; } = null!;

    public string TableName { get; set; } = null!;

    public string TableKind { get; set; } = null!;

    public string? TableShowWere { get; set; }

    public int? FixColCount { get; set; }

    public string? Mdkey { get; set; }

    public string? LocateKeys { get; set; }

    public string? OrderByField { get; set; }

    public string? FilterSql { get; set; }

    public string? RunSqlafterAdd { get; set; }

    public int IsUpdateMoney { get; set; }
}
