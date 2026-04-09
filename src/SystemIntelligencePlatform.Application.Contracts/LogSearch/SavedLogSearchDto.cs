using System;
using Volo.Abp.Application.Dtos;

namespace SystemIntelligencePlatform.LogSearch;

public class SavedLogSearchDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; } = null!;
    public string FilterJson { get; set; } = null!;
}
