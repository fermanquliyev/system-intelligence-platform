using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SystemIntelligencePlatform.LogSources;

public interface ILogSourceConfigurationAppService : IApplicationService
{
    Task<LogSourceConfigurationDto> GetAsync(Guid id);

    Task<PagedResultDto<LogSourceConfigurationDto>> GetListAsync(PagedAndSortedResultRequestDto input);

    Task<LogSourceConfigurationDto> CreateAsync(CreateUpdateLogSourceConfigurationDto input);

    Task<LogSourceConfigurationDto> UpdateAsync(Guid id, CreateUpdateLogSourceConfigurationDto input);

    Task DeleteAsync(Guid id);

    Task<LogSourceConfigurationDto> SetEnabledAsync(Guid id, bool isEnabled);
}
