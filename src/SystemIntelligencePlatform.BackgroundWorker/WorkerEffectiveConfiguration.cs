using System;
using System.Globalization;
using SystemIntelligencePlatform.InstanceConfiguration;

namespace SystemIntelligencePlatform.BackgroundWorker;

internal static class WorkerEffectiveConfiguration
{
    public static RabbitMqWorkerOptions GetRabbitMq(
        IInstanceConfigurationProvider provider,
        RabbitMqWorkerOptions fileDefaults)
    {
        return new RabbitMqWorkerOptions
        {
            Host = provider.GetEffectiveSetting("RabbitMQ:Host") ?? fileDefaults.Host,
            Port = ParseInt(provider.GetEffectiveSetting("RabbitMQ:Port"), fileDefaults.Port),
            Username = provider.GetEffectiveSetting("RabbitMQ:Username") ?? fileDefaults.Username,
            Password = provider.GetEffectiveSetting("RabbitMQ:Password") ?? fileDefaults.Password,
            VirtualHost = provider.GetEffectiveSetting("RabbitMQ:VirtualHost") ?? fileDefaults.VirtualHost,
        };
    }

    private static int ParseInt(string? s, int fallback) =>
        int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : fallback;
}
