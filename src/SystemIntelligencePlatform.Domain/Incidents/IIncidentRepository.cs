using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace SystemIntelligencePlatform.Incidents;

public interface IIncidentRepository : IRepository<Incident, Guid>
{
    Task<Incident?> FindByHashSignatureAsync(
        string hashSignature,
        Guid applicationId,
        CancellationToken cancellationToken = default);

    Task<List<Incident>> GetActiveIncidentsAsync(
        Guid? applicationId = null,
        int maxCount = 50,
        CancellationToken cancellationToken = default);

    Task<Dictionary<IncidentSeverity, int>> GetSeverityDistributionAsync(
        Guid? applicationId = null,
        CancellationToken cancellationToken = default);

    Task<List<IncidentTrendItem>> GetTrendAsync(
        int days = 30,
        Guid? applicationId = null,
        CancellationToken cancellationToken = default);
}

public class IncidentTrendItem
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
}
