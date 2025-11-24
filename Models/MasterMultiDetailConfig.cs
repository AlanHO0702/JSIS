namespace WebRazor.Models
{
    public class MasterMultiDetailConfig
    {
        public string DomId { get; set; }
        public string MasterTitle { get; set; }
        public string MasterTable { get; set; }
        public string MasterDict { get; set; }
        public string MasterApi { get; set; }
        public int MasterTop { get; set; } = 200;

        public List<DetailConfig> Details { get; set; } = new();
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
