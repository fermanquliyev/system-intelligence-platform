using System.Collections.Generic;
using System.Threading.Tasks;
using SystemIntelligencePlatform.Incidents;
using Volo.Abp.DependencyInjection;

namespace SystemIntelligencePlatform.Fakes;

/// <summary>
/// Deterministic fake for IIncidentAiAnalyzer that simulates LLM-style responses without calling any API.
/// Use in tests to avoid external API calls.
/// </summary>
[Dependency(ReplaceServices = true)]
public class FakeLlmIncidentAiAnalyzer : IIncidentAiAnalyzer, ITransientDependency
{
    public Task<AiAnalysisResult> AnalyzeAsync(IEnumerable<string> logMessages)
    {
        return Task.FromResult(new AiAnalysisResult
        {
            SentimentScore = 0.3,
            KeyPhrases = new List<string> { "NullReferenceException", "OrderService", "payment", "session" },
            Entities = new List<string> { "OrderService", "PaymentGateway" },
            RootCauseSummary = "Null reference in payment flow when session expires mid-checkout.",
            SuggestedFix = "Add null check for customer session before payment processing. Implement session refresh middleware.",
            SeverityJustification = "High severity: unhandled exception in critical path. [PII detected: none]",
            ConfidenceScore = 87
        });
    }
}
