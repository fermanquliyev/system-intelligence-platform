using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SystemIntelligencePlatform.Incidents;

namespace SystemIntelligencePlatform.AI;

/// <summary>
/// Registers LLM-based incident AI analyzer and fallback for use outside ABP (e.g. BackgroundWorker).
/// When using the Application module (ABP), options and HttpClient are configured there; analyzers are auto-registered.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLlmIncidentAiAnalyzer(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GoogleAiOptions>(configuration.GetSection(GoogleAiOptions.SectionName));
        services.AddHttpClient("GoogleAi");
        services.AddMemoryCache();
        services.AddTransient<LocalIncidentAiAnalyzer>();
        services.AddTransient<IIncidentAiAnalyzer, LlmIncidentAiAnalyzer>();
        return services;
    }
}
