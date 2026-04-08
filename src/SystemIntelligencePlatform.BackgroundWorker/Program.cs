using SystemIntelligencePlatform.AI;
using SystemIntelligencePlatform.BackgroundWorker;
using SystemIntelligencePlatform.InstanceConfiguration;
using Microsoft.EntityFrameworkCore;
using SystemIntelligencePlatform.EntityFrameworkCore;
using SystemIntelligencePlatform.LogEvents;

// Allow DateTime.Kind=Local when writing to PostgreSQL timestamptz.
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("ConnectionStrings:Default is required.");

builder.Services.AddDbContext<SystemIntelligencePlatformDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

builder.Services.AddDbContextFactory<SystemIntelligencePlatformDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

builder.Services.AddSingleton<WorkerInstanceConfigurationProvider>();
builder.Services.AddSingleton<IInstanceConfigurationProvider>(sp =>
    sp.GetRequiredService<WorkerInstanceConfigurationProvider>());

builder.Services.Configure<RabbitMqWorkerOptions>(builder.Configuration.GetSection(RabbitMqWorkerOptions.SectionName));
builder.Services.AddSingleton<AnomalyDetectionService>();
builder.Services.AddLlmIncidentAiAnalyzer(builder.Configuration);
builder.Services.AddHostedService<LogIngestionConsumerService>();

var host = builder.Build();
await host.RunAsync();
