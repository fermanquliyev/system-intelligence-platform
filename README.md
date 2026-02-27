# System Intelligence Platform

An Azure-native multi-tenant incident & system intelligence platform built on ABP.io Framework, designed to ingest, analyze, and manage log events at scale with AI-powered incident detection and real-time notifications.

## Architecture Overview

The System Intelligence Platform follows an event-driven architecture pattern, leveraging Azure cloud services for scalability, reliability, and intelligent processing.

```
┌─────────────┐
│   Client    │ (Angular SPA / API Consumers)
└──────┬──────┘
       │ HTTPS
       ▼
┌─────────────────────┐
│  API Management     │ (Azure API Management - Optional)
└──────┬──────────────┘
       │
       ▼
┌─────────────────────┐
│   App Service       │ (ASP.NET Core API)
│   HttpApi.Host      │
└──────┬──────────────┘
       │
       ├─────────────────┐
       │                 │
       ▼                 ▼
┌──────────────┐  ┌──────────────────┐
│ Service Bus  │  │  Azure SQL DB    │
│   (Queue)    │  │   (Serverless)   │
└──────┬───────┘  └──────────────────┘
       │
       ▼
┌─────────────────────┐
│  Azure Functions    │ (Isolated Worker)
│  IncidentProcessor  │
└──────┬──────────────┘
       │
       ├─────────────────┬─────────────────┬─────────────────┐
       │                 │                 │                 │
       ▼                 ▼                 ▼                 ▼
┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐
│ Azure SQL DB │  │ Azure AI     │  │ Azure AI     │  │ Azure        │
│              │  │ Language     │  │ Search       │  │ SignalR      │
│              │  │ Service      │  │              │  │              │
└──────────────┘  └──────────────┘  └──────────────┘  └──────┬───────┘
                                                               │
                                                               ▼
                                                      ┌─────────────────┐
                                                      │   Dashboard     │
                                                      │   (Real-time)   │
                                                      └─────────────────┘
```

### Event-Driven Flow

1. **Log Ingestion**: Clients send log events via REST API with API Key authentication
2. **Validation & Queuing**: API validates and enqueues events to Azure Service Bus
3. **Async Processing**: Azure Functions consume messages from Service Bus
4. **AI Analysis**: Functions analyze logs using Azure Language Service (sentiment, key phrases, entities)
5. **Incident Detection**: Hash-based deduplication groups similar events into incidents
6. **Search Indexing**: Incidents indexed in Azure AI Search for fast retrieval
7. **Real-time Updates**: SignalR broadcasts updates to tenant-scoped dashboard groups
8. **Data Persistence**: All data stored in Azure SQL Database with tenant isolation

## Technology Stack

### Backend
- **.NET 10.0** - Latest .NET runtime
- **ABP Framework 10.1.0** - Domain-driven design framework with multi-tenancy
- **Entity Framework Core** - ORM with code-first migrations
- **OpenIddict** - Authentication & authorization (via ABP)

### Frontend
- **Angular 21** - Modern web framework
- **ABP Angular UI** - Pre-built UI components and services
- **Lepton X Theme** - Professional admin theme

### Azure Services
- **Azure SQL Database (Serverless)** - Primary data store with auto-scaling
- **Azure Service Bus** - Message queuing for async processing
- **Azure Functions (Isolated Worker)** - Serverless compute for event processing
- **Azure SignalR Service** - Real-time bidirectional communication
- **Azure Language Service** - AI-powered text analysis (sentiment, key phrases, entities)
- **Azure AI Search** - Full-text search and indexing
- **Azure Key Vault** - Secrets management
- **Application Insights** - Application performance monitoring and telemetry

### Infrastructure & DevOps
- **Serilog** - Structured logging framework
- **GitHub Actions** - CI/CD pipeline automation

## Domain Model

### MonitoredApplication
Represents an application being monitored. Each application has:
- Unique name per tenant
- API Key (hashed) for authentication
- Environment designation (dev, staging, production)
- Active/inactive status

### LogEvent
Individual log entries ingested from monitored applications:
- Log level (Debug, Info, Warning, Error, Critical)
- Message content
- Source application identifier
- Exception details (type, stack trace)
- Hash signature for deduplication
- Correlation ID for distributed tracing
- Timestamp
- Associated incident (if grouped)

