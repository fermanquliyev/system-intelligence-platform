using System;
using System.Linq;
using System.Threading.Tasks;
using SystemIntelligencePlatform.Subscriptions;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace SystemIntelligencePlatform.Webhooks;

public class WebhookAppService : ApplicationService, IWebhookAppService
{
    private readonly IRepository<WebhookRegistration, Guid> _webhookRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly ICurrentTenant _currentTenant;

    public WebhookAppService(
        IRepository<WebhookRegistration, Guid> webhookRepository,
        ISubscriptionRepository subscriptionRepository,
        ICurrentTenant currentTenant)
    {
        _webhookRepository = webhookRepository;
        _subscriptionRepository = subscriptionRepository;
        _currentTenant = currentTenant;
    }

    public async Task<WebhookRegistrationDto> CreateAsync(CreateWebhookDto input)
    {
        await EnsureProPlanAsync();

        var webhook = new WebhookRegistration(
            GuidGenerator.Create(), input.Url, _currentTenant.Id, input.Secret);
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

    private async Task EnsureProPlanAsync()
    {
        var subscription = await _subscriptionRepository.FindByTenantIdAsync(_currentTenant.Id);
        var plan = subscription?.Plan ?? SubscriptionPlan.Free;
        if (!PlanLimits.GetLimits(plan).WebhookNotifications)
        {
            throw new BusinessException(SystemIntelligencePlatformDomainErrorCodes.FeatureNotAvailable)
                .WithData("feature", "Webhook Notifications")
                .WithData("plan", plan.ToString());
        }
    }
}
