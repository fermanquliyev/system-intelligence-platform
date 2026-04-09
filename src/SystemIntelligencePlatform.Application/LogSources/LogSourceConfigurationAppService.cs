using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SystemIntelligencePlatform.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SystemIntelligencePlatform.LogSources;

[Authorize(SystemIntelligencePlatformPermissions.LogSources.Default)]
public class LogSourceConfigurationAppService : ApplicationService, ILogSourceConfigurationAppService
{
    private readonly IRepository<LogSourceConfiguration, Guid> _repository;

    public LogSourceConfigurationAppService(IRepository<LogSourceConfiguration, Guid> repository)
    {
        _repository = repository;
    }

    public async Task<LogSourceConfigurationDto> GetAsync(Guid id)
    {
        var e = await _repository.GetAsync(id);
        return ObjectMapper.Map<LogSourceConfiguration, LogSourceConfigurationDto>(e);
    }

    public async Task<PagedResultDto<LogSourceConfigurationDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        var q = await _repository.GetQueryableAsync();
        var total = await AsyncExecuter.CountAsync(q);
        var list = await AsyncExecuter.ToListAsync(q.OrderBy(x => x.Name).Skip(input.SkipCount).Take(input.MaxResultCount));
        return new PagedResultDto<LogSourceConfigurationDto>(total,
            ObjectMapper.Map<List<LogSourceConfiguration>, List<LogSourceConfigurationDto>>(list));
    }

    [Authorize(SystemIntelligencePlatformPermissions.LogSources.Manage)]
    public async Task<LogSourceConfigurationDto> CreateAsync(CreateUpdateLogSourceConfigurationDto input)
    {
        var e = new LogSourceConfiguration(GuidGenerator.Create(), input.Name, input.SourceType, input.SettingsJson)
        {
            IsEnabled = input.IsEnabled
        };
        await _repository.InsertAsync(e);
        return ObjectMapper.Map<LogSourceConfiguration, LogSourceConfigurationDto>(e);
    }

    [Authorize(SystemIntelligencePlatformPermissions.LogSources.Manage)]
    public async Task<LogSourceConfigurationDto> UpdateAsync(Guid id, CreateUpdateLogSourceConfigurationDto input)
    {
        var e = await _repository.GetAsync(id);
        e.Name = input.Name;
        e.SourceType = input.SourceType;
        e.IsEnabled = input.IsEnabled;
        e.SettingsJson = input.SettingsJson;
        await _repository.UpdateAsync(e);
        return ObjectMapper.Map<LogSourceConfiguration, LogSourceConfigurationDto>(e);
    }

    [Authorize(SystemIntelligencePlatformPermissions.LogSources.Manage)]
    public async Task DeleteAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
    }

    [Authorize(SystemIntelligencePlatformPermissions.LogSources.Manage)]
    public async Task<LogSourceConfigurationDto> SetEnabledAsync(Guid id, bool isEnabled)
    {
        var e = await _repository.GetAsync(id);
        e.IsEnabled = isEnabled;
        await _repository.UpdateAsync(e);
        return ObjectMapper.Map<LogSourceConfiguration, LogSourceConfigurationDto>(e);
    }
}
