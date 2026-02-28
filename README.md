# ErrorIntel — AI-Powered Error Intelligence for .NET Teams

> Stop debugging blindly. Let AI tell you what went wrong, why, and how to fix it.

## What is ErrorIntel?

ErrorIntel is a SaaS platform that monitors your .NET applications, detects anomalies in real-time, and uses AI to provide root cause analysis with actionable fix suggestions.

Built for .NET SaaS teams running on Azure.

## Features

- **Adaptive Anomaly Detection** - Automatically detects spikes, bursts, and critical errors using statistical baselines per application
- **AI Root Cause Analysis** - Every incident gets an AI-generated summary explaining what happened, why, and how to fix it (Pro plan)
- **Real-Time Dashboard** - Live incident feed powered by Azure SignalR with severity charts and trend analysis  
- **Multi-Tenant Architecture** - Each team gets isolated data, API keys, and billing
- **Webhook Notifications** - Get instant alerts in Slack, Teams, or any HTTP endpoint when incidents occur (Pro plan)
- **Smart Retention** - Automatic archival of old logs with configurable retention per plan
- **Full-Text Search** - Search across all incidents and logs using Azure AI Search
- **API-First** - Simple REST API for log ingestion with API key authentication

## How It Works

1. **Install** — Add our NuGet package or send logs via REST API
2. **Ingest** — Your application logs flow through Azure Service Bus for reliable async processing  
3. **Detect** — Adaptive anomaly detection identifies issues in real-time
4. **Analyze** — Azure AI generates root cause summaries and fix suggestions
5. **Alert** — Get notified via dashboard, SignalR, or webhooks

## Pricing

| | Free | Pro | Enterprise |
|---|---|---|---|
| Logs/month | 10,000 | 500,000 | 10,000,000 |
| Applications | 3 | 20 | 100 |
| Retention | 7 days | 30 days | 90 days |
| AI Root Cause | ❌ | ✅ | ✅ |
| Webhooks | ❌ | ✅ | ✅ |
| Price | Free | $49/mo | Contact us |

## Tech Stack

- **Backend**: ABP.io Framework (.NET 10, EF Core)
- **Frontend**: Angular 19 with ABP UI
- **Database**: Azure SQL (serverless)
- **Messaging**: Azure Service Bus
- **Processing**: Azure Functions (isolated worker)
- **AI**: Azure Language Services (sentiment, key phrases, entity recognition)
- **Search**: Azure AI Search
- **Real-Time**: Azure SignalR Service
- **Auth**: OpenIddict (OAuth 2.0 / OIDC)

## Quick Start

### Prerequisites

- .NET 10 SDK
- Node.js 20+
- Azure subscription (or local development with emulators)

### Local Development

```bash
# Clone
git clone https://github.com/your-org/ErrorIntel.git
cd ErrorIntel

# Backend
dotnet build SystemIntelligencePlatform.slnx
cd src/SystemIntelligencePlatform.HttpApi.Host
dotnet run

# Frontend
cd angular
npm install
npm start
```

### Configuration

Set these in `appsettings.json` or Azure Key Vault:

```json
{
  "ConnectionStrings": {
    "Default": "Server=...;Database=ErrorIntel;..."
  },
  "Stripe": {
    "SecretKey": "sk_...",
    "WebhookSecret": "whsec_...",
    "ProPriceId": "price_...",
    "EnterprisePriceId": "price_..."
  },
  "Azure": {
    "ServiceBus": { "ConnectionString": "..." },
    "Language": { "Endpoint": "...", "Key": "..." },
    "Search": { "Endpoint": "...", "Key": "...", "IndexName": "incidents-index" },
    "SignalR": { "ConnectionString": "..." }
  }
}
```

### Running Tests

```bash
dotnet test SystemIntelligencePlatform.slnx
```

## Testing Strategy

- **Domain Tests**: Pure unit tests for entities, value objects, and domain services (anomaly detection, plan limits, subscription lifecycle)
- **Application Tests**: Service-level tests for plan enforcement, usage tracking, and webhook management
- **EF Core Tests**: Integration tests with in-memory SQLite for repository operations, data retention, and query behavior
- **Function Tests**: Unit tests for Azure Function processing logic using mocked dependencies

## Deployment

Infrastructure is defined in Bicep templates (`/infra`):

```bash
az deployment group create \
  --resource-group errorintel-rg \
  --template-file infra/main.bicep \
  --parameters environment=production
```

CI/CD via GitHub Actions (`.github/workflows/`).

## Roadmap

- [ ] Slack/Teams native integrations
- [ ] Custom anomaly detection rules
- [ ] Log replay and incident timeline
- [ ] Multi-region deployment
- [ ] SOC 2 compliance
- [ ] Mobile app for on-call engineers
- [ ] AI-powered incident grouping across services

## License

Proprietary. Copyright © 2026 ErrorIntel.
