namespace PcbErpApi.Models;
 public class DetailPickerConfig
    {
        public string ModalId   { get; set; } = "detailPicker";
        public string FetchApi  { get; set; } = "/api/OrderDetailSearch/fetch";
        public string InsertApi { get; set; } = "/api/OrderDetailSearch/insert";
        public string PaperNum  { get; set; } = "";

        // 你原本的拼法（保留兼容）
        public bool ShowRelpace { get; set; } = true;

        // 正確拼法；做成別名，讓 Razor 用 ShowReplace 也 OK
        public bool ShowReplace
        {
            get => ShowRelpace;
            set => ShowRelpace = value;
        }

        // 已有
        public string DictTableName { get; set; } = "";

        // ★ 新增：用來指定「用網址開啟辭典頁」的基底路徑
        //    例如："/DataDict/EditList" 或 "/CURdTableField/EditList"
        public string DictUrlBase { get; set; } = "/DataDict/EditList";
    }

public class LookupConfig
{
    public string Table { get; set; } = "";
    public string Key { get; set; } = "";
    public string Result { get; set; } = ""; // 允許 "A,B,C"（顯示以 " - " 串起）
}
