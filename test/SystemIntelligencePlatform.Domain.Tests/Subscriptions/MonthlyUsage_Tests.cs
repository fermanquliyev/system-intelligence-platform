using System;
using SystemIntelligencePlatform.Subscriptions;
using Shouldly;
using Xunit;

namespace SystemIntelligencePlatform.Subscriptions;

/// <summary>
/// Tests MonthlyUsage entity behavior: log and AI call incrementing, month format.
/// Pure unit tests, no DB needed.
/// </summary>
public class MonthlyUsage_Tests
{
    private MonthlyUsage CreateUsage(int month, Guid? tenantId = null)
    {
        return new MonthlyUsage(Guid.NewGuid(), month, tenantId);
    }

    [Fact]
    public void IncrementLogs_Should_Add_To_Total()
    {
        // Arrange
        var usage = CreateUsage(202501);

        // Act
        usage.IncrementLogs(100);
        usage.IncrementLogs(50);

        // Assert
        usage.LogsIngested.ShouldBe(150);
    }

    [Fact]
    public void IncrementLogs_With_Large_Count_Should_Accumulate_Correctly()
    {
        // Arrange
        var usage = CreateUsage(202501);

        // Act
        usage.IncrementLogs(1000);
        usage.IncrementLogs(5000);
        usage.IncrementLogs(10000);

        // Assert
        usage.LogsIngested.ShouldBe(16000);
    }

    [Fact]
    public void IncrementAiCalls_Should_Add_To_Total()
    {
        // Arrange
        var usage = CreateUsage(202501);

        // Act
        usage.IncrementAiCalls();
        usage.IncrementAiCalls();
        usage.IncrementAiCalls(5);

        // Assert
        usage.AiCallsUsed.ShouldBe(7); // 1 + 1 + 5
    }

    [Fact]
    public void IncrementAiCalls_With_Count_Parameter_Should_Add_Correctly()
    {
        // Arrange
        var usage = CreateUsage(202501);

        // Act
        usage.IncrementAiCalls(10);
        usage.IncrementAiCalls(20);

        // Assert
        usage.AiCallsUsed.ShouldBe(30);
    }

    [Fact]
    public void IncrementAiCalls_Default_Parameter_Should_Increment_By_One()
    {
        // Arrange
        var usage = CreateUsage(202501);

        // Act
        usage.IncrementAiCalls(); // Uses default parameter of 1

        // Assert
        usage.AiCallsUsed.ShouldBe(1);
    }

    [Fact]
    public void CurrentMonth_Should_Return_Correct_YYYYMM_Format()
    {
        // Act
        var currentMonth = MonthlyUsage.CurrentMonth();
        var now = DateTime.UtcNow;
        var expectedMonth = now.Year * 100 + now.Month;

        // Assert
        currentMonth.ShouldBe(expectedMonth);
    }

    [Fact]
    public void CurrentMonth_Should_Be_6_Digit_Integer()
    {
        // Act
        var currentMonth = MonthlyUsage.CurrentMonth();

        // Assert
        currentMonth.ShouldBeGreaterThan(200000); // Should be at least 200000 (year 2000, month 0)
        currentMonth.ShouldBeLessThan(300000); // Should be less than 300000 (year 3000, month 0)
        // Format: YYYYMM, so 202501 = January 2025
    }

    [Fact]
    public void Month_Property_Should_Store_YYYYMM_Format()
    {
        // Arrange
        var month = 202501; // January 2025

        // Act
        var usage = CreateUsage(month);

        // Assert
        usage.Month.ShouldBe(202501);
    }

    [Fact]
    public void Multiple_Increments_Should_Accumulate_Both_Logs_And_AiCalls()
    {
        // Arrange
        var usage = CreateUsage(202501);

        // Act
        usage.IncrementLogs(100);
        usage.IncrementAiCalls(5);
        usage.IncrementLogs(200);
        usage.IncrementAiCalls(3);

        // Assert
        usage.LogsIngested.ShouldBe(300);
        usage.AiCallsUsed.ShouldBe(8);
    }
}
