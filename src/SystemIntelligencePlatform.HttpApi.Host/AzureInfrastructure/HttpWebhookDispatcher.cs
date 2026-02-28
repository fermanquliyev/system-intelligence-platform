using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SystemIntelligencePlatform.Webhooks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace SystemIntelligencePlatform.AzureInfrastructure;

public class HttpWebhookDispatcher : IWebhookDispatcher, ITransientDependency
{
    private readonly IRepository<WebhookRegistration, Guid> _webhookRepository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HttpWebhookDispatcher> _logger;

    public HttpWebhookDispatcher(
        IRepository<WebhookRegistration, Guid> webhookRepository,
        IHttpClientFactory httpClientFactory,
        ILogger<HttpWebhookDispatcher> logger)
    {
        _webhookRepository = webhookRepository;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task DispatchIncidentCreatedAsync(Guid? tenantId, WebhookPayload payload)
    {
        var queryable = await _webhookRepository.GetQueryableAsync();
        var webhooks = queryable.Where(w => w.IsActive && w.TenantId == tenantId).ToList();

        if (webhooks.Count == 0) return;

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        foreach (var webhook in webhooks)
        {
            await SendWithRetryAsync(webhook.Url, json, webhook.Secret);
        }
    }

    private async Task SendWithRetryAsync(string url, string json, string? secret)
    {
        var client = _httpClientFactory.CreateClient("WebhookClient");

        for (int attempt = 0; attempt < WebhookConsts.MaxRetryAttempts; attempt++)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };

                if (!string.IsNullOrEmpty(secret))
                {
                    request.Headers.Add("X-Webhook-Secret", secret);
                }

                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Webhook delivered to {Url}", url);
                    return;
                }

                _logger.LogWarning("Webhook to {Url} returned {StatusCode}, attempt {Attempt}",
                    url, response.StatusCode, attempt + 1);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Webhook to {Url} failed, attempt {Attempt}", url, attempt + 1);
            }

            if (attempt < WebhookConsts.MaxRetryAttempts - 1)
            {
                await Task.Delay(TimeSpan.FromSeconds(WebhookConsts.RetryDelaySeconds * Math.Pow(2, attempt)));
            }
        }

        _logger.LogError("Webhook delivery to {Url} exhausted all retries", url);
    }
}
