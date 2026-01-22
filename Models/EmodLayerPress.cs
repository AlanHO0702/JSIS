namespace PcbErpApi.Models
{
    /// <summary>
    /// 疊構資料模型
    /// </summary>
    public class EmodLayerPress
    {
        /// <summary>
        /// 料號
        /// </summary>
        public string PartNum { get; set; } = string.Empty;

        /// <summary>
        /// 版序
        /// </summary>
        public string Revision { get; set; } = string.Empty;

        /// <summary>
        /// 層別 ID
        /// </summary>
        public string LayerId { get; set; } = string.Empty;

        /// <summary>
        /// 層壓圖資料 1
        /// </summary>
        public string? MapData_1 { get; set; }

        /// <summary>
        /// 層壓圖資料 2
        /// </summary>
        public string? MapData_2 { get; set; }

        /// <summary>
        /// 層壓圖資料 3
        /// </summary>
        public string? MapData_3 { get; set; }

        /// <summary>
        /// 材料代碼
        /// </summary>
        public string? MatCode { get; set; }

        /// <summary>
        /// 材料名稱
        /// </summary>
        public string? MatName { get; set; }

        /// <summary>
        /// 層厚度
        /// </summary>
        public decimal? Thickness { get; set; }
    }
}
