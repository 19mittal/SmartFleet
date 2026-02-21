using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using SmartFleet.Fleet.Application.Interfaces;
using SmartFleet.Fleet.Application.Services;
using System.Reflection;

namespace SmartFleet.Fleet.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // 1. Register Business Services
            // Scoped lifetime is ideal for services using a Unit of Work
            services.AddScoped<IVehicleService, VehicleService>();

            // 2. Mapster Configuration
            var config = TypeAdapterConfig.GlobalSettings;

            // Scan the current assembly (Application) for all IRegister implementations
            // This automatically picks up your mapping rules without manual registration
            var applicationAssembly = Assembly.GetExecutingAssembly();
            config.Scan(applicationAssembly);

            // Register Mapster as a Singleton for the configuration 
            // and Scoped for the IMapper instance
            services.AddSingleton(config);
            services.AddScoped<IMapper, ServiceMapper>();

            return services;
        }
    }
}
