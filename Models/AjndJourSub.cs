using System;
using System.Collections.Generic;

namespace PcbErpApi.Models;

public partial class AjndJourSub
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

    public string? ProjectRate { get; set; }

    public string? RelationUseId { get; set; }

    public string? SourPaperId { get; set; }

    public string? SourNum2 { get; set; }

    public int? SourItem2 { get; set; }

    public decimal AmountOg { get; set; }

    public int? MoneyCode { get; set; }

    public decimal? RateToNt { get; set; }

    public decimal? OgIn { get; set; }

    public decimal? OgOut { get; set; }

    public decimal? ChkIn { get; set; }

    public decimal? ChkOut { get; set; }

    public string? CompanyId { get; set; }

    public decimal OpenAmountOg { get; set; }

    public decimal OpenAmount { get; set; }

    public string? BudgetCode { get; set; }

    public string? AnaCode1 { get; set; }

    public string? AnaCode2 { get; set; }

    public string? AnaCode3 { get; set; }

    public string? AnaCode4 { get; set; }

    public string? AnaCode5 { get; set; }

    public string? AnaCode6 { get; set; }

    public DateTime? PayBackDate { get; set; }

    public string? PrjId { get; set; }

    public int IsExcluedDiv { get; set; }
}
