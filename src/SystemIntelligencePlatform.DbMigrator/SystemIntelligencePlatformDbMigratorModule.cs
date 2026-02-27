using SystemIntelligencePlatform.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace SystemIntelligencePlatform.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(SystemIntelligencePlatformEntityFrameworkCoreModule),
    typeof(SystemIntelligencePlatformApplicationContractsModule)
)]
public class SystemIntelligencePlatformDbMigratorModule : AbpModule
{
}
