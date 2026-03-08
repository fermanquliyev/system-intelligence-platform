using System.Collections.Generic;
using System.Threading.Tasks;
using SystemIntelligencePlatform.Fakes;
using SystemIntelligencePlatform.Incidents;
using Shouldly;
using Xunit;

namespace SystemIntelligencePlatform.Infrastructure.Tests;

public class FakeLlmIncidentAiAnalyzer_Tests
{
    [Fact]
    public async Task AnalyzeAsync_ReturnsDeterministicResult_WithoutCallingApi()
    {
        var fake = new FakeLlmIncidentAiAnalyzer();
        var messages = new List<string> { "NullReferenceException in OrderService.ProcessPayment" };

        var result = await fake.AnalyzeAsync(messages);

        result.ShouldNotBeNull();
        result.RootCauseSummary.ShouldNotBeNullOrEmpty();
        result.SuggestedFix.ShouldNotBeNullOrEmpty();
        result.SeverityJustification!.ShouldContain("PII detected");
        result.ConfidenceScore.ShouldBe(87);
        result.KeyPhrases.ShouldContain("NullReferenceException");
    }

    [Fact]
    public async Task AnalyzeAsync_SameInput_ReturnsSameOutput()
    {
        var fake = new FakeLlmIncidentAiAnalyzer();
        var messages = new List<string> { "Connection timeout" };

        var a = await fake.AnalyzeAsync(messages);
        var b = await fake.AnalyzeAsync(messages);

        a!.RootCauseSummary.ShouldBe(b!.RootCauseSummary);
        a.ConfidenceScore.ShouldBe(b.ConfidenceScore);
    }
}
