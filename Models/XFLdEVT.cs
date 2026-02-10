using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PcbErpApi.Models
{
    /// <summary>
    /// 流程事件表
    /// </summary>
    [Table("XFLdEVT")]
    public class XFLdEVT
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
        /// 關聯代碼 (複合主鍵之二)
        /// </summary>
        [Key]
        [Column(Order = 1)]
        [Required]
        [StringLength(30)]
        public string RELATEID { get; set; } = string.Empty;

        /// <summary>
        /// 事件名稱 (複合主鍵之三)
        /// </summary>
        [Key]
        [Column(Order = 2)]
        [Required]
        [StringLength(20)]
        public string EVTNAME { get; set; } = string.Empty;

        /// <summary>
        /// 事件類型 (0=流程事件, 1=活動事件)
        /// </summary>
        public byte EVTTYPE { get; set; }

        /// <summary>
        /// 執行動作 (SQL 或 Stored Procedure)
        /// </summary>
        public string? ONEXEC { get; set; }
    }
}
