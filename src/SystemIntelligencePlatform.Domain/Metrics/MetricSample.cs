using System;
using Volo.Abp.Domain.Entities;

namespace SystemIntelligencePlatform.Metrics;

public class MetricSample : BasicAggregateRoot<Guid>
{
    public Guid ApplicationId { get; set; }
    public string Name { get; set; } = null!;
    public DateTime Timestamp { get; set; }
    public double Value { get; set; }
    public string? TagsJson { get; set; }

    protected MetricSample() { }

    public MetricSample(Guid id, Guid applicationId, string name, DateTime timestamp, double value)
        : base(id)
    {
        ApplicationId = applicationId;
        Name = name;
        Timestamp = timestamp;
        Value = value;
    }
}
