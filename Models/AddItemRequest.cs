namespace PcbErpApi.Models
{
    public class AddItemRequest
    {
        public string ItemId { get; set; }
        public string SuperId { get; set; }
        public string ItemName { get; set; }
        public int ItemType { get; set; }
        
    }
}
