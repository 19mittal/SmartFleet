using SmartFleet.Shared;
using Yarp.ReverseProxy.Swagger.Extensions;

var builder = WebApplication.CreateBuilder(args);

// ── Aspire ─────────────────────────────────────────────────────────────────
builder.AddServiceDefaults();
// ── Auth (JWT + Role Policies) ─────────────────────────────
builder.Services.AddFleetJwtAuthentication(builder.Configuration);

// 1. ADD MISSING SWAGGER SERVICES
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // This fixes the ISwaggerProvider exception


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("GatewayAccess", policy => policy.RequireAuthenticatedUser());
});
// 3. Add YARP from JSON Configuration
// Use the EXACT section name from your appsettings.json
var proxyConfig = builder.Configuration.GetSection("ReverseProxy");

builder.Services.AddReverseProxy()
    .LoadFromConfig(proxyConfig)
    .AddSwagger(proxyConfig) // This tells the extension to look for 'Metadata: SwaggerDoc'
    .ConfigureHttpClient((context, handler) =>
    {
        // Essential for local HTTPS communication between services
        handler.SslOptions.RemoteCertificateValidationCallback = (s, c, ch, e) => true;
        Console.WriteLine($"[DEBUG] Gateway attempting to fetch docs for Cluster: {context.ClusterId}");
    });



var app = builder.Build();

app.Use(async (context, next) => {
    if (context.Request.Path.Value.Contains("swagger/auth-cluster"))
    {
        Console.WriteLine($"[DIAGNOSTIC] Intercepted request for: {context.Request.Path}");
    }
    await next();
});

if (app.Environment.IsDevelopment())
{
    // 1. Generate the JSON
    app.UseSwagger();

    // 2. Configure the UI to point to those virtual paths
    app.UseSwaggerUI(options =>
    {
        var clusters = builder.Configuration.GetSection("ReverseProxy:Clusters").GetChildren();
        foreach (var cluster in clusters)
        {
            // Path MUST match /swagger/{ClusterId}/swagger.json
            options.SwaggerEndpoint($"/swagger/{cluster.Key}/swagger.json", cluster.Key);
        }
    });
}
app.UseAuthentication();
app.UseAuthorization();
// 3. Map YARP
app.MapReverseProxy();

app.Run();