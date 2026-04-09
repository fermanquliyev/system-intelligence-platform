using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SystemIntelligencePlatform.LogEvents;
using SystemIntelligencePlatform.MonitoredApplications;
using SystemIntelligencePlatform.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SystemIntelligencePlatform.Alerting;

[Authorize(SystemIntelligencePlatformPermissions.AlertRules.Default)]
public class AlertRuleAppService : ApplicationService, IAlertRuleAppService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly IRepository<AlertRule, Guid> _ruleRepository;
    private readonly IRepository<AlertHistory, Guid> _historyRepository;
    private readonly IRepository<LogEvent, Guid> _logRepository;
    private readonly IRepository<MonitoredApplication, Guid> _appRepository;

    public AlertRuleAppService(
        IRepository<AlertRule, Guid> ruleRepository,
        IRepository<AlertHistory, Guid> historyRepository,
        IRepository<LogEvent, Guid> logRepository,
        IRepository<MonitoredApplication, Guid> appRepository)
    {
        _ruleRepository = ruleRepository;
        _historyRepository = historyRepository;
        _logRepository = logRepository;
        _appRepository = appRepository;
    }

    public async Task<AlertRuleDto> GetAsync(Guid id)
    {
        var e = await _ruleRepository.GetAsync(id);
        return ObjectMapper.Map<AlertRule, AlertRuleDto>(e);
    }

    public async Task<PagedResultDto<AlertRuleDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        var q = await _ruleRepository.GetQueryableAsync();
        var total = await AsyncExecuter.CountAsync(q);
        var list = await AsyncExecuter.ToListAsync(q.OrderByDescending(r => r.CreationTime).Skip(input.SkipCount).Take(input.MaxResultCount));
        return new PagedResultDto<AlertRuleDto>(total, ObjectMapper.Map<List<AlertRule>, List<AlertRuleDto>>(list));
    }

    [Authorize(SystemIntelligencePlatformPermissions.AlertRules.Manage)]
    public async Task<AlertRuleDto> CreateAsync(CreateUpdateAlertRuleDto input)
    {
        var e = new AlertRule(GuidGenerator.Create(), input.Name, input.DefinitionJson, input.ApplicationId)
        {
            IsEnabled = input.IsEnabled,
            SeverityOverride = input.SeverityOverride
        };
        await _ruleRepository.InsertAsync(e);
        return ObjectMapper.Map<AlertRule, AlertRuleDto>(e);
    }

    [Authorize(SystemIntelligencePlatformPermissions.AlertRules.Manage)]
    public async Task<AlertRuleDto> UpdateAsync(Guid id, CreateUpdateAlertRuleDto input)
    {
        var e = await _ruleRepository.GetAsync(id);
        e.Name = input.Name;
        e.DefinitionJson = input.DefinitionJson;
        e.IsEnabled = input.IsEnabled;
        e.SeverityOverride = input.SeverityOverride;
        e.ApplicationId = input.ApplicationId;
        await _ruleRepository.UpdateAsync(e);
        return ObjectMapper.Map<AlertRule, AlertRuleDto>(e);
    }

    [Authorize(SystemIntelligencePlatformPermissions.AlertRules.Manage)]
    public async Task DeleteAsync(Guid id)
    {
        await _ruleRepository.DeleteAsync(id);
    }

    [Authorize(SystemIntelligencePlatformPermissions.AlertRules.Manage)]
    public async Task<AlertRuleDto> SetEnabledAsync(Guid id, bool isEnabled)
    {
        var e = await _ruleRepository.GetAsync(id);
        e.IsEnabled = isEnabled;
        await _ruleRepository.UpdateAsync(e);
        return ObjectMapper.Map<AlertRule, AlertRuleDto>(e);
    }

    public async Task<PagedResultDto<AlertHistoryDto>> GetHistoryAsync(PagedAndSortedResultRequestDto input)
    {
        var q = await _historyRepository.GetQueryableAsync();
        var total = await AsyncExecuter.CountAsync(q);
        var list = await AsyncExecuter.ToListAsync(q.OrderByDescending(h => h.FiredAt).Skip(input.SkipCount).Take(input.MaxResultCount));
        return new PagedResultDto<AlertHistoryDto>(total, ObjectMapper.Map<List<AlertHistory>, List<AlertHistoryDto>>(list));
    }

    [Authorize(SystemIntelligencePlatformPermissions.AlertRules.Manage)]
    public async Task<int> EvaluateNowAsync(Guid? applicationId)
    {
        var rules = await AsyncExecuter.ToListAsync((await _ruleRepository.GetQueryableAsync()).Where(r => r.IsEnabled));
        var fired = 0;
        var appIds = applicationId.HasValue
            ? new List<Guid> { applicationId.Value }
            : await AsyncExecuter.ToListAsync((await _appRepository.GetQueryableAsync()).Select(a => a.Id));

        foreach (var rule in rules)
        {
            AlertRuleDefinitionModel? def;
            try
            {
                def = JsonSerializer.Deserialize<AlertRuleDefinitionModel>(rule.DefinitionJson, JsonOptions);
            }
            catch
            {
                continue;
            }

            if (def == null)
                continue;

            var targets = rule.ApplicationId.HasValue
                ? new List<Guid> { rule.ApplicationId.Value }.Where(appIds.Contains).ToList()
                : appIds;

            foreach (var appId in targets)
            {
                if (!await RuleMatchesAsync(def, appId))
                    continue;

                var payload = JsonSerializer.Serialize(new { ruleId = rule.Id, applicationId = appId, reason = "threshold" });
                await _historyRepository.InsertAsync(new AlertHistory(GuidGenerator.Create(), rule.Id, DateTime.UtcNow, payload, appId));
                fired++;
            }
        }

        return fired;
    }

    private async Task<bool> RuleMatchesAsync(AlertRuleDefinitionModel def, Guid applicationId)
    {
        var from = DateTime.UtcNow.AddHours(-1);
        var q = await _logRepository.GetQueryableAsync();

        if (def.MinErrorsLastHour.HasValue)
        {
            var n = await AsyncExecuter.CountAsync(q.Where(e =>
                e.ApplicationId == applicationId && e.Timestamp >= from && e.Level >= LogLevel.Error));
            if (n >= def.MinErrorsLastHour.Value)
                return true;
        }

        if (def.MinWarningsLastHour.HasValue)
        {
            var n = await AsyncExecuter.CountAsync(q.Where(e =>
                e.ApplicationId == applicationId && e.Timestamp >= from && e.Level == LogLevel.Warning));
            if (n >= def.MinWarningsLastHour.Value)
                return true;
        }

        return false;
    }
}
