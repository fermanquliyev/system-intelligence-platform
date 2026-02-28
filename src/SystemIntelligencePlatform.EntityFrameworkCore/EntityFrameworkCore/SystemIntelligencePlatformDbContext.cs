using Microsoft.EntityFrameworkCore;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using SystemIntelligencePlatform.FailedLogEvents;
using SystemIntelligencePlatform.Incidents;
using SystemIntelligencePlatform.LogEvents;
using SystemIntelligencePlatform.MonitoredApplications;
using SystemIntelligencePlatform.Subscriptions;
using SystemIntelligencePlatform.Webhooks;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.BlobStoring.Database.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;

namespace SystemIntelligencePlatform.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ReplaceDbContext(typeof(ITenantManagementDbContext))]
[ConnectionStringName("Default")]
public class SystemIntelligencePlatformDbContext :
    AbpDbContext<SystemIntelligencePlatformDbContext>,
    ITenantManagementDbContext,
    IIdentityDbContext
{
    public DbSet<MonitoredApplication> MonitoredApplications { get; set; }
    public DbSet<LogEvent> LogEvents { get; set; }
    public DbSet<Incident> Incidents { get; set; }
    public DbSet<IncidentComment> IncidentComments { get; set; }
    public DbSet<FailedLogEvent> FailedLogEvents { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<MonthlyUsage> MonthlyUsages { get; set; }
    public DbSet<WebhookRegistration> WebhookRegistrations { get; set; }

    #region Entities from the modules

    // Identity
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<IdentitySession> Sessions { get; set; }

    // Tenant Management
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }

    #endregion

    public SystemIntelligencePlatformDbContext(DbContextOptions<SystemIntelligencePlatformDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureFeatureManagement();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureTenantManagement();
        builder.ConfigureBlobStoring();

        builder.Entity<MonitoredApplication>(b =>
        {
            b.ToTable(SystemIntelligencePlatformConsts.DbTablePrefix + "MonitoredApplications",
                SystemIntelligencePlatformConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Name).IsRequired().HasMaxLength(MonitoredApplicationConsts.MaxNameLength);
            b.Property(x => x.Description).HasMaxLength(MonitoredApplicationConsts.MaxDescriptionLength);
            b.Property(x => x.Environment).HasMaxLength(MonitoredApplicationConsts.MaxEnvironmentLength);
            b.Property(x => x.ApiKeyHash).IsRequired().HasMaxLength(MonitoredApplicationConsts.ApiKeyHashLength);

            b.HasIndex(x => x.ApiKeyHash).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.Name }).IsUnique();
            b.HasIndex(x => x.IsActive);
        });

        builder.Entity<LogEvent>(b =>
        {
            b.ToTable(SystemIntelligencePlatformConsts.DbTablePrefix + "LogEvents",
                SystemIntelligencePlatformConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Message).IsRequired().HasMaxLength(LogEventConsts.MaxMessageLength);
            b.Property(x => x.Source).HasMaxLength(LogEventConsts.MaxSourceLength);
            b.Property(x => x.HashSignature).IsRequired().HasMaxLength(LogEventConsts.MaxHashSignatureLength);
            b.Property(x => x.ExceptionType).HasMaxLength(LogEventConsts.MaxExceptionTypeLength);
            b.Property(x => x.StackTrace).HasMaxLength(LogEventConsts.MaxStackTraceLength);
            b.Property(x => x.CorrelationId).HasMaxLength(LogEventConsts.MaxCorrelationIdLength);

            b.HasIndex(x => new { x.ApplicationId, x.HashSignature, x.Timestamp });
            b.HasIndex(x => x.Timestamp);
            b.HasIndex(x => x.IncidentId);
            b.HasIndex(x => x.TenantId);
        });

        builder.Entity<Incident>(b =>
        {
            b.ToTable(SystemIntelligencePlatformConsts.DbTablePrefix + "Incidents",
                SystemIntelligencePlatformConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Title).IsRequired().HasMaxLength(IncidentConsts.MaxTitleLength);
            b.Property(x => x.Description).HasMaxLength(IncidentConsts.MaxDescriptionLength);
            b.Property(x => x.HashSignature).IsRequired().HasMaxLength(IncidentConsts.MaxHashSignatureLength);
            b.Property(x => x.KeyPhrases).HasMaxLength(IncidentConsts.MaxKeyPhrasesLength);
            b.Property(x => x.Entities).HasMaxLength(IncidentConsts.MaxEntitiesLength);
            b.Property(x => x.RootCauseSummary).HasMaxLength(4000);
            b.Property(x => x.SuggestedFix).HasMaxLength(4000);
            b.Property(x => x.SeverityJustification).HasMaxLength(2000);

            b.HasIndex(x => new { x.ApplicationId, x.HashSignature }).IsUnique();
            b.HasIndex(x => x.Status);
            b.HasIndex(x => x.Severity);
            b.HasIndex(x => x.LastOccurrence);
            b.HasIndex(x => x.TenantId);

            b.HasMany(x => x.Comments).WithOne().HasForeignKey(x => x.IncidentId);
        });

        builder.Entity<IncidentComment>(b =>
        {
            b.ToTable(SystemIntelligencePlatformConsts.DbTablePrefix + "IncidentComments",
                SystemIntelligencePlatformConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Content).IsRequired().HasMaxLength(2000);

            b.HasIndex(x => x.IncidentId);
        });

        builder.Entity<FailedLogEvent>(b =>
        {
            b.ToTable(SystemIntelligencePlatformConsts.DbTablePrefix + "FailedLogEvents",
                SystemIntelligencePlatformConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.OriginalMessageBody).IsRequired();
            b.Property(x => x.ErrorMessage).IsRequired().HasMaxLength(FailedLogEventConsts.MaxErrorMessageLength);
            b.Property(x => x.StackTrace).HasMaxLength(FailedLogEventConsts.MaxStackTraceLength);
            b.Property(x => x.CorrelationId).HasMaxLength(64);

            b.HasIndex(x => x.TenantId);
            b.HasIndex(x => x.CreationTime);
        });

        builder.Entity<Subscription>(b =>
        {
            b.ToTable(SystemIntelligencePlatformConsts.DbTablePrefix + "Subscriptions",
                SystemIntelligencePlatformConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.StripeCustomerId).HasMaxLength(SubscriptionConsts.MaxStripeCustomerIdLength);
            b.Property(x => x.StripeSubscriptionId).HasMaxLength(SubscriptionConsts.MaxStripeSubscriptionIdLength);

            b.HasIndex(x => x.TenantId).IsUnique();
            b.HasIndex(x => x.StripeSubscriptionId).IsUnique().HasFilter("[StripeSubscriptionId] IS NOT NULL");
        });

        builder.Entity<MonthlyUsage>(b =>
        {
            b.ToTable(SystemIntelligencePlatformConsts.DbTablePrefix + "MonthlyUsages",
                SystemIntelligencePlatformConsts.DbSchema);
            b.ConfigureByConvention();

            b.HasIndex(x => new { x.TenantId, x.Month }).IsUnique();
        });

        builder.Entity<WebhookRegistration>(b =>
        {
            b.ToTable(SystemIntelligencePlatformConsts.DbTablePrefix + "WebhookRegistrations",
                SystemIntelligencePlatformConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Url).IsRequired().HasMaxLength(WebhookConsts.MaxUrlLength);
            b.Property(x => x.Secret).HasMaxLength(WebhookConsts.MaxSecretLength);

            b.HasIndex(x => x.TenantId);
        });
    }
}
