namespace PcbErpApi.Models;
public class DetailPickerConfig
{
    public string ModalId { get; set; } = "detailPicker";
    public string FetchApi { get; set; } = "/api/OrderDetailSearch/fetch";
    public string InsertApi { get; set; } = "/api/OrderDetailSearch/insert";
    public string PaperNum { get; set; } = "";
    public bool ShowReplace { get; set; } = true;

    // 欄位辭典：key=欄位名，如 "PartNum"、"CustomerPartNum"
    public Dictionary<string, LookupConfig> Dicts { get; set; } = new();
}

public class LookupConfig
{
    public string Table { get; set; } = "";
    public string Key { get; set; } = "";
    public string Result { get; set; } = ""; // 允許 "A,B,C"（顯示以 " - " 串起）
}
