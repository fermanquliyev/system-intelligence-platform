using System;
using Azure;
using Azure.AI.TextAnalytics;
using Azure.Identity;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SystemIntelligencePlatform.EntityFrameworkCore;

var host = new HostBuilder()
    .ConfigureAppConfiguration((context, config) =>
    {
        // In production (Azure), set Azure:KeyVault:VaultUri; all secrets loaded via managed identity.
        var vaultUri = context.Configuration["Azure:KeyVault:VaultUri"];
        if (!string.IsNullOrEmpty(vaultUri))
        {
            config.AddAzureKeyVault(
                new Uri(vaultUri),
                new DefaultAzureCredential()); // Uses System-assigned Managed Identity in Azure
        }
    })
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context, services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();

        var config = context.Configuration;

        var connectionString = config.GetConnectionString("Default");
        if (!string.IsNullOrEmpty(connectionString))
        {
            services.AddDbContext<SystemIntelligencePlatformDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });
        }

        // Azure Language (same keys as API; values from Key Vault in production)
        var langEndpoint = config["Azure:Language:Endpoint"];
        var langKey = config["Azure:Language:Key"];
        if (!string.IsNullOrEmpty(langEndpoint) && !string.IsNullOrEmpty(langKey))
        {
            services.AddSingleton(new TextAnalyticsClient(
                new Uri(langEndpoint), new AzureKeyCredential(langKey)));
        }

        // Azure AI Search (same keys as API; values from Key Vault in production)
        var searchEndpoint = config["Azure:Search:Endpoint"];
        var searchKey = config["Azure:Search:Key"];
        var searchIndexName = config["Azure:Search:IndexName"] ?? "incidents-index";
        if (!string.IsNullOrEmpty(searchEndpoint) && !string.IsNullOrEmpty(searchKey))
        {
            var searchUri = new Uri(searchEndpoint);
            var credential = new AzureKeyCredential(searchKey);
            services.AddSingleton(new SearchClient(searchUri, searchIndexName, credential));
            services.AddSingleton(new SearchIndexClient(searchUri, credential));
        }
    })
    .Build();

host.Run();
