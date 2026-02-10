using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PcbErpApi.Models
{
    /// <summary>
    /// 流程活動/節點表
    /// </summary>
    [Table("XFLdAct")]
    public class XFLdAct
    {
        /// <summary>
        /// 活動代碼 (主鍵)
        /// </summary>
        [Key]
        [StringLength(20)]
        public string ACTID { get; set; } = string.Empty;

        /// <summary>
        /// 流程代碼 (外鍵)
        /// </summary>
        [StringLength(20)]
        public string? PRCID { get; set; }

        /// <summary>
        /// 活動名稱
        /// </summary>
        [StringLength(100)]
        public string? ACTNAME { get; set; }

        /// <summary>
        /// X 座標位置
        /// </summary>
        public int? X { get; set; }

        /// <summary>
        /// Y 座標位置
        /// </summary>
        public int? Y { get; set; }

        /// <summary>
        /// 活動類型
        /// </summary>
        public byte? ACTTYPE { get; set; }
    }
}
