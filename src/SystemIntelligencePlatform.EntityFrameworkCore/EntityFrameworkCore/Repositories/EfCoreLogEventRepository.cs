using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SystemIntelligencePlatform.LogEvents;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace SystemIntelligencePlatform.EntityFrameworkCore.Repositories;

public class EfCoreLogEventRepository
    : EfCoreRepository<SystemIntelligencePlatformDbContext, LogEvent, Guid>,
      ILogEventRepository
{
    public EfCoreLogEventRepository(
        IDbContextProvider<SystemIntelligencePlatformDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task BulkInsertAsync(
        IEnumerable<LogEvent> logEvents, CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        await dbContext.LogEvents.AddRangeAsync(logEvents, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> GetCountByHashSignatureAsync(
        string hashSignature, Guid applicationId, TimeSpan window,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        var cutoff = DateTime.UtcNow - window;

        return await dbSet
            .AsNoTracking()
            .CountAsync(e => e.HashSignature == hashSignature
                          && e.ApplicationId == applicationId
                          && e.Timestamp >= cutoff,
                cancellationToken);
    }

    public async Task<List<LogEvent>> GetRecentByHashSignatureAsync(
        string hashSignature, Guid applicationId, int maxCount = 5,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();

        return await dbSet
            .AsNoTracking()
            .Where(e => e.HashSignature == hashSignature && e.ApplicationId == applicationId)
            .OrderByDescending(e => e.Timestamp)
            .Take(maxCount)
            .ToListAsync(cancellationToken);
    }

    public async Task<AnomalyMetrics> GetAnomalyMetricsAsync(
        string hashSignature, Guid applicationId,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        var now = DateTime.UtcNow;
        var fiveMinAgo = now.AddMinutes(-5);
        var oneHourAgo = now.AddHours(-1);
        var oneDayAgo = now.AddDays(-1);
        var sevenDaysAgo = now.AddDays(-7);

        var baseQuery = dbSet.AsNoTracking()
            .Where(e => e.HashSignature == hashSignature && e.ApplicationId == applicationId);

        var last5Min = await baseQuery.CountAsync(e => e.Timestamp >= fiveMinAgo, cancellationToken);
        var last1Hour = await baseQuery.CountAsync(e => e.Timestamp >= oneHourAgo, cancellationToken);
        var last24Hours = await baseQuery.CountAsync(e => e.Timestamp >= oneDayAgo, cancellationToken);

        // Compute 7-day hourly baseline: group events by hour, compute avg and stddev
        var hourlyCounts = await baseQuery
            .Where(e => e.Timestamp >= sevenDaysAgo)
            .GroupBy(e => new { e.Timestamp.Date, e.Timestamp.Hour })
            .Select(g => g.Count())
            .ToListAsync(cancellationToken);

        double avgBaseline = 0;
        double stdDev = 0;

        if (hourlyCounts.Count > 0)
        {
            avgBaseline = hourlyCounts.Average();
            var sumSquares = hourlyCounts.Sum(c => Math.Pow(c - avgBaseline, 2));
            stdDev = Math.Sqrt(sumSquares / hourlyCounts.Count);
        }

        return new AnomalyMetrics
        {
            EventsLast5Min = last5Min,
            EventsLast1Hour = last1Hour,
            EventsLast24Hours = last24Hours,
            AverageHourlyBaseline = avgBaseline,
            StandardDeviation = stdDev
        };
    }

    public async Task<long> GetCountOlderThanAsync(
        DateTime cutoff, CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.AsNoTracking().LongCountAsync(e => e.Timestamp < cutoff, cancellationToken);
    }

    public async Task<List<LogEvent>> GetOlderThanAsync(
        DateTime cutoff, int batchSize, CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(e => e.Timestamp < cutoff)
            .OrderBy(e => e.Timestamp)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task DeleteBatchAsync(
        IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        var idList = ids.ToList();
        await dbContext.LogEvents.Where(e => idList.Contains(e.Id)).ExecuteDeleteAsync(cancellationToken);
    }
}
