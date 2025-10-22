using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PcbErpApi.Models
{
    public class SPOdMPSOutMain
    {
        [Key]
        [MaxLength(16)]
        public string PaperNum { get; set; } = string.Empty;

        public DateTime PaperDate { get; set; }

        public int Status { get; set; }
        public int Finished { get; set; }

        [MaxLength(16)]
        public string? UserId { get; set; }

        public DateTime? BuildDate { get; set; }

        [MaxLength(16)]
        public string? FinishUser { get; set; }
        public DateTime? FinishDate { get; set; }

        [MaxLength(16)]
        public string? CancelUser { get; set; }
        public DateTime? CancelDate { get; set; }

        [MaxLength(16)]
        public string? UseId { get; set; }

        [MaxLength(32)]
        public string? PaperId { get; set; }

        [MaxLength(510)]
        public string? Notes { get; set; }

        [MaxLength(16)]
        public string CustomerId { get; set; } = string.Empty;

        public int PayWayCode { get; set; }
        public byte MoneyCode { get; set; }

        [Column(TypeName = "decimal(24,8)")]
        public decimal RateToNT { get; set; }

        [Column(TypeName = "decimal(24,8)")]
        public decimal SubTotal { get; set; }

        public int InvoiceType { get; set; }

        [Column(TypeName = "decimal(24,8)")]
        public decimal Tax { get; set; }

        [Column(TypeName = "decimal(24,8)")]
        public decimal Total { get; set; }

        [MaxLength(10)]
        public string AreaCode { get; set; } = string.Empty;

        [MaxLength(510)]
        public string? ShipTo { get; set; }

        [MaxLength(510)]
        public string? OutAddr { get; set; }

        [MaxLength(510)]
        public string? IndirectAddr { get; set; }

        [MaxLength(255)]
        public string ShipTerm { get; set; } = string.Empty;

        [MaxLength(32)]
        public string FdrCode { get; set; } = string.Empty;

        [MaxLength(510)]
        public string? PkgTitle { get; set; }

        [MaxLength(16)]
        public string? InvoiceNum { get; set; }

        public int? Box { get; set; }

        [MaxLength(12)]
        public string? PosId { get; set; }

        [MaxLength(4)]
        public string TransTypeCode { get; set; } = string.Empty;

        public DateTime? InvoiceDate { get; set; }
        public DateTime? ExpectDate { get; set; }

        [MaxLength(510)]
        public string? Notify { get; set; }

        [MaxLength(16)]
        public string? Forwarder { get; set; }

        [MaxLength(32)]
        public string? OutWay { get; set; }

        public byte IsReOut { get; set; }

        [MaxLength(16)]
        public string? ChkNum { get; set; }

        [MaxLength(80)]
        public string? Assistant { get; set; }

        [MaxLength(32)]
        public string? Cycle { get; set; }

        public int FlowStatus { get; set; }

        [MaxLength(16)]
        public string? SourNum2 { get; set; }

        [MaxLength(510)]
        public string? ArchivesNo { get; set; }

        [MaxLength(24)]
        public string? PrjId { get; set; }

        [MaxLength(510)]
        public string? NotesVarch1 { get; set; }
        [MaxLength(510)]
        public string? NotesVarch2 { get; set; }
        [MaxLength(510)]
        public string? NotesVarch3 { get; set; }
        [MaxLength(510)]
        public string? NotesVarch4 { get; set; }
        [MaxLength(510)]
        public string? NotesVarch5 { get; set; }
        [MaxLength(510)]
        public string? NotesVarch6 { get; set; }
        [MaxLength(510)]
        public string? NotesVarch7 { get; set; }
        [MaxLength(510)]
        public string? NotesVarch8 { get; set; }
        [MaxLength(510)]
        public string? NotesVarch9 { get; set; }
        [MaxLength(510)]
        public string? NotesVarch10 { get; set; }

        [Column(TypeName = "decimal(24,8)")]
        public decimal? NotesDecim1 { get; set; }
        [Column(TypeName = "decimal(24,8)")]
        public decimal? NotesDecim2 { get; set; }
        [Column(TypeName = "decimal(24,8)")]
        public decimal? NotesDecim3 { get; set; }
        [Column(TypeName = "decimal(24,8)")]
        public decimal? NotesDecim4 { get; set; }
        [Column(TypeName = "decimal(24,8)")]
        public decimal? NotesDecim5 { get; set; }
        [Column(TypeName = "decimal(24,8)")]
        public decimal? NotesDecim6 { get; set; }
        [Column(TypeName = "decimal(24,8)")]
        public decimal? NotesDecim7 { get; set; }
        [Column(TypeName = "decimal(24,8)")]
        public decimal? NotesDecim8 { get; set; }
        [Column(TypeName = "decimal(24,8)")]
        public decimal? NotesDecim9 { get; set; }
        [Column(TypeName = "decimal(24,8)")]
        public decimal? NotesDecim10 { get; set; }

        [MaxLength(12)]
        public string? DepartId { get; set; }

        [MaxLength(16)]
        public string? SalesId { get; set; }

        [MaxLength(100)]
        public string? CustPONum { get; set; }

        public int? dllPaperType { get; set; }

        [MaxLength(48)]
        public string? dllPaperTypeName { get; set; }

        [MaxLength(4)]
        public string? dllHeadFirst { get; set; }

        public DateTime? UpdateDate { get; set; }
        public DateTime? PreOutDate { get; set; }

        [MaxLength(120)]
        public string? Port { get; set; }

        [MaxLength(100)]
        public string? InvoiceNotes { get; set; }

        public int UseTaxPrice { get; set; }

        [MaxLength(24)]
        public string? TradeId { get; set; }

        [MaxLength(16)]
        public string? TradePaperNum { get; set; }

        [MaxLength(32)]
        public string? TradePaperId { get; set; }

        [Column(TypeName = "decimal(24,8)")]
        public decimal Volume { get; set; }

        [MaxLength(50)]
        public string? FwdNum { get; set; }

        [MaxLength(40)]
        public string? ExportType { get; set; }

        [Column(TypeName = "decimal(24,8)")]
        public decimal TotalAmountOg { get; set; }

        [MaxLength(48)]
        public string? Driver { get; set; }

        [MaxLength(16)]
        public string? FromNum { get; set; }

        public int? isPNLPrice { get; set; }
        public byte? Commerce { get; set; }

        public DateTime? DODate { get; set; }

        [Column(TypeName = "decimal(24,8)")]
        public decimal DepositTotal { get; set; }

        [Column(TypeName = "decimal(24,8)")]
        public decimal DepositTotalTax { get; set; }

        [MaxLength(16)]
        public string? SalesOutNum { get; set; }
    }
}
