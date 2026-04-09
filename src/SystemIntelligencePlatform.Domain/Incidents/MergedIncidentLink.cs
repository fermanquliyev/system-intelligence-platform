using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace SystemIntelligencePlatform.Incidents;

/// <summary>Links a merged (duplicate) incident to its canonical incident.</summary>
public class MergedIncidentLink : CreationAuditedEntity<Guid>
{
    public Guid CanonicalIncidentId { get; set; }
    public Guid MergedIncidentId { get; set; }
    public double SimilarityScore { get; set; }

    protected MergedIncidentLink() { }

    public MergedIncidentLink(Guid id, Guid canonicalIncidentId, Guid mergedIncidentId, double similarityScore)
        : base(id)
    {
        CanonicalIncidentId = canonicalIncidentId;
        MergedIncidentId = mergedIncidentId;
        SimilarityScore = similarityScore;
    }
}
