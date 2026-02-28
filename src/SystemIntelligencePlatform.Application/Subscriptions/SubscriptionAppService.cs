using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SystemIntelligencePlatform.MonitoredApplications;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace SystemIntelligencePlatform.Subscriptions;

public class SubscriptionAppService : ApplicationService, ISubscriptionAppService
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IRepository<MonthlyUsage, Guid> _usageRepository;
    private readonly IRepository<MonitoredApplication, Guid> _applicationRepository;
    private readonly ICurrentTenant _currentTenant;
    private readonly IConfiguration _configuration;

    public SubscriptionAppService(
        ISubscriptionRepository subscriptionRepository,
        IRepository<MonthlyUsage, Guid> usageRepository,
        IRepository<MonitoredApplication, Guid> applicationRepository,
        ICurrentTenant currentTenant,
        IConfiguration configuration)
    {
        _subscriptionRepository = subscriptionRepository;
        _usageRepository = usageRepository;
        _applicationRepository = applicationRepository;
        _currentTenant = currentTenant;
        _configuration = configuration;
    }

    public async Task<SubscriptionDto> GetCurrentAsync()
    {
        var subscription = await GetOrCreateSubscriptionAsync();
        var limits = subscription.GetPlanLimits();

        return new SubscriptionDto
        {
            Id = subscription.Id,
            Plan = subscription.Plan,
            Status = subscription.Status,
            CurrentPeriodStart = subscription.CurrentPeriodStart,
            CurrentPeriodEnd = subscription.CurrentPeriodEnd,
            Limits = new PlanLimitsDto
            {
                LogsPerMonth = limits.LogsPerMonth,
                MaxApplications = limits.MaxApplications,
                RetentionDays = limits.RetentionDays,
                AiRootCause = limits.AiRootCause,
                WebhookNotifications = limits.WebhookNotifications
            }
        };
    }

    public async Task<UsageDto> GetUsageAsync()
    {
        var subscription = await GetOrCreateSubscriptionAsync();
        var limits = subscription.GetPlanLimits();
        var currentMonth = MonthlyUsage.CurrentMonth();
        var usage = await GetOrCreateUsageAsync(currentMonth);

        var appQueryable = await _applicationRepository.GetQueryableAsync();
        var appCount = await AsyncExecuter.CountAsync(appQueryable);

        return new UsageDto
        {
            LogsIngested = usage.LogsIngested,
            LogsLimit = limits.LogsPerMonth,
            LogsPercentUsed = limits.LogsPerMonth > 0
                ? Math.Round((double)usage.LogsIngested / limits.LogsPerMonth * 100, 1)
                : 0,
            AiCallsUsed = usage.AiCallsUsed,
            ApplicationsUsed = appCount,
            ApplicationsLimit = limits.MaxApplications,
            Plan = subscription.Plan.ToString(),
            Month = currentMonth
        };
    }

    public async Task<string> CreateCheckoutSessionAsync(SubscriptionPlan plan)
    {
        if (plan == SubscriptionPlan.Free)
            throw new BusinessException("SIP:00010").WithData("message", "Cannot checkout for Free plan");

        var priceId = plan switch
        {
            SubscriptionPlan.Pro => _configuration["Stripe:ProPriceId"],
            SubscriptionPlan.Enterprise => _configuration["Stripe:EnterprisePriceId"],
            _ => throw new BusinessException("SIP:00010")
        };

        // Stripe session creation is handled by the infrastructure layer (StripeService in HttpApi.Host)
        // This returns the price ID; the controller creates the actual Stripe session.
        return priceId ?? throw new BusinessException("SIP:00010")
            .WithData("message", "Stripe price not configured");
    }

    internal async Task<Subscription> GetOrCreateSubscriptionAsync()
    {
        var subscription = await _subscriptionRepository.FindByTenantIdAsync(_currentTenant.Id);
        if (subscription != null) return subscription;

        subscription = new Subscription(GuidGenerator.Create(), SubscriptionPlan.Free, _currentTenant.Id);
        return await _subscriptionRepository.InsertAsync(subscription);
    }

    private async Task<MonthlyUsage> GetOrCreateUsageAsync(int month)
    {
        var queryable = await _usageRepository.GetQueryableAsync();
        var usage = await AsyncExecuter.FirstOrDefaultAsync(
            queryable.Where(u => u.Month == month));

        if (usage != null) return usage;

        usage = new MonthlyUsage(GuidGenerator.Create(), month, _currentTenant.Id);
        return await _usageRepository.InsertAsync(usage);
    }
}
