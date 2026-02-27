using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SystemIntelligencePlatform.Dashboard;

public interface IDashboardAppService : IApplicationService
{
    Task<DashboardDto> GetAsync(Guid? applicationId = null);
}
