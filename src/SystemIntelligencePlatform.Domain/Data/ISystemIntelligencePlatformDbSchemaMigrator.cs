using System.Threading.Tasks;

namespace SystemIntelligencePlatform.Data;

public interface ISystemIntelligencePlatformDbSchemaMigrator
{
    Task MigrateAsync();
}
