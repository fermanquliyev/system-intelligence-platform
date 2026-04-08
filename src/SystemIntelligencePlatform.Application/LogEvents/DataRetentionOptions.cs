namespace SystemIntelligencePlatform.LogEvents;

/// <summary>
/// How long raw log events are kept before the retention job can delete them.
/// </summary>
public class DataRetentionOptions
{
    public const string SectionName = "DataRetention";

    /// <summary>Days to retain LogEvent rows (default 90).</summary>
    public int LogRetentionDays { get; set; } = 90;
}
