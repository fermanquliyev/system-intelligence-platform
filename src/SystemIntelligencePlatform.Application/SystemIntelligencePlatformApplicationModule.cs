using Microsoft.Extensions.DependencyInjection;
using SystemIntelligencePlatform.AI;
using Volo.Abp.Account;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Mapperly;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.TenantManagement;

namespace SystemIntelligencePlatform;

[DependsOn(
    typeof(SystemIntelligencePlatformDomainModule),
    typeof(SystemIntelligencePlatformApplicationContractsModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpAccountApplicationModule),
    typeof(AbpTenantManagementApplicationModule),
    typeof(AbpSettingManagementApplicationModule)
    )]
public class SystemIntelligencePlatformApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        context.Services.Configure<GoogleAiOptions>(configuration.GetSection(GoogleAiOptions.SectionName));
        context.Services.AddHttpClient("GoogleAi");
    }
}
