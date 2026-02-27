using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SystemIntelligencePlatform.Data;
using Volo.Abp.DependencyInjection;

namespace SystemIntelligencePlatform.EntityFrameworkCore;

public class EntityFrameworkCoreSystemIntelligencePlatformDbSchemaMigrator
    : ISystemIntelligencePlatformDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreSystemIntelligencePlatformDbSchemaMigrator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolving the SystemIntelligencePlatformDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<SystemIntelligencePlatformDbContext>()
            .Database
            .MigrateAsync();
    }
}
