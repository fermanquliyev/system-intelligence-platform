using System;
using SystemIntelligencePlatform.Subscriptions;
using Shouldly;
using Xunit;

namespace SystemIntelligencePlatform.Subscriptions;

/// <summary>
/// Pure unit tests for Stripe webhook processing logic.
/// Tests verify that Subscription entity behavior is correct when processing Stripe events:
/// - subscription.created: BindStripe stores IDs, plan stays
/// - subscription.updated: ChangePlan changes to Pro
/// - subscription.deleted: Cancel sets Canceled status, reverts to Free
/// </summary>
public class StripeWebhook_Tests
{
    [Fact]
    public void Subscription_Created_Should_Bind_Stripe_Ids_And_Preserve_Plan()
    {
        // Arrange
        var subscription = new Subscription(Guid.NewGuid(), SubscriptionPlan.Free);
        var customerId = "cus_abc123xyz";
        var subscriptionId = "sub_def456uvw";

        // Act - Simulate subscription.created webhook
        subscription.BindStripe(customerId, subscriptionId);

        // Assert
        subscription.StripeCustomerId.ShouldBe(customerId);
        subscription.StripeSubscriptionId.ShouldBe(subscriptionId);
        subscription.Plan.ShouldBe(SubscriptionPlan.Free); // Plan should remain unchanged
        subscription.Status.ShouldBe(SubscriptionStatus.Active); // Status should remain Active
    }

    [Fact]
    public void Subscription_Updated_Should_Change_Plan_To_Pro()
    {
        // Arrange
        var subscription = new Subscription(Guid.NewGuid(), SubscriptionPlan.Free);
        subscription.BindStripe("cus_test", "sub_test");

        // Act - Simulate subscription.updated webhook (upgrade to Pro)
        subscription.ChangePlan(SubscriptionPlan.Pro);

        // Assert
        subscription.Plan.ShouldBe(SubscriptionPlan.Pro);
        subscription.StripeCustomerId.ShouldNotBeNull(); // Stripe IDs should remain
        subscription.StripeSubscriptionId.ShouldNotBeNull();
    }

    [Fact]
    public void Subscription_Updated_Should_Change_Plan_To_Enterprise()
    {
        // Arrange
        var subscription = new Subscription(Guid.NewGuid(), SubscriptionPlan.Pro);
        subscription.BindStripe("cus_test", "sub_test");

        // Act - Simulate subscription.updated webhook (upgrade to Enterprise)
        subscription.ChangePlan(SubscriptionPlan.Enterprise);

        // Assert
        subscription.Plan.ShouldBe(SubscriptionPlan.Enterprise);
    }

    [Fact]
    public void Subscription_Deleted_Should_Cancel_And_Set_Canceled_Status()
    {
        // Arrange
        var subscription = new Subscription(Guid.NewGuid(), SubscriptionPlan.Pro);
        subscription.BindStripe("cus_test", "sub_test");
        subscription.Status.ShouldBe(SubscriptionStatus.Active); // Verify initial state

        // Act - Simulate subscription.deleted webhook
        subscription.Cancel();

        // Assert
        subscription.Status.ShouldBe(SubscriptionStatus.Canceled);
        subscription.StripeCustomerId.ShouldNotBeNull(); // Stripe IDs remain for reference
        subscription.StripeSubscriptionId.ShouldNotBeNull();
    }

    [Fact]
    public void Subscription_Deleted_Then_Reverted_To_Free_Should_Have_Free_Plan_Limits()
    {
        // Arrange
        var subscription = new Subscription(Guid.NewGuid(), SubscriptionPlan.Pro);
        subscription.BindStripe("cus_test", "sub_test");

        // Act - Simulate subscription.deleted webhook (cancel)
        subscription.Cancel();
        // Then revert to Free plan (typical behavior after cancellation)
        subscription.ChangePlan(SubscriptionPlan.Free);

        // Assert
        subscription.Status.ShouldBe(SubscriptionStatus.Canceled);
        subscription.Plan.ShouldBe(SubscriptionPlan.Free);
        var limits = subscription.GetPlanLimits();
        limits.LogsPerMonth.ShouldBe(10_000);
        limits.AiRootCause.ShouldBeFalse();
        limits.WebhookNotifications.ShouldBeFalse();
    }

    [Fact]
    public void BindStripe_Should_Work_On_Any_Plan()
    {
        // Arrange
        var freeSubscription = new Subscription(Guid.NewGuid(), SubscriptionPlan.Free);
        var proSubscription = new Subscription(Guid.NewGuid(), SubscriptionPlan.Pro);
        var enterpriseSubscription = new Subscription(Guid.NewGuid(), SubscriptionPlan.Enterprise);

        // Act
        freeSubscription.BindStripe("cus_free", "sub_free");
        proSubscription.BindStripe("cus_pro", "sub_pro");
        enterpriseSubscription.BindStripe("cus_ent", "sub_ent");

        // Assert
        freeSubscription.StripeCustomerId.ShouldBe("cus_free");
        proSubscription.StripeCustomerId.ShouldBe("cus_pro");
        enterpriseSubscription.StripeCustomerId.ShouldBe("cus_ent");
    }

    [Fact]
    public void Multiple_Plan_Changes_Should_Update_Correctly()
    {
        // Arrange
        var subscription = new Subscription(Guid.NewGuid(), SubscriptionPlan.Free);

        // Act - Simulate multiple plan changes
        subscription.ChangePlan(SubscriptionPlan.Pro);
        subscription.ChangePlan(SubscriptionPlan.Enterprise);
        subscription.ChangePlan(SubscriptionPlan.Pro);

        // Assert
        subscription.Plan.ShouldBe(SubscriptionPlan.Pro);
    }
}
