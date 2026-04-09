using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace SystemIntelligencePlatform.LogSources;

public class LogSourceConfiguration : FullAuditedAggregateRoot<Guid>
{
    public string Name { get; set; } = null!;
    public LogSourceType SourceType { get; set; }
    public bool IsEnabled { get; set; } = true;
    /// <summary>Adapter-specific settings (path, port, URL, etc.).</summary>
    public string SettingsJson { get; set; } = "{}";

    protected LogSourceConfiguration() { }

    public LogSourceConfiguration(Guid id, string name, LogSourceType sourceType, string settingsJson)
        : base(id)
    {
        Name = name;
        SourceType = sourceType;
        SettingsJson = settingsJson;
    }
}
