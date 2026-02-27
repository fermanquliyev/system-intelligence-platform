using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SystemIntelligencePlatform.Incidents;

public interface IIncidentAppService : IApplicationService
{
    Task<IncidentDto> GetAsync(Guid id);
    Task<PagedResultDto<IncidentDto>> GetListAsync(GetIncidentListInput input);
    Task<IncidentDto> ResolveAsync(Guid id);
    Task<IncidentDto> UpdateStatusAsync(Guid id, IncidentStatus status);
    Task<IncidentCommentDto> AddCommentAsync(Guid incidentId, CreateIncidentCommentDto input);
    Task<PagedResultDto<IncidentCommentDto>> GetCommentsAsync(Guid incidentId, PagedResultRequestDto input);
}
