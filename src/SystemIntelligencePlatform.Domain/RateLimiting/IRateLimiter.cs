using System;
using System.Threading.Tasks;

namespace SystemIntelligencePlatform.RateLimiting;

public interface IRateLimiter
{
    Task<RateLimitResult> CheckAsync(Guid tenantId, string resource);
}

public class RateLimitResult
{
    public bool IsAllowed { get; set; }
    public int RetryAfterSeconds { get; set; }
    public int CurrentCount { get; set; }
    public int Limit { get; set; }
}

public class RateLimitingOptions
{
    public int MaxRequestsPerWindow { get; set; } = 1000;
    public int WindowSizeSeconds { get; set; } = 60;
}
