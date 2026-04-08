using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace SystemIntelligencePlatform.Webhooks;

public class WebhookRegistration : FullAuditedAggregateRoot<Guid>
{
    public string Url { get; private set; } = null!;
    public string? Secret { get; private set; }
    public bool IsActive { get; private set; }

    protected WebhookRegistration() { }

    public WebhookRegistration(Guid id, string url, string? secret = null) : base(id)
    {
        Url = url;
        Secret = secret;
        IsActive = true;
    }

    public void UpdateUrl(string url) => Url = url;
    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
