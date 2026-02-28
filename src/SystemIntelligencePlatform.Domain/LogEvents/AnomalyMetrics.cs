namespace SystemIntelligencePlatform.LogEvents;

/// <summary>
/// Per-application, per-hash-signature metrics used by the adaptive anomaly detection algorithm.
/// Computed from historical log event counts.
/// </summary>
public class AnomalyMetrics
{
    public int EventsLast5Min { get; set; }
    public int EventsLast1Hour { get; set; }
    public int EventsLast24Hours { get; set; }

    /// <summary>
    /// Average hourly event count computed over the trailing 7-day window.
    /// A value of 0 means no baseline exists (new signature).
    /// </summary>
    public double AverageHourlyBaseline { get; set; }

    /// <summary>
    /// Standard deviation of hourly counts over the trailing 7-day window.
    /// Used to distinguish volatile signatures from stable ones.
    /// </summary>
    public double StandardDeviation { get; set; }
}
