using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using SystemIntelligencePlatform.Incidents;
using Volo.Abp.DependencyInjection;

namespace SystemIntelligencePlatform.Realtime;

public class SelfHostedRealtimeNotifier : IRealtimeNotifier, ITransientDependency
{
    private readonly IHubContext<IncidentHub> _hubContext;

    public SelfHostedRealtimeNotifier(IHubContext<IncidentHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task NotifyIncidentCreatedAsync(IncidentNotification notification)
    {
        return _hubContext.Clients.All.SendAsync("IncidentCreated", notification);
    }

    public Task NotifyIncidentUpdatedAsync(IncidentNotification notification)
    {
        return _hubContext.Clients.All.SendAsync("IncidentUpdated", notification);
    }

    public Task NotifyIncidentResolvedAsync(IncidentNotification notification)
    {
        return _hubContext.Clients.All.SendAsync("IncidentResolved", notification);
    }
}
