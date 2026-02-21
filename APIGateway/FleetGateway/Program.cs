using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Yarp.ReverseProxy.Swagger.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 1. ADD MISSING SWAGGER SERVICES
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // This fixes the ISwaggerProvider exception


// 2. Gateway Level Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });


// 3. Add YARP from JSON Configuration
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddSwagger(builder.Configuration.GetSection("ReverseProxy")); ;

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("GatewayAccess", policy => policy.RequireAuthenticatedUser());
});

var app = builder.Build();



if (app.Environment.IsDevelopment())
{
    // 1. Generate the JSON
    app.UseSwagger();

    // 2. Setup the UI
    app.UseSwaggerUI(options =>
    {
        var clusters = builder.Configuration.GetSection("ReverseProxy:Clusters").GetChildren();
        foreach (var cluster in clusters)
        {
            options.SwaggerEndpoint($"/swagger/{cluster.Key}/swagger.json", cluster.Key);
        }
    });
}
app.UseAuthentication();
app.UseAuthorization();
// 3. Map YARP
app.MapReverseProxy();

app.Run();