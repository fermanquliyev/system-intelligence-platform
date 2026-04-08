using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace SystemIntelligencePlatform.InstanceConfiguration;

public class InstanceConfigurationSnapshotDto
{
    public List<InstanceFeatureStateDto> Features { get; set; } = new();
    public List<InstanceSettingStateDto> Settings { get; set; } = new();
}

public class InstanceFeatureStateDto : EntityDto<System.Guid>
{
    public string Name { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsEnabled { get; set; }
    public int DisplayOrder { get; set; }
}

public class InstanceSettingStateDto
{
    public string Key { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string? Description { get; set; }
    public string Category { get; set; } = null!;
    public bool IsSecret { get; set; }
    /// <summary>Masked or plain effective value for display.</summary>
    public string EffectiveDisplayValue { get; set; } = null!;
    /// <summary>True when the value comes from the database (vs appsettings/env).</summary>
    public bool IsOverriddenInDatabase { get; set; }
}

public class UpdateInstanceFeaturesDto
{
    public Dictionary<string, bool> Features { get; set; } = new();
}

public class UpdateInstanceSettingsDto
{
    /// <summary>Key/value pairs to persist. Omit keys to leave unchanged. For secrets, send empty to keep DB value.</summary>
    public Dictionary<string, string?> Values { get; set; } = new();
}

public class ApplyMigrationsResultDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}
