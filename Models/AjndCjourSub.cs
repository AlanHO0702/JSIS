using System;
using System.Collections.Generic;

namespace PcbErpApi.Models;

public partial class AjndCjourSub
{
    public string PaperNum { get; set; } = null!;

    public int Item { get; set; }

    public string? SourNum { get; set; }

    public int? SourItem { get; set; }

    public string? Notes { get; set; }

    public int IsD { get; set; }

    public string AccId { get; set; } = null!;

    public decimal Amount { get; set; }

    public string? Comment { get; set; }

    public string? SubAccId { get; set; }

    public string? DepartId { get; set; }

    public string? ProjectId { get; set; }

    public string? SourPaperId { get; set; }

    public int? ItemTypeId { get; set; }

    public string? MatClass { get; set; }

    public int? MoneyCode { get; set; }

    public decimal? RateToNt { get; set; }

    public decimal? OgIn { get; set; }

    public decimal? OgOut { get; set; }

    public decimal? ChkIn { get; set; }

    public decimal? ChkOut { get; set; }
}
