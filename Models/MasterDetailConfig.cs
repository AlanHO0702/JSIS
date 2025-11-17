namespace WebRazor.Models
{
    public sealed class MasterDetailConfig
    {
        /// <summary>唯一 DOM Id（同頁放多組時避免衝突）</summary>
        public string DomId { get; set; } = $"md_{Guid.NewGuid():N}";

        /// <summary>主/明細資料表名稱（拿來打 CommonTable API）</summary>
        public string MasterTable { get; set; } = "";
        public string DetailTable { get; set; } = "";

        /// <summary>主/明細辭典表名（若與資料表相同可不填）</summary>
        public string? MasterDict { get; set; }
        public string? DetailDict { get; set; }

        /// <summary>表頭標題（預設用表名）</summary>
        public string? MasterTitle { get; set; }
        public string? DetailTitle { get; set; }

        /// <summary>
        /// 鍵值對應；例： new [] { new KeyMap("MoneyCode","MoneyCode") }
        /// </summary>
        public KeyMap[] KeyMap { get; set; } = Array.Empty<KeyMap>();

        public string[] DetailKeyFields { get; set; } = Array.Empty<string>();

        /// <summary>主檔預設抓取筆數（使用 CommonTable/TopRows 時用）</summary>
        public int MasterTop { get; set; } = 200;

        /// <summary>是否顯示列號</summary>
        public bool ShowRowNumber { get; set; } = true;

        /// <summary>自訂 API（若不走通用 API，可在這裡覆蓋）</summary>
        public string? MasterApi { get; set; }    // e.g. "/api/AJNdClassMoney"
        public string? DetailApi { get; set; }    // e.g. "/api/AJNdClassMoneyHis?MoneyCode={MoneyCode}"

        /// <summary>可選：主檔排序欄與方向（CommonTable 若支援）</summary>
        public string? MasterOrderBy { get; set; }
        public string? MasterOrderDir { get; set; } = "ASC";

        
    }

    public sealed class KeyMap
    {
        public KeyMap() { }
        public KeyMap(string master, string detail) { Master = master; Detail = detail; }
        public string Master { get; set; } = "";
        public string Detail { get; set; } = "";
    }
}
