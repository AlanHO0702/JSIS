using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PcbErpApi.Models
{
    /// <summary>
    /// 系統互動流程圖連線端口覆寫表
    /// EdgeKey 格式："{fromNodeId}:{toNodeId}"
    /// </summary>
    [Table("SysFlowEdge")]
    public class SysFlowEdge
    {
        [Key, Column(Order = 0)]
        [StringLength(20)]
        public string SystemId { get; set; } = string.Empty;

        [Key, Column(Order = 1)]
        [StringLength(100)]
        public string EdgeKey { get; set; } = string.Empty;

        /// <summary>起點節點的連接方向（top / bottom / left / right）</summary>
        [StringLength(20)]
        public string FromPort { get; set; } = string.Empty;

        /// <summary>終點節點的連接方向（top / bottom / left / right）</summary>
        [StringLength(20)]
        public string ToPort { get; set; } = string.Empty;

        /// <summary>彎折段的位置偏移量（百分比空間，可正可負）</summary>
        public double PivotOffset { get; set; }
    }
}
