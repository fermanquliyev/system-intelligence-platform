using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SystemIntelligencePlatform.Alerting;

public interface IAlertRuleAppService : IApplicationService
{
    Task<AlertRuleDto> GetAsync(Guid id);

    Task<PagedResultDto<AlertRuleDto>> GetListAsync(PagedAndSortedResultRequestDto input);

    Task<AlertRuleDto> CreateAsync(CreateUpdateAlertRuleDto input);

    Task<AlertRuleDto> UpdateAsync(Guid id, CreateUpdateAlertRuleDto input);

    Task DeleteAsync(Guid id);

    Task<AlertRuleDto> SetEnabledAsync(Guid id, bool isEnabled);

    Task<PagedResultDto<AlertHistoryDto>> GetHistoryAsync(PagedAndSortedResultRequestDto input);

    Task<int> EvaluateNowAsync(Guid? applicationId);
}
