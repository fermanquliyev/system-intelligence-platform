using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace SystemIntelligencePlatform.LogEvents;

public interface ILogEventRepository : IRepository<LogEvent, Guid>
{
    Task BulkInsertAsync(
        IEnumerable<LogEvent> logEvents,
        CancellationToken cancellationToken = default);

    Task<int> GetCountByHashSignatureAsync(
        string hashSignature,
        Guid applicationId,
        TimeSpan window,
        CancellationToken cancellationToken = default);

    Task<List<LogEvent>> GetRecentByHashSignatureAsync(
        string hashSignature,
        Guid applicationId,
        int maxCount = 5,
        CancellationToken cancellationToken = default);
}
