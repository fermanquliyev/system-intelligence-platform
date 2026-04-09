using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SystemIntelligencePlatform.Playbooks;

public interface IPlaybookAppService : IApplicationService
{
    Task<PlaybookDto> GetAsync(Guid id);

    Task<PagedResultDto<PlaybookDto>> GetListAsync(PagedAndSortedResultRequestDto input);

    Task<PlaybookDto> CreateAsync(CreatePlaybookDto input);

    Task DeleteAsync(Guid id);

    Task<PlaybookRunDto> RunForIncidentAsync(Guid playbookId, Guid incidentId);

    Task<PlaybookRunDto> CompleteRunStepAsync(Guid runId, int stepOrder);

    Task<PlaybookRunDto?> GetActiveRunAsync(Guid incidentId);
}
