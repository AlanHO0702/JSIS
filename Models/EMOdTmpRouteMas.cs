using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PcbErpApi.Models;

/// <summary>
/// 途程主檔主表
/// </summary>
[Table("EMOdTmpRouteMas")]
public partial class EMOdTmpRouteMas
{
    [Key]
    [Column("TmpId")]
    [StringLength(12)]
    public string TmpId { get; set; } = null!;
}