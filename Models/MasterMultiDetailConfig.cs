namespace WebRazor.Models
{
    /// <summary>
    /// 佈局模式枚舉
    /// </summary>
    public enum LayoutMode
    {
        /// <summary>
        /// 預設：Tab 切換佈局（向後兼容）
        /// </summary>
        Tabs = 0,

        /// <summary>
        /// 三欄式橫向佈局（左側：上下分割，中間、右側：各佔一欄）
        /// </summary>
        ThreeColumn = 1,

        /// <summary>
        /// 垂直堆疊佈局（上中下三層）
        /// </summary>
        VerticalStack = 2
    }

    public class MasterMultiDetailConfig
    {
        public string DomId { get; set; }
        public string MasterTitle { get; set; }
        public string MasterTable { get; set; }
        public string MasterDict { get; set; }
        public string MasterApi { get; set; }
        public int MasterTop { get; set; } = 200;

        /// <summary>
        /// Master 主鍵/唯一鍵欄位（用於 SaveTableChanges / 刪除/更新定位）。
        /// </summary>
        public List<string> MasterPkFields { get; set; } = new();

        public List<DetailConfig> Details { get; set; } = new();

        /// <summary>
        /// 啟用 Detail Focus 聯動：點擊某層 Detail 時，自動載入下一層 Detail 的關聯資料
        /// </summary>
        public bool EnableDetailFocusCascade { get; set; } = false;

        /// <summary>
        /// 佈局模式：預設為 Tabs（向後兼容）
        /// </summary>
        public LayoutMode Layout { get; set; } = LayoutMode.Tabs;
    }

    public class DetailConfig
    {
        public string DetailTitle { get; set; }
        public string DetailTable { get; set; }
        public string DetailDict { get; set; }
        public string DetailApi { get; set; }

        public List<KeyMapMulti> KeyMap { get; set; } = new();
        public List<string> PkFields { get; set; } = new();
    }

    public class KeyMapMulti
    {
        public string Master { get; set; }
        public string Detail { get; set; }
    }
}
