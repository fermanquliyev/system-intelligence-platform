using System.Threading;
using System.Threading.Tasks;

namespace SystemIntelligencePlatform.LogSources;

/// <summary>Pluggable log ingestion adapter (file tail, HTTP, syslog). Host registers implementations.</summary>
public interface ILogSourceAdapter
{
    LogSourceType SourceType { get; }

    Task StartAsync(LogSourceConfiguration configuration, CancellationToken cancellationToken = default);

    Task StopAsync(CancellationToken cancellationToken = default);
}
