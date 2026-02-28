using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SystemIntelligencePlatform.Subscriptions;

public interface ISubscriptionAppService : IApplicationService
{
    Task<SubscriptionDto> GetCurrentAsync();
    Task<UsageDto> GetUsageAsync();
    Task<string> CreateCheckoutSessionAsync(SubscriptionPlan plan);
}
