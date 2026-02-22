using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace FleetGateway.Config
{
    public static class RateLimiterExtensions
    {
        public static IServiceCollection AddCustomRateLimiting(this IServiceCollection services)
        {
            services.AddRateLimiter(opts =>
            {
                // ── 1. GLOBAL fallback — fixed window per IP ──────────────────────────
                opts.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
                {
                    var ip = ClientIpHelper.GetClientIp(ctx);
                    return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = RateLimitPolicy.Limits.GlobalPermits,
                        Window = TimeSpan.FromSeconds(RateLimitPolicy.Limits.GlobalWindowSeconds),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 5
                    });
                });

                // ── 2. AUTH endpoints — fixed window, strict ──────────────────────────
                opts.AddFixedWindowLimiter(RateLimitPolicy.AuthEndpoints, limiterOpts =>
                {
                    limiterOpts.PermitLimit = RateLimitPolicy.Limits.AuthPermits;
                    limiterOpts.Window = TimeSpan.FromSeconds(RateLimitPolicy.Limits.AuthWindowSeconds);
                    limiterOpts.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    limiterOpts.QueueLimit = 0;
                });

                // ── 3. FLEET READ — sliding window ────────────────────────────────────
                opts.AddSlidingWindowLimiter(RateLimitPolicy.FleetRead, limiterOpts =>
                {
                    limiterOpts.PermitLimit = RateLimitPolicy.Limits.FleetReadPermits;
                    limiterOpts.Window = TimeSpan.FromSeconds(RateLimitPolicy.Limits.FleetReadWindowSeconds);
                    limiterOpts.SegmentsPerWindow = 6;
                    limiterOpts.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    limiterOpts.QueueLimit = 0;
                });

                // ── 4. FLEET WRITE — fixed window ─────────────────────────────────────
                opts.AddFixedWindowLimiter(RateLimitPolicy.FleetWrite, limiterOpts =>
                {
                    limiterOpts.PermitLimit = RateLimitPolicy.Limits.FleetWritePermits;
                    limiterOpts.Window = TimeSpan.FromSeconds(RateLimitPolicy.Limits.FleetWriteWindowSeconds);
                    limiterOpts.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    limiterOpts.QueueLimit = 0;
                });

                // ── 5. ADMIN routes — token bucket per user ───────────────────────────
                opts.AddPolicy(RateLimitPolicy.AdminRoutes, ctx =>
                {
                    var userId = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                                 ?? ClientIpHelper.GetClientIp(ctx);

                    return RateLimitPartition.GetTokenBucketLimiter(userId, _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = RateLimitPolicy.Limits.AdminPermits,
                        ReplenishmentPeriod = TimeSpan.FromSeconds(1),
                        TokensPerPeriod = RateLimitPolicy.Limits.PerUserReplenishPerSec,
                        AutoReplenishment = true,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 5
                    });
                });

                // ── 6. PER-USER token bucket ──────────────────────────────────────────
                opts.AddPolicy(RateLimitPolicy.PerUser, ctx =>
                {
                    var userId = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                                 ?? ClientIpHelper.GetClientIp(ctx);

                    return RateLimitPartition.GetTokenBucketLimiter(userId, _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = RateLimitPolicy.Limits.PerUserTokens,
                        ReplenishmentPeriod = TimeSpan.FromSeconds(1),
                        TokensPerPeriod = RateLimitPolicy.Limits.PerUserReplenishPerSec,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    });
                });

                // ── Rejection response (429) ──────────────────────────────────────────
                opts.OnRejected = async (context, cancellationToken) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    {
                        context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
                    }
                    context.HttpContext.Response.ContentType = "application/json";
                    await context.HttpContext.Response.WriteAsJsonAsync(new
                    {
                        statusCode = 429,
                        message = "Too many requests. Please slow down.",
                        retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var ra)
                            ? $"{(int)ra.TotalSeconds} seconds"
                            : "soon"
                    }, cancellationToken);
                };
            });

            return services;
        }
    }
}
