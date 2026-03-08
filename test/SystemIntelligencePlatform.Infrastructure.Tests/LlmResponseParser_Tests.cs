using System.Collections.Generic;
using SystemIntelligencePlatform.AI;
using SystemIntelligencePlatform.Incidents;
using Shouldly;
using Xunit;

namespace SystemIntelligencePlatform.Infrastructure.Tests;

public class LlmResponseParser_Tests
{
    [Fact]
    public void TryParse_ValidJson_ReturnsTrueAndMapsToAiAnalysisResult()
    {
        var json = @"{
            ""rootCauseSummary"": ""Connection timeout to database."",
            ""suggestedFix"": ""Check connection string and network."",
            ""severity"": ""High"",
            ""severityJustification"": ""Repeated timeouts indicate infrastructure issue."",
            ""confidenceScore"": 0.85,
            ""containsPII"": false,
            ""piiType"": ""none"",
            ""keyPhrases"": [""timeout"", ""database"", ""connection""]
        }";

        var ok = LlmResponseParser.TryParse(json, out var result);

        ok.ShouldBeTrue();
        result.ShouldNotBeNull();
        result!.RootCauseSummary.ShouldBe("Connection timeout to database.");
        result.SuggestedFix.ShouldBe("Check connection string and network.");
        result.SeverityJustification.ShouldBe("Repeated timeouts indicate infrastructure issue.");
        result.ConfidenceScore.ShouldBe(85);
        result.KeyPhrases.ShouldBe(new List<string> { "timeout", "database", "connection" });
    }

    [Fact]
    public void TryParse_JsonWithMarkdownCodeBlock_StripsAndParses()
    {
        var wrapped = "```json\n{\"rootCauseSummary\":\"x\",\"suggestedFix\":\"y\",\"severityJustification\":\"z\",\"confidenceScore\":0.5,\"containsPII\":false,\"keyPhrases\":[]}\n```";

        var ok = LlmResponseParser.TryParse(wrapped, out var result);

        ok.ShouldBeTrue();
        result.ShouldNotBeNull();
        result!.RootCauseSummary.ShouldBe("x");
        result.ConfidenceScore.ShouldBe(50);
    }

    [Fact]
    public void TryParse_ContainsPII_AppendsPiiToSeverityJustification()
    {
        var json = @"{
            ""rootCauseSummary"": ""Log may contain email."",
            ""suggestedFix"": ""Redact PII."",
            ""severityJustification"": ""User email visible in stack trace."",
            ""confidenceScore"": 0.9,
            ""containsPII"": true,
            ""piiType"": ""email"",
            ""keyPhrases"": []
        }";

        LlmResponseParser.TryParse(json, out var result).ShouldBeTrue();
        result.ShouldNotBeNull();
        result!.SeverityJustification.ShouldContain("[PII detected: email]");
        result.SeverityJustification!.ShouldContain("User email visible");
    }

    [Fact]
    public void TryParse_InvalidJson_ReturnsFalse()
    {
        LlmResponseParser.TryParse("not json at all", out var result).ShouldBeFalse();
        result.ShouldBeNull();
    }

    [Fact]
    public void TryParse_EmptyString_ReturnsFalse()
    {
        LlmResponseParser.TryParse("", out var result).ShouldBeFalse();
        result.ShouldBeNull();
    }

    [Fact]
    public void TryParse_MalformedJson_ReturnsFalse()
    {
        LlmResponseParser.TryParse("{ \"rootCauseSummary\": ", out var result).ShouldBeFalse();
        result.ShouldBeNull();
    }
}
