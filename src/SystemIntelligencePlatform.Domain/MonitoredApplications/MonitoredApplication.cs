using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace SystemIntelligencePlatform.MonitoredApplications;

public class MonitoredApplication : FullAuditedAggregateRoot<Guid>
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? Environment { get; set; }
    public string ApiKeyHash { get; set; } = null!;
    public bool IsActive { get; set; } = true;

    protected MonitoredApplication() { }

    public MonitoredApplication(
        Guid id,
        string name,
        string apiKeyHash,
        string? description = null,
        string? environment = null)
        : base(id)
    {
        Name = name;
        ApiKeyHash = apiKeyHash;
        Description = description;
        Environment = environment;
        IsActive = true;
    }

    public void RegenerateApiKey(string newApiKeyHash)
    {
        ApiKeyHash = newApiKeyHash;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }
}
