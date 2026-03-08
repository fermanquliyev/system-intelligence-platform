using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SystemIntelligencePlatform.Incidents;
using SystemIntelligencePlatform.Realtime;

namespace SystemIntelligencePlatform.Infrastructure;

/// <summary>
/// Consumes incident notification messages from RabbitMQ and pushes them via self-hosted SignalR.
/// </summary>
public class IncidentNotificationConsumerService : BackgroundService
{
    public const string QueueName = "incident-notifications";

    private readonly RabbitMqOptions _options;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<IncidentNotificationConsumerService> _logger;
    private IConnection? _connection;
    private IModel? _channel;

    public IncidentNotificationConsumerService(
        IOptions<RabbitMqOptions> options,
        IServiceScopeFactory scopeFactory,
        ILogger<IncidentNotificationConsumerService> logger)
    {
        _options = options.Value;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.Host,
            Port = _options.Port,
            UserName = _options.Username,
            Password = _options.Password,
            VirtualHost = string.IsNullOrEmpty(_options.VirtualHost) ? "/" : _options.VirtualHost,
            AutomaticRecoveryEnabled = true,
            DispatchConsumersAsync = true
        };

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                _channel.QueueDeclare(QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
                _channel.BasicQos(0, 1, false);

                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.Received += async (_, ea) =>
                {
                    try
                    {
                        await HandleMessageAsync(ea.Body.ToArray(), ea.DeliveryTag);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error handling incident notification");
                    }
                };

                _channel.BasicConsume(QueueName, autoAck: false, consumer);
                _logger.LogInformation("Incident notification consumer started for queue {Queue}", QueueName);

                while (_connection?.IsOpen == true && !stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RabbitMQ connection error in notification consumer; reconnecting in 10s");
                await Task.Delay(10000, stoppingToken);
            }
            finally
            {
                _channel?.Dispose();
                _connection?.Dispose();
            }
        }
    }

    private async Task HandleMessageAsync(byte[] body, ulong deliveryTag)
    {
        var json = Encoding.UTF8.GetString(body);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var eventType = root.GetProperty("EventType").GetString();
        Guid? tenantId = null;
        if (root.TryGetProperty("TenantId", out var tidEl) && tidEl.ValueKind == JsonValueKind.String)
        {
            var tidStr = tidEl.GetString();
            if (!string.IsNullOrEmpty(tidStr) && Guid.TryParse(tidStr, out var tid))
                tenantId = tid;
        }
        var notificationEl = root.GetProperty("Notification");
        var notification = JsonSerializer.Deserialize<IncidentNotification>(notificationEl.GetRawText());
        if (notification == null)
        {
            Ack(deliveryTag);
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var notifier = scope.ServiceProvider.GetRequiredService<IRealtimeNotifier>();

        switch (eventType)
        {
            case "IncidentCreated":
                await notifier.NotifyIncidentCreatedAsync(tenantId, notification);
                break;
            case "IncidentUpdated":
                await notifier.NotifyIncidentUpdatedAsync(tenantId, notification);
                break;
            case "IncidentResolved":
                await notifier.NotifyIncidentResolvedAsync(tenantId, notification);
                break;
            default:
                _logger.LogWarning("Unknown incident event type: {EventType}", eventType);
                break;
        }

        Ack(deliveryTag);
    }

    private void Ack(ulong deliveryTag)
    {
        try
        {
            _channel?.BasicAck(deliveryTag, false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to ack notification message {DeliveryTag}", deliveryTag);
        }
    }
}
