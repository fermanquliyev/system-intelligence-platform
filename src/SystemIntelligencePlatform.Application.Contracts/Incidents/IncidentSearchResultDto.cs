using System.Collections.Generic;

namespace SystemIntelligencePlatform.Incidents;

public class IncidentSearchResultDto
{
    public long TotalCount { get; set; }
    public List<IncidentSearchItemDto> Items { get; set; } = new();
}

public class IncidentSearchItemDto
{
    public string Id { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string Severity { get; set; } = null!;
    public string ApplicationName { get; set; } = null!;
    public string? KeyPhrases { get; set; }
    public string? Entities { get; set; }
}
