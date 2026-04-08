using System;
using System.Threading.Tasks;

namespace SystemIntelligencePlatform.Incidents;

/// <summary>
/// Pushes real-time events to connected clients.
/// </summary>
public interface IRealtimeNotifier
{
    Task NotifyIncidentCreatedAsync(IncidentNotification notification);
    Task NotifyIncidentUpdatedAsync(IncidentNotification notification);
    Task NotifyIncidentResolvedAsync(IncidentNotification notification);
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
