using System;
using Volo.Abp.Application.Dtos;

namespace SystemIntelligencePlatform.Playbooks;

public class PlaybookRunStepDto : EntityDto<Guid>
{
    public int StepOrder { get; set; }
    public string Title { get; set; } = null!;
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
}
