using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PcbErpApi.Models
{
    /// <summary>
    /// 流程定義主表
    /// </summary>
    [Table("XFLdPRC")]
    public class XFLdPRC
    {
        /// <summary>
        /// 流程代碼 (主鍵)
        /// </summary>
        [Key]
        [StringLength(20)]
        public string PRCID { get; set; } = string.Empty;

        /// <summary>
        /// 流程名稱
        /// </summary>
        [StringLength(100)]
        public string? PRCNAME { get; set; }

        /// <summary>
        /// 建立者
        /// </summary>
        [StringLength(20)]
        public string? CREATOR { get; set; }

        /// <summary>
        /// 修改者
        /// </summary>
        [StringLength(20)]
        public string? MODIFICATOR { get; set; }

        /// <summary>
        /// 建立日期
        /// </summary>
        public DateTime? CDATE { get; set; }

        /// <summary>
        /// 流程說明
        /// </summary>
        [StringLength(500)]
        public string? DESCRIP { get; set; }

        /// <summary>
        /// 流程圖資料 (JSON 格式)
        /// </summary>
        public string? FLOWCHART { get; set; }

        /// <summary>
        /// 啟用狀態 (0=停用, 1=啟用)
        /// </summary>
        public int Finished { get; set; }

        /// <summary>
        /// 啟用狀態名稱 (計算欄位)
        /// </summary>
        [NotMapped]
        public string Lk_FinishedName => Finished == 1 ? "啟用" : "停用";
    }
}
