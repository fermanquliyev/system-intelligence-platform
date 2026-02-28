using System;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using SystemIntelligencePlatform.LogEvents;
using Volo.Abp.DependencyInjection;

namespace SystemIntelligencePlatform.AzureInfrastructure;

public class AzureServiceBusLogEventPublisher : ILogEventPublisher, ISingletonDependency, IAsyncDisposable
{
    private const string QueueName = "log-ingestion";

    private readonly ServiceBusSender _sender;
    private readonly ServiceBusClient _client;
    private readonly ILogger<AzureServiceBusLogEventPublisher> _logger;

    public AzureServiceBusLogEventPublisher(
        ServiceBusClient client,
        ILogger<AzureServiceBusLogEventPublisher> logger)
    {
        _client = client;
        _sender = client.CreateSender(QueueName);
        _logger = logger;
    }

    public async Task PublishAsync(LogEventMessage message)
    {
        try
        {
            var json = JsonSerializer.Serialize(message);
            var sbMessage = new ServiceBusMessage(json)
            {
                ContentType = "application/json",
                Subject = message.ApplicationId.ToString(),
                MessageId = Guid.NewGuid().ToString(),
                CorrelationId = message.CorrelationId ?? Guid.NewGuid().ToString()
            };

            if (message.TenantId.HasValue)
            {
                sbMessage.ApplicationProperties["TenantId"] = message.TenantId.Value.ToString();
            }

            sbMessage.ApplicationProperties["ApplicationId"] = message.ApplicationId.ToString();

            await _sender.SendMessageAsync(sbMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish log event to Service Bus queue {QueueName}", QueueName);
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _sender.DisposeAsync();
        await _client.DisposeAsync();
    }
}
