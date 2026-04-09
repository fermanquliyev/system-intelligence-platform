using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace SystemIntelligencePlatform.Copilot;

public class CopilotConversationMessage : CreationAuditedEntity<Guid>
{
    public Guid IncidentId { get; set; }
    public CopilotMessageRole Role { get; set; }
    public string Content { get; set; } = null!;

    protected CopilotConversationMessage() { }

    public CopilotConversationMessage(Guid id, Guid incidentId, CopilotMessageRole role, string content)
        : base(id)
    {
        IncidentId = incidentId;
        Role = role;
        Content = content;
    }
}
