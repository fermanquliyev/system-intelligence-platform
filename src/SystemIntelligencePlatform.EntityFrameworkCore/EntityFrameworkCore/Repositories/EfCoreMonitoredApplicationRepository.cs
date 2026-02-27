using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SystemIntelligencePlatform.MonitoredApplications;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace SystemIntelligencePlatform.EntityFrameworkCore.Repositories;

public class EfCoreMonitoredApplicationRepository
    : EfCoreRepository<SystemIntelligencePlatformDbContext, MonitoredApplication, Guid>,
      IMonitoredApplicationRepository
{
    public EfCoreMonitoredApplicationRepository(
        IDbContextProvider<SystemIntelligencePlatformDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<MonitoredApplication?> FindByApiKeyHashAsync(
        string apiKeyHash, CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.ApiKeyHash == apiKeyHash, cancellationToken);
    }

    public async Task<MonitoredApplication?> FindByNameAsync(
        string name, Guid? tenantId, CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Name == name && a.TenantId == tenantId, cancellationToken);
    }
}
