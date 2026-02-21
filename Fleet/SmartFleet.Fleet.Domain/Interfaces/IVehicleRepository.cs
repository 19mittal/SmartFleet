using SmartFleet.Fleet.Domain.Entities;
using SmartFleet.Fleet.Domain.Enums;

namespace SmartFleet.Fleet.Domain.Interfaces
{
    public interface IVehicleRepository : IGenericRepository<Vehicle>
    {
        // Specialized query for VIN lookups (unique index)
        Task<Vehicle?> GetByVinAsync(string vin);

        // Filtered queries for the Fleet Dashboard
        Task<IEnumerable<Vehicle>> GetVehiclesByStatusAsync(VehicleStatus status);

        // Business-critical query: Find vehicles that haven't been serviced in a while
        Task<IEnumerable<Vehicle>> GetVehiclesNeedingServiceAsync(DateTime thresholdDate);

        // Check if a VIN already exists before attempting an insert
        Task<bool> ExistsByVinAsync(string vin);
    }
}
