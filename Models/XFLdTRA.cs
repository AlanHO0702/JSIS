using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PcbErpApi.Models
{
    /// <summary>
    /// 流程轉換/連線表
    /// </summary>
    [Table("XFLdTRA")]
    public class XFLdTRA
    {
        /// <summary>
        /// 流程代碼 (複合主鍵之一)
        /// </summary>
        [Key]
        [Column(Order = 0)]
        [Required]
        [StringLength(30)]
        public string PRCID { get; set; } = string.Empty;

        /// <summary>
        /// 轉換代碼 (複合主鍵之二)
        /// </summary>
        [Key]
        [Column(Order = 1)]
        [Required]
        [StringLength(30)]
        public string TRAID { get; set; } = string.Empty;

        /// <summary>
        /// 轉換名稱/標題
        /// </summary>
        [StringLength(400)]
        public string? CAPTION { get; set; }

        /// <summary>
        /// 來源活動代碼
        /// </summary>
        [StringLength(30)]
        public string? SRCACT { get; set; }

        /// <summary>
        /// 目標活動代碼
        /// </summary>
        [StringLength(30)]
        public string? DSTACT { get; set; }

        /// <summary>
        /// 建立者
        /// </summary>
        [StringLength(12)]
        public string? CREATOR { get; set; }

        /// <summary>
        /// 建立日期
        /// </summary>
        public DateTime? CDATE { get; set; }

        /// <summary>
        /// 轉換說明
        /// </summary>
        [StringLength(4000)]
        public string? DESCRIP { get; set; }

        /// <summary>
        /// 轉換類型
        /// </summary>
        public byte? TRATYPE { get; set; }
    }
}
