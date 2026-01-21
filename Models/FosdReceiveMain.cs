using System;
using System.Collections.Generic;

namespace PcbErpApi.Models;

public partial class FosdReceiveMain
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
    public string? CompanyId { get; set; }
    public string? ProcCode { get; set; }
    public string? SubProcCode { get; set; }
    public int PayWayCode { get; set; }
    public int MoneyCode { get; set; }
    public decimal RateToNT { get; set; }
    public decimal SubTotal { get; set; }
    public int InvoiceType { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public string? InvoiceNum { get; set; }
    public int PaperType { get; set; }
    public string? FinishUser { get; set; }
    public DateTime? FinishDate { get; set; }
    public string? UseId { get; set; }
    public string? PaperId { get; set; }
    public int TranTo_LPcs { get; set; }
    public string? FactorId { get; set; }
    public int IsSample { get; set; }
    public string? PassPaperNum { get; set; }
    public int OSType { get; set; }
    public decimal STDSubTotal { get; set; }
    public decimal STDTax { get; set; }
    public decimal STDTotal { get; set; }
    public int FromType { get; set; }
    public int? FlowStatus { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public int? dllPaperType { get; set; }
    public string? dllPaperTypeName { get; set; }
    public string? dllHeadFirst { get; set; }
    public string? RwkRoute { get; set; }
    public string? DProcCode { get; set; }
    public int? UseTaxPrice { get; set; }
    public int? iDoScrapFromFQCsys { get; set; }
    public string? Lk_QCFinishName { get; set; }
    public string? Lk_QCNum { get; set; }
    public int? IsDivided { get; set; }
    public int? DisTrig { get; set; }
    public int? DividedTimes { get; set; }
}
