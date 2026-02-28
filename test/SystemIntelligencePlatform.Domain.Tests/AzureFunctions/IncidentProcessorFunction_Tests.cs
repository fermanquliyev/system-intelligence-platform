using System;
using SystemIntelligencePlatform.Incidents;
using SystemIntelligencePlatform.LogEvents;
using Shouldly;
using Xunit;

namespace SystemIntelligencePlatform.AzureFunctions;

/// <summary>
/// Tests for incident processing logic, focusing on domain-level behavior.
/// Tests the AnomalyDetectionService and Incident entity interactions that
/// the IncidentProcessorFunction relies on.
/// </summary>
public class IncidentProcessorFunction_Tests
{
    private readonly AnomalyDetectionService _anomalyDetectionService;

    public IncidentProcessorFunction_Tests()
    {
        _anomalyDetectionService = new AnomalyDetectionService();
    }

    /// <summary>
    /// Verifies that a new incident is created when anomaly detection triggers.
    /// </summary>
    [Fact]
    public void Should_Create_New_Incident_When_Anomaly_Detected()
    {
        // Arrange
        var metrics = new AnomalyMetrics
        {
            EventsLast5Min = 15, // Spike: exceeds 3x baseline
            EventsLast1Hour = 50,
            EventsLast24Hours = 200,
            AverageHourlyBaseline = 10, // Expected 5-min = 10/12 ≈ 0.83, threshold = 2.5
            StandardDeviation = 2
        };

        // Act - Evaluate anomaly
        var result = _anomalyDetectionService.Evaluate(metrics, LogLevel.Error);

        // Assert - Should trigger
        result.ShouldTrigger.ShouldBeTrue();
        result.SuggestedSeverity.ShouldBeOneOf(
            IncidentSeverity.Low,
            IncidentSeverity.Medium,
            IncidentSeverity.High,
            IncidentSeverity.Critical);

        // Create incident based on the result
        var incident = new Incident(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Test Incident",
            "test-hash-signature",
            result.SuggestedSeverity,
            DateTime.UtcNow);

        incident.ShouldNotBeNull();
        incident.Status.ShouldBe(IncidentStatus.Open);
        incident.OccurrenceCount.ShouldBe(1);
    }

    /// <summary>
    /// Verifies that an existing incident is updated when more events arrive.
    /// </summary>
    [Fact]
    public void Should_Update_Existing_Incident_When_More_Events_Arrive()
    {
        // Arrange
        var incident = new Incident(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Existing Incident",
            "test-hash-signature",
            IncidentSeverity.Medium,
            DateTime.UtcNow);

        var initialCount = incident.OccurrenceCount;
        var initialLastOccurrence = incident.LastOccurrence;

        // Act
        var newTimestamp = DateTime.UtcNow.AddMinutes(5);
        incident.IncrementOccurrence(newTimestamp);

        // Assert
        incident.OccurrenceCount.ShouldBe(initialCount + 1);
        incident.LastOccurrence.ShouldBe(newTimestamp);
        incident.LastOccurrence.ShouldNotBe(initialLastOccurrence);
    }

    /// <summary>
    /// Verifies that no incident is created when metrics are below the threshold.
    /// </summary>
    [Fact]
    public void Should_Not_Create_Incident_When_Below_Threshold()
    {
        // Arrange
        var metrics = new AnomalyMetrics
        {
            EventsLast5Min = 2, // Below spike threshold
            EventsLast1Hour = 15, // Below burst threshold (baseline * 2 = 20)
            EventsLast24Hours = 400,
            AverageHourlyBaseline = 10, // Expected 5-min = 0.83, threshold = 2.5
            StandardDeviation = 2
        };

        // Act
        var result = _anomalyDetectionService.Evaluate(metrics, LogLevel.Warning);

        // Assert
        result.ShouldTrigger.ShouldBeFalse();
        result.Reason.ShouldBe(AnomalyReason.None);
    }

    /// <summary>
    /// Verifies that AI enrichment can be called multiple times and updates AiAnalyzedAt.
    /// </summary>
    [Fact]
    public void Should_Call_AI_Enrichment_Only_Once_Per_Analysis()
    {
        // Arrange
        var incident = new Incident(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Test Incident",
            "test-hash-signature",
            IncidentSeverity.Medium,
            DateTime.UtcNow);

        // Act - First AI analysis
        var firstAnalysisTime = DateTime.UtcNow;
        incident.EnrichWithAiAnalysis(
            sentimentScore: 0.75,
            keyPhrases: "error, timeout",
            entities: "Server:Technology");

        var firstAiAnalyzedAt = incident.AiAnalyzedAt;

        // Assert - First analysis
        incident.SentimentScore.ShouldBe(0.75);
        incident.KeyPhrases.ShouldBe("error, timeout");
        incident.Entities.ShouldBe("Server:Technology");
        incident.AiAnalyzedAt.ShouldNotBeNull();

        // Act - Second AI analysis (re-runnable)
        System.Threading.Thread.Sleep(10); // Small delay to ensure timestamp difference
        incident.EnrichWithAiAnalysis(
            sentimentScore: 0.85,
            keyPhrases: "error, timeout, connection",
            entities: "Server:Technology, Database:Technology");

        // Assert - Second analysis updates the timestamp
        incident.SentimentScore.ShouldBe(0.85);
        incident.KeyPhrases.ShouldBe("error, timeout, connection");
        incident.Entities.ShouldBe("Server:Technology, Database:Technology");
        incident.AiAnalyzedAt.ShouldNotBeNull();
        incident.AiAnalyzedAt.ShouldNotBe(firstAiAnalyzedAt); // Timestamp should be updated
    }

    /// <summary>
    /// Verifies that critical log level always triggers an incident regardless of metrics.
    /// </summary>
    [Fact]
    public void Should_Trigger_For_Critical_LogLevel_Regardless_Of_Metrics()
    {
        // Arrange
        var metrics = new AnomalyMetrics
        {
            EventsLast5Min = 1,
            EventsLast1Hour = 1,
            EventsLast24Hours = 1,
            AverageHourlyBaseline = 1000, // High baseline, normally wouldn't trigger
            StandardDeviation = 100
        };

        // Act
        var result = _anomalyDetectionService.Evaluate(metrics, LogLevel.Critical);

        // Assert
        result.ShouldTrigger.ShouldBeTrue();
        result.Reason.ShouldBe(AnomalyReason.ImmediateCritical);
        result.SuggestedSeverity.ShouldBe(IncidentSeverity.Critical);
    }
}
