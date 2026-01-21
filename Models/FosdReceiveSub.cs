using System;
using System.Collections.Generic;

namespace PcbErpApi.Models;

public partial class FosdReceiveSub
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
    public decimal QntyGood { get; set; }
    public decimal QntyLess { get; set; }
    public decimal QntyScrap { get; set; }
    public decimal QntyNG { get; set; }
    public decimal PriceGood { get; set; }
    public decimal PriceLess { get; set; }
    public decimal PriceScrap { get; set; }
    public decimal PriceNG { get; set; }
    public decimal SubTotal { get; set; }
    public string? TunePaperNum { get; set; }
    public string? DivPaperNum { get; set; }
    public int Invoiced { get; set; }
    public int? beTranTo_LPcs { get; set; }
    public decimal? WPnlQty { get; set; }
    public string? TranLpcsDivPaperNum { get; set; }
    public DateTime? LastDate { get; set; }
    public string? LastUserId { get; set; }
    public int IQCStatus { get; set; }
    public string? IQCPaperNum { get; set; }
    public int? IQCSerialNum { get; set; }
    public int IsOK { get; set; }
    public decimal BasUp { get; set; }
    public string? CmpSize { get; set; }
    public string? fromPaperId { get; set; }
    public string? PartSerial { get; set; }
    public string? WorkItem { get; set; }
    public string? SourPaperId { get; set; }
    public string? PrjId { get; set; }
    public int? PrPOP { get; set; }
    public decimal? PrQnty { get; set; }
    public decimal PrUnitPrice { get; set; }
    public decimal? IssueLength { get; set; }
    public decimal? IssueWidth { get; set; }
    public decimal? CNCLength { get; set; }
    public decimal? CNCWidth { get; set; }
    public decimal? TranMatQnty { get; set; }
    public int? iIsSysUpdate { get; set; }
    public decimal? TaxPrice { get; set; }
    public decimal? TaxPriceSubTotal { get; set; }
    public decimal? TaxPrUnitPrice { get; set; }
    public DateTime? OsCheckDate { get; set; }
    public int? iNeedQC { get; set; }
    public int? OsCheckType { get; set; }
    public int? iEnhance { get; set; }
    public int? iCycleM { get; set; }
    public int? iCycleN { get; set; }
    public int? iCountRec { get; set; }
    public int? iCountEnhance { get; set; }
    public string? DateCode { get; set; }
    public decimal FinishQnty { get; set; }
    public decimal? Up_Org { get; set; }
    public string? PassPaperNum { get; set; }
    public int? AllHoleSum2 { get; set; }
    public int? iCountStrict { get; set; }
}
