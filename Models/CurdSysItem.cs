using System;
using System.Collections.Generic;

namespace PcbErpApi.Models;

public partial class CurdSysItem
{
    public string ItemId { get; set; } = null!;

    public string? ItemName { get; set; }

    public int ItemType { get; set; }

    public string? ClassName { get; set; }

    public int? PowerType { get; set; }

    public int FunctionType { get; set; }

    public int WindowState { get; set; }

    public int Enabled { get; set; }

    public int LevelNo { get; set; }

    public string? SuperId { get; set; }

    public int SerialNum { get; set; }

    public string SystemId { get; set; } = null!;

    public string? ObjectName { get; set; }

    public int LinkType { get; set; }

    public int DisplayType { get; set; }

    public int OutputType { get; set; }

    public int KeepDialog { get; set; }

    public int ShowTree { get; set; }

    public string? TableIndex { get; set; }

    public string? Notes { get; set; }

    public int ShowTitle { get; set; }

    public string? PaperId { get; set; }

    public int PaperType { get; set; }

    public string? Ocxtemplete { get; set; }

    public int? BtnClose { get; set; }

    public int? BtnToExcel { get; set; }

    public int? BtnInq { get; set; }

    public int? BtnAdd { get; set; }

    public int? BtnUpdate { get; set; }

    public int? BtnDelete { get; set; }

    public int? BtnVoid { get; set; }

    public int? BtnSendExam { get; set; }

    public int? BtnRejExam { get; set; }

    public int? BtnExam { get; set; }

    public int? BtnUpdateMoney { get; set; }

    public int? BtnUpdateNotes { get; set; }

    public int? BtnPrintPaper { get; set; }

    public int? BtnPrintList { get; set; }

    public string? FlowPrcId { get; set; }

    public int? IsFlowPaper { get; set; }

    public string? FlowCondField { get; set; }

    public string? ItemNameCn { get; set; }

    public string? ItemNameEn { get; set; }

    public string? ItemNameJp { get; set; }

    public string? ItemNameTh { get; set; }

    public string? InsideCode { get; set; }

    public int? CopyFromPowerType { get; set; }

    public int? IShowTracePaperBtn { get; set; }

    public int IReportGridType { get; set; }

    public int? IFlowStopSend { get; set; }

    public string? ClassNameCn { get; set; }

    public string? ClassNameEn { get; set; }

    public string? ClassNameJp { get; set; }

    public string? ClassNameTh { get; set; }

    public int? IFlowBefExamCheck { get; set; }

    public int? IFullHeightDel { get; set; }

    public string? SBiexcelPath { get; set; }

    public int? IAttachment { get; set; }

    public string? FlowCondField2 { get; set; }

    public string? FlowTotalField { get; set; }

    public string? FlowCondField3 { get; set; }
}
