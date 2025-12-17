using System;
using System.Collections.Generic;

namespace PcbErpApi.Models;

public partial class FmedVProcNisToStd
{
    public string StockId { get; set; } = null!;

    public string ProcCode { get; set; } = null!;

    public string? ProcName { get; set; }

    public string LotNum { get; set; } = null!;

    public string PartNum { get; set; } = null!;

    public string Revision { get; set; } = null!;

    public string LayerId { get; set; } = null!;

    public int Pop { get; set; }

    public string? PopName { get; set; }

    public decimal? Qnty { get; set; }

    public int LotStatus { get; set; }

    public string? LotStatusName { get; set; }

    public int Halted { get; set; }

    public int FtrHalt { get; set; }

    public string? HaltProc { get; set; }

    public string? HaltProcName { get; set; }

    public int RouteSerial { get; set; }

    public int? ReWork { get; set; }

    public int? StockBack { get; set; }

    public int? Sc { get; set; }

    public int BNowPrePass { get; set; }

    public int? IInIqc { get; set; }

    public string? IInIqcname { get; set; }

    public decimal LotXoutQnty { get; set; }

    public string? L_LLPcs { get; set; }

    public int IsScback { get; set; }

    public float? Pivalue { get; set; }

    public string? MatName { get; set; }

    public DateTime? ExpStkTime { get; set; }

    public int? Qcstatus { get; set; }

    public string? QcstatusName { get; set; }

    public string? IssueNum { get; set; }

    public decimal? PivalueNis { get; set; }

    public int? IIsWork { get; set; }

    public int? IIsUrgent { get; set; }

    public string? SWorkSeq { get; set; }

    public string? SStatusMut { get; set; }

    public string? ProgressNotes { get; set; }

    public DateTime? Lk_InTime { get; set; }

    public int? IOnProcTime { get; set; }

    public string? StrL_LLpiece { get; set; }

    public string? HaltNotes { get; set; }

    public decimal? GoodPcs { get; set; }

    public decimal? EqualFgpcs { get; set; }

    public string? PaperNum { get; set; }

    public int? PoType { get; set; }

    public string? LotNotes { get; set; }

    public string? AftProcNameString { get; set; }

    public string? BprocCode { get; set; }

    public string? WorkProc { get; set; }

    public DateTime? WorkDate { get; set; }

    public string? Ultype { get; set; }

    public string CheckValue { get; set; } = null!;

    public int? LayerCount { get; set; }

    public int? IsMainMat { get; set; }

    public string PaperId { get; set; } = null!;

    public int? NewStatus { get; set; }

    public string? RevNum { get; set; }

    public string? MotherIssueNum { get; set; }

    public string? Hdi { get; set; }

    public string? DateCode { get; set; }

    public string? MidVarchar_12 { get; set; }

    public int? Issued { get; set; }

    public float? InsideBullHoldX { get; set; }

    public string? CustomerId { get; set; }

    public float? NickelRequestMax { get; set; }

    public string? Mark { get; set; }

    public decimal? FinishQnty { get; set; }
}
