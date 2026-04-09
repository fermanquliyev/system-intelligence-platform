using System;
using Volo.Abp.Domain.Entities;

namespace SystemIntelligencePlatform.Playbooks;

public class PlaybookStep : Entity<Guid>
{
    public Guid PlaybookId { get; set; }
    public int SortOrder { get; set; }
    public string Title { get; set; } = null!;
    public string? Body { get; set; }

    protected PlaybookStep() { }

    public PlaybookStep(Guid id, Guid playbookId, int sortOrder, string title, string? body = null)
        : base(id)
    {
        PlaybookId = playbookId;
        SortOrder = sortOrder;
        Title = title;
        Body = body;
    }
}
