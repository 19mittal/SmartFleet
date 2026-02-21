using SmartFleet.Auth.Domain.Entities;

namespace SmartFleet.Auth.Domain.Interfaces
{
    public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
    {
        Task<RefreshToken?> GetByTokenAsync(string token);
        Task RevokeByUserIdAsync(Guid userId);
    }
}
