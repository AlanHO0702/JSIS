using System;
using System.ComponentModel.DataAnnotations;

namespace PcbErpApi.Models;

public partial class MindStockCostPn
{
    [Key]
    public string HisId { get; set; } = null!;
    public string StockId { get; set; } = null!;
    public string PartNum { get; set; } = null!;
    public string Revision { get; set; } = null!;

    public decimal DealCost { get; set; }
    public decimal DealRate { get; set; }
    public decimal SouQnty { get; set; }
    public decimal InQnty { get; set; }
    public decimal InXOutQnty { get; set; }
    public decimal BorrowQnty { get; set; }
    public decimal TurnLendQnty { get; set; }
    public decimal BackQnty { get; set; }
    public decimal RejQnty { get; set; }
    public decimal LendQnty { get; set; }
    public decimal OtherOutQnty { get; set; }
    public decimal OutQnty { get; set; }
    public decimal TurnBorrowQnty { get; set; }
    public decimal ScrapQnty { get; set; }
    public decimal SaleQnty { get; set; }
    public decimal BalQnty { get; set; }
    public decimal EndQnty { get; set; }

    public decimal SouCost { get; set; }
    public decimal InCost { get; set; }
    public decimal InXOutCost { get; set; }
    public decimal BorrowCost { get; set; }
    public decimal TurnLendCost { get; set; }
    public decimal BackCost { get; set; }
    public decimal RejCost { get; set; }
    public decimal LendCost { get; set; }
    public decimal OtherOutCost { get; set; }
    public decimal OutCost { get; set; }
    public decimal TurnBorrowCost { get; set; }
    public decimal ScrapCost { get; set; }
    public decimal SaleCost { get; set; }
    public decimal BalCost { get; set; }
    public decimal EndCost { get; set; }

    public decimal UnitCost { get; set; }
    public decimal UnitArea { get; set; }
    public decimal? OldEndCost { get; set; }
    public decimal WIPQnty { get; set; }
    public decimal WIPCost { get; set; }

    public decimal ElseInQnty { get; set; }
    public decimal ElseOutQnty { get; set; }
    public decimal ElseInCost { get; set; }
    public decimal ElseOutCost { get; set; }

    public decimal FGInQnty { get; set; }
    public decimal FGInCost { get; set; }
    public decimal FarmInQnty { get; set; }
    public decimal FarmOutQnty { get; set; }
    public decimal FarmInCost { get; set; }
    public decimal FarmOutCost { get; set; }

    public decimal SalesReturnCost { get; set; }
    public decimal SalesReturnQnty { get; set; }

    public decimal Back4NetCost { get; set; }
    public decimal Back4NetQnty { get; set; }

    public decimal? IncludeDiff { get; set; }
    public decimal? IncludeTo { get; set; }

    public int MB { get; set; }
    public int? MaxRev { get; set; }

    public decimal UnitCostOrg { get; set; }
    public decimal? UnitCostLev0 { get; set; }
    public decimal? UnitCostLevN { get; set; }
    public decimal? OrgEndCost { get; set; }

    public decimal? FGInQnty_Normal { get; set; }
    public decimal? FGInQnty_Rework { get; set; }
    public decimal? FGInCost_Normal { get; set; }
    public decimal? FGInCost_Rework { get; set; }
    public decimal? UnitCost_Normal { get; set; }

    public decimal? YX_FG_BeginUp { get; set; }
    public decimal? YX_FG_NormalUp { get; set; }
    public decimal? YX_FG_Up { get; set; }
    public decimal? FGRew_ReInStk_Cost { get; set; }
}
