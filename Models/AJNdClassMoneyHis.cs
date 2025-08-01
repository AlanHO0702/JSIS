using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PcbErpApi.Models;

[Table("AJNdClassMoneyHis")]
public class AJNdClassMoneyHis
{
    [Key]
    public byte MoneyCode { get; set; }
    [Key]
    public string UseId { get; set; }
    
    [Column(TypeName = "date")]
    [Display(Name = "生效日")]
    public DateTime RateDate { get; set; }

    [Display(Name = "中間匯率(出納)")]
    public decimal? RateToNT { get; set; }

    [Display(Name = "海關買進匯率")]
    public decimal? RateToBuy { get; set; }

    [Display(Name = "海關賣出匯率")]
    public decimal? RateToSell { get; set; }

    public AJNdClassMoney? Money { get; set; }
}
