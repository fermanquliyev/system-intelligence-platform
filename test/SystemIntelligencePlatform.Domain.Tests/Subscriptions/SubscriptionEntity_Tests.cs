using System;
using SystemIntelligencePlatform.Subscriptions;
using Shouldly;
using Xunit;

namespace SystemIntelligencePlatform.Subscriptions;

/// <summary>
/// Tests Subscription entity behavior: plan changes, cancellation, Stripe binding,
/// period renewal, and plan limits. Pure unit tests, no DB needed.
/// </summary>
public class SubscriptionEntity_Tests
{
    private Subscription CreateSubscription(
        SubscriptionPlan plan = SubscriptionPlan.Free,
        Guid? tenantId = null)
    {
        return new Subscription(Guid.NewGuid(), plan, tenantId);
    }

    // --- Plan Changes ---

    [Fact]
    public void ChangePlan_Should_Update_Plan()
    {
        // Arrange
        var subscription = CreateSubscription(SubscriptionPlan.Free);

        // Act
        subscription.ChangePlan(SubscriptionPlan.Pro);

        // Assert
        subscription.Plan.ShouldBe(SubscriptionPlan.Pro);
    }

    [Fact]
    public void ChangePlan_FromProToEnterprise_Should_Update_Plan()
    {
        // Arrange
        var subscription = CreateSubscription(SubscriptionPlan.Pro);

        // Act
        subscription.ChangePlan(SubscriptionPlan.Enterprise);

        // Assert
        subscription.Plan.ShouldBe(SubscriptionPlan.Enterprise);
    }

    // --- Cancellation ---

    [Fact]
    public void Cancel_Should_Set_Status_To_Canceled()
    {
        // Arrange
        var subscription = CreateSubscription(SubscriptionPlan.Pro);
        subscription.Status.ShouldBe(SubscriptionStatus.Active); // Initial state

        // Act
        subscription.Cancel();

        // Assert
        subscription.Status.ShouldBe(SubscriptionStatus.Canceled);
    }

    // --- Period Renewal ---

    [Fact]
    public void RenewPeriod_Should_Update_Dates_And_Set_Active()
    {
        // Arrange
        var subscription = CreateSubscription(SubscriptionPlan.Pro);
        subscription.UpdateStatus(SubscriptionStatus.PastDue); // Set to non-active first
        
        var newStart = DateTime.UtcNow.AddMonths(1);
        var newEnd = DateTime.UtcNow.AddMonths(2);

        // Act
        subscription.RenewPeriod(newStart, newEnd);

        // Assert
        subscription.CurrentPeriodStart.ShouldBe(newStart);
        subscription.CurrentPeriodEnd.ShouldBe(newEnd);
        subscription.Status.ShouldBe(SubscriptionStatus.Active);
    }

    // --- Stripe Binding ---

    [Fact]
    public void BindStripe_Should_Store_Customer_And_Subscription_Ids()
    {
        // Arrange
        var subscription = CreateSubscription(SubscriptionPlan.Pro);
        var customerId = "cus_abc123";
        var subscriptionId = "sub_xyz789";

        // Act
        subscription.BindStripe(customerId, subscriptionId);

        // Assert
        subscription.StripeCustomerId.ShouldBe(customerId);
        subscription.StripeSubscriptionId.ShouldBe(subscriptionId);
    }

    // --- Plan Limits ---

    [Fact]
    public void GetPlanLimits_ForFreePlan_Should_Return_Correct_Limits()
    {
        // Arrange
        var subscription = CreateSubscription(SubscriptionPlan.Free);

        // Act
        var limits = subscription.GetPlanLimits();

        // Assert
        limits.LogsPerMonth.ShouldBe(10_000);
        limits.MaxApplications.ShouldBe(3);
        limits.RetentionDays.ShouldBe(7);
        limits.AiRootCause.ShouldBeFalse();
        limits.WebhookNotifications.ShouldBeFalse();
    }

    [Fact]
    public void GetPlanLimits_ForProPlan_Should_Return_Correct_Limits()
    {
        // Arrange
        var subscription = CreateSubscription(SubscriptionPlan.Pro);

        // Act
        var limits = subscription.GetPlanLimits();

        // Assert
        limits.LogsPerMonth.ShouldBe(500_000);
        limits.MaxApplications.ShouldBe(20);
        limits.RetentionDays.ShouldBe(30);
        limits.AiRootCause.ShouldBeTrue();
        limits.WebhookNotifications.ShouldBeTrue();
    }

    [Fact]
    public void GetPlanLimits_ForEnterprisePlan_Should_Return_Correct_Limits()
    {
        // Arrange
        var subscription = CreateSubscription(SubscriptionPlan.Enterprise);

        // Act
        var limits = subscription.GetPlanLimits();

        // Assert
        limits.LogsPerMonth.ShouldBe(10_000_000);
        limits.MaxApplications.ShouldBe(100);
        limits.RetentionDays.ShouldBe(90);
        limits.AiRootCause.ShouldBeTrue();
        limits.WebhookNotifications.ShouldBeTrue();
    }

    [Fact]
    public void FreePlan_Should_Have_No_AI_And_No_Webhooks()
    {
        // Arrange
        var subscription = CreateSubscription(SubscriptionPlan.Free);

        // Act
        var limits = subscription.GetPlanLimits();

        // Assert
        limits.AiRootCause.ShouldBeFalse();
        limits.WebhookNotifications.ShouldBeFalse();
    }

    [Fact]
    public void ProPlan_Should_Have_AI_And_Webhooks()
    {
        // Arrange
        var subscription = CreateSubscription(SubscriptionPlan.Pro);

        // Act
        var limits = subscription.GetPlanLimits();

        // Assert
        limits.AiRootCause.ShouldBeTrue();
        limits.WebhookNotifications.ShouldBeTrue();
    }

    [Fact]
    public void EnterprisePlan_Should_Have_AI_And_Webhooks()
    {
        // Arrange
        var subscription = CreateSubscription(SubscriptionPlan.Enterprise);

        // Act
        var limits = subscription.GetPlanLimits();

        // Assert
        limits.AiRootCause.ShouldBeTrue();
        limits.WebhookNotifications.ShouldBeTrue();
    }
}
