using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SystemIntelligencePlatform.AI;

/// <summary>
/// Expected JSON shape from the LLM for incident analysis. Used only for parsing; not part of Domain.
/// </summary>
internal class LlmResponseDto
{
    [JsonPropertyName("rootCauseSummary")]
    public string? RootCauseSummary { get; set; }

    [JsonPropertyName("suggestedFix")]
    public string? SuggestedFix { get; set; }

    [JsonPropertyName("severity")]
    public string? Severity { get; set; }

    [JsonPropertyName("severityJustification")]
    public string? SeverityJustification { get; set; }

    [JsonPropertyName("confidenceScore")]
    public double? ConfidenceScore { get; set; }

    [JsonPropertyName("containsPII")]
    public bool ContainsPII { get; set; }

    [JsonPropertyName("piiType")]
    public string? PiiType { get; set; }

    [JsonPropertyName("keyPhrases")]
    public List<string>? KeyPhrases { get; set; }
}
