using Microsoft.EntityFrameworkCore;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using SystemIntelligencePlatform.Alerting;
using SystemIntelligencePlatform.Copilot;
using SystemIntelligencePlatform.FailedLogEvents;
using SystemIntelligencePlatform.Incidents;
using SystemIntelligencePlatform.LogEvents;
using SystemIntelligencePlatform.LogSearch;
using SystemIntelligencePlatform.LogSources;
using SystemIntelligencePlatform.Metrics;
using SystemIntelligencePlatform.MonitoredApplications;
using SystemIntelligencePlatform.Playbooks;
using SystemIntelligencePlatform.Webhooks;
using SystemIntelligencePlatform.InstanceConfiguration;
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

namespace SystemIntelligencePlatform.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ConnectionStringName("Default")]
public class SystemIntelligencePlatformDbContext :
    AbpDbContext<SystemIntelligencePlatformDbContext>,
    IIdentityDbContext
{
    public DbSet<MonitoredApplication> MonitoredApplications { get; set; }
    public DbSet<LogEvent> LogEvents { get; set; }
    public DbSet<Incident> Incidents { get; set; }
    public DbSet<IncidentComment> IncidentComments { get; set; }
    public DbSet<FailedLogEvent> FailedLogEvents { get; set; }
    public DbSet<WebhookRegistration> WebhookRegistrations { get; set; }
    public DbSet<InstanceFeature> InstanceFeatures { get; set; }
    public DbSet<InstanceSetting> InstanceSettings { get; set; }

    public DbSet<AlertRule> AlertRules { get; set; }
    public DbSet<AlertHistory> AlertHistories { get; set; }
    public DbSet<MetricSample> MetricSamples { get; set; }
    public DbSet<SavedLogSearch> SavedLogSearches { get; set; }
    public DbSet<CopilotConversationMessage> CopilotConversationMessages { get; set; }
    public DbSet<MergedIncidentLink> MergedIncidentLinks { get; set; }
    public DbSet<Playbook> Playbooks { get; set; }
    public DbSet<PlaybookStep> PlaybookSteps { get; set; }
    public DbSet<PlaybookRun> PlaybookRuns { get; set; }
    public DbSet<PlaybookRunStep> PlaybookRunSteps { get; set; }
    public DbSet<LogSourceConfiguration> LogSourceConfigurations { get; set; }
    public DbSet<LogCluster> LogClusters { get; set; }

    #region Entities from the modules

    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<IdentitySession> Sessions { get; set; }

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
            b.HasIndex(x => x.Name).IsUnique();
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
            b.HasIndex(x => x.ClusterId);
            b.HasIndex(x => x.ContainsPii);
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
            b.HasIndex(x => x.AssignedUserId);
            b.HasIndex(x => x.MergedIntoIncidentId);
            b.HasIndex(x => x.ContainsPii);

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

            b.HasIndex(x => x.CreationTime);
        });

        builder.Entity<WebhookRegistration>(b =>
        {
            b.ToTable(SystemIntelligencePlatformConsts.DbTablePrefix + "WebhookRegistrations",
                SystemIntelligencePlatformConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Url).IsRequired().HasMaxLength(WebhookConsts.MaxUrlLength);
            b.Property(x => x.Secret).HasMaxLength(WebhookConsts.MaxSecretLength);
        });

        builder.Entity<InstanceFeature>(b =>
        {
            b.ToTable(SystemIntelligencePlatformConsts.DbTablePrefix + "InstanceFeatures",
                SystemIntelligencePlatformConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.DisplayName).IsRequired().HasMaxLength(256);
            b.Property(x => x.Description).HasMaxLength(2000);
            b.HasIndex(x => x.Name).IsUnique();
        });

        builder.Entity<InstanceSetting>(b =>
        {
            b.ToTable(SystemIntelligencePlatformConsts.DbTablePrefix + "InstanceSettings",
                SystemIntelligencePlatformConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Key).IsRequired().HasMaxLength(256);
            b.Property(x => x.Value).HasMaxLength(8000);
            b.HasIndex(x => x.Key).IsUnique();
        });

        builder.Entity<AlertRule>(b =>
        {
            b.ToTable(SystemIntelligencePlatformConsts.DbTablePrefix + "AlertRules", SystemIntelligencePlatformConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(256);
            b.Property(x => x.DefinitionJson).IsRequired();
            b.HasIndex(x => x.ApplicationId);
            b.HasIndex(x => x.IsEnabled);
        });

        builder.Entity<AlertHistory>(b =>
        {
            b.ToTable(SystemIntelligencePlatformConsts.DbTablePrefix + "AlertHistories", SystemIntelligencePlatformConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.PayloadJson).IsRequired();
            b.HasIndex(x => x.AlertRuleId);
            b.HasIndex(x => x.FiredAt);
            b.HasIndex(x => x.ApplicationId);
        });

        builder.Entity<MetricSample>(b =>
        {
            b.ToTable(SystemIntelligencePlatformConsts.DbTablePrefix + "MetricSamples", SystemIntelligencePlatformConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.TagsJson).HasMaxLength(2000);
            b.HasIndex(x => new { x.ApplicationId, x.Name, x.Timestamp });
            b.HasIndex(x => x.Timestamp);
        });

        builder.Entity<SavedLogSearch>(b =>
        {
            b.ToTable(SystemIntelligencePlatformConsts.DbTablePrefix + "SavedLogSearches", SystemIntelligencePlatformConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.FilterJson).IsRequired();
            b.HasIndex(x => x.UserId);
        });

        builder.Entity<CopilotConversationMessage>(b =>
        {
            b.ToTable(SystemIntelligencePlatformConsts.DbTablePrefix + "CopilotConversationMessages", SystemIntelligencePlatformConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Content).IsRequired().HasMaxLength(16000);
            b.HasIndex(x => x.IncidentId);
        });

        builder.Entity<MergedIncidentLink>(b =>
        {
            b.ToTable(SystemIntelligencePlatformConsts.DbTablePrefix + "MergedIncidentLinks", SystemIntelligencePlatformConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => x.CanonicalIncidentId);
            b.HasIndex(x => x.MergedIncidentId).IsUnique();
        });

        builder.Entity<Playbook>(b =>
        {
            b.ToTable(SystemIntelligencePlatformConsts.DbTablePrefix + "Playbooks", SystemIntelligencePlatformConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(256);
            b.Property(x => x.Description).HasMaxLength(2000);
            b.Property(x => x.TriggerDefinitionJson).IsRequired();
            b.HasMany(x => x.Steps).WithOne().HasForeignKey(x => x.PlaybookId).IsRequired();
        });

        builder.Entity<PlaybookStep>(b =>
        {
            b.ToTable(SystemIntelligencePlatformConsts.DbTablePrefix + "PlaybookSteps", SystemIntelligencePlatformConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Title).IsRequired().HasMaxLength(512);
            b.Property(x => x.Body).HasMaxLength(4000);
            b.HasIndex(x => x.PlaybookId);
        });

        builder.Entity<PlaybookRun>(b =>
        {
            b.ToTable(SystemIntelligencePlatformConsts.DbTablePrefix + "PlaybookRuns", SystemIntelligencePlatformConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasMany(x => x.RunSteps).WithOne().HasForeignKey(x => x.PlaybookRunId).IsRequired();
            b.HasIndex(x => x.IncidentId);
            b.HasIndex(x => x.PlaybookId);
        });

        builder.Entity<PlaybookRunStep>(b =>
        {
            b.ToTable(SystemIntelligencePlatformConsts.DbTablePrefix + "PlaybookRunSteps", SystemIntelligencePlatformConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Title).IsRequired().HasMaxLength(512);
            b.HasIndex(x => x.PlaybookRunId);
        });

        builder.Entity<LogSourceConfiguration>(b =>
        {
            b.ToTable(SystemIntelligencePlatformConsts.DbTablePrefix + "LogSourceConfigurations", SystemIntelligencePlatformConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(256);
            b.Property(x => x.SettingsJson).IsRequired();
            b.HasIndex(x => x.IsEnabled);
        });

        builder.Entity<LogCluster>(b =>
        {
            b.ToTable(SystemIntelligencePlatformConsts.DbTablePrefix + "LogClusters", SystemIntelligencePlatformConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.SignatureHash).IsRequired().HasMaxLength(128);
            b.Property(x => x.Summary).IsRequired().HasMaxLength(2000);
            b.HasIndex(x => x.ApplicationId);
            b.HasIndex(x => x.SignatureHash);
        });
    }
}
