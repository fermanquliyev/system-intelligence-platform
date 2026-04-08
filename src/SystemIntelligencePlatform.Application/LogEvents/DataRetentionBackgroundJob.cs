using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SystemIntelligencePlatform.InstanceConfiguration;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;

namespace SystemIntelligencePlatform.LogEvents;

/// <summary>
/// Deletes log events older than <see cref="DataRetentionOptions.LogRetentionDays"/>.
/// </summary>
public class DataRetentionBackgroundJob : AsyncBackgroundJob<DataRetentionArgs>, ITransientDependency
{
    private readonly ILogEventRepository _logEventRepository;
    private readonly IOptions<DataRetentionOptions> _fileOptions;
    private readonly IInstanceConfigurationProvider _instanceConfiguration;

    public DataRetentionBackgroundJob(
        ILogEventRepository logEventRepository,
        IOptions<DataRetentionOptions> fileOptions,
        IInstanceConfigurationProvider instanceConfiguration)
    {
        _logEventRepository = logEventRepository;
        _fileOptions = fileOptions;
        _instanceConfiguration = instanceConfiguration;
    }

    public override async Task ExecuteAsync(DataRetentionArgs args)
    {
        if (!_instanceConfiguration.IsFeatureEnabled(InstanceConfigurationFeatures.DataRetentionJob))
            return;

        var retention = EffectiveConfigurationBinder.GetDataRetention(_instanceConfiguration, _fileOptions);
        var days = Math.Max(1, retention.LogRetentionDays);
        var cutoff = DateTime.UtcNow.AddDays(-days);

        const int batchSize = 1000;
        while (true)
        {
            var oldEvents = await _logEventRepository.GetOlderThanAsync(cutoff, batchSize);
            if (oldEvents.Count == 0) break;

            await _logEventRepository.DeleteBatchAsync(oldEvents.Select(e => e.Id).ToList());

            Logger.LogInformation(
                "Data retention: deleted {Count} log events older than {Cutoff} ({Days} day policy)",
                oldEvents.Count, cutoff, days);

            if (oldEvents.Count < batchSize) break;
        }
    }
}

public class DataRetentionArgs { }
