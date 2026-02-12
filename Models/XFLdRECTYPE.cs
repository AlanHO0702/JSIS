using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PcbErpApi.Models
{
    /// <summary>
    /// 流程收件類型表
    /// </summary>
    [Table("XFLdRECTYPE")]
    public class XFLdRECTYPE
    {
        /// <summary>
        /// 排序序號 (主鍵)
        /// </summary>
        [Key]
        public int SEQ { get; set; }

        /// <summary>
        /// 收件類型名稱
        /// </summary>
        [StringLength(50)]
        public string? ACTRECTYPE { get; set; }

        /// <summary>
        /// 預設腳本（用於設定收件前行為）
        /// </summary>
        [StringLength(4000)]
        public string? DEFSTAT { get; set; }

        /// <summary>
        /// 參數查詢 SQL
        /// </summary>
        [StringLength(4000)]
        public string? LOOKUPSQL { get; set; }
    }
}
