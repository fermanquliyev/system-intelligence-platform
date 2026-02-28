using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SystemIntelligencePlatform.Subscriptions;

namespace SystemIntelligencePlatform.Controllers;

[Route("api/app/usage")]
[Authorize]
public class UsageController : SystemIntelligencePlatformController
{
    private readonly ISubscriptionAppService _subscriptionAppService;

    public UsageController(ISubscriptionAppService subscriptionAppService)
    {
        _subscriptionAppService = subscriptionAppService;
    }

    [HttpGet]
    public Task<UsageDto> GetAsync()
        => _subscriptionAppService.GetUsageAsync();
}
