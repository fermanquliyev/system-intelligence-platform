using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace SystemIntelligencePlatform.Playbooks;

public class PlaybookDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string TriggerDefinitionJson { get; set; } = null!;
    public bool IsActive { get; set; }
    public List<PlaybookStepDto> Steps { get; set; } = new();
}
