using Microsoft.EntityFrameworkCore;
using PcbErpApi.Models;

namespace PcbErpApi.Data
{
    public class PcbErpContext : DbContext
    {
        public PcbErpContext(DbContextOptions<PcbErpContext> options) : base(options) { }

        public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
        public DbSet<SpodOrderMain> SpodOrderMain => Set<SpodOrderMain>();
        public DbSet<CurdUser> CurdUser => Set<CurdUser>();
        public DbSet<MindStockCostPn> MindStockCostPn => Set<MindStockCostPn>();
        public DbSet<CurdSysItem> CurdSysItems { get; set; }



    }
}
