using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SystemIntelligencePlatform.Subscriptions;

namespace SystemIntelligencePlatform.Stripe;

/// <summary>
/// Handles Stripe webhook events for subscription lifecycle management.
/// Stripe sends JSON payloads to this endpoint when subscription status changes.
/// In production, validate the webhook signature using Stripe's signing secret.
/// </summary>
[Route("api/stripe/webhook")]
[AllowAnonymous]
[ApiExplorerSettings(IgnoreApi = true)]
public class StripeWebhookController : ControllerBase
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<StripeWebhookController> _logger;

    public StripeWebhookController(
        ISubscriptionRepository subscriptionRepository,
        IConfiguration configuration,
        ILogger<StripeWebhookController> logger)
    {
        _subscriptionRepository = subscriptionRepository;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> HandleWebhookAsync()
    {
        string json;
        using (var reader = new StreamReader(HttpContext.Request.Body))
        {
            json = await reader.ReadToEndAsync();
        }

        // In production, validate Stripe signature:
        // var sigHeader = Request.Headers["Stripe-Signature"];
        // var evt = EventUtility.ConstructEvent(json, sigHeader, webhookSecret);

        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var eventType = root.GetProperty("type").GetString();

        _logger.LogInformation("Stripe webhook received: {EventType}", eventType);

        switch (eventType)
        {
            case "customer.subscription.created":
            case "customer.subscription.updated":
                await HandleSubscriptionChangeAsync(root);
                break;

            case "customer.subscription.deleted":
                await HandleSubscriptionDeletedAsync(root);
                break;

            default:
                _logger.LogInformation("Unhandled Stripe event type: {EventType}", eventType);
                break;
        }

        return Ok();
    }

    private async Task HandleSubscriptionChangeAsync(JsonElement root)
    {
        var data = root.GetProperty("data").GetProperty("object");
        var stripeSubId = data.GetProperty("id").GetString()!;
        var stripeCustomerId = data.GetProperty("customer").GetString()!;
        var status = data.GetProperty("status").GetString();

        var proPriceId = _configuration["Stripe:ProPriceId"];
        var enterprisePriceId = _configuration["Stripe:EnterprisePriceId"];

        var priceId = data.GetProperty("items").GetProperty("data")[0]
            .GetProperty("price").GetProperty("id").GetString();

        var plan = priceId == enterprisePriceId ? SubscriptionPlan.Enterprise :
                   priceId == proPriceId ? SubscriptionPlan.Pro :
                   SubscriptionPlan.Free;

        var subscription = await _subscriptionRepository.FindByStripeSubscriptionIdAsync(stripeSubId);
        if (subscription == null)
        {
            // Look up by customer metadata or create - for MVP, log and skip
            _logger.LogWarning("No subscription found for Stripe sub {StripeSubId}", stripeSubId);
            return;
        }

        subscription.ChangePlan(plan);
        subscription.UpdateStatus(status switch
        {
            "active" => SubscriptionStatus.Active,
            "past_due" => SubscriptionStatus.PastDue,
            "canceled" => SubscriptionStatus.Canceled,
            _ => SubscriptionStatus.Active
        });

        if (data.TryGetProperty("current_period_start", out var startProp) &&
            data.TryGetProperty("current_period_end", out var endProp))
        {
            var start = DateTimeOffset.FromUnixTimeSeconds(startProp.GetInt64()).UtcDateTime;
            var end = DateTimeOffset.FromUnixTimeSeconds(endProp.GetInt64()).UtcDateTime;
            subscription.RenewPeriod(start, end);
        }

        await _subscriptionRepository.UpdateAsync(subscription);
        _logger.LogInformation("Updated subscription {SubId} to plan {Plan}", subscription.Id, plan);
    }

    private async Task HandleSubscriptionDeletedAsync(JsonElement root)
    {
        var data = root.GetProperty("data").GetProperty("object");
        var stripeSubId = data.GetProperty("id").GetString()!;

        var subscription = await _subscriptionRepository.FindByStripeSubscriptionIdAsync(stripeSubId);
        if (subscription == null) return;

        subscription.Cancel();
        subscription.ChangePlan(SubscriptionPlan.Free);
        await _subscriptionRepository.UpdateAsync(subscription);

        _logger.LogInformation("Subscription {SubId} cancelled, reverted to Free", subscription.Id);
    }
}
