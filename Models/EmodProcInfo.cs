using System;
using System.Collections.Generic;

namespace PcbErpApi.Models;

public partial class EmodProcInfo
{
    public string ProcCode { get; set; } = null!;

    public string ProcName { get; set; } = null!;

    public string? CostCenter { get; set; }

    public string? DepartId { get; set; }

    public string? CapId { get; set; }

    public float? StdDailyCap { get; set; }

    public float? StdDgr { get; set; }

    public float? AbsWip { get; set; }

    public float? StdWip { get; set; }

    public float? LeastLeadTime { get; set; }

    public float? StdLeadTime { get; set; }

    public float? DailyPilmt { get; set; }

    public string? FromTime { get; set; }

    public string? DueTime { get; set; }

    public int? IsAutoPass { get; set; }

    public int? IsPrePass { get; set; }

    public int IsStorage { get; set; }

    public int ProcType { get; set; }

    public int Halted { get; set; }

    public int FtrHalt { get; set; }

    public DateTime? HaltTime { get; set; }

    public string? Memo { get; set; }

    public float? StdDgr2 { get; set; }

    public int? IsCheckPass { get; set; }

    public float? AboutNum { get; set; }

    public float? Spvrate { get; set; }

    public int SpvdelayTimes { get; set; }

    public int IsDateCode { get; set; }

    public int IsXout { get; set; }

    public int IsShowPqnty { get; set; }

    public float? StdDgrqnty { get; set; }

    public float? Fosdgr { get; set; }

    public float? Fosdgrqnty { get; set; }

    public int? RuleItem { get; set; }

    public int Oscheck { get; set; }

    public int ProcPassNext { get; set; }

    public int OspassNext { get; set; }

    public int? OsprodTime { get; set; }

    public int Osproc { get; set; }

    public int EquipBase { get; set; }

    public string ProcGroup { get; set; } = null!;

    public decimal? CostDivRate_Human { get; set; }

    public decimal? CostDivRate_Expend { get; set; }

    public decimal? CostDivRate_Other { get; set; }

    public string? Other1 { get; set; }

    public string? Other2 { get; set; }

    public string? Other3 { get; set; }

    public string? Other4 { get; set; }

    public string? Other5 { get; set; }

    public string? Qctype { get; set; }

    public int XoutNeedDefect { get; set; }

    public int Oqc { get; set; }

    public int? BAftMergeDivEcncustPass { get; set; }

    public string? ProcNameEng { get; set; }

    public int? IsYs { get; set; }

    public int? ISensorPass { get; set; }

    public int? OsNeedUp { get; set; }

    public decimal? WorkAssignTime { get; set; }

    public decimal? OsworkAssignTime { get; set; }

    public string? ComputeId { get; set; }

    public decimal? ReworkRatio { get; set; }

    public int AutoTurnElseIn { get; set; }

    public int Oscarry { get; set; }

    public int IsRwkQnty { get; set; }
}
