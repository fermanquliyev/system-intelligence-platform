namespace SystemIntelligencePlatform.LogIngestion;

public class LogIngestionResultDto
{
    public int Accepted { get; set; }
    public string Status { get; set; } = "Queued";
}
