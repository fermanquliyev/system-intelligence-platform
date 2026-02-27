using System;

namespace SystemIntelligencePlatform.MonitoredApplications;

public class ApiKeyResultDto
{
    public Guid ApplicationId { get; set; }
    public string ApiKey { get; set; } = null!;
}
