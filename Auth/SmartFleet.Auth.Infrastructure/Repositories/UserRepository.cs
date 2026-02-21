using Microsoft.EntityFrameworkCore;
using SmartFleet.Auth.Domain.Entities;
using SmartFleet.Auth.Domain.Interfaces;
using SmartFleet.Auth.Infrastructure.Persistence;

namespace SmartFleet.Auth.Infrastructure.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(AuthDbContext context) : base(context) { }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
        }

    }
}
