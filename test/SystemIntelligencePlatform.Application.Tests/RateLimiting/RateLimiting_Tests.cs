using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using SystemIntelligencePlatform.RateLimiting;
using Xunit;

namespace SystemIntelligencePlatform.RateLimiting;

/// <summary>
/// Pure unit tests for rate limiting functionality.
/// Tests verify that the SlidingWindowRateLimiter correctly enforces rate limits
/// using a distributed cache backend.
/// </summary>
public class RateLimiting_Tests
{
    private readonly IDistributedCache _mockCache;
    private readonly IOptions<RateLimitingOptions> _mockOptions;

    public RateLimiting_Tests()
    {
        _mockCache = Substitute.For<IDistributedCache>();
        _mockOptions = Substitute.For<IOptions<RateLimitingOptions>>();
        _mockOptions.Value.Returns(new RateLimitingOptions
        {
            MaxRequestsPerWindow = 1000,
            WindowSizeSeconds = 60
        });
    }

    /// <summary>
    /// Verifies that requests are allowed when under the rate limit threshold.
    /// This is the happy path for rate limiting.
    /// </summary>
    [Fact]
    public async Task Should_Allow_When_Under_Limit()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var resource = "log-ingestion";
        var rateLimiter = new SlidingWindowRateLimiter(_mockCache, _mockOptions);

        // Mock cache returning null (no existing entries) or low count
        _mockCache.GetAsync(Arg.Any<string>(), Arg.Any<System.Threading.CancellationToken>())
            .Returns((byte[]?)null);

        // Act
        var result = await rateLimiter.CheckAsync(tenantId, resource);

        // Assert
        result.IsAllowed.ShouldBeTrue();
        result.RetryAfterSeconds.ShouldBe(0);
    }

    /// <summary>
    /// Verifies that requests are denied when over the rate limit and returns appropriate retry-after time.
    /// This ensures the rate limiter properly blocks excessive requests.
    /// </summary>
    [Fact]
    public async Task Should_Deny_When_Over_Limit_And_Return_RetryAfter()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var resource = "log-ingestion";
        var rateLimiter = new SlidingWindowRateLimiter(_mockCache, _mockOptions);

        // Mock cache returning a count that exceeds the limit
        // Simulating 1001 requests (over the 1000 limit)
        var cachedCountBytes = System.Text.Encoding.UTF8.GetBytes("1001");
        _mockCache.GetAsync(Arg.Any<string>(), Arg.Any<System.Threading.CancellationToken>())
            .Returns(cachedCountBytes);

        // Act
        var result = await rateLimiter.CheckAsync(tenantId, resource);

        // Assert
        result.IsAllowed.ShouldBeFalse();
        result.RetryAfterSeconds.ShouldBeGreaterThan(0);
    }

    /// <summary>
    /// Verifies that rate limits are isolated per tenant.
    /// TenantA's requests should not affect TenantB's rate limit.
    /// </summary>
    [Fact]
    public async Task TenantA_Should_Not_Affect_TenantB()
    {
        // Arrange
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var resource = "log-ingestion";
        var rateLimiter = new SlidingWindowRateLimiter(_mockCache, _mockOptions);

        // Mock cache to return high count for tenantA but null for tenantB
        var highCountBytes = System.Text.Encoding.UTF8.GetBytes("1001");
        _mockCache.GetAsync(Arg.Is<string>(key => key.Contains(tenantA.ToString())), Arg.Any<System.Threading.CancellationToken>())
            .Returns(highCountBytes);
        _mockCache.GetAsync(Arg.Is<string>(key => key.Contains(tenantB.ToString())), Arg.Any<System.Threading.CancellationToken>())
            .Returns((byte[]?)null);

        // Act
        var resultTenantA = await rateLimiter.CheckAsync(tenantA, resource);
        var resultTenantB = await rateLimiter.CheckAsync(tenantB, resource);

        // Assert
        resultTenantA.IsAllowed.ShouldBeFalse();
        resultTenantB.IsAllowed.ShouldBeTrue();
    }

    /// <summary>
    /// Verifies that rate limits reset after the time window expires.
    /// This ensures the sliding window mechanism works correctly.
    /// </summary>
    [Fact]
    public async Task Should_Reset_After_Window_Expires()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var resource = "log-ingestion";
        var rateLimiter = new SlidingWindowRateLimiter(_mockCache, _mockOptions);

        // First check: cache returns high count (over limit)
        var highCountBytes = System.Text.Encoding.UTF8.GetBytes("1001");
        _mockCache.GetAsync(Arg.Any<string>(), Arg.Any<System.Threading.CancellationToken>())
            .Returns(highCountBytes);

        var result1 = await rateLimiter.CheckAsync(tenantId, resource);
        result1.IsAllowed.ShouldBeFalse();

        // Simulate window expiration: cache now returns null (expired)
        _mockCache.GetAsync(Arg.Any<string>(), Arg.Any<System.Threading.CancellationToken>())
            .Returns((byte[]?)null);

        // Act
        var result2 = await rateLimiter.CheckAsync(tenantId, resource);

        // Assert
        result2.IsAllowed.ShouldBeTrue();
    }
}
