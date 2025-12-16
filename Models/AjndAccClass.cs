using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PcbErpApi.Models;

/// <summary>
/// 會計科目總類
/// </summary>
[Table("AJNdAccClass")]
public partial class AjndAccClass
{
    /// <summary>
    /// 總類代號 (主鍵)
    /// </summary>
    [Key]
    [Column("ClassId")]
    [StringLength(1)]
    public string ClassId { get; set; } = null!;

    /// <summary>
    /// 總類名稱
    /// </summary>
    [Column("ClassName")]
    [StringLength(48)]
    public string ClassName { get; set; } = null!;

    /// <summary>
    /// 總類類型 (0: 資產負債表, 1: 損益表)
    /// </summary>
    [Column("ClassType")]
    public int ClassType { get; set; }

    /// <summary>
    /// 使用者代號
    /// </summary>
    [Column("UseId")]
    [StringLength(8)]
    public string UseId { get; set; } = null!;
}