using Volo.Abp.Modularity;

namespace SystemIntelligencePlatform;

[DependsOn(
    typeof(SystemIntelligencePlatformDomainModule),
    typeof(SystemIntelligencePlatformTestBaseModule)
)]
public class SystemIntelligencePlatformDomainTestModule : AbpModule
{

}
