using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.AI.TextAnalytics;
using Azure.Messaging.ServiceBus;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SystemIntelligencePlatform.EntityFrameworkCore;
using SystemIntelligencePlatform.Incidents;
using SystemIntelligencePlatform.LogEvents;
using Volo.Abp.EntityFrameworkCore;

namespace SystemIntelligencePlatform.AzureFunctions.Functions;

/// <summary>
/// Core event-driven processor: triggered by Service Bus, stores log events,
/// detects anomalies, creates/updates incidents, enriches with AI, indexes for search,
/// and pushes real-time notifications.
/// </summary>
public class IncidentProcessorFunction
{
    private readonly IDbContextProvider<SystemIntelligencePlatformDbContext> _dbContextProvider;
    private readonly TextAnalyticsClient? _textAnalyticsClient;
    private readonly SearchClient? _searchClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<IncidentProcessorFunction> _logger;

    private const int AnomalyThreshold = IncidentConsts.AnomalyThreshold;

    public IncidentProcessorFunction(
        IDbContextProvider<SystemIntelligencePlatformDbContext> dbContextProvider,
        IConfiguration configuration,
        ILogger<IncidentProcessorFunction> logger,
        TextAnalyticsClient? textAnalyticsClient = null,
        SearchClient? searchClient = null)
    {
        _dbContextProvider = dbContextProvider;
        _configuration = configuration;
        _logger = logger;
        _textAnalyticsClient = textAnalyticsClient;
        _searchClient = searchClient;
    }

