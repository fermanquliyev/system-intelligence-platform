using System;
using Volo.Abp.Application.Dtos;

namespace SystemIntelligencePlatform.LogSources;

public class LogSourceConfigurationDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; } = null!;
    public LogSourceType SourceType { get; set; }
    public bool IsEnabled { get; set; }
    public string SettingsJson { get; set; } = null!;
}
