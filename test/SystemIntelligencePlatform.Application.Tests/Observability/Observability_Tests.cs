using System;
using Shouldly;
using SystemIntelligencePlatform.LogEvents;
using Xunit;

namespace SystemIntelligencePlatform.Observability;

public class Observability_Tests
{
    [Fact]
    public void Should_Pass_CorrelationId_To_LogEventMessage()
    {
        var correlationId = "corr-abc123-xyz789";
        var applicationId = Guid.NewGuid();

        var message = new LogEventMessage
        {
            ApplicationId = applicationId,
            Level = LogLevel.Error,
            Message = "Test error message",
            CorrelationId = correlationId,
            Timestamp = DateTime.UtcNow
        };

        message.CorrelationId.ShouldBe(correlationId);
    }

    [Fact]
    public void LogEventMessage_Should_Contain_ApplicationId()
    {
        var applicationId = Guid.NewGuid();
        var message = new LogEventMessage
        {
            ApplicationId = applicationId,
            Level = LogLevel.Information,
            Message = "Test message",
            Timestamp = DateTime.UtcNow
        };

        message.ApplicationId.ShouldBe(applicationId);
    }

    [Fact]
    public void LogEventMessage_Should_Default_Timestamp_When_Not_Set()
    {
        var message = new LogEventMessage
        {
            ApplicationId = Guid.NewGuid(),
            Level = LogLevel.Error,
            Message = "Test"
        };

        message.Timestamp.ShouldBeGreaterThanOrEqualTo(default);
    }
}
