using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace SystemIntelligencePlatform.FailedLogEvents;

public class FailedLogEvent : CreationAuditedEntity<Guid>
{
    public string OriginalMessageBody { get; set; } = null!;
    public string ErrorMessage { get; set; } = null!;
    public string? StackTrace { get; set; }
    public string? CorrelationId { get; set; }
    public int DeliveryAttempt { get; set; }
    public string? DeadLetterReason { get; set; }

    protected FailedLogEvent() { }

    public FailedLogEvent(
        Guid id,
        string originalMessageBody,
        string errorMessage,
        int deliveryAttempt,
        string? correlationId = null,
        string? stackTrace = null,
        string? deadLetterReason = null)
        : base(id)
    {
        OriginalMessageBody = originalMessageBody;
        ErrorMessage = errorMessage;
        DeliveryAttempt = deliveryAttempt;
        CorrelationId = correlationId;
        StackTrace = stackTrace;
        DeadLetterReason = deadLetterReason;
    }
}
