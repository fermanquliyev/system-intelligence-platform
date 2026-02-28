using System;
using Volo.Abp.Application.Dtos;

namespace SystemIntelligencePlatform.Webhooks;

public class WebhookRegistrationDto : EntityDto<Guid>
{
    public string Url { get; set; } = null!;
    public bool IsActive { get; set; }
}

public class CreateWebhookDto
{
    public string Url { get; set; } = null!;
    public string? Secret { get; set; }
}
