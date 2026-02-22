using System.Security.Claims;

namespace FleetGateway.Middleware
{
    public class RoleAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RoleAuthorizationMiddleware> _logger;

        public RoleAuthorizationMiddleware(RequestDelegate next, ILogger<RoleAuthorizationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext ctx)
        {
            var path = ctx.Request.Path.Value ?? string.Empty;

            //// Find the first matching rule
            //foreach (var (prefix, requiredRole) in RoutePolicy.Rules)
            //{
            //    if (!path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            //        continue;

            //    // Public route — skip role check
            //    if (requiredRole is null) break;

            //    var userRole = ctx.User.FindFirstValue(ClaimTypes.Role);

            //    bool allowed = requiredRole switch
            //    {
            //        "Admin" => userRole == "Admin",
            //        "User" => userRole is "User" or "Admin",  // Admin can access User routes
            //        _ => false
            //    };

            //    if (!allowed)
            //    {
            //        _logger.LogWarning(
            //            "Access denied. Path: {Path} | Required: {Required} | Has: {Has}",
            //            path, requiredRole, userRole ?? "none");

            //        ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
            //        await ctx.Response.WriteAsJsonAsync(new
            //        {
            //            statusCode = 403,
            //            message = $"Forbidden. '{requiredRole}' role required.",
            //            path
            //        });
            //        return;
            //    }

            //    break; // matched + authorized
            //}

            await _next(ctx);
        }
    }
}
