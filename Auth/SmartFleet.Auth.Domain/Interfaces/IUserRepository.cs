using SmartFleet.Auth.Domain.Entities;

namespace SmartFleet.Auth.Domain.Interfaces
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
    }
}
