using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PcbErpApi.Models
{
    [Table("CURdMenuTemp")]
    public class CurdMenuTemp
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(15)]
        public string ItemId { get; set; } = string.Empty;

        [Key]
        [Column(Order = 1)]
        public int Spid { get; set; }

        [StringLength(50)]
        public string? RealItemName { get; set; }

        [StringLength(50)]
        public string? ClassName { get; set; }

        [StringLength(100)]
        public string? OCXTemplete { get; set; }

        [StringLength(8)]
        public string? SystemId { get; set; }

        [StringLength(50)]
        public string? sServerName { get; set; }

        [StringLength(50)]
        public string? sDBName { get; set; }

        public int? ItemType { get; set; }

        [StringLength(8)]
        public string? SuperId { get; set; }

        public int? OutputType { get; set; }
    }
}
