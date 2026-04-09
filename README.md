# SystemIntelligencePlatform

Open-source log analysis and incident detection for teams that want full control of their data. SystemIntelligencePlatform ingests logs, detects anomalies, optionally enriches incidents with an LLM, and pushes live updates to the browser. It is **single-tenant**, **self-hosted**, and runs entirely on **PostgreSQL**, **RabbitMQ**, **ASP.NET Core**, and **Angular**—no SaaS, no tenant model, and no external cloud services required.

The repository is still named **system-intelligence-platform**; the product name for documentation and users is **SystemIntelligencePlatform**.

## Features

- **Log ingestion** — API key–authenticated HTTP endpoint; events are queued for async processing.
- **Incident detection** — Adaptive anomaly detection from recent log patterns.
- **AI-powered analysis** — `LlmIncidentAiAnalyzer` (Google Gemini-compatible API) with automatic fallback to `LocalIncidentAiAnalyzer` when no API key is set or the call fails.
- **Real-time updates** — SignalR broadcasts to all connected, authenticated clients.
- **Self-hosted** — One `docker-compose up` stack: Postgres, RabbitMQ, API, worker, Angular.

## Quick start

1. **Clone** this repository.

2. **Configure AI (optional)**  
   Copy `.env.example` to `.env` in the repo root and set your key:

   ```env
   AI__ApiKey=your-google-ai-studio-key
   ```

   Docker Compose reads `.env` for variable substitution. If `AI__ApiKey` is empty, the system still runs using the local analyzer only.

3. **Run the stack**

   ```bash
   docker-compose up --build
   ```

4. **Open the app**  
   - UI: [http://localhost:4200](http://localhost:4200)  
   - API / OpenIddict: [http://localhost:44397](http://localhost:44397)  
   - RabbitMQ management: [http://localhost:15672](http://localhost:15672) (guest / guest)

5. **Sign in**  
   Default admin user is seeded by ABP (see `SystemIntelligencePlatformConsts` for email/password defaults in development).

## Architecture

```
Angular → API → RabbitMQ (log-ingestion) → Worker → PostgreSQL
                              ↓
                    RabbitMQ (incident-notifications)
                              ↓
                         API → SignalR → Angular
```

- **API** (`SystemIntelligencePlatform.HttpApi.Host`) — REST, OpenIddict, SignalR hub, optional RabbitMQ consumer for incident notifications.
- **Worker** (`SystemIntelligencePlatform.BackgroundWorker`) — Consumes log messages, persists events, runs detection and AI, publishes incident notifications.
- **Database** — EF Core + PostgreSQL; search uses `DatabaseIncidentSearchService` (SQL `ILIKE`, no Elasticsearch).

## Configuration

| Area | Setting | Notes |
|------|---------|--------|
| AI | `AI:Provider`, `AI:ApiKey`, `AI:Model` | Bound to `GoogleAiOptions`; worker and API use the same shape. |
| Data retention | `DataRetention:LogRetentionDays` | Background job deletes old `LogEvent` rows (default 90). |
| Queue | `RabbitMQ:*` | Host, port, credentials, virtual host. |

Override with environment variables (e.g. `AI__ApiKey`, `DataRetention__LogRetentionDays`) or a `.env` file for Docker Compose.

## Customization

- **Replace the AI provider** — Implement `IIncidentAiAnalyzer` or adapt `LlmIncidentAiAnalyzer` / `ServiceCollectionExtensions.AddLlmIncidentAiAnalyzer` for another HTTP API; keep `LocalIncidentAiAnalyzer` as fallback.
- **Tune detection** — `AnomalyDetectionService` in the domain/application layer defines thresholds and rules.
- **Extend the model** — Add entities under `Domain`, register in `SystemIntelligencePlatformDbContext`, expose via application services and Angular as usual for ABP.

## Local development (without Docker)

- Restore NuGet packages, set `ConnectionStrings:Default` and RabbitMQ in `appsettings.Development.json`.
- Run the DbMigrator or apply EF migrations to PostgreSQL.
- Start `HttpApi.Host`, `BackgroundWorker`, and `ng serve` for the Angular app.

## License and contributions

Treat this as an open-source, self-hosted project: fork, run on your infrastructure, and adjust to your policies. Pull requests and issues are welcome.

## Tests

```bash
dotnet test
```

---

**Summary:** SystemIntelligencePlatform is a production-style, readable codebase focused on one deployment per installation—no multi-tenancy, no subscriptions, and minimal moving parts beyond Postgres, RabbitMQ, and your optional LLM API key.
