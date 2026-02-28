using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SystemIntelligencePlatform.FailedLogEvents;

public class FailedLogEvent : CreationAuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
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
        Guid? tenantId = null,
        string? correlationId = null,
        string? stackTrace = null,
        string? deadLetterReason = null)
        : base(id)
    {
        TenantId = tenantId;
        OriginalMessageBody = originalMessageBody;
        ErrorMessage = errorMessage;
        DeliveryAttempt = deliveryAttempt;
        CorrelationId = correlationId;
        StackTrace = stackTrace;
        DeadLetterReason = deadLetterReason;
    }
}
