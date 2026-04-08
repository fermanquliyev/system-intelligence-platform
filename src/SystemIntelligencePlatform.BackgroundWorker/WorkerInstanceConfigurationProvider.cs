using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SystemIntelligencePlatform.EntityFrameworkCore;
using SystemIntelligencePlatform.InstanceConfiguration;

namespace SystemIntelligencePlatform.BackgroundWorker;

/// <summary>
/// Loads <see cref="InstanceFeature"/> / <see cref="InstanceSetting"/> from the database for the worker process
/// (same merge rules as the API: DB value wins when non-empty, else configuration / env).
/// </summary>
public class WorkerInstanceConfigurationProvider : IInstanceConfigurationProvider
{
    private readonly IDbContextFactory<SystemIntelligencePlatformDbContext> _dbContextFactory;
    private readonly IConfiguration _configuration;
    private long _version;
    private (long version, Dictionary<string, string> settings, Dictionary<string, bool> features, DateTime loadedAt) _snapshot;

    public WorkerInstanceConfigurationProvider(
        IDbContextFactory<SystemIntelligencePlatformDbContext> dbContextFactory,
        IConfiguration configuration)
    {
        _dbContextFactory = dbContextFactory;
        _configuration = configuration;
        _snapshot = (0,
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
            new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase),
            DateTime.MinValue);
    }

    public void RequestRefresh()
    {
        Interlocked.Increment(ref _version);
    }

    public async Task EnsureSnapshotAsync(CancellationToken cancellationToken = default)
    {
        var v = Interlocked.Read(ref _version);
        lock (this)
        {
            if (_snapshot.version == v && DateTime.UtcNow - _snapshot.loadedAt < TimeSpan.FromSeconds(60))
                return;
        }

        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var settings = await db.InstanceSettings.AsNoTracking()
            .ToDictionaryAsync(s => s.Key, s => s.Value, StringComparer.OrdinalIgnoreCase, cancellationToken);
        var features = await db.InstanceFeatures.AsNoTracking()
            .ToDictionaryAsync(f => f.Name, f => f.IsEnabled, StringComparer.OrdinalIgnoreCase, cancellationToken);

        lock (this)
        {
            _snapshot = (v, settings, features, DateTime.UtcNow);
        }
    }

    public bool IsFeatureEnabled(string featureName)
    {
        lock (this)
        {
            if (_snapshot.features.TryGetValue(featureName, out var enabled))
                return enabled;
        }

        return true;
    }

    public string? GetEffectiveSetting(string key)
    {
        lock (this)
        {
            if (_snapshot.settings.TryGetValue(key, out var dbValue) && !string.IsNullOrWhiteSpace(dbValue))
                return dbValue;
        }

        return _configuration[key];
    }
}
