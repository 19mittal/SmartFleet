using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Yarp.ReverseProxy.Swagger;

namespace FleetGateway.Config
{
    public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly ReverseProxyDocumentFilterConfig _config;

        public ConfigureSwaggerOptions(
            IOptionsMonitor<ReverseProxyDocumentFilterConfig> monitor)
        {
            _config = monitor.CurrentValue;
        }

        public void Configure(SwaggerGenOptions options)
        {
            // One SwaggerDoc per cluster key — matches /swagger/{key}/swagger.json
            foreach (var cluster in _config.Clusters)
            {
                options.SwaggerDoc(cluster.Key, new OpenApiInfo
                {
                    Title = cluster.Key,
                    Version = cluster.Key
                });
            }

            // Treyt's filter that fetches + merges downstream swagger docs
            options.DocumentFilterDescriptors =
            [
                new FilterDescriptor
            {
                Type      = typeof(ReverseProxyDocumentFilter),
                Arguments = Array.Empty<object>()
            }
            ];

            // JWT Bearer
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter JWT from POST /api/auth/login"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id   = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
        }
    }
}
