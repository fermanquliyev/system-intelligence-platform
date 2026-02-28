using SystemIntelligencePlatform.Incidents;
using SystemIntelligencePlatform.LogEvents;
using Shouldly;
using Xunit;

namespace SystemIntelligencePlatform.LogEvents;

/// <summary>
/// Tests the adaptive anomaly detection algorithm.
/// All tests are pure unit tests with no external dependencies.
///
/// Coverage:
/// - Spike detection (5-min window vs baseline)
/// - Burst detection (1-hour window vs baseline)
/// - Immediate critical trigger
/// - No false positive when under baseline
/// - New signature fallback (no baseline)
/// - Baseline calculation correctness (via metric input)
/// </summary>
public class AnomalyDetectionService_Tests
{
    private readonly AnomalyDetectionService _sut = new();

    // --- SPIKE DETECTION ---

    [Fact]
    public void Spike_WhenEventsExceed3xExpected5Min_ShouldTrigger()
    {
        // Arrange: baseline = 12 events/hour → expected 5-min = 1. Threshold = 1 * 3 = 3.
        var metrics = new AnomalyMetrics
        {
            EventsLast5Min = 4, // > 3
            EventsLast1Hour = 5,
            EventsLast24Hours = 20,
            AverageHourlyBaseline = 12,
            StandardDeviation = 2
        };

        // Act
        var result = _sut.Evaluate(metrics, LogLevel.Error);

        // Assert
        result.ShouldTrigger.ShouldBeTrue();
        result.Reason.ShouldBe(AnomalyReason.SpikeDetected);
    }

    [Fact]
    public void Spike_WhenEventsBelowThreshold_ShouldNotTrigger()
    {
        // Arrange: baseline = 120/hour → expected 5-min = 10. Threshold = 30.
        var metrics = new AnomalyMetrics
        {
            EventsLast5Min = 15, // < 30
            EventsLast1Hour = 80, // < 240
            EventsLast24Hours = 2000,
            AverageHourlyBaseline = 120,
            StandardDeviation = 10
        };

        // Act
        var result = _sut.Evaluate(metrics, LogLevel.Warning);

        // Assert: neither spike nor burst
        result.ShouldTrigger.ShouldBeFalse();
    }

    [Fact]
    public void Spike_WithHighBaseline_RequiresHighVolume()
    {
        // Arrange: baseline = 600/hour → expected 5-min = 50. Threshold = 150.
        var metrics = new AnomalyMetrics
        {
            EventsLast5Min = 100, // < 150, no spike
            EventsLast1Hour = 500, // < 1200, no burst
            AverageHourlyBaseline = 600,
            StandardDeviation = 50
        };

        var result = _sut.Evaluate(metrics, LogLevel.Error);
        result.ShouldTrigger.ShouldBeFalse();
    }

    // --- BURST DETECTION ---

    [Fact]
    public void Burst_WhenEventsExceed2xBaseline_ShouldTrigger()
    {
        // Arrange: baseline = 50/hour. Burst threshold = 100.
        var metrics = new AnomalyMetrics
        {
            EventsLast5Min = 5, // no spike (50/12*3 = 12.5)
            EventsLast1Hour = 110, // > 100, burst!
            EventsLast24Hours = 500,
            AverageHourlyBaseline = 50,
            StandardDeviation = 8
        };

        var result = _sut.Evaluate(metrics, LogLevel.Error);

        result.ShouldTrigger.ShouldBeTrue();
        result.Reason.ShouldBe(AnomalyReason.BurstDetected);
    }

    [Fact]
    public void Burst_WhenExactlyAtThreshold_ShouldNotTrigger()
    {
        // Arrange: baseline = 50/hour. Burst = exactly 100 (not >100).
        var metrics = new AnomalyMetrics
        {
            EventsLast5Min = 5,
            EventsLast1Hour = 100, // == 100, not > 100
            AverageHourlyBaseline = 50,
            StandardDeviation = 5
        };

        var result = _sut.Evaluate(metrics, LogLevel.Warning);
        result.ShouldTrigger.ShouldBeFalse();
    }

    // --- IMMEDIATE CRITICAL ---

