using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PcbErpApi.Models;

[Table("FMEdIssuePO")]
public partial class FmedIssuePo
{
    [Key]
    [Column("PaperNum")]
    [StringLength(16)]
    public string PaperNum { get; set; } = null!;

    [Key]
    [Column("Item")]
    public int Item { get; set; }

    [Column("PONum")]
    [StringLength(16)]
    public string? PONum { get; set; }

    [Column("SerialNum")]
    public int? SerialNum { get; set; }

    [Column("DelDate")]
    public DateTime? DelDate { get; set; }

    [Column("POQnty")]
    public decimal? POQnty { get; set; }

    [Column("IssuePCSQnty")]
    public decimal? IssuePCSQnty { get; set; }

    [Column("ScrapLot")]
    public decimal? ScrapLot { get; set; }

    [Column("SourNum")]
    [StringLength(16)]
    public string? SourNum { get; set; }

    [Column("SourItem")]
    public int? SourItem { get; set; }

    [Column("SourPaperId")]
    [StringLength(50)]
    public string? SourPaperId { get; set; }

    [Column("iIsSys")]
    public int? IIsSys { get; set; }

    [Column("AllowQnty")]
    public decimal? AllowQnty { get; set; }

    [Column("WIPQnty")]
    public decimal? WIPQnty { get; set; }

    [Column("LessQnty")]
    public decimal? LessQnty { get; set; }

    [Column("ChoiceStockQnty")]
    public decimal? ChoiceStockQnty { get; set; }

    [Column("ChoiceWIPQnty")]
    public decimal? ChoiceWIPQnty { get; set; }

    [Column("ChoiceAllOutQnty")]
    public decimal? ChoiceAllOutQnty { get; set; }

    [Column("ChoiceNeedQnty")]
    public decimal? ChoiceNeedQnty { get; set; }

    [Column("IssQnty")]
    public decimal? IssQnty { get; set; }

    [Column("UnIssQnty")]
    public decimal? UnIssQnty { get; set; }

    [Column("IssueWPnlQnty")]
    public decimal? IssueWPnlQnty { get; set; }
}
