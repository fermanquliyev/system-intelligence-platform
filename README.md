# System Intelligence Platform — Self-Hosted Error Intelligence for .NET

> Monitor your .NET applications, detect anomalies in real-time, and get root cause analysis with actionable fix suggestions. **Fully self-hostable and cloud-agnostic.**

## What It Does

The platform ingests application logs, runs adaptive anomaly detection, enriches incidents with local AI analysis, and delivers real-time notifications. No Azure or other cloud lock-in: everything runs on **PostgreSQL**, **RabbitMQ**, **ASP.NET Core SignalR**, and a **.NET Background Worker**.

## Features

- **Adaptive Anomaly Detection** — Spikes, bursts, and critical errors detected using statistical baselines per application
- **Local AI Analysis** — Deterministic key phrase extraction, root cause summary, and suggested fixes (no external APIs)
- **Real-Time Dashboard** — Live incident feed via self-hosted SignalR with severity charts and trend analysis
- **Multi-Tenant Architecture** — Isolated data, API keys, and billing per tenant
- **Webhook Notifications** — HTTP callbacks to Slack, Teams, or any endpoint when incidents occur
- **Full-Text Search** — Search incidents by title, description, and root cause (PostgreSQL, no external search engine)
- **API-First** — REST API for log ingestion with API key authentication
- **OpenTelemetry** — Distributed tracing, metrics, and structured logging (Console exporter by default; Prometheus can be added)

## Architecture Flow

```
Angular → API (HttpApi.Host) → RabbitMQ (log-ingestion)
                                    ↓
                    BackgroundWorker consumes queue
                                    ↓
                    Incident detection + Local AI + PostgreSQL
                                    ↓
                    RabbitMQ (incident-notifications)
                                    ↓
                    API consumes → SignalR → Angular clients
```

- **Database**: PostgreSQL (EF Core + Npgsql). No changes to DbContext, repositories, or migrations.
- **Message queue**: RabbitMQ (durable queue `log-ingestion`). JSON serialization, retry and reconnection.
- **Background processing**: `SystemIntelligencePlatform.BackgroundWorker` (HostedService) instead of Azure Functions.
- **Search**: `DatabaseIncidentSearchService` — EF Core queries with ILIKE; tenant filtering, pagination, sort by timestamp.
- **Realtime**: Self-hosted ASP.NET Core SignalR (`IncidentHub`); no Azure SignalR.
- **Secrets**: appsettings.json and environment variables only (no Azure Key Vault).
- **Telemetry**: OpenTelemetry (tracing, metrics); Application Insights removed.

## Tech Stack

- **Backend**: ABP.io Framework (.NET 10, EF Core)
- **Frontend**: Angular with ABP UI
- **Database**: PostgreSQL 16
- **Messaging**: RabbitMQ 3 (with management UI on 15672)
- **Processing**: .NET Worker Service (BackgroundWorker)
- **AI**: Local deterministic analyzer (pure C#)
- **Search**: PostgreSQL full-text (EF Core)
- **Real-Time**: ASP.NET Core SignalR
- **Auth**: OpenIddict (OAuth 2.0 / OIDC)

## Quick Start (Local with Docker Compose)

### 1. Start infrastructure

```bash
docker-compose up -d postgres rabbitmq
```

- **PostgreSQL**: `localhost:5432` (user `postgres`, password `postgres`, database `sip`)
- **RabbitMQ**: AMQP `localhost:5672`, Management UI `http://localhost:15672` (guest/guest)

### 2. Run database migrations

```bash
cd src/SystemIntelligencePlatform.DbMigrator
dotnet run
```

### 3. Start the API

```bash
cd src/SystemIntelligencePlatform.HttpApi.Host
dotnet run
```

API runs at `https://localhost:44397` (or the port in `appsettings.json`).

### 4. Start the Background Worker

```bash
cd src/SystemIntelligencePlatform.BackgroundWorker
dotnet run
```

### 5. Start the Angular application

```bash
cd angular
npm install
npm start
```

Angular at `http://localhost:4200`.

## Running the Full Stack with Docker Compose

From the repository root:

```bash
docker-compose up --build
```

This starts:

- **postgres** — PostgreSQL 16 on 5432
- **rabbitmq** — RabbitMQ with management on 5672 and 15672
- **api** — ASP.NET Core API (port 44397 → 8080)
- **worker** — Background worker (consumes `log-ingestion`, publishes to `incident-notifications`)
- **angular** — Angular app served by nginx (port 4200 → 80)

Run migrations before or after `docker-compose up` (e.g. run DbMigrator once against the `postgres` service or a local PostgreSQL instance using the same connection string).

## Configuration

Use `appsettings.json` and/or environment variables. No Azure sections.

**API (HttpApi.Host)** — Example:

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=sip;Username=postgres;Password=postgres"
  },
  "RabbitMQ": {
    "Host": "localhost",
    "Port": 5672,
    "Username": "guest",
    "Password": "guest",
    "VirtualHost": "/"
  },
  "Search": { "Provider": "Database" },
  "OpenTelemetry": { "Enabled": true }
}
```

**Worker (BackgroundWorker)** — Same `ConnectionStrings:Default` and `RabbitMQ` section.

Secrets (e.g. Stripe, auth) can be overridden via environment variables or additional config files.

## Tests

All tests run without any external cloud services:

```bash
dotnet test SystemIntelligencePlatform.slnx
```

- **Domain / Application / EntityFrameworkCore** — Existing tests unchanged
- **Infrastructure.Tests** — Local AI analyzer (deterministic), database search (with in-memory SQLite in tests), and related behavior

## Success Criteria (Met)

- Solution builds successfully
- All tests pass
- No Azure packages or configuration in the codebase
- System runs locally with `docker-compose up` using PostgreSQL, RabbitMQ, SignalR, and the Background Worker
- Database layer remains PostgreSQL with EF Core + Npgsql; DDD and business logic unchanged

## License

Proprietary. Copyright © 2026.
