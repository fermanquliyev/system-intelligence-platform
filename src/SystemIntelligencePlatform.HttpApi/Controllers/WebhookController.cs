using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;
using SystemIntelligencePlatform.Webhooks;

namespace SystemIntelligencePlatform.Controllers;

[Route("api/app/webhooks")]
[Authorize]
public class WebhookController : SystemIntelligencePlatformController
{
    private readonly IWebhookAppService _webhookAppService;

    public WebhookController(IWebhookAppService webhookAppService)
    {
        _webhookAppService = webhookAppService;
    }

    [HttpGet]
    public Task<ListResultDto<WebhookRegistrationDto>> GetListAsync()
        => _webhookAppService.GetListAsync();

    [HttpPost]
    public Task<WebhookRegistrationDto> CreateAsync([FromBody] CreateWebhookDto input)
        => _webhookAppService.CreateAsync(input);

    [HttpDelete("{id}")]
    public Task DeleteAsync(Guid id)
        => _webhookAppService.DeleteAsync(id);

    [HttpPost("{id}/toggle")]
    public Task<WebhookRegistrationDto> ToggleAsync(Guid id)
        => _webhookAppService.ToggleAsync(id);
}
