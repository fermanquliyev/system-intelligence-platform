using System.Collections.Generic;
using System.Linq;

namespace SystemIntelligencePlatform.InstanceConfiguration;

public sealed class InstanceSettingDefinition
{
    public string Key { get; init; } = null!;
    public string DisplayName { get; init; } = null!;
    public string? Description { get; init; }
    public string Category { get; init; } = null!;
    public bool IsSecret { get; init; }
}

/// <summary>Known instance settings and features (metadata for UI and seeding).</summary>
public static class InstanceConfigurationRegistry
{
    public static IReadOnlyList<InstanceSettingDefinition> SettingDefinitions { get; } =
        new List<InstanceSettingDefinition>
        {
            new()
            {
                Key = "AI:Provider",
                DisplayName = "AI provider",
                Description = "e.g. Google",
                Category = "AI",
                IsSecret = false
            },
            new()
            {
                Key = "AI:Model",
                DisplayName = "AI model",
                Category = "AI",
                IsSecret = false
            },
            new()
            {
                Key = "AI:ApiKey",
                DisplayName = "AI API key",
                Category = "AI",
                IsSecret = true
            },
            new()
            {
                Key = "AI:Endpoint",
                DisplayName = "AI API endpoint",
                Category = "AI",
                IsSecret = false
            },
            new()
            {
                Key = "AI:MaxTokens",
                DisplayName = "Max output tokens",
                Category = "AI",
                IsSecret = false
            },
            new()
            {
                Key = "AI:Temperature",
                DisplayName = "Temperature",
                Category = "AI",
                IsSecret = false
            },
            new()
            {
                Key = "AI:TimeoutSeconds",
                DisplayName = "Request timeout (seconds)",
                Category = "AI",
                IsSecret = false
            },
            new()
            {
                Key = "RabbitMQ:Host",
                DisplayName = "RabbitMQ host",
                Category = "RabbitMQ",
                IsSecret = false
            },
            new()
            {
                Key = "RabbitMQ:Port",
                DisplayName = "RabbitMQ port",
                Category = "RabbitMQ",
                IsSecret = false
            },
            new()
            {
                Key = "RabbitMQ:Username",
                DisplayName = "RabbitMQ username",
                Category = "RabbitMQ",
                IsSecret = false
            },
            new()
            {
                Key = "RabbitMQ:Password",
                DisplayName = "RabbitMQ password",
                Category = "RabbitMQ",
                IsSecret = true
            },
            new()
            {
                Key = "RabbitMQ:VirtualHost",
                DisplayName = "RabbitMQ virtual host",
                Category = "RabbitMQ",
                IsSecret = false
            },
            new()
            {
                Key = "DataRetention:LogRetentionDays",
                DisplayName = "Log retention (days)",
                Description = "How long raw log events are kept.",
                Category = "Data",
                IsSecret = false
            },
            new()
            {
                Key = "ConnectionStrings:Default",
                DisplayName = "Default connection string",
                Description = "Stored for reference; the API process uses the startup connection until you restart.",
                Category = "Database",
                IsSecret = true
            },
            new()
            {
                Key = "Search:Provider",
                DisplayName = "Search provider",
                Description = "e.g. Database",
                Category = "Search",
                IsSecret = false
            },
            new()
            {
                Key = "OpenTelemetry:Enabled",
                DisplayName = "OpenTelemetry enabled",
                Description = "Requires API restart to take effect.",
                Category = "Observability",
                IsSecret = false
            },
            new()
            {
                Key = "RateLimiting:MaxRequestsPerWindow",
                DisplayName = "Rate limit: max requests per window",
                Category = "Rate limiting",
                IsSecret = false
            },
            new()
            {
                Key = "RateLimiting:WindowSizeSeconds",
                DisplayName = "Rate limit: window size (seconds)",
                Category = "Rate limiting",
                IsSecret = false
            },
            new()
            {
                Key = "Minio:Endpoint",
                DisplayName = "MinIO endpoint",
                Description = "Host and port only (no scheme), e.g. localhost:9000 or minio:9000 in Docker. Applied on next blob operation.",
                Category = "MinIO",
                IsSecret = false
            },
            new()
            {
                Key = "Minio:AccessKey",
                DisplayName = "MinIO access key",
                Category = "MinIO",
                IsSecret = false
            },
            new()
            {
                Key = "Minio:SecretKey",
                DisplayName = "MinIO secret key",
                Category = "MinIO",
                IsSecret = true
            },
            new()
            {
                Key = "Minio:UseSsl",
                DisplayName = "MinIO use SSL",
                Description = "true or false. Applied on next blob operation.",
                Category = "MinIO",
                IsSecret = false
            },
        };

    public static InstanceSettingDefinition? FindSetting(string key) =>
        SettingDefinitions.FirstOrDefault(d => d.Key == key);

    public sealed record FeatureSeed(string Name, string DisplayName, string? Description, bool DefaultEnabled, int Order);

    public static IReadOnlyList<FeatureSeed> FeatureSeeds { get; } = new List<FeatureSeed>
    {
        new(InstanceConfigurationFeatures.AiIncidentAnalysis, "AI incident analysis", "Call external LLM for root-cause summaries (falls back locally when off or on failure).", true, 0),
        new(InstanceConfigurationFeatures.RabbitMqMessaging, "RabbitMQ log pipeline", "Queue log events for the background worker. Disable only if you do not use ingestion.", true, 1),
        new(InstanceConfigurationFeatures.ApiRateLimiting, "API rate limiting", "Apply sliding-window limits to the public /api/ingest endpoint.", true, 2),
        new(InstanceConfigurationFeatures.DataRetentionJob, "Data retention job", "Allow the scheduled job to delete old log events.", true, 3),
        new(InstanceConfigurationFeatures.OpenTelemetry, "OpenTelemetry", "Export traces/metrics (effective after API restart).", true, 4),
    };
}
