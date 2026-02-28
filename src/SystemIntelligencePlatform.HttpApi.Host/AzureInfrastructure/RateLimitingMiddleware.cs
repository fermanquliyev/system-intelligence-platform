using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SystemIntelligencePlatform.RateLimiting;
using Volo.Abp.MultiTenancy;

namespace SystemIntelligencePlatform.RateLimiting;

/// <summary>
/// Middleware that enforces per-tenant rate limiting on the ingestion endpoint.
/// Returns 429 Too Many Requests with Retry-After header when limit is exceeded.
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;

    public RateLimitingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IRateLimiter rateLimiter, ICurrentTenant currentTenant)
    {
        if (!context.Request.Path.StartsWithSegments("/api/ingest"))
        {
            await _next(context);
            return;
        }

        var tenantId = currentTenant.Id ?? Guid.Empty;
        var result = await rateLimiter.CheckAsync(tenantId, "log-ingestion");

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
