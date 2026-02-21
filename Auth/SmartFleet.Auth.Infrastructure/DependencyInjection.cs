using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartFleet.Auth.Application.Interfaces;
using SmartFleet.Auth.Domain.Interfaces;
using SmartFleet.Auth.Infrastructure.Persistence;
using SmartFleet.Auth.Infrastructure.Repositories;
using SmartFleet.Auth.Infrastructure.Security;


namespace SmartFleet.Auth.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. Database Context - SQL Server / Postgres / SQLite
            services.AddDbContext<AuthDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(AuthDbContext).Assembly.FullName)));

            // 2. Repositories
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

            // 3. Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // 4. Security & Identity
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<ITokenService, TokenService>();

            return services;
        }
    }
}
