using System;
using System.Globalization;
using SystemIntelligencePlatform.AI;
using SystemIntelligencePlatform.Infrastructure;
using SystemIntelligencePlatform.LogEvents;
using SystemIntelligencePlatform.RateLimiting;
using Microsoft.Extensions.Options;

namespace SystemIntelligencePlatform.InstanceConfiguration;

public static class EffectiveConfigurationBinder
{
    public static GoogleAiOptions GetGoogleAi(IInstanceConfigurationProvider provider, IOptions<GoogleAiOptions> fileOptions)
    {
        var f = fileOptions.Value;
        var o = new GoogleAiOptions
        {
            Provider = provider.GetEffectiveSetting("AI:Provider") ?? f.Provider,
            Model = provider.GetEffectiveSetting("AI:Model") ?? f.Model,
            ApiKey = provider.GetEffectiveSetting("AI:ApiKey") ?? f.ApiKey ?? "",
            Endpoint = provider.GetEffectiveSetting("AI:Endpoint") ?? f.Endpoint,
            MaxTokens = ParseInt(provider.GetEffectiveSetting("AI:MaxTokens"), f.MaxTokens),
            Temperature = ParseDouble(provider.GetEffectiveSetting("AI:Temperature"), f.Temperature),
            TimeoutSeconds = ParseInt(provider.GetEffectiveSetting("AI:TimeoutSeconds"), f.TimeoutSeconds),
            MaxRetries = ParseInt(provider.GetEffectiveSetting("AI:MaxRetries"), f.MaxRetries),
            CircuitBreakerFailureThreshold = ParseInt(provider.GetEffectiveSetting("AI:CircuitBreakerFailureThreshold"), f.CircuitBreakerFailureThreshold),
            CircuitBreakerResetSeconds = ParseInt(provider.GetEffectiveSetting("AI:CircuitBreakerResetSeconds"), f.CircuitBreakerResetSeconds),
        };
        if (int.TryParse(provider.GetEffectiveSetting("AI:RequestsPerMinuteLimit"), NumberStyles.Integer, CultureInfo.InvariantCulture, out var rpm))
            o.RequestsPerMinuteLimit = rpm;
        else
            o.RequestsPerMinuteLimit = f.RequestsPerMinuteLimit;
        return o;
    }

    public static RabbitMqOptions GetRabbitMq(IInstanceConfigurationProvider provider, IOptions<RabbitMqOptions> fileOptions)
    {
        var f = fileOptions.Value;
        return new RabbitMqOptions
        {
            Host = provider.GetEffectiveSetting("RabbitMQ:Host") ?? f.Host,
            Port = ParseInt(provider.GetEffectiveSetting("RabbitMQ:Port"), f.Port),
            Username = provider.GetEffectiveSetting("RabbitMQ:Username") ?? f.Username,
            Password = provider.GetEffectiveSetting("RabbitMQ:Password") ?? f.Password,
            VirtualHost = provider.GetEffectiveSetting("RabbitMQ:VirtualHost") ?? f.VirtualHost,
        };
    }

    public static DataRetentionOptions GetDataRetention(IInstanceConfigurationProvider provider, IOptions<DataRetentionOptions> fileOptions)
    {
        var f = fileOptions.Value;
        return new DataRetentionOptions
        {
            LogRetentionDays = ParseInt(provider.GetEffectiveSetting("DataRetention:LogRetentionDays"), f.LogRetentionDays),
        };
    }

    public static RateLimitingOptions GetRateLimiting(IInstanceConfigurationProvider provider, IOptions<RateLimitingOptions> fileOptions)
    {
        var f = fileOptions.Value;
        return new RateLimitingOptions
        {
            MaxRequestsPerWindow = ParseInt(provider.GetEffectiveSetting("RateLimiting:MaxRequestsPerWindow"), f.MaxRequestsPerWindow),
            WindowSizeSeconds = ParseInt(provider.GetEffectiveSetting("RateLimiting:WindowSizeSeconds"), f.WindowSizeSeconds),
        };
    }

    private static int ParseInt(string? s, int fallback) =>
        int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : fallback;

    private static double ParseDouble(string? s, double fallback) =>
        double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : fallback;
}
