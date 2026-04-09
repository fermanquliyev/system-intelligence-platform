using System.Collections.Generic;
using System.Text;
using SystemIntelligencePlatform.Incidents;

namespace SystemIntelligencePlatform.Copilot;

/// <summary>Versioned, code-stored prompts for the incident copilot.</summary>
public static class CopilotPromptTemplates
{
    public const string Version = "v1";

    public static string VersionHeader() => $"[CopilotPromptTemplate:{Version}]";

    public static IReadOnlyList<string> BuildIncidentAnalysisLines(
        Incident incident,
        string applicationName,
        IReadOnlyList<string> recentLogMessages,
        IReadOnlyList<string>? recentMetricSummaries = null)
    {
        var lines = new List<string>
        {
            VersionHeader(),
            $"Incident title: {incident.Title}",
            $"Application: {applicationName}",
            $"Severity: {incident.Severity}",
            $"Status: {incident.Status}",
            $"Occurrences: {incident.OccurrenceCount}",
            $"Hash signature: {incident.HashSignature}"
        };

        if (!string.IsNullOrWhiteSpace(incident.Description))
            lines.Add("Description: " + incident.Description);

        lines.Add("--- Recent log messages ---");
        lines.AddRange(recentLogMessages);

        if (recentMetricSummaries is { Count: > 0 })
        {
            lines.Add("--- Metric snapshots ---");
            lines.AddRange(recentMetricSummaries);
        }

        return lines;
    }

    public static string BuildFollowUpBlock(IReadOnlyList<(CopilotMessageRole Role, string Content)> history, string userMessage)
    {
        var sb = new StringBuilder();
        sb.AppendLine(VersionHeader());
        sb.AppendLine("Follow-up conversation about the same incident. Answer concisely in the same JSON schema as incident log analysis when possible.");
        foreach (var (role, content) in history)
        {
            sb.Append(role).Append(": ").AppendLine(content);
        }

        sb.Append("User: ").AppendLine(userMessage);
        return sb.ToString();
    }
}
