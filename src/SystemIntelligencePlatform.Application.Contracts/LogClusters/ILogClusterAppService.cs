using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SystemIntelligencePlatform.LogClusters;

public interface ILogClusterAppService : IApplicationService
{
    Task<PagedResultDto<LogClusterDto>> GetListAsync(PagedAndSortedResultRequestDto input);

    Task<int> RunClusteringAsync(Guid? applicationId);
}
