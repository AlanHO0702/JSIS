using Microsoft.EntityFrameworkCore;
using PcbErpApi.Models;

namespace PcbErpApi.Data
{
    public partial class PcbErpContext : DbContext
    {
        public PcbErpContext(DbContextOptions<PcbErpContext> options) : base(options) { }
        public DbSet<SpodOrderMain> SpodOrderMain => Set<SpodOrderMain>();
        
        public DbSet<SPOdMPSOutMain> SPOdMPSOutMain => Set<SPOdMPSOutMain>();
        public DbSet<SpodPoKind> SpodPoKind => Set<SpodPoKind>();

        public DbSet<CurdUser> CurdUser => Set<CurdUser>();
        public DbSet<MindStockCostPn> MindStockCostPn => Set<MindStockCostPn>();
        public DbSet<CurdSysItem> CurdSysItems { get; set; }
        public DbSet<CURdUserOnline> CURdUserOnline { get; set; }
        public DbSet<CURdV_SysProcess_WEB> CURdV_SysProcess_WEB { get; set; }

        public DbSet<SpodOrderSub> SpodOrderSub { get; set; }
        public DbSet<SPOdMPSOutSub> SPOdMPSOutSub { get; set; }
        public DbSet<CurdSystemSelect> CurdSystemSelects { get; set; }
        public DbSet<CurdPaperSelected> CURdPaperSelected { get; set; }
        public DbSet<CURdTableField> CURdTableFields { get; set; }
        public DbSet<CURdOCXTableFieldLK> CURdOCXTableFieldLK { get; set; }
        public DbSet<CURdSysParams> CURdSysParams { get; set; } = default!;
        public DbSet<MindMatInfo> MindMatInfo { get; set; }
        public virtual DbSet<AjndCjourSub> AjndCjourSubs { get; set; }

        public virtual DbSet<AjndCjourMain> AjndCjourMains { get; set; }
        public virtual DbSet<CurdTableName> CurdTableNames { get; set; }
        public virtual DbSet<CurdAddonParam> CurdAddonParams { get; set; }
        public virtual DbSet<AjndJourMain> AjndJourMain { get; set; }
        public virtual DbSet<AjndJourSub> AjndJourSub { get; set; }
        public virtual DbSet<AjndDepart> AjndDepart { get; set; }
        public virtual DbSet<CurdUser> CurdUsers { get; set; }
        public virtual DbSet<EmodProdInfo> EmodProdInfos { get; set; }
        public virtual DbSet<EmodProcInfo> EmodProcInfos { get; set; }
        public virtual DbSet<FqcdProcInfo> FqcdProcInfos { get; set; }
        public virtual DbSet<FmedBigProcParam> FmedBigProcParams { get; set; }
        public virtual DbSet<CurdTableFieldLang> CurdTableFieldLangs { get; set; }
        public virtual DbSet<CurdOcxtableSetUp> CurdOcxtableSetUp { get; set; }
        public virtual DbSet<CurdPaperPaper> CurdPaperPaper { get; set; }
        public virtual DbSet<CurdBu> CurdBus { get; set; }
        public virtual DbSet<FmedVProcNisToStd> FmedVProcNisToStd { get; set; }
        public virtual DbSet<FmedIssueMain> FmedIssueMain { get; set; }
        public virtual DbSet<FmedIssueSub> FmedIssueSub { get; set; }
        public virtual DbSet<FmedIssuePo> FmedIssuePo { get; set; }
        public virtual DbSet<FmedIssueMat> FmedIssueMat { get; set; }
        public virtual DbSet<FmedIssueLayer> FmedIssueLayer { get; set; }
        public virtual DbSet<CurdNoticeBoard> CurdNoticeBoards { get; set; }
         public virtual DbSet<SpodClassArea> SpodClassAreas { get; set; }
        public virtual DbSet<CurdNoticeBoardUser> CurdNoticeBoardUsers { get; set; }
        public IEnumerable<object> TabConfigs { get; internal set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<CurdAddonParam>(entity =>
            {
                entity.HasKey(e => new { e.ItemId, e.ParamName });

                entity.ToTable("CURdAddonParams");

                entity.Property(e => e.ItemId)
                    .HasMaxLength(8)
                    .IsUnicode(false)
                    .IsFixedLength();
                entity.Property(e => e.ParamName)
                    .HasMaxLength(24)
                    .IsUnicode(false);
                entity.Property(e => e.CommandText)
                    .HasMaxLength(255)
                    .IsUnicode(false);
                entity.Property(e => e.DefaultValue).HasMaxLength(255);
                entity.Property(e => e.DisplayName).HasMaxLength(255);
                entity.Property(e => e.DisplayNameCn)
                    .HasMaxLength(255)
                    .HasColumnName("DisplayNameCN");
                entity.Property(e => e.DisplayNameEn)
                    .HasMaxLength(255)
                    .HasColumnName("DisplayNameEN");
                entity.Property(e => e.DisplayNameJp)
                    .HasMaxLength(255)
                    .HasColumnName("DisplayNameJP");
                entity.Property(e => e.DisplayNameTh)
                    .HasMaxLength(255)
                    .HasColumnName("DisplayNameTH");
                entity.Property(e => e.EditMask)
                    .HasMaxLength(24)
                    .IsUnicode(false);
                entity.Property(e => e.ParamSn).HasColumnName("ParamSN");
                entity.Property(e => e.SuperId)
                    .HasMaxLength(24)
                    .IsUnicode(false);
            });

            // FMEdIssuePO 複合主鍵配置
            modelBuilder.Entity<FmedIssuePo>(entity =>
            {
                entity.HasKey(e => new { e.PaperNum, e.Item });
            });

            modelBuilder.Entity<AjndCjourMain>(entity =>
            {
                entity.HasKey(e => e.PaperNum);

                entity.ToTable("AJNdCJourMain");

                entity.Property(e => e.PaperNum)
                    .HasMaxLength(16)
                    .IsUnicode(false);
                entity.Property(e => e.BuildDate).HasColumnType("datetime");
                entity.Property(e => e.CancelDate).HasColumnType("datetime");
                entity.Property(e => e.CancelUser)
                    .HasMaxLength(16)
                    .IsUnicode(false);
                entity.Property(e => e.CjourName)
                    .HasMaxLength(50)
                    .HasColumnName("CJourName");
                entity.Property(e => e.DllHeadFirst)
                    .HasMaxLength(4)
                    .IsUnicode(false)
                    .HasColumnName("dllHeadFirst");
                entity.Property(e => e.DllPaperType).HasColumnName("dllPaperType");
                entity.Property(e => e.DllPaperTypeName)
                    .HasMaxLength(24)
                    .HasColumnName("dllPaperTypeName");
                entity.Property(e => e.FinishDate).HasColumnType("datetime");
                entity.Property(e => e.FinishUser)
                    .HasMaxLength(16)
                    .IsUnicode(false);
                entity.Property(e => e.Notes).HasMaxLength(255);
                entity.Property(e => e.PaperDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.PaperId)
                    .HasMaxLength(32)
                    .IsUnicode(false);
                entity.Property(e => e.PaperId2)
                    .HasMaxLength(32)
                    .IsUnicode(false);
                entity.Property(e => e.UseId)
                    .HasMaxLength(16)
                    .IsUnicode(false);
                entity.Property(e => e.UserId)
                    .HasMaxLength(16)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<AjndCjourSub>(entity =>
            {
                entity.HasKey(e => new { e.PaperNum, e.Item });

                entity.ToTable("AJNdCJourSub", tb => tb.HasTrigger("AJNdCJourSub_tIU"));

                entity.Property(e => e.PaperNum)
                    .HasMaxLength(16)
                    .IsUnicode(false);
                entity.Property(e => e.AccId)
                    .HasMaxLength(8)
                    .IsUnicode(false);
                entity.Property(e => e.Amount).HasColumnType("decimal(18, 4)");
                entity.Property(e => e.ChkIn).HasColumnType("decimal(24, 8)");
                entity.Property(e => e.ChkOut).HasColumnType("decimal(24, 8)");
                entity.Property(e => e.Comment).HasMaxLength(255);
                entity.Property(e => e.DepartId)
                    .HasMaxLength(12)
                    .IsUnicode(false);
                entity.Property(e => e.MatClass)
                    .HasMaxLength(8)
                    .IsUnicode(false);
                entity.Property(e => e.Notes).HasMaxLength(255);
                entity.Property(e => e.OgIn).HasColumnType("decimal(24, 8)");
                entity.Property(e => e.OgOut).HasColumnType("decimal(24, 8)");
                entity.Property(e => e.ProjectId).HasMaxLength(16);
                entity.Property(e => e.RateToNt)
                    .HasColumnType("decimal(24, 8)")
                    .HasColumnName("RateToNT");
                entity.Property(e => e.SourNum)
                    .HasMaxLength(16)
                    .IsUnicode(false);
                entity.Property(e => e.SourPaperId)
                    .HasMaxLength(32)
                    .IsUnicode(false);
                entity.Property(e => e.SubAccId)
                    .HasMaxLength(16)
                    .IsUnicode(false);
            });
            modelBuilder.Entity<CurdTableName>(entity =>
            {
                entity.HasKey(e => e.TableName);

                entity.ToTable("CURdTableName");

                entity.Property(e => e.TableName)
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.DisplayLabel).HasMaxLength(50);
                entity.Property(e => e.DisplayLabelCn)
                    .HasMaxLength(50)
                    .HasColumnName("DisplayLabelCN");
                entity.Property(e => e.DisplayLabelEn)
                    .HasMaxLength(50)
                    .HasColumnName("DisplayLabelEN");
                entity.Property(e => e.DisplayLabelJp)
                    .HasMaxLength(50)
                    .HasColumnName("DisplayLabelJP");
                entity.Property(e => e.DisplayLabelTh)
                    .HasMaxLength(50)
                    .HasColumnName("DisplayLabelTH");
                entity.Property(e => e.LogKeildFieldName)
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.OrderByField)
                    .HasMaxLength(128)
                    .IsUnicode(false);
                entity.Property(e => e.RealTableName)
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.SuperId)
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.SystemId)
                    .HasMaxLength(8)
                    .IsUnicode(false)
                    .IsFixedLength();
                entity.Property(e => e.TableNote).HasMaxLength(50);
            });


