using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace vr_challenge.Data
{
    public class ShipmentContext : DbContext
    {
        public DbSet<ShipmentBox> Box { get; set; }
        public DbSet<ProductShipment> Shipment { get; set; }

        public string DbPath { get; }

        public ShipmentContext(string dbPath)
        {
            DbPath = dbPath;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source = {DbPath}", options =>
            {
                options.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
            });
            base.OnConfiguring(optionsBuilder);
        }
    }
}