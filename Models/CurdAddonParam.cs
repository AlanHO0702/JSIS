using System;
using System.Collections.Generic;

namespace PcbErpApi.Models;

public partial class CurdAddonParam
{
    public string ItemId { get; set; } = null!;

    public string ParamName { get; set; } = null!;

    public string? DisplayName { get; set; }

    public int? ControlType { get; set; }

    public string? CommandText { get; set; }

    public string? DefaultValue { get; set; }

    public int DefaultType { get; set; }

    public string? EditMask { get; set; }

    public string? SuperId { get; set; }

    public int ParamSn { get; set; }

    public string? DisplayNameCn { get; set; }

    public string? DisplayNameEn { get; set; }

    public string? DisplayNameJp { get; set; }

    public string? DisplayNameTh { get; set; }
}
