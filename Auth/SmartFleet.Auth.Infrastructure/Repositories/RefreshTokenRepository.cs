using Microsoft.EntityFrameworkCore;
using SmartFleet.Auth.Domain.Entities;
using SmartFleet.Auth.Domain.Interfaces;
using SmartFleet.Auth.Infrastructure.Persistence;

namespace SmartFleet.Auth.Infrastructure.Repositories
{
    public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(AuthDbContext context) : base(context) { }

        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            // Using your specific property names
            return await _dbSet.FirstOrDefaultAsync(rt => rt.Token == token);
        }

        public async Task RevokeByUserIdAsync(Guid userId)
        {
            var tokens = await _dbSet
                .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiryDate > DateTime.UtcNow)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.IsRevoked = true; // Matching your property
            }
        }
    }
}
