using System;
using SystemIntelligencePlatform.Incidents;

namespace SystemIntelligencePlatform.Copilot;

public class CopilotAnalysisDto
{
    public string RootCauseHypothesis { get; set; } = null!;
    public string SuggestedFixSteps { get; set; } = null!;
    public int ConfidenceScore { get; set; }
    public IncidentSeverity SuggestedSeverity { get; set; }
    public string? SeverityJustification { get; set; }
    public string PromptTemplateVersion { get; set; } = null!;
    public bool FromCache { get; set; }
}
