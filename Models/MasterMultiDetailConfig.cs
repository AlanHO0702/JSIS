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
        VerticalStack = 2,

        /// <summary>
        /// 資產負債表佈局（頂部：分組選擇器 + 工具列，主要區域：左右分割 Master/Detail）
        /// </summary>
        BalanceSheet = 3
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

        /// <summary>
        /// 啟用拖曳器功能（允許拖曳分隔器調整面板大小）
        /// </summary>
        public bool EnableSplitters { get; set; } = false;

        /// <summary>
        /// 啟用表格計數顯示（顯示「目前筆數 / 總筆數」）
        /// </summary>
        public bool EnableGridCounts { get; set; } = true;

        // ========== BalanceSheet 佈局專用屬性 ==========

        /// <summary>
        /// 頂層選擇器標題（BalanceSheet 佈局專用）
        /// </summary>
        public string TopSelectorTitle { get; set; }

        /// <summary>
        /// 頂層選擇器資料表（BalanceSheet 佈局專用）
        /// </summary>
        public string TopSelectorTable { get; set; }

        /// <summary>
        /// 頂層選擇器字典名稱（BalanceSheet 佈局專用）
        /// </summary>
        public string TopSelectorDict { get; set; }

        /// <summary>
        /// 頂層選擇器高度（BalanceSheet 佈局專用，預設 150px）
        /// </summary>
        public int TopSelectorHeight { get; set; } = 150;

        /// <summary>
        /// 頂層選擇器主鍵欄位（BalanceSheet 佈局專用）
        /// </summary>
        public List<string> TopSelectorPkFields { get; set; } = new();

        /// <summary>
        /// Master 到 TopSelector 的關聯鍵（BalanceSheet 佈局專用）
        /// 例如：Master.SerialNum -> TopSelector.SerialNum
        /// </summary>
        public List<KeyMapMulti> MasterToTopKeyMap { get; set; } = new();

        /// <summary>
        /// 啟用頂部工具列（新增項目功能，BalanceSheet 佈局專用）
        /// </summary>
        public bool EnableTopToolbar { get; set; } = false;
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
