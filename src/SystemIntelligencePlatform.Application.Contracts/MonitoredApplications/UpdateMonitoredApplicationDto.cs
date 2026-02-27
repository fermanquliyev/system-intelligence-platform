using System.ComponentModel.DataAnnotations;

namespace SystemIntelligencePlatform.MonitoredApplications;

public class UpdateMonitoredApplicationDto
{
    [Required]
    [StringLength(MonitoredApplicationConsts.MaxNameLength)]
    public string Name { get; set; } = null!;

    [StringLength(MonitoredApplicationConsts.MaxDescriptionLength)]
    public string? Description { get; set; }

    [StringLength(MonitoredApplicationConsts.MaxEnvironmentLength)]
    public string? Environment { get; set; }

    public bool IsActive { get; set; }
}