            modelBuilder.Entity<SpodClassArea>(entity =>
            {
                entity.HasKey(e => new { e.AreaCode, e.UseId });

                entity.ToTable("SPOdClassArea", tb =>
                    {
                        tb.HasTrigger("SPOdClassArea_tD");
                        tb.HasTrigger("SPOdClassArea_tIU");
                    });

                entity.Property(e => e.AreaCode)
                    .HasMaxLength(10)
                    .IsUnicode(false);
                entity.Property(e => e.UseId)
                    .HasMaxLength(8)
                    .IsUnicode(false)
                    .HasDefaultValue("A001");
                entity.Property(e => e.AreaName).HasMaxLength(50);
                entity.Property(e => e.Continent).HasMaxLength(50);
            });

            modelBuilder.Entity<CurdNoticeBoardUser>(entity =>
            {
                entity.HasKey(e => new { e.SerialNum, e.ToUserId });

                entity.ToTable("CURdNoticeBoardUser");

                entity.Property(e => e.ToUserId)
                    .HasMaxLength(16)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CURdV_SysProcess_WEB>()
            .HasNoKey()
            .ToView("CURdV_SysProcess_WEB");

            modelBuilder.Entity<CurdNoticeBoard>(entity =>
            {
                entity.HasKey(e => e.SerialNum);

                entity.ToTable("CURdNoticeBoard");

                entity.Property(e => e.SerialNum).ValueGeneratedNever();
                entity.Property(e => e.BeginDate).HasColumnType("datetime");
                entity.Property(e => e.BoardText).HasMaxLength(4000);
                entity.Property(e => e.BuildDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.EndDate).HasColumnType("datetime");
                entity.Property(e => e.PostUserId)
                    .HasMaxLength(16)
                    .IsUnicode(false);
                entity.Property(e => e.Subjects).HasMaxLength(128);
                entity.Property(e => e.ToAlluser).HasColumnName("ToALLUser");
            });


            // FMEdIssueMat 複合主鍵配置
            modelBuilder.Entity<FmedIssueMat>(entity =>
            {
                entity.HasKey(e => new { e.PaperNum, e.Item });
            });

            // FMEdIssueLayer 複合主鍵配置
            modelBuilder.Entity<FmedIssueLayer>(entity =>
            {
                entity.HasKey(e => new { e.PaperNum, e.Item });
            });

            modelBuilder.Entity<SPOdMPSOutSub>(entity =>
        {
            entity.HasKey(e => new { e.PaperNum, e.Item });

            entity.ToTable("SPOdMPSOutSub", tb =>
                {
                    tb.HasTrigger("SPOdMPSOutSub_tD");
                    tb.HasTrigger("SPOdMPSOutSub_tIU");
                });

            entity.Property(e => e.PaperNum)
                .HasMaxLength(16)
                .IsUnicode(false);
            entity.Property(e => e.Amount)
                .HasDefaultValue(0.0m)
                .HasColumnType("decimal(24, 8)");
            entity.Property(e => e.AmountFinish).HasColumnType("decimal(24, 8)");
            entity.Property(e => e.CustPONum)
                .HasMaxLength(50)
                .HasColumnName("CustPONum");
            entity.Property(e => e.Discount)
                .HasDefaultValue(1m)
                .HasColumnType("decimal(24, 8)");
            entity.Property(e => e.EngGaugeCus)
                .HasMaxLength(60)
                .HasColumnName("EngGauge_Cus");
            entity.Property(e => e.FreeLpiece)
                .HasColumnType("decimal(24, 8)")
                .HasColumnName("FreeLPiece");
            entity.Property(e => e.InvoiceNotes).HasMaxLength(510);
            entity.Property(e => e.ListPrice).HasColumnType("decimal(24, 8)");
            entity.Property(e => e.MatName).HasMaxLength(120);
            entity.Property(e => e.Noslpiece)
                .HasColumnType("decimal(24, 8)")
                .HasColumnName("NOSLPiece");
            entity.Property(e => e.Notes).HasMaxLength(255);
            entity.Property(e => e.PartNum)
                .HasMaxLength(24)
                .IsUnicode(false);
            entity.Property(e => e.Pnlprice)
                .HasColumnType("decimal(24, 8)")
                .HasColumnName("PNLPrice");
            entity.Property(e => e.Pnlqnty)
                .HasColumnType("decimal(24, 8)")
                .HasColumnName("PNLQnty");
            entity.Property(e => e.Pop)
                .HasDefaultValue((byte)4)
                .HasColumnName("POP");
            entity.Property(e => e.PrjId)
                .HasMaxLength(24)
                .IsUnicode(false);
            entity.Property(e => e.Qnty).HasColumnType("decimal(24, 8)");
            entity.Property(e => e.Ratio)
                .HasDefaultValue(1m)
                .HasColumnType("decimal(24, 8)");
            entity.Property(e => e.RealQnty).HasColumnType("decimal(24, 8)");
            entity.Property(e => e.Reason).HasMaxLength(60);
            entity.Property(e => e.Revision)
                .HasMaxLength(8)
                .IsUnicode(false);
            entity.Property(e => e.SourNum)
                .HasMaxLength(16)
                .IsUnicode(false);
            entity.Property(e => e.SourPaperId)
                .HasMaxLength(32)
                .IsUnicode(false);
            entity.Property(e => e.StockId)
                .HasMaxLength(8)
                .IsUnicode(false);
            entity.Property(e => e.SubTotal).HasColumnType("decimal(24, 8)");
            entity.Property(e => e.TaxPrice).HasColumnType("decimal(24, 8)");
            entity.Property(e => e.TaxSubTotal).HasColumnType("decimal(24, 8)");
            entity.Property(e => e.TransHubNum)
                .HasMaxLength(16)
                .IsUnicode(false);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(24, 8)");
            entity.Property(e => e.Uom)
                .HasMaxLength(4)
                .IsUnicode(false)
                .HasColumnName("UOM");
            entity.Property(e => e.Uomprice)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(24, 8)")
                .HasColumnName("UOMPrice");
            entity.Property(e => e.UOMQnty)
                .HasColumnType("decimal(24, 8)")
                .HasColumnName("UOMQnty");
        });
        
        
            modelBuilder.Entity<SPOdMPSOutMain>(entity =>
        {
            entity.HasKey(e => e.PaperNum);

            entity.ToTable("SPOdMPSOutMain", tb =>
                {
                    tb.HasTrigger("SPOdMPSOutMain_tD");
                    tb.HasTrigger("SPOdMPSOutMain_tI");
                    tb.HasTrigger("SPOdMPSOutMain_tU");
                    tb.HasTrigger("SPOdMPSOutMain_tU_Upper");
                });

            entity.Property(e => e.PaperNum)
                .HasMaxLength(16)
                .IsUnicode(false);
            entity.Property(e => e.ArchivesNo).HasMaxLength(255);
            entity.Property(e => e.AreaCode)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.Assistant).HasMaxLength(40);
            entity.Property(e => e.BuildDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CancelDate).HasColumnType("datetime");
            entity.Property(e => e.CancelUser)
                .HasMaxLength(16)
                .IsUnicode(false);
            entity.Property(e => e.ChkNum)
                .HasMaxLength(16)
                .IsUnicode(false);
            entity.Property(e => e.CustPonum)
                .HasMaxLength(50)
                .HasColumnName("CustPONum");
            entity.Property(e => e.CustomerId)
                .HasMaxLength(16)
                .IsUnicode(false)
                .HasDefaultValue("");
            entity.Property(e => e.Cycle).HasMaxLength(16);
            entity.Property(e => e.DepartId)
                .HasMaxLength(12)
                .IsUnicode(false);
            entity.Property(e => e.DepositTotal).HasColumnType("decimal(24, 8)");
            entity.Property(e => e.DepositTotalTax).HasColumnType("decimal(24, 8)");
            entity.Property(e => e.DllHeadFirst)
                .HasMaxLength(4)
                .IsUnicode(false)
                .HasColumnName("dllHeadFirst");
            entity.Property(e => e.DllPaperType).HasColumnName("dllPaperType");
            entity.Property(e => e.DllPaperTypeName)
                .HasMaxLength(24)
                .HasColumnName("dllPaperTypeName");
            entity.Property(e => e.Dodate)
                .HasColumnType("datetime")
                .HasColumnName("DODate");
            entity.Property(e => e.Driver).HasMaxLength(24);
            entity.Property(e => e.ExpectDate).HasColumnType("datetime");
            entity.Property(e => e.ExportType).HasMaxLength(20);
            entity.Property(e => e.FdrCode)
                .HasMaxLength(16)
                .HasDefaultValue("");
            entity.Property(e => e.FinishDate).HasColumnType("datetime");
            entity.Property(e => e.FinishUser)
                .HasMaxLength(16)
                .IsUnicode(false);
            entity.Property(e => e.Forwarder)
                .HasMaxLength(16)
                .IsUnicode(false);
            entity.Property(e => e.FromNum)
                .HasMaxLength(16)
                .IsUnicode(false);
            entity.Property(e => e.FwdNum)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.IndirectAddr).HasMaxLength(255);
            entity.Property(e => e.InvoiceDate).HasColumnType("datetime");
            entity.Property(e => e.InvoiceNotes).HasMaxLength(50);
            entity.Property(e => e.InvoiceNum)
                .HasMaxLength(16)
                .IsUnicode(false);
            entity.Property(e => e.InvoiceType).HasDefaultValue(3);
            entity.Property(e => e.IsPnlprice)
                .HasDefaultValue(0)
                .HasColumnName("isPNLPrice");
            entity.Property(e => e.MoneyCode).HasDefaultValue((byte)1);
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
            entity.Property(e => e.NotesVarch2).HasMaxLength(255);
            entity.Property(e => e.NotesVarch3).HasMaxLength(255);
            entity.Property(e => e.NotesVarch4).HasMaxLength(255);
            entity.Property(e => e.NotesVarch5).HasMaxLength(255);
            entity.Property(e => e.NotesVarch6).HasMaxLength(255);
            entity.Property(e => e.NotesVarch7).HasMaxLength(255);
            entity.Property(e => e.NotesVarch8).HasMaxLength(255);
            entity.Property(e => e.NotesVarch9).HasMaxLength(255);
            entity.Property(e => e.Notify).HasMaxLength(255);
            entity.Property(e => e.OutAddr).HasMaxLength(255);
            entity.Property(e => e.OutWay).HasMaxLength(16);
            entity.Property(e => e.PaperDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PaperId)
                .HasMaxLength(32)
                .IsUnicode(false);
            entity.Property(e => e.PayWayCode).HasDefaultValue(1);
            entity.Property(e => e.PkgTitle).HasMaxLength(255);
            entity.Property(e => e.Port).HasMaxLength(60);
            entity.Property(e => e.PosId)
                .HasMaxLength(12)
                .IsUnicode(false);
            entity.Property(e => e.PreOutDate).HasColumnType("datetime");
            entity.Property(e => e.PrjId)
                .HasMaxLength(24)
                .IsUnicode(false);
            entity.Property(e => e.RateToNt)
                .HasDefaultValue(1m)
                .HasColumnType("decimal(24, 8)")
                .HasColumnName("RateToNT");
            entity.Property(e => e.SalesId)
                .HasMaxLength(16)
                .IsUnicode(false);
            entity.Property(e => e.SalesOutNum)
                .HasMaxLength(16)
                .IsUnicode(false);
            entity.Property(e => e.ShipTerm)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasDefaultValueSql("((0))");
            entity.Property(e => e.ShipTo).HasMaxLength(255);
            entity.Property(e => e.SourNum2)
                .HasMaxLength(16)
                .IsUnicode(false);
            entity.Property(e => e.SubTotal).HasColumnType("decimal(24, 8)");
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
            entity.Property(e => e.TransTypeCode)
                .HasMaxLength(4)
                .IsUnicode(false)
                .HasDefaultValue("0");
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.UseId)
                .HasMaxLength(16)
                .IsUnicode(false);
            entity.Property(e => e.UserId)
                .HasMaxLength(16)
                .IsUnicode(false);
            entity.Property(e => e.Volume).HasColumnType("decimal(24, 8)");
        });

        
            modelBuilder.Entity<FmedIssueSub>(entity =>
        {
            entity.HasKey(e => new { e.PaperNum, e.Item });

            entity.ToTable("FMEdIssueSub", tb =>
                {
                    tb.HasTrigger("FMEdIssueSub_tD");
                    tb.HasTrigger("FMEdIssueSub_tI");
                    tb.HasTrigger("FMEdIssueSub_tU");
                });

            entity.HasIndex(e => e.LotNum, "Idx_FMEdIssueSub_LotNum");

            entity.Property(e => e.PaperNum)
                .HasMaxLength(16)
                .IsUnicode(false);
            entity.Property(e => e.CancelDate).HasColumnType("datetime");
            entity.Property(e => e.CancelUser)
                .HasMaxLength(16)
                .IsUnicode(false);
            entity.Property(e => e.ExpStkTime).HasColumnType("datetime");
            entity.Property(e => e.Ioqnty)
                .HasColumnType("decimal(24, 8)")
                .HasColumnName("IOQnty");
            entity.Property(e => e.LayerId)
                .HasMaxLength(8)
                .IsUnicode(false);
            entity.Property(e => e.LotNum)
                .HasMaxLength(24)
                .IsUnicode(false);
            entity.Property(e => e.Notes).HasMaxLength(255);
            entity.Property(e => e.PartNum)
                .HasMaxLength(24)
                .IsUnicode(false);
            entity.Property(e => e.Pop).HasColumnName("POP");
            entity.Property(e => e.Potype).HasColumnName("POType");
            entity.Property(e => e.ProcCode)
                .HasMaxLength(8)
                .IsUnicode(false);
            entity.Property(e => e.Revision)
                .HasMaxLength(8)
                .IsUnicode(false);
            entity.Property(e => e.SourNum)
                .HasMaxLength(16)
                .IsUnicode(false);
            entity.Property(e => e.SourPaperId)
                .HasMaxLength(32)
                .IsUnicode(false);
            entity.Property(e => e.StockId)
                .HasMaxLength(8)
                .IsUnicode(false)
                .IsFixedLength();
        });
        
            modelBuilder.Entity<FmedIssueMain>(entity =>
        {
            entity.HasKey(e => e.PaperNum);

            entity.ToTable("FMEdIssueMain", tb =>
                {
                    tb.HasTrigger("FMEdIssueMain_tD");
                    tb.HasTrigger("FMEdIssueMain_tI");
                    tb.HasTrigger("FMEdIssueMain_tU");
                });

            entity.HasIndex(e => e.Finished, "IX_FMEdIssueMain_Finished").HasFillFactor(90);

            entity.Property(e => e.PaperNum)
                .HasMaxLength(16)
                .IsUnicode(false);
            entity.Property(e => e.AssGoodQnty).HasColumnType("decimal(24, 8)");
            entity.Property(e => e.AssScrapQnty).HasColumnType("decimal(24, 8)");
            entity.Property(e => e.BIssueNum)
                .HasMaxLength(16)
                .IsUnicode(false)
                .HasColumnName("B_IssueNum");
            entity.Property(e => e.BackupQnty).HasColumnType("decimal(24, 8)");
            entity.Property(e => e.BackupRate).HasColumnType("decimal(24, 8)");
            entity.Property(e => e.BeginDate).HasColumnType("datetime");
            entity.Property(e => e.BombatchQnty)
                .HasColumnType("decimal(24, 8)")
                .HasColumnName("BOMBatchQnty");
            entity.Property(e => e.BompartNum)
                .HasMaxLength(24)
                .IsUnicode(false)
                .HasColumnName("BOMPartNum");
            entity.Property(e => e.BomverCount).HasColumnName("BOMVerCount");
            entity.Property(e => e.BomverNum)
                .HasMaxLength(24)
                .IsUnicode(false)
                .HasColumnName("BOMVerNum");
            entity.Property(e => e.BuildDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CancelDate).HasColumnType("datetime");
            entity.Property(e => e.CancelUser)
                .HasMaxLength(16)
                .IsUnicode(false);
            entity.Property(e => e.ChangeModelUser)
                .HasMaxLength(16)
                .IsUnicode(false);
            entity.Property(e => e.CloseDate).HasColumnType("datetime");
            entity.Property(e => e.CloseUser)
                .HasMaxLength(8)
                .IsUnicode(false);
            entity.Property(e => e.CompanyId)
                .HasMaxLength(16)
                .IsUnicode(false);
            entity.Property(e => e.DateCode)
                .HasMaxLength(12)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.DemenseNum)
                .HasMaxLength(16)
                .IsUnicode(false);
            entity.Property(e => e.DepartId)
                .HasMaxLength(12)
                .IsUnicode(false);
            entity.Property(e => e.DisTrig).HasDefaultValue(0);
            entity.Property(e => e.DllHeadFirst)
                .HasMaxLength(4)
                .IsUnicode(false)
                .HasColumnName("dllHeadFirst");
            entity.Property(e => e.DllPaperType).HasColumnName("dllPaperType");
            entity.Property(e => e.DllPaperTypeName)
                .HasMaxLength(24)
                .HasColumnName("dllPaperTypeName");
            entity.Property(e => e.ExpStkTime).HasColumnType("datetime");
            entity.Property(e => e.FinishDate).HasColumnType("datetime");
            entity.Property(e => e.FinishUser)
                .HasMaxLength(16)
                .IsUnicode(false);
            entity.Property(e => e.Hdi)
                .HasMaxLength(24)
                .IsUnicode(false)
                .HasColumnName("HDI");
            entity.Property(e => e.IFrontProc)
                .HasMaxLength(8)
                .IsUnicode(false)
                .HasColumnName("iFrontProc");
            entity.Property(e => e.IIsSys).HasColumnName("iIsSys");
            entity.Property(e => e.IsMerge).HasDefaultValue(0);
            entity.Property(e => e.IsYs).HasColumnName("IsYS");
            entity.Property(e => e.IssuanceDate).HasColumnType("datetime");
            entity.Property(e => e.IssueAllqnty)
                .HasColumnType("decimal(24, 8)")
                .HasColumnName("IssueALLQnty");
            entity.Property(e => e.LineId)
                .HasMaxLength(8)
                .IsUnicode(false);
            entity.Property(e => e.LotNotes)
                .HasMaxLength(12)
                .IsUnicode(false)
                .HasDefaultValue("一般");
            entity.Property(e => e.McutNum)
                .HasMaxLength(16)
                .IsUnicode(false);
            entity.Property(e => e.MotherIssueNum)
                .HasMaxLength(64)
                .IsUnicode(false);
            entity.Property(e => e.Mpsitem).HasColumnName("MPSItem");
            entity.Property(e => e.Mpsnum)
                .HasMaxLength(16)
                .IsUnicode(false)
                .HasColumnName("MPSNum");
            entity.Property(e => e.MpssumId).HasColumnName("MPSSumId");
            entity.Property(e => e.Notes).HasMaxLength(255);
            entity.Property(e => e.OperationType).HasDefaultValue(1);
            entity.Property(e => e.Other1).HasMaxLength(64);
            entity.Property(e => e.Other2).HasMaxLength(64);
            entity.Property(e => e.Other3).HasMaxLength(64);
            entity.Property(e => e.Other4).HasMaxLength(64);
            entity.Property(e => e.Other5).HasMaxLength(64);
            entity.Property(e => e.PaperDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PaperId)
                .HasMaxLength(32)
                .IsUnicode(false);
            entity.Property(e => e.PartNum)
                .HasMaxLength(24)
                .IsUnicode(false);
            entity.Property(e => e.Poitem).HasColumnName("POItem");
            entity.Property(e => e.Ponum)
                .HasMaxLength(16)
                .IsUnicode(false)
                .HasColumnName("PONum");
            entity.Property(e => e.Potype)
                .HasDefaultValue(0)
                .HasColumnName("POType");
            entity.Property(e => e.PrjId)
                .HasMaxLength(24)
                .IsUnicode(false);
            entity.Property(e => e.QntyPerLot).HasColumnType("decimal(24, 8)");
            entity.Property(e => e.RateToNt)
                .HasDefaultValue(1m)
                .HasColumnType("decimal(24, 8)")
                .HasColumnName("RateToNT");
            entity.Property(e => e.Revision)
                .HasMaxLength(8)
                .IsUnicode(false);
            entity.Property(e => e.SourceId)
                .HasMaxLength(64)
                .IsUnicode(false);
            entity.Property(e => e.SpareQnty).HasColumnType("decimal(24, 8)");
            entity.Property(e => e.SpareRate).HasColumnType("decimal(24, 8)");
            entity.Property(e => e.StdsubTotal)
                .HasColumnType("decimal(24, 8)")
                .HasColumnName("STDSubTotal");
            entity.Property(e => e.Stdtax)
                .HasColumnType("decimal(24, 8)")
                .HasColumnName("STDTax");
            entity.Property(e => e.Stdtotal)
                .HasColumnType("decimal(24, 8)")
                .HasColumnName("STDTotal");
            entity.Property(e => e.StdunitPrice)
                .HasColumnType("decimal(24, 8)")
                .HasColumnName("STDUnitPrice");
            entity.Property(e => e.SubTotal).HasColumnType("decimal(24, 8)");
            entity.Property(e => e.Tax).HasColumnType("decimal(24, 8)");
            entity.Property(e => e.TmpRouteId)
                .HasMaxLength(12)
                .IsUnicode(false);
            entity.Property(e => e.ToMatRequestNum)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Total).HasColumnType("decimal(24, 8)");
            entity.Property(e => e.TotalPcs).HasColumnType("decimal(24, 8)");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(24, 8)");
            entity.Property(e => e.UpdateDate)
                .HasColumnType("datetime")
                .HasColumnName("Update_Date");
            entity.Property(e => e.UseId)
                .HasMaxLength(16)
                .IsUnicode(false);
            entity.Property(e => e.UserId)
                .HasMaxLength(16)
                .IsUnicode(false);
            entity.Property(e => e.WorkDayCount).HasColumnType("decimal(24, 8)");
        });

            modelBuilder.Entity<FmedVProcNisToStd>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("FMEdV_ProcNIS_ToStd");
            
            entity.Property(e => e.AftProcNameString).HasMaxLength(255);
            entity.Property(e => e.BNowPrePass).HasColumnName("bNowPrePass");
            entity.Property(e => e.BprocCode)
                .HasMaxLength(8)
                .IsUnicode(false)
                .HasColumnName("BProcCode");
            entity.Property(e => e.CheckValue)
                .HasMaxLength(1)
                .IsUnicode(false);
            entity.Property(e => e.CustomerId)
                .HasMaxLength(16)
                .IsUnicode(false);
            entity.Property(e => e.DateCode)
                .HasMaxLength(12)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.EqualFgpcs)
                .HasColumnType("decimal(38, 6)")
                .HasColumnName("EqualFGPCS");
            entity.Property(e => e.ExpStkTime).HasColumnType("datetime");
            entity.Property(e => e.FinishQnty).HasColumnType("decimal(24, 8)");
            entity.Property(e => e.GoodPcs)
                .HasColumnType("decimal(38, 6)")
                .HasColumnName("GoodPCS");
            entity.Property(e => e.HaltNotes).HasMaxLength(50);
            entity.Property(e => e.HaltProc)
                .HasMaxLength(8)
                .IsUnicode(false);
            entity.Property(e => e.HaltProcName).HasMaxLength(24);
            entity.Property(e => e.Hdi)
                .HasMaxLength(24)
                .IsUnicode(false)
                .HasColumnName("HDI");
            entity.Property(e => e.IInIqc).HasColumnName("iInIQC");
            entity.Property(e => e.IInIqcname)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("iInIQCName");
            entity.Property(e => e.IIsUrgent).HasColumnName("iIsUrgent");
            entity.Property(e => e.IIsWork).HasColumnName("iIsWork");
            entity.Property(e => e.IOnProcTime).HasColumnName("iOnProcTime");
            entity.Property(e => e.IsScback).HasColumnName("IsSCBack");
            entity.Property(e => e.IssueNum)
                .HasMaxLength(16)
                .IsUnicode(false);
            entity.Property(e => e.L_LLPcs)
                .HasMaxLength(51)
                .IsUnicode(false)
                .HasColumnName("L_LLPcs");
            entity.Property(e => e.LayerId)
                .HasMaxLength(8)
                .IsUnicode(false);
            entity.Property(e => e.Lk_InTime)
                .HasColumnType("datetime")
                .HasColumnName("Lk_InTime");
            entity.Property(e => e.LotNotes)
                .HasMaxLength(12)
                .IsUnicode(false);
            entity.Property(e => e.LotNum)
                .HasMaxLength(24)
                .IsUnicode(false);
            entity.Property(e => e.LotStatusName).HasMaxLength(24);
            entity.Property(e => e.LotXoutQnty)
                .HasColumnType("decimal(38, 8)")
                .HasColumnName("LotXOutQnty");
            entity.Property(e => e.Mark).HasMaxLength(30);
            entity.Property(e => e.MatName).HasMaxLength(120);
            entity.Property(e => e.MidVarchar_12)
                .HasMaxLength(40)
                .HasColumnName("MidVarchar_12");
            entity.Property(e => e.MotherIssueNum)
                .HasMaxLength(64)
                .IsUnicode(false);
            entity.Property(e => e.NickelRequestMax).HasColumnName("NickelRequestMAX");
            entity.Property(e => e.PaperId)
                .HasMaxLength(13)
                .IsUnicode(false);
            entity.Property(e => e.PaperNum)
                .HasMaxLength(16)
                .IsUnicode(false);
            entity.Property(e => e.PartNum)
                .HasMaxLength(24)
                .IsUnicode(false);
            entity.Property(e => e.Pivalue).HasColumnName("PIValue");
            entity.Property(e => e.PivalueNis)
                .HasColumnType("decimal(24, 8)")
                .HasColumnName("PIValueNIS");
            entity.Property(e => e.Pop).HasColumnName("POP");
            entity.Property(e => e.PopName)
                .HasMaxLength(24)
                .HasColumnName("POPName");
            entity.Property(e => e.ProcCode)
                .HasMaxLength(8)
                .IsUnicode(false);
            entity.Property(e => e.ProcName).HasMaxLength(24);
            entity.Property(e => e.ProgressNotes).HasMaxLength(255);
            entity.Property(e => e.Qcstatus).HasColumnName("QCStatus");
            entity.Property(e => e.QcstatusName)
                .HasMaxLength(24)
                .IsUnicode(false)
                .HasColumnName("QCStatusName");
            entity.Property(e => e.Qnty).HasColumnType("decimal(38, 8)");
            entity.Property(e => e.RevNum)
                .HasMaxLength(8)
                .IsUnicode(false);
            entity.Property(e => e.Revision)
                .HasMaxLength(8)
                .IsUnicode(false);
            entity.Property(e => e.SStatusMut)
                .HasMaxLength(24)
                .HasColumnName("sStatusMUT");
            entity.Property(e => e.SWorkSeq)
                .HasMaxLength(24)
                .HasColumnName("sWorkSeq");
            entity.Property(e => e.Sc).HasColumnName("SC");
            entity.Property(e => e.StockId)
                .HasMaxLength(8)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.StrL_LLpiece)
                .HasMaxLength(51)
                .IsUnicode(false)
                .HasColumnName("StrL_LLpiece");
            entity.Property(e => e.Ultype)
                .HasMaxLength(20)
                .HasColumnName("ULType");
            entity.Property(e => e.WorkDate).HasColumnType("datetime");
            entity.Property(e => e.WorkProc)
                .HasMaxLength(8)
                .IsUnicode(false); 
        });
            
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

            modelBuilder.Entity<CurdPaperPaper>(entity =>
            {
                entity.HasKey(e => new { e.PaperId, e.SerialNum });

                entity.ToTable("CURdPaperPaper", tb => tb.HasTrigger("CURdPaperPaper_tD"));

                entity.Property(e => e.PaperId)
                    .HasMaxLength(32)
                    .IsUnicode(false);
                entity.Property(e => e.ClassName)
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.Enabled).HasDefaultValue(1);
                entity.Property(e => e.ItemCount).HasDefaultValue(8);
                entity.Property(e => e.ItemName).HasMaxLength(50);
                entity.Property(e => e.LinkType).HasDefaultValue(1);
                entity.Property(e => e.Notes).HasMaxLength(255);
                entity.Property(e => e.ObjectName)
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.PrintItemId)
                    .HasMaxLength(8)
                    .IsUnicode(false)
                    .IsFixedLength();
                entity.Property(e => e.ShowTitle).HasDefaultValue(1);
                entity.Property(e => e.TableIndex)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<AjndJourMain>(entity =>
            {
                entity.HasKey(e => e.PaperNum);

                entity.ToTable("AJNdJourMain", tb =>
                    {
                        tb.HasTrigger("AJNdJourMain_tD");
                        tb.HasTrigger("AJNdJourMain_tUI");
                    });

                entity.Property(e => e.PaperNum)
                    .HasMaxLength(16)
                    .IsUnicode(false);
                entity.Property(e => e.Accountant)
                    .HasMaxLength(24)
                    .IsUnicode(false);
                entity.Property(e => e.BuildDate).HasColumnType("datetime");
                entity.Property(e => e.CancelDate).HasColumnType("datetime");
                entity.Property(e => e.CancelUser)
                    .HasMaxLength(16)
                    .IsUnicode(false);
                entity.Property(e => e.DisTrig).HasDefaultValue(0);
                entity.Property(e => e.DllHeadFirst)
                    .HasMaxLength(4)
                    .IsUnicode(false)
                    .HasColumnName("dllHeadFirst");
                entity.Property(e => e.DllPaperType).HasColumnName("dllPaperType");
                entity.Property(e => e.DllPaperTypeName)
                    .HasMaxLength(24)
                    .HasColumnName("dllPaperTypeName");
                entity.Property(e => e.FinishDate).HasColumnType("datetime");
                entity.Property(e => e.FinishUser)
                    .HasMaxLength(16)
                    .IsUnicode(false);
                entity.Property(e => e.IsCost).HasDefaultValue(0);
                entity.Property(e => e.JourDate).HasColumnType("datetime");
                entity.Property(e => e.JourId)
                    .HasMaxLength(16)
                    .IsUnicode(false);
                entity.Property(e => e.JourType).HasDefaultValue(3);
                entity.Property(e => e.Notes).HasMaxLength(255);
                entity.Property(e => e.PaperDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.PaperId)
                    .HasMaxLength(32)
                    .IsUnicode(false);
                entity.Property(e => e.RateToNt)
                    .HasColumnType("decimal(24, 8)")
                    .HasColumnName("RateToNT");
                entity.Property(e => e.SourNum)
                    .HasMaxLength(16)
                    .IsUnicode(false);
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 4)");
                entity.Property(e => e.TotalAmountOg).HasColumnType("decimal(24, 8)");
                entity.Property(e => e.UseId)
                    .HasMaxLength(16)
                    .IsUnicode(false);
                entity.Property(e => e.UserId)
                    .HasMaxLength(16)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CurdOcxtableSetUp>(entity =>
            {
                entity.HasKey(e => new { e.ItemId, e.TableName });

                entity.ToTable("CURdOCXTableSetUp");

                entity.Property(e => e.ItemId)
                    .HasMaxLength(8)
                    .IsUnicode(false)
                    .IsFixedLength();
                entity.Property(e => e.TableName).HasMaxLength(50);
                entity.Property(e => e.FilterSql)
                    .HasMaxLength(1024)
                    .IsUnicode(false)
                    .HasColumnName("FilterSQL");
                entity.Property(e => e.LocateKeys)
                    .HasMaxLength(255)
                    .IsUnicode(false);
                entity.Property(e => e.Mdkey)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("MDKey");
                entity.Property(e => e.OrderByField)
                    .HasMaxLength(128)
                    .IsUnicode(false);
                entity.Property(e => e.RunSqlafterAdd)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("RunSQLAfterAdd");
                entity.Property(e => e.TableKind)
                    .HasMaxLength(12)
                    .IsUnicode(false);
                entity.Property(e => e.TableShowWere)
                    .HasMaxLength(12)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<AjndJourSub>(entity =>
            {
                entity.HasKey(e => new { e.PaperNum, e.Item });

                entity.ToTable("AJNdJourSub", tb =>
                    {
                        tb.HasTrigger("AJNdJourSub_tD");
                        tb.HasTrigger("AJNdJourSub_tIU");
                    });

                entity.Property(e => e.PaperNum)
                    .HasMaxLength(16)
                    .IsUnicode(false);
                entity.Property(e => e.AccId)
                    .HasMaxLength(8)
                    .IsUnicode(false);
                entity.Property(e => e.Amount).HasColumnType("decimal(18, 4)");
                entity.Property(e => e.AmountOg).HasColumnType("decimal(24, 8)");
                entity.Property(e => e.AnaCode1)
                    .HasMaxLength(24)
                    .HasColumnName("AnaCode_1");
                entity.Property(e => e.AnaCode2)
                    .HasMaxLength(24)
                    .HasColumnName("AnaCode_2");
                entity.Property(e => e.AnaCode3)
                    .HasMaxLength(24)
                    .HasColumnName("AnaCode_3");
                entity.Property(e => e.AnaCode4)
                    .HasMaxLength(24)
                    .HasColumnName("AnaCode_4");
                entity.Property(e => e.AnaCode5)
                    .HasMaxLength(24)
                    .HasColumnName("AnaCode_5");
                entity.Property(e => e.AnaCode6)
                    .HasMaxLength(24)
                    .HasColumnName("AnaCode_6");
                entity.Property(e => e.BudgetCode)
                    .HasMaxLength(12)
                    .IsUnicode(false);
                entity.Property(e => e.ChkIn).HasColumnType("decimal(24, 8)");
                entity.Property(e => e.ChkOut).HasColumnType("decimal(24, 8)");
                entity.Property(e => e.Comment).HasMaxLength(255);
                entity.Property(e => e.CompanyId)
                    .HasMaxLength(16)
                    .IsUnicode(false);
                entity.Property(e => e.DepartId)
                    .HasMaxLength(12)
                    .IsUnicode(false);
                entity.Property(e => e.MoneyCode).HasDefaultValue(0);
                entity.Property(e => e.Notes).HasMaxLength(255);
                entity.Property(e => e.OgIn).HasColumnType("decimal(24, 8)");
                entity.Property(e => e.OgOut).HasColumnType("decimal(24, 8)");
                entity.Property(e => e.OpenAmount).HasColumnType("decimal(24, 8)");
                entity.Property(e => e.OpenAmountOg).HasColumnType("decimal(24, 8)");
                entity.Property(e => e.PayBackDate).HasColumnType("datetime");
                entity.Property(e => e.PrjId)
                    .HasMaxLength(24)
                    .IsUnicode(false);
                entity.Property(e => e.ProjectId).HasMaxLength(16);
                entity.Property(e => e.ProjectRate)
                    .HasMaxLength(16)
                    .IsUnicode(false);
                entity.Property(e => e.RateToNt)
                    .HasDefaultValue(0m)
                    .HasColumnType("decimal(24, 8)")
                    .HasColumnName("RateToNT");
                entity.Property(e => e.RelationUseId)
                    .HasMaxLength(16)
                    .IsUnicode(false);
                entity.Property(e => e.SourNum)
                    .HasMaxLength(16)
                    .IsUnicode(false);
                entity.Property(e => e.SourNum2)
                    .HasMaxLength(16)
                    .IsUnicode(false);
                entity.Property(e => e.SourPaperId)
                    .HasMaxLength(32)
                    .IsUnicode(false);
                entity.Property(e => e.SubAccId)
                    .HasMaxLength(16)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<MindMatInfo>()
            .HasKey(e => new { e.Partnum, e.Revision });  // ✅ 複合主鍵設定

            modelBuilder.Entity<CURdTableField>()
            .HasKey(e => new { e.TableName, e.FieldName }); // 這裡改成你真正的複合主鍵欄位

            modelBuilder.Entity<CURdOCXTableFieldLK>()
            .HasKey(e => new { e.TableName, e.FieldName, e.KeyFieldName, e.KeySelfName }); // 這裡改成你真正的複合主鍵欄位

            modelBuilder.Entity<CURdTableField>().ToTable("CURdTableField");

            modelBuilder.Entity<SpodOrderSub>()
            .HasKey(x => new { x.PaperNum, x.Item });

            modelBuilder.Entity<CURdSysParams>(e =>
            {
                e.HasKey(x => new { x.SystemId, x.ParamId });   // ★ 複合主鍵

                // （可選）若要保險再指定長度
                e.Property(x => x.SystemId).HasMaxLength(8).IsRequired();
                e.Property(x => x.ParamId).HasMaxLength(24).IsRequired();

                e.ToTable(tb => tb.HasTrigger("CURdSysParams_tIU")); // ✅ EF 會改用普通 UPDATE

            });

            modelBuilder.Entity<CurdBu>(entity =>
            {
                entity.HasKey(e => e.Buid);

                entity.ToTable("CURdBU", tb =>
                    {
                        tb.HasTrigger("CURdBU_tI");
                        tb.HasTrigger("CURdBU_tU");
                    });

                entity.Property(e => e.Buid)
                    .HasMaxLength(8)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasColumnName("BUId");
                entity.Property(e => e.Address).HasMaxLength(255);
                entity.Property(e => e.BUseIdHead)
                    .HasDefaultValue(1)
                    .HasColumnName("bUseIdHead");
                entity.Property(e => e.Buname)
                    .HasMaxLength(24)
                    .HasColumnName("BUName");
                entity.Property(e => e.Butype).HasColumnName("BUType");
                entity.Property(e => e.Company)
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.Dbname)
                    .HasMaxLength(24)
                    .IsUnicode(false)
                    .HasColumnName("DBName");
                entity.Property(e => e.Dbserver)
                    .HasMaxLength(24)
                    .IsUnicode(false)
                    .HasColumnName("DBServer");
                entity.Property(e => e.DefaultDay4Ap)
                    .HasDefaultValue(25)
                    .HasColumnName("DefaultDay4AP");
                entity.Property(e => e.EnglishAddr).HasMaxLength(255);
                entity.Property(e => e.EnglishName).HasMaxLength(50);
                entity.Property(e => e.Fax).HasMaxLength(50);
                entity.Property(e => e.LoginName)
                    .HasMaxLength(24)
                    .IsUnicode(false)
                    .HasDefaultValue("JSIS");
                entity.Property(e => e.LoginPwd)
                    .HasMaxLength(24)
                    .IsUnicode(false)
                    .HasDefaultValue("JSIS")
                    .HasColumnName("LoginPWD");
                entity.Property(e => e.Logo).HasColumnType("image");
                entity.Property(e => e.Name).HasMaxLength(50);
                entity.Property(e => e.Phone).HasMaxLength(50);
                entity.Property(e => e.ReportServer)
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.SocketServer)
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.SuperId)
                    .HasMaxLength(8)
                    .IsUnicode(false)
                    .IsFixedLength();
                entity.Property(e => e.ToBuid)
                    .HasMaxLength(8)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasColumnName("ToBUId");
                entity.Property(e => e.WebreportLocal)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("WEBReportLocal");
                entity.Property(e => e.WebreportServer)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("WEBReportServer");
                entity.Property(e => e.WebreportShare)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("WEBReportShare");
            });
                modelBuilder.Entity<SpodPoKind>(entity =>
            {
                // 資料庫的實際表名：SPODPoKind  （照你 SQL 的名字）
                entity.ToTable("SPODPoKind");

                // PK_SPOdPoKind clustered, unique, primary key located on PRIMARY PoKind, UseId
                entity.HasKey(e => new { e.PoKind, e.UseId });

                // PoKind int not null（預設就可以，不一定要再設定）

                entity.Property(e => e.PoKindName)
                    .HasMaxLength(100);          // nvarchar(100)

                entity.Property(e => e.UseId)
                    .HasMaxLength(8)
                    .IsUnicode(false)            // char(8)，不是 nvarchar
                    .IsFixedLength()
                    .HasDefaultValue("A001");    // 如果資料庫有預設值就一起寫

                entity.Property(e => e.LotNotes)
                    .HasMaxLength(12)
                    .IsUnicode(false);           // varchar(12)
            });
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

            modelBuilder.Entity<CurdPaperSelected>(entity =>
           {
               entity.HasKey(e => new { e.PaperId, e.TableName, e.ColumnName, e.DefaultEqual });

               entity.ToTable("CURdPaperSelected", tb =>
                   {
                       tb.HasTrigger("CURdPaperSelected_tD");
                       tb.HasTrigger("CURdPaperSelected_tI");
                   });

               entity.Property(e => e.PaperId)
                   .HasMaxLength(50)
                   .IsUnicode(false);
               entity.Property(e => e.TableName)
                   .HasMaxLength(50)
                   .IsUnicode(false);
               entity.Property(e => e.ColumnName)
                   .HasMaxLength(50)
                   .IsUnicode(false);
               entity.Property(e => e.DefaultEqual)
                   .HasMaxLength(10)
                   .IsUnicode(false);
               entity.Property(e => e.AliasName)
                   .HasMaxLength(50)
                   .IsUnicode(false);
               entity.Property(e => e.ColumnCaption).HasMaxLength(50);
               entity.Property(e => e.CommandText)
                   .HasMaxLength(255)
                   .IsUnicode(false);
               entity.Property(e => e.DefaultValue).HasMaxLength(1024);
               entity.Property(e => e.EditMask)
                   .HasMaxLength(24)
                   .IsUnicode(false);
               entity.Property(e => e.IReadOnly).HasColumnName("iReadOnly");
               entity.Property(e => e.IVisible)
                   .HasDefaultValue(1)
                   .HasColumnName("iVisible");
               entity.Property(e => e.ParamType).HasDefaultValue(6);
               entity.Property(e => e.ParamValue)
                   .HasMaxLength(255)
                   .IsUnicode(false);
               entity.Property(e => e.SuperId)
                   .HasMaxLength(24)
                   .IsUnicode(false);
               entity.Property(e => e.TableKind)
                   .HasMaxLength(12)
                   .IsUnicode(false);
           });

            modelBuilder.Entity<EmodProcInfo>(entity =>
            {
                entity.HasKey(e => e.ProcCode);

                entity.ToTable("EMOdProcInfo");

                entity.Property(e => e.ProcCode)
                    .HasMaxLength(8)
                    .IsUnicode(false);
                entity.Property(e => e.ProcName).HasMaxLength(40);
                entity.Property(e => e.CostCenter)
                    .HasMaxLength(8)
                    .IsUnicode(false);
                entity.Property(e => e.DepartId)
                    .HasMaxLength(12)
                    .IsUnicode(false);
                entity.Property(e => e.CapId)
                    .HasMaxLength(12)
                    .IsUnicode(false);
                entity.Property(e => e.FromTime)
                    .HasMaxLength(5)
                    .IsUnicode(false);
                entity.Property(e => e.DueTime)
                    .HasMaxLength(5)
                    .IsUnicode(false);
                entity.Property(e => e.HaltTime).HasColumnType("datetime");
                entity.Property(e => e.Memo).HasMaxLength(100);
                entity.Property(e => e.RuleItem).HasMaxLength(16);
                entity.Property(e => e.ProcGroup)
                    .HasMaxLength(4)
                    .IsUnicode(false);
                entity.Property(e => e.Other1).HasMaxLength(50);
                entity.Property(e => e.Other2).HasMaxLength(50);
                entity.Property(e => e.Other3).HasMaxLength(50);
                entity.Property(e => e.Other4).HasMaxLength(50);
                entity.Property(e => e.Other5).HasMaxLength(50);
                entity.Property(e => e.Qctype)
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .HasColumnName("QCType");
                entity.Property(e => e.ProcNameEng).HasMaxLength(40);
                entity.Property(e => e.ComputeId)
                    .HasMaxLength(8)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<FqcdProcInfo>(entity =>
            {
                entity.HasKey(e => e.BProcCode);

                entity.ToTable("FQCdProcInfo");

                entity.Property(e => e.BProcCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.BProcName).HasMaxLength(200);
                entity.Property(e => e.SQU4ValueNum)
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.ProfitCenter)
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.BProcNameEng).HasMaxLength(200);

                // Ignore 用於 lookup 的欄位（不在資料表中）
                entity.Ignore(e => e.BPTypeNameM);
            });

            modelBuilder.Entity<FmedBigProcParam>(entity =>
            {
                entity.HasKey(e => new { e.ProcCode, e.ParamId });

                entity.ToTable("FMEdBigProcParam");

                entity.Property(e => e.ProcCode)
                    .HasMaxLength(8)
                    .IsUnicode(false);
                entity.Property(e => e.ProcGroup)
                    .HasMaxLength(4)
                    .IsUnicode(false);
                entity.Property(e => e.ParamId)
                    .HasMaxLength(20)
                    .IsUnicode(false);
                entity.Property(e => e.ParamName).HasMaxLength(100);
                entity.Property(e => e.ParamValue).HasMaxLength(100);
                entity.Property(e => e.StdValue).HasMaxLength(100);
                entity.Property(e => e.Unit).HasMaxLength(20);
                entity.Property(e => e.ParamType)
                    .HasMaxLength(20)
                    .IsUnicode(false);
                entity.Property(e => e.Memo).HasMaxLength(200);
                entity.Property(e => e.CreateDate).HasColumnType("datetime");
                entity.Property(e => e.ModifyDate).HasColumnType("datetime");
                entity.Property(e => e.Other1).HasMaxLength(50);
                entity.Property(e => e.Other2).HasMaxLength(50);
                entity.Property(e => e.Other3).HasMaxLength(50);
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

            modelBuilder.Entity<PaginationModel>().HasNoKey();
            modelBuilder.Entity<PaginationViewModel>().HasNoKey();
            modelBuilder.Entity<QueryFieldViewModel>().HasNoKey();
            modelBuilder.Entity<TableFieldViewModel>().HasNoKey();
            modelBuilder.Entity<UpdateDictFieldInput>().HasNoKey();
            modelBuilder.Entity<AddItemRequest>().HasNoKey();

            // 針對每個 entity 有 decimal 欄位時，自動設定 HasPrecision

            modelBuilder.Entity<AddItemRequest>(entity =>
            {
                foreach (var prop in typeof(AddItemRequest).GetProperties().Where(x => x.PropertyType == typeof(decimal) || x.PropertyType == typeof(decimal?)))
                {
                    entity.Property(prop.Name).HasPrecision(18, 6);
                }
            });

            modelBuilder.Entity<CURdOCXTableFieldLK>(entity =>
            {
                foreach (var prop in typeof(CURdOCXTableFieldLK).GetProperties().Where(x => x.PropertyType == typeof(decimal) || x.PropertyType == typeof(decimal?)))
                {
                    entity.Property(prop.Name).HasPrecision(18, 6);
                }
            });

            modelBuilder.Entity<CurdPaperSelected>(entity =>
            {
                foreach (var prop in typeof(CurdPaperSelected).GetProperties().Where(x => x.PropertyType == typeof(decimal) || x.PropertyType == typeof(decimal?)))
                {
                    entity.Property(prop.Name).HasPrecision(18, 6);
                }
            });

            modelBuilder.Entity<CurdSysItem>(entity =>
                    {
                        entity.HasKey(e => e.ItemId);

                        entity.ToTable("CURdSysItems", tb =>
                            {
                                tb.HasTrigger("CURdSysItems_tD");
                                tb.HasTrigger("CURdSysItems_tI");
                                tb.HasTrigger("CURdSysItems_tU");
                            });

                        entity.Property(e => e.ItemId)
                            .HasMaxLength(8)
                            .IsUnicode(false)
                            .IsFixedLength();
                        entity.Property(e => e.BtnAdd).HasColumnName("btnAdd");
                        entity.Property(e => e.BtnClose).HasColumnName("btnClose");
                        entity.Property(e => e.BtnDelete).HasColumnName("btnDelete");
                        entity.Property(e => e.BtnExam).HasColumnName("btnExam");
                        entity.Property(e => e.BtnInq).HasColumnName("btnInq");
                        entity.Property(e => e.BtnPrintList).HasColumnName("btnPrintList");
                        entity.Property(e => e.BtnPrintPaper).HasColumnName("btnPrintPaper");
                        entity.Property(e => e.BtnRejExam).HasColumnName("btnRejExam");
                        entity.Property(e => e.BtnSendExam).HasColumnName("btnSendExam");
                        entity.Property(e => e.BtnToExcel).HasColumnName("btnToExcel");
                        entity.Property(e => e.BtnUpdate).HasColumnName("btnUpdate");
                        entity.Property(e => e.BtnUpdateMoney).HasColumnName("btnUpdateMoney");
                        entity.Property(e => e.BtnUpdateNotes).HasColumnName("btnUpdateNotes");
                        entity.Property(e => e.BtnVoid).HasColumnName("btnVoid");
                        entity.Property(e => e.ClassName)
                            .HasMaxLength(50)
                            .IsUnicode(false);
                        entity.Property(e => e.ClassNameCn)
                            .HasMaxLength(50)
                            .IsUnicode(false)
                            .HasColumnName("ClassNameCN");
                        entity.Property(e => e.ClassNameEn)
                            .HasMaxLength(50)
                            .IsUnicode(false)
                            .HasColumnName("ClassNameEN");
                        entity.Property(e => e.ClassNameJp)
                            .HasMaxLength(50)
                            .IsUnicode(false)
                            .HasColumnName("ClassNameJP");
                        entity.Property(e => e.ClassNameTh)
                            .HasMaxLength(50)
                            .IsUnicode(false)
                            .HasColumnName("ClassNameTH");
                        entity.Property(e => e.FlowCondField)
                            .HasMaxLength(50)
                            .IsUnicode(false);
                        entity.Property(e => e.FlowCondField2)
                            .HasMaxLength(50)
                            .IsUnicode(false);
                        entity.Property(e => e.FlowCondField3)
                            .HasMaxLength(50)
                            .IsUnicode(false);
                        entity.Property(e => e.FlowPrcId)
                            .HasMaxLength(30)
                            .IsUnicode(false);
                        entity.Property(e => e.FlowTotalField)
                            .HasMaxLength(60)
                            .IsUnicode(false);
                        entity.Property(e => e.IAttachment)
                            .HasDefaultValue(0)
                            .HasColumnName("iAttachment");
                        entity.Property(e => e.IFlowBefExamCheck).HasColumnName("iFlowBefExamCheck");
                        entity.Property(e => e.IFlowStopSend).HasColumnName("iFlowStopSend");
                        entity.Property(e => e.IFullHeightDel).HasColumnName("iFullHeightDel");
                        entity.Property(e => e.IReportGridType)
                            .HasDefaultValue(1)
                            .HasColumnName("iReportGridType");
                        entity.Property(e => e.IShowTracePaperBtn)
                            .HasDefaultValue(0)
                            .HasColumnName("iShowTracePaperBtn");
                        entity.Property(e => e.InsideCode)
                            .HasMaxLength(50)
                            .IsUnicode(false);
                        entity.Property(e => e.ItemName).HasMaxLength(50);
                        entity.Property(e => e.ItemNameCn)
                            .HasMaxLength(50)
                            .HasColumnName("ItemNameCN");
                        entity.Property(e => e.ItemNameEn)
                            .HasMaxLength(50)
                            .HasColumnName("ItemNameEN");
                        entity.Property(e => e.ItemNameJp)
                            .HasMaxLength(50)
                            .HasColumnName("ItemNameJP");
                        entity.Property(e => e.ItemNameTh)
                            .HasMaxLength(50)
                            .HasColumnName("ItemNameTH");
                        entity.Property(e => e.LinkType).HasDefaultValue(1);
                        entity.Property(e => e.Notes).HasMaxLength(4000);
                        entity.Property(e => e.ObjectName)
                            .HasMaxLength(50)
                            .IsUnicode(false);
                        entity.Property(e => e.Ocxtemplete)
                            .HasMaxLength(64)
                            .IsUnicode(false)
                            .HasColumnName("OCXTemplete");
                        entity.Property(e => e.PaperId)
                            .HasMaxLength(32)
                            .IsUnicode(false);
                        entity.Property(e => e.PaperType).HasDefaultValue(255);
                        entity.Property(e => e.PowerType).HasDefaultValue(0);
                        entity.Property(e => e.SBiexcelPath)
                            .HasMaxLength(100)
                            .HasColumnName("sBIExcelPath");
                        entity.Property(e => e.ShowTitle).HasDefaultValue(1);
                        entity.Property(e => e.SuperId)
                            .HasMaxLength(8)
                            .IsUnicode(false);
                        entity.Property(e => e.SystemId)
                            .HasMaxLength(8)
                            .IsUnicode(false);
                        entity.Property(e => e.TableIndex)
                            .HasMaxLength(50)
                            .IsUnicode(false);
                    });

            modelBuilder.Entity<CurdSystemSelect>(entity =>
            {
                foreach (var prop in typeof(CurdSystemSelect).GetProperties().Where(x => x.PropertyType == typeof(decimal) || x.PropertyType == typeof(decimal?)))
                {
                    entity.Property(prop.Name).HasPrecision(18, 6);
                }
            });

            modelBuilder.Entity<CURdTableField>(entity =>
            {
                foreach (var prop in typeof(CURdTableField).GetProperties().Where(x => x.PropertyType == typeof(decimal) || x.PropertyType == typeof(decimal?)))
                {
                    entity.Property(prop.Name).HasPrecision(18, 6);
                }
            });

            modelBuilder.Entity<CurdTableFieldLang>(entity =>
            {
                foreach (var prop in typeof(CurdTableFieldLang).GetProperties().Where(x => x.PropertyType == typeof(decimal) || x.PropertyType == typeof(decimal?)))
                {
                    entity.Property(prop.Name).HasPrecision(18, 6);
                }
            });

            modelBuilder.Entity<CurdUser>(entity =>
            {
                foreach (var prop in typeof(CurdUser).GetProperties().Where(x => x.PropertyType == typeof(decimal) || x.PropertyType == typeof(decimal?)))
                {
                    entity.Property(prop.Name).HasPrecision(18, 6);
                }
            });

            modelBuilder.Entity<EmodProdInfo>(entity =>
            {
                foreach (var prop in typeof(EmodProdInfo).GetProperties().Where(x => x.PropertyType == typeof(decimal) || x.PropertyType == typeof(decimal?)))
                {
                    entity.Property(prop.Name).HasPrecision(18, 6);
                }
            });

            modelBuilder.Entity<MindStockCostPn>(entity =>
            {
                foreach (var prop in typeof(MindStockCostPn).GetProperties().Where(x => x.PropertyType == typeof(decimal) || x.PropertyType == typeof(decimal?)))
                {
                    entity.Property(prop.Name).HasPrecision(18, 6);
                }
            });

            modelBuilder.Entity<SpodOrderMain>(entity =>
            {
                foreach (var prop in typeof(SpodOrderMain).GetProperties().Where(x => x.PropertyType == typeof(decimal) || x.PropertyType == typeof(decimal?)))
                {
                    entity.Property(prop.Name).HasPrecision(18, 6);
                }
            });

            modelBuilder.Entity<SpodOrderSub>(entity =>
            {
                foreach (var prop in typeof(SpodOrderSub).GetProperties().Where(x => x.PropertyType == typeof(decimal) || x.PropertyType == typeof(decimal?)))
                {
                    entity.Property(prop.Name).HasPrecision(18, 6);
                }
            });

            foreach (var prop in modelBuilder.Model.GetEntityTypes()
                    .SelectMany(t => t.GetProperties())
                    .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                prop.SetPrecision(24);
                prop.SetScale(8);
            }

            OnModelCreatingPartial(modelBuilder);
        }
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}