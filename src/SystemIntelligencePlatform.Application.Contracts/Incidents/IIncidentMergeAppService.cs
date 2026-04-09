using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SystemIntelligencePlatform.Incidents;

public interface IIncidentMergeAppService : IApplicationService
{
    Task<int> ScanAndMergeAsync(Guid? applicationId);
}
