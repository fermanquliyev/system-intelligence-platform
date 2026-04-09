using System;
using System.Collections.Generic;
using System.Text.Json;
using SystemIntelligencePlatform.Incidents;

namespace SystemIntelligencePlatform.AI;

/// <summary>
/// Parses LLM JSON output into AiAnalysisResult. Exposed for unit testing.
/// </summary>
public static class LlmResponseParser
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true
    };

    /// <summary>
    /// Tries to parse the raw LLM response text (may be wrapped in markdown code blocks) into AiAnalysisResult.
    /// </summary>
    public static bool TryParse(string rawText, out AiAnalysisResult? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(rawText)) return false;

        var trimmed = rawText.Trim();
        if (trimmed.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
            trimmed = trimmed.AsSpan(7).Trim().ToString();
        else if (trimmed.StartsWith("```"))
            trimmed = trimmed.AsSpan(3).Trim().ToString();
        if (trimmed.EndsWith("```"))
            trimmed = trimmed.AsSpan(0, trimmed.Length - 3).Trim().ToString();

        try
        {
            var dto = JsonSerializer.Deserialize<LlmResponseDto>(trimmed, JsonOptions);
            if (dto == null) return false;

            var justification = dto.SeverityJustification ?? "";
            if (dto.ContainsPII && !string.IsNullOrEmpty(dto.PiiType))
                justification = $"[PII detected: {dto.PiiType}] " + justification;

            result = new AiAnalysisResult
            {
                RootCauseSummary = dto.RootCauseSummary ?? "",
                SuggestedFix = dto.SuggestedFix ?? "",
                SeverityJustification = justification,
                ConfidenceScore = dto.ConfidenceScore.HasValue ? (int)Math.Clamp(dto.ConfidenceScore.Value * 100, 0, 100) : 50,
                KeyPhrases = dto.KeyPhrases ?? new List<string>(),
                Entities = new List<string>(),
                SentimentScore = null,
                SuggestedSeverity = MapSeverity(dto.Severity)
            };
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static IncidentSeverity? MapSeverity(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        return s.Trim() switch
        {
            "Low" => IncidentSeverity.Low,
            "Medium" => IncidentSeverity.Medium,
            "High" => IncidentSeverity.High,
            "Critical" => IncidentSeverity.Critical,
            _ => null
        };
    }
}
