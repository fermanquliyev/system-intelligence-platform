using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace SystemIntelligencePlatform.MonitoredApplications;

public interface IMonitoredApplicationRepository : IRepository<MonitoredApplication, Guid>
{
    Task<MonitoredApplication?> FindByApiKeyHashAsync(
        string apiKeyHash,
        CancellationToken cancellationToken = default);

    Task<MonitoredApplication?> FindByNameAsync(
        string name,
        Guid? tenantId,
        CancellationToken cancellationToken = default);
}
