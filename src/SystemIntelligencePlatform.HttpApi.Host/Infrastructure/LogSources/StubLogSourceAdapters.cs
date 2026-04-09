using System.Threading;
using System.Threading.Tasks;
using SystemIntelligencePlatform.LogSources;
using Volo.Abp.DependencyInjection;

namespace SystemIntelligencePlatform.Infrastructure.LogSources;

/// <summary>Offline-safe stubs; replace with real file tail / HTTP / syslog listeners as needed.</summary>
public class FileLogSourceAdapterStub : ILogSourceAdapter, ITransientDependency
{
    public LogSourceType SourceType => LogSourceType.File;

    public Task StartAsync(LogSourceConfiguration configuration, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}

public class HttpLogSourceAdapterStub : ILogSourceAdapter, ITransientDependency
{
    public LogSourceType SourceType => LogSourceType.Http;

    public Task StartAsync(LogSourceConfiguration configuration, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}

public class SyslogLogSourceAdapterStub : ILogSourceAdapter, ITransientDependency
{
    public LogSourceType SourceType => LogSourceType.Syslog;

    public Task StartAsync(LogSourceConfiguration configuration, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}
