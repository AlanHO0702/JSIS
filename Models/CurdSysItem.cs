using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PcbErpApi.Models;

public partial class CurdSysItem
{
    [Key]
    public string ItemId { get; set; }
    public string? ItemName { get; set; }
    public string? ClassName { get; set; }
    public string? ObjectName { get; set; }
    public int? Enabled { get; set; }
    public int? WindowState { get; set; }
    public int SerialNum { get; set; }
    public string? SystemId { get; set; }
    public int LevelNo { get; set; }
    public string? SuperId { get; set; }
    public int ItemType { get; set; }

}
