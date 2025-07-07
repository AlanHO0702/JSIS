using System;
using System.ComponentModel.DataAnnotations;

namespace PcbErpApi.Models
{
    public class SpodOrderSub
    {
        [Key]
        public string PaperNum { get; set; }
        [Key]
        public int Item { get; set; }
        public string? SourNum { get; set; }
        public int? SourItem { get; set; }
        public string? Notes { get; set; }
        public string? ProdName { get; set; }
        public string CustomerPartNum { get; set; }
        public string PartNum { get; set; }
        public string Revision { get; set; }
        public decimal Qnty { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal SubTotal { get; set; }
        public decimal SourUnitPrice { get; set; }
        public DateTime? DelDate { get; set; }
        public DateTime? ExpStkDate { get; set; }
        public int? Hold { get; set; }
        public DateTime? HoldDate { get; set; }
        public decimal RealQnty { get; set; }
        public decimal CurrSCQnty { get; set; }
        public decimal FinishQnty { get; set; }
        public string? CustPONum { get; set; }
        public decimal XOut { get; set; }
        public decimal XOutRate { get; set; }
        public int? IsHUB { get; set; }
        public int? HubOut { get; set; }
        public DateTime? HisDelDate { get; set; }
        public string? SourCustomerId { get; set; }
        public byte IssueStatus { get; set; }
        public decimal? SCQnty { get; set; }
        public decimal Accept { get; set; }
        public decimal AcceptDown { get; set; }
        public string? DelDateNotes { get; set; }
        public byte Isproduce { get; set; }
        public decimal Amount { get; set; }
        public decimal PNLPrice { get; set; }
        public decimal PNLQnty { get; set; }
        public string? SourPaperId { get; set; }
        public string? UOM { get; set; }
        public decimal Ratio { get; set; }
        public decimal UOMQnty { get; set; }
        public decimal UOMPrice { get; set; }
        public string? PrjId { get; set; }
        public string? MPOComId { get; set; }
        public string? LotNotes { get; set; }
        public decimal TaxPrice { get; set; }
        public decimal TaxSubTotal { get; set; }
        public string? StockId { get; set; }
        public string? MatName { get; set; }
        public decimal TurnQnty { get; set; }
        public string? SpeCol { get; set; }
        public int HoldOut { get; set; }
        public DateTime? HoldOutDate { get; set; }
        public string? PreSourNum { get; set; }
        public decimal Discount { get; set; }
        public decimal ListPrice { get; set; }
        public decimal Volume { get; set; }
        public decimal ReadyQnty { get; set; }
        public decimal KitSourItem { get; set; }
        public decimal? SubItem { get; set; }
        public decimal? StockQnty { get; set; }
        public decimal? CanUseQnty { get; set; }
        public string? SubPlusCol_1 { get; set; }
        public byte IsCash { get; set; }
        public byte? ChkPrice { get; set; }
        public decimal PackQntyMax { get; set; }
        public decimal DeliQntyMax { get; set; }
        public decimal DeliVolume { get; set; }
        public int? IsMRP { get; set; }
        public byte NotLatest { get; set; }
        public int? MFHold { get; set; }
        public DateTime? MFHoldDate { get; set; }
        public string? CustPONum2 { get; set; }
        public int? isSummary { get; set; }

    }
}
