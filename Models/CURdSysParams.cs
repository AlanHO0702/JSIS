// Models/CURdSysParam.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("CURdSysParams")]
public class CURdSysParams
{
    [Key, Column(Order = 0), StringLength(8)]
    public string SystemId { get; set; } = "";

    [Key, Column(Order = 1), StringLength(24)]
    public string ParamId { get; set; } = "";

    [Column(TypeName = "nvarchar(510)")]
    public string? Notes { get; set; }          // 說明

    [Column(TypeName = "nvarchar(255)")]
    public string? Value { get; set; }          // 參數值

    public int ParamType { get; set; }          // 參數類型
    public int ComboStyle { get; set; }         // 顯示型態
    public int IsLock { get; set; }             // 鎖定(0/1)
    public int AllowUserUpdate { get; set; }    // 允許使用者修改(0/1)

    public DateTime? LastDate { get; set; }     // 最後異動日

    [Column(TypeName = "nvarchar(1000)")]
    public string? Note2 { get; set; }          // 備註2（若需要顯示再加）
}
