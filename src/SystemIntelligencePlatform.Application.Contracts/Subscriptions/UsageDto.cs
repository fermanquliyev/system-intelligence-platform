namespace SystemIntelligencePlatform.Subscriptions;

public class UsageDto
{
    public long LogsIngested { get; set; }
    public long LogsLimit { get; set; }
    public double LogsPercentUsed { get; set; }
    public int AiCallsUsed { get; set; }
    public int ApplicationsUsed { get; set; }
    public int ApplicationsLimit { get; set; }
    public string Plan { get; set; } = null!;
    public int Month { get; set; }
}
