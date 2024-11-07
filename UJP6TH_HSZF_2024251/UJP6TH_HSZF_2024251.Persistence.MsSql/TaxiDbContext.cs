using Microsoft.EntityFrameworkCore;
using UJP6TH_HSZF_2024251.Model;

namespace UJP6TH_HSZF_2024251.Persistence.MsSql
{
    public class TaxiDbContext : DbContext
    {
        public DbSet<TaxiCar> TaxiCars { get; set; }
        public DbSet<Fare> Fares { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TaxiCar>()
                .HasMany(t => t.Fares)
                .WithOne(f => f.TaxiCar)
                .HasForeignKey(f => f.TaxiID);

            modelBuilder.Entity<TaxiCar>()
                .HasIndex(tc => tc.LicensePlate)
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }

        public TaxiDbContext(DbContextOptions<TaxiDbContext> options) : base(options)
        {
            
        }
    }
}
