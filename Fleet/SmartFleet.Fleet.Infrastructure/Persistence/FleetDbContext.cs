using Microsoft.EntityFrameworkCore;
using SmartFleet.Fleet.Domain.Entities;

namespace SmartFleet.Fleet.Infrastructure.Persistence
{
    public class FleetDbContext : DbContext
    {
        public FleetDbContext(DbContextOptions<FleetDbContext> options) : base(options) { }

        public DbSet<Vehicle> Vehicles => Set<Vehicle>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Applying Fluent API configurations from the same assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(FleetDbContext).Assembly);
        }
    }
}
