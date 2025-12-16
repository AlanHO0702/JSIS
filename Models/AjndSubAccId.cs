using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PcbErpApi.Models;

/// <summary>
/// 會計明細科目
/// </summary>
[Table("AJNdSubAccId")]
public partial class AjndSubAccId
{
    /// <summary>
    /// 會計科目代號 (複合主鍵1)
    /// </summary>
    [Key]
    [Column("AccId", Order = 0)]
    [StringLength(8)]
    public string AccId { get; set; } = null!;

    /// <summary>
    /// 明細科目代號 (複合主鍵2)
    /// </summary>
    [Key]
    [Column("SubAccId", Order = 1)]
    [StringLength(16)]
    public string SubAccId { get; set; } = null!;

    /// <summary>
    /// 明細科目名稱
    /// </summary>
    [Column("SubAccName")]
    [StringLength(128)]
    public string SubAccName { get; set; } = null!;

    /// <summary>
    /// 期初餘額
    /// </summary>
    [Column("Surplus", TypeName = "decimal(24, 8)")]
    public decimal? Surplus { get; set; }

    /// <summary>
    /// 公司代號
    /// </summary>
    [Column("CompanyId")]
    [StringLength(16)]
    public string? CompanyId { get; set; }

    /// <summary>
    /// 銀行帳號
    /// </summary>
    [Column("AccountId")]
    [StringLength(32)]
    public string? AccountId { get; set; }

    /// <summary>
    /// 上層明細科目代號 (自關聯，次明細用)
    /// </summary>
    [Column("Parent")]
    [StringLength(16)]
    public string? Parent { get; set; }

    /// <summary>
    /// 使用者代號
    /// </summary>
    [Column("UseId")]
    [StringLength(8)]
    public string UseId { get; set; } = null!;

    /// <summary>
    /// 使用公司代號 (0: 否, 1: 是)
    /// </summary>
    [Column("UseComId")]
    public int UseComId { get; set; }

    /// <summary>
    /// 使用覆核 (0: 否, 1: 是)
    /// </summary>
    [Column("UseReAppr")]
    public int UseReAppr { get; set; }

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

    /// <summary>
    /// 需要專案代號 (0: 否, 1: 是)
    /// </summary>
    [Column("NeedPrjId")]
    public int? NeedPrjId { get; set; }

    /// <summary>
    /// 是否停用 (0: 啟用, 1: 停用)
    /// </summary>
    [Column("IsStop")]
    public byte IsStop { get; set; }
}