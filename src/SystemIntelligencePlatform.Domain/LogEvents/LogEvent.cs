using System;
using Volo.Abp.Domain.Entities;

namespace SystemIntelligencePlatform.LogEvents;

public class LogEvent : BasicAggregateRoot<Guid>
{
    public Guid ApplicationId { get; set; }
    public LogLevel Level { get; set; }
    public string Message { get; set; } = null!;
    public string? Source { get; set; }
    public string? ExceptionType { get; set; }
    public string? StackTrace { get; set; }
    public string HashSignature { get; set; } = null!;
    public string? CorrelationId { get; set; }
    public DateTime Timestamp { get; set; }
    public Guid? IncidentId { get; set; }

    public Guid? ClusterId { get; set; }

    public bool ContainsPii { get; set; }

    protected LogEvent() { }

    public LogEvent(
        Guid id,
        Guid applicationId,
        LogLevel level,
        string message,
        string hashSignature,
        DateTime timestamp)
        : base(id)
    {
        ApplicationId = applicationId;
        Level = level;
        Message = message;
        HashSignature = hashSignature;
        Timestamp = timestamp;
    }
}
