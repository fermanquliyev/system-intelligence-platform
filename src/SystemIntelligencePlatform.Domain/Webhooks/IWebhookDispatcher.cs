using System;
using System.Threading.Tasks;

namespace SystemIntelligencePlatform.Webhooks;

public interface IWebhookDispatcher
{
    Task DispatchIncidentCreatedAsync(Guid? tenantId, WebhookPayload payload);
}

public class WebhookPayload
{
    public Guid IncidentId { get; set; }
    public string Severity { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? RootCauseSummary { get; set; }
    public string ApplicationName { get; set; } = null!;
    public DateTime Timestamp { get; set; }
}
