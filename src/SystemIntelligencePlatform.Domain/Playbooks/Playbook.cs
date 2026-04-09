using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;

namespace SystemIntelligencePlatform.Playbooks;

public class Playbook : FullAuditedAggregateRoot<Guid>
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    /// <summary>Trigger conditions (severity, text match, etc.) as JSON.</summary>
    public string TriggerDefinitionJson { get; set; } = "{}";
    public bool IsActive { get; set; } = true;

    public ICollection<PlaybookStep> Steps { get; set; } = new List<PlaybookStep>();

    protected Playbook() { }

    public Playbook(Guid id, string name, string triggerDefinitionJson)
        : base(id)
    {
        Name = name;
        TriggerDefinitionJson = triggerDefinitionJson;
    }
}
