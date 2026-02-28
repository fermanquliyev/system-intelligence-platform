using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SystemIntelligencePlatform.EntityFrameworkCore;
using SystemIntelligencePlatform.FailedLogEvents;

namespace SystemIntelligencePlatform.AzureFunctions.Functions;

/// <summary>
/// Processes messages from the dead-letter sub-queue of "log-ingestion".
/// Stores failed messages as FailedLogEvent entities for manual review.
///
/// Retry strategy: Azure Service Bus handles retries via MaxDeliveryCount (configured to 10).
/// Each delivery attempt uses exponential backoff: delay = BaseRetryDelaySeconds * 2^(attempt-1).
/// After MaxDeliveryCount, the message goes to the dead-letter queue and this function captures it.
/// </summary>
public class DeadLetterProcessorFunction
{
    private readonly SystemIntelligencePlatformDbContext _dbContext;
    private readonly ILogger<DeadLetterProcessorFunction> _logger;

    public DeadLetterProcessorFunction(
        SystemIntelligencePlatformDbContext dbContext,
        ILogger<DeadLetterProcessorFunction> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    [Function("DeadLetterProcessor")]
    public async Task RunAsync(
        [ServiceBusTrigger("log-ingestion/$deadletterqueue", Connection = "ServiceBusConnection")]
        ServiceBusReceivedMessage message)
    {
        var correlationId = message.CorrelationId ?? message.MessageId;
        var tenantId = message.ApplicationProperties.TryGetValue("TenantId", out var tid)
            ? Guid.TryParse(tid?.ToString(), out var parsedTid) ? parsedTid : (Guid?)null
            : null;

        _logger.LogWarning(
            "Processing dead-letter message. CorrelationId={CorrelationId}, DeadLetterReason={Reason}, DeliveryCount={Count}",
            correlationId,
            message.DeadLetterReason,
            message.DeliveryCount);

        var dbContext = _dbContext;

        var failedEvent = new FailedLogEvent(
            Guid.NewGuid(),
            message.Body.ToString(),
            message.DeadLetterErrorDescription ?? "Unknown error",
            message.DeliveryCount,
            tenantId,
            correlationId,
            deadLetterReason: message.DeadLetterReason);

        dbContext.FailedLogEvents.Add(failedEvent);
        await dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "Dead-letter message stored as FailedLogEvent {Id}. CorrelationId={CorrelationId}",
            failedEvent.Id, correlationId);
    }
}
