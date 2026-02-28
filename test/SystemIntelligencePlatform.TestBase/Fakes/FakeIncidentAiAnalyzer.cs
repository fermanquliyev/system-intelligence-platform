using System.Collections.Generic;
using System.Threading.Tasks;
using SystemIntelligencePlatform.Incidents;
using Volo.Abp.DependencyInjection;

namespace SystemIntelligencePlatform.Fakes;

[Dependency(ReplaceServices = true)]
public class FakeIncidentAiAnalyzer : IIncidentAiAnalyzer, ITransientDependency
{
    public Task<AiAnalysisResult> AnalyzeAsync(IEnumerable<string> logMessages)
    {
        return Task.FromResult(new AiAnalysisResult
        {
            SentimentScore = 0.5,
            KeyPhrases = new List<string> { "test", "error" },
            Entities = new List<string> { "Server:Technology" },
            RootCauseSummary = "Test root cause analysis",
            SuggestedFix = "Review test configuration",
            SeverityJustification = "Test severity justification",
            ConfidenceScore = 75
        });
    }
}
