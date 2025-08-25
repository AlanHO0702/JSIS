using System;
using System.Collections.Generic;

namespace PcbErpApi.Models;

public partial class AjndJourMain
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

    public string? Notes { get; set; }

    public string? PaperId { get; set; }

    public string? SourNum { get; set; }

    public DateTime? JourDate { get; set; }

    public string? JourId { get; set; }

    public int JourType { get; set; }

    public string? Accountant { get; set; }

    public decimal? TotalAmount { get; set; }

    public int FlowStatus { get; set; }

    public decimal TotalAmountOg { get; set; }

    public int MoneyCode { get; set; }

    public decimal RateToNt { get; set; }

    public int? DllPaperType { get; set; }

    public string? DllPaperTypeName { get; set; }

    public string? DllHeadFirst { get; set; }

    public int? DisTrig { get; set; }

    public byte IsCopy { get; set; }

    public int? IsCost { get; set; }
}
