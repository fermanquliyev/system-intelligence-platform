namespace SystemIntelligencePlatform.Incidents;

public class IncidentSearchRequestDto
{
    public string Query { get; set; } = null!;
    public int Skip { get; set; }
    public int Take { get; set; } = 20;
}
