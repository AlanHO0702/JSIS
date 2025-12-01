using System;
using System.Collections.Generic;

namespace PcbErpApi.Models;

public partial class SPOdMPSOutSub
{
    public string PaperNum { get; set; } = null!;

    public int Item { get; set; }

    public string? SourNum { get; set; }

    public int? SourItem { get; set; }

    public string? Notes { get; set; }

    public string PartNum { get; set; } = null!;

    public string Revision { get; set; } = null!;

    public decimal Qnty { get; set; }

    public decimal Pnlqnty { get; set; }

    public decimal Pnlprice { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal SubTotal { get; set; }

    public decimal Noslpiece { get; set; }

    public decimal FreeLpiece { get; set; }

    public decimal AmountFinish { get; set; }

    public string? Reason { get; set; }

    public string? InvoiceNotes { get; set; }

    public string? TransHubNum { get; set; }

    public decimal? Amount { get; set; }

    public byte Pop { get; set; }

    public string? SourPaperId { get; set; }

    public decimal? UOMQnty { get; set; }

    public string? Uom { get; set; }

    public decimal Ratio { get; set; }

    public string? PrjId { get; set; }

    public decimal RealQnty { get; set; }

    public decimal TaxPrice { get; set; }

    public decimal TaxSubTotal { get; set; }

    public string? StockId { get; set; }

    public string? MatName { get; set; }

    public string? EngGaugeCus { get; set; }

    public decimal Discount { get; set; }

    public decimal ListPrice { get; set; }

    public byte IsCash { get; set; }

    public int InStrike { get; set; }

    public string? CustPONum { get; set; }

    public int? IsNoInv { get; set; }

    public decimal? Uomprice { get; set; }
}
