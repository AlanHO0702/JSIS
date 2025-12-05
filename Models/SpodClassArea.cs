using System;
using System.Collections.Generic;

namespace PcbErpApi.Models;

public partial class SpodClassArea
{
    public string AreaCode { get; set; } = null!;

    public string? AreaName { get; set; }

    public string? Continent { get; set; }

    public string UseId { get; set; } = null!;
}