### Incident
Aggregated incidents created from similar log events:
- Title and description
- Severity (Low, Medium, High, Critical)
- Status (Open, Investigating, Resolved, Closed)
- Hash signature for grouping similar events
- Occurrence count and timestamps (first/last)
- AI enrichment fields:
  - Sentiment score
  - Key phrases
  - Extracted entities
- Resolution tracking (resolved at, resolved by)

### IncidentComment
User comments on incidents for collaboration:
- Content
- Creation timestamp and creator
- Tenant isolation

## Project Structure

```
SystemIntelligencePlatform/
├── src/
│   ├── SystemIntelligencePlatform.Domain.Shared/
│   │   └── Constants, enums, localization, DTOs shared across layers
│   │
│   ├── SystemIntelligencePlatform.Domain/
│   │   ├── Incidents/          # Incident aggregate root
│   │   ├── LogEvents/          # LogEvent entity
│   │   ├── MonitoredApplications/ # MonitoredApplication aggregate
│   │   └── Domain services and repository interfaces
│   │
│   ├── SystemIntelligencePlatform.Application.Contracts/
│   │   ├── Incidents/          # Incident DTOs and service interfaces
│   │   ├── LogIngestion/       # Log ingestion contracts
│   │   ├── MonitoredApplications/ # Application management contracts
│   │   └── Dashboard/          # Dashboard data contracts
│   │
│   ├── SystemIntelligencePlatform.Application/
│   │   ├── Incidents/          # Incident application services
│   │   ├── LogIngestion/       # Log ingestion orchestration
│   │   ├── MonitoredApplications/ # Application management services
│   │   └── Dashboard/          # Dashboard aggregation logic
│   │
│   ├── SystemIntelligencePlatform.EntityFrameworkCore/
│   │   ├── EntityFrameworkCore/
│   │   │   ├── SystemIntelligencePlatformDbContext.cs
│   │   │   └── Repositories/   # Custom repository implementations
│   │   └── Migrations/         # EF Core database migrations
│   │
│   ├── SystemIntelligencePlatform.HttpApi/
│   │   └── Controllers/        # REST API controllers
│   │       ├── IncidentSearchController.cs
│   │       └── LogIngestionController.cs
│   │
│   ├── SystemIntelligencePlatform.HttpApi.Client/
│   │   └── Proxies for remote API calls
│   │
│   ├── SystemIntelligencePlatform.HttpApi.Host/
│   │   └── Main API host application (ASP.NET Core)
│   │
│   ├── SystemIntelligencePlatform.AzureFunctions/
│   │   ├── Functions/
│   │   │   └── IncidentProcessorFunction.cs
│   │   └── Program.cs          # Functions host configuration
│   │
│   └── SystemIntelligencePlatform.DbMigrator/
│       └── Console app for database migrations and seeding
│
├── angular/
│   ├── src/
│   │   ├── app/
│   │   │   ├── applications/   # Monitored applications management
│   │   │   ├── incidents/      # Incidents UI
│   │   │   └── dashboard/      # Dashboard components
│   │   └── proxy/              # Auto-generated API proxies
│   └── package.json
│
├── test/
│   ├── SystemIntelligencePlatform.Domain.Tests/
│   ├── SystemIntelligencePlatform.Application.Tests/
│   ├── SystemIntelligencePlatform.EntityFrameworkCore.Tests/
│   └── SystemIntelligencePlatform.TestBase/
│
├── infra/                      # Infrastructure as Code (Bicep templates)
└── .github/workflows/
    └── ci-cd.yml              # CI/CD pipeline
```

## Prerequisites

