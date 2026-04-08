using System;
using System.Diagnostics;

namespace SystemIntelligencePlatform.LogEvents;

/// <summary>
/// Observability helpers for structured logging and distributed tracing.
/// </summary>
public static class LogEventTelemetry
{
    public static readonly ActivitySource ActivitySource = new("SystemIntelligencePlatform");

    public static Activity? StartIngestionActivity(string correlationId, Guid applicationId)
    {
        var activity = ActivitySource.StartActivity("LogIngestion");
        activity?.SetTag("correlationId", correlationId);
        activity?.SetTag("applicationId", applicationId.ToString());
        return activity;
    }

    public static Activity? StartProcessingActivity(string correlationId, string hashSignature)
    {
        var activity = ActivitySource.StartActivity("IncidentProcessing");
        activity?.SetTag("correlationId", correlationId);
        activity?.SetTag("hashSignature", hashSignature);
        return activity;
    }

    public static Activity? StartAiAnalysisActivity(Guid incidentId)
    {
        var activity = ActivitySource.StartActivity("AiAnalysis");
        activity?.SetTag("incidentId", incidentId.ToString());
        return activity;
    }
}
