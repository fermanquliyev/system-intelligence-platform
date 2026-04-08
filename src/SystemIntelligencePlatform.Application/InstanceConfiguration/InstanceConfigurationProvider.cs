using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Threading;
using Volo.Abp.Uow;

namespace SystemIntelligencePlatform.InstanceConfiguration;

/// <summary>
/// Cached snapshot of DB features/settings. Database wins when a row exists and has a non-empty value;
/// otherwise <see cref="IConfiguration"/> (appsettings + environment variables) is used.
/// </summary>
public class InstanceConfigurationProvider : IInstanceConfigurationProvider, ISingletonDependency
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<InstanceConfigurationProvider> _logger;

    private long _version;
    private (long version, Dictionary<string, string> settings, Dictionary<string, bool> features, DateTime loadedAt) _snapshot;

    public InstanceConfigurationProvider(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<InstanceConfigurationProvider> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
        _snapshot = (0, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
            new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase), DateTime.MinValue);
    }

    public void RequestRefresh()
    {
        Interlocked.Increment(ref _version);
    }

    public bool IsFeatureEnabled(string featureName)
    {
        var snap = GetSnapshot();
        if (snap.features.TryGetValue(featureName, out var enabled))
            return enabled;
        // No row yet: default ON for safety (matches seeded defaults).
        return true;
    }

    public string? GetEffectiveSetting(string key)
    {
        var snap = GetSnapshot();
        if (snap.settings.TryGetValue(key, out var dbValue) && !string.IsNullOrWhiteSpace(dbValue))
            return dbValue;
        return _configuration[key];
    }

    private (Dictionary<string, string> settings, Dictionary<string, bool> features) LoadFromDatabase()
    {
        try
        {
            return AsyncHelper.RunSync(async () =>
            {
                using var scope = _scopeFactory.CreateScope();
                var uowManager = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
                using var uow = uowManager.Begin(requiresNew: true, isTransactional: false);
                var featureRepo = scope.ServiceProvider.GetRequiredService<IRepository<InstanceFeature, Guid>>();
                var settingRepo = scope.ServiceProvider.GetRequiredService<IRepository<InstanceSetting, Guid>>();
                var features = await featureRepo.GetListAsync();
                var settings = await settingRepo.GetListAsync();
                await uow.CompleteAsync();
                return (
                    settings.ToDictionary(s => s.Key, s => s.Value, StringComparer.OrdinalIgnoreCase),
                    features.ToDictionary(f => f.Name, f => f.IsEnabled, StringComparer.OrdinalIgnoreCase));
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not load instance configuration from database; using appsettings/env only.");
            return (new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase));
        }
    }

    private (long version, Dictionary<string, string> settings, Dictionary<string, bool> features, DateTime loadedAt) GetSnapshot()
    {
        var v = Interlocked.Read(ref _version);
        lock (this)
        {
            var age = DateTime.UtcNow - _snapshot.loadedAt;
            if (_snapshot.version == v && age < TimeSpan.FromSeconds(5))
                return _snapshot;

            var loaded = LoadFromDatabase();
            _snapshot = (v, loaded.settings, loaded.features, DateTime.UtcNow);
            return _snapshot;
        }
    }
}
