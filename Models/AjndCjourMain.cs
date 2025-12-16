using System;
using System.Collections.Generic;

namespace PcbErpApi.Models;

public partial class AjndCjourMain
{
    public string PaperNum { get; set; } = null!;

    public DateTime PaperDate { get; set; }

    public int Status { get; set; }

    public int Finished { get; set; }

    public string? UserId { get; set; }

    public DateTime? BuildDate { get; set; }

    public string? FinishUser { get; set; }

    public DateTime? FinishDate { get; set; }

    public string? CancelUser { get; set; }

    public DateTime? CancelDate { get; set; }

    public string? UseId { get; set; }

    public string? Notes { get; set; }

    public string? PaperId { get; set; }

    public string? CjourName { get; set; }

    public int IsRemark { get; set; }

    public int UseType { get; set; }

    public int FlowStatus { get; set; }

    public int? DllPaperType { get; set; }

    public string? DllPaperTypeName { get; set; }

    public string? DllHeadFirst { get; set; }

    public string? PaperId2 { get; set; }
}
