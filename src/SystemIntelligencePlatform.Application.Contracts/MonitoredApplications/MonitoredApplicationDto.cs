using System;
using Volo.Abp.Application.Dtos;

namespace SystemIntelligencePlatform.MonitoredApplications;

public class MonitoredApplicationDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? Environment { get; set; }
    public bool IsActive { get; set; }
}
