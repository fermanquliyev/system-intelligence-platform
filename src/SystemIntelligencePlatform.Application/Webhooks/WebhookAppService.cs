using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SystemIntelligencePlatform.Webhooks;

public class WebhookAppService : ApplicationService, IWebhookAppService
{
    private readonly IRepository<WebhookRegistration, Guid> _webhookRepository;

    public WebhookAppService(IRepository<WebhookRegistration, Guid> webhookRepository)
    {
        _webhookRepository = webhookRepository;
    }

    public async Task<WebhookRegistrationDto> CreateAsync(CreateWebhookDto input)
    {
        var webhook = new WebhookRegistration(GuidGenerator.Create(), input.Url, input.Secret);
        await _webhookRepository.InsertAsync(webhook);

        return new WebhookRegistrationDto
        {
            Id = webhook.Id,
            Url = webhook.Url,
            IsActive = webhook.IsActive
        };
    }

    public async Task<ListResultDto<WebhookRegistrationDto>> GetListAsync()
    {
        var queryable = await _webhookRepository.GetQueryableAsync();
        var items = await AsyncExecuter.ToListAsync(queryable);

        return new ListResultDto<WebhookRegistrationDto>(
            items.Select(w => new WebhookRegistrationDto
            {
                Id = w.Id,
                Url = w.Url,
                IsActive = w.IsActive
            }).ToList());
    }

    public async Task DeleteAsync(Guid id)
    {
        await _webhookRepository.DeleteAsync(id);
    }

    public async Task<WebhookRegistrationDto> ToggleAsync(Guid id)
    {
        var webhook = await _webhookRepository.GetAsync(id);
        if (webhook.IsActive) webhook.Deactivate();
        else webhook.Activate();
        await _webhookRepository.UpdateAsync(webhook);

        return new WebhookRegistrationDto
        {
            Id = webhook.Id,
            Url = webhook.Url,
            IsActive = webhook.IsActive
        };
    }
}
