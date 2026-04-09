using System;

namespace SystemIntelligencePlatform.Incidents;

public class IncidentRootCauseTimelineItemDto
{
    public string EventType { get; set; } = null!;
    public DateTime TimestampUtc { get; set; }
    public string Title { get; set; } = null!;
    public string? Detail { get; set; }
    public bool IsCritical { get; set; }
}
