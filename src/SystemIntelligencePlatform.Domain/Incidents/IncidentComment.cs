using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace SystemIntelligencePlatform.Incidents;

public class IncidentComment : CreationAuditedEntity<Guid>
{
    public Guid IncidentId { get; set; }
    public string Content { get; set; } = null!;

    protected IncidentComment() { }

    public IncidentComment(Guid id, Guid incidentId, string content)
        : base(id)
    {
        IncidentId = incidentId;
        Content = content;
    }
}
