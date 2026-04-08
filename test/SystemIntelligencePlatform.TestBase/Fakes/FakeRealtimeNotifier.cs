using System.Collections.Generic;
using System.Threading.Tasks;
using SystemIntelligencePlatform.Incidents;
using Volo.Abp.DependencyInjection;

namespace SystemIntelligencePlatform.Fakes;

[Dependency(ReplaceServices = true)]
public class FakeRealtimeNotifier : IRealtimeNotifier, ITransientDependency
{
    public List<IncidentNotification> SentNotifications { get; } = new();

    public Task NotifyIncidentCreatedAsync(IncidentNotification notification)
    {
        SentNotifications.Add(notification);
        return Task.CompletedTask;
    }

    public Task NotifyIncidentUpdatedAsync(IncidentNotification notification)
    {
        SentNotifications.Add(notification);
        return Task.CompletedTask;
    }

    public Task NotifyIncidentResolvedAsync(IncidentNotification notification)
    {
        SentNotifications.Add(notification);
        return Task.CompletedTask;
    }
}
