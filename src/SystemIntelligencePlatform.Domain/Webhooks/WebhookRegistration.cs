using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SystemIntelligencePlatform.Webhooks;

public class WebhookRegistration : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    public string Url { get; private set; } = null!;
    public string? Secret { get; private set; }
    public bool IsActive { get; private set; }

    protected WebhookRegistration() { }

    public WebhookRegistration(Guid id, string url, Guid? tenantId = null, string? secret = null) : base(id)
    {
        TenantId = tenantId;
        Url = url;
        Secret = secret;
        IsActive = true;
    }

    public void UpdateUrl(string url) => Url = url;
    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
