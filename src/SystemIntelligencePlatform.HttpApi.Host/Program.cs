using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;
using SystemIntelligencePlatform.Data;

namespace SystemIntelligencePlatform;

public class Program
{
    public async static Task<int> Main(string[] args)
    {
        // Allow DateTime.Kind=Local when writing to PostgreSQL timestamptz (ABP seed and others use local time).
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        Log.Logger = new LoggerConfiguration()
            .WriteTo.Async(c => c.File("Logs/logs.txt"))
            .WriteTo.Async(c => c.Console())
            .CreateBootstrapLogger();

        try
        {
            Log.Information("Starting SystemIntelligencePlatform.HttpApi.Host.");
            var builder = WebApplication.CreateBuilder(args);

            // OpenTelemetry: distributed tracing, metrics; default Console exporter (Prometheus can be added later)
            builder.Services.AddOpenTelemetry()
                .WithTracing(t =>
                {
                    t.AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddConsoleExporter();
                })
                .WithMetrics(m =>
                {
                    m.AddAspNetCoreInstrumentation()
                        .AddRuntimeInstrumentation()
                        .AddConsoleExporter();
                });

            builder.Host
                .AddAppSettingsSecretsJson()
                .UseAutofac()
                .UseSerilog((context, services, loggerConfiguration) =>
                {
                    loggerConfiguration
                        .ReadFrom.Configuration(context.Configuration)
                        .ReadFrom.Services(services)
                        .WriteTo.Async(c => c.AbpStudio(services));
                });
            await builder.AddApplicationAsync<SystemIntelligencePlatformHttpApiHostModule>();
            var app = builder.Build();

            await ApplyMigrationsAsync(app);

            await app.InitializeApplicationAsync();
            await app.RunAsync();
            return 0;
        }
        catch (Exception ex)
        {
            if (ex is HostAbortedException)
            {
                throw;
            }

            Log.Fatal(ex, "Host terminated unexpectedly!");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static async Task ApplyMigrationsAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var migrator = scope.ServiceProvider.GetRequiredService<SystemIntelligencePlatformDbMigrationService>();
        await migrator.MigrateAsync();
    }
}
