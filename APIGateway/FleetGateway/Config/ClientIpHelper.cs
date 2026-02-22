namespace FleetGateway.Config
{
    public static class ClientIpHelper
    {
        public static string GetClientIp(HttpContext ctx)
        {
            return ctx.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                ?? ctx.Connection.RemoteIpAddress?.ToString()
                ?? "unknown";
        }
    }
}
