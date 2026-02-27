using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SystemIntelligencePlatform.Incidents;
using SystemIntelligencePlatform.Permissions;

namespace SystemIntelligencePlatform.Controllers;

[Route("api/app/incidents/search")]
[Authorize(SystemIntelligencePlatformPermissions.Incidents.Search)]
public class IncidentSearchController : SystemIntelligencePlatformController
{
    private readonly IncidentAppService _incidentAppService;

    public IncidentSearchController(IncidentAppService incidentAppService)
    {
        _incidentAppService = incidentAppService;
    }

    [HttpPost]
    public async Task<IncidentSearchResultDto> SearchAsync([FromBody] IncidentSearchRequestDto input)
    {
        return await _incidentAppService.SearchAsync(input);
    }
}
