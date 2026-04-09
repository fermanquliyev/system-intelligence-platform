using System;

namespace SystemIntelligencePlatform.Incidents;

public class IncidentTimelineItemDto
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string Title { get; set; } = null!;
    public IncidentSeverity Severity { get; set; }
    public IncidentStatus Status { get; set; }
    public string ApplicationName { get; set; } = null!;
    public string Kind { get; set; } = "Incident";
}
