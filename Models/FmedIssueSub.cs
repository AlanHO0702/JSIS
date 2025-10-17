using System;
using System.Collections.Generic;

namespace PcbErpApi.Models;

public partial class FmedIssueSub
{
    public string PaperNum { get; set; } = null!;

    public int Item { get; set; }

    public string? SourNum { get; set; }

    public int? SourItem { get; set; }

    public string? Notes { get; set; }

    public string StockId { get; set; } = null!;

    public string ProcCode { get; set; } = null!;

    public string LotNum { get; set; } = null!;

    public string? PartNum { get; set; }

    public string? Revision { get; set; }

    public string LayerId { get; set; } = null!;

    public int Pop { get; set; }

    public decimal Ioqnty { get; set; }

    public int? Potype { get; set; }

    public DateTime? ExpStkTime { get; set; }

    public int Canceled { get; set; }

    public string? CancelUser { get; set; }

    public DateTime? CancelDate { get; set; }

    public int? RouteSerial { get; set; }

    public int? LayerRouteSerial { get; set; }

    public string? SourPaperId { get; set; }

    public int? IsSys { get; set; }
}
