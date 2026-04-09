using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SystemIntelligencePlatform.LogEvents;

public interface ILogSearchQuery
{
    Task<(IReadOnlyList<Guid> Ids, int TotalCount)> SearchAsync(
        string? fullTextQuery,
        string? containsFallback,
        Guid? applicationId,
        LogLevel? minLevel,
        DateTime? fromUtc,
        DateTime? toUtc,
        int skip,
        int take,
        CancellationToken cancellationToken = default);
}
