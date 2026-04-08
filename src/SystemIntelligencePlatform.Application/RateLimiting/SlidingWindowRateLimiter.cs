using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using SystemIntelligencePlatform.InstanceConfiguration;
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
    private readonly IOptions<RateLimitingOptions> _fileOptions;
    private readonly IInstanceConfigurationProvider _instanceConfiguration;

    public SlidingWindowRateLimiter(
        IDistributedCache cache,
        IOptions<RateLimitingOptions> fileOptions,
        IInstanceConfigurationProvider instanceConfiguration)
    {
        _cache = cache;
        _fileOptions = fileOptions;
        _instanceConfiguration = instanceConfiguration;
    }

    public async Task<RateLimitResult> CheckAsync(Guid tenantId, string resource)
    {
        var _options = EffectiveConfigurationBinder.GetRateLimiting(_instanceConfiguration, _fileOptions);
        var windowKey = GetWindowKey(tenantId, resource, _options);
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

    private static string GetWindowKey(Guid tenantId, string resource, RateLimitingOptions options)
    {
        var windowSlot = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / options.WindowSizeSeconds;
        return $"ratelimit:{tenantId}:{resource}:{windowSlot}";
    }
}