- **.NET 10.0 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet)
- **Node.js 22+** - [Download](https://nodejs.org/)
- **Azure CLI** - [Install](https://docs.microsoft.com/cli/azure/install-azure-cli)
- **SQL Server** - LocalDB for local development (included with Visual Studio)
- **Git** - Version control

## Getting Started

### 1. Clone the Repository

```bash
git clone <repository-url>
cd SystemIntelligencePlatform
```

### 2. Install Client Dependencies

```bash
cd angular
npm install
cd ..
```

Or use ABP CLI:

```bash
abp install-libs
```

### 3. Configure Database Connection

Update `appsettings.json` in `SystemIntelligencePlatform.HttpApi.Host`:

```json
{
  "ConnectionStrings": {
    "Default": "Server=(LocalDb)\\MSSQLLocalDB;Database=SystemIntelligencePlatform;Trusted_Connection=True;TrustServerCertificate=true"
  }
}
```

### 4. Run Database Migrations

```bash
dotnet run --project src/SystemIntelligencePlatform.DbMigrator
```

This will:
- Create the database
- Apply all migrations
- Seed initial data (admin user, roles, etc.)

### 5. Generate Signing Certificate (First Time)

```bash
dotnet dev-certs https -v -ep openiddict.pfx -p 214b1bcb-5a7c-4e24-b52f-23b4ecad6689
```

Copy `openiddict.pfx` to `src/SystemIntelligencePlatform.HttpApi.Host/`.

### 6. Run the API

```bash
dotnet run --project src/SystemIntelligencePlatform.HttpApi.Host
```

API will be available at `https://localhost:44397`

### 7. Run Angular Application

```bash
cd angular
npm start
```

Angular app will be available at `http://localhost:4200`

### 8. Run Azure Functions Locally (Optional)

```bash
cd src/SystemIntelligencePlatform.AzureFunctions
func start
```

Configure `local.settings.json` with your Azure service connection strings.

### Default Credentials

After running the DbMigrator, default admin credentials:
- **Username**: `admin`
- **Password**: `1q2w3E*`

**⚠️ Change these credentials in production!**

## Azure Deployment

### Prerequisites

1. Azure subscription
2. Azure CLI installed and logged in (`az login`)
3. Resource group created

### Deploy Infrastructure (Bicep)

```bash
# Set variables
RESOURCE_GROUP="rg-systemintelligence-platform"
LOCATION="eastus"
DEPLOYMENT_NAME="deploy-$(date +%Y%m%d-%H%M%S)"

# Deploy infrastructure
az deployment group create \
  --resource-group $RESOURCE_GROUP \
  --template-file infra/main.bicep \
  --parameters @infra/parameters.json \
  --name $DEPLOYMENT_NAME
```

### Configure GitHub Secrets

Add the following secrets to your GitHub repository:

- `AZURE_WEBAPP_NAME` - Name of your Azure App Service
- `AZURE_WEBAPP_PUBLISH_PROFILE` - Publish profile from App Service (Download from Azure Portal)
- `AZURE_FUNCTIONAPP_NAME` - Name of your Azure Function App
- `AZURE_FUNCTIONAPP_PUBLISH_PROFILE` - Publish profile from Function App

### Manual Deployment

#### Deploy API

```bash
dotnet publish src/SystemIntelligencePlatform.HttpApi.Host/SystemIntelligencePlatform.HttpApi.Host.csproj \
  --configuration Release \
  --output ./publish/api

# Deploy using Azure CLI
az webapp deploy \
  --resource-group $RESOURCE_GROUP \
  --name $AZURE_WEBAPP_NAME \
  --src-path ./publish/api \
  --type zip
```

#### Deploy Functions

```bash
dotnet publish src/SystemIntelligencePlatform.AzureFunctions/SystemIntelligencePlatform.AzureFunctions.csproj \
  --configuration Release \
  --output ./publish/functions

# Deploy using Azure CLI
func azure functionapp publish $AZURE_FUNCTIONAPP_NAME
```

## Configuration

### appsettings.json Structure

```json
{
  "App": {
    "SelfUrl": "https://your-api.azurewebsites.net",
    "AngularUrl": "https://your-app.azurewebsites.net",
    "CorsOrigins": "https://your-app.azurewebsites.net"
  },
  "ConnectionStrings": {
    "Default": "Server=tcp:your-server.database.windows.net,1433;Database=SystemIntelligencePlatform;..."
  },
  "Azure": {
    "KeyVault": {
      "VaultUri": "https://your-keyvault.vault.azure.net/"
    },
    "ServiceBus": {
      "ConnectionString": "Endpoint=sb://..."
    },
    "Language": {
      "Endpoint": "https://your-language.cognitiveservices.azure.com/",
      "Key": "from-keyvault"
    },
    "Search": {
      "Endpoint": "https://your-search.search.windows.net",
      "Key": "from-keyvault",
      "IndexName": "incidents-index"
    },
    "SignalR": {
      "ConnectionString": "Endpoint=https://..."
    },
    "ApplicationInsights": {
      "ConnectionString": "InstrumentationKey=..."
    }
  }
}
```

### Azure Key Vault Integration

The application automatically loads secrets from Azure Key Vault when `Azure:KeyVault:VaultUri` is configured. Use Managed Identity in production for secure access.

Secrets stored in Key Vault:
- Database connection strings
- Azure service keys (Service Bus, Language Service, AI Search, SignalR)
- API keys
- Certificate passwords

## Security

### API Key Authentication (Log Ingestion)

- API keys are hashed using SHA-256 before storage
- Constant-time comparison prevents timing attacks
- Keys are tenant-scoped and can be regenerated

### Tenant Isolation

- All entities implement `IMultiTenant`
- Data access automatically filtered by `TenantId`
- Cross-tenant data access prevented at the repository level

### Authentication & Authorization

- **OpenIddict** (via ABP) for user authentication
- **Azure AD** integration supported through ABP
- **Role-based access control**:
  - **Admin**: Full system access
  - **Developer**: Application and incident management
  - **Viewer**: Read-only access

### API Security

- HTTPS enforced in production
- CORS configured for allowed origins
- API rate limiting (configurable)
- Input validation on all endpoints

## Observability

### Application Insights

- Automatic dependency tracking
- Request/response telemetry
- Exception tracking with stack traces
- Custom events and metrics

### Structured Logging (Serilog)

- JSON-formatted logs
- Correlation IDs on all log events
- Log levels: Debug, Information, Warning, Error, Critical
- Sinks:
  - File (local development)
  - Console
  - Application Insights (production)

### Distributed Tracing

- Correlation IDs propagated across:
  - HTTP requests
  - Service Bus messages
  - Azure Functions
  - Database queries
- End-to-end request tracking

### Health Checks

- `/health-status` endpoint
- Database connectivity checks
- Azure service connectivity checks
- Custom health indicators

## Scaling Considerations

The platform is designed to handle **1M+ logs per day** with the following optimizations:

### Database

- **Bulk inserts** for LogEvent ingestion (batched writes)
- **No-tracking queries** for read-only operations
- **Indexed columns** on frequently queried fields:
  - `(ApplicationId, HashSignature, Timestamp)` on LogEvents
  - `TenantId` on all entities
  - `Timestamp` for time-range queries
- **Partitioned tables** (by TenantId + Timestamp) ready for future growth
- **Azure SQL Serverless** auto-pauses when idle, scales compute automatically

### Messaging

- **Azure Service Bus** handles backpressure automatically
- Queue-based decoupling prevents API overload
- Dead-letter queue for failed messages

### Compute

- **Azure Functions** auto-scale based on queue depth
- Isolated worker model for better performance
- Parallel processing of multiple messages

### Real-time Updates

- **Azure SignalR** fan-out for tenant-scoped groups
- Efficient message routing
- Connection management handled by Azure

### Caching Strategy

- Application-level caching for MonitoredApplications (API key lookups)
- EF Core query caching for frequently accessed data
- Consider Redis Cache for high-traffic scenarios

## API Reference

### Log Ingestion

**POST** `/api/log-ingestion/ingest`

Ingest log events from monitored applications.

- **Authentication**: API Key (header: `X-API-Key`)
- **Content-Type**: `application/json`
- **Body**: Array of log event objects

### Incidents

**GET** `/api/app/incidents`

List incidents with filtering and pagination.

**GET** `/api/app/incidents/{id}`

Get incident details.

**PUT** `/api/app/incidents/{id}/resolve`

Resolve an incident.

**POST** `/api/app/incidents/{id}/comments`

Add a comment to an incident.

### Monitored Applications

**GET** `/api/app/monitored-applications`

List monitored applications.

**POST** `/api/app/monitored-applications`

Create a new monitored application.

**PUT** `/api/app/monitored-applications/{id}/regenerate-key`

Regenerate API key for an application.

### Dashboard

**GET** `/api/app/dashboard/stats`

Get dashboard statistics (incident counts, recent activity).

### Search

**GET** `/api/app/incident-search/search`

Full-text search incidents using Azure AI Search.

## License

MIT License - see LICENSE file for details.
