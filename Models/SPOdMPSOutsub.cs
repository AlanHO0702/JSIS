using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PcbErpApi.Models
{
    // [Table("SPOdMPSOutSub")] // 若資料表名不同，解除註解並填正確表名
    public class SPOdMPSOutSub
    {
        [MaxLength(16)]
        public string PaperNum { get; set; } = string.Empty;

        public int Item { get; set; }

        [MaxLength(16)]
        public string? SourNum { get; set; }

        public int? SourItem { get; set; }

        [MaxLength(510)]
        public string? Notes { get; set; }

        [MaxLength(24)]
        public string PartNum { get; set; } = string.Empty;

        [MaxLength(8)]
        public string Revision { get; set; } = string.Empty;

        [Column(TypeName = "decimal(24,8)")]
        public decimal Qnty { get; set; }

        [Column(TypeName = "decimal(24,8)")]
        public decimal PNLQnty { get; set; }

        [Column(TypeName = "decimal(24,8)")]
        public decimal PNLPrice { get; set; }

        [Column(TypeName = "decimal(24,8)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(24,8)")]
        public decimal SubTotal { get; set; }

        [Column(TypeName = "decimal(24,8)")]
        public decimal NOSLPiece { get; set; }

        [Column(TypeName = "decimal(24,8)")]
        public decimal FreeLPiece { get; set; }

        [Column(TypeName = "decimal(24,8)")]
        public decimal AmountFinish { get; set; }

        [MaxLength(120)]
        public string? Reason { get; set; }

        [MaxLength(1020)]
        public string? InvoiceNotes { get; set; }

        [MaxLength(16)]
        public string? TransHubNum { get; set; }

        [Column(TypeName = "decimal(24,8)")]
        public decimal? Amount { get; set; }

        public byte POP { get; set; }

        [MaxLength(32)]
        public string? SourPaperId { get; set; }

        [Column(TypeName = "decimal(24,8)")]
        public decimal? UOMQnty { get; set; }

        [MaxLength(4)]
        public string? UOM { get; set; }

        [Column(TypeName = "decimal(24,8)")]
        public decimal Ratio { get; set; }

        [MaxLength(24)]
        public string? PrjId { get; set; }

        [Column(TypeName = "decimal(24,8)")]
        public decimal RealQnty { get; set; }

        [Column(TypeName = "decimal(24,8)")]
        public decimal TaxPrice { get; set; }

        [Column(TypeName = "decimal(24,8)")]
        public decimal TaxSubTotal { get; set; }

        [MaxLength(8)]
        public string? StockId { get; set; }

        [MaxLength(240)]
        public string? MatName { get; set; }

        [MaxLength(120)]
        public string? EngGauge_Cus { get; set; }

        [Column(TypeName = "decimal(24,8)")]
        public decimal Discount { get; set; }

        [Column(TypeName = "decimal(24,8)")]
        public decimal ListPrice { get; set; }

        public byte IsCash { get; set; }

        public int InStrike { get; set; }

        [MaxLength(100)]
        public string? CustPONum { get; set; }

        public int? IsNoInv { get; set; }

        [Column(TypeName = "decimal(24,8)")]
        public decimal? UOMPrice { get; set; }
    }
}
