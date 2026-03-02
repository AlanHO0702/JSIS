namespace PcbErpApi.Models;

public partial class CurdBuSystem
{
    public string Buid { get; set; } = null!;

    public string SystemId { get; set; } = null!;

    public string? Dbserver { get; set; }

    public string? Dbname { get; set; }

    public string? LoginName { get; set; }

    public string? LoginPwd { get; set; }

    public string? ReportServer { get; set; }

    public string? SocketServer { get; set; }

    public string? WebreportServer { get; set; }

    public string? WebreportShare { get; set; }

    public string? WebreportLocal { get; set; }
}
