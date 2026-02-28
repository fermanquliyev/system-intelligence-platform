using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace SystemIntelligencePlatform.Subscriptions;

public interface ISubscriptionRepository : IRepository<Subscription, Guid>
{
    Task<Subscription?> FindByTenantIdAsync(Guid? tenantId, CancellationToken cancellationToken = default);
    Task<Subscription?> FindByStripeSubscriptionIdAsync(string stripeSubscriptionId, CancellationToken cancellationToken = default);
}
