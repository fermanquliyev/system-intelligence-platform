using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SystemIntelligencePlatform.Webhooks;

public interface IWebhookAppService : IApplicationService
{
    Task<WebhookRegistrationDto> CreateAsync(CreateWebhookDto input);
    Task<ListResultDto<WebhookRegistrationDto>> GetListAsync();
    Task DeleteAsync(Guid id);
    Task<WebhookRegistrationDto> ToggleAsync(Guid id);
}
