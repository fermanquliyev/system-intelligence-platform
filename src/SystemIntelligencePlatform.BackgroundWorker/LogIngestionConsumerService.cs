using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SystemIntelligencePlatform.EntityFrameworkCore;
using SystemIntelligencePlatform.Incidents;
using SystemIntelligencePlatform.LogEvents;

namespace SystemIntelligencePlatform.BackgroundWorker;

public class LogIngestionConsumerService : BackgroundService
{
    public const string LogIngestionQueueName = "log-ingestion";
    public const string IncidentNotificationsQueueName = "incident-notifications";

    private readonly RabbitMqWorkerOptions _options;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LogIngestionConsumerService> _logger;
    private IConnection? _connection;
    private IModel? _channel;

    public LogIngestionConsumerService(
        IOptions<RabbitMqWorkerOptions> options,
        IServiceProvider serviceProvider,
        ILogger<LogIngestionConsumerService> logger)
    {
        _options = options.Value;
        _serviceProvider = serviceProvider;
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
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
            RequestedHeartbeat = TimeSpan.FromSeconds(60),
            DispatchConsumersAsync = true
        };

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                _channel.QueueDeclare(LogIngestionQueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
                _channel.QueueDeclare(IncidentNotificationsQueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
                _channel.BasicQos(0, 1, false);

                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.Received += async (_, ea) =>
                {
                    try
                    {
                        await ProcessMessageAsync(ea.Body.ToArray(), ea.DeliveryTag);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing log ingestion message");
                    }
                };

                _channel.BasicConsume(LogIngestionQueueName, autoAck: false, consumer);
                _logger.LogInformation("Log ingestion consumer started for queue {Queue}", LogIngestionQueueName);

                while (_connection?.IsOpen == true && !stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RabbitMQ connection error; reconnecting in 10s");
                await Task.Delay(10000, stoppingToken);
            }
            finally
            {
                _channel?.Dispose();
                _connection?.Dispose();
            }
        }
    }

