using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using SystemIntelligencePlatform.RateLimiting;
using Volo.Abp.DependencyInjection;

namespace SystemIntelligencePlatform.RateLimiting;

/// <summary>
/// Sliding window rate limiter using distributed cache.
/// Each tenant gets an independent counter keyed by tenant ID + resource + time window.
/// The window slides by bucketing into discrete time slots.
/// </summary>
public class SlidingWindowRateLimiter : IRateLimiter, ITransientDependency
{
    private readonly IDistributedCache _cache;
    private readonly RateLimitingOptions _options;

    public SlidingWindowRateLimiter(
        IDistributedCache cache,
        IOptions<RateLimitingOptions> options)
    {
        _cache = cache;
        _options = options.Value;
    }

    public async Task<RateLimitResult> CheckAsync(Guid tenantId, string resource)
    {
        var windowKey = GetWindowKey(tenantId, resource);
        var currentCountBytes = await _cache.GetAsync(windowKey);

        var currentCount = 0;
        if (currentCountBytes != null)
        {
            currentCount = int.Parse(Encoding.UTF8.GetString(currentCountBytes));
        }

        if (currentCount >= _options.MaxRequestsPerWindow)
        {
            return new RateLimitResult
            {
                IsAllowed = false,
                RetryAfterSeconds = _options.WindowSizeSeconds,
                CurrentCount = currentCount,
                Limit = _options.MaxRequestsPerWindow
            };
        }

        currentCount++;
        var newValue = Encoding.UTF8.GetBytes(currentCount.ToString());
        await _cache.SetAsync(windowKey, newValue, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_options.WindowSizeSeconds)
        });

        return new RateLimitResult
        {
            IsAllowed = true,
            CurrentCount = currentCount,
            Limit = _options.MaxRequestsPerWindow
        };
    }

    private string GetWindowKey(Guid tenantId, string resource)
    {
        var windowSlot = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / _options.WindowSizeSeconds;
        return $"ratelimit:{tenantId}:{resource}:{windowSlot}";
    }
}
