using System;
using SystemIntelligencePlatform.Subscriptions;
using Shouldly;
using Xunit;

namespace SystemIntelligencePlatform.Subscriptions;

/// <summary>
/// Pure unit tests for usage tracking functionality.
/// Tests verify that MonthlyUsage correctly accumulates logs and AI calls across multiple operations.
/// </summary>
public class UsageTracking_Tests
{
    [Fact]
    public void MonthlyUsage_IncrementLogs_Should_Accumulate_Across_Multiple_Calls()
    {
        // Arrange
        var usage = new MonthlyUsage(Guid.NewGuid(), MonthlyUsage.CurrentMonth());

        // Act
        usage.IncrementLogs(100);
        usage.IncrementLogs(250);
        usage.IncrementLogs(500);
        usage.IncrementLogs(150);

        // Assert
        usage.LogsIngested.ShouldBe(1000);
    }

    [Fact]
    public void MonthlyUsage_IncrementAiCalls_Should_Accumulate_Across_Multiple_Calls()
    {
        // Arrange
        var usage = new MonthlyUsage(Guid.NewGuid(), MonthlyUsage.CurrentMonth());

        // Act
        usage.IncrementAiCalls();
        usage.IncrementAiCalls();
        usage.IncrementAiCalls(5);
        usage.IncrementAiCalls(10);

        // Assert
        usage.AiCallsUsed.ShouldBe(17); // 1 + 1 + 5 + 10
    }

    [Fact]
    public void Month_Value_Should_Be_Correct_YYYYMM_Format()
    {
        // Arrange
        var expectedMonth = 202501; // January 2025

        // Act
        var usage = new MonthlyUsage(Guid.NewGuid(), expectedMonth);

        // Assert
        usage.Month.ShouldBe(202501);
    }

    [Fact]
    public void CurrentMonth_Should_Return_Current_Year_Month_Format()
    {
        // Act
        var currentMonth = MonthlyUsage.CurrentMonth();
        var now = DateTime.UtcNow;
        var expectedFormat = now.Year * 100 + now.Month;

        // Assert
        currentMonth.ShouldBe(expectedFormat);
    }

    [Fact]
    public void Multiple_Log_Increments_Should_Maintain_Correct_Total()
    {
        // Arrange
        var usage = new MonthlyUsage(Guid.NewGuid(), MonthlyUsage.CurrentMonth());
        var increments = new[] { 50, 100, 200, 300, 350 };
        var expectedTotal = 0;

        // Act
        foreach (var increment in increments)
        {
            usage.IncrementLogs(increment);
            expectedTotal += increment;
        }

        // Assert
        usage.LogsIngested.ShouldBe(expectedTotal);
    }

    [Fact]
    public void Multiple_AiCall_Increments_Should_Maintain_Correct_Total()
    {
        // Arrange
        var usage = new MonthlyUsage(Guid.NewGuid(), MonthlyUsage.CurrentMonth());
        var increments = new[] { 1, 1, 3, 5, 10 };
        var expectedTotal = 0;

        // Act
        foreach (var increment in increments)
        {
            usage.IncrementAiCalls(increment);
            expectedTotal += increment;
        }

        // Assert
        usage.AiCallsUsed.ShouldBe(expectedTotal);
    }

    [Fact]
    public void Mixed_Log_And_AiCall_Increments_Should_Track_Both_Independently()
    {
        // Arrange
        var usage = new MonthlyUsage(Guid.NewGuid(), MonthlyUsage.CurrentMonth());

        // Act
        usage.IncrementLogs(100);
        usage.IncrementAiCalls(2);
        usage.IncrementLogs(200);
        usage.IncrementAiCalls(3);
        usage.IncrementLogs(300);
        usage.IncrementAiCalls(5);

        // Assert
        usage.LogsIngested.ShouldBe(600);
        usage.AiCallsUsed.ShouldBe(10);
    }

    [Fact]
    public void Large_Log_Increments_Should_Handle_Correctly()
    {
        // Arrange
        var usage = new MonthlyUsage(Guid.NewGuid(), MonthlyUsage.CurrentMonth());

        // Act
        usage.IncrementLogs(1_000_000);
        usage.IncrementLogs(5_000_000);

        // Assert
        usage.LogsIngested.ShouldBe(6_000_000);
    }
}
