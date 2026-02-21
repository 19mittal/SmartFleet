using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartFleet.Fleet.Application.Interfaces;
using SmartFleet.Fleet.Domain.Interfaces;
using SmartFleet.Fleet.Infrastructure.Persistence;
using SmartFleet.Fleet.Infrastructure.Repositories;


namespace SmartFleet.Fleet.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. Database Context - SQL Server / Postgres / SQLite
            services.AddDbContext<FleetDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(FleetDbContext).Assembly.FullName)));

            // 2. Repositories
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IVehicleRepository, VehicleRepository>();

            // 3. Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
