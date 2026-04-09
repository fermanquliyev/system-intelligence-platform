using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace SystemIntelligencePlatform.LogSearch;

public class SavedLogSearch : FullAuditedAggregateRoot<Guid>
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = null!;
    /// <summary>Serialized filter (text, severity, time range, application id).</summary>
    public string FilterJson { get; set; } = "{}";

    protected SavedLogSearch() { }

    public SavedLogSearch(Guid id, Guid userId, string name, string filterJson)
        : base(id)
    {
        UserId = userId;
        Name = name;
        FilterJson = filterJson;
    }
}
