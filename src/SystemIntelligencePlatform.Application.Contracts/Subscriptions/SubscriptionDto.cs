using System;
using SystemIntelligencePlatform.Subscriptions;
using Volo.Abp.Application.Dtos;

namespace SystemIntelligencePlatform.Subscriptions;

public class SubscriptionDto : EntityDto<Guid>
{
    public SubscriptionPlan Plan { get; set; }
    public SubscriptionStatus Status { get; set; }
    public DateTime CurrentPeriodStart { get; set; }
    public DateTime CurrentPeriodEnd { get; set; }
    public PlanLimitsDto Limits { get; set; } = null!;
}

public class PlanLimitsDto
{
    public long LogsPerMonth { get; set; }
    public int MaxApplications { get; set; }
    public int RetentionDays { get; set; }
    public bool AiRootCause { get; set; }
    public bool WebhookNotifications { get; set; }
}
