using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PcbErpApi.Models;

[Table("SpodOrderMain")]
public partial class SpodOrderMain
{
    
    [Key]
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

    public string? Notes { get; set; }

    public string CustomerId { get; set; } = null!;

    public string SourCustomerId { get; set; } = null!;

    public int PayWayCode { get; set; }

    public byte MoneyCode { get; set; }

    public decimal RateToNt { get; set; }

    public decimal SubTotal { get; set; }

    public int InvoiceType { get; set; }

    public decimal Tax { get; set; }

    public decimal Total { get; set; }

    public int Potype { get; set; }

    public int OutNotIn { get; set; }

    public int Pokind { get; set; }

    public string? CustPonum { get; set; }

    public string? SalesId { get; set; }

    public string? Cycle { get; set; }

    public int? CycleDay { get; set; }

    public string? ShipTo { get; set; }

    public string? OutAddr { get; set; }

    public string? IndirectAddr { get; set; }

    public string? ShipTerm { get; set; }

    public string FdrCode { get; set; } = null!;

    public string? PkgTitle { get; set; }

    public int UseIn { get; set; }

    public string? SupplierId { get; set; }

    public string? RejectNum { get; set; }

    public int? IsBack { get; set; }

    public int? IsCm { get; set; }

    public string? Standard { get; set; }

    public string? Standard1 { get; set; }

    public int? Disks { get; set; }

    public int? BluePrint { get; set; }

    public int? Fax { get; set; }

    public int? Sample { get; set; }

    public int? Byte { get; set; }

    public string? Other { get; set; }

    public string? FilePath { get; set; }

    public string? FileName { get; set; }

    public string? Condition { get; set; }

    public string? Pkgtype { get; set; }

    public string? TransPlace { get; set; }

    public int? Approval { get; set; }

    public int? ExRpt { get; set; }

    public int? NegAtive { get; set; }

    public int? CrossSection { get; set; }

    public int? TestTin { get; set; }

    public string? DeliverType { get; set; }

    public string? PaperId { get; set; }

    public byte? IsSite3 { get; set; }

    public int? NonIoupdate { get; set; }

    public byte Isproduce { get; set; }

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

    public int? DllPaperType { get; set; }

    public string? DllPaperTypeName { get; set; }

    public string? DllHeadFirst { get; set; }

    public int? FromType { get; set; }

    public DateTime? UpdateDate { get; set; }

    public string? NotesVarch11 { get; set; }

    public string? NotesVarch12 { get; set; }

    public string? NotesVarch13 { get; set; }

    public string? NotesVarch14 { get; set; }

    public string? NotesVarch15 { get; set; }

    public string? NotesVarch16 { get; set; }

    public string? NotesVarch17 { get; set; }

    public string? NotesVarch18 { get; set; }

    public string? NotesVarch19 { get; set; }

    public string? NotesVarch20 { get; set; }

    public string? InvoiceNotes { get; set; }

    public int UseTaxPrice { get; set; }

    public string? Assistant { get; set; }

    public string? PreOrderNum { get; set; }

    public int? DisTrig { get; set; }

    public DateTime? OutDate { get; set; }

    public string? TradeId { get; set; }

    public string? TradePaperNum { get; set; }

    public string? TradePaperId { get; set; }

    public string? NotesVarch21 { get; set; }

    public string? NotesVarch22 { get; set; }

    public string? NotesVarch23 { get; set; }

    public string? NotesVarch24 { get; set; }

    public string? NotesVarch25 { get; set; }

    public string? NotesVarch26 { get; set; }

    public string? NotesVarch27 { get; set; }

    public string? NotesVarch28 { get; set; }

    public string? NotesVarch39 { get; set; }

    public string? NotesVarch30 { get; set; }

    public byte Commerce { get; set; }

    public string? NotesVarch31 { get; set; }

    public string? NotesVarch32 { get; set; }

    public string? NotesVarch33 { get; set; }

    public string? NotesVarch34 { get; set; }

    public string? NotesVarch35 { get; set; }

    public string? NotesVarch36 { get; set; }

    public string? NotesVarch37 { get; set; }

    public string? NotesVarch38 { get; set; }

    public string? NotesVarch29 { get; set; }

    public string? NotesVarch40 { get; set; }

    public decimal TotalAmountOg { get; set; }

    public int? IsPnlprice { get; set; }

    public int IsConsignment { get; set; }

    public decimal Deducted { get; set; }

    public decimal DepositTotal { get; set; }
}
