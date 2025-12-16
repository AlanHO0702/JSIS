namespace PcbErpApi.Models;

public sealed class GridToolbarConfig
{
    /// <summary>
    /// Button id prefix. Typically ends with "-". Example: "mg-" or $"{domId}-".
    /// </summary>
    public string Prefix { get; set; } = string.Empty;

    /// <summary>
    /// Show Cancel button.
    /// </summary>
    public bool ShowCancel { get; set; } = false;
}

