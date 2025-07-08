using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PcbErpApi.Models;

public partial class PcbErpContext : DbContext
{
    public PcbErpContext()
    {
    }

    public PcbErpContext(DbContextOptions<PcbErpContext> options)
        : base(options)
    {
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
       

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
