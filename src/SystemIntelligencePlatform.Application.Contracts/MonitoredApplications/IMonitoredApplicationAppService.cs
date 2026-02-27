using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SystemIntelligencePlatform.MonitoredApplications;

public interface IMonitoredApplicationAppService : IApplicationService
{
    Task<MonitoredApplicationDto> GetAsync(Guid id);
    Task<PagedResultDto<MonitoredApplicationDto>> GetListAsync(PagedAndSortedResultRequestDto input);
    Task<ApiKeyResultDto> CreateAsync(CreateMonitoredApplicationDto input);
    Task<MonitoredApplicationDto> UpdateAsync(Guid id, UpdateMonitoredApplicationDto input);
    Task DeleteAsync(Guid id);
    Task<ApiKeyResultDto> RegenerateApiKeyAsync(Guid id);
}
