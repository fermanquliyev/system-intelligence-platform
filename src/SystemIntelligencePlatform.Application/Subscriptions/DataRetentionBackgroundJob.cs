using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SystemIntelligencePlatform.LogEvents;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;

namespace SystemIntelligencePlatform.Subscriptions;

/// <summary>
/// Enforces per-plan data retention by deleting log events older than the plan's retention period.
/// Free plan: 7 days, Pro: 30 days, Enterprise: 90 days.
/// Incidents are retained longer (not deleted by this job).
/// </summary>
public class DataRetentionBackgroundJob : AsyncBackgroundJob<DataRetentionArgs>, ITransientDependency
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly ILogEventRepository _logEventRepository;
    private readonly ICurrentTenant _currentTenant;

    public DataRetentionBackgroundJob(
        ITenantRepository tenantRepository,
        ISubscriptionRepository subscriptionRepository,
        ILogEventRepository logEventRepository,
        ICurrentTenant currentTenant)
    {
        _tenantRepository = tenantRepository;
        _subscriptionRepository = subscriptionRepository;
        _logEventRepository = logEventRepository;
        _currentTenant = currentTenant;
    }

    public override async Task ExecuteAsync(DataRetentionArgs args)
    {
        // Process host tenant
        await EnforceRetentionAsync(null);

        // Process each tenant
        var tenants = await _tenantRepository.GetListAsync();
        foreach (var tenant in tenants)
        {
            using (_currentTenant.Change(tenant.Id))
            {
                await EnforceRetentionAsync(tenant.Id);
            }
        }
    }

    private async Task EnforceRetentionAsync(Guid? tenantId)
    {
        var subscription = await _subscriptionRepository.FindByTenantIdAsync(tenantId);
        var plan = subscription?.Plan ?? SubscriptionPlan.Free;
        var limits = PlanLimits.GetLimits(plan);

        var cutoff = DateTime.UtcNow.AddDays(-limits.RetentionDays);

        var batchSize = 1000;
        int deleted;
        do
        {
            var oldEvents = await _logEventRepository.GetOlderThanAsync(cutoff, batchSize);
            if (oldEvents.Count == 0) break;

            await _logEventRepository.DeleteBatchAsync(
                oldEvents.ConvertAll(e => e.Id));
            deleted = oldEvents.Count;

            Logger.LogInformation(
                "Retention: Deleted {Count} log events for tenant {TenantId} (plan: {Plan}, cutoff: {Cutoff})",
                deleted, tenantId, plan, cutoff);
        }
        while (deleted >= batchSize);
    }
}

public class DataRetentionArgs { }
