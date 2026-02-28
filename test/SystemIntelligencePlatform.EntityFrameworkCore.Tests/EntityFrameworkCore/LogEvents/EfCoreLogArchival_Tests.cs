using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shouldly;
using SystemIntelligencePlatform.Incidents;
using SystemIntelligencePlatform.LogEvents;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace SystemIntelligencePlatform.EntityFrameworkCore.LogEvents;

/// <summary>
/// Tests for log event archival operations.
/// Verifies that old log events can be identified, counted, and deleted in batches,
/// while preserving related incident data.
/// </summary>
[Collection(SystemIntelligencePlatformTestConsts.CollectionDefinitionName)]
public class EfCoreLogArchival_Tests : SystemIntelligencePlatformEntityFrameworkCoreTestBase
{
    private readonly ILogEventRepository _logEventRepository;
    private readonly IRepository<Incident, Guid> _incidentRepository;

    public EfCoreLogArchival_Tests()
    {
        _logEventRepository = GetRequiredService<ILogEventRepository>();
        _incidentRepository = GetRequiredService<IRepository<Incident, Guid>>();
    }

    [Fact]
    public async Task Should_Find_LogEvents_Older_Than_30_Days()
    {
        var applicationId = Guid.NewGuid();
        var hash = "test-hash-archival";
        var cutoff = DateTime.UtcNow.AddDays(-30);

        await WithUnitOfWorkAsync(async () =>
        {
            await _logEventRepository.BulkInsertAsync(new List<LogEvent>
            {
                new LogEvent(Guid.NewGuid(), applicationId, LogLevel.Error, "Old 1", hash, DateTime.UtcNow.AddDays(-40)),
                new LogEvent(Guid.NewGuid(), applicationId, LogLevel.Error, "Old 2", hash, DateTime.UtcNow.AddDays(-45)),
                new LogEvent(Guid.NewGuid(), applicationId, LogLevel.Error, "Recent 1", hash, DateTime.UtcNow.AddDays(-5)),
                new LogEvent(Guid.NewGuid(), applicationId, LogLevel.Error, "Recent 2", hash, DateTime.UtcNow.AddDays(-10))
            });
        });

        await WithUnitOfWorkAsync(async () =>
        {
            var old = await _logEventRepository.GetOlderThanAsync(cutoff, 100);
            old.Count.ShouldBe(2);
        });
    }

    [Fact]
    public async Task Should_Delete_Batch_Of_Old_Events()
    {
        var applicationId = Guid.NewGuid();
        var hash = "test-hash-delete";
        var e1 = Guid.NewGuid();
        var e2 = Guid.NewGuid();
        var e3 = Guid.NewGuid();

        await WithUnitOfWorkAsync(async () =>
        {
            await _logEventRepository.BulkInsertAsync(new List<LogEvent>
            {
                new LogEvent(e1, applicationId, LogLevel.Error, "Old 1", hash, DateTime.UtcNow.AddDays(-40)),
                new LogEvent(e2, applicationId, LogLevel.Error, "Old 2", hash, DateTime.UtcNow.AddDays(-45)),
                new LogEvent(e3, applicationId, LogLevel.Error, "Old 3", hash, DateTime.UtcNow.AddDays(-50))
            });
        });

        await WithUnitOfWorkAsync(async () =>
        {
            await _logEventRepository.DeleteBatchAsync(new[] { e1, e2 });
        });

        await WithUnitOfWorkAsync(async () =>
        {
            var remaining = await _logEventRepository.GetListAsync(e => e.ApplicationId == applicationId);
            remaining.Count.ShouldBe(1);
            remaining[0].Id.ShouldBe(e3);
        });
    }

    [Fact]
    public async Task Should_Not_Affect_Incident_Data()
    {
        var applicationId = Guid.NewGuid();
        var hash = "test-hash-inc";
        var incidentId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        await WithUnitOfWorkAsync(async () =>
        {
            await _incidentRepository.InsertAsync(new Incident(
                incidentId, applicationId, "Test Incident", hash,
                IncidentSeverity.Medium, DateTime.UtcNow.AddDays(-40)));

            await _logEventRepository.BulkInsertAsync(new List<LogEvent>
            {
                new LogEvent(eventId, applicationId, LogLevel.Error, "Old", hash, DateTime.UtcNow.AddDays(-40))
                {
                    IncidentId = incidentId
                }
            });
        });

        await WithUnitOfWorkAsync(async () =>
        {
            await _logEventRepository.DeleteBatchAsync(new[] { eventId });
        });

        await WithUnitOfWorkAsync(async () =>
        {
            var incident = await _incidentRepository.GetAsync(incidentId);
            incident.ShouldNotBeNull();
            incident.Title.ShouldBe("Test Incident");

            var logCount = await _logEventRepository.GetCountByHashSignatureAsync(hash, applicationId, TimeSpan.FromDays(365));
            logCount.ShouldBe(0);
        });
    }

    [Fact]
    public async Task Should_Count_Old_Events()
    {
        var applicationId = Guid.NewGuid();
        var hash = "test-hash-count";
        var cutoff = DateTime.UtcNow.AddDays(-30);

        await WithUnitOfWorkAsync(async () =>
        {
            await _logEventRepository.BulkInsertAsync(new List<LogEvent>
            {
                new LogEvent(Guid.NewGuid(), applicationId, LogLevel.Error, "Old 1", hash, DateTime.UtcNow.AddDays(-40)),
                new LogEvent(Guid.NewGuid(), applicationId, LogLevel.Error, "Old 2", hash, DateTime.UtcNow.AddDays(-45)),
                new LogEvent(Guid.NewGuid(), applicationId, LogLevel.Error, "Old 3", hash, DateTime.UtcNow.AddDays(-50)),
                new LogEvent(Guid.NewGuid(), applicationId, LogLevel.Error, "Recent", hash, DateTime.UtcNow.AddDays(-5))
            });
        });

        await WithUnitOfWorkAsync(async () =>
        {
            var oldCount = await _logEventRepository.GetCountOlderThanAsync(cutoff);
            oldCount.ShouldBeGreaterThanOrEqualTo(3);
        });
    }
}
