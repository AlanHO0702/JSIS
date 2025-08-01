using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PcbErpApi.Models;

[Table("AJNdClassMoney")]
public class AJNdClassMoney
{
    [Key]
    public byte MoneyCode { get; set; }

    [Key]
    public string UseId { get; set; }

    public string? MoneyName { get; set; }
    public string? EnglishName { get; set; }
    public decimal? RateToNT { get; set; }
    public decimal? RateToUS { get; set; }
    public decimal? RateToStd { get; set; }
    public byte? AmountOgStr { get; set; }
    public string? MoneyAccId { get; set; }
    public string? MoneySubAccId { get; set; }
    public ICollection<AJNdClassMoneyHis> Histories { get; set; } = new List<AJNdClassMoneyHis>();
}