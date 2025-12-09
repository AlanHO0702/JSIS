using System;
using System.Collections.Generic;

namespace PcbErpApi.Models;

public partial class CurdNoticeBoard
{
    public int SerialNum { get; set; }

    public string? PostUserId { get; set; }

    public DateTime? BuildDate { get; set; }

    public string? Subjects { get; set; }

    public string? BoardText { get; set; }

    public DateTime? BeginDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int? ToAlluser { get; set; }
}
