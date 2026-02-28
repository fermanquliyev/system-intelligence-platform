using SystemIntelligencePlatform.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace SystemIntelligencePlatform.Permissions;

public class SystemIntelligencePlatformPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(SystemIntelligencePlatformPermissions.GroupName);

        myGroup.AddPermission(SystemIntelligencePlatformPermissions.DashboardPermissions.Default, L("Permission:Dashboard"));

        var appsPermission = myGroup.AddPermission(SystemIntelligencePlatformPermissions.Applications.Default, L("Permission:Applications"));
        appsPermission.AddChild(SystemIntelligencePlatformPermissions.Applications.Create, L("Permission:Applications.Create"));
        appsPermission.AddChild(SystemIntelligencePlatformPermissions.Applications.Edit, L("Permission:Applications.Edit"));
        appsPermission.AddChild(SystemIntelligencePlatformPermissions.Applications.Delete, L("Permission:Applications.Delete"));
        appsPermission.AddChild(SystemIntelligencePlatformPermissions.Applications.RegenerateApiKey, L("Permission:Applications.RegenerateApiKey"));

        var incidentsPermission = myGroup.AddPermission(SystemIntelligencePlatformPermissions.Incidents.Default, L("Permission:Incidents"));
        incidentsPermission.AddChild(SystemIntelligencePlatformPermissions.Incidents.Update, L("Permission:Incidents.Update"));
        incidentsPermission.AddChild(SystemIntelligencePlatformPermissions.Incidents.Resolve, L("Permission:Incidents.Resolve"));
        incidentsPermission.AddChild(SystemIntelligencePlatformPermissions.Incidents.Comment, L("Permission:Incidents.Comment"));
        incidentsPermission.AddChild(SystemIntelligencePlatformPermissions.Incidents.Search, L("Permission:Incidents.Search"));

        myGroup.AddPermission(SystemIntelligencePlatformPermissions.LogEvents.Default, L("Permission:LogEvents"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<SystemIntelligencePlatformResource>(name);
    }
}
