namespace PcbErpApi.Models;

/// <summary>
/// 物料庫存明細查詢 - 基本資料
/// </summary>
public class MatDetailBaseDto
{
    public int SPId { get; set; }
    public string? PartNum { get; set; }
    public string? MatName { get; set; }
    public string? Unit { get; set; }
    public decimal? CanUseQnty { get; set; }
    public decimal? NeedQnty { get; set; }
    public decimal? StockQnty { get; set; }
}

/// <summary>
/// 庫存彙總
/// </summary>
public class StockSummaryDto
{
    public string? StockId { get; set; }
    public string? StockName { get; set; }
    public string? PartNum { get; set; }
    public decimal? Qnty { get; set; }
    public int? IsMRP { get; set; }
}

/// <summary>
/// 庫存明細
/// </summary>
public class StockDetailDto
{
    public string? StockId { get; set; }
    public string? StockName { get; set; }
    public string? PosId { get; set; }
    public string? LotNum { get; set; }
    public string? BatchNum { get; set; }
    public string? VolumeNum { get; set; }
    public decimal? Qnty { get; set; }
    public decimal? UOMQnty { get; set; }
    public string? UOM { get; set; }
    public decimal? Ratio { get; set; }
    public DateTime? InDate { get; set; }
    public DateTime? ExpiredDate { get; set; }
    public string? SizeLenth { get; set; }
    public string? SizeWidth { get; set; }
    public string? QCCode { get; set; }
}

/// <summary>
/// 供應/需求明細
/// </summary>
public class SupplyDemandDto
{
    public string? ColTitle { get; set; }
    public decimal? Qnty { get; set; }
    public int? FilterNum { get; set; }
    public int? SerialNum { get; set; }
}

/// <summary>
/// 往來單據明細
/// </summary>
public class PaperDetailDto
{
    public string? PaperId { get; set; }
    public string? PaperNum { get; set; }
    public DateTime? PaperDate { get; set; }
    public DateTime? RequireDate { get; set; }
    public string? PaperStatu { get; set; }
    public string? StrCol_1 { get; set; }
    public string? StrCol_2 { get; set; }
    public decimal? PriQnty { get; set; }
    public decimal? SubQnty { get; set; }
    public string? PartNum { get; set; }
    public string? Revision { get; set; }
    public string? MatName { get; set; }
    public int? FilterNum { get; set; }
    public int? Item { get; set; }
    public string? POTypeName { get; set; }
    public string? OutNotInName { get; set; }
}

/// <summary>
/// 預計庫存
/// </summary>
public class PreCountDto
{
    public string? IOSource { get; set; }
    public string? PaperId { get; set; }
    public string? PaperNum { get; set; }
    public DateTime? PaperDate { get; set; }
    public DateTime? RequireDate { get; set; }
    public decimal? IOQnty { get; set; }
    public decimal? StockQnty { get; set; }
    public int? FilterNum { get; set; }
    public int? Item { get; set; }
}

/// <summary>
/// 完整物料明細回應
/// </summary>
public class MatDetailFullResponseDto
{
    public MatDetailBaseDto? BaseData { get; set; }
    public List<StockSummaryDto>? StockSummary { get; set; }
    public List<SupplyDemandDto>? IncomingSupply { get; set; }
    public List<SupplyDemandDto>? OutgoingDemand { get; set; }
    public List<PreCountDto>? PreCount { get; set; }
}