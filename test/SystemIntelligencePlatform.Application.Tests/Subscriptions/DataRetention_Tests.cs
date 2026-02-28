using System;
using SystemIntelligencePlatform.Subscriptions;
using Shouldly;
using Xunit;

namespace SystemIntelligencePlatform.Subscriptions;

/// <summary>
/// Pure unit tests for data retention limits per subscription plan.
/// Tests verify that each plan tier has the correct retention period.
/// Note: The actual background job that enforces retention uses repository methods
/// that are tested separately in EF Core integration tests.
/// </summary>
public class DataRetention_Tests
{
    [Fact]
    public void Free_Plan_Retention_Should_Be_7_Days()
    {
        // Arrange
        var subscription = new Subscription(Guid.NewGuid(), SubscriptionPlan.Free);

        // Act
        var limits = subscription.GetPlanLimits();

        // Assert
        limits.RetentionDays.ShouldBe(7);
    }

    [Fact]
    public void Pro_Plan_Retention_Should_Be_30_Days()
    {
        // Arrange
        var subscription = new Subscription(Guid.NewGuid(), SubscriptionPlan.Pro);

        // Act
        var limits = subscription.GetPlanLimits();

        // Assert
        limits.RetentionDays.ShouldBe(30);
    }

    [Fact]
    public void Enterprise_Plan_Retention_Should_Be_90_Days()
    {
        // Arrange
        var subscription = new Subscription(Guid.NewGuid(), SubscriptionPlan.Enterprise);

        // Act
        var limits = subscription.GetPlanLimits();

        // Assert
        limits.RetentionDays.ShouldBe(90);
    }

    [Fact]
    public void Retention_Days_Should_Increase_With_Plan_Tier()
    {
        // Arrange & Act
        var freeLimits = PlanLimits.GetLimits(SubscriptionPlan.Free);
        var proLimits = PlanLimits.GetLimits(SubscriptionPlan.Pro);
        var enterpriseLimits = PlanLimits.GetLimits(SubscriptionPlan.Enterprise);

        // Assert
        freeLimits.RetentionDays.ShouldBeLessThan(proLimits.RetentionDays);
        proLimits.RetentionDays.ShouldBeLessThan(enterpriseLimits.RetentionDays);
    }

    [Fact]
    public void Plan_Change_Should_Update_Retention_Days()
    {
        // Arrange
        var subscription = new Subscription(Guid.NewGuid(), SubscriptionPlan.Free);
        var freeRetention = subscription.GetPlanLimits().RetentionDays;
        freeRetention.ShouldBe(7);

        // Act - Upgrade to Pro
        subscription.ChangePlan(SubscriptionPlan.Pro);
        var proRetention = subscription.GetPlanLimits().RetentionDays;

        // Assert
        proRetention.ShouldBe(30);
        proRetention.ShouldBeGreaterThan(freeRetention);
    }
}
