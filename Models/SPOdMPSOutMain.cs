using System;
using System.Collections.Generic;

namespace PcbErpApi.Models;

public partial class SPOdMPSOutMain
{
    public string PaperNum { get; set; } = null!;

    public DateTime PaperDate { get; set; }

    public int Status { get; set; }

    public int Finished { get; set; }

    public string? UserId { get; set; }

    public DateTime? BuildDate { get; set; }

    public string? FinishUser { get; set; }

    public DateTime? FinishDate { get; set; }

    public string? CancelUser { get; set; }

    public DateTime? CancelDate { get; set; }

    public string? UseId { get; set; }

    public string? PaperId { get; set; }

    public string? Notes { get; set; }

    public string CustomerId { get; set; } = null!;

    public int PayWayCode { get; set; }

    public byte MoneyCode { get; set; }

    public decimal RateToNt { get; set; }

    public decimal SubTotal { get; set; }

    public int InvoiceType { get; set; }

    public decimal Tax { get; set; }

    public decimal Total { get; set; }

    public string AreaCode { get; set; } = null!;

    public string? ShipTo { get; set; }

    public string? OutAddr { get; set; }

    public string? IndirectAddr { get; set; }

    public string ShipTerm { get; set; } = null!;

    public string FdrCode { get; set; } = null!;

    public string? PkgTitle { get; set; }

    public string? InvoiceNum { get; set; }

    public int? Box { get; set; }

    public string? PosId { get; set; }

    public string TransTypeCode { get; set; } = null!;

    public DateTime? InvoiceDate { get; set; }

    public DateTime? ExpectDate { get; set; }

    public string? Notify { get; set; }

    public string? Forwarder { get; set; }

    public string? OutWay { get; set; }

    public byte IsReOut { get; set; }

    public string? ChkNum { get; set; }

    public string? Assistant { get; set; }

    public string? Cycle { get; set; }

    public int FlowStatus { get; set; }

    public string? SourNum2 { get; set; }

    public string? ArchivesNo { get; set; }

    public string? PrjId { get; set; }

    public string? NotesVarch1 { get; set; }

    public string? NotesVarch2 { get; set; }

    public string? NotesVarch3 { get; set; }

    public string? NotesVarch4 { get; set; }

    public string? NotesVarch5 { get; set; }

    public string? NotesVarch6 { get; set; }

    public string? NotesVarch7 { get; set; }

    public string? NotesVarch8 { get; set; }

    public string? NotesVarch9 { get; set; }

    public string? NotesVarch10 { get; set; }

    public decimal? NotesDecim1 { get; set; }

    public decimal? NotesDecim2 { get; set; }

    public decimal? NotesDecim3 { get; set; }

    public decimal? NotesDecim4 { get; set; }

    public decimal? NotesDecim5 { get; set; }

    public decimal? NotesDecim6 { get; set; }

    public decimal? NotesDecim7 { get; set; }

    public decimal? NotesDecim8 { get; set; }

    public decimal? NotesDecim9 { get; set; }

    public decimal? NotesDecim10 { get; set; }

    public string? DepartId { get; set; }

    public string? SalesId { get; set; }

    public string? CustPonum { get; set; }

    public int? DllPaperType { get; set; }

    public string? DllPaperTypeName { get; set; }

    public string? DllHeadFirst { get; set; }

    public DateTime? UpdateDate { get; set; }

    public DateTime? PreOutDate { get; set; }

    public string? Port { get; set; }

    public string? InvoiceNotes { get; set; }

    public int UseTaxPrice { get; set; }

    public string? TradeId { get; set; }

    public string? TradePaperNum { get; set; }

    public string? TradePaperId { get; set; }

    public decimal Volume { get; set; }

    public string? FwdNum { get; set; }

    public string? ExportType { get; set; }

    public decimal TotalAmountOg { get; set; }

    public string? Driver { get; set; }

    public string? FromNum { get; set; }

    public int? IsPnlprice { get; set; }

    public byte? Commerce { get; set; }

    public DateTime? Dodate { get; set; }

    public decimal DepositTotal { get; set; }

    public decimal DepositTotalTax { get; set; }

    public string? SalesOutNum { get; set; }
}
