using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SystemIntelligencePlatform.LogEvents;
using SystemIntelligencePlatform.LogSearch;
using SystemIntelligencePlatform.MonitoredApplications;
using SystemIntelligencePlatform.Permissions;
using SystemIntelligencePlatform.Security;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace SystemIntelligencePlatform.LogSearch;

[Authorize(SystemIntelligencePlatformPermissions.LogEvents.Default)]
public class LogSearchAppService : ApplicationService, ILogSearchAppService
{
    private readonly ILogSearchQuery _logSearchQuery;
    private readonly IRepository<LogEvent, Guid> _logEventRepository;
    private readonly IRepository<MonitoredApplication, Guid> _applicationRepository;
    private readonly IRepository<SavedLogSearch, Guid> _savedRepository;
    private readonly IPiiDetector _piiDetector;

    public LogSearchAppService(
        ILogSearchQuery logSearchQuery,
        IRepository<LogEvent, Guid> logEventRepository,
        IRepository<MonitoredApplication, Guid> applicationRepository,
        IRepository<SavedLogSearch, Guid> savedRepository,
        IPiiDetector piiDetector)
    {
        _logSearchQuery = logSearchQuery;
        _logEventRepository = logEventRepository;
        _applicationRepository = applicationRepository;
        _savedRepository = savedRepository;
        _piiDetector = piiDetector;
    }

    [Authorize(SystemIntelligencePlatformPermissions.LogEvents.Search)]
    public async Task<PagedResultDto<LogEventSearchItemDto>> SearchAsync(LogSearchInput input)
    {
        var canUnmask = await AuthorizationService.IsGrantedAsync(SystemIntelligencePlatformPermissions.LogEvents.ViewUnmasked);

        var (ids, total) = await _logSearchQuery.SearchAsync(
            input.UseFullText ? input.Query : null,
            input.UseFullText ? null : input.Query,
            input.ApplicationId,
            input.MinLevel,
            input.FromUtc,
            input.ToUtc,
            input.SkipCount,
            input.MaxResultCount);

        if (ids.Count == 0)
            return new PagedResultDto<LogEventSearchItemDto>(total, new List<LogEventSearchItemDto>());

        var logQ = await _logEventRepository.GetQueryableAsync();
        var logs = await AsyncExecuter.ToListAsync(logQ.Where(e => ids.Contains(e.Id)));
        var order = ids.Select((id, i) => (id, i)).ToDictionary(x => x.id, x => x.i);
        logs.Sort((a, b) => order.GetValueOrDefault(a.Id, 0).CompareTo(order.GetValueOrDefault(b.Id, 0)));

        var appIds = logs.Select(l => l.ApplicationId).Distinct().ToList();
        var apps = await AsyncExecuter.ToListAsync(
            (await _applicationRepository.GetQueryableAsync()).Where(a => appIds.Contains(a.Id)));
        var appLookup = apps.ToDictionary(a => a.Id, a => a.Name);

        var items = logs.Select(e =>
        {
            var msg = e.Message;
            var mask = !canUnmask || !input.RevealSensitive;
            if (mask)
                msg = _piiDetector.Mask(msg);

            return new LogEventSearchItemDto
            {
                Id = e.Id,
                ApplicationId = e.ApplicationId,
                ApplicationName = appLookup.GetValueOrDefault(e.ApplicationId, "Unknown"),
                Level = e.Level,
                Message = msg,
                Timestamp = e.Timestamp,
                ContainsPii = e.ContainsPii || _piiDetector.ContainsPii(e.Message)
            };
        }).ToList();

        return new PagedResultDto<LogEventSearchItemDto>(total, items);
    }

    [Authorize(SystemIntelligencePlatformPermissions.LogEvents.Search)]
    public async Task<SavedLogSearchDto> CreateSavedAsync(CreateSavedLogSearchDto input)
    {
        var userId = CurrentUser.GetId();
        var entity = new SavedLogSearch(GuidGenerator.Create(), userId, input.Name, input.FilterJson);
        await _savedRepository.InsertAsync(entity);
        return ObjectMapper.Map<SavedLogSearch, SavedLogSearchDto>(entity);
    }

    [Authorize(SystemIntelligencePlatformPermissions.LogEvents.Search)]
    public async Task DeleteSavedAsync(Guid id)
    {
        var userId = CurrentUser.GetId();
        var entity = await _savedRepository.GetAsync(id);
        if (entity.UserId != userId)
            throw new AbpAuthorizationException();

        await _savedRepository.DeleteAsync(entity);
    }

    [Authorize(SystemIntelligencePlatformPermissions.LogEvents.Search)]
    public async Task<ListResultDto<SavedLogSearchDto>> GetSavedListAsync()
    {
        var userId = CurrentUser.GetId();
        var q = await _savedRepository.GetQueryableAsync();
        var list = await AsyncExecuter.ToListAsync(q.Where(s => s.UserId == userId).OrderByDescending(s => s.CreationTime));
        return new ListResultDto<SavedLogSearchDto>(ObjectMapper.Map<List<SavedLogSearch>, List<SavedLogSearchDto>>(list));
    }
}
