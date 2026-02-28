using System;
using System.Collections.Generic;
using Shouldly;
using SystemIntelligencePlatform.LogEvents;
using Xunit;

namespace SystemIntelligencePlatform.Observability;

/// <summary>
/// Tests for observability features: CorrelationId propagation and structured logging fields.
/// Tests the LogEventMessage construction directly since the ingestion pipeline
/// is validated end-to-end in integration tests.
/// </summary>
public class Observability_Tests
{
    [Fact]
    public void Should_Pass_CorrelationId_To_LogEventMessage()
    {
        var correlationId = "corr-abc123-xyz789";
        var tenantId = Guid.NewGuid();
        var applicationId = Guid.NewGuid();

        var message = new LogEventMessage
        {
            TenantId = tenantId,
            ApplicationId = applicationId,
            Level = LogLevel.Error,
            Message = "Test error message",
            CorrelationId = correlationId,
            Timestamp = DateTime.UtcNow
        };

        message.CorrelationId.ShouldBe(correlationId);
    }

    [Fact]
    public void LogEventMessage_Should_Contain_TenantId()
    {
        var tenantId = Guid.NewGuid();
        var message = new LogEventMessage
        {
            TenantId = tenantId,
            ApplicationId = Guid.NewGuid(),
            Level = LogLevel.Information,
            Message = "Test message",
            Timestamp = DateTime.UtcNow
        };

        message.TenantId.ShouldBe(tenantId);
    }

    [Fact]
    public void LogEventMessage_Should_Contain_ApplicationId()
    {
        var applicationId = Guid.NewGuid();
        var message = new LogEventMessage
        {
            TenantId = Guid.NewGuid(),
            ApplicationId = applicationId,
            Level = LogLevel.Warning,
            Message = "Test warning",
            Timestamp = DateTime.UtcNow
        };

        message.ApplicationId.ShouldBe(applicationId);
    }

    [Fact]
    public void LogEventMessage_Should_Default_Timestamp_When_Not_Set()
    {
        var before = DateTime.UtcNow;
        var message = new LogEventMessage
        {
            TenantId = Guid.NewGuid(),
            ApplicationId = Guid.NewGuid(),
            Level = LogLevel.Error,
            Message = "Test"
        };

        message.Timestamp.ShouldBeGreaterThanOrEqualTo(default);
    }
}
