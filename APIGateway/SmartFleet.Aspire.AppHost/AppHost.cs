var builder = DistributedApplication.CreateBuilder(args);

// ── Shared JWT config ──────────────────────────────────────────────────────
var jwtSecret = builder.AddParameter("JwtSecret", secret: true);
var jwtIssuer = "SmartFleetAuthAPI";
var jwtAudience = "SmartFleetUsers";

// ── Auth Service ───────────────────────────────────────────────────────────
var authApi = builder
    .AddProject<Projects.SmartFleet_Auth_Api>("smartfleet-auth")
    .WithEnvironment("Jwt__Secret", jwtSecret)
    .WithEnvironment("Jwt__Issuer", jwtIssuer)
    .WithEnvironment("Jwt__Audience", jwtAudience)
    .WithEnvironment("Jwt__AccessTokenMinutes", "15")
    .WithEnvironment("Jwt__RefreshTokenDays", "7");

// ── Fleet Service ──────────────────────────────────────────────────────────
var fleetApi = builder
    .AddProject<Projects.SmartFleet_Fleet_Api>("smartfleet-fleet")
    .WithEnvironment("Jwt__Secret", jwtSecret)
    .WithEnvironment("Jwt__Issuer", jwtIssuer)
    .WithEnvironment("Jwt__Audience", jwtAudience);

// ── API Gateway ────────────────────────────────────────────────────────────
builder
    .AddProject<Projects.FleetGateway>("smartfleet-gateway")
    .WithReference(authApi)
    .WithReference(fleetApi)
    .WithEnvironment("Jwt__Secret", jwtSecret)
    .WithEnvironment("Jwt__Issuer", jwtIssuer)
    .WithEnvironment("Jwt__Audience", jwtAudience);

builder.Build().Run();