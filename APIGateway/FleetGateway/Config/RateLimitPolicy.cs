namespace FleetGateway.Config
{
    public static class RateLimitPolicy
    {
        // Policy names (used in Program.cs + YARP metadata)
        public const string Global = "global";
        public const string AuthEndpoints = "auth_endpoints";   // login/register — strict
        public const string FleetRead = "fleet_read";       // GET fleet routes
        public const string FleetWrite = "fleet_write";      // POST/PUT/DELETE fleet
        public const string AdminRoutes = "admin_routes";     // admin 
        public const string PerUser = "per_user";         // per authenticated user
        // Limits
        public static class Limits
        {
            // Auth: 10 requests per minute (brute-force protection)
            //public const int AuthPermits = 10;
            //public const int AuthWindowSeconds = 60;
            public const int AuthPermits = 2;
            public const int AuthWindowSeconds = 60;
            //// Fleet read: 100 requests per minute per IP
            //public const int FleetReadPermits = 100;
            //public const int FleetReadWindowSeconds = 60;
            // Fleet read: 100 requests per minute per IP
            public const int FleetReadPermits = 2;
            public const int FleetReadWindowSeconds = 60;

            // Fleet write: 20 requests per minute per IP
            public const int FleetWritePermits = 20;
            public const int FleetWriteWindowSeconds = 60;

            // Admin: 200 requests per minute
            public const int AdminPermits = 200;
            public const int AdminWindowSeconds = 60;

            // Global fallback: 200 per minute per IP
            public const int GlobalPermits = 200;
            public const int GlobalWindowSeconds = 60;

            // Per authenticated user (token bucket): 50 tokens, refill 10/sec
            public const int PerUserTokens = 2;
            public const int PerUserReplenishPerSec = 60;
            //public const int PerUserTokens = 50;
            //public const int PerUserReplenishPerSec = 10;
        }
    }
}
