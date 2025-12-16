using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PcbErpApi.Models;

/// <summary>
/// 會計總帳科目
/// </summary>
[Table("AJNdAccId")]
public partial class AjndAccId
{
    /// <summary>
    /// 會計科目代號 (主鍵)
    /// </summary>
    [Key]
    [Column("AccId")]
    [StringLength(8)]
    public string AccId { get; set; } = null!;

    /// <summary>
    /// 會計科目名稱
    /// </summary>
    [Column("AccIdName")]
    [StringLength(120)]
    public string AccIdName { get; set; } = null!;

    /// <summary>
    /// 總類代號 (外鍵)
    /// </summary>
    [Column("ClassId")]
    [StringLength(1)]
    public string ClassId { get; set; } = null!;

    /// <summary>
    /// 分類代號 (外鍵)
    /// </summary>
    [Column("ClassDtlId")]
    [StringLength(2)]
    public string ClassDtlId { get; set; } = null!;

    /// <summary>
    /// 借方科目 (0: 否, 1: 是)
    /// </summary>
    [Column("DAccount")]
    public int? DAccount { get; set; }

    /// <summary>
    /// 明細科目 (0: 否, 1: 是)
    /// </summary>
    [Column("SubAccount")]
    public int? SubAccount { get; set; }

    /// <summary>
    /// 摘要科目 (0: 否, 1: 是)
    /// </summary>
    [Column("RemarkAccount")]
    public int? RemarkAccount { get; set; }

    /// <summary>
    /// 期初餘額
    /// </summary>
    [Column("Surplus", TypeName = "decimal(24, 8)")]
    public decimal? Surplus { get; set; }

    /// <summary>
    /// 借方科目內部值
    /// </summary>
    [Column("iDAccount")]
    public int iDAccount { get; set; }

    /// <summary>
    /// 明細科目內部值
    /// </summary>
    [Column("iSubAccount")]
    public int iSubAccount { get; set; }

    /// <summary>
    /// 使用者代號
    /// </summary>
    [Column("UseId")]
    [StringLength(8)]
    public string UseId { get; set; } = null!;

    /// <summary>
    /// 科目類型
    /// </summary>
    [Column("Type")]
    public byte Type { get; set; }

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
    /// 全部使用覆核 (0: 否, 1: 是)
    /// </summary>
    [Column("UseReApprAll")]
    public int UseReApprAll { get; set; }

    /// <summary>
    /// 全部使用公司代號 (0: 否, 1: 是)
    /// </summary>
    [Column("UseComIdAll")]
    public int UseComIdAll { get; set; }
}