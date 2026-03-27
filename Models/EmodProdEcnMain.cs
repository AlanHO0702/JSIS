using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PcbErpApi.Models;

[Table("EMOdProdECNMain")]
public partial class EmodProdEcnMain
{
    [Key]
    public string PaperNum { get; set; } = null!;

    public DateTime PaperDate { get; set; }
    public string? Notes { get; set; }
    public string? UserId { get; set; }
    public DateTime BuildDate { get; set; }
    public int Status { get; set; }
    public int Finished { get; set; }
    public string? CancelUser { get; set; }
    public DateTime? CancelDate { get; set; }
    public string? FinishUser { get; set; }
    public DateTime? FinishDate { get; set; }
    public string? UseId { get; set; }
    public string? PaperId { get; set; }
    public int? FlowStatus { get; set; }
    public int? dllPaperType { get; set; }
    public string? dllPaperTypeName { get; set; }
    public string? dllHeadFirst { get; set; }
    public byte ForSQU { get; set; }
}