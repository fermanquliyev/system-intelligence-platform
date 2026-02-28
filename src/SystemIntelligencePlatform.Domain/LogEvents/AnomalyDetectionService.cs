using SystemIntelligencePlatform.Incidents;
using Volo.Abp.DependencyInjection;

namespace SystemIntelligencePlatform.LogEvents;

/// <summary>
/// Adaptive anomaly detection replacing the static threshold.
///
/// Algorithm:
/// 1. SPIKE DETECTION: Short-burst check over 5-minute window.
///    Expected 5-min rate = baseline_hourly / 12.
///    Trigger if actual > expected * 3 (3x spike multiplier).
///
/// 2. BURST DETECTION: Sustained-rate check over 1-hour window.
///    Trigger if actual > baseline_hourly * 2 (2x sustained multiplier).
///
/// 3. IMMEDIATE CRITICAL: Any Critical-level log event bypasses
///    statistical checks and triggers an incident immediately.
///
/// 4. NEW SIGNATURE FALLBACK: When no 7-day baseline exists
///    (AverageHourlyBaseline == 0), fall back to absolute thresholds
///    to avoid division-by-zero and still catch real problems.
///
/// Returns: whether an incident should be created/updated, and the suggested severity.
/// </summary>
public class AnomalyDetectionService : ITransientDependency
{
    private const double SpikeMultiplier = 3.0;
    private const double BurstMultiplier = 2.0;
    private const int NewSignatureSpikeThreshold = 10;
    private const int NewSignatureBurstThreshold = 30;

    public AnomalyDetectionResult Evaluate(AnomalyMetrics metrics, LogLevel logLevel)
    {
        // Rule 3: Critical logs always trigger immediately
        if (logLevel == LogLevel.Critical)
        {
            return new AnomalyDetectionResult
            {
                ShouldTrigger = true,
                Reason = AnomalyReason.ImmediateCritical,
                SuggestedSeverity = IncidentSeverity.Critical
            };
        }

        var hasBaseline = metrics.AverageHourlyBaseline > 0;

        // Rule 1: Spike detection (5-minute window)
        if (IsSpikeDetected(metrics, hasBaseline))
        {
            return new AnomalyDetectionResult
            {
                ShouldTrigger = true,
                Reason = AnomalyReason.SpikeDetected,
                SuggestedSeverity = DetermineSeverity(logLevel, metrics.EventsLast5Min)
            };
        }

        // Rule 2: Burst detection (1-hour window)
        if (IsBurstDetected(metrics, hasBaseline))
        {
            return new AnomalyDetectionResult
            {
                ShouldTrigger = true,
                Reason = AnomalyReason.BurstDetected,
                SuggestedSeverity = DetermineSeverity(logLevel, metrics.EventsLast1Hour)
            };
        }

        return new AnomalyDetectionResult { ShouldTrigger = false };
    }

    private static bool IsSpikeDetected(AnomalyMetrics metrics, bool hasBaseline)
    {
        if (hasBaseline)
        {
            // Expected events in 5 minutes = hourly baseline / 12
            var expected5Min = metrics.AverageHourlyBaseline / 12.0;
            return metrics.EventsLast5Min > expected5Min * SpikeMultiplier;
        }

        // No baseline: use absolute threshold
        return metrics.EventsLast5Min > NewSignatureSpikeThreshold;
    }

    private static bool IsBurstDetected(AnomalyMetrics metrics, bool hasBaseline)
    {
        if (hasBaseline)
        {
            return metrics.EventsLast1Hour > metrics.AverageHourlyBaseline * BurstMultiplier;
        }

        return metrics.EventsLast1Hour > NewSignatureBurstThreshold;
    }

    private static IncidentSeverity DetermineSeverity(LogLevel level, int eventCount)
    {
        return (level, eventCount) switch
        {
            (LogLevel.Error, >= 100) => IncidentSeverity.Critical,
            (LogLevel.Error, >= 50) => IncidentSeverity.High,
            (LogLevel.Error, _) => IncidentSeverity.Medium,
            (LogLevel.Warning, >= 200) => IncidentSeverity.High,
            (LogLevel.Warning, _) => IncidentSeverity.Medium,
            _ => IncidentSeverity.Low
        };
    }
}

public class AnomalyDetectionResult
{
    public bool ShouldTrigger { get; set; }
    public AnomalyReason Reason { get; set; }
    public IncidentSeverity SuggestedSeverity { get; set; }
}

public enum AnomalyReason
{
    None,
    SpikeDetected,
    BurstDetected,
    ImmediateCritical
}
