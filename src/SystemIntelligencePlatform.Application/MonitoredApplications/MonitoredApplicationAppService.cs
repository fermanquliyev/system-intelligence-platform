using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SystemIntelligencePlatform.Permissions;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SystemIntelligencePlatform.MonitoredApplications;

[Authorize(SystemIntelligencePlatformPermissions.Applications.Default)]
public class MonitoredApplicationAppService : ApplicationService, IMonitoredApplicationAppService
{
    private readonly IMonitoredApplicationRepository _repository;

    public MonitoredApplicationAppService(IMonitoredApplicationRepository repository)
    {
        _repository = repository;
    }

    public async Task<MonitoredApplicationDto> GetAsync(Guid id)
    {
        var app = await _repository.GetAsync(id);
        return ObjectMapper.Map<MonitoredApplication, MonitoredApplicationDto>(app);
    }

    public async Task<PagedResultDto<MonitoredApplicationDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        var queryable = await _repository.GetQueryableAsync();
        var query = queryable
            .OrderBy(input.Sorting.IsNullOrWhiteSpace() ? "Name" : input.Sorting)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount);

        var apps = await AsyncExecuter.ToListAsync(query);
        var totalCount = await AsyncExecuter.CountAsync(queryable);

        return new PagedResultDto<MonitoredApplicationDto>(
            totalCount,
            ObjectMapper.Map<List<MonitoredApplication>, List<MonitoredApplicationDto>>(apps)
        );
    }

    [Authorize(SystemIntelligencePlatformPermissions.Applications.Create)]
    public async Task<ApiKeyResultDto> CreateAsync(CreateMonitoredApplicationDto input)
    {
        var existing = await _repository.FindByNameAsync(input.Name, CurrentTenant.Id);
        if (existing != null)
        {
            throw new BusinessException(SystemIntelligencePlatformDomainErrorCodes.DuplicateApplicationName);
        }

        var apiKey = ApiKeyGenerator.Generate();
        var apiKeyHash = ApiKeyGenerator.Hash(apiKey);

        var app = new MonitoredApplication(
            GuidGenerator.Create(),
            input.Name,
            apiKeyHash,
            CurrentTenant.Id,
            input.Description,
            input.Environment
        );

        await _repository.InsertAsync(app);

        return new ApiKeyResultDto
        {
            ApplicationId = app.Id,
            ApiKey = apiKey
        };
    }

    [Authorize(SystemIntelligencePlatformPermissions.Applications.Edit)]
    public async Task<MonitoredApplicationDto> UpdateAsync(Guid id, UpdateMonitoredApplicationDto input)
    {
        var app = await _repository.GetAsync(id);

        app.Name = input.Name;
        app.Description = input.Description;
        app.Environment = input.Environment;
        app.IsActive = input.IsActive;

        await _repository.UpdateAsync(app);

        return ObjectMapper.Map<MonitoredApplication, MonitoredApplicationDto>(app);
    }

    [Authorize(SystemIntelligencePlatformPermissions.Applications.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
    }

    [Authorize(SystemIntelligencePlatformPermissions.Applications.RegenerateApiKey)]
    public async Task<ApiKeyResultDto> RegenerateApiKeyAsync(Guid id)
    {
        var app = await _repository.GetAsync(id);
        var apiKey = ApiKeyGenerator.Generate();
        app.RegenerateApiKey(ApiKeyGenerator.Hash(apiKey));
        await _repository.UpdateAsync(app);

        return new ApiKeyResultDto
        {
            ApplicationId = app.Id,
            ApiKey = apiKey
        };
    }
}
