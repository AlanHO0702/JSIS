using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PcbErpApi.Models
{
    [Table("XFLdAgent")]
    public class XFLdAgent
    {
        [Key]
        [Column("iSeq")]
        public int? iSeq { get; set; }

        public string? USERID { get; set; }
        public string? AGENTID { get; set; }

        [Column("SDATE")]
        public DateTime? SDATE { get; set; }

        [Column("EDATE")]
        public DateTime? EDATE { get; set; }

        [Column("AGENTPART")]
        public string? AGENTPART { get; set; }
    }
}
