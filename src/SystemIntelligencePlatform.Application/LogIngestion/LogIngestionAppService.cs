using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using SystemIntelligencePlatform.LogEvents;
using SystemIntelligencePlatform.MonitoredApplications;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace SystemIntelligencePlatform.LogIngestion;

/// <summary>
/// Lightweight ingestion endpoint: validates the API key, publishes to Service Bus, returns immediately.
/// All heavy processing is done asynchronously via Azure Functions.
/// </summary>
public class LogIngestionAppService : ApplicationService, ILogIngestionAppService
{
    private readonly IMonitoredApplicationRepository _applicationRepository;
    private readonly ILogEventPublisher _logEventPublisher;

    public LogIngestionAppService(
        IMonitoredApplicationRepository applicationRepository,
        ILogEventPublisher logEventPublisher)
    {
        _applicationRepository = applicationRepository;
        _logEventPublisher = logEventPublisher;
    }

    public async Task<LogIngestionResultDto> IngestAsync(string apiKey, LogIngestionDto input)
    {
        var apiKeyHash = ApiKeyGenerator.Hash(apiKey);
        var app = await _applicationRepository.FindByApiKeyHashAsync(apiKeyHash);

        if (app == null || !app.IsActive)
        {
            throw new BusinessException(SystemIntelligencePlatformDomainErrorCodes.InvalidApiKey);
        }

        if (!ApiKeyGenerator.ValidateHash(apiKey, app.ApiKeyHash))
        {
            throw new BusinessException(SystemIntelligencePlatformDomainErrorCodes.InvalidApiKey);
        }

        var count = 0;
        foreach (var evt in input.Events)
        {
            var message = new LogEventMessage
            {
                TenantId = app.TenantId,
                ApplicationId = app.Id,
                Level = evt.Level,
                Message = evt.Message,
                Source = evt.Source,
                ExceptionType = evt.ExceptionType,
                StackTrace = evt.StackTrace,
                CorrelationId = evt.CorrelationId,
                Timestamp = evt.Timestamp ?? DateTime.UtcNow
            };

            await _logEventPublisher.PublishAsync(message);
            count++;
        }

        return new LogIngestionResultDto
        {
            Accepted = count,
            Status = "Queued"
        };
    }
}
