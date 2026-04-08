using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace SystemIntelligencePlatform.InstanceConfiguration;

public class InstanceFeature : FullAuditedAggregateRoot<Guid>
{
    public string Name { get; protected set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsEnabled { get; set; }
    public int DisplayOrder { get; set; }

    protected InstanceFeature()
    {
    }

    public InstanceFeature(
        Guid id,
        string name,
        string displayName,
        string? description,
        bool isEnabled,
        int displayOrder = 0)
        : base(id)
    {
        Name = name;
        DisplayName = displayName;
        Description = description;
        IsEnabled = isEnabled;
        DisplayOrder = displayOrder;
    }
}
