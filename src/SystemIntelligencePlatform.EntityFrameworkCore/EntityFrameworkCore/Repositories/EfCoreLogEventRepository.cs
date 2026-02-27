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
}
