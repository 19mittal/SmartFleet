using Application;
using SmartFleet.Auth.Application;
using SmartFleet.Auth.Application.DTOs;
using SmartFleet.Auth.Application.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// 1. Register our Layered Architecture Services
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// 2. Auth Endpoints Group
var authGroup = app.MapGroup("/api/auth").WithTags("Authentication");

authGroup.MapPost("/signup", async (SignupRequest request, IAuthService authService) =>
{
    var response = await authService.SignupAsync(request);
    return Results.Ok(response);
})
.WithName("Signup");

authGroup.MapPost("/login", async (LoginRequest request, IAuthService authService) =>
{
    try
    {
        var response = await authService.LoginAsync(request);
        return Results.Ok(response);
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
})
.WithName("Login");

authGroup.MapPost("/refresh", async (RefreshTokenRequest request, IAuthService authService) =>
{
    try
    {
        var response = await authService.RefreshAsync(request);
        return Results.Ok(response);
    }
    catch (Exception)
    {
        return Results.BadRequest("Invalid refresh token");
    }
})
.WithName("RefreshToken");

app.Run();