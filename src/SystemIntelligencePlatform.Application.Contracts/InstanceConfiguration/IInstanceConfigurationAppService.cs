using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SystemIntelligencePlatform.InstanceConfiguration;

public interface IInstanceConfigurationAppService : IApplicationService
{
    /// <summary>GET /api/app/instance-configuration/snapshot (parameterless GetAsync is not mapped reliably by conventional controllers).</summary>
    Task<InstanceConfigurationSnapshotDto> GetSnapshotAsync();

    Task UpdateFeaturesAsync(UpdateInstanceFeaturesDto input);

    Task UpdateSettingsAsync(UpdateInstanceSettingsDto input);

    Task<ApplyMigrationsResultDto> ApplyMigrationsAsync();
}
