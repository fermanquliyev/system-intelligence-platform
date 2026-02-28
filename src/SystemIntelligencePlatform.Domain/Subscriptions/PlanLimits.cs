namespace SystemIntelligencePlatform.Subscriptions;

/// <summary>
/// Defines the hard limits for each subscription tier.
/// These are enforced at ingestion time and in the application layer.
/// </summary>
public static class PlanLimits
{
    public static PlanConfig GetLimits(SubscriptionPlan plan) => plan switch
    {
        SubscriptionPlan.Free => new PlanConfig(
            LogsPerMonth: 10_000,
            MaxApplications: 3,
            RetentionDays: 7,
            AiRootCause: false,
            WebhookNotifications: false),

        SubscriptionPlan.Pro => new PlanConfig(
            LogsPerMonth: 500_000,
            MaxApplications: 20,
            RetentionDays: 30,
            AiRootCause: true,
            WebhookNotifications: true),

        SubscriptionPlan.Enterprise => new PlanConfig(
            LogsPerMonth: 10_000_000,
            MaxApplications: 100,
            RetentionDays: 90,
            AiRootCause: true,
            WebhookNotifications: true),

        _ => GetLimits(SubscriptionPlan.Free)
    };
}

public record PlanConfig(
    long LogsPerMonth,
    int MaxApplications,
    int RetentionDays,
    bool AiRootCause,
    bool WebhookNotifications);
