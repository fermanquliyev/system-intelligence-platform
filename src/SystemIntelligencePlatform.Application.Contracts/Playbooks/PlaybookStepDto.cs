using System;
using Volo.Abp.Application.Dtos;

namespace SystemIntelligencePlatform.Playbooks;

public class PlaybookStepDto : EntityDto<Guid>
{
    public int SortOrder { get; set; }
    public string Title { get; set; } = null!;
    public string? Body { get; set; }
}
