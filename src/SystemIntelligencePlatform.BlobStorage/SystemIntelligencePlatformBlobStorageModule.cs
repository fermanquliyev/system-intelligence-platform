using Microsoft.Extensions.DependencyInjection;
using SystemIntelligencePlatform;
using SystemIntelligencePlatform.LogEvents;
using Volo.Abp.Modularity;

namespace SystemIntelligencePlatform.BlobStorage;

[DependsOn(typeof(SystemIntelligencePlatformDomainModule))]
public class SystemIntelligencePlatformBlobStorageModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        context.Services.Configure<MinioBlobStorageOptions>(
            configuration.GetSection(MinioBlobStorageOptions.SectionName));

        context.Services.AddTransient<IBlobStorageService, MinioBlobStorageService>();
    }
}
