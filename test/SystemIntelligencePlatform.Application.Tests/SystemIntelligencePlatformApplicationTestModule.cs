using Volo.Abp.Modularity;

namespace SystemIntelligencePlatform;

[DependsOn(
    typeof(SystemIntelligencePlatformApplicationModule),
    typeof(SystemIntelligencePlatformDomainTestModule)
)]
public class SystemIntelligencePlatformApplicationTestModule : AbpModule
{

}
