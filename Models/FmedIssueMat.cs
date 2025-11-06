using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PcbErpApi.Models;

[Table("FMEdIssueMat")]
public partial class FmedIssueMat
{
    [Key]
    [Column("PaperNum")]
    [StringLength(16)]
    public string PaperNum { get; set; } = null!;

    [Key]
    [Column("Item")]
    public int Item { get; set; }

    [Column("MatCode")]
    [StringLength(30)]
    public string? MatCode { get; set; }

    [Column("BeDisplace")]
    public int? BeDisplace { get; set; }

    [Column("SuperId")]
    [StringLength(50)]
    public string? SuperId { get; set; }

    [Column("IssueMatQnty")]
    public decimal? IssueMatQnty { get; set; }

    [Column("StockQnty")]
    public decimal? StockQnty { get; set; }

    [Column("ProcCode")]
    [StringLength(16)]
    public string? ProcCode { get; set; }

    [Column("IsCS")]
    public int? IsCS { get; set; }

    [Column("MatPos")]
    [StringLength(10)]
    public string? MatPos { get; set; }

    [Column("SerialNum")]
    public int? SerialNum { get; set; }

    [Column("LevelNo")]
    public int? LevelNo { get; set; }

    [Column("OrgQnty")]
    public decimal? OrgQnty { get; set; }

    [Column("ScrapRate")]
    public decimal? ScrapRate { get; set; }

    [Column("SourPaperId")]
    [StringLength(50)]
    public string? SourPaperId { get; set; }

    [Column("SourNum")]
    [StringLength(16)]
    public string? SourNum { get; set; }

    [Column("SourItem")]
    public int? SourItem { get; set; }

    [Column("IssueSeq")]
    public int? IssueSeq { get; set; }

    [Column("CompanyId")]
    [StringLength(10)]
    public string? CompanyId { get; set; }

    [Column("RequireDate")]
    public DateTime? RequireDate { get; set; }

    [Column("DispMatCode")]
    [StringLength(30)]
    public string? DispMatCode { get; set; }

    [Column("IsLock")]
    public int? IsLock { get; set; }

    [Column("TempQnty")]
    public decimal? TempQnty { get; set; }

    [Column("TempGaveQnty")]
    public decimal? TempGaveQnty { get; set; }

    [Column("TempBackQnty")]
    public decimal? TempBackQnty { get; set; }

    [Column("OrgDispMatCode")]
    [StringLength(30)]
    public string? OrgDispMatCode { get; set; }

    [Column("CheckProcCode")]
    [StringLength(16)]
    public string? CheckProcCode { get; set; }
}
