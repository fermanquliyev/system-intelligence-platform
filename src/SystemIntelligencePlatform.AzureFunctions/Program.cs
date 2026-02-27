using Azure;
using Azure.AI.TextAnalytics;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SystemIntelligencePlatform.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Autofac;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context, services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        var config = context.Configuration;

        // EF Core for direct DB access in the function
        services.AddAbpDbContext<SystemIntelligencePlatformDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
        });

        // Azure Language
        var langEndpoint = config["AzureLanguage:Endpoint"];
        var langKey = config["AzureLanguage:Key"];
        if (!string.IsNullOrEmpty(langEndpoint) && !string.IsNullOrEmpty(langKey))
        {
            services.AddSingleton(new TextAnalyticsClient(
                new Uri(langEndpoint), new AzureKeyCredential(langKey)));
        }

        // Azure AI Search
        var searchEndpoint = config["AzureSearch:Endpoint"];
        var searchKey = config["AzureSearch:Key"];
        if (!string.IsNullOrEmpty(searchEndpoint) && !string.IsNullOrEmpty(searchKey))
        {
            var searchUri = new Uri(searchEndpoint);
            var credential = new AzureKeyCredential(searchKey);
            services.AddSingleton(new SearchClient(searchUri, "incidents-index", credential));
            services.AddSingleton(new SearchIndexClient(searchUri, credential));
        }
    })
    .Build();

host.Run();
