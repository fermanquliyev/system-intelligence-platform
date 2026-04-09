using System;
using Volo.Abp.Application.Dtos;

namespace SystemIntelligencePlatform.Incidents;

public class GetGlobalTimelineInput : PagedAndSortedResultRequestDto
{
    public Guid? ApplicationId { get; set; }
    public IncidentSeverity? Severity { get; set; }
    public DateTime? FromUtc { get; set; }
    public DateTime? ToUtc { get; set; }
    public string? TimeScale { get; set; }
}
