using System;
using Volo.Abp.Application.Dtos;

namespace SystemIntelligencePlatform.Incidents;

public class IncidentCommentDto : CreationAuditedEntityDto<Guid>
{
    public Guid IncidentId { get; set; }
    public string Content { get; set; } = null!;
}
