using LogiTrack.Models;
using Microsoft.EntityFrameworkCore;

namespace LogiTrack.Context
{
    public class LogiTrackContext : DbContext
    {
        public DbSet<InventoryItem> InventoryItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite("Data Source=logitrack.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>()
                .HasMany(o => o.ItemList)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.SetNull);
                
            base.OnModelCreating(modelBuilder);
        }
    }
}