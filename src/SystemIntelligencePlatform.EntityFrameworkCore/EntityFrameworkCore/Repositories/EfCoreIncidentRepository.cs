using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SystemIntelligencePlatform.Incidents;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace SystemIntelligencePlatform.EntityFrameworkCore.Repositories;

public class EfCoreIncidentRepository
    : EfCoreRepository<SystemIntelligencePlatformDbContext, Incident, Guid>,
      IIncidentRepository
{
    public EfCoreIncidentRepository(
        IDbContextProvider<SystemIntelligencePlatformDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<Incident?> FindByHashSignatureAsync(
        string hashSignature, Guid applicationId,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .FirstOrDefaultAsync(i => i.HashSignature == hashSignature
                                   && i.ApplicationId == applicationId
                                   && i.Status != IncidentStatus.Resolved
                                   && i.Status != IncidentStatus.Closed,
                cancellationToken);
    }

    public async Task<List<Incident>> GetActiveIncidentsAsync(
        Guid? applicationId = null, int maxCount = 50,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        var query = dbSet.AsNoTracking()
            .Where(i => i.Status != IncidentStatus.Resolved && i.Status != IncidentStatus.Closed);

        if (applicationId.HasValue)
            query = query.Where(i => i.ApplicationId == applicationId.Value);

        return await query
            .OrderByDescending(i => i.LastOccurrence)
            .Take(maxCount)
            .ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<IncidentSeverity, int>> GetSeverityDistributionAsync(
        Guid? applicationId = null,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        var query = dbSet.AsNoTracking()
            .Where(i => i.Status != IncidentStatus.Resolved && i.Status != IncidentStatus.Closed);

        if (applicationId.HasValue)
            query = query.Where(i => i.ApplicationId == applicationId.Value);

        return await query
            .GroupBy(i => i.Severity)
            .Select(g => new { Severity = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Severity, x => x.Count, cancellationToken);
    }

    public async Task<List<IncidentTrendItem>> GetTrendAsync(
        int days = 30, Guid? applicationId = null,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        var cutoff = DateTime.UtcNow.AddDays(-days);

        var query = dbSet.AsNoTracking()
            .Where(i => i.CreationTime >= cutoff);

        if (applicationId.HasValue)
            query = query.Where(i => i.ApplicationId == applicationId.Value);

        return await query
            .GroupBy(i => i.CreationTime.Date)
            .Select(g => new IncidentTrendItem { Date = g.Key, Count = g.Count() })
            .OrderBy(t => t.Date)
            .ToListAsync(cancellationToken);
    }
}
