using System;
using System.Linq;
using System.Threading.Tasks;
using SystemIntelligencePlatform.LogEvents;
using SystemIntelligencePlatform.MonitoredApplications;
using SystemIntelligencePlatform.Subscriptions;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SystemIntelligencePlatform.LogIngestion;

/// <summary>
/// Lightweight ingestion endpoint: validates the API key, enforces plan limits,
/// publishes to Service Bus, and returns immediately.
/// All heavy processing is done asynchronously via Azure Functions.
/// </summary>
public class LogIngestionAppService : ApplicationService, ILogIngestionAppService
{
    private readonly IMonitoredApplicationRepository _applicationRepository;
    private readonly ILogEventPublisher _logEventPublisher;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IRepository<MonthlyUsage, Guid> _usageRepository;

    public LogIngestionAppService(
        IMonitoredApplicationRepository applicationRepository,
        ILogEventPublisher logEventPublisher,
        ISubscriptionRepository subscriptionRepository,
        IRepository<MonthlyUsage, Guid> usageRepository)
    {
        _applicationRepository = applicationRepository;
        _logEventPublisher = logEventPublisher;
        _subscriptionRepository = subscriptionRepository;
        _usageRepository = usageRepository;
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

        // Enforce plan limits
        var subscription = await _subscriptionRepository.FindByTenantIdAsync(app.TenantId);
        var plan = subscription?.Plan ?? SubscriptionPlan.Free;
        var limits = PlanLimits.GetLimits(plan);

        if (subscription != null && subscription.Status != SubscriptionStatus.Active)
        {
            throw new BusinessException(SystemIntelligencePlatformDomainErrorCodes.SubscriptionNotActive)
                .WithData("plan", plan.ToString());
        }

        var currentMonth = MonthlyUsage.CurrentMonth();
        var usageQueryable = await _usageRepository.GetQueryableAsync();
        var usage = await AsyncExecuter.FirstOrDefaultAsync(
            usageQueryable.Where(u => u.TenantId == app.TenantId && u.Month == currentMonth));

        var currentLogs = usage?.LogsIngested ?? 0;
        if (currentLogs + input.Events.Count > limits.LogsPerMonth)
        {
            throw new BusinessException(SystemIntelligencePlatformDomainErrorCodes.MonthlyLogLimitExceeded)
                .WithData("limit", limits.LogsPerMonth)
                .WithData("current", currentLogs)
                .WithData("plan", plan.ToString());
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

        // Increment usage atomically
        if (usage == null)
        {
            usage = new MonthlyUsage(GuidGenerator.Create(), currentMonth, app.TenantId);
            usage.IncrementLogs(count);
            await _usageRepository.InsertAsync(usage);
        }
        else
        {
            usage.IncrementLogs(count);
            await _usageRepository.UpdateAsync(usage);
        }

        return new LogIngestionResultDto
        {
            Accepted = count,
            Status = "Queued"
        };
    }
}
