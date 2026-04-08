using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace SystemIntelligencePlatform.InstanceConfiguration;

public class InstanceSetting : FullAuditedAggregateRoot<Guid>
{
    public string Key { get; protected set; } = null!;
    public string Value { get; set; } = string.Empty;
    public bool IsSecret { get; set; }

    protected InstanceSetting()
    {
    }

    public InstanceSetting(Guid id, string key, string value, bool isSecret)
        : base(id)
    {
        Key = key;
        Value = value;
        IsSecret = isSecret;
    }
}
