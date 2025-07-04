namespace PcbErpApi.Models;

public class WorkOrder
{
    public int Id { get; set; }
    public string OrderNumber { get; set; }
    public string ProductName { get; set; }
    public DateTime DueDate { get; set; }
}