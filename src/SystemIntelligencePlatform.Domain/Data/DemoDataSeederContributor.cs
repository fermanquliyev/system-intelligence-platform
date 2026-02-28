using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using SystemIntelligencePlatform.Incidents;
using SystemIntelligencePlatform.MonitoredApplications;
using SystemIntelligencePlatform.Subscriptions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;

namespace SystemIntelligencePlatform.Data;

public class DemoDataSeederContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<MonitoredApplication, Guid> _applicationRepository;
    private readonly IRepository<Incident, Guid> _incidentRepository;
    private readonly IRepository<Subscription, Guid> _subscriptionRepository;
    private readonly IGuidGenerator _guidGenerator;

    public DemoDataSeederContributor(
        IRepository<MonitoredApplication, Guid> applicationRepository,
        IRepository<Incident, Guid> incidentRepository,
        IRepository<Subscription, Guid> subscriptionRepository,
        IGuidGenerator guidGenerator)
    {
        _applicationRepository = applicationRepository;
        _incidentRepository = incidentRepository;
        _subscriptionRepository = subscriptionRepository;
        _guidGenerator = guidGenerator;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        // Only seed if no applications exist (avoid duplicates)
        if (await _applicationRepository.GetCountAsync() > 0)
            return;

        // 1. Create Free subscription for demo tenant (null tenantId)
        var subscription = new Subscription(
            _guidGenerator.Create(),
            SubscriptionPlan.Free,
            tenantId: null);
        await _subscriptionRepository.InsertAsync(subscription);

        // 2. Create three MonitoredApplications
        var appId1 = _guidGenerator.Create();
        var appId2 = _guidGenerator.Create();
        var appId3 = _guidGenerator.Create();

        var app1 = new MonitoredApplication(
            appId1,
            "Contoso API",
            ApiKeyGenerator.Hash("demo-api-key-contoso"),
            tenantId: null)
        {
            IsActive = true,
            Description = "Main API service for Contoso e-commerce platform",
            Environment = "Production"
        };

        var app2 = new MonitoredApplication(
            appId2,
            "Northwind Web",
            ApiKeyGenerator.Hash("demo-api-key-northwind"),
            tenantId: null)
        {
            IsActive = true,
            Description = "Web application frontend for Northwind trading",
            Environment = "Production"
        };

        var app3 = new MonitoredApplication(
            appId3,
            "AdventureWorks Worker",
            ApiKeyGenerator.Hash("demo-api-key-adventureworks"),
            tenantId: null)
        {
            IsActive = true,
            Description = "Background worker service for AdventureWorks data processing",
            Environment = "Production"
        };

        await _applicationRepository.InsertAsync(app1);
        await _applicationRepository.InsertAsync(app2);
        await _applicationRepository.InsertAsync(app3);

        // 3. Create sample incidents with AI analysis
        var baseTime = DateTime.UtcNow.AddDays(-7);

        // Incident 1: Critical - NullReferenceException
        var incident1Hash = ComputeHashSignature("NullReferenceException in OrderService.ProcessPayment", "OrderService", "NullReferenceException");
        var incident1 = new Incident(
            _guidGenerator.Create(),
            appId1,
            "NullReferenceException in OrderService.ProcessPayment",
            incident1Hash,
            IncidentSeverity.Critical,
            baseTime.AddDays(-5),
            tenantId: null)
        {
            Description = "Payment processing encounters null customer object when session expires during checkout flow.",
            OccurrenceCount = 156
        };
        incident1.EnrichWithAiAnalysis(new AiAnalysisResult
        {
            SentimentScore = 0.15,
            KeyPhrases = new List<string> { "NullReferenceException", "OrderService", "ProcessPayment", "customer object", "session expires" },
            Entities = new List<string> { "OrderService:Service", "Payment:Transaction", "Customer:Entity" },
            RootCauseSummary = "Payment processing encounters null customer object when session expires during checkout flow. The OrderService.ProcessPayment method attempts to access customer properties without validating session state, leading to null reference exceptions.",
            SuggestedFix = "Add null check for customer session before payment processing. Implement session validation middleware and handle session expiration gracefully with appropriate error messages.",
            SeverityJustification = "Critical severity due to high occurrence count (156) and impact on payment processing functionality.",
            ConfidenceScore = 87
        });
        await _incidentRepository.InsertAsync(incident1);

        // Incident 2: High - SQL timeout
        var incident2Hash = ComputeHashSignature("SQL timeout in ProductCatalog.GetInventory", "ProductCatalog", "SqlException");
        var incident2 = new Incident(
            _guidGenerator.Create(),
            appId2,
            "SQL timeout in ProductCatalog.GetInventory",
            incident2Hash,
            IncidentSeverity.High,
            baseTime.AddDays(-4),
            tenantId: null)
        {
            Description = "Database query times out under heavy load when retrieving inventory data.",
            OccurrenceCount = 43
        };
        incident2.EnrichWithAiAnalysis(new AiAnalysisResult
        {
            SentimentScore = 0.25,
            KeyPhrases = new List<string> { "SQL timeout", "ProductCatalog", "GetInventory", "database query", "heavy load" },
            Entities = new List<string> { "ProductCatalog:Service", "Database:Technology", "Inventory:Data" },
            RootCauseSummary = "Database query times out under heavy load when retrieving inventory data. The GetInventory method performs complex joins without proper indexing, causing performance degradation during peak traffic.",
            SuggestedFix = "Add index on Products.CategoryId and consider query optimization with pagination or caching. Review execution plan and optimize JOIN operations.",
            SeverityJustification = "High severity due to impact on product catalog functionality and user experience during peak traffic.",
            ConfidenceScore = 92
        });
        await _incidentRepository.InsertAsync(incident2);

        // Incident 3: Medium - Redis ConnectionException
        var incident3Hash = ComputeHashSignature("Redis ConnectionException in CacheService", "CacheService", "ConnectionException");
        var incident3 = new Incident(
            _guidGenerator.Create(),
            appId3,
            "Redis ConnectionException in CacheService",
            incident3Hash,
            IncidentSeverity.Medium,
            baseTime.AddDays(-3),
            tenantId: null)
        {
            Description = "Redis connection pool exhausted during peak traffic periods.",
            OccurrenceCount = 12
        };
        incident3.EnrichWithAiAnalysis(new AiAnalysisResult
        {
            SentimentScore = 0.30,
            KeyPhrases = new List<string> { "Redis", "ConnectionException", "CacheService", "connection pool", "peak traffic" },
            Entities = new List<string> { "Redis:Technology", "CacheService:Service", "ConnectionPool:Resource" },
            RootCauseSummary = "Redis connection pool exhausted during peak traffic periods. The CacheService maintains insufficient connection pool size for concurrent requests, leading to connection failures.",
            SuggestedFix = "Increase connection pool size and add circuit breaker pattern to gracefully handle connection failures. Implement connection retry logic with exponential backoff.",
            SeverityJustification = "Medium severity due to impact on caching functionality but with fallback mechanisms available.",
            ConfidenceScore = 78
        });
        await _incidentRepository.InsertAsync(incident3);

        // Incident 4: Critical - OutOfMemoryException
        var incident4Hash = ComputeHashSignature("OutOfMemoryException in ReportGenerator", "ReportGenerator", "OutOfMemoryException");
        var incident4 = new Incident(
            _guidGenerator.Create(),
            appId2,
            "OutOfMemoryException in ReportGenerator",
            incident4Hash,
            IncidentSeverity.Critical,
            baseTime.AddDays(-2),
            tenantId: null)
        {
            Description = "Report generation loads entire dataset into memory causing out of memory exceptions.",
            OccurrenceCount = 8
        };
        incident4.EnrichWithAiAnalysis(new AiAnalysisResult
        {
            SentimentScore = 0.10,
            KeyPhrases = new List<string> { "OutOfMemoryException", "ReportGenerator", "dataset", "memory", "large reports" },
            Entities = new List<string> { "ReportGenerator:Service", "Memory:Resource", "Dataset:Data" },
            RootCauseSummary = "Report generation loads entire dataset into memory causing out of memory exceptions. The ReportGenerator processes large datasets without pagination or streaming, exhausting available memory.",
            SuggestedFix = "Implement streaming/pagination for large reports. Use database cursors or chunked processing to avoid loading entire datasets into memory.",
            SeverityJustification = "Critical severity due to application crashes and inability to generate reports for large datasets.",
            ConfidenceScore = 95
        });
        await _incidentRepository.InsertAsync(incident4);

        // Incident 5: Low - Authentication token expired
        var incident5Hash = ComputeHashSignature("Authentication token expired", "AuthService", "TokenExpiredException");
        var incident5 = new Incident(
            _guidGenerator.Create(),
            appId1,
            "Authentication token expired",
            incident5Hash,
            IncidentSeverity.Low,
            baseTime.AddDays(-1),
            tenantId: null)
        {
            Description = "JWT token expiration not properly handled in authentication middleware.",
            OccurrenceCount = 234
        };
        incident5.EnrichWithAiAnalysis(new AiAnalysisResult
        {
            SentimentScore = 0.40,
            KeyPhrases = new List<string> { "authentication", "token expired", "JWT", "middleware", "session" },
            Entities = new List<string> { "AuthService:Service", "JWT:Technology", "Token:Security" },
            RootCauseSummary = "JWT token expiration not properly handled in authentication middleware. Users experience unexpected logouts when tokens expire without refresh mechanism.",
            SuggestedFix = "Implement token refresh logic in middleware. Add automatic token renewal before expiration and handle expired tokens gracefully with appropriate error responses.",
            SeverityJustification = "Low severity due to high occurrence but low impact on core functionality, primarily affecting user experience.",
            ConfidenceScore = 70
        });
        await _incidentRepository.InsertAsync(incident5);
    }

    private static string ComputeHashSignature(string message, string source, string exceptionType)
    {
        // Deterministic hash signature for demo data (matches IncidentProcessorFunction logic)
        var input = $"{message?.Substring(0, Math.Min(message.Length, 200))}|{source}|{exceptionType}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexStringLower(bytes);
    }
}
