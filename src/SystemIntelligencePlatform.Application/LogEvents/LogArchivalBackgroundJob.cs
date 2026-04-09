using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;

namespace SystemIntelligencePlatform.LogEvents;

/// <summary>
/// Moves LogEvents older than 30 days to Blob Storage and removes from SQL.
/// Runs as a background job. Processes in batches to avoid memory pressure.
/// Incident data remains intact - only raw log events are archived.
/// </summary>
public class LogArchivalBackgroundJob(
    ILogEventRepository logEventRepository,
    IBlobStorageService blobStorageService,
    ILogger<LogArchivalBackgroundJob> logger) : AsyncBackgroundJob<LogArchivalArgs>, ITransientDependency
{
    private const int BatchSize = 1000;
    private const int RetentionDays = 30;
    private const string ContainerName = "archived-logs";

    public override async Task ExecuteAsync(LogArchivalArgs args)
    {
        var cutoff = DateTime.UtcNow.AddDays(-RetentionDays);
        var totalArchived = 0L;

        logger.LogInformation("Starting log archival. Cutoff: {Cutoff}", cutoff);

        while (true)
        {
            var batch = await logEventRepository.GetOlderThanAsync(cutoff, BatchSize);
            if (batch.Count == 0)
                break;

            var blobName = $"{cutoff:yyyy/MM/dd}/batch_{DateTime.UtcNow:HHmmss}_{Guid.NewGuid():N}.json";
            var json = JsonSerializer.Serialize(batch.Select(e => new
            {
                e.Id,
                e.ApplicationId,
                Level = e.Level.ToString(),
                e.Message,
                e.Source,
                e.ExceptionType,
                e.HashSignature,
                e.CorrelationId,
                e.Timestamp,
                e.IncidentId
            }));

            await blobStorageService.UploadAsync(ContainerName, blobName, json);
            await logEventRepository.DeleteBatchAsync(batch.Select(e => e.Id));

            totalArchived += batch.Count;
            logger.LogInformation("Archived batch of {Count} log events to {BlobName}", batch.Count, blobName);
        }

        logger.LogInformation("Log archival complete. Total archived: {Total}", totalArchived);
    }
}

public class LogArchivalArgs
{
}
