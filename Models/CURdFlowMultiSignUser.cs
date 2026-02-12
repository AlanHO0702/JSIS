using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PcbErpApi.Models
{
    /// <summary>
    /// 流程會簽人員表
    /// </summary>
    [Table("CURdFlowMultiSignUser")]
    public class CURdFlowMultiSignUser
    {
        /// <summary>
        /// 流程代碼 (複合主鍵1)
        /// </summary>
        [Key]
        [Column(Order = 0)]
        [StringLength(30)]
        public string PRCID { get; set; } = string.Empty;

        /// <summary>
        /// 活動代碼 (複合主鍵2)
        /// </summary>
        [Key]
        [Column(Order = 1)]
        [StringLength(30)]
        public string ACTID { get; set; } = string.Empty;

        /// <summary>
        /// 會簽人員編號 (複合主鍵3)
        /// </summary>
        [Key]
        [Column(Order = 2)]
        [StringLength(16)]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// 建立日期
        /// </summary>
        public DateTime? BuildDate { get; set; }

        /// <summary>
        /// 更新人員編號
        /// </summary>
        [StringLength(16)]
        public string? Update_UserId { get; set; }

        /// <summary>
        /// 會簽人員姓名 (僅用於顯示，不存資料庫)
        /// </summary>
        [NotMapped]
        public string? UserName { get; set; }
    }
}
