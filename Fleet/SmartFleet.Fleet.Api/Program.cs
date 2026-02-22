using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using SmartFleet.Fleet.Application;
using SmartFleet.Fleet.Application.DTOs;
using SmartFleet.Fleet.Application.Interfaces;
using SmartFleet.Fleet.Domain.Enums;
using SmartFleet.Fleet.Infrastructure;
using SmartFleet.Shared;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

// ──Fleet Auth (JWT + Policies) ─────────────────────────────
builder.Services.AddFleetJwtAuthentication(builder.Configuration);

// Add Infrastructure & Application layers (Assuming these extension methods exist)
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Document info
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SmartFleet API",
        Version = "v1",
        Description = "JWT Authentication",

    });

    // Define Bearer security scheme
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description =
            "Enter your JWT access token.\n\nExample: `eyJhbGci...`"
    });

    // Apply Bearer to every endpoint globally
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

    // Include XML comments from controller doc comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});


builder.Services.AddAuthorization(); // Ensure Auth is registered

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // This generates the /swagger/v1/swagger.json file
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// --- Fleet Management Endpoints ---

var fleetGroup = app.MapGroup("/api/fleet").RequireAuthorization(policy => policy.RequireRole("Admin"))
    .WithTags("Fleet Management");

// GET: All vehicles
fleetGroup.MapGet("/", async (IVehicleService vehicleService) =>
{
    var vehicles = await vehicleService.GetAllVehiclesAsync();
    return Results.Ok(vehicles);
})
.WithName("GetAllVehicles");

// GET: Single vehicle by ID
fleetGroup.MapGet("/{id:guid}", async (Guid id, IVehicleService vehicleService) =>
{
    var vehicle = await vehicleService.GetVehicleByIdAsync(id);
    return vehicle is not null ? Results.Ok(vehicle) : Results.NotFound();
})
.WithName("GetVehicleById");

// POST: Register a new vehicle
fleetGroup.MapPost("/", async ([FromBody] CreateVehicleRequest request, IVehicleService vehicleService) =>
{
    try
    {
        var vehicle = await vehicleService.AddVehicleAsync(request);
        return Results.CreatedAtRoute("GetVehicleById", new { id = vehicle.Id }, vehicle);
    }
    catch (InvalidOperationException ex)
    {
        return Results.Conflict(new { message = ex.Message });
    }
})
.WithName("AddVehicle");

// PATCH: Update vehicle status (e.g., set to Maintenance)
fleetGroup.MapPatch("/{id:guid}/status", async (Guid id, [FromBody] VehicleStatus status, IVehicleService vehicleService) =>
{
    var success = await vehicleService.UpdateVehicleStatusAsync(id, status);
    return success ? Results.NoContent() : Results.NotFound();
})
.WithName("UpdateVehicleStatus");

// GET: Maintenance Alerts (AI readiness)
fleetGroup.MapGet("/alerts", async (IVehicleService vehicleService) =>
{
    var alerts = await vehicleService.GetMaintenanceAlertsAsync();
    return Results.Ok(alerts);
})
.WithName("GetMaintenanceAlerts");

app.Run();