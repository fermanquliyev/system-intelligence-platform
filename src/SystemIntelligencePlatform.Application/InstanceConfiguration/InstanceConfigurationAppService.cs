using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SystemIntelligencePlatform.Data;
using SystemIntelligencePlatform.Permissions;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SystemIntelligencePlatform.InstanceConfiguration;

[Authorize(SystemIntelligencePlatformPermissions.InstanceConfiguration.Default)]
public class InstanceConfigurationAppService : ApplicationService, IInstanceConfigurationAppService
{
    private readonly IRepository<InstanceFeature, Guid> _featureRepository;
    private readonly IRepository<InstanceSetting, Guid> _settingRepository;
    private readonly IConfiguration _configuration;
    private readonly IInstanceConfigurationProvider _instanceConfigurationProvider;
    private readonly IEnumerable<ISystemIntelligencePlatformDbSchemaMigrator> _migrators;

    public InstanceConfigurationAppService(
        IRepository<InstanceFeature, Guid> featureRepository,
        IRepository<InstanceSetting, Guid> settingRepository,
        IConfiguration configuration,
        IInstanceConfigurationProvider instanceConfigurationProvider,
        IEnumerable<ISystemIntelligencePlatformDbSchemaMigrator> migrators)
    {
        _featureRepository = featureRepository;
        _settingRepository = settingRepository;
        _configuration = configuration;
        _instanceConfigurationProvider = instanceConfigurationProvider;
        _migrators = migrators;
    }

    public async Task<InstanceConfigurationSnapshotDto> GetSnapshotAsync()
    {
        var features = await _featureRepository.GetListAsync();
        var settings = await _settingRepository.GetListAsync();
        var settingDict = settings.ToDictionary(s => s.Key, s => s, StringComparer.OrdinalIgnoreCase);

        var featureDtos = features
            .OrderBy(f => f.DisplayOrder)
            .ThenBy(f => f.DisplayName)
            .Select(f => new InstanceFeatureStateDto
            {
                Id = f.Id,
                Name = f.Name,
                DisplayName = f.DisplayName,
                Description = f.Description,
                IsEnabled = f.IsEnabled,
                DisplayOrder = f.DisplayOrder
            })
            .ToList();

        var settingDtos = new List<InstanceSettingStateDto>();
        foreach (var def in InstanceConfigurationRegistry.SettingDefinitions)
        {
            settingDict.TryGetValue(def.Key, out var row);
            var fromDb = row?.Value;
            var fromFile = _configuration[def.Key];
            var effective = !string.IsNullOrWhiteSpace(fromDb) ? fromDb : fromFile;
            var overridden = !string.IsNullOrWhiteSpace(fromDb);

            settingDtos.Add(new InstanceSettingStateDto
            {
                Key = def.Key,
                DisplayName = def.DisplayName,
                Description = def.Description,
                Category = def.Category,
                IsSecret = def.IsSecret,
                EffectiveDisplayValue = FormatDisplayValue(effective, def.IsSecret),
                IsOverriddenInDatabase = overridden
            });
        }

        return new InstanceConfigurationSnapshotDto
        {
            Features = featureDtos,
            Settings = settingDtos.OrderBy(s => s.Category).ThenBy(s => s.DisplayName).ToList()
        };
    }

    public async Task UpdateFeaturesAsync(UpdateInstanceFeaturesDto input)
    {
        var all = await _featureRepository.GetListAsync();
        foreach (var (name, enabled) in input.Features)
        {
            var feature = all.FirstOrDefault(f => string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase));
            if (feature != null)
            {
                feature.IsEnabled = enabled;
                await _featureRepository.UpdateAsync(feature);
            }
        }

        _instanceConfigurationProvider.RequestRefresh();
    }

    public async Task UpdateSettingsAsync(UpdateInstanceSettingsDto input)
    {
        var existingRows = await _settingRepository.GetListAsync();
        foreach (var (key, value) in input.Values)
        {
            var def = InstanceConfigurationRegistry.FindSetting(key);
            if (def == null)
                continue;

            var existing = existingRows.FirstOrDefault(s => string.Equals(s.Key, key, StringComparison.OrdinalIgnoreCase));
            if (def.IsSecret && string.IsNullOrWhiteSpace(value))
            {
                if (existing == null)
                    continue;
                // keep stored secret
                continue;
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                if (existing != null)
                    await _settingRepository.DeleteAsync(existing);
                continue;
            }

            if (existing == null)
            {
                await _settingRepository.InsertAsync(new InstanceSetting(
                    GuidGenerator.Create(),
                    key,
                    value.Trim(),
                    def.IsSecret));
            }
            else
            {
                existing.Value = value.Trim();
                existing.IsSecret = def.IsSecret;
                await _settingRepository.UpdateAsync(existing);
            }
        }

        _instanceConfigurationProvider.RequestRefresh();
    }

    [Authorize(SystemIntelligencePlatformPermissions.InstanceConfiguration.Migrate)]
    public async Task<ApplyMigrationsResultDto> ApplyMigrationsAsync()
    {
        try
        {
            foreach (var migrator in _migrators)
            {
                await migrator.MigrateAsync();
            }

            return new ApplyMigrationsResultDto { Success = true, Message = "Migrations completed." };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Apply migrations failed");
            return new ApplyMigrationsResultDto { Success = false, Message = ex.Message };
        }
    }

    private static string FormatDisplayValue(string? value, bool isSecret)
    {
        if (string.IsNullOrEmpty(value))
            return "";
        return isSecret ? "••••••••" : value;
    }
}
