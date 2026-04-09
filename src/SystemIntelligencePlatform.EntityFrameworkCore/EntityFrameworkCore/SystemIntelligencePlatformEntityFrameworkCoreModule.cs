using System;
using Microsoft.Extensions.DependencyInjection;
using SystemIntelligencePlatform.Incidents;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.BlobStoring.Database.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.PostgreSql;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.Studio;
namespace SystemIntelligencePlatform.EntityFrameworkCore;

[DependsOn(
    typeof(SystemIntelligencePlatformDomainModule),
    typeof(AbpPermissionManagementEntityFrameworkCoreModule),
    typeof(AbpSettingManagementEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCorePostgreSqlModule),
    typeof(AbpBackgroundJobsEntityFrameworkCoreModule),
    typeof(AbpAuditLoggingEntityFrameworkCoreModule),
    typeof(AbpFeatureManagementEntityFrameworkCoreModule),
    typeof(AbpIdentityEntityFrameworkCoreModule),
    typeof(AbpOpenIddictEntityFrameworkCoreModule),
    typeof(BlobStoringDatabaseEntityFrameworkCoreModule)
    )]
public class SystemIntelligencePlatformEntityFrameworkCoreModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        // Npgsql 6+ requires UTC for timestamptz; ABP/OpenIddict may use Local. Allow legacy behavior so writes succeed.
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        SystemIntelligencePlatformEfCoreEntityExtensionMappings.Configure();
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<SystemIntelligencePlatformDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
            options.AddRepository<MonitoredApplications.MonitoredApplication, Repositories.EfCoreMonitoredApplicationRepository>();
            options.AddRepository<global::SystemIntelligencePlatform.LogEvents.LogEvent, Repositories.EfCoreLogEventRepository>();
            options.AddRepository<Incident, Repositories.EfCoreIncidentRepository>();
        });

        if (AbpStudioAnalyzeHelper.IsInAnalyzeMode)
        {
            return;
        }

        Configure<AbpDbContextOptions>(options =>
        {
            /* The main point to change your DBMS.
             * See also SystemIntelligencePlatformDbContextFactory for EF Core tooling. */

            options.UseNpgsql();

        });
        
    }
}
