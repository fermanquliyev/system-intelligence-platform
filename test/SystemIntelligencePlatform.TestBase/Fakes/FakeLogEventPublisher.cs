using System.Collections.Generic;
using System.Threading.Tasks;
using SystemIntelligencePlatform.LogEvents;
using Volo.Abp.DependencyInjection;

namespace SystemIntelligencePlatform.Fakes;

/// <summary>
/// In-memory fake for ILogEventPublisher used during tests.
/// Captures published messages for assertion.
/// </summary>
[Dependency(ReplaceServices = true)]
public class FakeLogEventPublisher : ILogEventPublisher, ITransientDependency
{
    public List<LogEventMessage> PublishedMessages { get; } = new();

    public Task PublishAsync(LogEventMessage message)
    {
        PublishedMessages.Add(message);
        return Task.CompletedTask;
    }
}
