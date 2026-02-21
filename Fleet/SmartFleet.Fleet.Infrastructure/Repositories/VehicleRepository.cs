using Microsoft.EntityFrameworkCore;
using SmartFleet.Fleet.Domain.Entities;
using SmartFleet.Fleet.Domain.Enums;
using SmartFleet.Fleet.Domain.Interfaces;
using SmartFleet.Fleet.Infrastructure.Persistence;

namespace SmartFleet.Fleet.Infrastructure.Repositories
{
    public class VehicleRepository : GenericRepository<Vehicle>, IVehicleRepository
    {
        private readonly FleetDbContext _context;

        public VehicleRepository(FleetDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Vehicle?> GetByVinAsync(string vin)
        {
            // Explicitly using the concrete context to find a unique vehicle
            return await _context.Vehicles
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.VIN == vin);
        }

        public async Task<IEnumerable<Vehicle>> GetVehiclesByStatusAsync(VehicleStatus status)
        {
            return await _context.Vehicles
                .Where(v => v.Status == status)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Vehicle>> GetVehiclesNeedingServiceAsync(DateTime thresholdDate)
        {
            // Logic: Vehicles never serviced OR serviced before the threshold
            return await _context.Vehicles
                .Where(v => v.LastServiceDate == null || v.LastServiceDate <= thresholdDate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> ExistsByVinAsync(string vin)
        {
            // Highly efficient check using AnyAsync
            return await _context.Vehicles.AnyAsync(v => v.VIN == vin);
        }
    }
}
