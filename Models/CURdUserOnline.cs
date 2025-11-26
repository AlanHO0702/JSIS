using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PcbErpApi.Models
{
    [Table("CURdUserOnline")]
    public class CURdUserOnline
    {
        [Key]
        public Guid JwtId { get; set; }

        [Required]
        [StringLength(50)]
        public string UserId { get; set; } = "";

        [StringLength(200)]
        public string? HostName { get; set; }


        [StringLength(50)]
        public string? ClientIp { get; set; }   // ⭐ 新增

        public DateTime LoginTime { get; set; }
        public DateTime LastActive { get; set; }


    }
}
