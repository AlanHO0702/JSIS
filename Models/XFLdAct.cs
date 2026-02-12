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

        /// <summary>
        /// 建立人員
        /// </summary>
        [StringLength(12)]
        public string? CREATOR { get; set; }

        /// <summary>
        /// 建立日期
        /// </summary>
        public DateTime? CDATE { get; set; }

        /// <summary>
        /// 活動描述
        /// </summary>
        [StringLength(4000)]
        public string? DESCRIP { get; set; }

        /// <summary>
        /// 收件類型
        /// </summary>
        [StringLength(50)]
        public string? RECTYPE { get; set; }

        /// <summary>
        /// 收件參數
        /// </summary>
        [StringLength(50)]
        public string? RECPARAM { get; set; }

        /// <summary>
        /// 是否允許加簽 (0:否, 1:是)
        /// </summary>
        public byte? ALLOWADD { get; set; }

        /// <summary>
        /// 是否允許退回 (0:否, 1:是)
        /// </summary>
        public byte? ALLOWRETURN { get; set; }

        /// <summary>
        /// 是否為會簽 (0:否, 1:是)
        /// </summary>
        public int? iMultiSign { get; set; }

        /// <summary>
        /// 核准所需的同意人數
        /// </summary>
        public int? iMultiSignAllow { get; set; }

        /// <summary>
        /// 是否發送郵件通知 (0:否, 1:是)
        /// </summary>
        public int? ALLOWMAIL { get; set; }
    }
}
