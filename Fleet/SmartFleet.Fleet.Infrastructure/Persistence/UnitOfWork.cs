using SmartFleet.Fleet.Application.Interfaces;
using SmartFleet.Fleet.Domain.Interfaces;

namespace SmartFleet.Fleet.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly FleetDbContext _context;
        public IVehicleRepository Vehicles { get; }

        public UnitOfWork(FleetDbContext context, IVehicleRepository vehicleRepository)
        {
            _context = context;
            Vehicles = vehicleRepository;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
