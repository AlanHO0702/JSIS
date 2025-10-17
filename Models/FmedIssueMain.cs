using System;
using System.Collections.Generic;

namespace PcbErpApi.Models;

public partial class FmedIssueMain
{
    public string PaperNum { get; set; } = null!;

    public DateTime PaperDate { get; set; }

    public string? Notes { get; set; }

    public string? UserId { get; set; }

    public DateTime BuildDate { get; set; }

    public int Status { get; set; }

    public int Finished { get; set; }

    public string? CancelUser { get; set; }

    public DateTime? CancelDate { get; set; }

    public string? PartNum { get; set; }

    public string? Revision { get; set; }

    public int? Potype { get; set; }

    public decimal? TotalPcs { get; set; }

    public decimal? SpareRate { get; set; }

    public decimal? SpareQnty { get; set; }

    public decimal? QntyPerLot { get; set; }

    public DateTime? ExpStkTime { get; set; }

    public string? DemenseNum { get; set; }

    public string? LotNotes { get; set; }

    public string? UseId { get; set; }

    public string? FinishUser { get; set; }

    public DateTime? FinishDate { get; set; }

    public int? IsMerge { get; set; }

    public decimal? BackupRate { get; set; }

    public decimal? BackupQnty { get; set; }

    public string? PaperId { get; set; }

    public string? CompanyId { get; set; }

    public string? McutNum { get; set; }

    public int HadDemen { get; set; }

    public string? MotherIssueNum { get; set; }

    public decimal? AssGoodQnty { get; set; }

    public decimal? AssScrapQnty { get; set; }

    public DateTime? BeginDate { get; set; }

    public decimal? WorkDayCount { get; set; }

    public string? Mpsnum { get; set; }

    public int? IsBegin { get; set; }

    public int OperationType { get; set; }

    public int ProduceType { get; set; }

    public int? Flowstatus { get; set; }

    public string? Ponum { get; set; }

    public int? Poitem { get; set; }

    public string? BompartNum { get; set; }

    public string? BomverNum { get; set; }

    public int PayWayCode { get; set; }

    public int InvoiceType { get; set; }

    public int MoneyCode { get; set; }

    public decimal RateToNt { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal SubTotal { get; set; }

    public decimal Tax { get; set; }

    public decimal Total { get; set; }

    public decimal StdunitPrice { get; set; }

    public decimal StdsubTotal { get; set; }

    public decimal Stdtax { get; set; }

    public decimal Stdtotal { get; set; }

    public string? Other1 { get; set; }

    public string? Other2 { get; set; }

    public string? Other3 { get; set; }

    public string? Other4 { get; set; }

    public string? Other5 { get; set; }

    public int? BomverCount { get; set; }

    public decimal? BombatchQnty { get; set; }

    public int? Mpsitem { get; set; }

    public int? MpssumId { get; set; }

    public string? DepartId { get; set; }

    public string? TmpRouteId { get; set; }

    public string? ToMatRequestNum { get; set; }

    public string? PrjId { get; set; }

    public int FromType { get; set; }

    public decimal? IssueAllqnty { get; set; }

    public int IsLock { get; set; }

    public int? DllPaperType { get; set; }

    public string? DllPaperTypeName { get; set; }

    public string? DllHeadFirst { get; set; }

    public DateTime? UpdateDate { get; set; }

    public string? CloseUser { get; set; }

    public DateTime? CloseDate { get; set; }

    public string? LineId { get; set; }

    public string? SourceId { get; set; }

    public string? ChangeModelUser { get; set; }

    public int? IIsSys { get; set; }

    public int? IsYs { get; set; }

    public string? BIssueNum { get; set; }

    public string? IFrontProc { get; set; }

    public DateTime? IssuanceDate { get; set; }

    public string? Hdi { get; set; }

    public int? DisTrig { get; set; }

    public string? DateCode { get; set; }

    public int? ReworkTimes { get; set; }
}
