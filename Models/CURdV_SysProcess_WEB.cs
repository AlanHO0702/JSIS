using System;

namespace PcbErpApi.Models
{
    public class CURdV_SysProcess_WEB
    {
        public Guid JwtId { get; set; } 
        public string UserId { get; set; } = "";
        public string HostName { get; set; } = "";
        public string ClientIp { get; set; } = "";

        public DateTime? MinLoginTime { get; set; }
        public DateTime? MaxLoginTime { get; set; }

        public int DeviceCount { get; set; }
    }
}
