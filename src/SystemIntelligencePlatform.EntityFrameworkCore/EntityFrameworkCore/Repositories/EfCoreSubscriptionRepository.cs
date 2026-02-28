using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SystemIntelligencePlatform.Subscriptions;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace SystemIntelligencePlatform.EntityFrameworkCore.Repositories;

public class EfCoreSubscriptionRepository
    : EfCoreRepository<SystemIntelligencePlatformDbContext, Subscription, Guid>,
      ISubscriptionRepository
{
    public EfCoreSubscriptionRepository(
        IDbContextProvider<SystemIntelligencePlatformDbContext> dbContextProvider)
        : base(dbContextProvider) { }

    public async Task<Subscription?> FindByTenantIdAsync(
        Guid? tenantId, CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.FirstOrDefaultAsync(
            s => s.TenantId == tenantId, cancellationToken);
    }

    public async Task<Subscription?> FindByStripeSubscriptionIdAsync(
        string stripeSubscriptionId, CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.FirstOrDefaultAsync(
            s => s.StripeSubscriptionId == stripeSubscriptionId, cancellationToken);
    }
}
