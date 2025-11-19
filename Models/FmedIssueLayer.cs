using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PcbErpApi.Models;

[Table("FMEdIssueLayer")]
public partial class FmedIssueLayer
{
    [Key]
    [Column("PaperNum")]
    [StringLength(16)]
    public string PaperNum { get; set; } = null!;

    [Key]
    [Column("Item")]
    public int Item { get; set; }

    [Column("LayerId")]
    [StringLength(16)]
    public string? LayerId { get; set; }

    [Column("IssuePNLQnty")]
    public decimal? IssuePNLQnty { get; set; }

    [Column("SourNum")]
    [StringLength(16)]
    public string? SourNum { get; set; }

    [Column("SourItem")]
    public int? SourItem { get; set; }

    [Column("SourPaperId")]
    [StringLength(50)]
    public string? SourPaperId { get; set; }

    [Column("IsIssue")]
    public int? IsIssue { get; set; }

    [Column("ToIssue")]
    public int? ToIssue { get; set; }

    [Column("CanDelete")]
    public int? CanDelete { get; set; }

    [Column("IsFromSys")]
    public int? IsFromSys { get; set; }

    [Column("LotCount")]
    public int? LotCount { get; set; }

    [Column("SumPOPQnty")]
    public decimal? SumPOPQnty { get; set; }

    [Column("SumABSPiece")]
    public decimal? SumABSPiece { get; set; }

    [Column("POP")]
    public int? POP { get; set; }

    [Column("SumSPNLQnty")]
    public int? SumSPNLQnty { get; set; }

    [Column("PartNum")]
    [StringLength(30)]
    public string? PartNum { get; set; }

    [Column("Revision")]
    [StringLength(10)]
    public string? Revision { get; set; }

    [Column("isECN")]
    public int? IsECN { get; set; }

    [Column("IsChgRoute")]
    public int? IsChgRoute { get; set; }

    [Column("isMCN")]
    public int? IsMCN { get; set; }
}