    private async Task ProcessMessageAsync(byte[] body, ulong deliveryTag)
    {
        LogEventMessage? logEventMessage;
        try
        {
            logEventMessage = JsonSerializer.Deserialize<LogEventMessage>(body);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize log event message");
            Ack(deliveryTag);
            return;
        }

        if (logEventMessage == null)
        {
            _logger.LogWarning("Received null log event, skipping");
            Ack(deliveryTag);
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SystemIntelligencePlatformDbContext>();
        var aiAnalyzer = scope.ServiceProvider.GetRequiredService<IIncidentAiAnalyzer>();
        var anomalyDetection = scope.ServiceProvider.GetRequiredService<AnomalyDetectionService>();

        var hashSignature = ComputeHashSignature(logEventMessage);

        var logEvent = new LogEvent(
            Guid.NewGuid(),
            logEventMessage.ApplicationId,
            logEventMessage.Level,
            logEventMessage.Message,
            hashSignature,
            logEventMessage.Timestamp,
            logEventMessage.TenantId)
        {
            Source = logEventMessage.Source,
            ExceptionType = logEventMessage.ExceptionType,
            StackTrace = logEventMessage.StackTrace,
            CorrelationId = logEventMessage.CorrelationId
        };

        dbContext.LogEvents.Add(logEvent);
        await dbContext.SaveChangesAsync();

        _logger.LogInformation("Stored LogEvent {LogEventId} for Application {ApplicationId}", logEvent.Id, logEvent.ApplicationId);

        var now = DateTime.UtcNow;
        var fiveMinAgo = now.AddMinutes(-5);
        var oneHourAgo = now.AddHours(-1);
        var oneDayAgo = now.AddDays(-1);
        var sevenDaysAgo = now.AddDays(-7);

        var baseQuery = dbContext.LogEvents.AsNoTracking()
            .Where(e => e.HashSignature == hashSignature && e.ApplicationId == logEventMessage.ApplicationId);

        var metrics = new AnomalyMetrics
        {
            EventsLast5Min = await baseQuery.CountAsync(e => e.Timestamp >= fiveMinAgo),
            EventsLast1Hour = await baseQuery.CountAsync(e => e.Timestamp >= oneHourAgo),
            EventsLast24Hours = await baseQuery.CountAsync(e => e.Timestamp >= oneDayAgo)
        };

        var hourlyCounts = await baseQuery
            .Where(e => e.Timestamp >= sevenDaysAgo)
            .GroupBy(e => new { e.Timestamp.Date, e.Timestamp.Hour })
            .Select(g => g.Count())
            .ToListAsync();

        if (hourlyCounts.Count > 0)
        {
            metrics.AverageHourlyBaseline = hourlyCounts.Average();
            var sumSquares = hourlyCounts.Sum(c => Math.Pow(c - metrics.AverageHourlyBaseline, 2));
            metrics.StandardDeviation = Math.Sqrt(sumSquares / hourlyCounts.Count);
        }

        var anomalyResult = anomalyDetection.Evaluate(metrics, logEventMessage.Level);
        if (!anomalyResult.ShouldTrigger)
        {
            Ack(deliveryTag);
            return;
        }

        _logger.LogInformation("Anomaly detected: {Reason}, Severity={Severity}, Hash={Hash}",
            anomalyResult.Reason, anomalyResult.SuggestedSeverity, hashSignature);

        var existingIncident = await dbContext.Incidents
            .FirstOrDefaultAsync(i => i.HashSignature == hashSignature
                && i.ApplicationId == logEventMessage.ApplicationId
                && i.Status != IncidentStatus.Resolved
                && i.Status != IncidentStatus.Closed);

        Incident incident;
        bool isNew;

        if (existingIncident != null)
        {
            existingIncident.IncrementOccurrence(logEventMessage.Timestamp);
            incident = existingIncident;
            isNew = false;
        }
        else
        {
            incident = new Incident(
                Guid.NewGuid(),
                logEventMessage.ApplicationId,
                TruncateTitle(logEventMessage.Message),
                hashSignature,
                anomalyResult.SuggestedSeverity,
                logEventMessage.Timestamp,
                logEventMessage.TenantId)
            {
                Description = logEventMessage.StackTrace ?? logEventMessage.Message,
                OccurrenceCount = metrics.EventsLast1Hour
            };
            dbContext.Incidents.Add(incident);
            isNew = true;
        }

        logEvent.IncidentId = incident.Id;

        var recentMessages = await dbContext.LogEvents
            .AsNoTracking()
            .Where(e => e.HashSignature == hashSignature && e.ApplicationId == logEventMessage.ApplicationId)
            .OrderByDescending(e => e.Timestamp)
            .Take(5)
            .Select(e => e.Message)
            .ToListAsync();

        var aiResult = await aiAnalyzer.AnalyzeAsync(recentMessages);
        incident.EnrichWithAiAnalysis(aiResult);

        await dbContext.SaveChangesAsync();

        var appName = await dbContext.MonitoredApplications
            .AsNoTracking()
            .Where(a => a.Id == logEventMessage.ApplicationId)
            .Select(a => a.Name)
            .FirstOrDefaultAsync() ?? "Unknown";

        var notification = new IncidentNotification
        {
            IncidentId = incident.Id,
            Title = incident.Title,
            Severity = incident.Severity.ToString(),
            Status = incident.Status.ToString(),
            ApplicationName = appName,
            OccurrenceCount = incident.OccurrenceCount,
            Timestamp = incident.LastOccurrence
        };

        PublishNotification(isNew ? "IncidentCreated" : "IncidentUpdated", incident.TenantId, notification);

        _logger.LogInformation("Incident {IncidentId} processed: isNew={IsNew}, severity={Severity}",
            incident.Id, isNew, incident.Severity);
        Ack(deliveryTag);
    }

    private void PublishNotification(string eventType, Guid? tenantId, IncidentNotification notification)
    {
        if (_channel?.IsOpen != true) return;
        try
        {
            var payload = JsonSerializer.Serialize(new { EventType = eventType, TenantId = tenantId, Notification = notification });
            var body = Encoding.UTF8.GetBytes(payload);
            var props = _channel.CreateBasicProperties();
            props.ContentType = "application/json";
            props.Persistent = true;
            _channel.BasicPublish("", IncidentNotificationsQueueName, props, body);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to publish incident notification");
        }
    }

    private void Ack(ulong deliveryTag)
    {
        try
        {
            _channel?.BasicAck(deliveryTag, false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to ack message {DeliveryTag}", deliveryTag);
        }
    }

    private static string ComputeHashSignature(LogEventMessage msg)
    {
        var input = $"{msg.Message?.Substring(0, Math.Min(msg.Message?.Length ?? 0, 200))}|{msg.Source}|{msg.ExceptionType}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string TruncateTitle(string message)
    {
        if (string.IsNullOrEmpty(message)) return string.Empty;
        if (message.Length <= IncidentConsts.MaxTitleLength) return message;
        return message[..IncidentConsts.MaxTitleLength];
    }
}
