using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PcbErpApi.Models;

/// <summary>
/// 會計科目分類
/// </summary>
[Table("AJNdAccClassDtl")]
public partial class AjndAccClassDtl
{
    /// <summary>
    /// 分類代號 (主鍵)
    /// </summary>
    [Key]
    [Column("ClassDtlId")]
    [StringLength(2)]
    public string ClassDtlId { get; set; } = null!;

    /// <summary>
    /// 分類名稱
    /// </summary>
    [Column("ClassDtlName")]
    [StringLength(48)]
    public string ClassDtlName { get; set; } = null!;

    /// <summary>
    /// 總類代號 (外鍵)
    /// </summary>
    [Column("ClassId")]
    [StringLength(1)]
    public string ClassId { get; set; } = null!;

    /// <summary>
    /// 分類類型
    /// </summary>
    [Column("Type")]
    public byte Type { get; set; }

    /// <summary>
    /// 使用者代號
    /// </summary>
    [Column("UseId")]
    [StringLength(8)]
    public string UseId { get; set; } = null!;

    /// <summary>
    /// 需要分析碼1 (0: 否, 1: 是)
    /// </summary>
    [Column("NeedAnaCode_1")]
    public int? NeedAnaCode_1 { get; set; }

    /// <summary>
    /// 需要分析碼2 (0: 否, 1: 是)
    /// </summary>
    [Column("NeedAnaCode_2")]
    public int? NeedAnaCode_2 { get; set; }

    /// <summary>
    /// 需要分析碼3 (0: 否, 1: 是)
    /// </summary>
    [Column("NeedAnaCode_3")]
    public int? NeedAnaCode_3 { get; set; }

    /// <summary>
    /// 需要分析碼4 (0: 否, 1: 是)
    /// </summary>
    [Column("NeedAnaCode_4")]
    public int? NeedAnaCode_4 { get; set; }

    /// <summary>
    /// 需要分析碼5 (0: 否, 1: 是)
    /// </summary>
    [Column("NeedAnaCode_5")]
    public int? NeedAnaCode_5 { get; set; }

    /// <summary>
    /// 需要分析碼6 (0: 否, 1: 是)
    /// </summary>
    [Column("NeedAnaCode_6")]
    public int? NeedAnaCode_6 { get; set; }
}