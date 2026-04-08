using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SystemIntelligencePlatform.InstanceConfiguration;
using SystemIntelligencePlatform.RateLimiting;

namespace SystemIntelligencePlatform.RateLimiting;

/// <summary>
/// Rate limiting for the public log ingestion endpoint.
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;

    public RateLimitingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IRateLimiter rateLimiter,
        IInstanceConfigurationProvider instanceConfiguration)
    {
        if (!context.Request.Path.StartsWithSegments("/api/ingest"))
        {
            await _next(context);
            return;
        }

        if (!instanceConfiguration.IsFeatureEnabled(InstanceConfigurationFeatures.ApiRateLimiting))
        {
            await _next(context);
            return;
        }

        var result = await rateLimiter.CheckAsync(Guid.Empty, "log-ingestion");

        context.Response.Headers["X-RateLimit-Limit"] = result.Limit.ToString();
        context.Response.Headers["X-RateLimit-Remaining"] = Math.Max(0, result.Limit - result.CurrentCount).ToString();

        if (!result.IsAllowed)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.Headers["Retry-After"] = result.RetryAfterSeconds.ToString();
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Rate limit exceeded",
                retryAfter = result.RetryAfterSeconds
            });
            return;
        }

        await _next(context);
    }
}
