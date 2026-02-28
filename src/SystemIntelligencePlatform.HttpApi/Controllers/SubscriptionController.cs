using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SystemIntelligencePlatform.Subscriptions;

namespace SystemIntelligencePlatform.Controllers;

[Route("api/app/subscription")]
[Authorize]
public class SubscriptionController : SystemIntelligencePlatformController
{
    private readonly ISubscriptionAppService _subscriptionAppService;

    public SubscriptionController(ISubscriptionAppService subscriptionAppService)
    {
        _subscriptionAppService = subscriptionAppService;
    }

    [HttpGet]
    public Task<SubscriptionDto> GetCurrentAsync()
        => _subscriptionAppService.GetCurrentAsync();

    [HttpGet("usage")]
    public Task<UsageDto> GetUsageAsync()
        => _subscriptionAppService.GetUsageAsync();

    [HttpPost("checkout")]
    public Task<string> CreateCheckoutAsync([FromQuery] SubscriptionPlan plan)
        => _subscriptionAppService.CreateCheckoutSessionAsync(plan);
}
