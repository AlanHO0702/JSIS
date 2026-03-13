using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PcbErpApi.Models;

[Table("EMOdProdLayer")]
[PrimaryKey(nameof(PartNum), nameof(Revision), nameof(LayerId))]
public partial class EmodProdLayer
{
    public string PartNum { get; set; } = null!;
    public string Revision { get; set; } = null!;
    public string LayerId { get; set; } = null!;

    public string? LayerName { get; set; }
    public string? AftLayerId { get; set; }
    public int? IssLayer { get; set; }
    public int? Degree { get; set; }
    public int? FL { get; set; }
    public int? EL { get; set; }
    public string? TmpRouteId { get; set; }
    public string? StdPressCode { get; set; }
    public float? Film_PPM2 { get; set; }
    public float? Film_ChkReceUp2 { get; set; }
    public string? LayerNotes { get; set; }
    public string? CMapPath { get; set; }
    public string? SMapPath { get; set; }
    public string? TmpPressId { get; set; }
    public int? IsUseNotes { get; set; }
    public string? ProcRemark { get; set; }
    public DateTime? EditTime { get; set; }
    public string? EditUser { get; set; }
}