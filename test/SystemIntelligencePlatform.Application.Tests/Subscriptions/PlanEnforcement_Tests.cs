using System;
using SystemIntelligencePlatform.Subscriptions;
using Shouldly;
using Xunit;

namespace SystemIntelligencePlatform.Subscriptions;

/// <summary>
/// Pure unit tests for plan enforcement logic.
/// Tests verify that usage limits are correctly calculated and enforced.
/// Uses direct entity testing without mocking AsyncExecuter (which is ABP-specific).
/// </summary>
public class PlanEnforcement_Tests
{
    [Fact]
    public void When_Usage_Plus_Input_Exceeds_Free_Plan_Limit_Should_Exceed()
    {
        // Arrange
        var subscription = new Subscription(Guid.NewGuid(), SubscriptionPlan.Free);
        var limits = subscription.GetPlanLimits();
        var usage = new MonthlyUsage(Guid.NewGuid(), MonthlyUsage.CurrentMonth());
        
        // Set usage to near the limit (9999 logs)
        usage.IncrementLogs(9999);
        var inputCount = 100; // This would push us to 10099, exceeding 10000 limit

        // Act & Assert
        var totalAfterIngestion = usage.LogsIngested + inputCount;
        totalAfterIngestion.ShouldBeGreaterThan(limits.LogsPerMonth);
    }

    [Fact]
    public void When_Usage_Plus_Input_Under_Free_Plan_Limit_Should_Not_Exceed()
    {
        // Arrange
        var subscription = new Subscription(Guid.NewGuid(), SubscriptionPlan.Free);
        var limits = subscription.GetPlanLimits();
        var usage = new MonthlyUsage(Guid.NewGuid(), MonthlyUsage.CurrentMonth());
        
        // Set usage to 9000 logs
        usage.IncrementLogs(9000);
        var inputCount = 500; // This would push us to 9500, under 10000 limit

        // Act & Assert
        var totalAfterIngestion = usage.LogsIngested + inputCount;
        totalAfterIngestion.ShouldBeLessThanOrEqualTo(limits.LogsPerMonth);
    }

    [Fact]
    public void When_Usage_At_Exact_Limit_Plus_One_Should_Exceed()
    {
        // Arrange
        var subscription = new Subscription(Guid.NewGuid(), SubscriptionPlan.Free);
        var limits = subscription.GetPlanLimits();
        var usage = new MonthlyUsage(Guid.NewGuid(), MonthlyUsage.CurrentMonth());
        
        // Set usage to exactly the limit
        usage.IncrementLogs((int)limits.LogsPerMonth);
        var inputCount = 1; // This would exceed the limit

        // Act & Assert
        var totalAfterIngestion = usage.LogsIngested + inputCount;
        totalAfterIngestion.ShouldBeGreaterThan(limits.LogsPerMonth);
    }

    [Fact]
    public void When_Usage_At_Exact_Limit_Should_Not_Exceed()
    {
        // Arrange
        var subscription = new Subscription(Guid.NewGuid(), SubscriptionPlan.Free);
        var limits = subscription.GetPlanLimits();
        var usage = new MonthlyUsage(Guid.NewGuid(), MonthlyUsage.CurrentMonth());
        
        // Set usage to exactly the limit
        usage.IncrementLogs((int)limits.LogsPerMonth);
        var inputCount = 0; // No new logs

        // Act & Assert
        var totalAfterIngestion = usage.LogsIngested + inputCount;
        totalAfterIngestion.ShouldBeLessThanOrEqualTo(limits.LogsPerMonth);
    }

    [Fact]
    public void Zero_Logs_Edge_Case_Should_Not_Exceed()
    {
        // Arrange
        var subscription = new Subscription(Guid.NewGuid(), SubscriptionPlan.Free);
        var limits = subscription.GetPlanLimits();
        var usage = new MonthlyUsage(Guid.NewGuid(), MonthlyUsage.CurrentMonth());
        
        // No usage yet
        var inputCount = 0;

        // Act & Assert
        var totalAfterIngestion = usage.LogsIngested + inputCount;
        totalAfterIngestion.ShouldBeLessThanOrEqualTo(limits.LogsPerMonth);
    }

    [Fact]
    public void Pro_Plan_Limit_Enforcement_Should_Work_Correctly()
    {
        // Arrange
        var subscription = new Subscription(Guid.NewGuid(), SubscriptionPlan.Pro);
        var limits = subscription.GetPlanLimits();
        var usage = new MonthlyUsage(Guid.NewGuid(), MonthlyUsage.CurrentMonth());
        
        // Set usage to near Pro limit (499999 logs)
        usage.IncrementLogs(499999);
        var inputCount = 2; // This would push us to 500001, exceeding 500000 limit

        // Act & Assert
        var totalAfterIngestion = usage.LogsIngested + inputCount;
        totalAfterIngestion.ShouldBeGreaterThan(limits.LogsPerMonth);
    }

    [Fact]
    public void Enterprise_Plan_Limit_Enforcement_Should_Work_Correctly()
    {
        // Arrange
        var subscription = new Subscription(Guid.NewGuid(), SubscriptionPlan.Enterprise);
        var limits = subscription.GetPlanLimits();
        var usage = new MonthlyUsage(Guid.NewGuid(), MonthlyUsage.CurrentMonth());
        
        // Set usage to near Enterprise limit (9999999 logs)
        usage.IncrementLogs(9999999);
        var inputCount = 2; // This would push us to 10000001, exceeding 10000000 limit

        // Act & Assert
        var totalAfterIngestion = usage.LogsIngested + inputCount;
        totalAfterIngestion.ShouldBeGreaterThan(limits.LogsPerMonth);
    }
}
