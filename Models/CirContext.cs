using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PcbErpApi.Models;

public partial class CirContext : DbContext
{
    public CirContext()
    {
    }
    public DbSet<CurdSysItem> CurdSysItems { get; set; }
    public DbSet<SpodOrderSub> SpodOrderSub { get; set; }
    public DbSet<CurdSystemSelect> CurdSystemSelects { get; set; }
     public DbSet<CURdTableField> CURdTableFields { get; set; }
    public CirContext(DbContextOptions<CirContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CurdUser> CurdUsers { get; set; }

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

        modelBuilder.Entity<SpodOrderSub>()
        .HasKey(x => new { x.PaperNum, x.Item });
        
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
