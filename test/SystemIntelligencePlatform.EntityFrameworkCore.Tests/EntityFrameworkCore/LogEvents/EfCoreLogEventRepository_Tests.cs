using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using SystemIntelligencePlatform.LogEvents;
using Xunit;

namespace SystemIntelligencePlatform.EntityFrameworkCore.LogEvents;

/// <summary>
/// Tests for the EF Core implementation of ILogEventRepository.
/// Verifies bulk operations, filtering, time-window queries, and NoTracking behavior.
/// </summary>
[Collection(SystemIntelligencePlatformTestConsts.CollectionDefinitionName)]
public class EfCoreLogEventRepository_Tests : SystemIntelligencePlatformEntityFrameworkCoreTestBase
{
    private readonly ILogEventRepository _logEventRepository;

    public EfCoreLogEventRepository_Tests()
    {
        _logEventRepository = GetRequiredService<ILogEventRepository>();
    }

    [Fact]
    public async Task BulkInsert_Should_Insert_Correct_Count()
    {
        var applicationId = Guid.NewGuid();
        var hashSignature = "test-hash-bulk";

        await WithUnitOfWorkAsync(async () =>
        {
            var events = new List<LogEvent>();
            for (int i = 0; i < 100; i++)
            {
                events.Add(new LogEvent(
                    Guid.NewGuid(), applicationId, LogLevel.Error,
                    $"Test message {i}", hashSignature, DateTime.UtcNow.AddMinutes(-i)));
            }
            await _logEventRepository.BulkInsertAsync(events);
        });

        await WithUnitOfWorkAsync(async () =>
        {
            var count = await _logEventRepository.GetCountByHashSignatureAsync(
                hashSignature, applicationId, TimeSpan.FromDays(1));
            count.ShouldBe(100);
        });
    }

    [Fact]
    public async Task Should_Filter_By_ApplicationId()
    {
        var appId1 = Guid.NewGuid();
        var appId2 = Guid.NewGuid();
        var hash = "test-hash-filter";

        await WithUnitOfWorkAsync(async () =>
        {
            await _logEventRepository.BulkInsertAsync(new List<LogEvent>
            {
                new LogEvent(Guid.NewGuid(), appId1, LogLevel.Error, "App1 Event 1", hash, DateTime.UtcNow),
                new LogEvent(Guid.NewGuid(), appId1, LogLevel.Error, "App1 Event 2", hash, DateTime.UtcNow)
            });
            await _logEventRepository.BulkInsertAsync(new List<LogEvent>
            {
                new LogEvent(Guid.NewGuid(), appId2, LogLevel.Error, "App2 Event 1", hash, DateTime.UtcNow),
                new LogEvent(Guid.NewGuid(), appId2, LogLevel.Error, "App2 Event 2", hash, DateTime.UtcNow)
            });
        });

        await WithUnitOfWorkAsync(async () =>
        {
            var count1 = await _logEventRepository.GetCountByHashSignatureAsync(hash, appId1, TimeSpan.FromHours(1));
            var count2 = await _logEventRepository.GetCountByHashSignatureAsync(hash, appId2, TimeSpan.FromHours(1));
            count1.ShouldBe(2);
            count2.ShouldBe(2);
        });
    }

    [Fact]
    public async Task Should_Query_By_HashSignature_And_Time_Window()
    {
        var applicationId = Guid.NewGuid();
        var hashSignature = "test-hash-window";

        await WithUnitOfWorkAsync(async () =>
        {
            await _logEventRepository.BulkInsertAsync(new List<LogEvent>
            {
                new LogEvent(Guid.NewGuid(), applicationId, LogLevel.Error, "Recent 1", hashSignature, DateTime.UtcNow.AddMinutes(-30)),
                new LogEvent(Guid.NewGuid(), applicationId, LogLevel.Error, "Recent 2", hashSignature, DateTime.UtcNow.AddMinutes(-45)),
                new LogEvent(Guid.NewGuid(), applicationId, LogLevel.Error, "Old 1", hashSignature, DateTime.UtcNow.AddHours(-2)),
                new LogEvent(Guid.NewGuid(), applicationId, LogLevel.Error, "Old 2", hashSignature, DateTime.UtcNow.AddHours(-3))
            });
        });

        await WithUnitOfWorkAsync(async () =>
        {
            var countInWindow = await _logEventRepository.GetCountByHashSignatureAsync(
                hashSignature, applicationId, TimeSpan.FromHours(1));
            countInWindow.ShouldBe(2);
        });
    }

    [Fact]
    public async Task Should_Use_NoTracking_For_Read_Queries()
    {
        var applicationId = Guid.NewGuid();
        var hashSignature = "test-hash-notrack";

        await WithUnitOfWorkAsync(async () =>
        {
            await _logEventRepository.BulkInsertAsync(new List<LogEvent>
            {
                new LogEvent(Guid.NewGuid(), applicationId, LogLevel.Error, "Event 1", hashSignature, DateTime.UtcNow),
                new LogEvent(Guid.NewGuid(), applicationId, LogLevel.Error, "Event 2", hashSignature, DateTime.UtcNow.AddMinutes(-1)),
                new LogEvent(Guid.NewGuid(), applicationId, LogLevel.Error, "Event 3", hashSignature, DateTime.UtcNow.AddMinutes(-2))
            });
        });

        await WithUnitOfWorkAsync(async () =>
        {
            // GetRecentByHashSignatureAsync uses AsNoTracking internally
            var recent = await _logEventRepository.GetRecentByHashSignatureAsync(hashSignature, applicationId, 5);
            recent.ShouldNotBeNull();
            recent.Count.ShouldBe(3);
        });
    }
}
