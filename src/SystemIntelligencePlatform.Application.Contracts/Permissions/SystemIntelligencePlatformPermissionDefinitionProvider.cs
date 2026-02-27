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

        var booksPermission = myGroup.AddPermission(SystemIntelligencePlatformPermissions.Books.Default, L("Permission:Books"));
        booksPermission.AddChild(SystemIntelligencePlatformPermissions.Books.Create, L("Permission:Books.Create"));
        booksPermission.AddChild(SystemIntelligencePlatformPermissions.Books.Edit, L("Permission:Books.Edit"));
        booksPermission.AddChild(SystemIntelligencePlatformPermissions.Books.Delete, L("Permission:Books.Delete"));
        //Define your own permissions here. Example:
        //myGroup.AddPermission(SystemIntelligencePlatformPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<SystemIntelligencePlatformResource>(name);
    }
}
