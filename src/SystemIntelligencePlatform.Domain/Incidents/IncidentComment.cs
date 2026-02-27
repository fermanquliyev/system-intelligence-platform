using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SystemIntelligencePlatform.Incidents;

public class IncidentComment : CreationAuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    public Guid IncidentId { get; set; }
    public string Content { get; set; } = null!;

    protected IncidentComment() { }

    public IncidentComment(Guid id, Guid incidentId, string content, Guid? tenantId = null)
        : base(id)
    {
        TenantId = tenantId;
        IncidentId = incidentId;
        Content = content;
    }
}
