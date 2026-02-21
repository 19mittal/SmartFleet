using SmartFleet.Fleet.Domain.Interfaces;

namespace SmartFleet.Fleet.Application.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IVehicleRepository Vehicles { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
