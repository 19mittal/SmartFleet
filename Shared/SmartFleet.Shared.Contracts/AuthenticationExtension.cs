using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace SmartFleet.Shared
{
    public static class AuthenticationExtension
    {
        public static IServiceCollection AddFleetJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opts =>
                {
                    opts.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = configuration["Jwt:Issuer"],
                        ValidAudience = configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!)),
                        ClockSkew = TimeSpan.Zero

                    };
                });

            services.AddAuthorizationBuilder()
                .AddPolicy("UserOrAdmin", p => p.RequireRole("User", "Admin"))
                .AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
            return services;
        }
    }
}
