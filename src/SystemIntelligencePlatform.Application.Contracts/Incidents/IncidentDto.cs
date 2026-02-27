using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace SystemIntelligencePlatform.Incidents;

public class IncidentDto : FullAuditedEntityDto<Guid>
{
    public Guid ApplicationId { get; set; }
    public string ApplicationName { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public IncidentSeverity Severity { get; set; }
    public IncidentStatus Status { get; set; }
    public string HashSignature { get; set; } = null!;
    public int OccurrenceCount { get; set; }
    public DateTime FirstOccurrence { get; set; }
    public DateTime LastOccurrence { get; set; }
    public double? SentimentScore { get; set; }
    public string? KeyPhrases { get; set; }
    public string? Entities { get; set; }
    public DateTime? AiAnalyzedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public List<IncidentCommentDto> Comments { get; set; } = new();
}
