using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;

namespace SystemIntelligencePlatform.Playbooks;

public class PlaybookRun : FullAuditedAggregateRoot<Guid>
{
    public Guid PlaybookId { get; set; }
    public Guid IncidentId { get; set; }
    public PlaybookRunStatus Status { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public ICollection<PlaybookRunStep> RunSteps { get; set; } = new List<PlaybookRunStep>();

    protected PlaybookRun() { }

    public PlaybookRun(Guid id, Guid playbookId, Guid incidentId)
        : base(id)
    {
        PlaybookId = playbookId;
        IncidentId = incidentId;
        Status = PlaybookRunStatus.InProgress;
        StartedAt = DateTime.UtcNow;
    }
}
