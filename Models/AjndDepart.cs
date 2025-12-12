using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PcbErpApi.Models;

/// <summary>
/// 部門主檔
/// </summary>
[Table("AJNdDepart")]
public partial class AjndDepart
{
    /// <summary>
    /// 部門代號 (主鍵)
    /// </summary>
    [Key]
    [Column("DepartId")]
    [StringLength(12)]
    public string DepartId { get; set; } = null!;

    /// <summary>
    /// 部門名稱
    /// </summary>
    [Column("DepartName")]
    [StringLength(48)]
    public string? DepartName { get; set; }

    /// <summary>
    /// 是否為成本中心 (0: 否, 1: 是)
    /// </summary>
    [Column("IsCostCenter")]
    public byte? IsCostCenter { get; set; }

    /// <summary>
    /// 階層編號
    /// </summary>
    [Column("LEVelNo")]
    public byte? LEVelNo { get; set; }

    /// <summary>
    /// 上層部門代號
    /// </summary>
    [Column("SuperId")]
    [StringLength(12)]
    public string? SuperId { get; set; }

    /// <summary>
    /// 部門類別
    /// </summary>
    [Column("Type")]
    public int? Type { get; set; }

    /// <summary>
    /// 部門主管代號
    /// </summary>
    [Column("ManagerId")]
    [StringLength(16)]
    public string? ManagerId { get; set; }

    /// <summary>
    /// 是否為事業單位 (0: 否, 1: 是)
    /// </summary>
    [Column("IsBU")]
    public byte? IsBU { get; set; }

    /// <summary>
    /// 使用者代號
    /// </summary>
    [Column("UseId")]
    [StringLength(8)]
    public string UseId { get; set; } = null!;

    /// <summary>
    /// 是否停用 (0: 啟用, 1: 停用)
    /// </summary>
    [Column("IsStop")]
    public byte? IsStop { get; set; }

    /// <summary>
    /// 薪資會計科目1
    /// </summary>
    [Column("SalAccId_1")]
    [StringLength(8)]
    public string? SalAccId_1 { get; set; }

    /// <summary>
    /// 薪資會計科目2
    /// </summary>
    [Column("SalAccId_2")]
    [StringLength(8)]
    public string? SalAccId_2 { get; set; }

    /// <summary>
    /// 薪資會計科目3
    /// </summary>
    [Column("SalAccId_3")]
    [StringLength(8)]
    public string? SalAccId_3 { get; set; }

    /// <summary>
    /// 線別代號
    /// </summary>
    [Column("LineId")]
    [StringLength(8)]
    public string? LineId { get; set; }

    /// <summary>
    /// 事業部
    /// </summary>
    [Column("Division")]
    [StringLength(8)]
    public string? Division { get; set; }

    /// <summary>
    /// 最後修改時間
    /// </summary>
    [Column("ModifiedTime")]
    public DateTime? ModifiedTime { get; set; }
}
