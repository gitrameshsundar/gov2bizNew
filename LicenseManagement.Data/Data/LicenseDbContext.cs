using LicenseManagement.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace LicenseManagement.Data.Data
{
    public class LicenseDbContext : DbContext
    {
        public LicenseDbContext(DbContextOptions<LicenseDbContext> options) : base(options) { }

        public DbSet<License> Licenses { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<License>(entity =>
            {
                entity.HasKey(e => e.LicenseID);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.CreatedDate).IsRequired();
                entity.HasIndex(e => e.Name).IsUnique();
            });
        }
    }
}