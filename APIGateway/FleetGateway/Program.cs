using FleetGateway.Config;
using FleetGateway.Middleware;
using Microsoft.Extensions.Options;
using SmartFleet.Shared;
using Swashbuckle.AspNetCore.SwaggerGen;
using Yarp.ReverseProxy.Swagger;
using Yarp.ReverseProxy.Swagger.Extensions;

var builder = WebApplication.CreateBuilder(args);

// ── Aspire ─────────────────────────────────────────────────────────────────
builder.AddServiceDefaults();
// ── Auth (JWT + Role Policies) ─────────────────────────────
builder.Services.AddFleetJwtAuthentication(builder.Configuration);
// ── Rate Limiting ──────────────────────────────────────────────────────────
builder.Services.AddCustomRateLimiting();
// 1. ADD MISSING SWAGGER SERVICES
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // This fixes the ISwaggerProvider exception


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("GatewayAccess", policy => policy.RequireAuthenticatedUser());
});
// 3. Add YARP from JSON Configuration
// Use the EXACT section name from your appsettings.json
// ── YARP Reverse Proxy ─────────────────────────────────────────────────────
var proxyConfig = builder.Configuration.GetSection("ReverseProxy");

builder.Services.AddServiceDiscovery();
builder.Services.AddReverseProxy()
    .LoadFromConfig(proxyConfig)
    .AddServiceDiscoveryDestinationResolver()
    .AddSwagger(proxyConfig);           // ← single line wires all Swagger config


// ── Swashbuckle SwaggerGen ────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
// ── SwaggerGen — let ConfigureSwaggerOptions do all the work ──────────────
builder.Services
    .AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen();

/// ── CORS (adjust origins for production) ──────────────────────────────────
builder.Services.AddCors(opts =>
{
    opts.AddDefaultPolicy(p => p
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
});

var app = builder.Build();

// ── Swagger UI ─────────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(opts =>
    {
        var config = app.Services
            .GetRequiredService<IOptionsMonitor<ReverseProxyDocumentFilterConfig>>()
            .CurrentValue;

        // Dynamically add one tab per cluster
        foreach (var cluster in config.Clusters)
        {
            opts.SwaggerEndpoint(
                $"/swagger/{cluster.Key}/swagger.json",
                cluster.Key);
        }

        opts.RoutePrefix = "swagger";
        opts.DocumentTitle = "SmartFleet Gateway";
        opts.DisplayRequestDuration();
        opts.EnableTryItOutByDefault();
        opts.DefaultModelsExpandDepth(-1);
    });
}
// ── Middleware pipeline ORDER ──────────────────────────────────────────────
app.UseCors();                                          //1. Allow Cors
app.UseRateLimiter();                                    //2 ← Rate limiting BEFORE auth
app.UseMiddleware<JwtValidationMiddleware>();  //3.Validate JWT and populate ctx.User
app.UseMiddleware<RoleAuthorizationMiddleware>(); //4.Enforce role per route prefix
app.UseAuthentication(); //5.ASP.NET Core built-in auth (for any [Authorize] attributes on gateway controllers)
app.UseAuthorization(); //6.ASP.NET Core built-in auth (for any [Authorize] attributes on gateway controllers)

app.MapDefaultEndpoints();  //7.Aspire health/readiness endpoints
app.MapReverseProxy();  //8.YARP proxies the request to the correct downstream service

app.Run();