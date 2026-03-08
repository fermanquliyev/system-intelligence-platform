using System.Collections.Generic;
using System.Threading.Tasks;
using SystemIntelligencePlatform.Incidents;
using Shouldly;
using Xunit;

namespace SystemIntelligencePlatform.Infrastructure.Tests;

public class LocalIncidentAiAnalyzer_Tests
{
    private readonly LocalIncidentAiAnalyzer _analyzer = new();

    [Fact]
    public async Task AnalyzeAsync_EmptyMessages_ReturnsEmptyResult()
    {
        var result = await _analyzer.AnalyzeAsync(new List<string>());
        result.KeyPhrases.ShouldBeEmpty();
        result.Entities.ShouldBeEmpty();
        result.ConfidenceScore.ShouldBe(0);
    }

    [Fact]
    public async Task AnalyzeAsync_SameInput_ProducesDeterministicOutput()
    {
        var messages = new List<string>
        {
            "Connection timeout occurred when calling database server",
            "Failed to connect to the remote host"
        };
        var result1 = await _analyzer.AnalyzeAsync(messages);
        var result2 = await _analyzer.AnalyzeAsync(messages);
        result1.KeyPhrases.ShouldBe(result2.KeyPhrases);
        result1.RootCauseSummary.ShouldBe(result2.RootCauseSummary);
        result1.SuggestedFix.ShouldBe(result2.SuggestedFix);
        result1.ConfidenceScore.ShouldBe(result2.ConfidenceScore);
    }

    [Fact]
    public async Task AnalyzeAsync_TimeoutMessage_ProducesTimeoutSuggestedFix()
    {
        var messages = new List<string> { "Request timeout after 30 seconds" };
        var result = await _analyzer.AnalyzeAsync(messages);
        result.SuggestedFix!.ShouldContain("timeout");
        result.ConfidenceScore.ShouldBeInRange(0, 100);
    }

    [Fact]
    public async Task AnalyzeAsync_KeyPhrasesAndRootCause_ArePopulated()
    {
        var messages = new List<string>
        {
            "NullReferenceException in PaymentService.ProcessOrder",
            "Object reference not set to an instance of an object"
        };
        var result = await _analyzer.AnalyzeAsync(messages);
        result.KeyPhrases.ShouldNotBeEmpty();
        result.RootCauseSummary.ShouldNotBeNullOrEmpty();
        result.SeverityJustification.ShouldNotBeNullOrEmpty();
    }
}
