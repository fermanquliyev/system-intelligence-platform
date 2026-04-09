using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace SystemIntelligencePlatform.Playbooks;

public class PlaybookRunDto : FullAuditedEntityDto<Guid>
{
    public Guid PlaybookId { get; set; }
    public Guid IncidentId { get; set; }
    public PlaybookRunStatus Status { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<PlaybookRunStepDto> RunSteps { get; set; } = new();
}
