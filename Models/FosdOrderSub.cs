using System;
using System.Collections.Generic;

namespace PcbErpApi.Models;

public partial class FosdOrderSub
{
    public string PaperNum { get; set; } = null!;
    public int Item { get; set; }
    public string? SourNum { get; set; }
    public int? SourItem { get; set; }
    public string? Notes { get; set; }
    public string StockId { get; set; } = null!;
    public string LotNum { get; set; } = null!;
    public string PartNum { get; set; } = null!;
    public string Revision { get; set; } = null!;
    public string LayerId { get; set; } = null!;
    public int POP { get; set; }
    public decimal Qnty { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal SubTotal { get; set; }
    public DateTime? DelDate { get; set; }
    public decimal FinishQnty { get; set; }
    public string? DivPaperNum { get; set; }
    public int LotStatus { get; set; }
    public int Canceled { get; set; }
    public string? CancelUser { get; set; }
    public DateTime? CancelDate { get; set; }
    public string? PressLotNum { get; set; }
    public string? AftProc { get; set; }
    public string? AftLayer { get; set; }
    public DateTime? LastDate { get; set; }
    public string? LastUserId { get; set; }
    public decimal BasUp { get; set; }
    public string? OrderTuneNum { get; set; }
    public string? CmpSize { get; set; }
    public string? PartSerial { get; set; }
    public string? WorkItem { get; set; }
    public string? SourPaperId { get; set; }
    public string? PrjId { get; set; }
    public decimal? TranMatQnty { get; set; }
    public decimal? TaxPrice { get; set; }
    public decimal? TaxPriceSubTotal { get; set; }
    public string? WPNLPOP2 { get; set; }
    public string? WPNLPOP3 { get; set; }
    public string? SPNLPOP4 { get; set; }
    public decimal? PrQnty { get; set; }
    public int? PrPOP { get; set; }
}
