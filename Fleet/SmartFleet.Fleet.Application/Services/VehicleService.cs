using SmartFleet.Fleet.Application.DTOs;
using SmartFleet.Fleet.Application.Interfaces;
using SmartFleet.Fleet.Domain.Entities;
using SmartFleet.Fleet.Domain.Enums;

namespace SmartFleet.Fleet.Application.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly IUnitOfWork _unitOfWork;

        public VehicleService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Vehicle>> GetAllVehiclesAsync()
        {
            return await _unitOfWork.Vehicles.GetAllAsync();
        }

        public async Task<Vehicle?> GetVehicleByIdAsync(Guid id)
        {
            return await _unitOfWork.Vehicles.GetByIdAsync(id);
        }

        public async Task<Vehicle> AddVehicleAsync(CreateVehicleRequest request)
        {
            // Business Rule: Check for duplicate VIN before adding
            var exists = await _unitOfWork.Vehicles.ExistsByVinAsync(request.VIN);
            if (exists) throw new InvalidOperationException("A vehicle with this VIN already exists.");

            var vehicle = new Vehicle
            {
                VIN = request.VIN,
                Make = request.Make,
                Model = request.Model,
                Year = request.Year,
                Status = VehicleStatus.Active,
                ImageUrl = request.ImageUrl // Later we'll integrate Azure Blob logic here
            };

            await _unitOfWork.Vehicles.AddAsync(vehicle);
            await _unitOfWork.SaveChangesAsync();

            return vehicle;
        }

        public async Task<bool> UpdateVehicleStatusAsync(Guid id, VehicleStatus status)
        {
            var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(id);
            if (vehicle == null) return false;

            vehicle.Status = status;
            vehicle.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Vehicles.Update(vehicle);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<Vehicle>> GetMaintenanceAlertsAsync()
        {
            // Logic: Alert for vehicles not serviced in the last 6 months
            var threshold = DateTime.UtcNow.AddMonths(-6);
            return await _unitOfWork.Vehicles.GetVehiclesNeedingServiceAsync(threshold);
        }
    }
}