    [Function("IncidentProcessor")]
    public async Task RunAsync(
        [ServiceBusTrigger("log-ingestion", Connection = "ServiceBusConnection")]
        ServiceBusReceivedMessage message)
    {
        LogEventMessage? logEventMessage;
        try
        {
            logEventMessage = JsonSerializer.Deserialize<LogEventMessage>(message.Body.ToString());
            if (logEventMessage == null)
            {
                _logger.LogWarning("Received null message body, skipping");
                return;
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize Service Bus message");
            return;
        }

        var dbContext = await _dbContextProvider.GetDbContextAsync();

        // 1. Compute hash signature for grouping
        var hashSignature = ComputeHashSignature(logEventMessage);

        // 2. Store LogEvent
        var logEvent = new LogEvent(
            Guid.NewGuid(),
            logEventMessage.ApplicationId,
            logEventMessage.Level,
            logEventMessage.Message,
            hashSignature,
            logEventMessage.Timestamp,
            logEventMessage.TenantId)
        {
            Source = logEventMessage.Source,
            ExceptionType = logEventMessage.ExceptionType,
            StackTrace = logEventMessage.StackTrace,
            CorrelationId = logEventMessage.CorrelationId
        };

        dbContext.LogEvents.Add(logEvent);
        await dbContext.SaveChangesAsync();

        _logger.LogInformation("Stored LogEvent {LogEventId} for Application {ApplicationId}",
            logEvent.Id, logEvent.ApplicationId);

        // 3. Check anomaly threshold (count within 1-hour window)
        var windowStart = DateTime.UtcNow.AddHours(-1);
        var recentCount = await dbContext.LogEvents
            .AsNoTracking()
            .CountAsync(e => e.HashSignature == hashSignature
                          && e.ApplicationId == logEventMessage.ApplicationId
                          && e.Timestamp >= windowStart);

        if (recentCount < AnomalyThreshold)
            return;

        // 4. Create or update Incident
        var existingIncident = await dbContext.Incidents
            .FirstOrDefaultAsync(i => i.HashSignature == hashSignature
                                   && i.ApplicationId == logEventMessage.ApplicationId
                                   && i.Status != IncidentStatus.Resolved
                                   && i.Status != IncidentStatus.Closed);

        Incident incident;
        bool isNew;

        if (existingIncident != null)
        {
            existingIncident.IncrementOccurrence(logEventMessage.Timestamp);
            incident = existingIncident;
            isNew = false;
            _logger.LogInformation("Updated Incident {IncidentId}, count: {Count}",
                incident.Id, incident.OccurrenceCount);
        }
        else
        {
            incident = new Incident(
                Guid.NewGuid(),
                logEventMessage.ApplicationId,
                TruncateTitle(logEventMessage.Message),
                hashSignature,
                DetermineSeverity(logEventMessage.Level, recentCount),
                logEventMessage.Timestamp,
                logEventMessage.TenantId)
            {
                Description = logEventMessage.StackTrace ?? logEventMessage.Message,
                OccurrenceCount = recentCount
            };

            dbContext.Incidents.Add(incident);
            isNew = true;
            _logger.LogInformation("Created Incident {IncidentId} for hash {Hash}",
                incident.Id, hashSignature);
        }

        // Link LogEvent to Incident
        logEvent.IncidentId = incident.Id;

        // 5. AI enrichment - analyze top 5 recent messages
        if (_textAnalyticsClient != null)
        {
            try
            {
                var recentMessages = await dbContext.LogEvents
                    .AsNoTracking()
                    .Where(e => e.HashSignature == hashSignature && e.ApplicationId == logEventMessage.ApplicationId)
                    .OrderByDescending(e => e.Timestamp)
                    .Take(5)
                    .Select(e => e.Message)
                    .ToListAsync();

                var sentimentResults = await _textAnalyticsClient.AnalyzeSentimentBatchAsync(recentMessages);
                var avgSentiment = sentimentResults.Value
                    .Where(r => !r.HasError)
                    .Select(r => r.DocumentSentiment.ConfidenceScores.Positive)
                    .DefaultIfEmpty(0)
                    .Average();

                var keyPhraseResults = await _textAnalyticsClient.ExtractKeyPhrasesBatchAsync(recentMessages);
                var keyPhrases = keyPhraseResults.Value
                    .Where(r => !r.HasError)
                    .SelectMany(r => r.KeyPhrases)
                    .Distinct()
                    .Take(20)
                    .ToList();

                var entityResults = await _textAnalyticsClient.RecognizeEntitiesBatchAsync(recentMessages);
                var entities = entityResults.Value
                    .Where(r => !r.HasError)
                    .SelectMany(r => r.Select(e => $"{e.Text}:{e.Category}"))
                    .Distinct()
                    .Take(20)
                    .ToList();

                incident.EnrichWithAiAnalysis(
                    avgSentiment,
                    string.Join(", ", keyPhrases),
                    string.Join(", ", entities));

                _logger.LogInformation("AI enrichment completed for Incident {IncidentId}", incident.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "AI enrichment failed for Incident {IncidentId}", incident.Id);
            }
        }

        await dbContext.SaveChangesAsync();

        // 6. Index into Azure AI Search
        if (_searchClient != null)
        {
            try
            {
                var appName = await dbContext.MonitoredApplications
                    .AsNoTracking()
                    .Where(a => a.Id == logEventMessage.ApplicationId)
                    .Select(a => a.Name)
                    .FirstOrDefaultAsync() ?? "Unknown";

                var searchDoc = new IncidentSearchDocument
                {
                    Id = incident.Id.ToString(),
                    Title = incident.Title,
                    Description = incident.Description,
                    Severity = incident.Severity.ToString(),
                    ApplicationName = appName,
                    KeyPhrases = incident.KeyPhrases,
                    Entities = incident.Entities,
                    TenantId = incident.TenantId?.ToString()
                };

                var batch = IndexDocumentsBatch.Upload(new[] { searchDoc });
                await _searchClient.IndexDocumentsAsync(batch);

                _logger.LogInformation("Indexed Incident {IncidentId} in Azure AI Search", incident.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Search indexing failed for Incident {IncidentId}", incident.Id);
            }
        }

        // 7. SignalR notification would be sent via Azure SignalR Management SDK
        // In production, use IServiceHubContext from Microsoft.Azure.SignalR.Management
        _logger.LogInformation(
            "Incident {IncidentId} processed: isNew={IsNew}, severity={Severity}, count={Count}",
            incident.Id, isNew, incident.Severity, incident.OccurrenceCount);
    }

    private static string ComputeHashSignature(LogEventMessage msg)
    {
        // Group by: message template (first 200 chars) + source + exception type
        var input = $"{msg.Message?.Substring(0, Math.Min(msg.Message.Length, 200))}|{msg.Source}|{msg.ExceptionType}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexStringLower(bytes);
    }

    private static IncidentSeverity DetermineSeverity(LogLevel level, int count)
    {
        return (level, count) switch
        {
            (LogLevel.Critical, _) => IncidentSeverity.Critical,
            (LogLevel.Error, >= 50) => IncidentSeverity.Critical,
            (LogLevel.Error, >= 20) => IncidentSeverity.High,
            (LogLevel.Error, _) => IncidentSeverity.Medium,
            (LogLevel.Warning, >= 100) => IncidentSeverity.High,
            (LogLevel.Warning, _) => IncidentSeverity.Medium,
            _ => IncidentSeverity.Low
        };
    }

    private static string TruncateTitle(string message)
    {
        if (message.Length <= IncidentConsts.MaxTitleLength)
            return message;
        return message[..IncidentConsts.MaxTitleLength];
    }
}
