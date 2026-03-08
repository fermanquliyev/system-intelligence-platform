using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SystemIntelligencePlatform.Incidents;
using Volo.Abp.DependencyInjection;

namespace SystemIntelligencePlatform.Incidents;

/// <summary>
/// Local deterministic AI analyzer. No external API calls; pure C# heuristics
/// for key phrase extraction, root cause summary, suggested fix, and confidence.
/// </summary>
public class LocalIncidentAiAnalyzer : IIncidentAiAnalyzer, ITransientDependency
{
    private static readonly Regex WordBreakRegex = new(@"[\w]+", RegexOptions.Compiled);
    private static readonly string[] ErrorSignatures =
    [
        "timeout", "connection", "refused", "null", "reference", "memory", "out of memory",
        "authentication", "unauthorized", "exception", "error", "failed", "database",
        "network", "socket", "certificate", "ssl", "deadlock", "constraint"
    ];

    public Task<AiAnalysisResult> AnalyzeAsync(IEnumerable<string> logMessages)
    {
        var messages = logMessages.Take(5).ToList();
        var result = new AiAnalysisResult();

        if (messages.Count == 0)
            return Task.FromResult(result);

        // Key phrase extraction: significant words (length >= 4) that appear in error signatures or are repeated
        var allWords = messages
            .SelectMany(m => WordBreakRegex.Matches(m.ToLowerInvariant()).Select(match => match.Value))
            .Where(w => w.Length >= 4)
            .ToList();
        var phraseCounts = allWords.GroupBy(w => w).ToDictionary(g => g.Key, g => g.Count());
        result.KeyPhrases = phraseCounts
            .OrderByDescending(kv => kv.Value)
            .ThenByDescending(kv => ErrorSignatures.Any(s => kv.Key.Contains(s)) ? 1 : 0)
            .Select(kv => kv.Key)
            .Distinct()
            .Take(20)
            .ToList();

        // Simple entity-like tokens: PascalCase or UPPER segments
        var entityRegex = new Regex(@"(?:[A-Z][a-z]+(?=[A-Z]|$)|[A-Z]{2,})", RegexOptions.Compiled);
        result.Entities = messages
            .SelectMany(m => entityRegex.Matches(m).Select(m => m.Value))
            .Distinct()
            .Take(20)
            .ToList();

        // Sentiment: heuristic from message content (error/warning/exception = lower)
        var negativeIndicators = new[] { "error", "exception", "fail", "critical", "fatal" };
        var positiveCount = messages.Count(m => !negativeIndicators.Any(n => m.Contains(n, StringComparison.OrdinalIgnoreCase)));
        result.SentimentScore = messages.Count > 0 ? (double)positiveCount / messages.Count : 0.5;

        // Root cause summary from key phrases and first message
        result.RootCauseSummary = BuildRootCauseSummary(result.KeyPhrases, result.Entities, messages);

        // Suggested fix from error signatures
        result.SuggestedFix = BuildSuggestedFix(result.KeyPhrases);

        // Severity justification
        result.SeverityJustification = $"Analyzed {messages.Count} log message(s) with {result.KeyPhrases.Count} key phrase(s) identified.";

        // Confidence: 0–100 from phrase coverage and consistency
        var confidence = result.KeyPhrases.Count * 4;
        if (result.KeyPhrases.Any(k => ErrorSignatures.Any(s => k.Contains(s, StringComparison.OrdinalIgnoreCase))))
            confidence += 25;
        result.ConfidenceScore = Math.Clamp(confidence, 0, 100);

        return Task.FromResult(result);
    }

    private static string BuildRootCauseSummary(List<string> keyPhrases, List<string> entities, List<string> messages)
    {
        var parts = new List<string>();
        if (keyPhrases.Count > 0)
            parts.Add("Key indicators: " + string.Join(", ", keyPhrases.Take(5)));
        if (entities.Count > 0)
            parts.Add("Related tokens: " + string.Join(", ", entities.Take(3)));
        if (messages.Count > 0)
        {
            var sample = messages[0].Length <= 100 ? messages[0] : messages[0][..100] + "...";
            parts.Add("Sample: \"" + sample + "\"");
        }
        return parts.Count > 0 ? string.Join(". ", parts) : "No summary available.";
    }

    private static string BuildSuggestedFix(List<string> keyPhrases)
    {
        var lower = keyPhrases.Select(k => k.ToLowerInvariant()).ToList();
        if (lower.Any(k => k.Contains("timeout")))
            return "Consider increasing timeout values or checking network connectivity.";
        if (lower.Any(k => k.Contains("memory") || k.Contains("out of memory")))
            return "Review memory allocation and consider scaling up the service.";
        if (lower.Any(k => k.Contains("connection") || k.Contains("refused")))
            return "Verify database/service connection strings and endpoint availability.";
        if (lower.Any(k => k.Contains("null") || k.Contains("reference")))
            return "Add null checks or validate input data before processing.";
        if (lower.Any(k => k.Contains("authentication") || k.Contains("unauthorized")))
            return "Verify credentials and token expiration settings.";
        if (lower.Any(k => k.Contains("database")))
            return "Check database connectivity, constraints, and query performance.";
        return "Review the error logs and stack traces for detailed analysis.";
    }
}
