using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PcbErpApi.Models;

/// <summary>
/// 壓合疊構主檔
/// </summary>
[Table("EMOdTmpPressMas")]
public partial class EMOdTmpPressMas
{
    [Key]
    [Column("TmpId")]
    [StringLength(12)]
    public string TmpId { get; set; } = null!;
}
