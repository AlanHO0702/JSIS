using System;
using System.Collections.Generic;

namespace PcbErpApi.Models;

public partial class CurdNoticeBoardUser
{
    public int SerialNum { get; set; }

    public string ToUserId { get; set; } = null!;
}
