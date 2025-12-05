using System;
using System.Collections.Generic;

namespace PcbErpApi.Models
{
    public partial class SpodPoKind
    {
        public int PoKind { get; set; }

        public string PoKindName { get; set; } = null!;   // nvarchar(100) NOT NULL

        public string UseId { get; set; } = null!;        // cvSId / char(8) NOT NULL

        public string? LotNotes { get; set; }             // varchar(12) NULL
    }
}
