namespace PcbErpApi.Models
{
    /// <summary>
    /// 產品工程圖資料模型
    /// </summary>
    public class EmodProdMap
    {
        /// <summary>
        /// 料號
        /// </summary>
        public string PartNum { get; set; } = string.Empty;

        /// <summary>
        /// 版次
        /// </summary>
        public string Revision { get; set; } = string.Empty;

        /// <summary>
        /// 圖別：1=裁板圖, 3=排板圖, 9=PF裁剪圖
        /// </summary>
        public byte MapKindNo { get; set; }

        /// <summary>
        /// 序號
        /// </summary>
        public byte SerialNum { get; set; }

        /// <summary>
        /// 圖形資料
        /// </summary>
        public string? MapData { get; set; }
    }
}
