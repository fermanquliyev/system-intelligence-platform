using System;
using System.Threading.Tasks;

namespace SystemIntelligencePlatform.Incidents;

/// <summary>
/// Pushes real-time events to connected clients, scoped by tenant.
/// Infrastructure layer provides the Azure SignalR implementation.
/// </summary>
public interface IRealtimeNotifier
{
    Task NotifyIncidentCreatedAsync(Guid? tenantId, IncidentNotification notification);
    Task NotifyIncidentUpdatedAsync(Guid? tenantId, IncidentNotification notification);
    Task NotifyIncidentResolvedAsync(Guid? tenantId, IncidentNotification notification);
}

public class IncidentNotification
{
    public Guid IncidentId { get; set; }
    public string Title { get; set; } = null!;
    public string Severity { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string ApplicationName { get; set; } = null!;
    public int OccurrenceCount { get; set; }
    public DateTime Timestamp { get; set; }
}
