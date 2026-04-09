using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace SystemIntelligencePlatform.Alerting;

public class AlertRule : FullAuditedAggregateRoot<Guid>
{
    public string Name { get; set; } = null!;
    public bool IsEnabled { get; set; } = true;
    /// <summary>JSON definition for composite conditions (error spike, latency, Z-score, etc.).</summary>
    public string DefinitionJson { get; set; } = "{}";
    public int? SeverityOverride { get; set; }
    public Guid? ApplicationId { get; set; }

    protected AlertRule() { }

    public AlertRule(Guid id, string name, string definitionJson, Guid? applicationId = null)
        : base(id)
    {
        Name = name;
        DefinitionJson = definitionJson;
        ApplicationId = applicationId;
    }
}
