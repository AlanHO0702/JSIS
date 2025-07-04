using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PcbErpApi.Models
{
    [Table("CURdSystemSelect")]
    public partial class CurdSystemSelect
    {
        [Key]
        [StringLength(8)] // 根據你的表結構 length=8
        public string SystemId { get; set; }

        public int ImageIndex { get; set; }

        public int Selected { get; set; }

        [StringLength(8)]
        public string? ModuleId { get; set; }

        public int SerialNum { get; set; }

        public int OrderNum { get; set; }

        public int IsDLL { get; set; }

        [StringLength(120)]
        public string? GraphName { get; set; }

        [StringLength(120)]
        public string? ManualName { get; set; }

        [StringLength(120)]
        public string? SOPName { get; set; }
    }
}
