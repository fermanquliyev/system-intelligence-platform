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
        incidentsPermission.AddChild(SystemIntelligencePlatformPermissions.Incidents.Assign, L("Permission:Incidents.Assign"));
        incidentsPermission.AddChild(SystemIntelligencePlatformPermissions.Incidents.Copilot, L("Permission:Incidents.Copilot"));
        incidentsPermission.AddChild(SystemIntelligencePlatformPermissions.Incidents.Timeline, L("Permission:Incidents.Timeline"));

        var logEvents = myGroup.AddPermission(SystemIntelligencePlatformPermissions.LogEvents.Default, L("Permission:LogEvents"));
        logEvents.AddChild(SystemIntelligencePlatformPermissions.LogEvents.Search, L("Permission:LogEvents.Search"));
        logEvents.AddChild(SystemIntelligencePlatformPermissions.LogEvents.ViewUnmasked, L("Permission:LogEvents.ViewUnmasked"));

        var alertRules = myGroup.AddPermission(SystemIntelligencePlatformPermissions.AlertRules.Default, L("Permission:AlertRules"));
        alertRules.AddChild(SystemIntelligencePlatformPermissions.AlertRules.Manage, L("Permission:AlertRules.Manage"));

        var metrics = myGroup.AddPermission(SystemIntelligencePlatformPermissions.Metrics.Default, L("Permission:Metrics"));
        metrics.AddChild(SystemIntelligencePlatformPermissions.Metrics.Ingest, L("Permission:Metrics.Ingest"));

        var playbooks = myGroup.AddPermission(SystemIntelligencePlatformPermissions.Playbooks.Default, L("Permission:Playbooks"));
        playbooks.AddChild(SystemIntelligencePlatformPermissions.Playbooks.Manage, L("Permission:Playbooks.Manage"));
        playbooks.AddChild(SystemIntelligencePlatformPermissions.Playbooks.Run, L("Permission:Playbooks.Run"));

        var logSources = myGroup.AddPermission(SystemIntelligencePlatformPermissions.LogSources.Default, L("Permission:LogSources"));
        logSources.AddChild(SystemIntelligencePlatformPermissions.LogSources.Manage, L("Permission:LogSources.Manage"));

        var instanceCfg = myGroup.AddPermission(SystemIntelligencePlatformPermissions.InstanceConfiguration.Default, L("Permission:InstanceConfiguration"));
        instanceCfg.AddChild(SystemIntelligencePlatformPermissions.InstanceConfiguration.Migrate, L("Permission:InstanceConfiguration.Migrate"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<SystemIntelligencePlatformResource>(name);
    }
}
