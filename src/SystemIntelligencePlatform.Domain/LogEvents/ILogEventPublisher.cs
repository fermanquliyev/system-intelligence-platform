using System.Threading.Tasks;

namespace SystemIntelligencePlatform.LogEvents;

/// <summary>
/// Publishes log events to a message queue for async processing.
/// Infrastructure layer provides the Azure Service Bus implementation.
/// </summary>
public interface ILogEventPublisher
{
    Task PublishAsync(LogEventMessage message);
}
