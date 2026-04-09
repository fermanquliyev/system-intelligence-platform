using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;
using Volo.Abp.PermissionManagement;
using SystemIntelligencePlatform.Permissions;

namespace SystemIntelligencePlatform.InstanceConfiguration;

/// <summary>Grants instance configuration permissions to the admin role when present.</summary>
public class InstanceConfigurationAdminPermissionDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IPermissionManager _permissionManager;
    private readonly IIdentityRoleRepository _roleRepository;

    public InstanceConfigurationAdminPermissionDataSeedContributor(
        IPermissionManager permissionManager,
        IIdentityRoleRepository roleRepository)
    {
        _permissionManager = permissionManager;
        _roleRepository = roleRepository;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        var roles = await _roleRepository.GetListAsync();
        var adminRole = roles.FirstOrDefault(r =>
            string.Equals(r.Name, "admin", System.StringComparison.OrdinalIgnoreCase));
        if (adminRole == null)
            return;

        await _permissionManager.SetForRoleAsync(adminRole.Name,
            SystemIntelligencePlatformPermissions.InstanceConfiguration.Default, true);
        await _permissionManager.SetForRoleAsync(adminRole.Name,
            SystemIntelligencePlatformPermissions.InstanceConfiguration.Migrate, true);

        foreach (var permission in new[]
                 {
                     SystemIntelligencePlatformPermissions.Incidents.Assign,
                     SystemIntelligencePlatformPermissions.Incidents.Copilot,
                     SystemIntelligencePlatformPermissions.Incidents.Timeline,
                     SystemIntelligencePlatformPermissions.LogEvents.Default,
                     SystemIntelligencePlatformPermissions.LogEvents.Search,
                     SystemIntelligencePlatformPermissions.LogEvents.ViewUnmasked,
                     SystemIntelligencePlatformPermissions.AlertRules.Default,
                     SystemIntelligencePlatformPermissions.AlertRules.Manage,
                     SystemIntelligencePlatformPermissions.Metrics.Default,
                     SystemIntelligencePlatformPermissions.Metrics.Ingest,
                     SystemIntelligencePlatformPermissions.Playbooks.Default,
                     SystemIntelligencePlatformPermissions.Playbooks.Manage,
                     SystemIntelligencePlatformPermissions.Playbooks.Run,
                     SystemIntelligencePlatformPermissions.LogSources.Default,
                     SystemIntelligencePlatformPermissions.LogSources.Manage
                 })
        {
            await _permissionManager.SetForRoleAsync(adminRole.Name, permission, true);
        }
    }
}
