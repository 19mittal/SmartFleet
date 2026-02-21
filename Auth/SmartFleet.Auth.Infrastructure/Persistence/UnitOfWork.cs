using SmartFleet.Auth.Application.Interfaces;
using SmartFleet.Auth.Domain.Interfaces;

namespace SmartFleet.Auth.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AuthDbContext _context;
        public IUserRepository Users { get; }
        public IRefreshTokenRepository RefreshTokens { get; }

        public UnitOfWork(AuthDbContext context, IUserRepository userRepository, IRefreshTokenRepository refreshTokens)
        {
            _context = context;
            Users = userRepository;
            RefreshTokens = refreshTokens;
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
