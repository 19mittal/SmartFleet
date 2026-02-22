using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FleetGateway.Middleware
{
    public class JwtValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _config;
        private readonly ILogger<JwtValidationMiddleware> _logger;

        public JwtValidationMiddleware(RequestDelegate next, IConfiguration config, ILogger<JwtValidationMiddleware> logger)
        {
            _next = next;
            _config = config;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext ctx)
        {
            var path = ctx.Request.Path.Value ?? string.Empty;

            // ── Check if this path is public ──────────────────────────────────
           // if (IsPublicRoute(path))
            {
                await _next(ctx);
                return;
            }

            // ── Extract Bearer token ──────────────────────────────────────────
            var authHeader = ctx.Request.Headers.Authorization.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(authHeader) ||
                !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Missing or malformed Authorization header: {Path}", path);
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await ctx.Response.WriteAsJsonAsync(new
                {
                    statusCode = 401,
                    message = "Authorization header missing or malformed. " +
                                 "Use: Authorization: Bearer <token>"
                });
                return;
            }

            var token = authHeader["Bearer ".Length..].Trim();

            // ── Validate token ────────────────────────────────────────────────
            var principal = ValidateToken(token);
            if (principal is null)
            {
                _logger.LogWarning("Invalid or expired JWT for path: {Path}", path);
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await ctx.Response.WriteAsJsonAsync(new
                {
                    statusCode = 401,
                    message = "Token is invalid or expired."
                });
                return;
            }

            // ── Attach claims to HttpContext.User ──────────────────────────────
            ctx.User = principal;

            // ── Forward user info to downstream services via headers ───────────
            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var userRole = principal.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
            var email = principal.FindFirstValue(ClaimTypes.Email) ?? string.Empty;

            ctx.Request.Headers["X-User-Id"] = userId;
            ctx.Request.Headers["X-User-Role"] = userRole;
            ctx.Request.Headers["X-User-Email"] = email;

            await _next(ctx);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        //private static bool IsPublicRoute(string path)
        //{
        //    foreach (var (prefix, role) in RoutePolicy.Rules)
        //    {
        //        if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        //            return role is null;   // first match: null role = public
        //    }
        //    return false;   // no match → treat as protected
        //}

        private ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                var secret = _config["Jwt:Secret"]!;
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
                var handler = new JwtSecurityTokenHandler();

                return handler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = true,
                    ValidIssuer = _config["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _config["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out _);
            }
            catch (Exception ex)
            {
                _logger.LogDebug("Token validation failed: {Reason}", ex.Message);
                return null;
            }
        }
    }
}
