using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SystemIntelligencePlatform.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class SystemIntelligencePlatformDbContextFactory : IDesignTimeDbContextFactory<SystemIntelligencePlatformDbContext>
{
    public SystemIntelligencePlatformDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        
        SystemIntelligencePlatformEfCoreEntityExtensionMappings.Configure();

        var builder = new DbContextOptionsBuilder<SystemIntelligencePlatformDbContext>()
            .UseSqlServer(configuration.GetConnectionString("Default"));
        
        return new SystemIntelligencePlatformDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../SystemIntelligencePlatform.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables();

        return builder.Build();
    }
}
