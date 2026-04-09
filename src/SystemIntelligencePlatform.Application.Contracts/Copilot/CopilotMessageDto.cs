using System;
using Volo.Abp.Application.Dtos;

namespace SystemIntelligencePlatform.Copilot;

public class CopilotMessageDto : CreationAuditedEntityDto<Guid>
{
    public Guid IncidentId { get; set; }
    public CopilotMessageRole Role { get; set; }
    public string Content { get; set; } = null!;
}
