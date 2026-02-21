using SmartFleet.Fleet.Application.DTOs;
using SmartFleet.Fleet.Domain.Entities;
using SmartFleet.Fleet.Domain.Enums;

namespace SmartFleet.Fleet.Application.Interfaces
{
    public interface IVehicleService
    {
        Task<IEnumerable<Vehicle>> GetAllVehiclesAsync();
        Task<Vehicle?> GetVehicleByIdAsync(Guid id);
        Task<Vehicle> AddVehicleAsync(CreateVehicleRequest request);
        Task<bool> UpdateVehicleStatusAsync(Guid id, VehicleStatus status);
        Task<IEnumerable<Vehicle>> GetMaintenanceAlertsAsync();
    }
}
