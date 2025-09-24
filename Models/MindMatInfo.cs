using System;
using System.Collections.Generic;

namespace PcbErpApi.Models;
public partial class MindMatInfo
{
    public string Partnum { get; set; } = null!;      // cvPartNum，NOT NULL
    public string Revision { get; set; } = null!;      // cvRevision，NOT NULL
    public string? MatClass { get; set; }     // varchar(8)，Nullable
    public string? StockId { get; set; }      // cvSId，Nullable
    public string? Grade { get; set; }        // varchar(1)，Nullable
    public string? MatName { get; set; }      // nvarchar(240)，Nullable
    public string? Gauge { get; set; }        // nvarchar(64)，Nullable
    public string? EngGauge { get; set; }     // nvarchar(1020)，Nullable
    public byte Prepared { get; set; }        // tinyint，NOT NULL
    public string? Unit { get; set; }         // varchar(4)，Nullable
    public decimal StdUsage { get; set; }     // cvDecQnty (decimal(24,8))，NOT NULL
    public decimal? RealUsage { get; set; }   // Nullable
    public decimal? AvgCost { get; set; }     // Nullable
    public short LeadTime { get; set; }       // smallint，NOT NULL
    public decimal StockQnty { get; set; }    // NOT NULL
    public decimal? UnCheckQnty { get; set; }
    public decimal? WilledQnty { get; set; }
    public decimal? ScrapQnty { get; set; }
    public decimal SafeQnty { get; set; }
    public decimal MaxQnty { get; set; }
    public decimal MinQnty { get; set; }
    public decimal FixQnty { get; set; }
    public decimal MinLot { get; set; }
    public decimal? OnWayQnty { get; set; }
    public DateTime? LRUDate { get; set; }    // smalldatetime
    public string? PosId { get; set; }              // cvMId
    public string? Notes { get; set; }              // nvarchar(510)
    public int? AccountType { get; set; }
    public int? MatClassType { get; set; }
    public decimal? ScrapRate { get; set; }
    public byte? AccType { get; set; }
    public byte? IsNormal { get; set; }
    public byte? IsTryCompleted { get; set; }
    public byte? IsChkStandard { get; set; }
    public byte? IsULCertificated { get; set; }
    public decimal Length { get; set; }
    public decimal Width { get; set; }
    public DateTime? ModDateTime { get; set; }
    public string? MatClass1 { get; set; }          // cvSId
    public string? MatClass2 { get; set; }
    public string? MatClass3 { get; set; }
    public decimal? KeepTime { get; set; }
    public decimal? AvgMonQnty { get; set; }
    public decimal? BakFree { get; set; }
    public byte Heavy { get; set; }
    public byte GP { get; set; }
    public byte UL { get; set; }
    public DateTime? GPKeepDate { get; set; }
    public DateTime? ICPKeepDate { get; set; }
    public string? GPURL { get; set; }
    public string? ICPURL { get; set; }
    public byte ICP { get; set; }
    public byte IsHold { get; set; }
    public byte GPIsHold { get; set; }
    public decimal? Weight { get; set; }
    public decimal LeastBag { get; set; }
    public int? FixDate { get; set; }
    public int? SafeDay { get; set; }
    public decimal? AvgDayQnty { get; set; }
    public DateTime? SetFromDate { get; set; }
    public DateTime? SetDueDate { get; set; }
    public decimal OrderQnty { get; set; }
    public decimal? Accept { get; set; }
    public int MB { get; set; }
    public string? CustomerId { get; set; }         // cvCompanyId
    public string? CustomerPartNum { get; set; }
    public string? MapNo { get; set; }
    public string? OldPartNum { get; set; }
    public int? UseIn { get; set; }
    public int Status { get; set; }
    public int IsTrans { get; set; }
    public int IsEMO { get; set; }
    public string? NewPartNum { get; set; }         // cvPartNum
    public string? Description { get; set; }
    public string? Material { get; set; }
    public DateTime? Build_Date { get; set; }
    public string? Build_UserId { get; set; }
    public DateTime? Update_Date { get; set; }
    public string? Update_UserId { get; set; }
    public DateTime? Tran_Date { get; set; }
    public string? Tran_UserId { get; set; }
    public DateTime? ToEMO_Date { get; set; }
    public string? ToEMO_UserId { get; set; }
    public int? SpecType { get; set; }
    public string? UnitQuote { get; set; }
    public string? ChangRate { get; set; }
    public string? PaperNum { get; set; }
    public string? Ref_PartNum { get; set; }
    public int OperationType { get; set; }
    public string? QCType { get; set; }
    public decimal? STDCostUp { get; set; }
    public string? NotesVarch1 { get; set; }
    public string? NotesVarch2 { get; set; }
    public string? NotesVarch3 { get; set; }
    public string? NotesVarch4 { get; set; }
    public string? NotesVarch5 { get; set; }
    public string? NotesVarch6 { get; set; }
    public string? NotesVarch7 { get; set; }
    public string? NotesVarch8 { get; set; }
    public string? NotesVarch9 { get; set; }
    public string? NotesVarch10 { get; set; }
    public decimal? NotesDecim1 { get; set; }
    public decimal? NotesDecim2 { get; set; }
    public decimal? NotesDecim3 { get; set; }
    public decimal? NotesDecim4 { get; set; }
    public decimal? NotesDecim5 { get; set; }
    public decimal? NotesDecim6 { get; set; }
    public decimal? NotesDecim7 { get; set; }
    public decimal? NotesDecim8 { get; set; }
    public decimal? NotesDecim9 { get; set; }
    public decimal? NotesDecim10 { get; set; }
    public decimal? UnitPrice1 { get; set; }
    public decimal? UnitPrice2 { get; set; }
    public decimal? UnitPrice3 { get; set; }
    public decimal? UnitPrice4 { get; set; }
    public decimal? UnitPrice5 { get; set; }
    public decimal? UnitPrice6 { get; set; }
    public decimal? UnitPrice7 { get; set; }
    public decimal? UnitPrice8 { get; set; }
    public decimal? UnitPrice9 { get; set; }
    public decimal? UnitPrice10 { get; set; }
    public int iNeedBatchNum { get; set; }
    public int iNeedDateCode { get; set; }
    public int iNeedExpiredDate { get; set; }
    public decimal? LLPCS { get; set; }
    public int StopOrder { get; set; }
    public string? FromTmpPartNum { get; set; }
    public string? MergePartNum { get; set; }
    public string? GlobalId4Add { get; set; }
    public int? BarCodeId { get; set; }
    public string? AgentCompanyId { get; set; }
    public string? MerMPartNum { get; set; }
    public int AllowVoid { get; set; }
    public string? MetalType { get; set; }
    public int IsSolution { get; set; }
    public int IsNorm { get; set; }
    public int Bonded { get; set; }
    public string? EMO_Rec_UserId { get; set; }
    public DateTime? EMO_Rec_Date { get; set; }
    public string? HDI { get; set; }
    public int? WordNum { get; set; }
    public string? ProdStyle { get; set; }
    public int? iIsKitItem { get; set; }
    public string? BarCode { get; set; }
    public decimal? dHeight { get; set; }
    public decimal? dVolume { get; set; }
    public decimal? dLengthMax { get; set; }
    public decimal? dWidthMax { get; set; }
    public decimal? dHeightMax { get; set; }
    public decimal? dVolumeMax { get; set; }
    public decimal? dPackQntyMax { get; set; }
    public decimal? dVolumeAddon { get; set; }
    public int? IsTranToC { get; set; }
    public string? TranToCUserId { get; set; }
    public DateTime? TranToCDate { get; set; }
    public decimal? ProdLT { get; set; }
    public decimal? ProdTime { get; set; }
    public decimal? MinOrderQnty { get; set; }
    public int iPNStatus { get; set; }
    public int? AllowXOutQnty { get; set; }
    public decimal? AllowXOutRate { get; set; }
    public int? IsTranFrom { get; set; }
    public string? TranFromUserId { get; set; }
    public DateTime? IsTranDate { get; set; }
    public int? TranFacType { get; set; }
    public int? IsTranToB_JH { get; set; }
    public int? IsTranToC_JH { get; set; }
    public int? Finished { get; set; }
    public int? YsLayer_i1 { get; set; }
    public int? YsLayer_i2 { get; set; }
    public decimal? YsLayer_s4 { get; set; }
    public string? YsLayer_s5 { get; set; }
    public string? YsLayer_s6 { get; set; }
    public string? YsLineHole_s1 { get; set; }
    public string? YsLineHole_s2 { get; set; }
    public string? YsLineHole_s3 { get; set; }
    public string? YsLineHole_s4 { get; set; }
    public string? YsOSP_s1 { get; set; }
    public string? YsOSP_s2 { get; set; }
    public string? YsOSP_s3 { get; set; }
    public int? YsOSP_i4 { get; set; }
    public int? YsWord_i1 { get; set; }
    public int? YsWord_i2 { get; set; }
    public int? YsWord_i3 { get; set; }
    public int? YsWord_i4 { get; set; }
    public string? YsWord_s5 { get; set; }
    public int? YsFace_i1 { get; set; }
    public int? YsFace_i2 { get; set; }
    public decimal? YsFace_s3 { get; set; }
    public decimal? YsFace_s4 { get; set; }
    public int? YsFace_i5 { get; set; }
    public int? YsFace_i6 { get; set; }
    public decimal? YsFace_s7 { get; set; }
    public decimal? YsFace_s8 { get; set; }
    public int? YsFace_i9 { get; set; }
    public int? YsFace_i10 { get; set; }
    public string? YsFace_s11 { get; set; }
    public int? YsCNC_i1 { get; set; }
    public int? YsCNC_i2 { get; set; }
    public int? YsCNC_i3 { get; set; }
    public int? YsCNC_i4 { get; set; }
    public string? YsCNC_s5 { get; set; }
    public string? YsCNC_s6 { get; set; }
    public int? YsDateCode_i1 { get; set; }
    public int? YsDateCode_i2 { get; set; }
    public int? YsDateCode_i3 { get; set; }
    public int? YsDateCode_i4 { get; set; }
    public string? YsDateCode_s5 { get; set; }
    public int? YsTest_i1 { get; set; }
    public int? YsTest_i2 { get; set; }
    public int? YsTest_i3 { get; set; }
    public int? YsTest_i4 { get; set; }
    public string? YsTest_s5 { get; set; }
    public int? YsPacking_i1 { get; set; }
    public int? YsPacking_i2 { get; set; }
    public int? YsPacking_i3 { get; set; }
    public string? YsPacking_s4 { get; set; }
    public int? YsPacking_i5 { get; set; }
    public int? YsPacking_i6 { get; set; }
    public string? YsPacking_s7 { get; set; }
    public string? YsPacking_s8 { get; set; }
    public int? YsPacking_i9 { get; set; }
    public int? YsPacking_i10 { get; set; }
    public string? YsPacking_s11 { get; set; }
    public int? YsUL_i1 { get; set; }
    public int? YsUL_i2 { get; set; }
    public string? YsUL_s3 { get; set; }
    public int? YsUL_i4 { get; set; }
    public int? YsUL_i5 { get; set; }
    public int? YsUL_i6 { get; set; }
    public string? YsUL_s7 { get; set; }
    public int? YsLayer_s3 { get; set; }
    public decimal? YsLLPCS_s1 { get; set; }
    public decimal? YsLLPCS_s2 { get; set; }
    public decimal? YsLLPCS_s3 { get; set; }
    public string? YsAttch_s1 { get; set; }
    public string? YsAttch_s2 { get; set; }
    public string? YsAttch_s3 { get; set; }
    public string? YsAttch_s4 { get; set; }
    public string? Cstandard { get; set; }
    public int IsLongDelivery { get; set; }
    public decimal? BoardArea { get; set; }
    public int IsOld { get; set; }
    public string? substrate { get; set; }
    public string? EngGauge2 { get; set; }
    public int PRTOPO { get; set; }

}