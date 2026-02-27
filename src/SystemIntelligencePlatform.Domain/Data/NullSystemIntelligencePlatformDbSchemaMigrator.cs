using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace SystemIntelligencePlatform.Data;

/* This is used if database provider does't define
 * ISystemIntelligencePlatformDbSchemaMigrator implementation.
 */
public class NullSystemIntelligencePlatformDbSchemaMigrator : ISystemIntelligencePlatformDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
