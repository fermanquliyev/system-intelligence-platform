using Volo.Abp.Modularity;

namespace SystemIntelligencePlatform;

/* Inherit from this class for your domain layer tests. */
public abstract class SystemIntelligencePlatformDomainTestBase<TStartupModule> : SystemIntelligencePlatformTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
