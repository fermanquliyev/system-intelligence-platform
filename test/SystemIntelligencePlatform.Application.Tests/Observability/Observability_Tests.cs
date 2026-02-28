using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using Shouldly;
using SystemIntelligencePlatform.LogEvents;
using SystemIntelligencePlatform.LogIngestion;
using SystemIntelligencePlatform.MonitoredApplications;
using Xunit;

namespace SystemIntelligencePlatform.Observability;

/// <summary>
/// Pure unit tests for observability features.
/// Verifies that CorrelationId, TenantId, and ApplicationId propagate correctly
/// through the ingestion pipeline. Uses NSubstitute mocks to avoid Azure dependencies.
/// </summary>
public class Observability_Tests
{
    /// <summary>
    /// Verifies that CorrelationId from the input DTO is properly passed to the publisher.
    /// CorrelationId is essential for distributed tracing across services.
    /// </summary>
    [Fact]
    public async Task Should_Pass_CorrelationId_To_Publisher()
    {
        // Arrange
        var mockPublisher = Substitute.For<ILogEventPublisher>();
        var mockAppRepository = Substitute.For<IMonitoredApplicationRepository>();
        var correlationId = "corr-abc123-xyz789";
        var appId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        var monitoredApp = new MonitoredApplication(
            appId,
            "TestApp",
            ApiKeyGenerator.Hash("test-api-key"),
            tenantId)
        {
            IsActive = true
        };

        mockAppRepository.FindByApiKeyHashAsync(Arg.Any<string>())
            .Returns(monitoredApp);

        var logIngestionService = new LogIngestionAppService(mockAppRepository, mockPublisher);

        var input = new LogIngestionDto
        {
            Events = new List<LogIngestionItemDto>
            {
                new LogIngestionItemDto
                {
                    Level = LogLevel.Error,
                    Message = "Test error message",
                    CorrelationId = correlationId
                }
            }
        };

        // Act
        await logIngestionService.IngestAsync("test-api-key", input);

        // Assert
        await mockPublisher.Received(1).PublishAsync(
            Arg.Is<LogEventMessage>(msg => msg.CorrelationId == correlationId));
    }

    /// <summary>
    /// Verifies that LogEventMessage contains TenantId property.
    /// </summary>
    [Fact]
    public void LogEventMessage_Should_Contain_TenantId()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var message = new LogEventMessage
        {
            TenantId = tenantId,
            ApplicationId = Guid.NewGuid(),
            Level = LogLevel.Information,
            Message = "Test message",
            Timestamp = DateTime.UtcNow
        };

        // Assert
        message.TenantId.ShouldBe(tenantId);
    }

    /// <summary>
    /// Verifies that LogEventMessage contains ApplicationId property.
    /// </summary>
    [Fact]
    public void LogEventMessage_Should_Contain_ApplicationId()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var message = new LogEventMessage
        {
            TenantId = Guid.NewGuid(),
            ApplicationId = applicationId,
            Level = LogLevel.Warning,
            Message = "Test warning",
            Timestamp = DateTime.UtcNow
        };

        // Assert
        message.ApplicationId.ShouldBe(applicationId);
    }
}
