using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SystemIntelligencePlatform.Webhooks;
using Volo.Abp.DependencyInjection;

namespace SystemIntelligencePlatform.Fakes;

[Dependency(ReplaceServices = true)]
public class FakeWebhookDispatcher : IWebhookDispatcher, ITransientDependency
{
    public List<WebhookPayload> DispatchedPayloads { get; } = new();

    public Task DispatchIncidentCreatedAsync(Guid? tenantId, WebhookPayload payload)
    {
        DispatchedPayloads.Add(payload);
        return Task.CompletedTask;
    }
}
