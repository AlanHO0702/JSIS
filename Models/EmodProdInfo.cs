using System;
using System.Collections.Generic;

namespace PcbErpApi.Models;

public partial class EmodProdInfo
{
    public string PartNum { get; set; } = null!;

    public string Revision { get; set; } = null!;

    public int? Status { get; set; }

    public int? HoleCheck { get; set; }

    public int? Halted { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime? EditTime { get; set; }

    public DateTime? CustStopDate { get; set; }

    public string? TmpPressId { get; set; }

    public string? TmpMapId { get; set; }

    public string? TmpBomid { get; set; }

    public string? StructId { get; set; }

    public string? StructCode { get; set; }

    public string? CustomerPartNum { get; set; }

    public string? DoType { get; set; }

    public string? Ecno { get; set; }

    public float? Usage { get; set; }

    public float? RftNetThick { get; set; }

    public float? RftNetThickM { get; set; }

    public float? RftNetThickP { get; set; }

    public float? NetThickLow { get; set; }

    public float? NetThickUpp { get; set; }

    public float? Press { get; set; }

    public float? PressP { get; set; }

    public float? PressM { get; set; }

    public float? PressLow { get; set; }

    public float? PressUpp { get; set; }

    public byte? NumOfLayer { get; set; }

    public string? PatternNum { get; set; }

    public byte? OneTimeHole { get; set; }

    public string? MachWay { get; set; }

    public float? GoldArea { get; set; }

    public float? GoldThick { get; set; }

    public string? LineWid { get; set; }

    public string? CustomerId { get; set; }

    public string? ProdClass { get; set; }

    public string? Materialq { get; set; }

    public string? MaterialCode { get; set; }

    public int? Package { get; set; }

    public string? ProDstyle { get; set; }

    public string? Gauge { get; set; }

    public string? LsmColor { get; set; }

    public string? LsmFace { get; set; }

    public string? LsmMaker { get; set; }

    public string? LsmViahole { get; set; }

    public string? CharColor { get; set; }

    public string? CharFace { get; set; }

    public string? MarkCycle { get; set; }

    public string? MarkCdate { get; set; }

    public string? MarkBlue { get; set; }

    public string? Ulmark94V { get; set; }

    public string? UlmarkFace { get; set; }

    public string? PcbClass { get; set; }

    public string? CoreSpec { get; set; }

    public string? Osseal { get; set; }

    public string? DieType { get; set; }

    public string? DieSeq { get; set; }

    public byte? Pinvcu { get; set; }

    public float? VcutDist { get; set; }

    public byte? VcutCross { get; set; }

    public byte? VcutRip { get; set; }

    public float? VcutDisM { get; set; }

    public float? VcutWid { get; set; }

    public int? Pinslash { get; set; }

    public byte? GfslashAngA { get; set; }

    public byte? GfslashAngB { get; set; }

    public float? GfslashHa { get; set; }

    public float? GfslashHb { get; set; }

    public float? GfslashHap { get; set; }

    public float? GfslashHam { get; set; }

    public float? GfslashHbp { get; set; }

    public float? GfslashHbm { get; set; }

    public float? SiBaX1 { get; set; }

    public float? SiBaX2 { get; set; }

    public float? SiBaX3 { get; set; }

    public float? SiBaX4 { get; set; }

    public float? SiBaX5 { get; set; }

    public float? SiBaY1 { get; set; }

    public float? SiBaY2 { get; set; }

    public float? SiBaY3 { get; set; }

    public float? SiBaY4 { get; set; }

    public float? SiBaY5 { get; set; }

    public decimal? GareaC { get; set; }

    public decimal? GareaS { get; set; }

    public float? NiRequest { get; set; }

    public float? GoldRequest { get; set; }

    public float? CuholeMin { get; set; }

    public float? CuholeAvg { get; set; }

    public float? NiPadMin { get; set; }

    public float? TinThick { get; set; }

    public float? ChGoldThick { get; set; }

    public float? NickelThick { get; set; }

    public float? CuviaMax { get; set; }

    public float? CusurfaceMax { get; set; }

    public string? MarkPlace { get; set; }

    public string? Ullayer { get; set; }

    public string? E187 { get; set; }

    public string? E187face { get; set; }

    public string? E187layer { get; set; }

    public string? FireLevel { get; set; }

    public string? FireLevelFace { get; set; }

    public string? FireLevelLayer { get; set; }

    public string? PcbLayer { get; set; }

    public string? CharColorB { get; set; }

    public string? BarCodeColorT { get; set; }

    public string? BarCodeColorB { get; set; }

    public float? BarCodeThick { get; set; }

    public float? BarCodeSize { get; set; }

    public float? ChSilverThick { get; set; }

    public float? ChTinThick { get; set; }

    public float? Membrane { get; set; }

    public int? VcutUsual { get; set; }

    public int? VcutJump { get; set; }

    public int? SlashUsual { get; set; }

    public int? SlashJump { get; set; }

    public int? Slant { get; set; }

    public string? LsmColorB { get; set; }

    public float? ExpectPress { get; set; }

    public string? Ecnno { get; set; }

    public string? SlantHigh { get; set; }

    public string? MadeIn { get; set; }

    public int? CustomerSerial { get; set; }

    public string? CustomerPart { get; set; }

    public string? Enuber { get; set; }

    public string? Ls { get; set; }

    public byte? PinslashO { get; set; }

    public byte? TwoSeq { get; set; }

    public byte? LetterHl { get; set; }

    public byte? VcutHl { get; set; }

    public byte? Lsmbelow { get; set; }

    public string? PinslashA { get; set; }

    public string? PinslashB { get; set; }

    public string? Vstandard { get; set; }

    public string? Postandard { get; set; }

    public string? Gofinger { get; set; }

    public string? Pistandard { get; set; }

    public string? Gifinger { get; set; }

    public string? Cnc { get; set; }

    public string? Gfinger { get; set; }

    public string? CharColorC { get; set; }

    public string? W250 { get; set; }

    public string? Xp120 { get; set; }

    public string? AchGoldThick { get; set; }

    public float? AnickelThick { get; set; }

    public float? AgoldArea { get; set; }

    public float? AgoldThick { get; set; }

    public string? CycleTime { get; set; }

    public float? UsageP { get; set; }

    public string? ProdNotes { get; set; }

    public string? OssealNotes { get; set; }

    public float? VcutAngleAvg { get; set; }

    public float? VcutAngleMin { get; set; }

    public float? VcutAngleMax { get; set; }

    public float? VcutRelicAvg { get; set; }

    public float? VcutRelicMin { get; set; }

    public float? VcutRelicMax { get; set; }

    public float? SlashAngleAvg { get; set; }

    public float? SlashAngleMin { get; set; }

    public float? SlashAngleMax { get; set; }

    public float? SlashRelicAvg { get; set; }

    public float? SlashRelicMin { get; set; }

    public float? SlashRelicMax { get; set; }

    public float? SlashDeepAvg { get; set; }

    public float? SlashDeepMin { get; set; }

    public float? SlashDeepMax { get; set; }

    public string? Flow { get; set; }

    public string? CharMark { get; set; }

    public string? Cncmark { get; set; }

    public float? GoldMin { get; set; }

    public float? GoldMax { get; set; }

    public float? GoldAvg { get; set; }

    public float? NickelMin { get; set; }

    public float? NickelMax { get; set; }

    public float NickelAvg { get; set; }

    public float? GoldRequestMin { get; set; }

    public float? GoldRequestMax { get; set; }

    public float? GoldRequestAvg { get; set; }

    public float NickelRequestMin { get; set; }

    public float NickelRequestMax { get; set; }

    public float NickelRequestAvg { get; set; }

    public float? AchGoldMin { get; set; }

    public float? AchGoldMax { get; set; }

    public float? AchGoldAvg { get; set; }

    public float? AchNickelMin { get; set; }

    public float AchNickelMax { get; set; }

    public float? AchNickelAvg { get; set; }

    public float TinMin { get; set; }

    public float TinMax { get; set; }

    public float TinAvg { get; set; }

    public float? ChsilverMin { get; set; }

    public float? ChsilverMax { get; set; }

    public float? ChsilverAvg { get; set; }

    public float? ChTinMin { get; set; }

    public float? ChTinMax { get; set; }

    public float? ChTinAvg { get; set; }

    public string? Achramify { get; set; }

    public string? AchCs { get; set; }

    public string? AchTong { get; set; }

    public string? CuviaArea { get; set; }

    public string? AchMark { get; set; }

    public float? Cncmax { get; set; }

    public float? Cncmin { get; set; }

    public float? Entekdepth { get; set; }

    public int MergeRoute { get; set; }

    public float? EntekdepthMax { get; set; }

    public string? ProdHints { get; set; }

    public float? Belong { get; set; }

    public string? Cncrequest { get; set; }

    public string? LsmIsIn { get; set; }

    public string? TestManuFac { get; set; }

    public float? AnnularRanMinIn { get; set; }

    public float? AnnularWorkMinIn { get; set; }

    public float? AnnularNonIn { get; set; }

    public float? AnnularRanMinOut { get; set; }

    public float? AnnularWorkMinOut { get; set; }

    public float? AnnularNonOut { get; set; }

    public float? NetOpticAvgC { get; set; }

    public float? NetOpticMinC { get; set; }

    public float? NetOpticMaxC { get; set; }

    public float? NetOpticNonC { get; set; }

    public float? NetOpticAvgS { get; set; }

    public float? NetOpticMinS { get; set; }

    public float? NetOpticMaxS { get; set; }

    public float? NetOpticNonS { get; set; }

    public float? VcutSum { get; set; }

    public string? Designer { get; set; }

    public string? CmapPath { get; set; }

    public string? SmapPath { get; set; }

    public int? IsSoft { get; set; }

    public string? CustomerPartSer { get; set; }

    public string? CustomerSname { get; set; }

    public string? CompColor { get; set; }

    public string? CompType { get; set; }

    public string? SlotColor { get; set; }

    public string? SlotType { get; set; }

    public string? Mtype { get; set; }

    public float? SibarX1 { get; set; }

    public float? SibarX2 { get; set; }

    public float? SibarX3 { get; set; }

    public float? SibarX4 { get; set; }

    public float? SibarY1 { get; set; }

    public float? SibarY2 { get; set; }

    public float? SibarY3 { get; set; }

    public float? SibarY4 { get; set; }

    public float? SoftGoldRequest { get; set; }

    public float? SoftGareaC { get; set; }

    public float? SoftGreaS { get; set; }

    public float? SoftNiRequest { get; set; }

    public float? SoftGoldRequestMax { get; set; }

    public float? SoftGoldRequestMin { get; set; }

    public float? SoftNickelRequestMax { get; set; }

    public float? SoftNickelRequestMin { get; set; }

    public float? SoftGoldRequestAvg { get; set; }

    public float? SoftNickelRequestAvg { get; set; }

    public byte? PinslashBb { get; set; }

    public string? SlashAngleAvgB { get; set; }

    public string? SlashAngleMaxB { get; set; }

    public string? SlashAngleMinB { get; set; }

    public float? SlashDeepAvgB { get; set; }

    public float? SlashDeepMaxB { get; set; }

    public float? SlashDeepMinB { get; set; }

    public float? SlashRelicAvgB { get; set; }

    public float? SlashRelicMaxB { get; set; }

    public float? SlashRelicMinB { get; set; }

    public byte? PinslashOb { get; set; }

    public string? SlantHighB { get; set; }

    public string? CustomerSsname { get; set; }

    public float MinSmd { get; set; }

    public float MinLsm { get; set; }

    public int SlashUsualB { get; set; }

    public int? SlashJumpB { get; set; }

    public int? SlantB { get; set; }

    public int? VcutSumB { get; set; }

    public string? PistandardB { get; set; }

    public string? GifingerB { get; set; }

    public string? PostandardB { get; set; }

    public string? GofingerB { get; set; }

    public float? AchNickelMinb { get; set; }

    public float? AchNickelMaxb { get; set; }

    public float? AchNickelAvgb { get; set; }

    public string? Flowstructure { get; set; }

    public float? HoleToCuMin { get; set; }

    public byte? PlusCompen { get; set; }

    public float? AchNickelMinc { get; set; }

    public float? AchNickelMaxc { get; set; }

    public float? AchNickelAvgc { get; set; }

    public byte? VcutJumpChk { get; set; }

    public float? Pitch { get; set; }

    public string? LsmModel { get; set; }

    public string? Film { get; set; }

    public byte? PrintCarbon { get; set; }

    public byte? Strip { get; set; }

    public string? SharePartNum { get; set; }

    public string? Ultype { get; set; }

    public string? UltypeFace { get; set; }

    public string? UltypeLayer { get; set; }

    public string? Ulno { get; set; }

    public string? Ulnoface { get; set; }

    public string? Ulnolayer { get; set; }

    public float? OilThick { get; set; }

    public byte? Passivation { get; set; }

    public int? InnerGroove { get; set; }

    public int? OuterGroove { get; set; }

    public float? NetTin { get; set; }

    public float? NetTinMin { get; set; }

    public float? NetTinMax { get; set; }

    public float? NetTinAvg { get; set; }

    public byte? TinChk { get; set; }

    public byte? NetTinChk { get; set; }

    public byte? GoldChk { get; set; }

    public byte? NickelChk { get; set; }

    public byte? GoldRequestChk { get; set; }

    public byte? NickelRequestChk { get; set; }

    public byte? SoftGoldReqChk { get; set; }

    public byte? SoftNickelReqChk { get; set; }

    public byte? ChsilverChk { get; set; }

    public byte? ChTinChk { get; set; }

    public byte? VcutDisMchk { get; set; }

    public byte? Entekchk { get; set; }

    public string? Cam { get; set; }

    public byte? SlashAjump { get; set; }

    public byte? SlashBjump { get; set; }

    public float? ThickCuMin { get; set; }

    public float? ThickCuMax { get; set; }

    public float? ThickCuAvg { get; set; }

    public float? FingerWid { get; set; }

    public float? FingerLen { get; set; }

    public int? FingerCount { get; set; }

    public byte? HardGoldChk { get; set; }

    public byte? TentProc { get; set; }

    public int? VcutJumpNum { get; set; }

    public string? PinSlashWay { get; set; }

    public string? PinSlashBbway { get; set; }

    public float? Bgasep { get; set; }

    public float? Bgsline { get; set; }

    public string? EngName { get; set; }

    public string? HaltNotes { get; set; }

    public int? MoldPcs { get; set; }

    public string? MoldPlace { get; set; }

    public double Density { get; set; }

    public double UnitArea { get; set; }

    public float? Hardness { get; set; }

    public float? BarLength { get; set; }

    public float? RollInDiameter { get; set; }

    public float? RollOutDiameter { get; set; }

    public float? RollDepth { get; set; }

    public string? BarPacking { get; set; }

    public string? RollPacking { get; set; }

    public int? UseIn { get; set; }

    public string UseId { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public int Finished { get; set; }

    public string PaperNum { get; set; } = null!;

    public int? ChkField1 { get; set; }

    public int? ChkField2 { get; set; }

    public int DefaultRev { get; set; }

    public int? StatusChk1 { get; set; }

    public int? StatusChk2 { get; set; }

    public int? StatusChk3 { get; set; }

    public int? StatusChk4 { get; set; }

    public int? StatusChk5 { get; set; }

    public int? StatusChk6 { get; set; }

    public int? StatusChk7 { get; set; }

    public int? StatusChk8 { get; set; }

    public int FlowStatus { get; set; }

    public string? ComboRev { get; set; }

    public byte MapType { get; set; }

    public DateTime? DatetimeCol1 { get; set; }

    public DateTime? DatetimeCol2 { get; set; }

    public DateTime? DatetimeCol3 { get; set; }

    public DateTime? DatetimeCol4 { get; set; }

    public int? DisTrig { get; set; }

    public string? WordNum { get; set; }

    public float? UsagePcs { get; set; }

    public string? SpecDemandCont { get; set; }

    public string? PlusVarCol1 { get; set; }

    public string? PlusVarCol2 { get; set; }

    public string? PlusVarCol3 { get; set; }

    public string? PlusVarCol4 { get; set; }

    public string? PlusVarCol5 { get; set; }

    public string? PlusVarCol6 { get; set; }

    public string? PlusVarCol7 { get; set; }

    public string? PlusVarCol8 { get; set; }

    public string? PlusVarCol9 { get; set; }

    public string? PlusVarCol10 { get; set; }

    public string? EngGaugeCus { get; set; }

    public int MixedCut { get; set; }

    public string? ProcSpecCode { get; set; }

    public int? FmeperNum { get; set; }

    public int? QntyPerLot { get; set; }

    public decimal? BoardArea { get; set; }

    public int NeedDivPrintPaper { get; set; }

    public byte ForSqu { get; set; }

    public string? PlusNoteCol1 { get; set; }

    public string? PlusNoteCol2 { get; set; }

    public string? PlusNoteCol3 { get; set; }

    public string? PlusNoteCol4 { get; set; }

    public string? PlusNoteCol5 { get; set; }

    public string? PlusNoteCol6 { get; set; }

    public string? PlusNoteCol7 { get; set; }

    public string? PlusNoteCol8 { get; set; }

    public string? PlusNoteCol9 { get; set; }

    public string? PlusNoteCol10 { get; set; }

    public string? PlusNoteCol11 { get; set; }

    public string? PlusNoteCol12 { get; set; }

    public string? PlusNoteCol13 { get; set; }

    public decimal? GareaC2 { get; set; }

    public decimal? GareaS2 { get; set; }

    public decimal? GareaC3 { get; set; }

    public decimal? GareaS3 { get; set; }

    public decimal? GareaC4 { get; set; }

    public decimal? GareaS4 { get; set; }

    public float? AnnularRanMinOut2 { get; set; }

    public float? AnnularRanMinOut3 { get; set; }

    public float? AnnularRanMinOut4 { get; set; }
}
