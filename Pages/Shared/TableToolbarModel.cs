using PcbErpApi.Models;

public class TableToolbarModel
{
    public string SearchBtnId { get; set; } = "btnSearch";
    public string AddBtnId { get; set; } = "btnAddNew";
    public string DeleteBtnId { get; set; } = "btnBatchDelete";
    public string SearchText { get; set; } = "查詢";
    public string AddText { get; set; } = "新增";
    public string DeleteText { get; set; } = "作廢";
    public string ModalId { get; set; } = "searchModal";
    public List<QueryFieldViewModel> QueryFields { get; set; } = new();
    // ✅ 新增一個屬性，讓不同單據指定對應的 SP
    public string ReportSpName { get; set; } = "";
}
