using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SystemIntelligencePlatform.Incidents;

public class Incident : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    public Guid ApplicationId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public IncidentSeverity Severity { get; set; }
    public IncidentStatus Status { get; set; }
    public string HashSignature { get; set; } = null!;
    public int OccurrenceCount { get; set; }
    public DateTime FirstOccurrence { get; set; }
    public DateTime LastOccurrence { get; set; }

    // AI enrichment fields
    public double? SentimentScore { get; set; }
    public string? KeyPhrases { get; set; }
    public string? Entities { get; set; }
    public string? RootCauseSummary { get; set; }
    public string? SuggestedFix { get; set; }
    public string? SeverityJustification { get; set; }
    public int? ConfidenceScore { get; set; }
    public DateTime? AiAnalyzedAt { get; set; }

    public DateTime? ResolvedAt { get; set; }
    public Guid? ResolvedByUserId { get; set; }

    public ICollection<IncidentComment> Comments { get; set; } = new List<IncidentComment>();

    protected Incident() { }

    public Incident(
        Guid id,
        Guid applicationId,
        string title,
        string hashSignature,
        IncidentSeverity severity,
        DateTime firstOccurrence,
        Guid? tenantId = null)
        : base(id)
    {
        TenantId = tenantId;
        ApplicationId = applicationId;
        Title = title;
        HashSignature = hashSignature;
        Severity = severity;
        Status = IncidentStatus.Open;
        OccurrenceCount = 1;
        FirstOccurrence = firstOccurrence;
        LastOccurrence = firstOccurrence;
    }

    public void IncrementOccurrence(DateTime timestamp)
    {
        OccurrenceCount++;
        LastOccurrence = timestamp;
        EscalateSeverityIfNeeded();
    }

    public void Resolve(Guid userId)
    {
        Status = IncidentStatus.Resolved;
        ResolvedAt = DateTime.UtcNow;
        ResolvedByUserId = userId;
    }

    public void EnrichWithAiAnalysis(AiAnalysisResult result)
    {
        SentimentScore = result.SentimentScore;
        KeyPhrases = result.KeyPhrases.Count > 0 ? string.Join(", ", result.KeyPhrases) : null;
        Entities = result.Entities.Count > 0 ? string.Join(", ", result.Entities) : null;
        RootCauseSummary = result.RootCauseSummary;
        SuggestedFix = result.SuggestedFix;
        SeverityJustification = result.SeverityJustification;
        ConfidenceScore = result.ConfidenceScore;
        AiAnalyzedAt = DateTime.UtcNow;
    }

    private void EscalateSeverityIfNeeded()
    {
        Severity = OccurrenceCount switch
        {
            >= 100 => IncidentSeverity.Critical,
            >= 50 => IncidentSeverity.High,
            >= 10 => IncidentSeverity.Medium,
            _ => Severity
        };
    }
}
