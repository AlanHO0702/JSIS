using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PcbErpApi.Models;

/// <summary>
/// 組合模型主檔
/// </summary>
[Table("EMOdTmpBOMMas")]
public partial class EMOdTmpBOMMas
{
    [Key]
    [Column("TmpId")]
    [StringLength(12)]
    public string TmpId { get; set; } = null!;
}
