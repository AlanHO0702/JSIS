using Microsoft.EntityFrameworkCore;
using PcbErpApi.Models;

namespace PcbErpApi.Data
{
    public partial class PcbErpContext : DbContext
    {
        public PcbErpContext(DbContextOptions<PcbErpContext> options) : base(options) { }
        public DbSet<SpodOrderMain> SpodOrderMain => Set<SpodOrderMain>();
        public DbSet<CurdUser> CurdUser => Set<CurdUser>();
        public DbSet<MindStockCostPn> MindStockCostPn => Set<MindStockCostPn>();
        public DbSet<CurdSysItem> CurdSysItems { get; set; }
        public DbSet<SpodOrderSub> SpodOrderSub { get; set; }
        public DbSet<CurdSystemSelect> CurdSystemSelects { get; set; }
        public DbSet<CURdTableField> CURdTableFields { get; set; }
        public DbSet<CURdOCXTableFieldLK> CURdOCXTableFieldLK { get; set; }
        public virtual DbSet<CurdUser> CurdUsers { get; set; }
        public virtual DbSet<EmodProdInfo> EmodProdInfos { get; set; }
        public virtual DbSet<CurdTableFieldLang> CurdTableFieldLangs { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CurdUser>(entity =>
            {
                entity.HasKey(e => e.UserId);

                entity.ToTable("CURdUsers", tb =>
                    {
                        tb.HasTrigger("CURdUsers_tD");
                        tb.HasTrigger("CURdUsers_tI");
                        tb.HasTrigger("CURdUsers_tU");
                    });

                entity.Property(e => e.UserId)
                    .HasMaxLength(16)
                    .IsUnicode(false);
                entity.Property(e => e.Buid)
                    .HasMaxLength(8)
                    .IsUnicode(false)
                    .HasDefaultValue("A001")
                    .HasColumnName("BUId");
                entity.Property(e => e.Cphone)
                    .HasMaxLength(24)
                    .IsUnicode(false)
                    .HasColumnName("CPhone");
                entity.Property(e => e.CtelePhone)
                    .HasMaxLength(24)
                    .IsUnicode(false)
                    .HasColumnName("CTelePhone");
                entity.Property(e => e.CtelePhoneAll)
                    .HasMaxLength(24)
                    .IsUnicode(false)
                    .HasColumnName("CTelePhoneAll");
                entity.Property(e => e.Email)
                    .HasMaxLength(50)
                    .HasColumnName("EMAIL");
                entity.Property(e => e.FlowCommtWidth)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasDefaultValueSql("((302))");
                entity.Property(e => e.FlowDtlHeight)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasDefaultValueSql("((120))");
                entity.Property(e => e.FlowHisCommtWidth)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasDefaultValueSql("((679))");
                entity.Property(e => e.FontSizeName)
                    .HasMaxLength(10)
                    .IsUnicode(false);
                entity.Property(e => e.GlobalUser).HasDefaultValue(1);
                entity.Property(e => e.IChangeDllheight)
                    .HasDefaultValue(0)
                    .HasColumnName("iChangeDLLHeight");
                entity.Property(e => e.IsStockUser).HasDefaultValue(0);
                entity.Property(e => e.LanguageId)
                    .HasMaxLength(4)
                    .IsUnicode(false);
                entity.Property(e => e.LastPwChangeDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.Notes).HasMaxLength(255);
                entity.Property(e => e.Permit).HasDefaultValue(1);
                entity.Property(e => e.Unitid)
                    .HasMaxLength(12)
                    .IsUnicode(false);
                entity.Property(e => e.UseId)
                    .HasMaxLength(8)
                    .IsUnicode(false)
                    .HasDefaultValue("");
                entity.Property(e => e.UserName).HasMaxLength(24);
                entity.Property(e => e.UserPassword)
                    .HasMaxLength(512)
                    .IsUnicode(false);
                entity.Property(e => e.UserSignGraph).HasColumnType("image");
            });

            modelBuilder.Entity<CURdTableField>()
            .HasKey(e => new { e.TableName, e.FieldName }); // 這裡改成你真正的複合主鍵欄位

            modelBuilder.Entity<CURdOCXTableFieldLK>()
            .HasKey(e => new { e.TableName, e.FieldName,e.KeyFieldName,e.KeySelfName }); // 這裡改成你真正的複合主鍵欄位

            modelBuilder.Entity<CURdTableField>().ToTable("CURdTableField");

            modelBuilder.Entity<SpodOrderSub>()
            .HasKey(x => new { x.PaperNum, x.Item });

            modelBuilder.Entity<SpodOrderMain>(entity =>
            {
                entity.HasKey(e => e.PaperNum);

                entity.ToTable("SPOdOrderMain", tb =>
                    {
                        tb.HasTrigger("SPOdOrderMain_tD");
                        tb.HasTrigger("SPOdOrderMain_tU");
                        tb.HasTrigger("SPOdOrderMain_tU_Upper");
                    });

                entity.Property(e => e.PaperNum)
                    .HasMaxLength(16)
                    .IsUnicode(false);
                entity.Property(e => e.ArchivesNo).HasMaxLength(255);
                entity.Property(e => e.Assistant).HasMaxLength(40);
                entity.Property(e => e.BuildDate).HasColumnType("datetime");
                entity.Property(e => e.CancelDate).HasColumnType("datetime");
                entity.Property(e => e.CancelUser)
                    .HasMaxLength(16)
                    .IsUnicode(false);
                entity.Property(e => e.Commerce).HasDefaultValue((byte)1);
                entity.Property(e => e.Condition).HasMaxLength(20);
                entity.Property(e => e.CustPonum)
                    .HasMaxLength(50)
                    .HasColumnName("CustPONum");
                entity.Property(e => e.CustomerId)
                    .HasMaxLength(16)
                    .IsUnicode(false)
                    .HasDefaultValue("");
                entity.Property(e => e.Cycle).HasMaxLength(20);
                entity.Property(e => e.Deducted)
                    .HasDefaultValueSql("('0')")
                    .HasColumnType("decimal(24, 8)");
                entity.Property(e => e.DeliverType)
                    .HasMaxLength(10)
                    .IsUnicode(false);
                entity.Property(e => e.DepartId)
                    .HasMaxLength(12)
                    .IsUnicode(false);
                entity.Property(e => e.DepositTotal).HasColumnType("decimal(24, 8)");
                entity.Property(e => e.DisTrig).HasDefaultValue(0);
                entity.Property(e => e.DllHeadFirst)
                    .HasMaxLength(4)
                    .IsUnicode(false)
                    .HasColumnName("dllHeadFirst");
                entity.Property(e => e.DllPaperType).HasColumnName("dllPaperType");
                entity.Property(e => e.DllPaperTypeName)
                    .HasMaxLength(24)
                    .HasColumnName("dllPaperTypeName");
                entity.Property(e => e.FdrCode)
                    .HasMaxLength(16)
                    .IsUnicode(false)
                    .HasDefaultValue("");
                entity.Property(e => e.FileName).HasMaxLength(50);
                entity.Property(e => e.FilePath).HasMaxLength(50);
                entity.Property(e => e.FinishDate).HasColumnType("datetime");
                entity.Property(e => e.FinishUser)
                    .HasMaxLength(16)
                    .IsUnicode(false);
                entity.Property(e => e.FromType).HasDefaultValue(0);
                entity.Property(e => e.IndirectAddr).HasMaxLength(255);
                entity.Property(e => e.InvoiceNotes).HasMaxLength(50);
                entity.Property(e => e.IsCm).HasColumnName("IsCM");
                entity.Property(e => e.IsConsignment)
                    .HasDefaultValueSql("('0')")
                    .HasColumnName("isConsignment");
                entity.Property(e => e.IsPnlprice)
                    .HasDefaultValue(0)
                    .HasColumnName("isPNLPrice");
                entity.Property(e => e.Isproduce).HasDefaultValue((byte)1);
                entity.Property(e => e.NonIoupdate).HasColumnName("NonIOUpdate");
                entity.Property(e => e.Notes).HasMaxLength(255);
                entity.Property(e => e.NotesDecim1).HasColumnType("decimal(24, 8)");
                entity.Property(e => e.NotesDecim10).HasColumnType("decimal(24, 8)");
                entity.Property(e => e.NotesDecim2).HasColumnType("decimal(24, 8)");
                entity.Property(e => e.NotesDecim3).HasColumnType("decimal(24, 8)");
                entity.Property(e => e.NotesDecim4).HasColumnType("decimal(24, 8)");
                entity.Property(e => e.NotesDecim5).HasColumnType("decimal(24, 8)");
                entity.Property(e => e.NotesDecim6).HasColumnType("decimal(24, 8)");
                entity.Property(e => e.NotesDecim7).HasColumnType("decimal(24, 8)");
                entity.Property(e => e.NotesDecim8).HasColumnType("decimal(24, 8)");
                entity.Property(e => e.NotesDecim9).HasColumnType("decimal(24, 8)");
                entity.Property(e => e.NotesVarch1).HasMaxLength(255);
                entity.Property(e => e.NotesVarch10).HasMaxLength(255);
                entity.Property(e => e.NotesVarch11).HasMaxLength(255);
                entity.Property(e => e.NotesVarch12).HasMaxLength(255);
                entity.Property(e => e.NotesVarch13).HasMaxLength(255);
                entity.Property(e => e.NotesVarch14).HasMaxLength(255);
                entity.Property(e => e.NotesVarch15).HasMaxLength(255);
                entity.Property(e => e.NotesVarch16).HasMaxLength(255);
                entity.Property(e => e.NotesVarch17).HasMaxLength(255);
                entity.Property(e => e.NotesVarch18).HasMaxLength(255);
                entity.Property(e => e.NotesVarch19).HasMaxLength(255);
                entity.Property(e => e.NotesVarch2).HasMaxLength(255);
                entity.Property(e => e.NotesVarch20).HasMaxLength(255);
                entity.Property(e => e.NotesVarch21).HasMaxLength(60);
                entity.Property(e => e.NotesVarch22).HasMaxLength(60);
                entity.Property(e => e.NotesVarch23).HasMaxLength(60);
                entity.Property(e => e.NotesVarch24).HasMaxLength(60);
                entity.Property(e => e.NotesVarch25).HasMaxLength(60);
                entity.Property(e => e.NotesVarch26).HasMaxLength(60);
                entity.Property(e => e.NotesVarch27).HasMaxLength(60);
                entity.Property(e => e.NotesVarch28).HasMaxLength(60);
                entity.Property(e => e.NotesVarch29).HasMaxLength(60);
                entity.Property(e => e.NotesVarch3).HasMaxLength(255);
                entity.Property(e => e.NotesVarch30).HasMaxLength(60);
                entity.Property(e => e.NotesVarch31).HasMaxLength(60);
                entity.Property(e => e.NotesVarch32).HasMaxLength(60);
                entity.Property(e => e.NotesVarch33).HasMaxLength(60);
                entity.Property(e => e.NotesVarch34).HasMaxLength(60);
                entity.Property(e => e.NotesVarch35).HasMaxLength(60);
                entity.Property(e => e.NotesVarch36).HasMaxLength(60);
                entity.Property(e => e.NotesVarch37).HasMaxLength(60);
                entity.Property(e => e.NotesVarch38).HasMaxLength(60);
                entity.Property(e => e.NotesVarch39).HasMaxLength(60);
                entity.Property(e => e.NotesVarch4).HasMaxLength(255);
                entity.Property(e => e.NotesVarch40).HasMaxLength(60);
                entity.Property(e => e.NotesVarch5).HasMaxLength(255);
                entity.Property(e => e.NotesVarch6).HasMaxLength(255);
                entity.Property(e => e.NotesVarch7).HasMaxLength(255);
                entity.Property(e => e.NotesVarch8).HasMaxLength(255);
                entity.Property(e => e.NotesVarch9).HasMaxLength(255);
                entity.Property(e => e.Other).HasMaxLength(30);
                entity.Property(e => e.OutAddr).HasMaxLength(255);
                entity.Property(e => e.OutDate).HasColumnType("datetime");
                entity.Property(e => e.PaperDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.PaperId)
                    .HasMaxLength(32)
                    .IsUnicode(false)
                    .HasColumnName("PaperID");
                entity.Property(e => e.PkgTitle).HasMaxLength(255);
                entity.Property(e => e.Pkgtype)
                    .HasMaxLength(50)
                    .HasColumnName("PKGType");
                entity.Property(e => e.Pokind).HasColumnName("POKind");
                entity.Property(e => e.Potype).HasColumnName("POType");
                entity.Property(e => e.PreOrderNum)
                    .HasMaxLength(16)
                    .IsUnicode(false);
                entity.Property(e => e.PrjId)
                    .HasMaxLength(24)
                    .IsUnicode(false);
                entity.Property(e => e.RateToNt)
                    .HasDefaultValue(1m)
                    .HasColumnType("decimal(24, 8)")
                    .HasColumnName("RateToNT");
                entity.Property(e => e.RejectNum)
                    .HasMaxLength(16)
                    .IsUnicode(false);
                entity.Property(e => e.SalesId)
                    .HasMaxLength(16)
                    .IsUnicode(false);
                entity.Property(e => e.ShipTerm)
                    .HasMaxLength(255)
                    .IsUnicode(false);
                entity.Property(e => e.ShipTo).HasMaxLength(255);
                entity.Property(e => e.SourCustomerId)
                    .HasMaxLength(16)
                    .IsUnicode(false)
                    .HasDefaultValue("");
                entity.Property(e => e.SourNum2)
                    .HasMaxLength(16)
                    .IsUnicode(false);
                entity.Property(e => e.Standard).HasMaxLength(20);
                entity.Property(e => e.Standard1).HasMaxLength(10);
                entity.Property(e => e.SubTotal).HasColumnType("decimal(24, 8)");
                entity.Property(e => e.SupplierId)
                    .HasMaxLength(16)
                    .IsUnicode(false);
                entity.Property(e => e.Tax).HasColumnType("decimal(24, 8)");
                entity.Property(e => e.Total).HasColumnType("decimal(24, 8)");
                entity.Property(e => e.TotalAmountOg).HasColumnType("decimal(24, 8)");
                entity.Property(e => e.TradeId)
                    .HasMaxLength(24)
                    .IsUnicode(false);
                entity.Property(e => e.TradePaperId)
                    .HasMaxLength(32)
                    .IsUnicode(false);
                entity.Property(e => e.TradePaperNum)
                    .HasMaxLength(16)
                    .IsUnicode(false);
                entity.Property(e => e.TransPlace).HasMaxLength(255);
                entity.Property(e => e.UpdateDate).HasColumnType("datetime");
                entity.Property(e => e.UseId)
                    .HasMaxLength(16)
                    .IsUnicode(false);
                entity.Property(e => e.UserId)
                    .HasMaxLength(16)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<EmodProdInfo>(entity =>
            {
                entity.HasKey(e => new { e.PartNum, e.Revision });

                entity.ToTable("EMOdProdInfo", tb =>
                    {
                        tb.HasTrigger("EMOdProdInfo_tD");
                        tb.HasTrigger("EMOdProdInfo_tI");
                        tb.HasTrigger("EMOdProdInfo_tU");
                        tb.HasTrigger("EMOdProdInfo_tU2");
                    });

                entity.Property(e => e.PartNum)
                    .HasMaxLength(24)
                    .IsUnicode(false);
                entity.Property(e => e.Revision)
                    .HasMaxLength(8)
                    .IsUnicode(false);
                entity.Property(e => e.AchCs)
                    .HasMaxLength(24)
                    .HasColumnName("AchCS");
                entity.Property(e => e.AchGoldAvg).HasColumnName("AChGoldAVG");
                entity.Property(e => e.AchGoldMax).HasColumnName("AChGoldMAX");
                entity.Property(e => e.AchGoldMin).HasColumnName("AChGoldMIN");
                entity.Property(e => e.AchGoldThick)
                    .HasMaxLength(20)
                    .HasColumnName("AChGoldThick");
                entity.Property(e => e.AchMark).HasMaxLength(24);
                entity.Property(e => e.AchNickelAvg).HasColumnName("AChNickelAVG");
                entity.Property(e => e.AchNickelAvgb).HasColumnName("AChNickelAVGB");
                entity.Property(e => e.AchNickelAvgc).HasColumnName("AChNickelAVGC");
                entity.Property(e => e.AchNickelMax).HasColumnName("AChNickelMAX");
                entity.Property(e => e.AchNickelMaxb).HasColumnName("AChNickelMAXB");
                entity.Property(e => e.AchNickelMaxc).HasColumnName("AChNickelMAXC");
                entity.Property(e => e.AchNickelMin).HasColumnName("AChNickelMIN");
                entity.Property(e => e.AchNickelMinb).HasColumnName("AChNickelMINB");
                entity.Property(e => e.AchNickelMinc).HasColumnName("AChNickelMINC");
                entity.Property(e => e.AchTong).HasMaxLength(24);
                entity.Property(e => e.Achramify).HasMaxLength(24);
                entity.Property(e => e.AgoldArea).HasColumnName("AGoldArea");
                entity.Property(e => e.AgoldThick).HasColumnName("AGoldThick");
                entity.Property(e => e.AnickelThick).HasColumnName("ANickelThick");
                entity.Property(e => e.BarCodeColorB).HasMaxLength(24);
                entity.Property(e => e.BarCodeColorT).HasMaxLength(24);
                entity.Property(e => e.BarPacking).HasMaxLength(20);
                entity.Property(e => e.Belong).HasColumnName("belong");
                entity.Property(e => e.Bgasep).HasColumnName("BGASep");
                entity.Property(e => e.Bgsline).HasColumnName("BGSLine");
                entity.Property(e => e.BoardArea).HasColumnType("decimal(24, 8)");
                entity.Property(e => e.Cam)
                    .HasMaxLength(24)
                    .HasColumnName("CAM");
                entity.Property(e => e.ChTinAvg).HasColumnName("ChTinAVG");
                entity.Property(e => e.ChTinMax).HasColumnName("ChTinMAX");
                entity.Property(e => e.ChTinMin).HasColumnName("ChTinMIN");
                entity.Property(e => e.CharColor).HasMaxLength(20);
                entity.Property(e => e.CharColorB).HasMaxLength(20);
                entity.Property(e => e.CharColorC).HasMaxLength(10);
                entity.Property(e => e.CharFace).HasMaxLength(6);
                entity.Property(e => e.CharMark).HasMaxLength(24);
                entity.Property(e => e.ChsilverAvg).HasColumnName("ChsilverAVG");
                entity.Property(e => e.ChsilverMax).HasColumnName("ChsilverMAX");
                entity.Property(e => e.ChsilverMin).HasColumnName("ChsilverMIN");
                entity.Property(e => e.CmapPath)
                    .HasMaxLength(255)
                    .HasColumnName("CMapPath");
                entity.Property(e => e.Cnc)
                    .HasMaxLength(10)
                    .HasColumnName("CNC");
                entity.Property(e => e.Cncmark)
                    .HasMaxLength(24)
                    .HasColumnName("CNCMark");
                entity.Property(e => e.Cncmax).HasColumnName("CNCMAX");
                entity.Property(e => e.Cncmin).HasColumnName("CNCMIN");
                entity.Property(e => e.Cncrequest)
                    .HasMaxLength(20)
                    .HasColumnName("CNCRequest");
                entity.Property(e => e.ComboRev)
                    .HasMaxLength(8)
                    .IsUnicode(false);
                entity.Property(e => e.CompColor).HasMaxLength(50);
                entity.Property(e => e.CompType).HasMaxLength(50);
                entity.Property(e => e.CoreSpec).HasMaxLength(40);
                entity.Property(e => e.CuholeAvg).HasColumnName("CUHoleAVG");
                entity.Property(e => e.CuholeMin).HasColumnName("CUHoleMIN");
                entity.Property(e => e.CustStopDate)
                    .HasColumnType("datetime")
                    .HasColumnName("Cust_StopDate");
                entity.Property(e => e.CustomerId)
                    .HasMaxLength(20)
                    .IsUnicode(false);
                entity.Property(e => e.CustomerPart).HasMaxLength(12);
                entity.Property(e => e.CustomerPartNum).HasMaxLength(50);
                entity.Property(e => e.CustomerPartSer).HasMaxLength(50);
                entity.Property(e => e.CustomerSname)
                    .HasMaxLength(50)
                    .HasColumnName("CustomerSName");
                entity.Property(e => e.CustomerSsname)
                    .HasMaxLength(255)
                    .HasColumnName("CustomerSSName");
                entity.Property(e => e.CusurfaceMax).HasColumnName("CUSurfaceMax");
                entity.Property(e => e.CuviaArea)
                    .HasMaxLength(24)
                    .HasColumnName("CUViaArea");
                entity.Property(e => e.CuviaMax).HasColumnName("CUViaMax");
                entity.Property(e => e.CycleTime)
                    .HasMaxLength(8)
                    .IsUnicode(false)
                    .HasDefaultValueSql("(0)");
                entity.Property(e => e.DatetimeCol1)
                    .HasColumnType("datetime")
                    .HasColumnName("DatetimeCol_1");
                entity.Property(e => e.DatetimeCol2)
                    .HasColumnType("datetime")
                    .HasColumnName("DatetimeCol_2");
                entity.Property(e => e.DatetimeCol3)
                    .HasColumnType("datetime")
                    .HasColumnName("DatetimeCol_3");
                entity.Property(e => e.DatetimeCol4)
                    .HasColumnType("datetime")
                    .HasColumnName("DatetimeCol_4");
                entity.Property(e => e.Designer)
                    .HasMaxLength(16)
                    .IsUnicode(false);
                entity.Property(e => e.DieSeq).HasMaxLength(8);
                entity.Property(e => e.DieType).HasMaxLength(4);
                entity.Property(e => e.DisTrig).HasDefaultValue(0);
                entity.Property(e => e.DoType).HasMaxLength(10);
                entity.Property(e => e.E187).HasMaxLength(10);
                entity.Property(e => e.E187face)
                    .HasMaxLength(10)
                    .HasColumnName("E187Face");
                entity.Property(e => e.E187layer)
                    .HasMaxLength(10)
                    .HasColumnName("E187Layer");
                entity.Property(e => e.Ecnno)
                    .HasMaxLength(20)
                    .HasColumnName("ECNno");
                entity.Property(e => e.Ecno)
                    .HasMaxLength(12)
                    .HasColumnName("ECNo");
                entity.Property(e => e.EditTime)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.EndDate).HasColumnType("datetime");
                entity.Property(e => e.EngGaugeCus)
                    .HasMaxLength(60)
                    .HasColumnName("EngGauge_Cus");
                entity.Property(e => e.EngName).HasMaxLength(40);
                entity.Property(e => e.Entekchk).HasColumnName("ENTEKChk");
                entity.Property(e => e.Entekdepth).HasColumnName("ENTEKDepth");
                entity.Property(e => e.EntekdepthMax).HasColumnName("ENTEKDepthMAX");
                entity.Property(e => e.Enuber)
                    .HasMaxLength(8)
                    .HasColumnName("ENuber");
                entity.Property(e => e.Film).HasMaxLength(20);
                entity.Property(e => e.Finished).HasDefaultValueSql("('0')");
                entity.Property(e => e.FireLevel)
                    .HasMaxLength(20)
                    .HasColumnName("FireLEVel");
                entity.Property(e => e.FireLevelFace)
                    .HasMaxLength(20)
                    .HasColumnName("FireLEVelFace");
                entity.Property(e => e.FireLevelLayer)
                    .HasMaxLength(20)
                    .HasColumnName("FireLEVelLayer");
                entity.Property(e => e.Flow)
                    .HasMaxLength(20)
                    .HasColumnName("flow");
                entity.Property(e => e.Flowstructure).HasMaxLength(12);
                entity.Property(e => e.FmeperNum).HasColumnName("FMEperNum");
                entity.Property(e => e.ForSqu)
                    .HasDefaultValueSql("('0')")
                    .HasColumnName("ForSQU");
                entity.Property(e => e.GareaC)
                    .HasColumnType("decimal(24, 8)")
                    .HasColumnName("GAreaC");
                entity.Property(e => e.GareaC2)
                    .HasColumnType("decimal(24, 8)")
                    .HasColumnName("GAreaC2");
                entity.Property(e => e.GareaC3)
                    .HasColumnType("decimal(24, 8)")
                    .HasColumnName("GAreaC3");
                entity.Property(e => e.GareaC4)
                    .HasColumnType("decimal(24, 8)")
                    .HasColumnName("GAreaC4");
                entity.Property(e => e.GareaS)
                    .HasColumnType("decimal(24, 8)")
                    .HasColumnName("GAreaS");
                entity.Property(e => e.GareaS2)
                    .HasColumnType("decimal(24, 8)")
                    .HasColumnName("GAreaS2");
                entity.Property(e => e.GareaS3)
                    .HasColumnType("decimal(24, 8)")
                    .HasColumnName("GAreaS3");
                entity.Property(e => e.GareaS4)
                    .HasColumnType("decimal(24, 8)")
                    .HasColumnName("GAreaS4");
                entity.Property(e => e.Gauge).HasMaxLength(20);
                entity.Property(e => e.Gfinger)
                    .HasMaxLength(10)
                    .HasColumnName("GFinger");
                entity.Property(e => e.GfslashAngA).HasColumnName("GFslashAngA");
                entity.Property(e => e.GfslashAngB).HasColumnName("GFslashAngB");
                entity.Property(e => e.GfslashHa).HasColumnName("GFslashHA");
                entity.Property(e => e.GfslashHam).HasColumnName("GFslashHAM");
                entity.Property(e => e.GfslashHap).HasColumnName("GFslashHAP");
                entity.Property(e => e.GfslashHb).HasColumnName("GFslashHB");
                entity.Property(e => e.GfslashHbm).HasColumnName("GFslashHBM");
                entity.Property(e => e.GfslashHbp).HasColumnName("GFslashHBP");
                entity.Property(e => e.Gifinger)
                    .HasMaxLength(20)
                    .HasColumnName("GIFinger");
                entity.Property(e => e.GifingerB)
                    .HasMaxLength(50)
                    .HasColumnName("GIFingerB");
                entity.Property(e => e.Gofinger)
                    .HasMaxLength(20)
                    .HasColumnName("GOFinger");
                entity.Property(e => e.GofingerB)
                    .HasMaxLength(50)
                    .HasColumnName("GOFingerB");
                entity.Property(e => e.GoldAvg).HasColumnName("GoldAVG");
                entity.Property(e => e.GoldChk).HasDefaultValue((byte)0);
                entity.Property(e => e.GoldMax).HasColumnName("GoldMAX");
                entity.Property(e => e.GoldMin).HasColumnName("GoldMIN");
                entity.Property(e => e.GoldRequestAvg).HasColumnName("GoldRequestAVG");
                entity.Property(e => e.GoldRequestChk).HasDefaultValue((byte)0);
                entity.Property(e => e.GoldRequestMax).HasColumnName("GoldRequestMAX");
                entity.Property(e => e.GoldRequestMin).HasColumnName("GoldRequestMIN");
                entity.Property(e => e.HaltNotes).HasMaxLength(30);
                entity.Property(e => e.Halted).HasDefaultValue(0);
                entity.Property(e => e.HardGoldChk).HasDefaultValue((byte)0);
                entity.Property(e => e.HoleCheck).HasDefaultValue(0);
                entity.Property(e => e.LetterHl).HasColumnName("LetterHL");
                entity.Property(e => e.LineWid).HasMaxLength(12);
                entity.Property(e => e.Ls)
                    .HasMaxLength(8)
                    .HasColumnName("LS");
                entity.Property(e => e.LsmColor).HasMaxLength(20);
                entity.Property(e => e.LsmColorB).HasMaxLength(20);
                entity.Property(e => e.LsmFace).HasMaxLength(6);
                entity.Property(e => e.LsmIsIn).HasMaxLength(20);
                entity.Property(e => e.LsmMaker).HasMaxLength(20);
                entity.Property(e => e.LsmModel).HasMaxLength(20);
                entity.Property(e => e.LsmViahole).HasMaxLength(16);
                entity.Property(e => e.MachWay).HasMaxLength(12);
                entity.Property(e => e.MadeIn).HasMaxLength(8);
                entity.Property(e => e.MarkBlue).HasMaxLength(20);
                entity.Property(e => e.MarkCdate).HasMaxLength(16);
                entity.Property(e => e.MarkCycle).HasMaxLength(20);
                entity.Property(e => e.MarkPlace).HasMaxLength(20);
                entity.Property(e => e.MaterialCode).HasMaxLength(10);
                entity.Property(e => e.Materialq).HasMaxLength(20);
                entity.Property(e => e.MergeRoute).HasDefaultValue(1);
                entity.Property(e => e.MinLsm).HasColumnName("minLsm");
                entity.Property(e => e.MinSmd).HasColumnName("minSMD");
                entity.Property(e => e.MoldPlace).HasMaxLength(12);
                entity.Property(e => e.Mtype)
                    .HasMaxLength(50)
                    .HasColumnName("MType");
                entity.Property(e => e.NetTinAvg).HasColumnName("NetTinAVG");
                entity.Property(e => e.NetTinMax).HasColumnName("NetTinMAX");
                entity.Property(e => e.NetTinMin).HasColumnName("NetTinMIN");
                entity.Property(e => e.NiPadMin).HasColumnName("NiPadMIN");
                entity.Property(e => e.NickelAvg).HasColumnName("NickelAVG");
                entity.Property(e => e.NickelChk).HasDefaultValue((byte)0);
                entity.Property(e => e.NickelMax).HasColumnName("NickelMAX");
                entity.Property(e => e.NickelMin).HasColumnName("NickelMIN");
                entity.Property(e => e.NickelRequestAvg).HasColumnName("NickelRequestAVG");
                entity.Property(e => e.NickelRequestChk).HasDefaultValue((byte)0);
                entity.Property(e => e.NickelRequestMax).HasColumnName("NickelRequestMAX");
                entity.Property(e => e.NickelRequestMin).HasColumnName("NickelRequestMIN");
                entity.Property(e => e.Osseal)
                    .HasMaxLength(6)
                    .HasColumnName("OSseal");
                entity.Property(e => e.OssealNotes)
                    .HasMaxLength(255)
                    .HasColumnName("OSsealNotes");
                entity.Property(e => e.PaperNum)
                    .HasMaxLength(32)
                    .IsUnicode(false)
                    .HasDefaultValue("----");
                entity.Property(e => e.PatternNum)
                    .HasMaxLength(8)
                    .IsUnicode(false)
                    .HasDefaultValueSql("(0)");
                entity.Property(e => e.PcbClass).HasMaxLength(8);
                entity.Property(e => e.PcbLayer).HasMaxLength(10);
                entity.Property(e => e.PinSlashBbway)
                    .HasMaxLength(8)
                    .HasColumnName("PinSlashBBWay");
                entity.Property(e => e.PinSlashWay).HasMaxLength(8);
                entity.Property(e => e.Pinslash).HasColumnName("PINSlash");
                entity.Property(e => e.PinslashA)
                    .HasMaxLength(20)
                    .HasColumnName("PINSlashA");
                entity.Property(e => e.PinslashB)
                    .HasMaxLength(20)
                    .HasColumnName("PINSlashB");
                entity.Property(e => e.PinslashBb).HasColumnName("PINSlashBB");
                entity.Property(e => e.PinslashO).HasColumnName("PINSlashO");
                entity.Property(e => e.PinslashOb).HasColumnName("PINSlashOB");
                entity.Property(e => e.Pinvcu).HasColumnName("PINVCU");
                entity.Property(e => e.Pistandard)
                    .HasMaxLength(20)
                    .HasColumnName("PIStandard");
                entity.Property(e => e.PistandardB)
                    .HasMaxLength(50)
                    .HasColumnName("PIStandardB");
                entity.Property(e => e.PlusCompen).HasDefaultValue((byte)0);
                entity.Property(e => e.PlusNoteCol1)
                    .HasMaxLength(500)
                    .HasColumnName("PlusNoteCol_1");
                entity.Property(e => e.PlusNoteCol10)
                    .HasMaxLength(500)
                    .HasColumnName("PlusNoteCol_10");
                entity.Property(e => e.PlusNoteCol11)
                    .HasMaxLength(500)
                    .HasColumnName("PlusNoteCol_11");
                entity.Property(e => e.PlusNoteCol12)
                    .HasMaxLength(500)
                    .HasColumnName("PlusNoteCol_12");
                entity.Property(e => e.PlusNoteCol13)
                    .HasMaxLength(500)
                    .HasColumnName("PlusNoteCol_13");
                entity.Property(e => e.PlusNoteCol2)
                    .HasMaxLength(500)
                    .HasColumnName("PlusNoteCol_2");
                entity.Property(e => e.PlusNoteCol3)
                    .HasMaxLength(500)
                    .HasColumnName("PlusNoteCol_3");
                entity.Property(e => e.PlusNoteCol4)
                    .HasMaxLength(500)
                    .HasColumnName("PlusNoteCol_4");
                entity.Property(e => e.PlusNoteCol5)
                    .HasMaxLength(500)
                    .HasColumnName("PlusNoteCol_5");
                entity.Property(e => e.PlusNoteCol6)
                    .HasMaxLength(500)
                    .HasColumnName("PlusNoteCol_6");
                entity.Property(e => e.PlusNoteCol7)
                    .HasMaxLength(500)
                    .HasColumnName("PlusNoteCol_7");
                entity.Property(e => e.PlusNoteCol8)
                    .HasMaxLength(500)
                    .HasColumnName("PlusNoteCol_8");
                entity.Property(e => e.PlusNoteCol9)
                    .HasMaxLength(500)
                    .HasColumnName("PlusNoteCol_9");
                entity.Property(e => e.PlusVarCol1)
                    .HasMaxLength(30)
                    .HasColumnName("PlusVarCol_1");
                entity.Property(e => e.PlusVarCol10)
                    .HasMaxLength(24)
                    .HasColumnName("PlusVarCol_10");
                entity.Property(e => e.PlusVarCol2)
                    .HasMaxLength(24)
                    .HasColumnName("PlusVarCol_2");
                entity.Property(e => e.PlusVarCol3)
                    .HasMaxLength(24)
                    .HasColumnName("PlusVarCol_3");
                entity.Property(e => e.PlusVarCol4)
                    .HasMaxLength(24)
                    .HasColumnName("PlusVarCol_4");
                entity.Property(e => e.PlusVarCol5)
                    .HasMaxLength(24)
                    .HasColumnName("PlusVarCol_5");
                entity.Property(e => e.PlusVarCol6)
                    .HasMaxLength(24)
                    .HasColumnName("PlusVarCol_6");
                entity.Property(e => e.PlusVarCol7)
                    .HasMaxLength(24)
                    .HasColumnName("PlusVarCol_7");
                entity.Property(e => e.PlusVarCol8)
                    .HasMaxLength(24)
                    .HasColumnName("PlusVarCol_8");
                entity.Property(e => e.PlusVarCol9)
                    .HasMaxLength(24)
                    .HasColumnName("PlusVarCol_9");
                entity.Property(e => e.Postandard)
                    .HasMaxLength(20)
                    .HasColumnName("POStandard");
                entity.Property(e => e.PostandardB)
                    .HasMaxLength(50)
                    .HasColumnName("POStandardB");
                entity.Property(e => e.ProDstyle)
                    .HasMaxLength(40)
                    .HasColumnName("ProDStyle");
                entity.Property(e => e.ProcSpecCode)
                    .HasMaxLength(8)
                    .IsUnicode(false);
                entity.Property(e => e.ProdClass).HasMaxLength(20);
                entity.Property(e => e.ProdHints).HasMaxLength(4000);
                entity.Property(e => e.ProdNotes).HasMaxLength(4000);
                entity.Property(e => e.RftNetThick).HasColumnName("rftNetThick");
                entity.Property(e => e.RftNetThickM).HasColumnName("rftNetThickM");
                entity.Property(e => e.RftNetThickP).HasColumnName("rftNetThickP");
                entity.Property(e => e.RollPacking).HasMaxLength(20);
                entity.Property(e => e.SharePartNum)
                    .HasMaxLength(24)
                    .IsUnicode(false);
                entity.Property(e => e.SibarX1).HasDefaultValue(0f);
                entity.Property(e => e.SibarX2).HasDefaultValue(0f);
                entity.Property(e => e.SibarX3).HasDefaultValue(0f);
                entity.Property(e => e.SibarX4).HasDefaultValue(0f);
                entity.Property(e => e.SibarY1).HasDefaultValue(0f);
                entity.Property(e => e.SibarY2).HasDefaultValue(0f);
                entity.Property(e => e.SibarY3).HasDefaultValue(0f);
                entity.Property(e => e.SibarY4).HasDefaultValue(0f);
                entity.Property(e => e.SlantHigh).HasMaxLength(10);
                entity.Property(e => e.SlantHighB).HasMaxLength(10);
                entity.Property(e => e.SlashAngleAvgB).HasMaxLength(32);
                entity.Property(e => e.SlashAngleMaxB).HasMaxLength(32);
                entity.Property(e => e.SlashAngleMinB).HasMaxLength(32);
                entity.Property(e => e.SlotColor).HasMaxLength(50);
                entity.Property(e => e.SlotType).HasMaxLength(50);
                entity.Property(e => e.SmapPath)
                    .HasMaxLength(255)
                    .HasColumnName("SMapPath");
                entity.Property(e => e.SoftGareaC)
                    .HasDefaultValue(0f)
                    .HasColumnName("SoftGAreaC");
                entity.Property(e => e.SoftGoldReqChk).HasDefaultValue((byte)0);
                entity.Property(e => e.SoftGoldRequest).HasDefaultValue(0f);
                entity.Property(e => e.SoftGoldRequestAvg)
                    .HasDefaultValue(0f)
                    .HasColumnName("SoftGoldRequestAVG");
                entity.Property(e => e.SoftGoldRequestMax)
                    .HasDefaultValue(0f)
                    .HasColumnName("SoftGoldRequestMAX");
                entity.Property(e => e.SoftGoldRequestMin)
                    .HasDefaultValue(0f)
                    .HasColumnName("SoftGoldRequestMIN");
                entity.Property(e => e.SoftGreaS).HasDefaultValue(0f);
                entity.Property(e => e.SoftNiRequest).HasDefaultValue(0f);
                entity.Property(e => e.SoftNickelReqChk).HasDefaultValue((byte)0);
                entity.Property(e => e.SoftNickelRequestAvg)
                    .HasDefaultValue(0f)
                    .HasColumnName("SoftNickelRequestAVG");
                entity.Property(e => e.SoftNickelRequestMax)
                    .HasDefaultValue(0f)
                    .HasColumnName("SoftNickelRequestMAX");
                entity.Property(e => e.SoftNickelRequestMin)
                    .HasDefaultValue(0f)
                    .HasColumnName("SoftNickelRequestMIN");
                entity.Property(e => e.SpecDemandCont).HasMaxLength(36);
                entity.Property(e => e.Status).HasDefaultValue(0);
                entity.Property(e => e.StatusChk1).HasDefaultValue(0);
                entity.Property(e => e.StatusChk2).HasDefaultValue(0);
                entity.Property(e => e.StatusChk3).HasDefaultValue(0);
                entity.Property(e => e.StatusChk4).HasDefaultValue(0);
                entity.Property(e => e.StatusChk5).HasDefaultValue(0);
                entity.Property(e => e.StatusChk6).HasDefaultValue(0);
                entity.Property(e => e.StatusChk7).HasDefaultValue(0);
                entity.Property(e => e.StatusChk8).HasDefaultValue(0);
                entity.Property(e => e.StructCode).HasMaxLength(24);
                entity.Property(e => e.StructId).HasMaxLength(24);
                entity.Property(e => e.TestManuFac).HasMaxLength(24);
                entity.Property(e => e.ThickCuAvg).HasColumnName("ThickCuAVG");
                entity.Property(e => e.ThickCuMax).HasColumnName("ThickCuMAX");
                entity.Property(e => e.ThickCuMin).HasColumnName("ThickCuMIN");
                entity.Property(e => e.TinAvg).HasColumnName("TinAVG");
                entity.Property(e => e.TinMax).HasColumnName("TinMAX");
                entity.Property(e => e.TinMin).HasColumnName("TinMIN");
                entity.Property(e => e.TmpBomid)
                    .HasMaxLength(12)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasColumnName("TmpBOMId");
                entity.Property(e => e.TmpMapId)
                    .HasMaxLength(12)
                    .IsUnicode(false)
                    .IsFixedLength();
                entity.Property(e => e.TmpPressId)
                    .HasMaxLength(12)
                    .IsUnicode(false)
                    .IsFixedLength();
                entity.Property(e => e.Ullayer)
                    .HasMaxLength(20)
                    .HasColumnName("ULLayer");
                entity.Property(e => e.Ulmark94V)
                    .HasMaxLength(20)
                    .HasColumnName("ULMark94V");
                entity.Property(e => e.UlmarkFace)
                    .HasMaxLength(36)
                    .HasColumnName("ULMarkFace");
                entity.Property(e => e.Ulno)
                    .HasMaxLength(20)
                    .HasColumnName("ULNO");
                entity.Property(e => e.Ulnoface)
                    .HasMaxLength(20)
                    .HasColumnName("ULNOFace");
                entity.Property(e => e.Ulnolayer)
                    .HasMaxLength(20)
                    .HasColumnName("ULNOLayer");
                entity.Property(e => e.Ultype)
                    .HasMaxLength(20)
                    .HasColumnName("ULType");
                entity.Property(e => e.UltypeFace)
                    .HasMaxLength(20)
                    .HasColumnName("ULTypeFace");
                entity.Property(e => e.UltypeLayer)
                    .HasMaxLength(20)
                    .HasColumnName("ULTypeLayer");
                entity.Property(e => e.UseId)
                    .HasMaxLength(16)
                    .IsUnicode(false)
                    .HasDefaultValue("A001");
                entity.Property(e => e.UserId)
                    .HasMaxLength(16)
                    .IsUnicode(false)
                    .HasDefaultValue("----");
                entity.Property(e => e.VcutAngleAvg).HasColumnName("VCutAngleAvg");
                entity.Property(e => e.VcutAngleMax).HasColumnName("VCutAngleMax");
                entity.Property(e => e.VcutAngleMin).HasColumnName("VCutAngleMin");
                entity.Property(e => e.VcutDisMchk).HasColumnName("VcutDisMChk");
                entity.Property(e => e.VcutHl).HasColumnName("VcutHL");
                entity.Property(e => e.VcutJumpChk).HasColumnName("VCutJumpChk");
                entity.Property(e => e.VcutJumpNum).HasColumnName("VCutJumpNum");
                entity.Property(e => e.VcutRelicAvg).HasColumnName("VCutRelicAvg");
                entity.Property(e => e.VcutRelicMax).HasColumnName("VCutRelicMax");
                entity.Property(e => e.VcutRelicMin).HasColumnName("VCutRelicMin");
                entity.Property(e => e.VcutSum).HasColumnName("VCutSum");
                entity.Property(e => e.VcutSumB).HasColumnName("VCutSumB");
                entity.Property(e => e.VcutUsual).HasColumnName("VCutUsual");
                entity.Property(e => e.Vstandard)
                    .HasMaxLength(20)
                    .HasColumnName("VStandard");
                entity.Property(e => e.W250).HasMaxLength(10);
                entity.Property(e => e.WordNum)
                    .HasMaxLength(2)
                    .IsUnicode(false);
                entity.Property(e => e.Xp120)
                    .HasMaxLength(10)
                    .HasColumnName("XP120");
            });

            modelBuilder.Entity<CurdTableFieldLang>(entity =>
            {
                entity.HasKey(e => new { e.LanguageId, e.TableName, e.FieldName });

                entity.ToTable("CURdTableFieldLang", tb =>
                    {
                        tb.HasTrigger("CURdTableFieldLang_tI");
                        tb.HasTrigger("CURdTableFieldLang_tU");
                    });

                entity.Property(e => e.LanguageId)
                    .HasMaxLength(4)
                    .IsUnicode(false);
                entity.Property(e => e.TableName)
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.FieldName)
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.DisplayLabel).HasMaxLength(50);
                entity.Property(e => e.EditColor)
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.FontColor).HasMaxLength(50);
                entity.Property(e => e.FontName).HasMaxLength(50);
                entity.Property(e => e.FontStyle).HasMaxLength(50);
                entity.Property(e => e.HintComment).HasMaxLength(255);
                entity.Property(e => e.IFieldHeight).HasColumnName("iFieldHeight");
                entity.Property(e => e.IFieldLeft).HasColumnName("iFieldLeft");
                entity.Property(e => e.IFieldTop).HasColumnName("iFieldTop");
                entity.Property(e => e.IFieldWidth).HasColumnName("iFieldWidth");
                entity.Property(e => e.ILabHeight).HasColumnName("iLabHeight");
                entity.Property(e => e.ILabLeft).HasColumnName("iLabLeft");
                entity.Property(e => e.ILabTop).HasColumnName("iLabTop");
                entity.Property(e => e.ILabWidth).HasColumnName("iLabWidth");
                entity.Property(e => e.ILayColumn).HasColumnName("iLayColumn");
                entity.Property(e => e.ILayRow).HasColumnName("iLayRow");
                entity.Property(e => e.IShowWhere).HasColumnName("iShowWhere");
            });

            OnModelCreatingPartial(modelBuilder);
        }
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    }
}
