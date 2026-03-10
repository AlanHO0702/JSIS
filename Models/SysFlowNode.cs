using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PcbErpApi.Models
{
    /// <summary>
    /// 系統互動流程圖節點位置表（儲存各系統模組流程圖的節點座標）
    /// </summary>
    [Table("SysFlowNode")]
    public class SysFlowNode
    {
        /// <summary>
        /// 系統模組代碼（如 SPO、CCS、CPN）
        /// </summary>
        [Key, Column(Order = 0)]
        [StringLength(20)]
        public string SystemId { get; set; } = string.Empty;

        /// <summary>
        /// 節點識別碼（對應 JS 設定中的 id）
        /// </summary>
        [Key, Column(Order = 1)]
        [StringLength(50)]
        public string NodeId { get; set; } = string.Empty;

        /// <summary>
        /// X 座標（百分比，0–100）
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Y 座標（百分比，0–100）
        /// </summary>
        public double Y { get; set; }
    }
}
