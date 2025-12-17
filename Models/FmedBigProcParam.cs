using System;
using System.Collections.Generic;

namespace PcbErpApi.Models;

/// <summary>
/// 製程參數明細 (FMEdBigProcParam)
/// </summary>
public partial class FmedBigProcParam
{
    /// <summary>
    /// 製程代號（複合主鍵之一）
    /// </summary>
    public string ProcCode { get; set; } = null!;

    /// <summary>
    /// 製程群組
    /// </summary>
    public string? ProcGroup { get; set; }

    /// <summary>
    /// 參數代號（複合主鍵之一）
    /// </summary>
    public string ParamId { get; set; } = null!;

    /// <summary>
    /// 參數名稱
    /// </summary>
    public string? ParamName { get; set; }

    /// <summary>
    /// 參數值
    /// </summary>
    public string? ParamValue { get; set; }

    /// <summary>
    /// 標準值
    /// </summary>
    public string? StdValue { get; set; }

    /// <summary>
    /// 上限值
    /// </summary>
    public decimal? UpperLimit { get; set; }

    /// <summary>
    /// 下限值
    /// </summary>
    public decimal? LowerLimit { get; set; }

    /// <summary>
    /// 單位
    /// </summary>
    public string? Unit { get; set; }

    /// <summary>
    /// 參數類型
    /// </summary>
    public string? ParamType { get; set; }

    /// <summary>
    /// 是否必填
    /// </summary>
    public int? IsRequired { get; set; }

    /// <summary>
    /// 是否檢查
    /// </summary>
    public int? IsCheck { get; set; }

    /// <summary>
    /// 排序序號
    /// </summary>
    public int? SerialNum { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    public string? Memo { get; set; }

    /// <summary>
    /// 建立日期
    /// </summary>
    public DateTime? CreateDate { get; set; }

    /// <summary>
    /// 修改日期
    /// </summary>
    public DateTime? ModifyDate { get; set; }

    /// <summary>
    /// 其他欄位1
    /// </summary>
    public string? Other1 { get; set; }

    /// <summary>
    /// 其他欄位2
    /// </summary>
    public string? Other2 { get; set; }

    /// <summary>
    /// 其他欄位3
    /// </summary>
    public string? Other3 { get; set; }
}
