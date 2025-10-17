using System;
using System.Collections.Generic;

namespace PcbErpApi.Models;

public partial class CurdBu
{
    public string Buid { get; set; } = null!;

    public string? Buname { get; set; }

    public string? Name { get; set; }

    public string? EnglishName { get; set; }

    public string? Address { get; set; }

    public string? EnglishAddr { get; set; }

    public string? Phone { get; set; }

    public string? Fax { get; set; }

    public byte[]? Logo { get; set; }

    public int Butype { get; set; }

    public string? SuperId { get; set; }

    public int LevelNo { get; set; }

    public string? Dbserver { get; set; }

    public string? Dbname { get; set; }

    public string? LoginName { get; set; }

    public string? LoginPwd { get; set; }

    public string? ReportServer { get; set; }

    public string? SocketServer { get; set; }

    public string? WebreportServer { get; set; }

    public string? WebreportShare { get; set; }

    public string? WebreportLocal { get; set; }

    public int? Seq { get; set; }

    public string? Company { get; set; }

    public byte? ImportType { get; set; }

    public string? ToBuid { get; set; }

    public int IsCompany { get; set; }

    public int BUseIdHead { get; set; }

    public int? DefaultDay4Ap { get; set; }
}
