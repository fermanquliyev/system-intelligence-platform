using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SystemIntelligencePlatform.LogSearch;

public interface ILogSearchAppService : IApplicationService
{
    Task<PagedResultDto<LogEventSearchItemDto>> SearchAsync(LogSearchInput input);

    Task<SavedLogSearchDto> CreateSavedAsync(CreateSavedLogSearchDto input);

    Task DeleteSavedAsync(Guid id);

    Task<ListResultDto<SavedLogSearchDto>> GetSavedListAsync();
}
