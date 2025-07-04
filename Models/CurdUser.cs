using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PcbErpApi.Models;

public partial class CurdUser
{
    [Key]
    public string UserId { get; set; } = null!;

    public string? UserName { get; set; }

    public string? UserPassword { get; set; }

    public string? Unitid { get; set; }

    public string? Notes { get; set; }

    public int Permit { get; set; }

    public string UseId { get; set; } = null!;

    public int GlobalUser { get; set; }

    public string Buid { get; set; } = null!;

    public string? Email { get; set; }

    public string? LanguageId { get; set; }

    public int? IChangeDllheight { get; set; }

    public DateTime LastPwChangeDate { get; set; }

    public byte[]? UserSignGraph { get; set; }

    public string? CtelePhone { get; set; }

    public string? Cphone { get; set; }

    public string? CtelePhoneAll { get; set; }

    public string? FontSizeName { get; set; }

    public int? IsStockUser { get; set; }

    public string FlowHisCommtWidth { get; set; } = null!;

    public string FlowDtlHeight { get; set; } = null!;

    public string FlowCommtWidth { get; set; } = null!;

    public int? IsAdmin { get; set; }
}
