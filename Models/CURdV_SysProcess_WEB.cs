using System;

namespace PcbErpApi.Models
{
    public class CURdV_SysProcess_WEB
    {
        public string UserId { get; set; } = "";
        public string HostName { get; set; } = "";
        public string ClientIp { get; set; } = "";

        public DateTime? MinLoginTime { get; set; }
        public DateTime? MaxLoginTime { get; set; }
    }
}
