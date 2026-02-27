using System;
using Volo.Abp.Application.Dtos;

namespace SystemIntelligencePlatform.Incidents;

public class GetIncidentListInput : PagedAndSortedResultRequestDto
{
    public Guid? ApplicationId { get; set; }
    public IncidentSeverity? Severity { get; set; }
    public IncidentStatus? Status { get; set; }
    public string? Filter { get; set; }
}