    [Fact]
    public void Critical_LogLevel_ShouldAlwaysTrigger_RegardlessOfMetrics()
    {
        // Arrange: even with zero events, Critical level should trigger
        var metrics = new AnomalyMetrics
        {
            EventsLast5Min = 1,
            EventsLast1Hour = 1,
            EventsLast24Hours = 1,
            AverageHourlyBaseline = 1000,
            StandardDeviation = 100
        };

        var result = _sut.Evaluate(metrics, LogLevel.Critical);

        result.ShouldTrigger.ShouldBeTrue();
        result.Reason.ShouldBe(AnomalyReason.ImmediateCritical);
        result.SuggestedSeverity.ShouldBe(IncidentSeverity.Critical);
    }

    // --- NO FALSE POSITIVE ---

    [Fact]
    public void NoTrigger_WhenAllMetricsUnderBaseline()
    {
        // Arrange: everything is well within normal range
        var metrics = new AnomalyMetrics
        {
            EventsLast5Min = 2,
            EventsLast1Hour = 40,
            EventsLast24Hours = 800,
            AverageHourlyBaseline = 50,
            StandardDeviation = 10
        };

        var result = _sut.Evaluate(metrics, LogLevel.Information);
        result.ShouldTrigger.ShouldBeFalse();
        result.Reason.ShouldBe(AnomalyReason.None);
    }

    [Fact]
    public void NoTrigger_ForDebugLevel_EvenWithHighCounts()
    {
        // Arrange: high counts but baseline is also high, so no spike/burst
        var metrics = new AnomalyMetrics
        {
            EventsLast5Min = 20,
            EventsLast1Hour = 300,
            AverageHourlyBaseline = 500,
            StandardDeviation = 50
        };

        var result = _sut.Evaluate(metrics, LogLevel.Debug);
        result.ShouldTrigger.ShouldBeFalse();
    }

    // --- NEW SIGNATURE FALLBACK ---

    [Fact]
    public void NewSignature_NoBaseline_Spike_ShouldUseFallbackThreshold()
    {
        // Arrange: no baseline (new hash), 5-min count > 10 (fallback threshold)
        var metrics = new AnomalyMetrics
        {
            EventsLast5Min = 15,
            EventsLast1Hour = 20,
            AverageHourlyBaseline = 0,
            StandardDeviation = 0
        };

        var result = _sut.Evaluate(metrics, LogLevel.Error);

        result.ShouldTrigger.ShouldBeTrue();
        result.Reason.ShouldBe(AnomalyReason.SpikeDetected);
    }

    [Fact]
    public void NewSignature_NoBaseline_Burst_ShouldUseFallbackThreshold()
    {
        // Arrange: no baseline, 1-hour count > 30 (fallback threshold)
        var metrics = new AnomalyMetrics
        {
            EventsLast5Min = 5,
            EventsLast1Hour = 35,
            AverageHourlyBaseline = 0,
            StandardDeviation = 0
        };

        var result = _sut.Evaluate(metrics, LogLevel.Error);

        result.ShouldTrigger.ShouldBeTrue();
        result.Reason.ShouldBe(AnomalyReason.BurstDetected);
    }

    [Fact]
    public void NewSignature_NoBaseline_LowCounts_ShouldNotTrigger()
    {
        var metrics = new AnomalyMetrics
        {
            EventsLast5Min = 3,
            EventsLast1Hour = 8,
            AverageHourlyBaseline = 0,
            StandardDeviation = 0
        };

        var result = _sut.Evaluate(metrics, LogLevel.Error);
        result.ShouldTrigger.ShouldBeFalse();
    }

    // --- SEVERITY DETERMINATION ---

    [Fact]
    public void Severity_ErrorWith100Events_ShouldBeCritical()
    {
        var metrics = new AnomalyMetrics
        {
            EventsLast5Min = 100, // huge spike
            EventsLast1Hour = 200,
            AverageHourlyBaseline = 10,
            StandardDeviation = 2
        };

        var result = _sut.Evaluate(metrics, LogLevel.Error);

        result.ShouldTrigger.ShouldBeTrue();
        result.SuggestedSeverity.ShouldBe(IncidentSeverity.Critical);
    }

    [Fact]
    public void Severity_WarningLevel_ShouldBeMediumByDefault()
    {
        var metrics = new AnomalyMetrics
        {
            EventsLast5Min = 20,
            EventsLast1Hour = 40,
            AverageHourlyBaseline = 5,
            StandardDeviation = 1
        };

        var result = _sut.Evaluate(metrics, LogLevel.Warning);

        result.ShouldTrigger.ShouldBeTrue();
        result.SuggestedSeverity.ShouldBe(IncidentSeverity.Medium);
    }
}
