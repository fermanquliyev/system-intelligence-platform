using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using SystemIntelligencePlatform.InstanceConfiguration;
using SystemIntelligencePlatform.LogEvents;
using Volo.Abp.DependencyInjection;

namespace SystemIntelligencePlatform.Infrastructure;

public class RabbitMqLogEventPublisher : ILogEventPublisher, ISingletonDependency, IAsyncDisposable
{
    public const string QueueName = "log-ingestion";
    private const int MaxPublishRetries = 3;
    private const int RetryDelayMs = 500;

    private readonly IOptions<RabbitMqOptions> _fileOptions;
    private readonly IInstanceConfigurationProvider _instanceConfiguration;
    private readonly ILogger<RabbitMqLogEventPublisher> _logger;
    private IConnection? _connection;
    private IModel? _channel;
    private string _connectionFingerprint = "";
    private readonly object _lock = new();
    private bool _disposed;

    public RabbitMqLogEventPublisher(
        IOptions<RabbitMqOptions> fileOptions,
        IInstanceConfigurationProvider instanceConfiguration,
        ILogger<RabbitMqLogEventPublisher> logger)
    {
        _fileOptions = fileOptions;
        _instanceConfiguration = instanceConfiguration;
        _logger = logger;
    }

    private IModel GetOrCreateChannel()
    {
        var options = EffectiveConfigurationBinder.GetRabbitMq(_instanceConfiguration, _fileOptions);
        var fingerprint = $"{options.Host}|{options.Port}|{options.Username}|{options.VirtualHost}|{options.Password}";

        if (_channel?.IsOpen == true && fingerprint == _connectionFingerprint)
            return _channel;

        lock (_lock)
        {
            if (_channel?.IsOpen == true && fingerprint == _connectionFingerprint)
                return _channel;

            if (fingerprint != _connectionFingerprint)
            {
                try
                {
                    _channel?.Dispose();
                    _connection?.Dispose();
                }
                catch { /* ignore */ }
                _channel = null;
                _connection = null;
                _connectionFingerprint = fingerprint;
            }

            if (_connection?.IsOpen != true)
            {
                var factory = new ConnectionFactory
                {
                    HostName = options.Host,
                    Port = options.Port,
                    UserName = options.Username,
                    Password = options.Password,
                    VirtualHost = string.IsNullOrEmpty(options.VirtualHost) ? "/" : options.VirtualHost,
                    AutomaticRecoveryEnabled = true,
                    NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
                    RequestedHeartbeat = TimeSpan.FromSeconds(60)
                };
                _connection = factory.CreateConnection();
                _logger.LogInformation("RabbitMQ connection established to {Host}:{Port}", options.Host, options.Port);
            }

            if (_channel?.IsOpen != true)
            {
                _channel = _connection.CreateModel();
                _channel.QueueDeclare(QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            }

            return _channel;
        }
    }

    public async Task PublishAsync(LogEventMessage message)
    {
        if (message == null)
            throw new ArgumentNullException(nameof(message));

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        Exception? lastEx = null;
        for (var attempt = 1; attempt <= MaxPublishRetries; attempt++)
        {
            try
            {
                var channel = GetOrCreateChannel();
                var props = channel.CreateBasicProperties();
                props.ContentType = "application/json";
                props.MessageId = Guid.NewGuid().ToString();
                props.CorrelationId = message.CorrelationId ?? Guid.NewGuid().ToString();
                props.Persistent = true;

                channel.BasicPublish("", QueueName, props, body);
                return;
            }
            catch (Exception ex)
            {
                lastEx = ex;
                _logger.LogWarning(ex, "Publish attempt {Attempt}/{Max} failed for log event", attempt, MaxPublishRetries);
                if (_channel?.IsOpen == true)
                {
                    try { _channel?.Dispose(); } catch { /* ignore */ }
                    _channel = null;
                }
                if (attempt < MaxPublishRetries)
                    await Task.Delay(RetryDelayMs * attempt);
            }
        }

        _logger.LogError(lastEx, "Failed to publish log event to RabbitMQ queue {QueueName} after {Max} attempts", QueueName, MaxPublishRetries);
        throw lastEx ?? new InvalidOperationException("Publish failed");
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        lock (_lock)
        {
            if (_disposed) return;
            try
            {
                _channel?.Dispose();
                _connection?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error disposing RabbitMQ connection");
            }
            _channel = null;
            _connection = null;
            _disposed = true;
        }
        await Task.CompletedTask;
    }
}
