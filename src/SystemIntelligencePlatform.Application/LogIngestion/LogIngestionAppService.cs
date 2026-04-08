using System;
using System.Threading.Tasks;
using SystemIntelligencePlatform;
using SystemIntelligencePlatform.InstanceConfiguration;
using SystemIntelligencePlatform.LogEvents;
using SystemIntelligencePlatform.MonitoredApplications;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace SystemIntelligencePlatform.LogIngestion;

public class LogIngestionAppService : ApplicationService, ILogIngestionAppService
{
    private readonly IMonitoredApplicationRepository _applicationRepository;
    private readonly ILogEventPublisher _logEventPublisher;
    private readonly IInstanceConfigurationProvider _instanceConfiguration;

    public LogIngestionAppService(
        IMonitoredApplicationRepository applicationRepository,
        ILogEventPublisher logEventPublisher,
        IInstanceConfigurationProvider instanceConfiguration)
    {
        _applicationRepository = applicationRepository;
        _logEventPublisher = logEventPublisher;
        _instanceConfiguration = instanceConfiguration;
    }

    public async Task<LogIngestionResultDto> IngestAsync(string apiKey, LogIngestionDto input)
    {
        if (!_instanceConfiguration.IsFeatureEnabled(InstanceConfigurationFeatures.RabbitMqMessaging))
        {
            throw new BusinessException(SystemIntelligencePlatformDomainErrorCodes.RabbitMqPipelineDisabled);
        }

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
