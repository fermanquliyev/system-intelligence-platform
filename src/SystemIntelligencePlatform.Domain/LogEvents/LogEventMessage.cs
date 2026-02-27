using System;

namespace SystemIntelligencePlatform.LogEvents;

/// <summary>
/// Message payload published to the Service Bus queue for async processing.
/// </summary>
public class LogEventMessage
{
    public Guid? TenantId { get; set; }
    public Guid ApplicationId { get; set; }
    public LogLevel Level { get; set; }
    public string Message { get; set; } = null!;
    public string? Source { get; set; }
    public string? ExceptionType { get; set; }
    public string? StackTrace { get; set; }
    public string? CorrelationId { get; set; }
    public DateTime Timestamp { get; set; }
}
