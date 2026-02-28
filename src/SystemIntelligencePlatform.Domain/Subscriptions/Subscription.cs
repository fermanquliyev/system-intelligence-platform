using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SystemIntelligencePlatform.Subscriptions;

public class Subscription : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    public SubscriptionPlan Plan { get; private set; }
    public SubscriptionStatus Status { get; private set; }
    public string? StripeCustomerId { get; private set; }
    public string? StripeSubscriptionId { get; private set; }
    public DateTime CurrentPeriodStart { get; private set; }
    public DateTime CurrentPeriodEnd { get; private set; }

    protected Subscription() { }

    public Subscription(
        Guid id,
        SubscriptionPlan plan,
        Guid? tenantId = null)
        : base(id)
    {
        TenantId = tenantId;
        Plan = plan;
        Status = SubscriptionStatus.Active;
        CurrentPeriodStart = DateTime.UtcNow;
        CurrentPeriodEnd = DateTime.UtcNow.AddMonths(1);
    }

    public void BindStripe(string customerId, string subscriptionId)
    {
        StripeCustomerId = customerId;
        StripeSubscriptionId = subscriptionId;
    }

    public void ChangePlan(SubscriptionPlan newPlan)
    {
        Plan = newPlan;
    }

    public void UpdateStatus(SubscriptionStatus status)
    {
        Status = status;
    }

    public void RenewPeriod(DateTime start, DateTime end)
    {
        CurrentPeriodStart = start;
        CurrentPeriodEnd = end;
        Status = SubscriptionStatus.Active;
    }

    public void Cancel()
    {
        Status = SubscriptionStatus.Canceled;
    }

    public PlanConfig GetPlanLimits() => PlanLimits.GetLimits(Plan);
}
