using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SystemIntelligencePlatform.Incidents;
using SystemIntelligencePlatform.MonitoredApplications;
using SystemIntelligencePlatform.Permissions;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SystemIntelligencePlatform.Dashboard;

[Authorize(SystemIntelligencePlatformPermissions.DashboardPermissions.Default)]
public class DashboardAppService : ApplicationService, IDashboardAppService
{
    private readonly IRepository<MonitoredApplication, Guid> _applicationRepository;
    private readonly IIncidentRepository _incidentRepository;
    private readonly IRepository<LogEvents.LogEvent, Guid> _logEventRepository;

    public DashboardAppService(
        IRepository<MonitoredApplication, Guid> applicationRepository,
        IIncidentRepository incidentRepository,
        IRepository<LogEvents.LogEvent, Guid> logEventRepository)
    {
        _applicationRepository = applicationRepository;
        _incidentRepository = incidentRepository;
        _logEventRepository = logEventRepository;
    }

    public async Task<DashboardDto> GetAsync(Guid? applicationId = null)
    {
        var appQueryable = await _applicationRepository.GetQueryableAsync();
        var totalApps = await AsyncExecuter.CountAsync(appQueryable);

        var incidentQueryable = await _incidentRepository.GetQueryableAsync();
        var openIncidents = await AsyncExecuter.CountAsync(
            incidentQueryable.Where(i => i.Status != IncidentStatus.Resolved && i.Status != IncidentStatus.Closed));

        var criticalIncidents = await AsyncExecuter.CountAsync(
            incidentQueryable.Where(i => i.Severity == IncidentSeverity.Critical &&
                                         i.Status != IncidentStatus.Resolved &&
                                         i.Status != IncidentStatus.Closed));

        var today = DateTime.UtcNow.Date;
        var logQueryable = await _logEventRepository.GetQueryableAsync();
        var logsToday = await AsyncExecuter.LongCountAsync(
            logQueryable.Where(l => l.Timestamp >= today));

        var severityDist = await _incidentRepository.GetSeverityDistributionAsync(applicationId);
        var trend = await _incidentRepository.GetTrendAsync(30, applicationId);
        var recent = await _incidentRepository.GetActiveIncidentsAsync(applicationId, 10);

        var appIds = recent.Select(i => i.ApplicationId).Distinct().ToList();
        var apps = await AsyncExecuter.ToListAsync(appQueryable.Where(a => appIds.Contains(a.Id)));
        var appLookup = apps.ToDictionary(a => a.Id, a => a.Name);

        var recentDtos = ObjectMapper.Map<System.Collections.Generic.List<Incident>, System.Collections.Generic.List<IncidentDto>>(recent);
        foreach (var dto in recentDtos)
        {
            dto.ApplicationName = appLookup.TryGetValue(dto.ApplicationId, out var name) ? name : "Unknown";
        }

        return new DashboardDto
        {
            TotalApplications = totalApps,
            TotalOpenIncidents = openIncidents,
            TotalCriticalIncidents = criticalIncidents,
            TotalLogsToday = logsToday,
            SeverityDistribution = severityDist.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value),
            IncidentTrend = trend.Select(t => new IncidentTrendItemDto { Date = t.Date, Count = t.Count }).ToList(),
            RecentIncidents = recentDtos
        };
    }
}
