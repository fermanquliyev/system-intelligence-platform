using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using SystemIntelligencePlatform.Incidents;
using Volo.Abp.DependencyInjection;

namespace SystemIntelligencePlatform.AzureInfrastructure;

public class SignalRRealtimeNotifier : IRealtimeNotifier, ITransientDependency
{
    private readonly IHubContext<IncidentHub> _hubContext;

    public SignalRRealtimeNotifier(IHubContext<IncidentHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyIncidentCreatedAsync(Guid? tenantId, IncidentNotification notification)
    {
        var group = GetTenantGroup(tenantId);
        await _hubContext.Clients.Group(group).SendAsync("IncidentCreated", notification);
    }

    public async Task NotifyIncidentUpdatedAsync(Guid? tenantId, IncidentNotification notification)
    {
        var group = GetTenantGroup(tenantId);
        await _hubContext.Clients.Group(group).SendAsync("IncidentUpdated", notification);
    }

    public async Task NotifyIncidentResolvedAsync(Guid? tenantId, IncidentNotification notification)
    {
        var group = GetTenantGroup(tenantId);
        await _hubContext.Clients.Group(group).SendAsync("IncidentResolved", notification);
    }

    private static string GetTenantGroup(Guid? tenantId)
    {
        return tenantId.HasValue ? $"tenant_{tenantId.Value}" : "host";
    }
}
