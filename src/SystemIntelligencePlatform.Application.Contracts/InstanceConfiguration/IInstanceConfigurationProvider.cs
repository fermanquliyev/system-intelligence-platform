namespace SystemIntelligencePlatform.InstanceConfiguration;

/// <summary>
/// Runtime configuration: database overrides, then <see cref="Microsoft.Extensions.Configuration.IConfiguration"/> (appsettings + environment).
/// </summary>
public interface IInstanceConfigurationProvider
{
    /// <summary>Invalidate cached snapshot (call after updating instance settings or features).</summary>
    void RequestRefresh();

    /// <summary>Returns whether the feature exists and is enabled (missing row defaults to enabled).</summary>
    bool IsFeatureEnabled(string featureName);

    /// <summary>Effective value for a configuration key (colon-separated path).</summary>
    string? GetEffectiveSetting(string key);
}
