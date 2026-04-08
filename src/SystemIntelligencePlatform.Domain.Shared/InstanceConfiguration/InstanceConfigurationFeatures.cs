namespace SystemIntelligencePlatform.InstanceConfiguration;

/// <summary>Feature flag names stored in <see cref="InstanceFeature"/>.</summary>
public static class InstanceConfigurationFeatures
{
    public const string AiIncidentAnalysis = nameof(AiIncidentAnalysis);
    public const string RabbitMqMessaging = nameof(RabbitMqMessaging);
    public const string ApiRateLimiting = nameof(ApiRateLimiting);
    public const string DataRetentionJob = nameof(DataRetentionJob);
    public const string OpenTelemetry = nameof(OpenTelemetry);
}
