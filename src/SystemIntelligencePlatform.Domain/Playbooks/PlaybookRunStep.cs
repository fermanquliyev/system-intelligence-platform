using System;
using Volo.Abp.Domain.Entities;

namespace SystemIntelligencePlatform.Playbooks;

public class PlaybookRunStep : Entity<Guid>
{
    public Guid PlaybookRunId { get; set; }
    public int StepOrder { get; set; }
    public string Title { get; set; } = null!;
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }

    protected PlaybookRunStep() { }

    public PlaybookRunStep(Guid id, Guid playbookRunId, int stepOrder, string title)
        : base(id)
    {
        PlaybookRunId = playbookRunId;
        StepOrder = stepOrder;
        Title = title;
    }
}
