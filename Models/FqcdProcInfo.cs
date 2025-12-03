using System;
using System.Collections.Generic;

namespace PcbErpApi.Models;

/// <summary>
/// 製程大站 (FQCdProcInfo)
/// </summary>
public partial class FqcdProcInfo
{
    /// <summary>
    /// 製程代號（主鍵）
    /// </summary>
    public string BProcCode { get; set; } = null!;

    /// <summary>
    /// 序號
    /// </summary>
    public int? SerialNum { get; set; }

    /// <summary>
    /// 比率
    /// </summary>
    public double? MRatio { get; set; }

    /// <summary>
    /// 製程名稱
    /// </summary>
    public string? BProcName { get; set; }

    /// <summary>
    /// 產能ID
    /// </summary>
    public int? CapacityId { get; set; }

    /// <summary>
    /// POP
    /// </summary>
    public int? POP { get; set; }

    /// <summary>
    /// SQU4數值
    /// </summary>
    public string? SQU4ValueNum { get; set; }

    /// <summary>
    /// 利潤中心
    /// </summary>
    public string? ProfitCenter { get; set; }

    /// <summary>
    /// 製程英文名稱
    /// </summary>
    public string? BProcNameEng { get; set; }

    /// <summary>
    /// 製程類型
    /// </summary>
    public int? BProcType { get; set; }

    /// <summary>
    /// 製程名稱主檔（用於 lookup，可能不在資料表中）
    /// </summary>
    public string? BPTypeNameM { get; set; }
}
