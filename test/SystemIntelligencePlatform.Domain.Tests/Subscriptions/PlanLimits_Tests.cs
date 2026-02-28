using SystemIntelligencePlatform.Subscriptions;
using Shouldly;
using Xunit;

namespace SystemIntelligencePlatform.Subscriptions;

/// <summary>
/// Tests PlanLimits static class behavior: verifying correct limits for each plan tier.
/// Pure unit tests, no DB needed.
/// </summary>
public class PlanLimits_Tests
{
    [Fact]
    public void FreePlan_Should_Return_10000_Logs_3_Apps_7_Days()
    {
        // Act
        var limits = PlanLimits.GetLimits(SubscriptionPlan.Free);

        // Assert
        limits.LogsPerMonth.ShouldBe(10_000);
        limits.MaxApplications.ShouldBe(3);
        limits.RetentionDays.ShouldBe(7);
    }

    [Fact]
    public void ProPlan_Should_Return_500000_Logs_20_Apps_30_Days_AI_Enabled()
    {
        // Act
        var limits = PlanLimits.GetLimits(SubscriptionPlan.Pro);

        // Assert
        limits.LogsPerMonth.ShouldBe(500_000);
        limits.MaxApplications.ShouldBe(20);
        limits.RetentionDays.ShouldBe(30);
        limits.AiRootCause.ShouldBeTrue();
        limits.WebhookNotifications.ShouldBeTrue();
    }

    [Fact]
    public void EnterprisePlan_Should_Return_10000000_Logs_100_Apps_90_Days()
    {
        // Act
        var limits = PlanLimits.GetLimits(SubscriptionPlan.Enterprise);

        // Assert
        limits.LogsPerMonth.ShouldBe(10_000_000);
        limits.MaxApplications.ShouldBe(100);
        limits.RetentionDays.ShouldBe(90);
        limits.AiRootCause.ShouldBeTrue();
        limits.WebhookNotifications.ShouldBeTrue();
    }

    [Fact]
    public void InvalidPlan_Should_Default_To_Free()
    {
        // Act - Cast invalid enum value
        var invalidPlan = (SubscriptionPlan)999;
        var limits = PlanLimits.GetLimits(invalidPlan);

        // Assert - Should default to Free plan limits
        limits.LogsPerMonth.ShouldBe(10_000);
        limits.MaxApplications.ShouldBe(3);
        limits.RetentionDays.ShouldBe(7);
    }
}
