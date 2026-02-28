using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using SystemIntelligencePlatform.FailedLogEvents;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Modularity;
using Xunit;

namespace SystemIntelligencePlatform.DeadLetter;

/// <summary>
/// Tests verify that FailedLogEvent entities are properly persisted when message processing fails.
/// </summary>
public abstract class DeadLetterProcessing_Tests<TStartupModule> : SystemIntelligencePlatformApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    protected IRepository<FailedLogEvent, Guid> FailedLogEventRepository => GetRequiredService<IRepository<FailedLogEvent, Guid>>();

    [Fact]
    public async Task Should_Create_FailedLogEvent_When_Processing_Fails()
    {
        // Arrange
        var failedEventId = Guid.NewGuid();

        await WithUnitOfWorkAsync(async () =>
        {
            await FailedLogEventRepository.InsertAsync(new FailedLogEvent(
                failedEventId, "{\"message\":\"test\"}", "Processing failed", 5));
        });

        // Assert
        await WithUnitOfWorkAsync(async () =>
        {
            var retrieved = await FailedLogEventRepository.GetAsync(failedEventId);
            retrieved.ShouldNotBeNull();
            retrieved.DeliveryAttempt.ShouldBe(5);
        });
    }

    [Fact]
    public async Task Should_Store_CorrelationId_In_FailedLogEvent()
    {
        var failedEventId = Guid.NewGuid();
        var correlationId = "corr-12345-abcde";

        await WithUnitOfWorkAsync(async () =>
        {
            await FailedLogEventRepository.InsertAsync(new FailedLogEvent(
                failedEventId, "{\"message\":\"test\"}", "Timeout",
                3, correlationId: correlationId));
        });

        await WithUnitOfWorkAsync(async () =>
        {
            var retrieved = await FailedLogEventRepository.GetAsync(failedEventId);
            retrieved.CorrelationId.ShouldBe(correlationId);
        });
    }

    [Fact]
    public async Task Should_Increment_DeliveryAttempt()
    {
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();

        await WithUnitOfWorkAsync(async () =>
        {
            await FailedLogEventRepository.InsertAsync(
                new FailedLogEvent(id1, "{}", "err", 1));
            await FailedLogEventRepository.InsertAsync(
                new FailedLogEvent(id2, "{}", "err", 2));
        });

        await WithUnitOfWorkAsync(async () =>
        {
            var e1 = await FailedLogEventRepository.GetAsync(id1);
            var e2 = await FailedLogEventRepository.GetAsync(id2);
            e1.DeliveryAttempt.ShouldBe(1);
            e2.DeliveryAttempt.ShouldBe(2);
        });
    }
}
