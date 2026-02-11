using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PcbErpApi.Models
{
    /// <summary>
    /// 流程系統參數表
    /// </summary>
    [Table("XFLdSYSPARAMS")]
    [PrimaryKey(nameof(EVTNAME), nameof(PARAMNAME))]
    public class XFLdSYSPARAMS
    {
        /// <summary>
        /// 事件名稱 (主鍵之一)
        /// </summary>
        [Required]
        [StringLength(20)]
        public string EVTNAME { get; set; } = string.Empty;

        /// <summary>
        /// 參數名稱 (主鍵之二)
        /// </summary>
        [Required]
        [StringLength(30)]
        public string PARAMNAME { get; set; } = string.Empty;
    }
}
