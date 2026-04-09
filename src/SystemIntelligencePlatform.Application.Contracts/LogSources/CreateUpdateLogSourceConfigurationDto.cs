using System.ComponentModel.DataAnnotations;

namespace SystemIntelligencePlatform.LogSources;

public class CreateUpdateLogSourceConfigurationDto
{
    [Required]
    [StringLength(256)]
    public string Name { get; set; } = null!;

    public LogSourceType SourceType { get; set; }

    public bool IsEnabled { get; set; } = true;

    [Required]
    public string SettingsJson { get; set; } = "{}";
}
