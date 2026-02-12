using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PcbErpApi.Models
{
    /// <summary>
    /// 流程函式表
    /// </summary>
    [Table("XFLdFUNC")]
    public class XFLdFUNC
    {
        /// <summary>
        /// 函式名稱 (主鍵)
        /// </summary>
        [Key]
        [Required]
        [StringLength(250)]
        public string FUNCNAME { get; set; } = string.Empty;

        /// <summary>
        /// 函式說明
        /// </summary>
        [StringLength(250)]
        public string? FUNCDESC { get; set; }
    }
}
