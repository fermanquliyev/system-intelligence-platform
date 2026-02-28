using System;
using SystemIntelligencePlatform.Incidents;
using Shouldly;
using Xunit;

namespace SystemIntelligencePlatform.Incidents;

/// <summary>
/// Tests Incident entity behavior: escalation rules, severity upgrades,
/// AI enrichment, and resolve logic. Pure unit tests, no DB needed.
/// </summary>
public class IncidentEntity_Tests
{
    private Incident CreateIncident(IncidentSeverity severity = IncidentSeverity.Low, int occurrenceCount = 1)
    {
        var incident = new Incident(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Test incident",
            "abc123hash",
            severity,
            DateTime.UtcNow);
        // Set occurrence count by incrementing
        for (int i = 1; i < occurrenceCount; i++)
            incident.IncrementOccurrence(DateTime.UtcNow);
        return incident;
    }

    // --- Escalation Rules ---

    [Fact]
    public void IncrementOccurrence_Below10_ShouldNotEscalate()
    {
        // Arrange
        var incident = CreateIncident(IncidentSeverity.Low, 1);

        // Act
        for (int i = 0; i < 8; i++)
            incident.IncrementOccurrence(DateTime.UtcNow);

        // Assert: 9 total occurrences (1 initial + 8), below Medium threshold of 10
        incident.OccurrenceCount.ShouldBe(9);
        incident.Severity.ShouldBe(IncidentSeverity.Low);
    }

    [Fact]
    public void IncrementOccurrence_At10_ShouldEscalateToMedium()
    {
        // Arrange
        var incident = CreateIncident(IncidentSeverity.Low, 1);

        // Act: reach 10 occurrences
        for (int i = 0; i < 9; i++)
            incident.IncrementOccurrence(DateTime.UtcNow);

        // Assert
        incident.OccurrenceCount.ShouldBe(10);
        incident.Severity.ShouldBe(IncidentSeverity.Medium);
    }

    [Fact]
    public void IncrementOccurrence_At50_ShouldEscalateToHigh()
    {
        var incident = CreateIncident(IncidentSeverity.Low, 1);

        for (int i = 0; i < 49; i++)
            incident.IncrementOccurrence(DateTime.UtcNow);

        incident.OccurrenceCount.ShouldBe(50);
        incident.Severity.ShouldBe(IncidentSeverity.High);
    }

    [Fact]
    public void IncrementOccurrence_At100_ShouldEscalateToCritical()
    {
        var incident = CreateIncident(IncidentSeverity.Low, 1);

        for (int i = 0; i < 99; i++)
            incident.IncrementOccurrence(DateTime.UtcNow);

        incident.OccurrenceCount.ShouldBe(100);
        incident.Severity.ShouldBe(IncidentSeverity.Critical);
    }

    [Fact]
    public void IncrementOccurrence_ShouldUpdateLastOccurrence()
    {
        var incident = CreateIncident();
        var newTime = DateTime.UtcNow.AddMinutes(5);

        incident.IncrementOccurrence(newTime);

        incident.LastOccurrence.ShouldBe(newTime);
    }

    // --- Resolve Logic ---

    [Fact]
    public void Resolve_ShouldSetStatusAndTimestamp()
    {
        var incident = CreateIncident();
        var userId = Guid.NewGuid();

        incident.Resolve(userId);

        incident.Status.ShouldBe(IncidentStatus.Resolved);
        incident.ResolvedByUserId.ShouldBe(userId);
        incident.ResolvedAt.ShouldNotBeNull();
    }

    // --- AI Enrichment ---

    [Fact]
    public void EnrichWithAiAnalysis_ShouldSetAllFields()
    {
        var incident = CreateIncident();

        incident.EnrichWithAiAnalysis(0.75, "error, timeout", "Server:Technology");

        incident.SentimentScore.ShouldBe(0.75);
        incident.KeyPhrases.ShouldBe("error, timeout");
        incident.Entities.ShouldBe("Server:Technology");
        incident.AiAnalyzedAt.ShouldNotBeNull();
    }
}
